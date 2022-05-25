using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Octokit;
using OnboardRS.ToDoIssues.Business.Models;
using OnboardRS.ToDoIssues.Business.Models.Config;
using OnboardRS.ToDoIssues.Business.Models.GeneratedCode.GitHub;
using OnboardRS.ToDoIssues.Business.Models.Tasks;
using RestSharp;

namespace OnboardRS.ToDoIssues.Business.Utilities;

public class GitHubAgent
{

	private readonly ILogger<GitHubAgent> _logger;
	private readonly ToDoIssuesConfig _config;
	private long? _issueRepoId;

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

	public GitHubAgent(ILogger<GitHubAgent> logger, ToDoIssuesConfig config)
	{
		_logger = logger;
		_config = config;
	}

	//public async Task<string> CreateTask(ToDoIssueModel model)
	//{

	//	//	const graphql = require('@octokit/graphql').defaults({
	//	//headers:
	//	//		{
	//	//authorization: `token ${
	//	//				process.env.GITHUB_TOKEN ||
	//	//invariant(false, 'Required GITHUB_TOKEN variable.')}`,
	//	//    },
	//	//  })
	//	//  const result = await graphql(
	//	//    `
	//	//      mutation CreateIssue($input: CreateIssueInput!) {
	//	//		createIssue(input: $input) {
	//	//			issue {
	//	//				number


	//	//		  }
	//	//		}
	//	//	}
	//	//    `,
	//	//    {
	//	//input:
	//	//		{
	//	//repositoryId: RepositoryInfo.issueSourceRepoContext.repositoryNodeId,
	//	//        title: information.title,
	//	//        body: information.body,
	//	//      },
	//	//    },
	//	//  )
	//	//  log.debug('Create issue result:', result)
	//	//  return result.createIssue.issue.number
	//	//	? `#${result.createIssue.issue.number}`
	//	//    : invariant(
	//	//		false,
	//	//		'Failed to get issue number out of createIssue API call.',

	//	//	  )
	//}

	public async Task CompleteTask(string issueReference)
	{
		//	const Octokit = (await import('@octokit/rest')).default
		//  const octokit = new Octokit({
		//	auth: `token ${
		//		process.env.GITHUB_TOKEN ||
		//invariant(false, 'Required GITHUB_TOKEN variable.')}`,
		//  })
		//  const result = await octokit.issues.update({
		//    owner: RepositoryInfo.issueSourceRepoContext.repositoryOwner,
		//	repo: RepositoryInfo.issueSourceRepoContext.repositoryName,
		//	issue_number: +taskReference.substr(1),
		//	state: 'closed',
		//  })
		//  log.debug('Issue close result:', result.data)
	}

	public async Task updateTask(string issueReference, ToDoIssueModel toDoIssueModel)
	{
		//	const Octokit = (await import('@octokit/rest')).default
		//  const octokit = new Octokit({
		//	auth: `token ${
		//		process.env.GITHUB_TOKEN ||
		//invariant(false, 'Required GITHUB_TOKEN variable.')}`,
		//  })
		//  const result = await octokit.issues.update({
		//    owner: RepositoryInfo.issueSourceRepoContext.repositoryOwner,
		//	repo: RepositoryInfo.issueSourceRepoContext.repositoryName,
		//	issue_number: +taskReference.substr(1),
		//	title: information.title,
		//	body: information.body,
		//  })
		//  log.debug('Issue update result:', result.data)
	}


	//async void Main()
	//{
	//	var dropbox = Environment.GetEnvironmentVariable("Dropbox");
	//	var envConfig = Path.Combine(dropbox, ".releasehub", "env.json");
	//	var configContent = File.ReadAllText(envConfig);
	//	var processedRepoFile = Path.Combine(dropbox, ".releasehub", "repos.txt");

	//	if (!File.Exists(processedRepoFile))
	//	{
	//		File.Create(processedRepoFile);
	//	}

	//	Thread.Sleep(1000 * 2);
	//	var processedRepos = File.ReadAllLines(processedRepoFile);


	//	AppConfig = JsonConvert.DeserializeObject<EnvConfig>(configContent);




	//	var client = new RestClient("https://api.github.com/");
	//	client.AddDefaultHeader("Authorization", $" token {AppConfig.GitHub.AccessToken}");
	//	client.AddDefaultHeader("Accept", "application/vnd.github.v3+json");


