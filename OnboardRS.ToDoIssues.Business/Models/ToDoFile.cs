namespace OnboardRS.ToDoIssues.Business.Models;

public class ToDoFile : BaseReflectedToStringObject, IToDoFile
{
	public ToDoFile(string fileName)
	{
		FileName = fileName;

		Contents = new ToDoFileContents(File.ReadAllText(fileName));
	}

	public string FileName { get; set; }
	public IToDoFileContents Contents { get; set; }
}