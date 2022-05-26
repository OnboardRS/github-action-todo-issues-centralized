using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace OnboardRS.ToDoIssues.Business.Utilities.Extensions;

public static class ModelActionExtension
{
	public static void ValidateIssueReference(this IToDo toDo, ILogger logger)
	{
		if (null == toDo.IssueReference)
		{
			var errorMessage = $"Unexpected unidentified {ToDoConstants.TASK_MARKER} marker.";
			logger.LogError(errorMessage);
			throw new ArgumentException(errorMessage);
		}
	}

	/// <summary>
	///     Saves the file back into the file system.
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

	public static async Task SaveNewAssociationChanges(this List<IToDo> toDos, ILogger logger)
	{
		if (!toDos.Any())
		{
			return;
		}

		var updatedReferences = toDos.Select(x => x.IssueReference).OrderBy(x => x).ToList();
		var toDoFiles = toDos.Select(x => x.ToDoFile).ToList();
		var toDoFilesSet = new HashSet<IToDoFile>();

		foreach (var toDoFile in toDoFiles)
		{
			toDoFilesSet.Add(toDoFile);
		}

		var toDoFilesToUpdate = toDoFilesSet.ToList();
		var commitMessage = $"Updated {ToDoConstants.TASK_MARKER} references: " + string.Join(", ", updatedReferences);
		await toDoFilesToUpdate.ToList().SaveChanges(commitMessage, logger);
	}

	/// <summary>
	///     Saves all ToDo files and updates git.
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

			if (Debugger.IsAttached)
			{
				logger.LogInformation("Pretend I did the Git and what not.");
			}
			else
			{
				var gitAddCommand = $"git add {string.Join(" ", changedFiles)}";
				await gitAddCommand.RunBashCommandAsync(logger);
				var gitCommitCommand = $"git commit -m \"{commitMessage}\"";
				await gitCommitCommand.RunBashCommandAsync(logger);
				var gitPushCommand = "git push \"https://x-access-token:$GITHUB_TOKEN@github.com/$GITHUB_REPOSITORY.git\" HEAD:\"$GITHUB_REF\"";
				await gitPushCommand.RunBashCommandAsync(logger);
			}
		}
		else
		{
			logger.LogInformation("No TODO file changes found.");
		}
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
		var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(randomString));
		foreach (var theByte in crypto)
		{
			hash.Append(theByte.ToString("x2"));
		}

		return hash.ToString();
	}

	public static ToDoIssueModel GenerateToDoIssueModelFromTodo(this IToDo todo, ToDoIssuesConfig config)
	{
		var title = todo.Title ?? string.Empty;
		var file = todo.ToDoFile.FileName;

		//TODO: Also link to end line in addition to just the starting line.
		// This requires changing `IFile` interface and `File` class to also keep track of where the {ToDoConstants.TASK_MARKER} comment ends.
		var line = todo.StartLine;
		var owner = config.CodeRepoInfoModel.Owner;
		var repo = config.CodeRepoInfoModel.Name;
		var defaultBranch = config.CodeRepoInfoModel.Branch;

		var url = $"https://github.com/{owner}/{repo}/blob/{defaultBranch}/{file}#L{line}";
		var link = $"[{file}:{line}]({url})";
		var builder = new StringBuilder();
		builder.AppendLine(todo.Body);
		builder.AppendLine();
		builder.AppendLine("---");
		builder.AppendLine($"This issue has been automatically created by [github-action-todo-issues-centralized] (https://github.com/OnboardRS/github-action-todo-issues-centralized) based on a {ToDoConstants.TASK_MARKER} comment found in {link}. ");
		builder.AppendLine("_");
		var fullBody = builder.ToString();
		var hash = fullBody.HashString();
		var model = new ToDoIssueModel(hash, title, fullBody);
		return model;
	}
}