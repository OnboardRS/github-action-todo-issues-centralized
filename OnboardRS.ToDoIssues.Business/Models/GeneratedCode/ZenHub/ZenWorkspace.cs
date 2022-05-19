using ZenHub.Models;

namespace OnboardRS.ToDoIssues.Business.Models.GeneratedCode.ZenHub;

public class ZenWorkspace
{
	public string Name { get; set; }
	public string BaseRepo { get; set; }
	public List<ReleaseReport> Releases { get; set; }
	public ZenWorkspace()
	{
		Releases = new List<ReleaseReport>();
	}
}