using Octokit;
using OnboardRS.ToDoIssues.Business.Interfaces;

namespace OnboardRS.ToDoIssues.Tests;

[TestClass]
public class GitHubIntegrationTests : BaseIntegrationTests
{

	[TestInitialize]
	public void SetUp()
	{
		BaseSetUp();
	}

	[TestMethod]
	public async Task GetGitHubLabelsAsyncTest()
	{
		var result = await GitHubAgent.GetGitHubLabelsAsync();
		Assert.IsTrue(result.Count > 0);
	}
}