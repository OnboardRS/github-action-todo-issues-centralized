using System;

namespace OnboardRS.ToDoIssues.Tests;

[TestClass]
public class MongoAccessIntegrationTests : BaseIntegrationTests
{

	[TestMethod]
	public async Task SelectTest()
	{
		var items = await MongoAgent.FindAllUncompletedTasksAsync("R_kgDOHJq_hg");
		var items2 = await MongoAgent.FindAllTasksAsync();
	}

	[TestMethod]	
	public async Task MongoUnassociatedCreateTest()
	{
		var items = await MongoAgent.FindAllUncompletedTasksAsync("R_kgDOHJq_hg");
		var items2 = await MongoAgent.FindAllTasksAsync();
	}
}

[TestClass]
public class BaseIntegrationTests
{
	private static string _mongoConnectionString = Environment.GetEnvironmentVariable("TO_DO_MONGO_URL");
	private static string _githubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN");

	protected MongoAgent MongoAgent { get; set; }
	protected ToDoIssuesConfig ToDoIssuesConfig { get; set; }
	protected ToDoIssueAgent ToDoIssueAgent { get; set; }
	protected RepoInfoModel CodeRepo { get; set; }
	protected RepoInfoModel IssueRepo { get; set; }

	[TestInitialize]
	public void SetUp()
	{
		var thisRepo = "github-action-todo-issues-centralized";
		var thisOwner = "OnboardRS";
		var thisBranch = "integration-tests";
		CodeRepo = new RepoInfoModel(thisRepo, thisOwner, thisBranch);
		IssueRepo = new RepoInfoModel(thisRepo, thisOwner, thisBranch);
		ToDoIssuesConfig = new ToDoIssuesConfig(_githubToken, _mongoConnectionString, "IntegrationTest,test-integration", CodeRepo, IssueRepo);
		 MongoAgent = new MongoAgent(ToDoIssuesConfig, new ConsoleLogger<MongoAgent>());
		ToDoIssueAgent = new ToDoIssueAgent(ToDoIssuesConfig, new ConsoleLogger<ToDoIssueAgent>());
	}

}


[TestClass]
public class ToDoIssueAgentTests : BaseIntegrationTests
{
	[TestMethod]
	public async Task SelectTest()
	{
		
	}
}
