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
    OnPushBranches = new[] { "Sql" },
    OnPullRequestBranches = new[] { "Sql" },
    
    InvokedTargets = new[] { nameof(Compile) })]

[GitHubActions("Tests",
    GitHubActionsImage.UbuntuLatest,
    AutoGenerate = true,
    OnPushBranches = new[] { "Sql" },
    OnPullRequestBranches = new[] { "Sql" },
    InvokedTargets = new[] { nameof(Test) })]

[GitHubActions("Release",
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
    public static int Main () => Execute<Build>(x => x.Test);

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
    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetNoRestore(InvokedTargets.Contains(Restore))
                .SetFileVersion(Version())
                .SetConfiguration(Configuration));
        });
    Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            var project = Solution.GetProject("SharpPulsar.Sql.Tests");
            Information($"Running tests from {project.Name}");

            foreach (var fw in project.GetTargetFrameworks())
            {
                Information($"Running for {project.Name} ({fw}) ...");
                DotNetTest(c => c
                        .SetProjectFile(project)
                        .SetConfiguration(Configuration.ToString())
                        .SetFramework(fw)
                        //.SetDiagnosticsFile(TestsDirectory)
                        //.SetLoggers("trx")
                        .SetVerbosity(verbosity: DotNetVerbosity.Normal)
                        .EnableNoBuild()); 
            }
        });
    Target PackSql => _ => _
        .DependsOn(Test)
        .Executes(() =>
        {
            var project = Solution.GetProject("SharpPulsar.Sql");
            DotNetPack(s => s
                .SetProject(project)
                .SetConfiguration(Configuration)
                .EnableNoBuild()
                .EnableNoRestore()
                .SetAssemblyVersion(Version())
                .SetVersion(Version())
                .SetPackageReleaseNotes("Maintenance Release")
                .SetDescription("Implements Apache Pulsar Trino's REST API. For sample visit https://github.com/eaba/SharpPulsar/tree/Sql/Extras/SharpPulsar.Sql.Tests")
                .SetPackageTags("Apache Pulsar", "SharpPulsar", "Trino")
                .AddAuthors("Ebere Abanonu (@mestical)")
                .SetPackageProjectUrl("https://github.com/eaba/SharpPulsar/tree/Sql/Extras/SharpPulsar.Sql")
                .SetOutputDirectory(ArtifactsDirectory / "nuget")); ;

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

    static string Version()
    {
        return "2.1.1";
    }
    static void Information(string info)
    {
        Serilog.Log.Information(info);
    }
}
