﻿using System.Collections.Generic;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Execution;
using Nuke.Common.Utilities;

[CustomGitHubActions("build",
    GitHubActionsImage.WindowsLatest,
    GitHubActionsImage.UbuntuLatest,
    AutoGenerate = false,
    OnPushBranches = new[] { "main", "dev" },
    OnPullRequestBranches = new[] { "release" },
    InvokedTargets = new[] { nameof(Compile) })]

[CustomGitHubActions("pr_validation",
    GitHubActionsImage.UbuntuLatest,
    AutoGenerate = false,
    OnPullRequestBranches = new[] { "main", "dev", "release" },
    InvokedTargets = new[] { nameof(Test) })]

[CustomGitHubActions("release",
    GitHubActionsImage.UbuntuLatest,
    AutoGenerate = false,
    OnPushBranches = new[] { "release" },
    InvokedTargets = new[] { nameof(PublishNuget) })]

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
        writer.WriteLine("- uses: actions/setup-dotnet@v1");

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