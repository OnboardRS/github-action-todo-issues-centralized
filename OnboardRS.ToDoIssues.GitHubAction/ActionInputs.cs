﻿using CommandLine;

namespace OnboardRS.ToDoIssues.GitHubAction;

public class ActionInputs
{
	string _codeRepoRepositoryName = null!;
	string _codeRepoBranchName = null!;
	string _codeRepoOwner = null!;
	string _codeRepoNodeId = null!;
	string _issueRepoRepositoryName = null!;
	string _issueRepoBranchName = null!;
	string _issueRepoOwner = null!;
	string _issueRepoNodeId = null!;
	string _issueLabel = null!;


	[Option("code-repo-owner",
			   Required = true,
			   HelpText = "The code repo owner, for example: \"OnboardRS\". Assign from `github.repository_owner`.")]
	public string CodeRepoOwner
	{
		get => _codeRepoOwner;
		set => ParseAndAssign(value, str => _codeRepoOwner = str);
	}

	[Option("code-repo-name",
			   Required = true,
			   HelpText = "The code repo repository name, for example: \"github-action-todo-issues-centralized\". Assign from `github.repository`.")]
	public string CodeRepoName
	{
		get => _codeRepoRepositoryName;
		set => ParseAndAssign(value, str => _codeRepoRepositoryName = str);
	}

	[Option("code-repo-branch",
			   Required = true,
			   HelpText = "The code repo branch name, for example: \"refs/heads/develop\". Assign from `github.ref`.")]
	public string CodeRepoBranch
	{
		get => _codeRepoBranchName;
		set => ParseAndAssign(value, str => _codeRepoBranchName = str);
	}

	[Option("code-repo-node-id",
			   Required = true,
			   HelpText = "The code repo node id, for example: \"R_kgDOHlz\". Assign from `github.repository.node_id`.")]
	public string CodeRepoNodeId
	{
		get => _codeRepoNodeId;
		set => ParseAndAssign(value, str => _codeRepoNodeId = str);
	}

	[Option("issue-repo-owner",
			   Required = true,
			   HelpText = "The issue repo owner, for example: \"OnboardRS\". Assign from centralized issue repo.")]
	public string IssueRepoOwner
	{
		get => _issueRepoOwner;
		set => ParseAndAssign(value, str => _issueRepoOwner = str);
	}

	[Option("issue-repo-name",
			   Required = true,
			   HelpText = "The issue repo repository name, for example: \"zenhub-dev\". Assign from centralized issue repo.")]
	public string IssueRepoName
	{
		get => _issueRepoRepositoryName;
		set => ParseAndAssign(value, str => _issueRepoRepositoryName = str);
	}

	[Option("issue-repo-branch",
			   Required = true,
			   HelpText = "The issue repo branch name, for example: \"refs/heads/develop\". Assign from centralized issue repo.")]
	public string IssueRepoBranch
	{
		get => _issueRepoBranchName;
		set => ParseAndAssign(value, str => _issueRepoBranchName = str);
	}

	[Option("issue-repo-node-id",
			   Required = true,
			   HelpText = "The issue repo node id, for example: \"R_kgDOHlz\". Assign from centralized issue repo.")]
	public string IssueRepoNodeId
	{
		get => _issueRepoNodeId;
		set => ParseAndAssign(value, str => _issueRepoNodeId = str);
	}

	[Option("issue-label",
			   Required = true,
			   HelpText = "The issue label TODO GitHub issues should be created with, for example: \"github-actions\". Assign per usage.")]
	public string IssueLabel
	{
		get => _issueLabel;
		set => ParseAndAssign(value, str => _issueLabel = str);
	}

	static void ParseAndAssign(string? value, Action<string> assign)
	{
		if (value is { Length: > 0 })
		{
			assign(value.Split("/")[^1]);
		}
	}
}