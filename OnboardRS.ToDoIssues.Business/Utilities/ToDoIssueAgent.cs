using System.Diagnostics;
using System.Runtime.CompilerServices;
using MongoDB.Bson;
using OnboardRS.ToDoIssues.Business.Interfaces;
using OnboardRS.ToDoIssues.Business.Models.Mongo;
using OnboardRS.ToDoIssues.Business.Models.Tasks;

namespace OnboardRS.ToDoIssues.Business.Utilities;

public class ToDoIssueAgent
{
	private readonly ILogger<ToDoIssueAgent> _logger;
	private readonly ToDoIssuesConfig _toDoIssuesConfig;
	private readonly MongoAgent _mongoAgent;
	private readonly GitHubAgent _gitHubAgent;

	private string? _processId;

	public string ProcessId
	{
		get
		{
			if (null == _processId)
			{
				_processId = Process.GetCurrentProcess().Id.ToString();
			}

			return _processId;
		}
	}

	public ToDoIssueAgent(ToDoIssuesConfig toDoIssuesConfig, ILogger<ToDoIssueAgent> logger, MongoAgent mongoAgent, GitHubAgent gitHubAgent)
	{
		_logger = logger;
		_toDoIssuesConfig = toDoIssuesConfig;
		_gitHubAgent = gitHubAgent;
		_mongoAgent = mongoAgent;
	}


	public async Task ProcessRepoToDoActionsAsync()
	{
		// Find the files via grep
		var toDoFiles = await GetToDoFilesFromRepositoryAsync();

		// Parse out the ToDos.
		var toDos = await GetToDosFromToDoFilesAsync(toDoFiles);

		// Create stub references for items with no references.
		var codeUpdated = await ProcessToDosWithoutReferenceAsync(toDos);

		if (codeUpdated)
		{ _logger.LogInformation("Stub created and code changed. Stopping process.");
			return;
		}

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

		//NOTE: Next run will assign the issues for the stubbed references


		// Update all the tasks according to the item state.
		var associated = await EnsureAllTodosAreAssociatedAsync(toDos);




		//await associated.SaveChanges("Updated {ToDoConstants.TASK_MARKER} references: " + associated.join(", "), _logger);

		// Reconcile all tasks
		//await ReconcileTasksAsync(todoComments);
	}

