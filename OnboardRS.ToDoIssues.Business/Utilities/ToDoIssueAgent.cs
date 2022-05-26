using System.Diagnostics;
using MongoDB.Bson;

namespace OnboardRS.ToDoIssues.Business.Utilities;

public class ToDoIssueAgent
{
	private readonly GitHubAgent _gitHubAgent;
	private readonly ILogger<ToDoIssueAgent> _logger;
	private readonly MongoAgent _mongoAgent;
	private readonly ToDoIssuesConfig _toDoIssuesConfig;

	public ToDoIssueAgent(ToDoIssuesConfig toDoIssuesConfig, ILogger<ToDoIssueAgent> logger, MongoAgent mongoAgent, GitHubAgent gitHubAgent)
	{
		_logger = logger;
		_toDoIssuesConfig = toDoIssuesConfig;
		_gitHubAgent = gitHubAgent;
		_mongoAgent = mongoAgent;
	}

	public async Task ProcessRepoToDoActionsAsync()
	{
		List<IToDoFile> toDoFiles;
		if (Debugger.IsAttached)
		{
			toDoFiles = await GetToDoFilesFromDirectory(@"C:\GitHub\github-action-todo-issues-centralized");
		}
		else
		{
			// Find the files via grep
			toDoFiles = await GetToDoFilesFromRepositoryAsync();
		}



		// Parse out the ToDos.
		var toDos = GetToDosFromToDoFiles(toDoFiles);

		// Create stub references for items with no references.
		await ProcessToDosWithoutReferenceAsync(toDos);

		// Sanity Check: Every item must have a reference by now.
		foreach (var todo in toDos)
		{
			if (null == todo.IssueReference)
			{
				var errorMessage = $"{ToDoConstants.TASK_MARKER} {todo.Title} at {todo.ToDoFile.FileName} must have a reference by now!";
				_logger.LogError(errorMessage);
				throw new ApplicationException(errorMessage);
			}
		}

		// Create any new tasks
		var newAssociations = await EnsureAllToDosAreAssociatedAsync(toDos);
		await newAssociations.SaveNewAssociationChanges(_logger);

		// Reconcile all tasks
		//await ReconcileTasksAsync(toDos);
	}

	/// <summary>
	///     Creates code and mongo db stubs for items without a refernce.
	/// </summary>
	/// <param name="toDos"></param>
	public async Task ProcessToDosWithoutReferenceAsync(List<IToDo> toDos)
	{
		var todDsWithoutReference = toDos.Where(todo => string.IsNullOrWhiteSpace(todo.IssueReference)).ToList();
		_logger.LogInformation($"{ToDoConstants.TASK_MARKER}s without references: {todDsWithoutReference.Count}");

		if (todDsWithoutReference.Any())
		{
			foreach (var todo in todDsWithoutReference)
			{
				//Create a stub id to be replaced later, starts with $
				todo.IssueReference = $"${BsonUtils.ToHexString(new ObjectId().ToByteArray())}";
			}

			var todoFilesWithoutReference = todDsWithoutReference.Select(x => x.ToDoFile).Distinct().ToList();
			await todoFilesWithoutReference.SaveChanges($"Collecting {todDsWithoutReference.Count} new {ToDoConstants.TASK_MARKER} comments.", _logger);
		}

		_logger.LogInformation($"Created stub reference for {todDsWithoutReference.Count} items.");
	}

	public List<IToDo> GetToDosFromToDoFiles(List<IToDoFile> toDoFiles)
	{
		var toDos = new List<IToDo>();
		foreach (var toDoFile in toDoFiles)
		{
			//TODO: Implement ignoring paths
			if (_toDoIssuesConfig.ExcludeList.Any(x => x == toDoFile.FileName))
			{
				continue;
			}

			var todos = toDoFile.ParseToDos();
			_logger.LogInformation($"{toDoFile.FileName}: {todos.Count} found.");
			toDos.AddRange(todos);
		}

		_logger.LogInformation($"Total {ToDoConstants.TASK_MARKER}s found: {toDos.Count}");
		return toDos;
	}

