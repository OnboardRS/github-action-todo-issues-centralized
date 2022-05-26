namespace OnboardRS.ToDoIssues.Tests;

[TestClass]
public class ToDoIssueAgentIntegrationTests : BaseIntegrationTests
{

	private ToDoModel _toDoModel;

	[TestInitialize]
	public void SetUp()
	{
		BaseSetUp();
		_toDoModel = GetDefaultTestToDo();
	}

	[TestMethod]
	public async Task FullProcessTest()
	{
		await ToDoIssueAgent.ProcessRepoToDoActionsAsync();
	}
}