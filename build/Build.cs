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

    //public static int Main () => Execute<Build>(x => x.Test);
    public static int Main () => Execute<Build>(x => x.Test);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    //readonly Configuration Configuration = Configuration.Release;
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;
    [GitVersion(Framework = "net6.0")] readonly GitVersion GitVersion;

    [Parameter] string NugetApiUrl = "https://api.nuget.org/v3/index.json"; 
    [Parameter] string GithubSource = "https://nuget.pkg.github.com/OWNER/index.json"; 

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
            .GlobDirectories("**/bin", "**/obj", Output, OutputTests, OutputPerfTests, OutputNuget, DocSiteDirectory)
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
    Target RunMemoryAllocation => _ => _
        .DependsOn(Compile)
        .OnlyWhenStatic(() => IsLocalBuild)
        .Executes(() =>
        {
            
            var testAssembly = RootDirectory + "\\Tests\\SharpPulsar.Test.Memory\\bin\\Release\net5.0\\SharpPulsar.Test.Memory.dll";
            DotMemoryUnit($"{XunitTasks.XunitPath.DoubleQuoteIfNeeded()} --propagate-exit-code -- {testAssembly}", timeout: 120_000);
            //Nuke.Common.Tools.DotMemoryUnit.DotMemoryUnitTasks.DotMemoryUnit($"{XunitTasks.XunitPath.DoubleQuoteIfNeeded()} --propagate-exit-code -- {testAssembly}", timeout: 120_000);
        });
    Target Benchmark => _ => _
        .DependsOn(Compile)
        .OnlyWhenStatic(() => IsLocalBuild)
        .Executes(() =>
        {
            var benchmarkAssembly = RootDirectory+"\\SharpPulsar.Benchmarks\\bin\\Release\\net5.0\\SharpPulsar.Benchmarks.exe";
            var process = ProcessTasks.StartProcess(benchmarkAssembly);
            process.AssertZeroExitCode();
            var output = process.Output;
        });
    //IEnumerable<Project> TestProjects => Solution.GetProjects("*.Test");
    Target Test => _ => _
        .DependsOn(Compile)
        .DependsOn(SetupPulsar)
        .Executes(() =>
        {
            var testProject = "SharpPulsar.Test";
            var projects = Solution.GetProjects("*.Test")
            .Where(x=> !x.Name.EndsWith("EventSourcing") 
                    && !x.Name.EndsWith("Memory"));
            var project = Solution.GetProject(testProject);
            Information($"Running tests from {project.Name}");

            try
            {
                DotNetTest(c => c
                    .SetProjectFile(project)
                    .SetConfiguration(Configuration.ToString())
                    .SetFramework("net6.0")
                    .SetLoggers("GitHubActions")
                    //.SetDiagnosticsFile(TestsDirectory)
                    .SetVerbosity(verbosity: DotNetVerbosity.Detailed)
                    .EnableNoBuild());
            }
            catch (Exception ex)
            {
                Information(ex.Message);
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
        .Executes(() => DocFXServe(s => s.SetFolder(DocFxDir)));

    Target TxnTest => _ => _
        .Partition(4)
        .DependsOn(Test)
        .Triggers(StopPulsar)
        .Executes(() =>
        {
            var testProject = "SharpPulsar.Test.Transaction";
            var project = Solution.GetProject(testProject);
            Information($"Running tests from {project.Name}");
            foreach(var fw in project.GetTargetFrameworks())
            {
                DotNetTest(c => c
                    .SetProjectFile(project)
                    .SetConfiguration(Configuration.ToString())
                    .SetFramework(fw)
                    .SetResultsDirectory(OutputTests)
                    .SetProcessWorkingDirectory(Directory.GetParent(project).FullName)
                    .SetLoggers("trx")
                    .SetVerbosity(verbosity: DotNetVerbosity.Normal)
                    .EnableNoBuild());
            }            
        });

    Target StartPulsar => _ => _
      .Before(SetupPulsar)
      .DependsOn(CheckDockerVersion)
      .Executes(async () =>
       {
           DockerTasks.DockerRun(b =>
            b
            .SetDetach(true)
            .SetInteractive(true)
            .SetName("pulsar_test")
            .SetPublish("6650:6650", "8080:8080","8081:8081", "2181:2181")
            .SetMount("source=pulsardata,target=/pulsar/data")
            .SetMount("source=pulsarconf,target=/pulsar/conf")
            .SetImage("apachepulsar/pulsar-all:2.9.1")
            .SetEnv("PULSAR_MEM= -Xms512m -Xmx512m -XX:MaxDirectMemorySize=1g", @"PULSAR_PREFIX_acknowledgmentAtBatchIndexLevelEnabled=true", "PULSAR_PREFIX_nettyMaxFrameSizeBytes=5253120", @"PULSAR_PREFIX_transactionCoordinatorEnabled=true", @"PULSAR_PREFIX_brokerDeleteInactiveTopicsEnabled=false", @"PULSAR_PREFIX_exposingBrokerEntryMetadataToClientEnabled=true", @"PULSAR_PREFIX_brokerEntryMetadataInterceptors=org.apache.pulsar.common.intercept.AppendBrokerTimestampMetadataInterceptor,org.apache.pulsar.common.intercept.AppendIndexMetadataInterceptor")
            .SetCommand("bash")
            .SetArgs("-c", "bin/apply-config-from-env.py conf/standalone.conf && bin/pulsar standalone -nss -nfw && bin/pulsar initialize-transaction-coordinator-metadata -cs localhost:2181 -c standalone --initial-num-transaction-coordinators 2")) ;
           //.SetArgs("-c", "bin/apply-config-from-env.py conf/standalone.conf && bin/pulsar standalone -nss -nfw && bin/pulsar initialize-transaction-coordinator-metadata -cs localhost:2181 -c standalone --initial-num-transaction-coordinators 2")) ;
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
    Target SetupPulsar => _ => _
      .Before(Test)
      .DependsOn(StartPulsar)
      .Executes(() =>
       {
           DockerTasks.DockerExec(x => x
                .SetContainer("pulsar_test")
                .SetCommand("bin/pulsar-admin")
                .SetArgs("namespaces", "set-retention", "public/default","--time","3600")
            );
           DockerTasks.DockerExec(x => x
                .SetContainer("pulsar_test")
                .SetCommand("bin/pulsar-admin")
                .SetArgs("namespaces", "set-schema-validation-enforce", "--enable", "public/default")
            );
           DockerTasks.DockerExec(x => x
                .SetDetach(true)
                .SetInteractive(true)   
                .SetContainer("pulsar_test")
                .SetCommand("bin/pulsar")
                //.SetArgs("sql-worker", "run")
                .SetArgs("sql-worker", "start")//as daemon process
            );
       });
    Target CheckDockerVersion => _ => _
      .DependsOn(CheckBranch)
        .Executes(() =>
        {
            DockerTasks.DockerVersion();
        });

    Target CheckBranch => _ => _
       .Executes(() =>
       {
           Information(GitRepository.Branch);
       });
    Target StopPulsar => _ => _
    .AssuredAfterFailure()
    .Executes(() =>
    {

        try
        {
           DockerTasks.DockerRm(b => b
          .SetContainers("pulsar_test")
          .SetForce(true));
        }
        catch(Exception ex)
        {
            Information(ex.ToString());
        }

    });
    Target CreateNuget => _ => _
      .DependsOn(Compile)
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
    static void Information(string info)
    {
        Serilog.Log.Information(info);
    }
}