	/// <summary>
	/// Creates code and mongo db stubs for items without a refernce.
	/// </summary>
	/// <param name="toDos"></param>
	/// <returns></returns>
	public async Task<bool> ProcessToDosWithoutReferenceAsync(List<IToDo> toDos)
	{
		var todDsWithoutReference = toDos.Where(todo => null == todo.IssueReference).ToList();
		_logger.LogInformation($"{ToDoConstants.TASK_MARKER}s without references: {todDsWithoutReference.Count}");

		if (todDsWithoutReference.Any())
		{
			foreach (var todo in todDsWithoutReference)
			{
				//Create a stub id to be replaced later, starts with $
				todo.IssueReference = $"${ BsonUtils.ToHexString(new ObjectId().ToByteArray())}";
			}

			List<IToDoFile> todoFilesWithoutReference = todDsWithoutReference.Select(x => x.ToDoFile).Distinct().ToList();
			await todoFilesWithoutReference.SaveChanges($"Collecting {todDsWithoutReference.Count} new {ToDoConstants.TASK_MARKER} comments.", _logger);
		}
		_logger.LogInformation($"Created stub reference for {todDsWithoutReference.Count} items.");
		return todDsWithoutReference.Any();
	}
	public async Task<List<IToDo>> GetToDosFromToDoFilesAsync(List<IToDoFile> toDoFiles)
	{
		var toDos = new List<IToDo>();
		foreach (var toDoFile in toDoFiles)
		{
			// TODO: Implement ignoring paths
			if (_toDoIssuesConfig.ExcludeList.Any(x => x == toDoFile.FileName))
			{
				continue;
			}

			List<IToDo> todos = await toDoFile.ParseToDosAsync();
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

	public async Task UpdateGitFromCodeRepositoryState(CodeRepositoryState codeRepositoryState)
	{
		//	async saveChanges(commitMessage)
		//	{
		//		const changedFiles = files.filter(file => file.contents.changed)

		//		_logger.LogInformation("Files changed: {}", changedFiles.length)

		//		if (changedFiles.length === 0)
		//		{
		//			return

		//		}
		//		for (const file of changedFiles) {
		//			file.save()

		//		}
		//		execFileSync("git", ["add", ...changedFiles.map(file => file.fileName)])

		//		execFileSync("git", ["commit", "-m", commitMessage], {
		//			stdio: "inherit",
		//		})
		//		if (!process.env.GITHUB_TOKEN)
		//		{
		//			throw "Maybe you forgot to enable the GITHUB_TOKEN secret?"

		//		}
		//		execSync(
		//		         "git push "https://x-access-token:$GITHUB_TOKEN@github.com/$GITHUB_REPOSITORY.git" HEAD:"$GITHUB_REF"",

		//		{ stdio: "inherit" },
		//		)
	}

	public async Task<List<string>> EnsureAllTodosAreAssociatedAsync(List<IToDo> todos)
	{
		var references = new List<string>();
		foreach (var todo in todos)
		{
			if (null == todo.IssueReference)
			{
				var errorMessage = $"Unexpected unidentified {ToDoConstants.TASK_MARKER} marker.";
				_logger.LogError(errorMessage);
				throw new ArgumentException(errorMessage);
			}

			var unassociated = todo.IssueReference.StartsWith("$");


			if (unassociated)
			{
				// TODO: Isolate error when creating tasks
				// Failure to create a task should not prevent the action from progressing forward.
				// We can simply skip processing this comment for now.
				// Since this script is designed to be idempotent, it can be retried later.
				var todoUniqueKey = todo.IssueReference.Substring(1);
				_logger.LogDebug($"Found unresolved {ToDoConstants.TASK_MARKER} issue reference {todoUniqueKey}, resolving task...");
				//var taskReference = await ResolveTaskAsync(todoUniqueKey, todo);
				//_logger.LogDebug($"Resolved {ToDoConstants.TASK_MARKER} {todoUniqueKey} => task {taskReference}");
				//todo.Reference = taskReference;
				//references.Add(taskReference);
			}
		}

		return references;
	}

	//export async function reconcileTasks(todos: ITodo[]) {
	//	const uncompletedTasks = await DataStore.findAllUncompletedTasks(
	//	  CodeRepository.repoContext.repositoryNodeId,

	//	)
	//  log.info(
	//	"Number of registered uncompleted tasks: {}",
	//	uncompletedTasks.length,
	//  )

	//  for (const todo of todos) {
	//		const reference =
	//		  todo.reference || invariant(false, "Unexpected unidentified {ToDoConstants.TASK_MARKER} marker")

	//	invariant(
	//	  !reference.startsWith("$"),
	//	  "Expected all {ToDoConstants.TASK_MARKER} comments to be associated by now.",

	//	)

	//	const task = uncompletedTasks.find(t => t.taskReference === reference)

	//	if (!task)
	//		{
	//			log.warn(
	//			  "Cannot find a matching task for {ToDoConstants.TASK_MARKER} comment with reference "{}"",
	//			  reference,

	//			)

	//	  continue

	//	}
	//		// TODO [#4]: Isolate error when updating tasks
	//		// Failure to update a task should not prevent the action from progressing forward.
	//		// We can simply skip processing this task for now.
	//		// Since this script is designed to be idempotent, it can be retried later.
	//		const {
	//			title,
	//      body,
	//      state,
	//    } = TaskInformationGenerator.generateTaskInformationFromTodo(todo)

	//	if (task.state.hash !== state.hash)
	//		{
	//			log.info(
	//			  "Hash for "{}" changed: "{}" => "{}" -- must update task.",
	//			  reference,
	//			  task.state.hash,
	//			  state.hash,

	//			)

	//	  await TaskManagementSystem.updateTask(reference, { title, body })
	//      await task.updateState(state)

	//	}
	//		else
	//		{
	//			// TODO [#7]: Test to remove later B
	//			log.info(
	//			  "Hash for "{}" remains unchanged: "{}".",
	//			  reference,
	//			  task.state.hash,

	//			)

	//	}
	//	}

	//	for (const task of uncompletedTasks) {
	//		if (todos.find(todo => todo.reference === task.taskReference))
	//		{
	//			continue

	//	}


	//		if (getShouldCloseIssueOnDelete())
	//		{
	//			log.info("{ToDoConstants.TASK_MARKER} for task "{}" is gone -- completing task!", task.taskReference,)
	//	      // TODO [#5]: Isolate error when completing tasks
	//		  // Failure to complete a task should not prevent the action from progressing forward.
	//		  // We can simply skip processing this task for now.
	//		  // Since this script is designed to be idempotent, it can be retried later.
	//			await TaskManagementSystem.completeTask(task.taskReference)

	//	}
	//		else
	//		{
	//			// TODO [#8]: Test to remove later A
	//			log.info("{ToDoConstants.TASK_MARKER} for task "{}" is gone TODO_ACTIONS_SHOULD_CLOSE_ISSUE_ON_DELETE is false, ignoring task.", task.taskReference,)

	//	}

	//		// This removes it from the code {ToDoConstants.TASK_MARKER} tracker we want to keep this regardless. Reduces calls for when you don"t want auto close.
	//		await task.markAsCompleted()

	//  }
	//}

	public async Task<ToDoTaskResolutionModel> AssignIssueToNewToDo(string todoUniqueKey, IToDo todo)
	{
		var entity = todo.ToTodDoEntity(todoUniqueKey, _toDoIssuesConfig.CodeRepoInfoModel.Name);
		await _mongoAgent.AcquireTaskCreationLock(entity, ProcessId);
		var resolution = await DataStore.beginTaskResolution(
		  todoUniqueKey,
		  CodeRepository.repoContext.repositoryNodeId,
		  todo,


		)
		  if ("existingTaskReference" in resolution) {
			return resolution.existingTaskReference
	}
		const taskCreationLock = await resolution.acquireTaskCreationLock()
		  _logger.LogDebug("Lock acquired. Now creating task for {ToDoConstants.TASK_MARKER} {}.", todoUniqueKey)
		  const {
			title,
		    body,
		    state,
		  } = TaskInformationGenerator.generateTaskInformationFromTodo(todo)
		  const taskReference = await TaskManagementSystem.createTask({ title, body })
		  taskCreationLock.finish(taskReference, state)
		  return taskReference
		}

}
