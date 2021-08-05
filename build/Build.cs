using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Nuke.Common;
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
//https://github.com/AvaloniaUI/Avalonia/blob/master/nukebuild/Build.cs
//https://github.com/cfrenzel/Eventfully/blob/master/build/Build.cs
[CheckBuildProjectConfigurations]
[ShutdownDotNetAfterServerBuild]
[GitHubActions("Build",
    GitHubActionsImage.WindowsLatest,
    GitHubActionsImage.UbuntuLatest,
    AutoGenerate = true,
    OnPushBranches = new[] { "main", "dev" },
    OnPullRequestBranches = new[] { "main", "dev" },
    
    InvokedTargets = new[] { nameof(Compile) })]

[GitHubActions("Tests",
    GitHubActionsImage.UbuntuLatest,
    AutoGenerate = true,
    OnPushBranches = new[] { "main", "dev" },
    OnPullRequestBranches = new[] { "main", "dev" },
    InvokedTargets = new[] { nameof(Test) })]

[GitHubActions("Beta",
    GitHubActionsImage.UbuntuLatest,
    AutoGenerate = true,
    OnPushBranches = new[] { "dev" },
    InvokedTargets = new[] { nameof(PushBeta) })]

[GitHubActions("Publish",
    GitHubActionsImage.UbuntuLatest,
    AutoGenerate = true,
    OnPushBranches = new[] { "main" },
    InvokedTargets = new[] { nameof(Push) })]

[GitHubActions("Admin",
    GitHubActionsImage.UbuntuLatest,
    AutoGenerate = true,
    OnPushBranches = new[] { "admin" },
    InvokedTargets = new[] { nameof(ReleaseAdmin) })]

