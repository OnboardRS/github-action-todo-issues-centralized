using OnboardRS.ToDoIssues.Business.Models.GeneratedCode.ZenHub;

namespace OnboardRS.ToDoIssues.Business.Models.Config;

public class ZenHubConfig : BaseReflectedToStringObject
{
	public string AccessToken { get; set; }
	public List<ZenWorkspace> Workspaces { get; set; } = new List<ZenWorkspace>();

	public ZenHubConfig(string accessToken)
	{
		AccessToken = accessToken;
	}
}