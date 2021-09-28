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

[GitHubActions("Admin",
    GitHubActionsImage.UbuntuLatest,
    AutoGenerate = true,
    OnPushBranches = new[] { "Admin" },
    InvokedTargets = new[] { nameof(ReleaseAdmin) })]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode 

    //public static int Main () => Execute<Build>(x => x.Test);
    public static int Main () => Execute<Build>(x => x.TestAdmin);

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
    
    [Parameter("GitHub Build Number", Name = "BUILD_NUMBER")]
    readonly string BuildNumber;

    [Parameter("GitHub Access Token for Packages", Name = "GH_API_KEY")]
    readonly string GitHubApiKey;

    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";

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
    Target StartPulsar => _ => _
        .DependsOn(CheckDockerVersion)
        .Executes(() =>
        {
            DockerTasks.DockerRun(b =>
                b
                    .SetDetach(true)
                    .SetInteractive(true)
                    .SetName("pulsar_test")
                    .SetPublish("6650:6650", "8080:8080", "8081:8081", "2181:2181")
                    .SetMount("source=pulsardata,target=/pulsar/data")
                    .SetMount("source=pulsarconf,target=/pulsar/conf")
                    .SetImage("apachepulsar/pulsar-all:2.8.0")
                    .SetEnv("PULSAR_MEM= -Xms512m -Xmx512m -XX:MaxDirectMemorySize=1g", @"PULSAR_PREFIX_acknowledgmentAtBatchIndexLevelEnabled=true", "PULSAR_PREFIX_nettyMaxFrameSizeBytes=5253120", @"PULSAR_PREFIX_transactionCoordinatorEnabled=true, PULSAR_PREFIX_brokerDeleteInactiveTopicsEnabled=false")
                    .SetCommand("bash")
                    .SetArgs("-c", "bin/apply-config-from-env.py conf/standalone.conf && bin/pulsar standalone -nss -nfw && bin/pulsar initialize-transaction-coordinator-metadata -cs localhost:2181 -c standalone --initial-num-transaction-coordinators 2"));
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
                .SetAssemblyVersion("1.2.0")
                .SetVersion("1.2.0")
                .SetPackageReleaseNotes("Fix NRE")
                .SetDescription("Implements Apache Pulsar Admin and Function REST API.")
                .SetPackageTags("Apache Pulsar", "SharpPulsar")
                .AddAuthors("Ebere Abanonu (@mestical)")
                .SetPackageProjectUrl("https://github.com/eaba/SharpPulsar/tree/Admin/Extras/SharpPulsar.Admin")
                .SetOutputDirectory(ArtifactsDirectory / "nuget")); ;

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


    static void Information(string info)
    {
        Logger.Info(info);
    }
}
