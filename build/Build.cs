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
using Nuke.Common.Tools.Docker;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.Xunit;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
//https://github.com/AvaloniaUI/Avalonia/blob/master/nukebuild/Build.cs
[CheckBuildProjectConfigurations]
[ShutdownDotNetAfterServerBuild]
[GitHubActions("Build",
    GitHubActionsImage.WindowsLatest,
    GitHubActionsImage.UbuntuLatest,
    AutoGenerate = true,
    OnPushBranches = new[] { "master", "dev" },
    OnPullRequestBranches = new[] { "master", "dev" },
    
    InvokedTargets = new[] { nameof(Compile) })]

[GitHubActions("Tests",
    GitHubActionsImage.UbuntuLatest,
    AutoGenerate = true,
    OnPushBranches = new[] { "master", "dev" },
    OnPullRequestBranches = new[] { "master", "dev" },
    InvokedTargets = new[] { nameof(Test) })]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode 

    public static int Main () => Execute<Build>(x => x.Test);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;
    [GitVersion] readonly GitVersion GitVersion;

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

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetNoRestore(InvokedTargets.Contains(Restore))
                .SetConfiguration(Configuration));
        });
    Target Test => _ => _
        .DependsOn(Compile)
        .DependsOn(AdminPulsar)
        .Triggers(StopPulsar)
        .Executes(() =>
        {
            var projectName = "SharpPulsar.Test";
            var project = Solution.GetProjects("*.Test").First();
            Information($"Running tests from {projectName}");

            foreach (var fw in project.GetTargetFrameworks())
            {
                if (fw.StartsWith("net4")
                    && RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                    && Environment.GetEnvironmentVariable("FORCE_LINUX_TESTS") != "1")
                {
                    Information($"Skipping {projectName} ({fw}) tests on Linux - https://github.com/mono/mono/issues/13969");
                    continue;
                }

                Information($"Running for {projectName} ({fw}) ...");
                try
                {
                    DotNetTest(c => c
                        .SetProjectFile(project)
                        .SetConfiguration(Configuration.ToString())
                        .SetFramework(fw)
                        .SetVerbosity(verbosity: DotNetVerbosity.Normal)
                        .EnableNoBuild()); ;
                }
                catch(Exception ex)
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
            .SetPublish("6650:6650", "8080:8080", "2181:2181")
            .SetMount("source=pulsardata,target=/pulsar/data")
            .SetMount("source=pulsarconf,target=/pulsar/conf")
            .SetImage("apachepulsar/pulsar-all:2.7.0")
            .SetEnv(@"PULSAR_PREFIX_acknowledgmentAtBatchIndexLevelEnabled=true", "PULSAR_PREFIX_nettyMaxFrameSizeBytes=5253120",@"PULSAR_PREFIX_transactionCoordinatorEnabled=true")
            .SetCommand("bash")
            .SetArgs("-c", "bin/set_python_version.sh && bin/apply-config-from-env.py conf/standalone.conf && bin/pulsar standalone")) ;
       });
    Target AdminPulsar => _ => _
      .DependsOn(StartPulsar)
      .Executes(() =>
       {
           Thread.Sleep(TimeSpan.FromSeconds(30));
           DockerTasks.DockerExec(x => x
                .SetContainer("pulsar_test")
                .SetCommand("bin/pulsar-admin")
                .SetArgs("namespaces","create","public/retention")
            );
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
                .SetArgs("tenants", "create", "pulsar", "-r", "appid1", "--allowed-clusters", "standalone")
            );
           DockerTasks.DockerExec(x => x
                .SetContainer("pulsar_test")
                .SetCommand("bin/pulsar-admin")
                .SetArgs("namespaces","create", "tnx/ns1")
            );
           DockerTasks.DockerExec(x => x
                .SetContainer("pulsar_test")
                .SetCommand("bin/pulsar-admin")
                .SetArgs("namespaces","create", "pulsar/system")
            );
           DockerTasks.DockerExec(x => x
                .SetContainer("pulsar_test")
                .SetCommand("bin/pulsar-admin")
                .SetArgs("namespaces","create", "public/deduplication")
            );
           DockerTasks.DockerExec(x => x
                .SetContainer("pulsar_test")
                .SetCommand("bin/pulsar-admin")
                .SetArgs("namespaces", "set-retention", "public/retention","--time","3h", "--size", "1G")
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
                .SetArgs("namespaces", "set-schema-validation-enforce", "--enable", "public/default")
            );
           DockerTasks.DockerExec(x => x
                .SetContainer("pulsar_test")
                .SetCommand("bin/pulsar-admin")
                .SetArgs("topics", "create-partitioned-topic", "persistent://public/default/partitioned", "--partitions", "3")
            );
           DockerTasks.DockerExec(x => x
                .SetContainer("pulsar_test")
                .SetCommand("bin/pulsar-admin")
                .SetArgs("topics", "create-partitioned-topic", "persistent://pulsar/system/transaction_coordinator_assign", "--partitions", "16")
            );
           DockerTasks.DockerExec(x => x
                .SetContainer("pulsar_test")
                .SetCommand("bin/pulsar-admin")
                .SetArgs("topics", "create-partitioned-topic", "persistent://tnx/ns1/output", "--partitions", "3")
            );
           DockerTasks.DockerExec(x => x
                .SetContainer("pulsar_test")
                .SetCommand("bin/pulsar-admin")
                .SetArgs("topics", "create-partitioned-topic", "persistent://tnx/ns1/message-ack-test", "--partitions", "3")
            );
           DockerTasks.DockerExec(x => x
                .SetContainer("pulsar_test")
                .SetCommand("bin/pulsar-admin")
                .SetArgs("topics", "create-partitioned-topic", "persistent://public/default/partitioned2", "--partitions", "2")
            );
           DockerTasks.DockerExec(x => x
                .SetContainer("pulsar_test")
                .SetCommand("bin/pulsar-admin")
                .SetArgs("topics", "create-partitioned-topic", "persistent://public/default/partitioned3", "--partitions", "3")
            );
           DockerTasks.DockerExec(x => x
                .SetContainer("pulsar_test")
                .SetCommand("bin/pulsar-admin")
                .SetArgs("topics", "create-partitioned-topic", "persistent://public/default/partitioned4", "--partitions", "2")
            );
           DockerTasks.DockerExec(x => x
                .SetContainer("pulsar_test")
                .SetCommand("bin/pulsar-admin")
                .SetArgs("topics", "create-partitioned-topic", "persistent://public/deduplication/partitioned", "--partitions", "3")
            );
           DockerTasks.DockerExec(x => x
                .SetContainer("pulsar_test")
                .SetCommand("bin/pulsar")
                .SetArgs("initialize-transaction-coordinator-metadata", "-cs", "localhost:2181", "-c", "standalone", "--initial-num-transaction-coordinators", "16")
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
    static void Information(string info)
    {
        Logger.Info(info);
    }
}
