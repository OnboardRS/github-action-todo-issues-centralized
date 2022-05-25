namespace OnboardRS.ToDoIssues.Business.Interfaces;

public interface IToDo
{
	IToDoFile ToDoFile { get; set; }
	string? IssueReference { get; set; }
	string? Title { get; set; }
	string? Body { get; set; }
	int StartLine { get; set; }
}