namespace OnboardRS.ToDoIssues.Business.Interfaces;

/// <summary>
/// A representation of a file being processed
/// with mutable contents.
/// </summary>
public interface IToDoFile
{
	string FileName { get; set; }
	IToDoFileContents Contents { get; set; }
}