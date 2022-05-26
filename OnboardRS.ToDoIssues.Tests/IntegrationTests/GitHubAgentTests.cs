namespace OnboardRS.ToDoIssues.Tests;

[TestClass]
public class GitHubAgentIntegrationTests : BaseIntegrationTests
{

	private ToDoModel _toDoModel;

	[TestInitialize]
	public void SetUp()
	{
		BaseSetUp();
		_toDoModel = GetDefaultTestToDo();
	}


	[TestMethod]
	public async Task CreateIssueTest()
	{
		var test = await GitHubAgent.CreateGitHubIssueAsync(new ToDoIssueModel(string.Empty, TO_DO_DEFAULT_TITLE, TO_DO_DEFAULT_BODY));
		Assert.IsNotNull(test);
		Assert.IsFalse(string.IsNullOrWhiteSpace(test.Number.ToString()));
	}


	[TestMethod]
	public async Task GetGitHubLabelsAsyncTest()
	{
		var result = await GitHubAgent.GetGitHubLabelsAsync();
		Assert.IsTrue(result.Count > 0);
	}
}