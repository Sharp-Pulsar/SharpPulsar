using System;
using System.Linq;
using System.Text.Json;
using Nuke.Common;
using Nuke.Common.ChangeLog;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Docker;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.Xunit;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.ChangeLog.ChangelogTasks;
using static Nuke.Common.Tools.Git.GitTasks;
using static Nuke.Common.Tools.DocFX.DocFXTasks;
using Nuke.Common.Tools.DocFX;
using System.IO;
using System.Collections.Generic;
using Octokit;
using System.Net.Http;
using System.Threading.Tasks;
using Nuke.Common.Tools.MSBuild;
using Nuke.Common.CI.GitHubActions;
using SharpPulsar.TestContainer.Container;
using SharpPulsar.TestContainer.Configuration;
using DotNet.Testcontainers.Builders;
using SharpPulsar.TestContainer;
//https://github.com/AvaloniaUI/Avalonia/blob/master/nukebuild/Build.cs
//https://github.com/cfrenzel/Eventfully/blob/master/build/Build.cs


[ShutdownDotNetAfterServerBuild]
[DotNetVerbosityMapping]
[UnsetVisualStudioEnvironmentVariables]
partial class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode 
    ///   

    ///   - https://ithrowexceptions.com/2020/06/05/reusable-build-components-with-interface-default-implementations.html

    public static int Main () => Execute<Build>(x => x.API);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    //readonly Configuration Configuration = Configuration.Release;
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Required][Solution] public readonly Solution Solution;
    [Required][GitRepository] public readonly GitRepository GitRepository;
    [Required][GitVersion(Framework = "net6.0")] public readonly GitVersion GitVersion = default!;
    [CI] public readonly GitHubActions GitHubActions;
    [Parameter] string NugetApiUrl = "https://api.nuget.org/v3/index.json";

    [Parameter][Secret] string NugetApiKey;
    public string GitHubPackageSource => $"https://nuget.pkg.github.com/{GitHubActions.RepositoryOwner}/index.json";

    AbsolutePath Output => RootDirectory / "bin";
    AbsolutePath OutputContainer => RootDirectory / "container";
    AbsolutePath OutputNuget => Output / "nuget";
    AbsolutePath OutputTests => RootDirectory / "TestResults";
    AbsolutePath OutputPerfTests => RootDirectory / "PerfResults";
    AbsolutePath DocSiteDirectory => RootDirectory / "docs" / "_site";
    public string ChangelogFile => RootDirectory / "CHANGELOG.md";
    public AbsolutePath DocFxDir => RootDirectory / "docs";
    public AbsolutePath DocFxDirJson => DocFxDir / "docfx.json";

    GitHubClient GitHubClient;
    public ChangeLog Changelog => ReadChangelog(ChangelogFile);

    public ReleaseNotes LatestVersion => Changelog.ReleaseNotes.OrderByDescending(s => s.Version).FirstOrDefault() ?? throw new ArgumentException("Bad Changelog File. Version Should Exist");
    public string ReleaseVersion => LatestVersion.Version?.ToString() ?? throw new ArgumentException("Bad Changelog File. Define at least one version");

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            RootDirectory
            .GlobDirectories("sharppulsar/bin", "sharppulsar/obj", Output, OutputTests, OutputPerfTests, OutputNuget, DocSiteDirectory)
            .ForEach(DeleteDirectory);
            EnsureCleanDirectory(Output);
        });

    Target Restore => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            //var vers = GitVersion.MajorMinorPatch;
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetNoRestore(InvokedTargets.Contains(Restore))
                .SetConfiguration(Configuration)
                //.SetAssemblyVersion(vers)
                //.SetFileVersion(vers)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetFileVersion(GitVersion.AssemblySemFileVer)
                .SetInformationalVersion(GitVersion.InformationalVersion)
                .SetVersion(GitVersion.NuGetVersionV2));
        });
    IEnumerable<string> ChangelogSectionNotes => ExtractChangelogSectionNotes(ChangelogFile);

    Target RunChangelog => _ => _
        .Requires(() => IsLocalBuild)
        .OnlyWhenDynamic(() => GitRepository.Branch.Equals("main", StringComparison.OrdinalIgnoreCase))
        .Executes(() =>
        {
            FinalizeChangelog(ChangelogFile, GitVersion.MajorMinorPatch, GitRepository);
            Information($"Please review CHANGELOG.md ({ChangelogFile}) and press 'Y' to validate (any other key will cancel changes)...");
            ConsoleKeyInfo keyInfo = Console.ReadKey();
            if (keyInfo.Key == ConsoleKey.Y)
            {
                Git($"add {ChangelogFile}");

                Git($"commit -S -m \"Finalize {Path.GetFileName(ChangelogFile)} for {GitVersion.MajorMinorPatch}.\"");

                Git($"tag -f {GitVersion.MajorMinorPatch}");
            }
        });

    Target Test => _ => _
        .DependsOn(TestContainer)
        .DependsOn(Compile)
        .Executes(async() =>
        {
            var projects = new List<string> 
            {
                "SharpPulsar.Test",
                "SharpPulsar.Sql.Tests",
                "SharpPulsar.Test.Admin"
            };

            foreach (var projectName in projects)
            {
                   var project = Solution.GetProject(projectName).NotNull("project != null");
                   Information($"Running tests from {project}");
                    foreach (var fw in project.GetTargetFrameworks())
                    {
                        try
                        {
                            DotNetTest(c => c
                            .SetProjectFile(project)
                            .SetConfiguration(Configuration)
                            .SetFramework(fw)
                            .EnableNoBuild()
                            .EnableNoRestore()
                            .When(true, _ => _
                                 .SetLoggers("console;verbosity=detailed")
                                .SetResultsDirectory(OutputTests)));
                        }
                        catch (Exception ex)
                        {
                            Information(ex.Message);
                        }
                    }
            }
            await Container.StopAsync();
            await Container.DisposeAsync();
        });
    Target Token => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            CoreTest("SharpPulsar.Test.Token");
        });
    Target API => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            CoreTest("SharpPulsar.Test.API");
        });
    void CoreTest(string projectName)
    {

        var project = Solution.GetProject(projectName).NotNull("project != null");
        Information($"Running tests from {projectName}");
        foreach (var fw in project.GetTargetFrameworks())
        {
            Information($"Running for {projectName} ({fw}) .....");
            try
            {
                 DotNetTest(c => c
                 .SetProjectFile(project)
                 .SetConfiguration(Configuration)
                 .SetFramework(fw)
                 .EnableNoBuild()
                 .EnableNoRestore()
                 .When(true, _ => _
                      .SetLoggers("console;verbosity=detailed")
                     .SetResultsDirectory(OutputTests)));
            }
            catch (Exception ex)
            {
                Information(ex.Message);
            }
        }
    }
    //---------------------
    //-----------------------------------------------------------
    // Documentation 
    //--------------------------------------------------------------------------------
    Target DocsInit => _ => _
    .DependsOn(Compile)
    .Executes(() =>
    {
        DocFXInit(s => s.SetOutputFolder(DocFxDir).SetQuiet(true));
    });
    Target DocsMetadata => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DocFXMetadata(s => s
            .SetProjects(DocFxDirJson)
            .SetLogLevel(DocFXLogLevel.Verbose));
        });

    Target DocBuild => _ => _
        .DependsOn(DocsMetadata)
        .Executes(() =>
        {
            DocFXBuild(s => s
            .SetConfigFile(DocFxDirJson)
            .SetLogLevel(DocFXLogLevel.Verbose));
        });

    Target ServeDocs => _ => _
        .DependsOn(DocBuild)
        .Executes(() => DocFXServe(s => s.SetFolder(DocFxDir).SetPort(8090)));

    Target CreateNuget => _ => _
      .DependsOn(Compile)
      .DependsOn(Test)
      .DependsOn(Token)
      .Executes(() =>
      {
          var branchName = GitRepository.Branch;

          if (branchName.Equals("main", StringComparison.OrdinalIgnoreCase)
          && !GitVersion.MajorMinorPatch.Equals(LatestVersion.Version.ToString()))
          {
              // Force CHANGELOG.md in case it skipped the mind
              Assert.Fail($"CHANGELOG.md needs to be update for final release. Current version: '{LatestVersion.Version}'. Next version: {GitVersion.MajorMinorPatch}");
          }
          var releaseNotes = branchName.Equals("main", StringComparison.OrdinalIgnoreCase)
                             ? GetNuGetReleaseNotes(ChangelogFile, GitRepository)
                             : ParseReleaseNote();
          var project = Solution.GetProject("SharpPulsar");
          DotNetPack(s => s
              .SetProject(project)
              .SetConfiguration(Configuration)
              .EnableNoBuild()
              .EnableNoRestore()
              //.SetIncludeSymbols(true)
              .SetAssemblyVersion(GitVersion.AssemblySemVer)
              .SetFileVersion(GitVersion.AssemblySemFileVer)
              .SetInformationalVersion(GitVersion.InformationalVersion)
              .SetVersion(GitVersion.NuGetVersionV2)
              .SetPackageReleaseNotes(releaseNotes)
              .SetDescription("SharpPulsar is Apache Pulsar Client built using Akka.net")
              .SetPackageTags("Apache Pulsar", "Akka.Net", "Event Driven", "Event Sourcing", "Distributed System", "Microservice")
              .AddAuthors("Ebere Abanonu (@mestical)")
              .SetPackageProjectUrl("https://github.com/eaba/SharpPulsar")
              .SetOutputDirectory(OutputNuget));

      });
    Target PublishNuget => _ => _
      .DependsOn(CreateNuget)
      .Requires(() => NugetApiUrl)
      .Requires(() => !NugetApiKey.IsNullOrEmpty())
      .Executes(async() =>
      {
          var packages = OutputNuget.GlobFiles("*.nupkg", "*.symbols.nupkg").NotNull();
          foreach (var package in packages)
          {
              DotNetNuGetPush(s => s
                  .SetTimeout(TimeSpan.FromMinutes(10).Minutes)
                  .SetTargetPath(package)
                  .SetSource(NugetApiUrl)
                  .SetApiKey(NugetApiKey));
          }
          await GitHubRelease();
      });
   async Task GitHubRelease ()
        {
            GitHubClient = new GitHubClient(new ProductHeaderValue("nuke-build"))
            {
                Credentials = new Credentials(GitHubActions.Token, AuthenticationType.Bearer)
            };
            var version = GitVersion.NuGetVersionV2;
            var releaseNotes = GetNuGetReleaseNotes(ChangelogFile);
            Release release;


            var identifier = GitRepository.Identifier.Split("/");
            var (gitHubOwner, repoName) = (identifier[0], identifier[1]);
            try
            {
                release = await GitHubClient.Repository.Release.Get(gitHubOwner, repoName, version);
            }
            catch (NotFoundException)
            {
                var newRelease = new NewRelease(version)
                {
                    Body = releaseNotes,
                    Name = version,
                    Draft = false,
                    Prerelease = GitRepository.IsOnReleaseBranch()
                };
                release = await GitHubClient.Repository.Release.Create(gitHubOwner, repoName, newRelease);
            }

            foreach (var existingAsset in release.Assets)
            {
                await GitHubClient.Repository.Release.DeleteAsset(gitHubOwner, repoName, existingAsset.Id);
            }

            Information($"GitHub Release {version}");
            var packages = OutputNuget.GlobFiles("*.nupkg", "*.symbols.nupkg").NotNull();
            foreach (var artifact in packages)
            {
                var releaseAssetUpload = new ReleaseAssetUpload(artifact.Name, "application/zip", File.OpenRead(artifact), null);
                var releaseAsset = await GitHubClient.Repository.Release.UploadAsset(release, releaseAssetUpload);
                Information($"  {releaseAsset.BrowserDownloadUrl}");
            }
        }
    Target TestContainer => _ => _
    .Executes(async () =>
    {
        Information("Test Container");
        Container = BuildContainer();
        await Container.StartAsync();//;.GetAwaiter().GetResult();]
        Information("Start Test Container");
        await AwaitPortReadiness($"http://127.0.0.1:8080/metrics/");
        Information("ExecAsync Test Container");
        await Container.ExecAsync(new List<string> { @"./bin/pulsar", "sql-worker", "start" });

        await AwaitPortReadiness($"http://127.0.0.1:8081/");
        Information("AwaitPortReadiness Test Container");
    });
    private PulsarTestContainer BuildContainer()
    {
        return new TestcontainersBuilder<PulsarTestContainer>()
          .WithName("pulsar-tests")
          .WithPulsar(new PulsarTestContainerConfiguration("apachepulsar/pulsar-all:2.10.1", 6650))
          .WithPortBinding(6650, 6650)
          .WithPortBinding(6651, 6651)
          .WithPortBinding(8080, 8080)
          .WithPortBinding(8081, 8081)
          .WithExposedPort(6650)
          .WithExposedPort(6651)
          .WithExposedPort(8080)
          .WithExposedPort(8081)
          .WithCleanUp(true)
          .Build();
    }
    private async ValueTask AwaitPortReadiness(string address)
    {
        var waitTries = 20;

        using var handler = new HttpClientHandler
        {
            AllowAutoRedirect = true
        };

        using var client = new HttpClient(handler);

        while (waitTries > 0)
        {
            try
            {
                await client.GetAsync(address).ConfigureAwait(false);
                return;
            }
            catch
            {
                waitTries--;
                await Task.Delay(5000).ConfigureAwait(false);
            }
        }

        throw new Exception("Unable to confirm Pulsar has initialized");
    }
    public PulsarTestContainer Container { get; set; }
    private string MajorMinorPatchVersion => GitVersion.MajorMinorPatch;

    string ParseReleaseNote()
    {
        return XmlTasks.XmlPeek(RootDirectory / "Directory.Build.props", "//Project/PropertyGroup/PackageReleaseNotes").FirstOrDefault();
    }
    static void Information(string info)
    {
        Serilog.Log.Information(info);
    }
}