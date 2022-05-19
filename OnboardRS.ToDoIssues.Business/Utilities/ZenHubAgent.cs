using System;
using System.Collections.Generic;
using System.Linq;
using Octokit;
using OnboardRS.ToDoIssues.Business.Models.Config;
using ZenHub;
using ZenHub.Models;

namespace OnboardRS.ToDoIssues.Business.Utilities;

public class ZenHubAgent
{
	private readonly ZenHubConfig _config;

	public ZenHubAgent(ZenHubConfig config)
	{
		_config = config;
	}

	public Workspace[] GetZenHubWorkspace(Repository repo)
	{
		var client = new ZenHubClient(_config.AccessToken);
		return client.GetRepositoryClient(repo).GetWorkspacesAsync().Result;

	}

	public List<ReleaseReport> GetZenHubReleases(Repository repo)
	{
		var client = new ZenHubClient(_config.AccessToken);
		return client.GetRepositoryClient(repo).GetReleaseReportsAsync().Result.Value.ToList();
	}

	public ReleaseReport GetZenHubReleaseReport(string releaseId)
	{
		var client = new ZenHubClient(_config.AccessToken);
		return client.GetReleaseClient(releaseId).GetReportAsync().Result;
	}

	public IssueDetails[] GetZenHubReleaseIssues(string releaseId)
	{
		var client = new ZenHubClient(_config.AccessToken);
		return client.GetReleaseClient(releaseId).GetIssuesAsync().Result;
	}

	public EpicList GetZenHubRepoEpics(long repoId)
	{
		var client = new ZenHubClient(_config.AccessToken);
		return client.GetRepositoryClient(repoId).GetEpicsAsync().Result.Value;
	}

	public EpicDetails GetZenHubEpicDetails(long repoId, int issueNumber)
	{
		var client = new ZenHubClient(_config.AccessToken);
		return client.GetEpicClient(repoId, issueNumber).GetDetailsAsync().Result;
	}

	public IssueDetails GetZenHubIssueDetails(long repoId, int issueNumber)
	{
		var client = new ZenHubClient(_config.AccessToken);
		return client.GetIssueClient(repoId, issueNumber).GetDetailsAsync().Result;
	}
}