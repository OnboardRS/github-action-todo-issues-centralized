using System;

namespace OnboardRS.ToDoIssues.Tests;

[TestClass]
public class MongoAccessIntegrationTests
{

	private static string connectionString = Environment.GetEnvironmentVariable("TO_DO_MONGO_URL");
	private ILogger<MongoAgent> _logger = new TypedConsoleLogger<MongoAgent>();

	[TestMethod]
	public async Task SelectTest()
	{
		var repo = new RepoInfoModel(string.Empty, string.Empty, string.Empty, null);
		var config = new ToDoIssuesConfig(string.Empty, connectionString, "Test", repo, repo);
		var agent = new MongoAgent(config, _logger);
		var items = await agent.FindAllUncompletedTasksAsync("R_kgDOHJq_hg");
		var items2 = await agent.FindAllTasksAsync();
	}
}
