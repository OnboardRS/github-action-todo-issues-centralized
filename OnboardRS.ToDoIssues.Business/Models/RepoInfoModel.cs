namespace OnboardRS.ToDoIssues.Business.Models;

public class RepoInfoModel : BaseReflectedToStringObject
{
	public string Name { get; set; }
	public string Owner { get; set; }
	public string Branch { get; set; }

	public RepoInfoModel(string name, string owner, string branch)
	{
		Name = name;
		Owner = owner;
		Branch = branch;
	}

	public void ValidateInputs()
	{
		AssertNotNullOrNotEmpty(nameof(Name),Name);
		AssertNotNullOrNotEmpty(nameof(Owner), Owner);
		AssertNotNullOrNotEmpty(nameof(Branch), Branch);
	}
}