	//	var labelsToCreate = new List<GHLabel>();
	//	labelsToCreate.Add(new GHLabel { name = "actual: 0", description = "actual estimate", color = "fff" });
	//	labelsToCreate.Add(new GHLabel { name = "actual: 1", description = "actual estimate", color = "fff" });
	//	labelsToCreate.Add(new GHLabel { name = "actual: 2", description = "actual estimate", color = "fff" });
	//	labelsToCreate.Add(new GHLabel { name = "actual: 3", description = "actual estimate", color = "fff" });
	//	labelsToCreate.Add(new GHLabel { name = "actual: 5", description = "actual estimate", color = "fff" });
	//	labelsToCreate.Add(new GHLabel { name = "actual: 8", description = "actual estimate", color = "fff" });
	//	labelsToCreate.Add(new GHLabel { name = "actual: 13", description = "actual estimate", color = "fff" });
	//	labelsToCreate.Add(new GHLabel { name = "actual: 21", description = "actual estimate", color = "fff" });
	//	labelsToCreate.Add(new GHLabel { name = "bug", description = "defect", color = "B60205" });
	//	labelsToCreate.Add(new GHLabel { name = "documentation", description = "", color = "0052CC" });
	//	labelsToCreate.Add(new GHLabel { name = "dupe", description = "", color = "FFA500" });
	//	labelsToCreate.Add(new GHLabel { name = "Epic", description = "", color = "4660F9" });
	//	labelsToCreate.Add(new GHLabel { name = "feature", description = "new feature", color = "0E8A16" });
	//	labelsToCreate.Add(new GHLabel { name = "legacy-admin", description = "Related to PHP Admin", color = "51906B" });
	//	labelsToCreate.Add(new GHLabel { name = "marketing-materials", description = "", color = "0D0D02" });
	//	labelsToCreate.Add(new GHLabel { name = "ops", description = "", color = "BFD4F2" });
	//	labelsToCreate.Add(new GHLabel { name = "owner-portal", description = "insight product line", color = "7DDEBF" });
	//	labelsToCreate.Add(new GHLabel { name = "property-score", description = "iq product line", color = "EC92FE" });
	//	labelsToCreate.Add(new GHLabel { name = "release", description = "release ticket", color = "FBCA04" });
	//	labelsToCreate.Add(new GHLabel { name = "rent-roll", description = "", color = "B52300" });
	//	labelsToCreate.Add(new GHLabel { name = "research", description = "discovery", color = "5319E7" });
	//	labelsToCreate.Add(new GHLabel { name = "resident-portal", description = "", color = "A0FA6F" });
	//	labelsToCreate.Add(new GHLabel { name = "sales-force", description = "", color = "1D76DB" });
	//	labelsToCreate.Add(new GHLabel { name = "wont-fix", description = "rejected, not being worked on.", color = "ffffff" });

	//	var repoLabels = labelsToCreate.Select(s => new { s.name, s.description, s.color }).ToList();

	//	var ghOrg = "onboardrs";
	//	var request = new RestRequest($"/orgs/{ghOrg}/repos?per_page=100&page=2");
	//	var repos = JsonConvert.DeserializeObject<List<Repo>>(client.ExecuteGetAsync(request).Result.Content);


	//	foreach (var r in repos)
	//	{
	//		if (processedRepos.Contains($"{ghOrg}/{r.name}"))
	//		{
	//			Console.WriteLine($"{ghOrg}/{r.name} already processed.");
	//			continue;
	//		}

	//		Console.WriteLine($"Getting labels for {ghOrg}/{r.name}");

	//		request = new RestRequest($"/repos/{ghOrg}/{r.name}/labels");
	//		var labels = JsonConvert.DeserializeObject<List<GHLabel>>(client.ExecuteAsync(request).Result.Content);



	//		// Delete Repo Labels
	//		// DELETE: repos/{owner}/{repo}/labels/{name}
	//		/*
	//		foreach(var l in labels)
	//		{
	//		//	Console.WriteLine($"Deleting... /repos/{ghOrg}/{r.name}/labels/{l.name}");
	//			request = new RestRequest($"repos/{ghOrg}/{r.name}/labels/{l.name}", Method.Delete);
	//			var result = client.ExecuteAsync(request).Result;
	//			if(result.StatusCode != System.Net.HttpStatusCode.NoContent)
	//			{
	//				throw new Exception($"{result.StatusCode} - {result.StatusDescription}");
	//			}
	//		}
	//		*/

	//		// Create Repo Labels
	//		// POST: /repos/{owner}/{repo}/labels
	//		foreach (var l in repoLabels)
	//		{
	//			//	Console.WriteLine($"Creating... /repos/{ghOrg}/{r.name}/labels/{l.name}");
	//			request = new RestRequest($"/repos/{ghOrg}/{r.name}/labels", Method.Post);
	//			//var labelData = 
	//			request.AddBody(l, "application/vnd.github.v3+json");
	//			var result = client.ExecuteAsync(request).Result;
	//			/*
	//			if (result.StatusCode != System.Net.HttpStatusCode.Created)
	//			{
	//				throw new Exception($"{result.StatusCode} - {result.StatusDescription}");
	//			}
	//			*/
	//		}

	//		File.AppendAllText(processedRepoFile, $"{ghOrg}/{r.name}{IToDoFileContents.UNIX_LINE_ENDING}");

	//		// Trying to avoid rate limit
	//		Thread.Sleep(1000 * 2);
	//	}
	//}

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

	public async Task<Issue> CreateGitHubIssueAsync(ToDoIssueModel toDoIssueModel)
	{
		var newIssue = new NewIssue(toDoIssueModel.Title);
		newIssue.Body = toDoIssueModel.Body;
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
			string noTokenMessage = $"No value for {nameof(_config.GitHubAccessToken)}. Check configuration input parameters and / or environment variables.";
			_logger.LogError(noTokenMessage);
			throw new ApplicationException(noTokenMessage);
		}

		var tokenAuth = new Credentials(_config.GitHubAccessToken);
		var githubClient = new GitHubClient(new ProductHeaderValue(_config.IssueRepoInfoModel.Name));
		githubClient.Credentials = tokenAuth;
		return githubClient;
	}

}