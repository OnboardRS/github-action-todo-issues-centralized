using OnboardRS.ToDoIssues.Business.Utilities;

namespace OnboardRS.ToDoIssues.Business.Models;

public class ToDoIssuesConfig : BaseReflectedToStringObject
{

	public ToDoIssuesConfig(string gitHubAccessToken, string mongoDbUrl, string issueLabelsCsv, string excludeListCsv, RepoInfoModel codeRepoInfoModel, RepoInfoModel issueRepoInfoModel)
	{
		GitHubAccessToken = gitHubAccessToken;
		MongoDbUrl = mongoDbUrl.Trim(); //In case secret has extra space.
		IssueLabels = SplitCsv(issueLabelsCsv);
		ExcludeList = string.IsNullOrWhiteSpace(excludeListCsv) ? new List<string> { "README.md" } : SplitCsv(excludeListCsv);
		CodeRepoInfoModel = codeRepoInfoModel;
		IssueRepoInfoModel = issueRepoInfoModel;
	}

	public string GitHubAccessToken { get; set; }
	public string MongoDbName { get; set; } = MongoAgent.TODO_DATABASE_NAME;
	public string MongoDbCollectionName { get; set; } = MongoAgent.TODO_COLLECTION_NAME;
	public string MongoDbUrl { get; set; }
	public List<string> IssueLabels { get; set; }
	public RepoInfoModel CodeRepoInfoModel { get; set; }
	public RepoInfoModel IssueRepoInfoModel { get; set; }
	public List<string> ExcludeList { get; set; }

	public void ValidateInputs()
	{
		AssertNotNullOrNotEmpty(nameof(GitHubAccessToken), GitHubAccessToken);
		AssertNotNullOrNotEmpty(nameof(MongoDbUrl), MongoDbUrl);
		if (IssueLabels.Count == 0 || IssueLabels.Any(string.IsNullOrWhiteSpace))
		{
			throw new ApplicationException($"{nameof(IssueLabels)} must have at least one non-whitespace value.");
		}
		CodeRepoInfoModel.ValidateInputs();
		IssueRepoInfoModel.ValidateInputs();
	}

	private List<string> SplitCsv(string inputCsv)
	{
		var result = inputCsv.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
		return result;
	}
}