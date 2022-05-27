using System.Text.RegularExpressions;

namespace OnboardRS.ToDoIssues.Business.Utilities.Extensions;

public static class ToDoParserExtensions
{
	public const string TODO_REGULAR_EXPRESSION = $@"^(\W+\s){ToDoConstants.TO_DO_MARKER}?: \[([^\]\s]+)\])?:(.*)";
	private static readonly Regex Regex = new(TODO_REGULAR_EXPRESSION, RegexOptions.Multiline);

	public static Match? ParseLineForToDo(this string input)
	{
		var match = Regex.Matches(input).FirstOrDefault();
		return match;
	}

	public static List<IToDo> ParseToDos(this IToDoFile file)
	{
		var toDos = new List<IToDo>();

		ToDoModel? currentTodo = null;
		for (var lineIndex = 0; lineIndex < file.Contents.Lines.Count; lineIndex++)
		{
			var line = file.Contents.Lines[lineIndex];
			var match = line.ParseLineForToDo();
			if (null != match)
			{
				var todo = new ToDoModel(file, lineIndex, match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value);
				currentTodo = todo;
				toDos.Add(todo);
			}
			else if (null != currentTodo)
			{
				var beforePrefix = line.Substring(0, Math.Min(currentTodo.Prefix.Length, line.Length));
				var afterPrefix = line.Substring(Math.Min(currentTodo.Prefix.Length, line.Length));
				var hasSamePrefix = beforePrefix.TrimEnd() == currentTodo.Prefix.TrimEnd();
				var shouldAppendText = !string.IsNullOrWhiteSpace(afterPrefix) || !string.IsNullOrWhiteSpace(beforePrefix);
				if (hasSamePrefix && shouldAppendText)
				{
					currentTodo.HandleLine(afterPrefix);
				}
				else
				{
					currentTodo = null;
				}
			}
		}

		return toDos;
	}
}