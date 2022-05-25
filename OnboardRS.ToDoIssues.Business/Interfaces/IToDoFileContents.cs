﻿namespace OnboardRS.ToDoIssues.Business.Interfaces;

public interface IToDoFileContents
{
	public const string WINDOWS_LINE_ENDING = "\r\n";
	public const string UNIX_LINE_ENDING = "\n";

	bool Changed { get; set; }

	/// <summary>
	/// File contents as array of lines.
	/// The newline character has been stripped.
	/// May be mutated to change the contents of the file.
	/// </summary>
	List<string> Lines { get; set; }

	/// <summary>
	/// Change a line
	/// </summary>
	/// <param name="lineIndex"></param>
	/// <param name="newLineContents"></param>
	/// <returns></returns>
	void ChangeLine(int lineIndex, string newLineContents);
}