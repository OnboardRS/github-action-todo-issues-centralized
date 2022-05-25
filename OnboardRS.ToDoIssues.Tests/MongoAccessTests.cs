﻿using System;
using Octokit;
using OnboardRS.ToDoIssues.Business.Models.Tasks;

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

public class BaseIntegrationTests
{
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
		ToDoIssuesConfig = new ToDoIssuesConfig(_githubToken, _mongoConnectionString, "IntegrationTest,test-integration", CodeRepo, IssueRepo);
		MongoAgent = new MongoAgent(ToDoIssuesConfig, new ConsoleLogger<MongoAgent>());
		ToDoIssueAgent = new ToDoIssueAgent(ToDoIssuesConfig, new ConsoleLogger<ToDoIssueAgent>());
		GitHubAgent = new GitHubAgent(new ConsoleLogger<GitHubAgent>(), ToDoIssuesConfig);
	}
}


[TestClass]
public class ToDoIssueAgentTests : BaseIntegrationTests
{

	private ToDoModel _toDoModel;
	private readonly string _title = "Test Item Title";
	private readonly string _body = "Test Item Body";
	private readonly string _file = "TestFile.cs";
	private readonly string _fileContents = "Behold the file contents!";
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
}

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
