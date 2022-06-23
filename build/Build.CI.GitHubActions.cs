using System.Collections.Generic;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Execution;
using Nuke.Common.Utilities;

[CustomGitHubActions("build",
    GitHubActionsImage.WindowsLatest,
    GitHubActionsImage.UbuntuLatest,    
    AutoGenerate = true,
    OnPushBranches = new[] { "main", "dev", "release" },
    OnPullRequestBranches = new[] { "main", "dev", "release" },
    InvokedTargets = new[] { nameof(Compile) })]

[CustomGitHubActions("run_tests",
    GitHubActionsImage.UbuntuLatest,
    AutoGenerate = true,
    OnPullRequestBranches = new[] { "main", "dev", "release" },
    OnPushBranches = new[] { "main", "dev", "release" },
    InvokedTargets = new[] { nameof(Test) },
    PublishArtifacts = true)]


[CustomGitHubActions("run_tests_acks",
    GitHubActionsImage.UbuntuLatest,
    AutoGenerate = true,
    OnPullRequestBranches = new[] { "main", "dev", "release" },
    OnPushBranches = new[] { "main", "dev", "release" },
    InvokedTargets = new[] { nameof(Acks) },
    PublishArtifacts = true)]


[CustomGitHubActions("run_tests_partitioned",
    GitHubActionsImage.UbuntuLatest,
    AutoGenerate = true,
    OnPullRequestBranches = new[] { "main", "dev", "release" },
    OnPushBranches = new[] { "main", "dev", "release" },
    InvokedTargets = new[] { nameof(Partitioned) },
    PublishArtifacts = true)]

[CustomGitHubActions("run_tests_transaction",
    GitHubActionsImage.UbuntuLatest,
    AutoGenerate = true,
    OnPullRequestBranches = new[] { "main", "dev", "release" },
    OnPushBranches = new[] { "main", "dev", "release" },
    InvokedTargets = new[] { nameof(Transaction) },
    PublishArtifacts = true)]


[CustomGitHubActions("run_tests_api",
    GitHubActionsImage.UbuntuLatest,
    AutoGenerate = true,
    OnPullRequestBranches = new[] { "main", "dev", "release" },
    OnPushBranches = new[] { "main", "dev", "release" },
    InvokedTargets = new[] { nameof(TestAPI) },
    PublishArtifacts = true)]

[CustomGitHubActions("run_tests_autocluster",
    GitHubActionsImage.UbuntuLatest,
    AutoGenerate = true,
    OnPullRequestBranches = new[] { "main", "dev", "release" },
    OnPushBranches = new[] { "main", "dev", "release" },
    InvokedTargets = new[] { nameof(AutoClusterFailover) },
    PublishArtifacts = true)]

[CustomGitHubActions("run_tests_tableview",
    GitHubActionsImage.UbuntuLatest,
    AutoGenerate = true,
    OnPullRequestBranches = new[] { "main", "dev", "release" },
    OnPushBranches = new[] { "main", "dev", "release" },
    InvokedTargets = new[] { nameof(TableView) },
    PublishArtifacts = true)]

[CustomGitHubActions("run_tests_eventsource",
    GitHubActionsImage.UbuntuLatest,
    AutoGenerate = true,
    OnPullRequestBranches = new[] { "main", "dev", "release" },
    OnPushBranches = new[] { "main", "dev", "release" },
    InvokedTargets = new[] { nameof(EventSource) },
    PublishArtifacts = true)]

[CustomGitHubActions("run_tests_multitopic",
    GitHubActionsImage.UbuntuLatest,
    AutoGenerate = true,
    OnPullRequestBranches = new[] { "main", "dev", "release" },
    OnPushBranches = new[] { "main", "dev", "release" },
    InvokedTargets = new[] { nameof(MultiTopic) },
    PublishArtifacts = true)]

[CustomGitHubActions("nuget",
    GitHubActionsImage.WindowsLatest,
    AutoGenerate = true,
    OnPushBranches = new[] { "main", "dev", "release" },
    InvokedTargets = new[] { nameof(PublishNuget) },
    ImportSecrets = new[] { "NUGET_API_KEY", "GITHUB_TOKEN" })]


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
        foreach (var version in new[] { "6.0.*", "5.0.*" })
        {            
            newSteps.Insert(1, new GitHubActionsSetupDotNetStep
            {
                Version = version
            });
        }

        job.Steps = newSteps.ToArray();
        return job;
    }
}

public class GitHubActionsSetupDotNetStep : GitHubActionsStep
{
    public string Version { get; init; }

    public override void Write(CustomFileWriter writer)
    {
        writer.WriteLine("- uses: actions/setup-dotnet@v2");

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

public class GitHubActionsUploadArtifact : GitHubActionsStep
{
    public override void Write(CustomFileWriter writer)
    {
        writer.WriteLine("- name: Upload a Build Artifact");

        using (writer.Indent())
        {
            writer.WriteLine("uses: actions/upload-artifact@v3.1.0");
            writer.WriteLine("with:");
            using (writer.Indent())
            {
                writer.WriteLine("name: assets-for-download");
                writer.WriteLine("path: /home/runner/work/SharpPulsar/SharpPulsar/TestResults");
            }
        }
    }
}