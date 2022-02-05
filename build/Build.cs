using System;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Nuke.Common;
using Nuke.Common.ChangeLog;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
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
using static Nuke.Common.Tools.BenchmarkDotNet.BenchmarkDotNetTasks;
using Nuke.Common.Tools.DocFX;
using System.IO;
using System.Collections.Generic;
using SharpPulsar.TestContainer.TestUtils;
using Docker.DotNet;
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
    public static int Main () => Execute<Build>(x => x.ApiTest);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    //readonly Configuration Configuration = Configuration.Release;
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;
    [GitVersion(Framework = "net6.0")] readonly GitVersion GitVersion;

    [Parameter] string NugetApiUrl = "https://api.nuget.org/v3/index.json"; 
    [Parameter] string GithubSource = "https://nuget.pkg.github.com/OWNER/index.json"; 

    [Parameter] bool Container = false;

    readonly static DIContainer DIContainer = DIContainer.Default;

    //[Parameter] string NugetApiKey = Environment.GetEnvironmentVariable("SHARP_PULSAR_NUGET_API_KEY");
    [Parameter("NuGet API Key", Name = "NUGET_API_KEY")]
    readonly string NugetApiKey;
    
    [Parameter("Admin NuGet API Key", Name = "ADMIN_NUGET_KEY")]
    readonly string AdminNugetApiKey;
    
    [Parameter("GitHub Build Number", Name = "BUILD_NUMBER")]
    readonly string BuildNumber;

    [Parameter("GitHub Access Token for Packages", Name = "GH_API_KEY")]
    readonly string GitHubApiKey;

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
            var version = LatestVersion;
            var vers = $"{version.Version.Major}.{version.Version.Minor}.{version.Version.Patch}";
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetNoRestore(InvokedTargets.Contains(Restore))
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(vers)
                .SetFileVersion(vers));
        });
    IEnumerable<string> ChangelogSectionNotes => ExtractChangelogSectionNotes(ChangelogFile);

    Target RunChangelog => _ => _
        .Executes(() =>
        {
            var branch = GitVersion.BranchName;
            switch (branch)
            {
                case "main":
                case "master":
                    break;
                default:
                    Assert.Fail($"Current branch:'{branch}'. You can only execute this in main branch");
                    break;
            }
            FinalizeChangelog(ChangelogFile, GitVersion.SemVer, GitRepository);

            Git($"add {ChangelogFile}");

            Git($"commit -S -m \"Finalize {Path.GetFileName(ChangelogFile)} for {GitVersion.SemVer}.\"");

            Git($"tag -f {GitVersion.SemVer}");
        });
    Target SqlTest => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            var project = Solution.GetProject("SharpPulsar.Test.SQL");
            Information($"Running tests from {project.Name}");
            DotNetTest(c => c
                    .SetProjectFile(project)
                    .SetConfiguration(Configuration.ToString())
                    .SetFramework("net6.0")
                    .SetProcessExecutionTimeout((int)TimeSpan.FromMinutes(30).TotalMilliseconds)
                    //.SetResultsDirectory(OutputTests / "sql")
                    //.SetLoggers("trx")
                    //.SetBlameCrash(true)//Runs the tests in blame mode and collects a crash dump when the test host exits unexpectedly
                    //.SetBlameMode(true)//captures the order of tests that were run before the crash.
                    .SetVerbosity(verbosity: DotNetVerbosity.Normal)
                    .EnableNoBuild());
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
                    //.SetResultsDirectory(OutputTests / "tls")
                    //.SetLoggers("trx")
                    //.SetBlameCrash(true)//Runs the tests in blame mode and collects a crash dump when the test host exits unexpectedly
                    //.SetBlameMode(true)//captures the order of tests that were run before the crash.
                    .SetVerbosity(verbosity: DotNetVerbosity.Normal)
                    .EnableNoBuild());
            }
        });
    Target TxnTest => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            var project = Solution.GetProject("SharpPulsar.Test.Transaction");
            Information($"Running tests from {project.Name}");
            foreach (var fw in project.GetTargetFrameworks())
            {
                DotNetTest(c => c
                    .SetProjectFile(project)
                    .SetConfiguration(Configuration.ToString())
                    .SetFramework(fw)
                    .SetProcessExecutionTimeout((int)TimeSpan.FromMinutes(30).TotalMilliseconds)
                    //.SetResultsDirectory(OutputTests / "txn")
                    //.SetLoggers("trx")
                    //.SetBlameCrash(true)//Runs the tests in blame mode and collects a crash dump when the test host exits unexpectedly
                    //.SetBlameMode(true)//captures the order of tests that were run before the crash.
                    .SetVerbosity(verbosity: DotNetVerbosity.Normal)
                    .EnableNoBuild());
            }
        });

    Target EventTest => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            var project = Solution.GetProject("SharpPulsar.Test.EventSourcing");
            Information($"Running tests from {project.Name}");
            foreach (var fw in project.GetTargetFrameworks())
            {
                DotNetTest(c => c
                    .SetProjectFile(project)
                    .SetConfiguration(Configuration.ToString())
                    .SetFramework(fw)
                    .SetProcessExecutionTimeout((int)TimeSpan.FromMinutes(30).TotalMilliseconds)
                    //.SetResultsDirectory(OutputTests / "event")
                    //.SetLoggers("trx")
                    //.SetBlameCrash(true)//Runs the tests in blame mode and collects a crash dump when the test host exits unexpectedly
                    //.SetBlameMode(true)//captures the order of tests that were run before the crash.
                    .SetVerbosity(verbosity: DotNetVerbosity.Normal)
                    .EnableNoBuild());
            }
        });

    Target IntegrationTest => _ => _
        .DependsOn(Compile)
        .Executes(async () =>
        {
            var project = Solution.GetProject("SharpPulsar.Test.Integration");
            Information($"Running tests from {project.Name}");
            foreach (var fw in project.GetTargetFrameworks())
            {
                DotNetTest(c => c
                    .SetProjectFile(project)
                    .SetConfiguration(Configuration.ToString())
                    .SetFramework(fw)
                    .SetProcessExecutionTimeout((int)TimeSpan.FromMinutes(60).TotalMilliseconds)
                    //.SetResultsDirectory(OutputTests / "integration")
                    //.SetLoggers("trx")
                    //.SetBlameCrash(true)//Runs the tests in blame mode and collects a crash dump when the test host exits unexpectedly
                    //.SetBlameMode(true)//captures the order of tests that were run before the crash.
                    .SetVerbosity(verbosity: DotNetVerbosity.Normal)
                    .EnableNoBuild());
            }
            //if(Container)
               // await SaveFile("test-integration", OutputTests / "integration", "/host/documents/testresult");
        });
    Target ApiTest => _ => _
       .DependsOn(Compile)
       .Executes(() =>
       {
           var project = Solution.GetProject("SharpPulsar.Test");
           Information($"Running tests from {project.Name}");
           DotNetTest(c => c
                  .SetProjectFile(project)
                  .SetConfiguration(Configuration.ToString())
                  .SetFramework("net6.0")
                  //.SetLoggers("trx")
                  //.SetBlameCrash(true)//Runs the tests in blame mode and collects a crash dump when the test host exits unexpectedly
                  //.SetBlameMode(true)//captures the order of tests that were run before the crash.
                  //.SetResultsDirectory(OutputTests / "api")
                  .SetVerbosity(verbosity: DotNetVerbosity.Normal)
                  .EnableNoBuild());

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
        .Executes(() => DocFXServe(s => s.SetFolder(DocFxDir)));

    Target CreateNuget => _ => _
      .DependsOn(ApiTest, IntegrationTest)
      .Executes(() =>
      {
          var version = LatestVersion;
          var project = Solution.GetProject("SharpPulsar");
          DotNetPack(s => s
              .SetProject(project)
              .SetConfiguration(Configuration)
              .EnableNoBuild()
              .EnableNoRestore()
              .SetIncludeSymbols(true)
              .SetAssemblyVersion(version.Version.ToString())
              .SetFileVersion(version.Version.ToString())
              .SetVersion(version.Version.ToString())
              .SetPackageReleaseNotes(GetNuGetReleaseNotes(ChangelogFile, GitRepository))
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
      .Requires(() => !GitHubApiKey.IsNullOrEmpty())
      .Requires(() => Configuration.Equals(Configuration.Release))
      .Executes(() =>
      {
          OutputNuget.GlobFiles("*.nupkg")
              .Where(x => !x.ToString().EndsWith("symbols.nupkg"))
              .ForEach(x =>
              {
                  DotNetNuGetPush(s => s
                      .SetTargetPath(x)
                      .SetSource(NugetApiUrl)
                      .SetApiKey(NugetApiKey)
                  );
                  
                  DotNetNuGetPush(s => s
                      .SetApiKey(GitHubApiKey)
                      .SetSymbolApiKey(GitHubApiKey)
                      .SetTargetPath(x)
                      .SetSource(GithubSource)
                      .SetSymbolSource(GithubSource));
              });
      });
    protected override void OnBuildCreated()
    {
        if (Container)
            DIContainer.RegisterDockerClient();

        base.OnBuildCreated();
    }
    static async Task SaveFile(string containerName, string sourcePath, string outputPath)
    {
        var client = DIContainer.Get<DockerClient>();
        var file = await client.Containers.GetArchiveFromContainerByNameAsync(sourcePath, containerName);
        ArchiveHelper.Extract(file.Stream, outputPath);
    }
    static void Information(string info)
    {
        Serilog.Log.Information(info);
    }
}
