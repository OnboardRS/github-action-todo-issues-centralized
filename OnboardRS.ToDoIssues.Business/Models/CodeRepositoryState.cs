namespace OnboardRS.ToDoIssues.Business.Models;

public class CodeRepositoryState : BaseReflectedToStringObject
{
	public List<IToDoFile> ToDoFiles { get; set; } = new List<IToDoFile>();
}