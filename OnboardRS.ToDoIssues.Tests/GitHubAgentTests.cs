namespace OnboardRS.ToDoIssues.Tests;

[TestClass]
public class GitHubAgentTests : BaseIntegrationTests
{

	private ToDoModel _toDoModel;
	private readonly string _title = "Test Item Title";
	private readonly string _body = "Test Item Body";
	private readonly string _file = "TestFile.cs";
	private readonly string _fileContents = @"Behold the file contents!
This
Is
A
Bunch
Of 
Random
Contents
Sad
Panda";
	private readonly string _prefix = "First!";
	private readonly string _reference = "ye olde reference";
	private readonly string _suffix = "le suffix";
	private readonly int _startLine = 7;

	[TestInitialize]
	public void SetUp()
	{
		BaseSetUp();
		_toDoModel = new ToDoModel(new ToDoMockFile(_file, _fileContents), _startLine, _prefix, _reference, _suffix);
		_toDoModel.Title = _title;
		_toDoModel.Body = _body;
	}


	[TestMethod]
	public async Task CreateIssueTest()
	{
		var test = await GitHubAgent.CreateGitHubIssueAsync(new ToDoIssueModel(string.Empty, _title, _body));
	}
}