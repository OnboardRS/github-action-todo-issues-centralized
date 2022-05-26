using System;

namespace OnboardRS.ToDoIssues.Tests;

public class BaseIntegrationTests
{


	public const string TO_DO_DEFAULT_TITLE = "Test Item Title";
	public const string TO_DO_DEFAULT_BODY = "Test Item Body";
	public const string TO_DO_DEFAULT_FILE = "TestFile.cs";
	public const string TO_DO_DEFAULT_FILE_CONTENTS = TO_DO_DEFAULT_PREFIX + "TO" + "DO [" + TO_DO_DEFAULT_REFERENCE + "]:" + TO_DO_DEFAULT_SUFFIX;
	public const string TO_DO_DEFAULT_PREFIX = "        // ";
	public const string TO_DO_DEFAULT_REFERENCE = "-1";
	public const string TO_DO_DEFAULT_SUFFIX = " " + TO_DO_DEFAULT_TITLE;
	public const int TO_DO_DEFAULT_STARTLINE = 0;


	private static string _mongoConnectionString = Environment.GetEnvironmentVariable("TO_DO_MONGO_URL");
	private static string _githubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN");

	protected GitHubAgent GitHubAgent { get; set; }
	protected MongoAgent MongoAgent { get; set; }
	protected ToDoIssuesConfig ToDoIssuesConfig { get; set; }
	protected ToDoIssueAgent ToDoIssueAgent { get; set; }
	protected RepoInfoModel CodeRepo { get; set; }
	protected RepoInfoModel IssueRepo { get; set; }

	public void BaseSetUp()
	{
		var thisRepo = "github-action-todo-issues-centralized";
		var thisOwner = "OnboardRS";
		var thisBranch = "integration-tests";
		CodeRepo = new RepoInfoModel(thisRepo, thisOwner, thisBranch);
		IssueRepo = new RepoInfoModel(thisRepo, thisOwner, thisBranch);
		ToDoIssuesConfig = new ToDoIssuesConfig(_githubToken, _mongoConnectionString, "IntegrationTest,test-integration", CodeRepo, IssueRepo)
		                   {
							   MongoDbName = "ToDo",
							   MongoDbCollectionName = MongoAgent.TODO_COLLECTION_NAME+"Test"
		                   };
		MongoAgent = new MongoAgent(ToDoIssuesConfig, new ConsoleLogger<MongoAgent>());
		GitHubAgent = new GitHubAgent(new ConsoleLogger<GitHubAgent>(), ToDoIssuesConfig);

		ToDoIssueAgent = new ToDoIssueAgent(ToDoIssuesConfig, new ConsoleLogger<ToDoIssueAgent>(), MongoAgent, GitHubAgent);
	}



	public ToDoModel GetDefaultTestToDo()
	{
		var _toDoModel = new ToDoModel(new ToDoMockFile(TO_DO_DEFAULT_FILE, TO_DO_DEFAULT_FILE_CONTENTS), TO_DO_DEFAULT_STARTLINE, TO_DO_DEFAULT_PREFIX, TO_DO_DEFAULT_REFERENCE, TO_DO_DEFAULT_SUFFIX);
		_toDoModel.Title = TO_DO_DEFAULT_TITLE;
		_toDoModel.Body = TO_DO_DEFAULT_BODY;
		return _toDoModel;
	}
}