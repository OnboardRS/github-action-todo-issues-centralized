using OnboardRS.ToDoIssues.Business;
using OnboardRS.ToDoIssues.Business.Models;

namespace OnboardRS.ToDoIssues.GitHubAction;

public class ActionInputs : BaseReflectedToStringObject
{
	private string _codeRepoBranchName = null!;
	private string _codeRepoOwner = null!;
	private string _codeRepoRepositoryName = null!;
	private string _exludedFileNamesCsv = null!;
	private string _gitHubActionToken = null!;
	private string _issueLabel = null!;
	private string _issueRepoBranchName = null!;
	private string _issueRepoOwner = null!;
	private string _issueRepoRepositoryName = null!;
	private string _mongoDbUrl = null!;


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
			   HelpText = "The issue repo branch name, for example: \"refs/heads/master\". Assign from centralized issue repo.")]
	public string IssueRepoBranch
	{
		get => _issueRepoBranchName;
		set => ParseAndAssign(value, str => _issueRepoBranchName = str);
	}

	[Option("issue-labels-csv",
			   Required = true,
			   HelpText = $"The issue label(s) {ToDoConstants.TO_DO_MARKER} GitHub issues should be created with, for example: \"github-actions\". Assign per usage.")]
	public string IssueLabelCsv
	{
		get => _issueLabel;
		set => ParseAndAssign(value, str => _issueLabel = str);
	}

	[Option("github-action-token",
			   Required = true,
			   HelpText = "The GitHub token with permission to read and change the code. Assign per usage.")]
	public string GitHubActionToken
	{
		get => _gitHubActionToken;
		set => ParseAndAssign(value, str => _gitHubActionToken = str);
	}

	[Option("mongo-db-url",
			   Required = true,
			   HelpText = $"The Mongo DB Url to use to store {ToDoConstants.TO_DO_MARKER} hashes. Assign per usage.")]
	public string MongoDbUrl
	{
		get => _mongoDbUrl;
		set => ParseAndAssign(value, str => _mongoDbUrl = str);
	}

	[Option("excluded-file-names-csv",
			   Required = true,
			   HelpText = $"Comma separated values for case insensitive file names you don't want to search for {ToDoConstants.TO_DO_MARKER}s. Assign per usage.")]
	public string ExcludedFileNamesCsv
	{
		get => _exludedFileNamesCsv;
		set => ParseAndAssign(value, str => _exludedFileNamesCsv = str);
	}

	public ToDoIssuesConfig ToToDoIssuesConfig()
	{
		var codeRepo = new RepoInfoModel(CodeRepoName, CodeRepoOwner, CodeRepoBranch);
		var issueRepo = new RepoInfoModel(IssueRepoName, IssueRepoOwner, IssueRepoBranch);

		var config = new ToDoIssuesConfig(GitHubActionToken, MongoDbUrl, IssueLabelCsv, ExcludedFileNamesCsv, codeRepo, issueRepo);
		return config;
	}

	private static void ParseAndAssign(string? value, Action<string> assign)
	{
		if (value is { Length: > 0 })
		{
			assign(value.Split("/")[^1]);
		}
	}

	public void LogInputs(ILogger<ActionInputs> actionInputsLogger)
	{
		actionInputsLogger.LogInformation($"{nameof(ActionInputs)}.{nameof(LogInputs)}\n{ToString()}");
	}
}