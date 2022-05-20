using System.Security;
using System.Text.RegularExpressions;

namespace OnboardRS.ToDoIssues.Business.Utilities.Extensions;

public static class ModelActionExtension
{

	/// <summary>
	/// Saves the file back into the file system.
	/// </summary>
	/// <returns></returns>

	public static async Task SaveToDoFileAsync(this IToDoFile toDoFile)
	{
		if (toDoFile.Contents.Changed)
		{
			await File.WriteAllTextAsync(toDoFile.FileName, toDoFile.Contents.ToString());
			toDoFile.Contents.Changed = false;

		}
	}

	/// <summary>
	/// Saves all ToDo files and updates git.
	/// </summary>
	/// <param name="toDoFiles"></param>
	/// <param name="commitMessage"></param>
	/// <param name="logger"></param>
	/// <returns></returns>
	public static async Task SaveChanges(this List<IToDoFile> toDoFiles, string commitMessage, ILogger logger)
	{
		var changedFiles = toDoFiles.Where(x => x.Contents.Changed).ToList();
		if (changedFiles.Any())
		{
			logger.LogInformation($"TODO files changed: {changedFiles.Count}");

			foreach (var changedFile in changedFiles)
			{
				await changedFile.SaveToDoFileAsync();
			}

			var gitAddCommand = $"git add {string.Join(' ', changedFiles)}";
			await gitAddCommand.RunBashCommandAsync(logger);
			var gitCommitCommand = $"git commit -m '{commitMessage}'";
			await gitCommitCommand.RunBashCommandAsync(logger);
			var gitPushCommand = $"git push \"https://x-access-token:$GITHUB_TOKEN@github.com/$GITHUB_REPOSITORY.git\" HEAD:\"$GITHUB_REF\"";
			await gitPushCommand.RunBashCommandAsync(logger);
		}
		else
		{
			logger.LogInformation("No TODO file changes found.");
		}
	}
}

public static class ToDoParserExtensions
{
	public const string TODO_REGULAR_EXPRESSION = @"^(\W+\s)TODO(?: \[([^\]\s]+)\])?:(.*)";
	private static readonly Regex Regex = new Regex(TODO_REGULAR_EXPRESSION, RegexOptions.Multiline);
	public static Match? ParseLineForToDo(this string input)
	{
		var match = Regex.Matches(input).FirstOrDefault();
		return match;
	}

	public static async Task<List<IToDo>> ParseToDosAsync(this IToDoFile file)
	{
		var todos = new List<IToDo>();

		ToDoModel? currentTodo = null;
		for (int lineIndex = 0; lineIndex < file.Contents.Lines.Count; lineIndex++)
		{
			var line = file.Contents.Lines[lineIndex];
			var match = line.ParseLineForToDo();
			if (null != match)
			{
				var todo = new ToDoModel(file, lineIndex, match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value);
				currentTodo = todo;
				todos.Add(todo);
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
		return todos;
	}
}