using System;
using System.Linq;
using System.Runtime.InteropServices;
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

    public static int Main () => Execute<Build>(x => x.Compile);

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
        .DependsOn(StartPulsar)
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

                DotNetTest(c => c
                    .SetProjectFile(project)
                    .SetConfiguration(Configuration.ToString())
                    .SetFramework(fw)
                    .EnableNoBuild()
                    .SetNoRestore(InvokedTargets.Contains(Restore)));
            }
        });
    Target StartPulsar => _ => _
      .DependsOn(CheckDockerVersion)
      .Executes(() =>
      {
          DockerTasks.DockerRun(b =>
          b
          .SetDetach(true)
          .SetName("pulsar_test")
          .SetPublish("6650:6650")
          .SetPublish("8080:8080")
          .SetMount("source=pulsardata,target=/pulsar/data")
          .SetMount("source=pulsarconf,target=/pulsar/conf")
          .SetImage("apachepulsar/pulsar:2.7.0")
          .SetCommand("/pulsar standalone")
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
    .After(Test)
    .AssuredAfterFailure()
    .Executes(() =>
    {

        DockerTasks.DockerRm(b => b
          .SetContainers("pulsar_test")
          .SetForce(true));

    });
    static void Information(string info)
    {
        Logger.Info(info);
    }
}
