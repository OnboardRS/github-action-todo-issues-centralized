using Newtonsoft.Json;
using Octokit;
using RestSharp;

namespace OnboardRS.ToDoIssues.Business.Utilities;

public class GitHubAgent
{
	private readonly ToDoIssuesConfig _config;

	private readonly ILogger<GitHubAgent> _logger;
	private long? _issueRepoId;

	public GitHubAgent(ILogger<GitHubAgent> logger, ToDoIssuesConfig config)
	{
		_logger = logger;
		_config = config;
	}

	private long IssueRepoId
	{
		get
		{
			if (null == _issueRepoId)
			{
				var repo = GetGitHubIssueRepoAsync().Result;
				_issueRepoId = repo.Id;
			}

			return _issueRepoId.Value;
		}
	}

	public async Task<List<Label>> GetGitHubLabelsAsync()
	{
		var client = GetGitHubRestClient();
		var request = new RestRequest($"/repos/{_config.IssueRepoInfoModel.Owner}/{_config.IssueRepoInfoModel.Name}/labels");
		var result = await client.ExecuteAsync(request);
		if (!result.IsSuccessful || string.IsNullOrWhiteSpace(result.Content))
		{
			var message = $"{nameof(GetGitHubLabelsAsync)} request failed with code {result.StatusCode} and result [{result.Content}]\nError: [{result.ErrorMessage}]";
			_logger.LogError(message);
			throw new ApplicationException(message);
		}

		var labels = JsonConvert.DeserializeObject<List<Label>>(result.Content);
		if (null == labels)
		{
			var message = $"{nameof(GetGitHubLabelsAsync)} deserialize failed with content: [{result.Content}]";
			_logger.LogError(message);
			throw new ApplicationException(message);
		}

		return labels;
	}

	public async Task<Repository> GetGitHubIssueRepoAsync()
	{
		var client = GetGitHubClient();
		return await client.Repository.Get(_config.IssueRepoInfoModel.Owner, _config.IssueRepoInfoModel.Name);
	}

	public async Task<Issue> GetGitHubIssueAsync(int issueId)
	{
		var client = GetGitHubClient();
		return await client.Issue.Get(IssueRepoId, issueId);
	}

	public async Task<Issue> UpdateGitHubIssueAsync(ToDoIssueModel toDoIssueModel)
	{
		if (int.TryParse(toDoIssueModel.IssueNumber, out var issueNumber))
		{
			var issue = await GetGitHubIssueAsync(issueNumber);
			var issueUpdate = issue.ToUpdate();
			issueUpdate.Title = toDoIssueModel.Title;
			issueUpdate.Body = toDoIssueModel.Body;
			foreach (var issueLabel in _config.IssueLabels)
			{
				issueUpdate.AddLabel(issueLabel);
			}
			var client = GetGitHubClient();
			await client.Issue.Update(IssueRepoId, issueNumber, issueUpdate);
			return issue;
		}

		var message = $"Invalid {nameof(toDoIssueModel.IssueNumber)} for use in {nameof(UpdateGitHubIssueAsync)}. Could not parse to int. Got {toDoIssueModel.IssueNumber}";
		_logger.LogError(message);
		throw new ApplicationException(message);
	}

	public async Task<Issue> CreateGitHubIssueAsync(ToDoIssueModel toDoIssueModel)
	{
		// TODO [$000000000000000000000000]: Test new todo label.
		var newIssue = new NewIssue(toDoIssueModel.Title)
		{
			Body = toDoIssueModel.Body
		};
		foreach (var issueLabel in _config.IssueLabels)
		{
			newIssue.Labels.Add(issueLabel);
		}

		return await CreateGitHubIssueAsync(newIssue);
	}

	public async Task<Issue> CreateGitHubIssueAsync(NewIssue newIssue)
	{
		var client = GetGitHubClient();
		return await client.Issue.Create(IssueRepoId, newIssue);
	}

	private GitHubClient GetGitHubClient()
	{
		if (string.IsNullOrWhiteSpace(_config.GitHubAccessToken))
		{
			var noTokenMessage = $"No value for {nameof(_config.GitHubAccessToken)}. Check configuration input parameters and / or environment variables.";
			_logger.LogError(noTokenMessage);
			throw new ApplicationException(noTokenMessage);
		}

		var tokenAuth = new Credentials(_config.GitHubAccessToken);
		var githubClient = new GitHubClient(new ProductHeaderValue(_config.IssueRepoInfoModel.Name));
		githubClient.Credentials = tokenAuth;
		return githubClient;
	}

	private RestClient GetGitHubRestClient()
	{
		var client = new RestClient("https://api.github.com/");
		client.AddDefaultHeader("Authorization", $" token {_config.GitHubAccessToken}");
		client.AddDefaultHeader("Accept", "application/vnd.github.v3+json");
		return client;
	}
}