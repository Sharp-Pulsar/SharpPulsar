using System.Collections.Generic;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Execution;
using Nuke.Common.Utilities;

[CustomGitHubActions("build",
    GitHubActionsImage.WindowsLatest,
    GitHubActionsImage.UbuntuLatest,
    //AutoGenerate = false,
    FetchDepth = 0,
    OnPushBranches = new[] { "main", "dev", "release" },
    OnPullRequestBranches = new[] { "main", "dev", "release" },
    InvokedTargets = new[] { nameof(Compile), nameof(API) },
    PublishArtifacts = false,
    EnableGitHubToken = false)]

[CustomGitHubActions("test",
    GitHubActionsImage.UbuntuLatest,
    //AutoGenerate = false,
    FetchDepth = 0,
    OnPullRequestBranches = new[] { "main", "dev", "release" },
    OnPushBranches = new[] { "main", "dev", "release" },
    InvokedTargets = new[] { nameof(Test) },
    PublishArtifacts = false,
    EnableGitHubToken = false)]

[CustomGitHubActions("nuget",
    GitHubActionsImage.WindowsLatest,
    //AutoGenerate = false,
    FetchDepth = 0,
    OnPushTags = new[] { "*" },
    InvokedTargets = new[] { nameof(PublishNuget) },
    ImportSecrets = new[] { "NUGET_API_KEY"},
    PublishArtifacts = true,
    EnableGitHubToken = true)]

[CustomGitHubActions("docs",
    GitHubActionsImage.UbuntuLatest,
    AutoGenerate = false,
    FetchDepth = 0,
    OnPullRequestBranches = new[] { "main", "dev", "release" },
    OnPushBranches = new[] { "main", "dev", "release" },
    InvokedTargets = new[] { nameof(DocBuild) },
    PublishArtifacts = true,
    EnableGitHubToken = true)]


partial class Build
{

}
public class CustomGitHubActionsAttribute : GitHubActionsAttribute
{
    public CustomGitHubActionsAttribute(string name, GitHubActionsImage image, params GitHubActionsImage[] images) : base(name, image, images)
    {
    }

    protected override GitHubActionsJob GetJobs(GitHubActionsImage image, IReadOnlyCollection<ExecutableTarget> relevantTargets)
    {
        var job = base.GetJobs(image, relevantTargets);
        
        var newSteps = new List<GitHubActionsStep>(job.Steps);
        //newSteps.Insert(1, new GitHubActionsUploadArtifact{ });
        foreach (var version in new[] { "8.0.*"})
        {            
            newSteps.Insert(1, new GitHubActionsSetupDotNetStep
            {
                Version = version
            });
        }
        newSteps.Insert(1, new GitHubActionsSetupChmod
        {
            File = "build.cmd"
        });
        newSteps.Insert(1, new GitHubActionsSetupChmod
        {
            File = "build.sh"
        });
        job.Steps = newSteps.ToArray();
        return job;
    }
}

public class GitHubActionsSetupDotNetStep : GitHubActionsStep
{
    public string Version { get; init; }

    public override void Write(CustomFileWriter writer)
    {
        writer.WriteLine("- uses: actions/setup-dotnet@v3");

        using (writer.Indent())
        {
            writer.WriteLine("with:");
            using (writer.Indent())
            {
                writer.WriteLine($"dotnet-version: {Version}");
            }
        }
    }
}

class GitHubActionsSetupChmod : GitHubActionsStep
{
    public string File { get; init; }

    public override void Write(CustomFileWriter writer)
    {
        writer.WriteLine($"- name: Make {File} executable");
        using (writer.Indent())
        {
            writer.WriteLine($"run: chmod +x ./{File}");
        }
    }
}