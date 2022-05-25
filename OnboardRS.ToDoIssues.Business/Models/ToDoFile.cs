namespace OnboardRS.ToDoIssues.Business.Models;

public class ToDoFile : IToDoFile
{

	public string FileName { get; set; }
	public IToDoFileContents Contents { get; set; }

	public ToDoFile(string fileName)
	{
		FileName = fileName;

		Contents = new ToDoFileContents(File.ReadAllText(fileName));
	}
}