[GitHubActions("Sql",
    GitHubActionsImage.UbuntuLatest,
    AutoGenerate = true,
    OnPushBranches = new[] { "Sql" },
    InvokedTargets = new[] { nameof(ReleaseSql) })]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode 

    //public static int Main () => Execute<Build>(x => x.Test);
    public static int Main () => Execute<Build>(x => x.TxnTest);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    //readonly Configuration Configuration = Configuration.Release;
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;
    [GitVersion(Framework = "net5.0")] readonly GitVersion GitVersion;

    [Parameter] string NugetApiUrl = "https://api.nuget.org/v3/index.json"; 
    [Parameter] string GithubSource = "https://nuget.pkg.github.com/OWNER/index.json"; 

    //[Parameter] string NugetApiKey = Environment.GetEnvironmentVariable("SHARP_PULSAR_NUGET_API_KEY");
    [Parameter("NuGet API Key", Name = "NUGET_API_KEY")]
    readonly string NugetApiKey;
    
    [Parameter("Admin NuGet API Key", Name = "ADMIN_NUGET_KEY")]
    readonly string AdminNugetApiKey;
    
    [Parameter("Sql NuGet API Key", Name = "SQL_NUGET_KEY")]
    readonly string SqlNugetApiKey;
    
    [Parameter("GitHub Build Number", Name = "BUILD_NUMBER")]
    readonly string BuildNumber;

    [Parameter("GitHub Access Token for Packages", Name = "GH_API_KEY")]
    readonly string GitHubApiKey;

    [PackageExecutable("JetBrains.dotMemoryUnit", "dotMemoryUnit.exe")] readonly Tool DotMemoryUnit;

    AbsolutePath TestsDirectory => RootDirectory / "tests";
    AbsolutePath OutputDirectory => RootDirectory / "output";
    AbsolutePath TestSourceDirectory => RootDirectory / "SharpPulsar.Test";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            TestsDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            EnsureCleanDirectory(OutputDirectory);
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target RestoreAdmin => _ => _
        .Executes(() =>
        {
            var projectName = "SharpPulsar.Admin";
            var project = Solution.GetProject(projectName);
            DotNetRestore(s => s
                .SetProjectFile(project));
        });
    Target RestoreSql => _ => _
        .Executes(() =>
        {
            var projectName = "SharpPulsar.Sql";
            var project = Solution.GetProject(projectName);
            DotNetRestore(s => s
                .SetProjectFile(project));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetNoRestore(InvokedTargets.Contains(Restore))
                .SetConfiguration(Configuration));
        });
    Target CompileAdmin => _ => _
        .DependsOn(RestoreAdmin)
        .Executes(() =>
        {
            var projectName = "SharpPulsar.Admin";
            var project = Solution.GetProject(projectName);
            DotNetBuild(s => s
                .SetProjectFile(project)
                .SetNoRestore(InvokedTargets.Contains(Restore))
                .SetConfiguration(Configuration));
        });
    Target CompileSql => _ => _
        .DependsOn(RestoreSql)
        .Executes(() =>
        {
            var projectName = "SharpPulsar.Sql";
            var project = Solution.GetProject(projectName);
            DotNetBuild(s => s
                .SetProjectFile(project)
                .SetNoRestore(InvokedTargets.Contains(Restore))
                .SetConfiguration(Configuration));
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
        .DependsOn(AdminPulsar)
        .Executes(() =>
        {
            var testProject = "SharpPulsar.Test";
            var project = Solution.GetProject(testProject);
            Information($"Running tests from {project.Name}");

            foreach (var fw in project.GetTargetFrameworks())
            {
                if (fw.StartsWith("net4")
                    && RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                    && Environment.GetEnvironmentVariable("FORCE_LINUX_TESTS") != "1")
                {
                    Information($"Skipping {project.Name} ({fw}) tests on Linux - https://github.com/mono/mono/issues/13969");
                    continue;
                }

                Information($"Running for {project.Name} ({fw}) ...");
                try
                {
                    DotNetTest(c => c
                        .SetProjectFile(project)
                        .SetConfiguration(Configuration.ToString())
                        .SetFramework(fw)
                        //.SetDiagnosticsFile(TestsDirectory)
                        //.SetLogger("trx")
                        .SetVerbosity(verbosity: DotNetVerbosity.Normal)
                        .EnableNoBuild()); ;
                }
                catch (Exception ex)
                {
                    Information(ex.Message);
                }
            }
        });
    Target TestPaths => _ => _
        .Executes(() =>
        {
            Information($"Directory: { Solution.Directory}");
            Information($"Path: { Solution.Path}");
        });
    Target TestAdmin => _ => _
        .DependsOn(CompileAdmin)
        .DependsOn(AdminPulsar)
        .Triggers(StopPulsar)
        .Executes(() =>
        {
            var testProject = "SharpPulsar.Test.Admin";
            var project = Solution.GetProject(testProject);
            Information($"Running tests from {project.Name}");

            foreach (var fw in project.GetTargetFrameworks())
            {
                if (fw.StartsWith("net4")
                    && RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                    && Environment.GetEnvironmentVariable("FORCE_LINUX_TESTS") != "1")
                {
                    Information($"Skipping {project.Name} ({fw}) tests on Linux - https://github.com/mono/mono/issues/13969");
                    continue;
                }

                Information($"Running for {project.Name} ({fw}) ...");
                try
                {
                    DotNetTest(c => c
                        .SetProjectFile(project)
                        .SetConfiguration(Configuration.ToString())
                        .SetFramework(fw)
                        //.SetDiagnosticsFile(TestsDirectory)
                        //.SetLogger("trx")
                        .SetVerbosity(verbosity: DotNetVerbosity.Normal)
                        .EnableNoBuild()); ;
                }
                catch (Exception ex)
                {
                    Information(ex.Message);
                }
            }
        });
    Target TxnTest => _ => _
        .DependsOn(Test)
        .Triggers(StopPulsar)
        .Executes(() =>
        {
            var testProject = "SharpPulsar.Test.Transaction";
            var project = Solution.GetProject(testProject);
            Information($"Running tests from {project.Name}");

            foreach (var fw in project.GetTargetFrameworks())
            {
                if (fw.StartsWith("net4")
                    && RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                    && Environment.GetEnvironmentVariable("FORCE_LINUX_TESTS") != "1")
                {
                    Information($"Skipping {project.Name} ({fw}) tests on Linux - https://github.com/mono/mono/issues/13969");
                    continue;
                }

                Information($"Running for {project.Name} ({fw}) ...");
                try
                {
                    DotNetTest(c => c
                        .SetProjectFile(project)
                        .SetConfiguration(Configuration.ToString())
                        .SetFramework(fw)
                        //.SetDiagnosticsFile(TestsDirectory)
                        //.SetLogger("trx")
                        .SetVerbosity(verbosity: DotNetVerbosity.Normal)
                        .EnableNoBuild()); ;
                }
                catch (Exception ex)
                {
                    Information(ex.Message);
                }
            }
        });
    Target StartPulsar => _ => _
      .DependsOn(CheckDockerVersion)
      .Executes(() =>
       {
           DockerTasks.DockerRun(b =>
            b
            .SetDetach(true)
            .SetInteractive(true)
            .SetName("pulsar_test")
            .SetPublish("6650:6650", "8080:8080","8081:8081", "2181:2181")
            .SetMount("source=pulsardata,target=/pulsar/data")
            .SetMount("source=pulsarconf,target=/pulsar/conf")
            .SetImage("apachepulsar/pulsar-all:2.8.0")
            .SetEnv("PULSAR_MEM= -Xms512m -Xmx512m -XX:MaxDirectMemorySize=1g", @"PULSAR_PREFIX_acknowledgmentAtBatchIndexLevelEnabled=true", "PULSAR_PREFIX_nettyMaxFrameSizeBytes=5253120", @"PULSAR_PREFIX_transactionCoordinatorEnabled=true, PULSAR_PREFIX_brokerDeleteInactiveTopicsEnabled=false")
            .SetCommand("bash")
            .SetArgs("-c", "bin/apply-config-from-env.py conf/standalone.conf && bin/pulsar standalone -nss -nfw && bin/pulsar initialize-transaction-coordinator-metadata -cs localhost:2181 -c standalone --initial-num-transaction-coordinators 2")) ;
       });
    Target AdminPulsar => _ => _
      .DependsOn(StartPulsar)
      .Executes(() =>
       {
           Thread.Sleep(TimeSpan.FromSeconds(30));
           DockerTasks.DockerExec(x => x
                .SetContainer("pulsar_test")
                .SetCommand("bin/pulsar-admin")
                .SetArgs("clusters", "create", "standalone-cluster", "--url", "http://localhost:8080")
            );
           DockerTasks.DockerExec(x => x
                .SetContainer("pulsar_test")
                .SetCommand("bin/pulsar-admin")
                .SetArgs("tenants", "create", "tnx", "-r", "appid1", "--allowed-clusters", "standalone")
            );
           DockerTasks.DockerExec(x => x
                .SetContainer("pulsar_test")
                .SetCommand("bin/pulsar-admin")
                .SetArgs("namespaces","create", "tnx/ns1")
            );
           DockerTasks.DockerExec(x => x
                .SetContainer("pulsar_test")
                .SetCommand("bin/pulsar-admin")
                .SetArgs("namespaces","create", "public/deduplication")
            );
           DockerTasks.DockerExec(x => x
                .SetContainer("pulsar_test")
                .SetCommand("bin/pulsar-admin")
                .SetArgs("namespaces", "set-retention", "public/default","--time","3600", "--size", "-1")
            );
           DockerTasks.DockerExec(x => x
                .SetContainer("pulsar_test")
                .SetCommand("bin/pulsar-admin")
                .SetArgs("namespaces", "set-deduplication", "public/deduplication", "--enable")
            );
           DockerTasks.DockerExec(x => x
                .SetContainer("pulsar_test")
                .SetCommand("bin/pulsar-admin")
                .SetArgs("namespaces", "set-schema-validation-enforce", "--enable", "public/default")
            );
           DockerTasks.DockerExec(x => x
                .SetContainer("pulsar_test")
                .SetCommand("bin/pulsar-admin")
                .SetArgs("topics", "create-partitioned-topic", "persistent://pulsar/system/transaction_coordinator_assign", "--partitions", "16")
            );
           DockerTasks.DockerExec(x => x
                .SetContainer("pulsar_test")
                .SetCommand("bin/pulsar-admin")
                .SetArgs("topics", "create-partitioned-topic", "persistent://public/default/testReadFromPartition", "--partitions", "3")
            );
           DockerTasks.DockerExec(x => x
                .SetContainer("pulsar_test")
                .SetCommand("bin/pulsar-admin")
                .SetArgs("topics", "create-partitioned-topic", "persistent://public/default/TestReadMessageWithBatchingWithMessageInclusive", "--partitions", "3")
            );
           DockerTasks.DockerExec(x => x
                .SetContainer("pulsar_test")
                .SetCommand("bin/pulsar-admin")
                .SetArgs("topics", "create-partitioned-topic", "persistent://public/default/TestReadMessageWithoutBatchingWithMessageInclusive", "--partitions", "3")
            );
           DockerTasks.DockerExec(x => x
                .SetContainer("pulsar_test")
                .SetCommand("bin/pulsar-admin")
                .SetArgs("topics", "create-partitioned-topic", "persistent://public/default/TestReadMessageWithBatching", "--partitions", "3")
            );
           DockerTasks.DockerExec(x => x
                .SetContainer("pulsar_test")
                .SetCommand("bin/pulsar-admin")
                .SetArgs("topics", "create-partitioned-topic", "persistent://public/default/TestReadMessageWithoutBatching", "--partitions", "3")
            );
           /*DockerTasks.DockerExec(x => x
                .SetContainer("pulsar_test")
                .SetCommand("bin/pulsar")
                .SetArgs("sql-worker", "run")
            );*/
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
    Target Pack => _ => _
      .DependsOn(Compile)
      .Executes(() =>
      {
          var project = Solution.GetProject("SharpPulsar");
          DotNetPack(s => s
              .SetProject(project)
              .SetConfiguration(Configuration)
              .EnableNoBuild()
              .EnableNoRestore()
              .SetVersion($"2.0.0.{BuildNumber}")
              .SetPackageReleaseNotes("Support Avro DateTime and Decimal Logical Types via `SpecificDatumReader<T>`")
              .SetDescription("SharpPulsar is Apache Pulsar Client built using Akka.net")
              .SetPackageTags("Apache Pulsar", "Akka.Net", "Event Sourcing", "Distributed System", "Microservice")
              .AddAuthors("Ebere Abanonu (@mestical)")
              .SetPackageProjectUrl("https://github.com/eaba/SharpPulsar")
              .SetOutputDirectory(ArtifactsDirectory / "nuget")); ;

      });
    Target PackAdmin => _ => _
        .DependsOn(TestAdmin)
        .Executes(() =>
        {
            var project = Solution.GetProject("SharpPulsar.Admin");
            DotNetPack(s => s
                .SetProject(project)
                .SetConfiguration(Configuration)
                .EnableNoBuild()
                .EnableNoRestore()
                .SetVersion($"1.0.0.{BuildNumber}")
                .SetPackageReleaseNotes("First release SharpPulsar.Admin - this was taken from the main repo. How to use sample can be found here https://github.com/eaba/SharpPulsar/blob/Admin/Tests/SharpPulsar.Test.Admin/TransactionAPITest.cs")
                .SetDescription("Implements Apache Pulsar Admin and Function REST API.")
                .SetPackageTags("Apache Pulsar", "SharpPulsar")
                .AddAuthors("Ebere Abanonu (@mestical)")
                .SetPackageProjectUrl("https://github.com/eaba/SharpPulsar/tree/Admin/Extras/SharpPulsar.Admin")
                .SetOutputDirectory(ArtifactsDirectory / "nuget")); ;

        });

    Target PackSql => _ => _
        .DependsOn(CompileSql)
        .Executes(() =>
        {
            var project = Solution.GetProject("SharpPulsar.Sql");
            DotNetPack(s => s
                .SetProject(project)
                .SetConfiguration(Configuration)
                .EnableNoBuild()
                .EnableNoRestore()
                .SetAssemblyVersion($"1.0.{BuildNumber}")
                .SetVersion($"1.0.{BuildNumber}")
                .SetPackageReleaseNotes("Fix null exception when reading messages")
                .SetDescription("Implements Apache Pulsar Trino's REST API. For sample, visit https://github.com/eaba/SharpPulsar/blob/dev/Tests/SharpPulsar.Test.SQL/SqlTests.cs")
                .SetPackageTags("Apache Pulsar", "SharpPulsar", "Trino")
                .AddAuthors("Ebere Abanonu (@mestical)")
                .SetPackageProjectUrl("https://github.com/eaba/SharpPulsar/tree/Sql/Extras/SharpPulsar.Sql")
                .SetOutputDirectory(ArtifactsDirectory / "nuget")); ;

        });
    Target PackBeta => _ => _
      .DependsOn(Compile)
      .Executes(() =>
      {
          var project = Solution.GetProject("SharpPulsar");
          DotNetPack(s => s
              .SetProject(project)
              .SetConfiguration(Configuration)
              .EnableNoBuild()
              .EnableNoRestore()
              .SetAssemblyVersion("2.0.0")
              .SetVersionPrefix("2.0.0")
              .SetPackageReleaseNotes("Made it possible for user to handle connection errors")
              .SetVersionSuffix($"beta.{BuildNumber}")
              .SetDescription("SharpPulsar is Apache Pulsar Client built using Akka.net")
              .SetPackageTags("Apache Pulsar", "Akka.Net", "Event Sourcing", "Distributed System", "Microservice")
              .AddAuthors("Ebere Abanonu (@mestical)")
              .SetPackageProjectUrl("https://github.com/eaba/SharpPulsar")
              .SetOutputDirectory(ArtifactsDirectory / "nuget")); ;

      });
    Target PushBeta => _ => _
      .DependsOn(PackBeta)
      .Requires(() => NugetApiUrl)
      .Requires(() => !NugetApiKey.IsNullOrEmpty())
      .Requires(() => !GitHubApiKey.IsNullOrEmpty())
      .Requires(() => !BuildNumber.IsNullOrEmpty())
      .Requires(() => Configuration.Equals(Configuration.Release))
      .Executes(() =>
      {
          GlobFiles(ArtifactsDirectory / "nuget", "*.nupkg")
              .NotEmpty()
              .Where(x => !x.EndsWith("symbols.nupkg"))
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
    Target Push => _ => _
      .DependsOn(Pack)
      .Requires(() => NugetApiUrl)
      .Requires(() => !NugetApiKey.IsNullOrEmpty())
      .Requires(() => !GitHubApiKey.IsNullOrEmpty())
      .Requires(() => !BuildNumber.IsNullOrEmpty())
      .Requires(() => Configuration.Equals(Configuration.Release))
      .Executes(() =>
      {
          GlobFiles(ArtifactsDirectory / "nuget", "*.nupkg")
              .NotEmpty()
              .Where(x => !x.EndsWith("symbols.nupkg"))
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

    Target ReleaseAdmin => _ => _
      .DependsOn(PackAdmin)
      .Requires(() => NugetApiUrl)
      .Requires(() => !AdminNugetApiKey.IsNullOrEmpty())
      .Requires(() => !GitHubApiKey.IsNullOrEmpty())
      .Requires(() => !BuildNumber.IsNullOrEmpty())
      .Requires(() => Configuration.Equals(Configuration.Release))
      .Executes(() =>
      {
          GlobFiles(ArtifactsDirectory / "nuget", "*.nupkg")
              .NotEmpty()
              .Where(x => !x.EndsWith("symbols.nupkg"))
              .ForEach(x =>
              {
                  DotNetNuGetPush(s => s
                      .SetTargetPath(x)
                      .SetSource(NugetApiUrl)
                      .SetApiKey(AdminNugetApiKey)
                  );
              });
      });

    Target ReleaseSql => _ => _
        .DependsOn(PackSql)
        .Requires(() => NugetApiUrl)
        .Requires(() => !SqlNugetApiKey.IsNullOrEmpty())
        .Requires(() => !GitHubApiKey.IsNullOrEmpty())
        .Requires(() => !BuildNumber.IsNullOrEmpty())
        .Requires(() => Configuration.Equals(Configuration.Release))
        .Executes(() =>
        {
            GlobFiles(ArtifactsDirectory / "nuget", "*.nupkg")
                .NotEmpty()
                .Where(x => !x.EndsWith("symbols.nupkg"))
                .ForEach(x =>
                {
                    DotNetNuGetPush(s => s
                        .SetTargetPath(x)
                        .SetSource(NugetApiUrl)
                        .SetApiKey(SqlNugetApiKey)
                    );
                });
        });

    static void Information(string info)
    {
        Logger.Info(info);
    }
}
