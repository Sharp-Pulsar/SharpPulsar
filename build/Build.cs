using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.ChangeLog;
using Nuke.Common.CI;
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
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.ChangeLog.ChangelogTasks;
using static Nuke.Common.Tools.Git.GitTasks;
using static Nuke.Common.Tools.DocFX.DocFXTasks;
using Nuke.Common.Tools.DocFX;
using System.IO;
using System.Collections.Generic;
using Octokit;
using System.Threading.Tasks;
using Nuke.Common.Tools.MSBuild;
using Nuke.Common.CI.GitHubActions;
//https://github.com/AvaloniaUI/Avalonia/blob/master/nukebuild/Build.cs
//https://github.com/cfrenzel/Eventfully/blob/master/build/Build.cs


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

    public static int Main () => Execute<Build>(x => x.Test);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    //readonly Configuration Configuration = Configuration.Release;
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Required][Solution] public readonly Solution Solution;
    [Required][GitRepository] public readonly GitRepository GitRepository;
    [Required][GitVersion(Framework = "net8.0")] public readonly GitVersion GitVersion = default!;
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

    //usage:
    //./build.cmd RunChangelog --major-minor-patch 2.14.1
    [Parameter] string MajorMinorPatch = "2.14.0";

    GitHubClient GitHubClient;
    public ChangeLog Changelog => MdHelper.ReadChangelog(ChangelogFile);
    string TagVersion => GitVersion.MajorMinorPatch;

    bool IsTaggedBuild => !string.IsNullOrWhiteSpace(TagVersion);

    string VersionSuffix;
    protected override void OnBuildInitialized()
    {
        VersionSuffix = !IsTaggedBuild
            ? $"dev-{DateTime.UtcNow:yyyyMMdd-HHmm}"
            : "";

        if (IsLocalBuild)
        {
            VersionSuffix = $"dev-{DateTime.UtcNow:yyyyMMdd-HHmm}";
        }

        Information("BUILD SETUP");
        Information($"Configuration:\t{Configuration}");
        Information($"Version suffix:\t{VersionSuffix}");
        Information($"Tagged build:\t{IsTaggedBuild}");
    }
    public ReleaseNotes LatestVersion => Changelog.ReleaseNotes.OrderByDescending(s => s.Version).FirstOrDefault() ?? throw new ArgumentException("Bad Changelog File. Version Should Exist");
    public string ReleaseVersion => LatestVersion.Version?.ToString() ?? throw new ArgumentException("Bad Changelog File. Define at least one version");

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            RootDirectory
            .GlobDirectories("sharppulsar/bin", "sharppulsar/obj", Output, OutputTests, OutputPerfTests, OutputNuget, DocSiteDirectory)
            .ForEach(AbsolutePathExtensions.DeleteDirectory);
            AbsolutePathExtensions.CreateOrCleanDirectory(Output);
        });

    Target Restore => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });
    Target Tests => _ => _
    .DependsOn(Test);
    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            //var vers = GitVersion.MajorMinorPatch;
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoRestore());
        });
    IEnumerable<string> ChangelogSectionNotes => ExtractChangelogSectionNotes(ChangelogFile);

    Target RunChangelog => _ => _
        .Requires(() => IsLocalBuild)
        //.OnlyWhenDynamic(() => GitRepository.Branch.Equals("main", StringComparison.OrdinalIgnoreCase))
        .Executes(() =>
        {
            // ./build.cmd RunChangelog --major-minor-patch 2.15.1
            FinalizeChangelog(ChangelogFile, MajorMinorPatch, GitRepository);
            Information($"Please review CHANGELOG.md ({ChangelogFile}) and press 'Y' to validate (any other key will cancel changes)...");
            ConsoleKeyInfo keyInfo = Console.ReadKey();
            if (keyInfo.Key == ConsoleKey.Y)
            {
                Git($"add {ChangelogFile}");
                Information($"Add:\t{ChangelogFile}");
                //Git($"commit -m \"Finalize {Path.GetFileName(ChangelogFile)} for {GitVersion.MajorMinorPatch}.\"");

                //Git($"tag -f {GitVersion.MajorMinorPatch}");

                //git push --atomic origin <branch name> <tag>
                //https://gist.github.com/danielestevez/2044589
                //https://www.atlassian.com/git/tutorials/merging-vs-rebasing./build.cmd createnuget
                //https://git-scm.com/docs/git-push
                //Kleopatra certificates
                // https://stackoverflow.com/questions/55111685/git-doesnt-see-gpg-key-as-secret-even-though-it-is-how-do-i-fix-it/55126589#55126589
                //https://git-scm.com/docs/git-config#Documentation/git-config.txt-gpgprogram
                //https://gist.github.com/paolocarrasco/18ca8fe6e63490ae1be23e84a7039374?permalink_comment_id=3976510
                //https://docs.github.com/en/authentication/managing-commit-signature-verification/telling-git-about-your-signing-key
                //http://irtfweb.ifa.hawaii.edu/~lockhart/gpg/
                //https://devconnected.com/how-to-delete-local-and-remote-tags-on-git/
                //https://docs.github.com/en/actions/managing-workflow-runs/skipping-workflow-runs
                //https://www.ankursheel.com/blog/securing-git-commits-windows
                //https://tau.gr/posts/2018-06-29-how-to-set-up-signing-commits-with-git/
            }
        });

    Target Test => _ => _        
        .DependsOn(Compile)
        .Executes(() =>
        {
            
            var projects = new List<string> 
            {
                "SharpPulsar.Test",
                //"SharpPulsar.Trino.Test",
                //"SharpPulsar.Admin.Test"
            };

            foreach (var projectName in projects)
            {
                   var project = Solution.GetProject(projectName).NotNull("project != null");
                   Information($"Running tests from {project}");
                    foreach (var fw in project.GetTargetFrameworks())
                    {
                    
                         DotNetTest(c => c
                        .SetProjectFile(project)
                        .SetConfiguration(Configuration)
                        .SetFramework(fw)
                        .EnableNoBuild()
                        .SetBlameCrash(true)
                        .SetBlameHang(true)
                        .SetBlameHangTimeout("30m")
                        .EnableNoRestore()
                        .When(true, _ => _
                            .SetLoggers("console;verbosity=detailed")
                            .SetResultsDirectory(OutputTests)));
                    }
            }            
            
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
        .Produces(DocFxDir / "_site")
        .DependsOn(DocsMetadata)
        .Executes(() =>
        {
            DocFXBuild(s => s
            .SetConfigFile(DocFxDirJson)
            .SetLogLevel(DocFXLogLevel.Verbose));
        });

    Target ServeDocs => _ => _
        //.DependsOn(DocBuild)
        .OnlyWhenStatic(() => IsLocalBuild)
        .Executes(() => DocFXServe(s => s.SetFolder(DocFxDir).SetPort(9090)));

    Target CreateNuget => _ => _
      .DependsOn(Compile)
      //.DependsOn(Test)
      //.DependsOn(Token)
      .Executes(() =>
      {
          var branchName = GitRepository.Branch;

          var releaseNotes = GetNuGetReleaseNotes(ChangelogFile, GitRepository);
          var projects = new List<string>
            {
                "SharpPulsar",
                "SharpPulsar.Trino",
                "SharpPulsar.Admin"
            };
          foreach (var projectName in projects)
          {
              var project = Solution.GetProject(projectName).NotNull("project != null");
              DotNetPack(s => s
              .SetProject(project)
              .SetConfiguration(Configuration)
              //.EnableNoBuild()
              .EnableNoRestore()
              //.SetIncludeSymbols(true)
              .SetAssemblyVersion(TagVersion)
              .SetVersion(TagVersion)
              .SetFileVersion(TagVersion)
              .SetInformationalVersion(TagVersion)
              //.SetVersionSuffix(TagVersion)
              .SetPackageReleaseNotes(releaseNotes)
              .SetDescription("SharpPulsar is Apache Pulsar Client built using Akka.net")
              .SetPackageTags("Apache Pulsar", "Akka.Net", "Event Driven", "Event Sourcing", "Distributed System", "Microservice")
              .AddAuthors("Ebere Abanonu (@mestical)")
              .SetPackageProjectUrl("https://github.com/eaba/SharpPulsar")
              .SetOutputDirectory(OutputNuget));
          }
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
    Target Install => _ => _
       .Description("Install `Nuke.GlobalTool`")
       .Executes(() =>
       {
           DotNetTasks.DotNet($"tool install Nuke.GlobalTool --global");
       });
    Target protobuf => _ => _
       
       .Executes(() =>
       {
           DotNetTasks.DotNet($"tool install Nuke.GlobalTool --global");
       });
    async Task GitHubRelease ()
        {
            GitHubClient = new GitHubClient(new ProductHeaderValue("nuke-build"))
            {
                Credentials = new Credentials(GitHubActions.Token, AuthenticationType.Bearer)
            };
            var version = TagVersion;
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