namespace OnboardRS.ToDoIssues.Business.Models;

public class ToDoIssueModel
{
	public ToDoIssueModel(string issueNumber, string title, string body)
	{
		IssueNumber = issueNumber;
		Title = title;
		Body = body;
	}

	public string Title { get; set; }
	public string Body { get; set; }
	public string IssueNumber { get; set; }
}