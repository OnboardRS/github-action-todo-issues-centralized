namespace OnboardRS.ToDoIssues.Tests;

[TestClass]
public class ToDoIssueAgentTests : BaseIntegrationTests
{

	private ToDoModel _toDoModel;

	[TestInitialize]
	public void SetUp()
	{
		BaseSetUp();
		_toDoModel = GetDefaultTestToDo();
	}
}