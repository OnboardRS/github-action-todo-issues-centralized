using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Octokit;

namespace OnboardRS.ToDoIssues.Business.Models;

public class MongoTask
{
	public string TaskReference { get; set; }
	public IToDoTaskState TaskState { get; set; }

	public async Task<string> MarkAsCompletedAsync()
	{
		return null;
	}

	public async Task UpdateStateAsync(IToDoTaskState newState)
	{

	}
}



public interface IToDoFileContents
{
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



public interface IToDoTaskState
{
	string Hash { get; set; }
}


public interface IToDo
{
	IToDoFile? ToDoFile { get; set; }
	string? Reference { get; set; }
	string? Title { get; set; }
	string? Body { get; set; }
	int GetStartLine();

}

public class ToDoModel : BaseReflectedToStringObject, IToDo
{
	public IToDoFile? ToDoFile { get; set; }
	public string Prefix { get; set; }
	public int Line { get; set; }
	public string Suffix { get; set; }
	public string Body { get; set; }
	public string? Title { get; set; }

	private string? _reference;
	public string? Reference
	{
		get { return _reference; }
		set
		{
			_reference = value;
			if (null != ToDoFile)
			{
				ToDoFile.Contents.ChangeLine(Line, $"{Prefix}TODO{(null == value ? $"[{value}]" : string.Empty)}:${Suffix}");
			}
		}
	}

	public ToDoModel(IToDoFile file, int line, string prefix, string? reference, string suffix)
	{
		ToDoFile = file;
		Line = line;
		Prefix = prefix;
		Reference = reference;
		Suffix = suffix;
		Title = suffix.Trim();
		Body = string.Empty;
	}

	public int GetStartLine()
	{
		var result = Line + 1;
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

public class CodeRepositoryState : BaseReflectedToStringObject
{
	public List<IToDoFile> ToDoFiles { get; set; } = new List<IToDoFile>();
}


/// <summary>
/// A representation of a file being processed
/// with mutable contents.
/// </summary>
public interface IToDoFile
{
	string FileName { get; set; }
	IToDoFileContents Contents { get; set; }
}

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
		Lines = contents.Split(Environment.NewLine).ToList();
	}

	public void ChangeLine(int lineIndex, string newLineContents)
	{
		Lines[lineIndex] = newLineContents;



		Changed = true;
	}

	public override string ToString()
	{
		return string.Join(Environment.NewLine, Lines);
	}
}

