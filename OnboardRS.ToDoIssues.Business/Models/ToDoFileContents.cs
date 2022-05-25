namespace OnboardRS.ToDoIssues.Business.Models;

public class ToDoFileContents : IToDoFileContents
{
	public bool Changed { get; set; }

	/// <summary>
	/// File contents as array of lines.
	/// The newline character has been stripped.
	/// May be mutated to change the contents of the file.
	/// </summary>
	public List<string> Lines { get; set; }

	public ToDoFileContents(string contents)
	{
		Lines = contents.Replace(IToDoFileContents.WINDOWS_LINE_ENDING, IToDoFileContents.UNIX_LINE_ENDING).Split(IToDoFileContents.UNIX_LINE_ENDING).ToList();
	}

	public void ChangeLine(int lineIndex, string newLineContents)
	{
		Lines[lineIndex] = newLineContents;
		Changed = true;
	}

	public override string ToString()
	{
		return string.Join(IToDoFileContents.UNIX_LINE_ENDING, Lines);
	}
}