namespace OnboardRS.ToDoIssues.Business.Models.Config;

public class GitHubConfig : BaseReflectedToStringObject
{
	public string Org { get; set; }
	public string AccessToken { get; set; }
	public string ReleaseRepo { get; set; }

	public GitHubConfig(string org, string accessToken, string releaseRepo)
	{
		Org = org;
		AccessToken = accessToken;
		ReleaseRepo = releaseRepo;
	}
}