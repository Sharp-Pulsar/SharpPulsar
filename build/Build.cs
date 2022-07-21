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
using static Nuke.Common.Tools.Xunit.XunitTasks;
using Octokit;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Build.Execution;
using Nuke.Common.Tools.MSBuild;
using System.Diagnostics;
using System.Runtime.InteropServices;
using SharpPulsar.TestContainer;
using DotNet.Testcontainers.Builders;
using Nuke.Common.CI.GitHubActions;
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

    //public static int Main () => Execute<Build>(x => x.Test);
    public static int Main() => Execute<Build>(x => x.EventSource);

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

    Target TlsStartPulsar => _ => _
      .DependsOn(CheckDockerVersion)
      .Executes(async () =>
      {
          DockerTasks.DockerRun(b =>
           b
           .SetDetach(true)
           .SetInteractive(true)
           .SetName("pulsar")
           .SetPublish("6651:6651", "8080:8080", "8081:8081", "2181:2181")
           .SetMount("source=pulsardata,target=/pulsar/data")
           .SetMount("source=pulsarconf,target=/pulsar/conf")
           .SetImage("apachepulsar/pulsar-all:2.10.0")
           .SetEnv("PULSAR_MEM= -Xms512m -Xmx512m -XX:MaxDirectMemorySize=1g", @"PULSAR_PREFIX_acknowledgmentAtBatchIndexLevelEnabled=true", "PULSAR_PREFIX_nettyMaxFrameSizeBytes=5253120", @"PULSAR_PREFIX_transactionCoordinatorEnabled=true", "PULSAR_PREFIX_brokerDeleteInactiveTopicsEnabled=false", "PULSAR_PREFIX_exposingBrokerEntryMetadataToClientEnabled=true", "PULSAR_PREFIX_brokerEntryMetadataInterceptors=org.apache.pulsar.common.intercept.AppendBrokerTimestampMetadataInterceptor,org.apache.pulsar.common.intercept.AppendIndexMetadataInterceptor")
           .SetCommand("bash")
           .SetArgs("-c", "bin/apply-config-from-env.py conf/standalone.conf && bin/pulsar standalone -nss -nfw && bin/pulsar initialize-transaction-coordinator-metadata -cs localhost:2181 -c standalone --initial-num-transaction-coordinators 2"));
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
                  await client.GetAsync("http://127.0.0.1:8080/metrics/").ConfigureAwait(false);
                  Information("Apache Pulsar Server live at: http://127.0.0.1");
                  return;
              }
              catch (Exception ex)
              {
                  Information(ex.Message);
                  waitTries--;
                  await Task.Delay(5000).ConfigureAwait(false);
              }
          }

          throw new Exception("Unable to confirm Pulsar has initialized");
      });
    Target CheckDockerVersion => _ => _
    .Unlisted()
      .DependsOn(CheckBranch)
        .Executes(() =>
        {
            DockerTasks.DockerVersion();
        });
    Target CheckBranch => _ => _
       .Unlisted()
       .Executes(() =>
       {
           Information(GitRepository.Branch);
       });
    private PulsarTestcontainer BuildContainer()
    {
        return new TestcontainersBuilder<PulsarTestcontainer>()
          .WithName("Tests")
          .WithPulsar(new PulsarTestcontainerConfiguration("apachepulsar/pulsar-all:2.10.0", 6650))
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
    Target StartPulsar => _ => _
      .DependsOn(CheckDockerVersion)
      .Executes(async () =>
      {
          DockerTasks.DockerRun(b =>
           b
           .SetDetach(true)
           .SetInteractive(true)
           .SetName("pulsar")
           .SetPublish("6650:6650", "8080:8080", "8081:8081", "2181:2181")
           //.SetMount("source=pulsardata,target=/pulsar/data")
           //.SetMount("source=pulsarconf,target=/pulsar/conf")
           .SetImage("apachepulsar/pulsar-all:2.10.0")
           .SetEnv("PULSAR_MEM= -Xms512m -Xmx512m -XX:MaxDirectMemorySize=1g", @"PULSAR_PREFIX_acknowledgmentAtBatchIndexLevelEnabled=true", "PULSAR_PREFIX_nettyMaxFrameSizeBytes=5253120", @"PULSAR_PREFIX_transactionCoordinatorEnabled=true", "PULSAR_PREFIX_brokerDeleteInactiveTopicsEnabled=false", "PULSAR_PREFIX_exposingBrokerEntryMetadataToClientEnabled=true", "PULSAR_PREFIX_brokerEntryMetadataInterceptors=org.apache.pulsar.common.intercept.AppendBrokerTimestampMetadataInterceptor,org.apache.pulsar.common.intercept.AppendIndexMetadataInterceptor")
           .SetCommand("bash")
           .SetArgs("-c", "bin/apply-config-from-env.py conf/standalone.conf && bin/pulsar standalone -nss -nfw && bin/pulsar initialize-transaction-coordinator-metadata -cs localhost:2181 -c standalone --initial-num-transaction-coordinators 2 && bin/pulsar sql-worker start"));

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
                  var get = await client.GetAsync("http://127.0.0.1:8080/metrics/").ConfigureAwait(false);
                  var j = await get.Content.ReadAsStringAsync();
                  Information(j);
                  Information("Apache Pulsar Server live at: http://127.0.0.1");


                  DockerTasks.DockerContainerExec(b =>
                  b
                  .SetContainer("pulsar")
                  .SetCommand("bash")
                  .SetArgs("-c", "bin/pulsar sql-worker start"));
                  return;
              }
              catch (Exception ex)
              {
                  Information(ex.Message);
                  waitTries--;
                  await Task.Delay(5000).ConfigureAwait(false);
              }
          }

          throw new Exception("Unable to confirm Pulsar has initialized");
      });
    Target SqlPulsar => _ => _
      .DependsOn(StartPulsar)
      .Executes(() =>
      {
          DockerTasks.DockerContainerExec(b =>
          b
          .SetContainer("pulsar")
          .SetCommand("bash")
          .SetArgs("-c", "bin/pulsar sql-worker start"));
      });
    Target StopPulsar => _ => _
    .Unlisted()
    .AssuredAfterFailure()
    .Executes(() =>
    {

        try
        {
            DockerTasks.DockerRm(b => b
            .SetContainers("pulsar")
            .SetForce(true));
        }
        catch (Exception ex)
        {
            Information(ex.ToString());
        }

    });
    Target AdminPulsar => _ => _
      .DependsOn(SqlPulsar)
      .Executes(() =>
      {
          DockerTasks.DockerExec(x => x
                .SetContainer("pulsar")
                .SetCommand("bin/pulsar-admin")
                .SetArgs("tenants", "create", "tnx", "-r", "appid1", "--allowed-clusters", "standalone")
            );
          DockerTasks.DockerExec(x => x
               .SetContainer("pulsar")
               .SetCommand("bin/pulsar-admin")
               .SetArgs("namespaces", "create", "public/deduplication")
           );
          DockerTasks.DockerExec(x => x
               .SetContainer("pulsar")
               .SetCommand("bin/pulsar-admin")
               .SetArgs("namespaces", "set-retention", "public/default", "--time", "3600", "--size", "-1")
           );
          DockerTasks.DockerExec(x => x
               .SetContainer("pulsar")
               .SetCommand("bin/pulsar-admin")
               .SetArgs("namespaces", "set-deduplication", "public/deduplication", "--enable")
           );
          DockerTasks.DockerExec(x => x
               .SetContainer("pulsar")
               .SetCommand("bin/pulsar-admin")
               .SetArgs("namespaces", "set-schema-validation-enforce", "--enable", "public/default")
           );
          /*DockerTasks.DockerExec(x => x
               .SetContainer("pulsar")
               .SetCommand("bin/pulsar")
               .SetArgs("sql-worker", "run")
           );*/
      });
    Target TestContainer => _ => _
    .Executes(async () =>
    {
        Information("Test Container");
        var container = BuildContainer();
        await container.StartAsync();//;.GetAwaiter().GetResult();]
        Information("Start Test Container");
        await AwaitPortReadiness($"http://127.0.0.1:8080/metrics/");
        Information("ExecAsync Test Container");
        await container.ExecAsync(new List<string> { @"./bin/pulsar", "sql-worker", "start" });

        await AwaitPortReadiness($"http://127.0.0.1:8081/");
        Information("AwaitPortReadiness Test Container");
    });

    Target TestAPI => _ => _
       .DependsOn(Compile, AdminPulsar)
       .Executes(() =>
       {
           CoreTest("SharpPulsar.Test.API");
       });
    Target Test => _ => _
        .DependsOn(TestAPI)
        .Executes(() =>
        {
            CoreTest("SharpPulsar.Test");
        });
    Target Transaction => _ => _
       .DependsOn(Test)
       .Executes(() =>
       {
           CoreTest("SharpPulsar.Test.Transaction");
       });
    Target Partitioned => _ => _
       .DependsOn(Transaction)
       .Executes(() =>
       {
           CoreTest("SharpPulsar.Test.Partitioned");
       });
    Target Acks => _ => _
       .DependsOn(Partitioned)
       .Executes(() =>
       {
           CoreTest("SharpPulsar.Test.Acks");
       });
    Target MultiTopic => _ => _
       .DependsOn(Acks)
       .Executes(() =>
       {
           CoreTest("SharpPulsar.Test.MultiTopic");
       });
    Target AutoClusterFailover => _ => _
        .DependsOn(MultiTopic)
        .Executes(() =>
        {
            CoreTest("SharpPulsar.Test.AutoClusterFailover");
        });
    Target TableView => _ => _
       .DependsOn(AutoClusterFailover)
       .Executes(() =>
       {
           CoreTest("SharpPulsar.Test.TableView");
       });
    Target EventSource => _ => _
       .DependsOn(TableView)
       .Triggers(StopPulsar)
       .Executes(() =>
       {
           CoreTest("SharpPulsar.Test.EventSource");
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
      //.DependsOn(IntegrationTest)
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
      .Triggers(GitHubRelease)
      .Executes(() =>
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

      });

    Target AuthenticatedGitHubClient => _ => _
        .Unlisted()
        .OnlyWhenDynamic(() => !string.IsNullOrWhiteSpace(GitHubActions.Token))
        .Executes(() =>
        {
            GitHubClient = new GitHubClient(new ProductHeaderValue("nuke-build"))
            {
                Credentials = new Credentials(GitHubActions.Token, AuthenticationType.Bearer)
            };
        });
    Target GitHubRelease => _ => _
        .Unlisted()
        .Description("Creates a GitHub release (or amends existing) and uploads the artifact")
        .OnlyWhenDynamic(() => !string.IsNullOrWhiteSpace(GitHubActions.Token))
        .DependsOn(AuthenticatedGitHubClient)
        .Executes(async () =>
        {
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
        });
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