using OnboardRS.ToDoIssues.Business.Utilities;

namespace OnboardRS.ToDoIssues.Business.Models;

public class ToDoIssuesConfig : BaseReflectedToStringObject
{
	public ToDoIssuesConfig(string gitHubAccessToken, string mongoDbUrl, string issueLabel, RepoInfoModel codeRepoInfoModel, RepoInfoModel issueRepoInfoModel)
	{
		GitHubAccessToken = gitHubAccessToken;
		MongoDbUrl = mongoDbUrl;
		IssueLabel = issueLabel;
		CodeRepoInfoModel = codeRepoInfoModel;
		IssueRepoInfoModel = issueRepoInfoModel;
	}

	public string GitHubAccessToken { get; set; }
	public string MongoDbName { get; set; } = MongoAgent.TODO_DATABASE_NAME;
	public string MongoDbCollectionName { get; set; } = MongoAgent.TODO_COLLECTION_NAME;
	public string MongoDbUrl { get; set; }
	public string IssueLabel { get; set; }
	public RepoInfoModel CodeRepoInfoModel { get; set; }
	public RepoInfoModel IssueRepoInfoModel { get; set; }

	public List<string> ExcludeList { get; set; } = new()
	                                                {
		                                                "README.md"
	                                                };

	public void ValidateInputs()
	{
		AssertNotNullOrNotEmpty(nameof(GitHubAccessToken), GitHubAccessToken);
		AssertNotNullOrNotEmpty(nameof(MongoDbUrl), MongoDbUrl);
		AssertNotNullOrNotEmpty(nameof(IssueLabel), IssueLabel);
		CodeRepoInfoModel.ValidateInputs();
		IssueRepoInfoModel.ValidateInputs();
	}
}