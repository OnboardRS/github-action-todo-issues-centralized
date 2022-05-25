using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using OnboardRS.ToDoIssues.Business.Interfaces;
using OnboardRS.ToDoIssues.Business.Models.Mongo;
using OnboardRS.ToDoIssues.Business.Models.Tasks;

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
			logger.LogInformation($"{ToDoConstants.TASK_MARKER} files changed: {changedFiles.Count}");

			foreach (var changedFile in changedFiles)
			{
				await changedFile.SaveToDoFileAsync();
			}

			var gitAddCommand = $"git add {string.Join(" ", changedFiles)}";
			await gitAddCommand.RunBashCommandAsync(logger);
			var gitCommitCommand = $"git commit -m \"{ commitMessage}\"";
			await gitCommitCommand.RunBashCommandAsync(logger);
			var gitPushCommand = $"git push \"https://x-access-token:$GITHUB_TOKEN@github.com/$GITHUB_REPOSITORY.git\" HEAD:\"$GITHUB_REF\"";
			await gitPushCommand.RunBashCommandAsync(logger);
		}
		else
		{
			logger.LogInformation("No TODO file changes found.");
		}
	}

	//public static async Task<string> MarkAsCompletedAsync(this ToDoTask toDoTask)
	//{
	//	return null;
	//}

	//public static async Task UpdateStateAsync(this ToDoTask toDoTask, IToDoTaskState newState)
	//{

	//}

	public static ToDoEntity ToTodDoEntity(this IToDo todo, string todoUniqueKey, string repositoryId)
	{
		var objectId = new ObjectId(todoUniqueKey);
		var model = new ToDoEntity(objectId, repositoryId)
		{
			Completed = null,
			CreatedAt = DateTime.UtcNow,
			Hash = todo.GetToDoHash(),
			OwnerProcessId = null,
			OwnerProcessTimestamp = null,
			IssueReference = todo.IssueReference
		};
		return model;
	}

	public static string GetToDoHash(this IToDo todo)
	{
		var hashSource = $"{todo.Title}|{todo.Body}";
		var hash = HashString(hashSource);
		return hash;
	}

	public static string HashString(this string randomString)
	{
		var crypt = SHA256.Create();
		var hash = new StringBuilder();
		byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(randomString));
		foreach (byte theByte in crypto)
		{
			hash.Append(theByte.ToString("x2"));
		}
		return hash.ToString();
	}

	public static ToDoIssueModel GenerateToDoIssueModelFromTodo(this IToDo todo, ToDoIssuesConfig config)
	{

		var title = todo.Title ?? string.Empty;
		var file = todo.ToDoFile.FileName;

		// TODO: Also link to end line in addition to just the starting line.
		// This requires changing `IFile` interface and `File` class to also keep track of where the {ToDoConstants.TASK_MARKER} comment ends.
		var line = todo.StartLine;
		var owner = config.CodeRepoInfoModel.Owner;
		var repo = config.CodeRepoInfoModel.Name;
		var defaultBranch = config.CodeRepoInfoModel.Branch;

		var url = $"https://github.com/{owner}/{repo}/blob/{defaultBranch}/{file}#L{line}";
		var link = $"[{ file}:{ line}]({ url})";
		var builder = new StringBuilder();
		builder.AppendLine(todo.Body);
		builder.AppendLine();
		builder.AppendLine("---");
		builder.AppendLine($"This issue has been automatically created by [github-action-todo-issues-centralized] (https://github.com/OnboardRS/github-action-todo-issues-centralizeds) based on a {ToDoConstants.TASK_MARKER} comment found in ${link}. ");
		builder.AppendLine("_");
		var fullBody = builder.ToString();
		var hash = fullBody.HashString();
		var model = new ToDoIssueModel(hash, title, fullBody);
		return model;
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