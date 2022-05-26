using OnboardRS.ToDoIssues.Business;
using OnboardRS.ToDoIssues.Business.Models;

namespace OnboardRS.ToDoIssues.GitHubAction;

public class ActionInputs : BaseReflectedToStringObject
{
	[Option("code-repo-owner",
			   Required = true,
			   HelpText = "The code repo owner, for example: \"OnboardRS\". Assign from `github.repository_owner`.")]
	public string CodeRepoOwner { get; set; } = string.Empty;

	[Option("code-repo-name",
			   Required = true,
			   HelpText = "The code repo repository name, for example: \"github-action-todo-issues-centralized\". Assign from `github.repository`.")]
	public string CodeRepoName { get; set; } = string.Empty;

	[Option("code-repo-branch",
			   Required = true,
			   HelpText = "The code repo branch name, for example: \"refs/heads/develop\". Assign from `github.ref`.")]
	public string CodeRepoBranch { get; set; } = string.Empty;

	[Option("issue-repo-owner",
			   Required = true,
			   HelpText = "The issue repo owner, for example: \"OnboardRS\". Assign from centralized issue repo.")]
	public string IssueRepoOwner { get; set; } = string.Empty;

	[Option("issue-repo-name",
			   Required = true,
			   HelpText = "The issue repo repository name, for example: \"zenhub-dev\". Assign from centralized issue repo.")]
	public string IssueRepoName { get; set; } = string.Empty;

	[Option("issue-repo-branch",
			   Required = true,
			   HelpText = "The issue repo branch name, for example: \"refs/heads/master\". Assign from centralized issue repo.")]
	public string IssueRepoBranch { get; set; } = string.Empty;

	[Option("issue-labels-csv",
			   Required = true,
			   HelpText = $"The issue label(s) {ToDoConstants.TO_DO_MARKER} GitHub issues should be created with, for example: \"github-actions\". Assign per usage.")]
	public string IssueLabelCsv { get; set; } = string.Empty;

	[Option("github-action-token",
			   Required = true,
			   HelpText = "The GitHub token with permission to read and change the code. Assign per usage.")]
	public string GitHubActionToken { get; set; } = string.Empty;

	[Option("mongo-db-url",
			   Required = true,
			   HelpText = $"The Mongo DB Url to use to store {ToDoConstants.TO_DO_MARKER} hashes. Assign per usage.")]
	public string MongoDbUrl { get; set; } = string.Empty;

	[Option("excluded-file-names-csv",
			   Required = true,
			   HelpText = $"Comma separated values for case insensitive file names you don't want to search for {ToDoConstants.TO_DO_MARKER}s. Assign per usage.")]
	public string ExcludedFileNamesCsv { get; set; } = string.Empty;

	public ToDoIssuesConfig ToToDoIssuesConfig()
	{
		var codeRepo = new RepoInfoModel(CodeRepoName, CodeRepoOwner, CodeRepoBranch);
		var issueRepo = new RepoInfoModel(IssueRepoName, IssueRepoOwner, IssueRepoBranch);
		var config = new ToDoIssuesConfig(GitHubActionToken, MongoDbUrl, IssueLabelCsv, ExcludedFileNamesCsv, codeRepo, issueRepo);
		return config;
	}

	public void LogInputs(ILogger<ActionInputs> actionInputsLogger)
	{
		actionInputsLogger.LogInformation($"{nameof(ActionInputs)}.{nameof(LogInputs)}");
		actionInputsLogger.LogInformation($"{nameof(CodeRepoOwner)}: [{CodeRepoOwner}]");
		actionInputsLogger.LogInformation($"{nameof(CodeRepoName)}: [{CodeRepoName}]");
		actionInputsLogger.LogInformation($"{nameof(CodeRepoBranch)}: [{CodeRepoBranch}]");
		actionInputsLogger.LogInformation($"{nameof(IssueRepoOwner)}: [{IssueRepoOwner}]");
		actionInputsLogger.LogInformation($"{nameof(IssueRepoName)}: [{IssueRepoName}]");
		actionInputsLogger.LogInformation($"{nameof(IssueRepoBranch)}: [{IssueRepoBranch}]");
		actionInputsLogger.LogInformation($"{nameof(IssueLabelCsv)}: [{IssueLabelCsv}]");
		actionInputsLogger.LogInformation($"{nameof(GitHubActionToken)}: [{GitHubActionToken.Length} chars]");
		actionInputsLogger.LogInformation($"{nameof(ExcludedFileNamesCsv)}: [{ExcludedFileNamesCsv}]");
		actionInputsLogger.LogInformation($"{nameof(MongoDbUrl)}: [{MongoDbUrl.Length} chars]");
	}
}