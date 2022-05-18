using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnboardRS.ToDoIssues.Business.Models;

public class RepoInfoModel : BaseReflectedToStringObject
{
	public string Name { get; set; }
	public string Owner { get; set; }
	public string Branch { get; set; }
	public string NodeId { get; set; }

	public RepoInfoModel(string name, string owner, string branch, string? nodeId = null)
	{
		Name = name;
		Owner = owner;
		Branch = branch;
		NodeId = nodeId ?? string.Empty;
	}
}
