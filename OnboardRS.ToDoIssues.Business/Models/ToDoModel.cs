namespace OnboardRS.ToDoIssues.Business.Models;

public class ToDoModel : BaseReflectedToStringObject, IToDo
{
	public IToDoFile ToDoFile { get; set; }
	public string Prefix { get; set; }
	public int StartLine { get; set; }
	public string Suffix { get; set; }
	public string Body { get; set; }
	public string? Title { get; set; }

	private string? _reference;
	public string? IssueReference
	{
		get { return _reference; }
		set
		{
			_reference = value;
			ToDoFile.Contents.ChangeLine(StartLine, $"{Prefix}{ToDoConstants.TO_DO_MARKER}{(null != value ? $" [#{value.TrimStart('#')}]" : string.Empty)}:{Suffix}");
		}
	}

	public ToDoModel(IToDoFile file, int startLine, string prefix, string? reference, string suffix)
	{
		ToDoFile = file;
		StartLine = startLine;
		Prefix = prefix;
		IssueReference = reference;
		Suffix = suffix;
		Title = suffix.Trim();
		Body = string.Empty;
	}

	public int GetStartLine()
	{
		var result = StartLine + 1;
		return result;
	}

	public void HandleLine(string line)
	{
		if (string.IsNullOrWhiteSpace(Title))
		{
			Title = line;
		}
		else if (string.IsNullOrWhiteSpace(Body))
		{
			if (!string.IsNullOrWhiteSpace(line))
			{
				Body = line;
			}
		}
		else
		{
			Body += "\n" + line;
		}
	}
}