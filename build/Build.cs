using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
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
using SharpPulsar.TestContainer.TestUtils;
using Docker.DotNet;
using Octokit;
using Nuke.Common.Tools.Git;
//https://github.com/AvaloniaUI/Avalonia/blob/master/nukebuild/Build.cs
//https://github.com/cfrenzel/Eventfully/blob/master/build/Build.cs
[CheckBuildProjectConfigurations]
[ShutdownDotNetAfterServerBuild]

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
    public static int Main() => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    //readonly Configuration Configuration = Configuration.Release;
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;

    [GitVersion(Framework = "net6.0")] readonly GitVersion GitVersion;

    [Parameter] string NugetApiUrl = "https://api.nuget.org/v3/index.json";

    [Parameter] bool Container = false;

    readonly static DIContainer DIContainer = DIContainer.Default;

    [Parameter][Secret] string NugetApiKey;

    [Parameter] [Secret] string Toke;
    [Parameter] [Secret] string GitHubToken;

    [PackageExecutable("JetBrains.dotMemoryUnit", "dotMemoryUnit.exe")] readonly Tool DotMemoryUnit;

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

    static readonly JsonElement? _githubContext = string.IsNullOrWhiteSpace(EnvironmentInfo.GetVariable<string>("GITHUB_CONTEXT")) ?
        null
        : JsonSerializer.Deserialize<JsonElement>(EnvironmentInfo.GetVariable<string>("GITHUB_CONTEXT"));

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
            var vers = GitVersion.MajorMinorPatch;
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetNoRestore(InvokedTargets.Contains(Restore))
                .SetConfiguration(Configuration)
                //.SetAssemblyVersion(vers)
                .SetFileVersion(vers)
                .SetVersion(GitVersion.SemVer));
        });
    IEnumerable<string> ChangelogSectionNotes => ExtractChangelogSectionNotes(ChangelogFile);

    Target RunChangelog => _ => _
        .OnlyWhenDynamic(()=> GitVersion.BranchName.Equals("main", StringComparison.OrdinalIgnoreCase))
        .Executes(() =>
        {
            FinalizeChangelog(ChangelogFile, GitVersion.SemVer, GitRepository);

            Git($"add {ChangelogFile}");

            Git($"commit -S -m \"Finalize {Path.GetFileName(ChangelogFile)} for {GitVersion.SemVer}.\"");

            Git($"tag -f {GitVersion.SemVer}");
        });
    Target TlsTest => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        { 
            var project = Solution.GetProject("SharpPulsar.Test.Tls");
            Information($"Running tests from {project.Name}");
            foreach (var fw in project.GetTargetFrameworks())
            {
                DotNetTest(c => c
                    .SetProjectFile(project)
                    .SetConfiguration(Configuration.ToString())
                    .SetFramework(fw)
                    .SetProcessExecutionTimeout((int)TimeSpan.FromMinutes(30).TotalMilliseconds)
                    .SetResultsDirectory(OutputTests / "tls")
                    .SetLoggers("trx", "console")
                    //.SetBlameCrash(true)//Runs the tests in blame mode and collects a crash dump when the test host exits unexpectedly
                    .SetBlameMode(true)//captures the order of tests that were run before the crash.
                    .SetVerbosity(verbosity: DotNetVerbosity.Normal)
                    .EnableNoBuild());
            }
        });
    Target IntegrationTest => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            var project = Solution.GetProject("SharpPulsar.Test");
            Information($"Running tests from {project.Name}");
            foreach (var fw in project.GetTargetFrameworks())
            {
                DotNetTest(c => c
                    .SetProjectFile(project)
                    .SetConfiguration(Configuration.ToString())
                    .SetFramework(fw)
                    .SetProcessExecutionTimeout((int)TimeSpan.FromMinutes(60).TotalMilliseconds)
                    .SetResultsDirectory(OutputTests / "tests")
                    .SetLoggers("trx")
                    //.SetBlameCrash(true)//Runs the tests in blame mode and collects a crash dump when the test host exits unexpectedly
                    .SetBlameMode(true)//captures the order of tests that were run before the crash.
                    .SetVerbosity(verbosity: DotNetVerbosity.Normal)
                    .EnableNoBuild());
            }
            //if(Container)
               // await SaveFile("test-integration", OutputTests / "integration", "/host/documents/testresult");
        });
    Target AutoFailOverTest => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            var project = Solution.GetProject("SharpPulsar.Test.AutoClusterFailover");
            Information($"Running tests from {project.Name}");
            foreach (var fw in project.GetTargetFrameworks())
            {
                DotNetTest(c => c
                    .SetProjectFile(project)
                    .SetConfiguration(Configuration.ToString())
                    .SetFramework(fw)
                    .SetProcessExecutionTimeout((int)TimeSpan.FromMinutes(60).TotalMilliseconds)
                    .SetResultsDirectory(OutputTests / "tests")
                    .SetLoggers("trx")
                    //.SetBlameCrash(true)//Runs the tests in blame mode and collects a crash dump when the test host exits unexpectedly
                    .SetBlameMode(true)//captures the order of tests that were run before the crash.
                    .SetVerbosity(verbosity: DotNetVerbosity.Normal)
                    .EnableNoBuild());
            }
        });
    //--------------------------------------------------------------------------------
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
          var version = GitVersion.SemVer;
          var branchName = GitVersion.BranchName;

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
              .SetAssemblyVersion(version)
              .SetFileVersion(version)
              .SetVersion(version)
              .SetPackageReleaseNotes(releaseNotes)
              .SetDescription("SharpPulsar is Apache Pulsar Client built using Akka.net")
              .SetPackageTags("Apache Pulsar", "Akka.Net", "Event Driven","Event Sourcing", "Distributed System", "Microservice")
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
        .OnlyWhenDynamic(() => !string.IsNullOrWhiteSpace(GitHubToken))
        .Executes(() =>
        {
            GitHubClient = new GitHubClient(new ProductHeaderValue("nuke-build"))
            {
                Credentials = new Octokit.Credentials(GitHubToken, AuthenticationType.Bearer)
            };
        });
    Target GitHubRelease => _ => _
        .Unlisted()
        .Description("Creates a GitHub release (or amends existing) and uploads the artifact")
        .OnlyWhenDynamic(() => !string.IsNullOrWhiteSpace(GitHubToken))
        .DependsOn(AuthenticatedGitHubClient)
        .Executes(async () =>
        {
            var version = GitVersion.SemVer;
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

    string ParseReleaseNote()
    {
        return XmlTasks.XmlPeek(RootDirectory / "Directory.Build.props", "//Project/PropertyGroup/PackageReleaseNotes").FirstOrDefault();
    }
    static void Information(string info)
    {
        Serilog.Log.Information(info);
    }
}