	public async Task<List<IToDoFile>> GetToDoFilesFromRepositoryAsync()
	{
		_logger.LogInformation($"Search for files with {ToDoConstants.TASK_MARKER} tags...");
		var result = await $"git grep -Il {ToDoConstants.TASK_MARKER}".RunBashCommandAsync(_logger);

		//split on newlines and remove duplicates
		var paths = result.Split("\n").ToHashSet().ToList();

		_logger.LogInformation($"Parsing {ToDoConstants.TASK_MARKER} tags...");
		var files = new List<IToDoFile>();

		foreach (var path in paths)
		{
			var toDoFile = new ToDoFile(path);
			files.Add(toDoFile);
		}

		return files;
	}

	public async Task<List<IToDoFile>> GetToDoFilesFromDirectory(string directoryPath)
	{
		var paths = Directory.GetFiles(directoryPath, "*.cs", SearchOption.AllDirectories);
		
		_logger.LogInformation($"Parsing {ToDoConstants.TASK_MARKER} tags...");
		var files = new List<IToDoFile>();

		foreach (var path in paths)
		{
			var toDoFile = new ToDoFile(path);
			if (toDoFile.Contents.Lines.Any(x => x.Contains(ToDoConstants.TASK_MARKER)))
			{
				files.Add(toDoFile);
			}
		}

		return files;
	}

	public async Task<List<IToDo>> EnsureAllToDosAreAssociatedAsync(List<IToDo> toDos)
	{
		var newAssociations = new List<IToDo>();
		foreach (var toDo in toDos)
		{
			var newAssociation = await EnsureToDoIsAssociatedAsync(toDo);
			if (null != newAssociation)
			{
				newAssociations.Add(newAssociation);
			}
		}

		return newAssociations;
	}

	/// <summary>
	///     If the item is a stub, create or find the DB entry, and create an issue. If the item is already associated, do
	///     nothing.
	/// </summary>
	/// <param name="toDo"></param>
	/// <returns>The <see cref="IToDoFile" /> if the item was unassociated, or null if nothing changed.  </returns>
	/// <exception cref="ArgumentException"></exception>
	/// <exception cref="ApplicationException"></exception>
	public async Task<IToDo?> EnsureToDoIsAssociatedAsync(IToDo toDo)
	{
		if (null == toDo.IssueReference)
		{
			var errorMessage = $"Unexpected unidentified {ToDoConstants.TASK_MARKER} marker.";
			_logger.LogError(errorMessage);
			throw new ArgumentException(errorMessage);
		}

		var unassociated = toDo.IssueReference.StartsWith(ToDoConstants.STUB_REFERENCE_MARKER);
		if (unassociated)
		{
			//TODO: Isolate error when creating tasks
			// Failure to create a task should not prevent the action from progressing forward.
			// We can simply skip processing this comment for now.
			// Since this script is designed to be idempotent, it can be retried later.

			var todoUniqueKey = toDo.IssueReference.Substring(1);
			_logger.LogDebug($"Found unresolved {ToDoConstants.TASK_MARKER} issue reference {todoUniqueKey}, resolving task...");
			var lockedEntity = await _mongoAgent.AcquireTaskCreationLock(toDo);
			if (null == lockedEntity)
			{
				_logger.LogWarning($"Couldn't aquire lock. Todo with issue reference {toDo.IssueReference} skipped.");
			}
			else
			{
				if (null == toDo.Title || null == toDo.Body)
				{
					var message = $"Title and Body must be set before reaching here. Issue Reference: {toDo.IssueReference}";
					_logger.LogError(message);
					throw new ApplicationException(message);
				}

				var issueModel = toDo.GenerateToDoIssueModelFromTodo(_toDoIssuesConfig);
				var newIssue = await _gitHubAgent.CreateGitHubIssueAsync(issueModel);
				toDo.IssueReference = newIssue.Number.ToString(); //This updates the file as well
				lockedEntity.IssueReference = toDo.IssueReference;
				lockedEntity.Completed = false;
				await _mongoAgent.UpsertByIdAsync(lockedEntity);
				_logger.LogDebug($"Resolved {ToDoConstants.TASK_MARKER} {todoUniqueKey} => task {toDo.IssueReference}");
			}

			return toDo;
		}

		return null;
	}
}