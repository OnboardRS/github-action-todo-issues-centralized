using System.Diagnostics;
using MongoDB.Bson;
using OnboardRS.ToDoIssues.Business.Interfaces;

namespace OnboardRS.ToDoIssues.Business.Utilities;

public class ToDoIssueAgent
{
	private readonly ILogger<ToDoIssueAgent> _logger;
	private readonly ToDoIssuesConfig _toDoIssuesConfig;

	public ToDoIssueAgent(ToDoIssuesConfig toDoIssuesConfig, ILogger<ToDoIssueAgent> logger)
	{
		_logger = logger;
		_toDoIssuesConfig = toDoIssuesConfig;
	}


	public async Task ProcessRepoToDoActionsAsync()
	{
		_logger.LogInformation("Search for files with TODO tags...");
		var toDoFiles = await GetToDoFilesFromRepositoryAsync();

		var todoComments = new List<IToDo>();
		foreach (var toDoFile in toDoFiles)
		{
			// TODO: Implement ignoring paths

			if (_toDoIssuesConfig.ExcludeList.Any(x => x == toDoFile.FileName))
			{
				continue;
			}

			List<IToDo> todos = await toDoFile.ParseToDosAsync();
			_logger.LogInformation($"{toDoFile.FileName}: {todos.Count} found.");
			todoComments.AddRange(todos);
		}


		_logger.LogInformation($"Total TODOs found: {todoComments.Count}");
		var todosWithoutReference = todoComments.Where(todo => null == todo.Reference).ToList();
		_logger.LogInformation($"TODOs without references: {todosWithoutReference.Count}");

		if (todosWithoutReference.Any())
		{
			foreach (var todo in todosWithoutReference)
			{
				//Create a stub id to be replaced later, starts with $
				todo.Reference = $"${ BsonUtils.ToHexString(new ObjectId().ToByteArray())}";
			}

			List<IToDoFile> todoFilesWithoutReference = todosWithoutReference.Select(x => x.ToDoFile).Distinct().ToList();
			await todoFilesWithoutReference.SaveChanges($"Collecting {todosWithoutReference.Count} new TODO comments.", _logger);
		}

		// Every item must have a reference by now.
		foreach (var todo in todoComments)
		{
			Debug.Assert(null != todo.ToDoFile, $"TODO {todo.Title} at {todo.ToDoFile?.FileName} must have a filename by now!");
			Debug.Assert(null != todo.Reference, $"TODO {todo.Title} at {todo.ToDoFile?.FileName} must have a reference by now!");
		}

		// Update all the tasks according to the item state.
		var associated = await EnsureAllTodosAreAssociatedAsync(todoComments);
		//await associated.SaveChanges("Updated TODO references: " + associated.join(", "), _logger);

		// Reconcile all tasks
		//await ReconcileTasksAsync(todoComments);
	}

	public async Task<List<IToDoFile>> GetToDoFilesFromRepositoryAsync()
	{
		_logger.LogInformation("Search for files with TODO tags...");
		var result = await "git grep -Il TODO".RunBashCommandAsync(_logger);

		//split on newlines and remove duplicates
		var paths = result.Split("\n").ToHashSet().ToList();

		_logger.LogInformation("Parsing TODO tags...");
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

	//	import { invariant, logger
	//}
	//from "tkt"
	//import { ITodo } from "./types"

	//import * as CodeRepository from "./CodeRepository"
	//import * as TaskManagementSystem from "./TaskManagementSystem"
	//import * as DataStore from "./DataStore"
	//import * as TaskInformationGenerator from "./TaskInformationGenerator"

	//const log = logger("TaskUpdater")
	//let shouldCloseIssueOnDelete : boolean | null = null;

	//export function getShouldCloseIssueOnDelete(){
	//	_logger.LogDebug("Environment variable process.env.TODO_ACTIONS_SHOULD_CLOSE_ISSUE_ON_DELETE: " + process.env.TODO_ACTIONS_SHOULD_CLOSE_ISSUE_ON_DELETE)
	//  if (shouldCloseIssueOnDelete == null)
	//	{
	//		shouldCloseIssueOnDelete = "true" === (
	//		  process.env.TODO_ACTIONS_SHOULD_CLOSE_ISSUE_ON_DELETE ||
	//			invariant(
	//			false,
	//			"Missing environment variable: ")
	//		  )

	//	  _logger.LogDebug("Calulated shouldCloseIssueOnDelete: " + shouldCloseIssueOnDelete)
	//  }
	//	return shouldCloseIssueOnDelete;
	//}

	public async Task<List<string>> EnsureAllTodosAreAssociatedAsync(List<IToDo> todos)
	{
		var references = new List<string>();
		foreach (var todo in todos)
		{
			if (null == todo.Reference)
			{
				var errorMessage = $"Unexpected unidentified TODO marker.";
				_logger.LogError(errorMessage);
				throw new ArgumentException(errorMessage);
			}

			string reference = todo.Reference;


			var unassociated = todo.Reference.StartsWith("$");


			if (unassociated)
			{
				// TODO: Isolate error when creating tasks
				// Failure to create a task should not prevent the action from progressing forward.
				// We can simply skip processing this comment for now.
				// Since this script is designed to be idempotent, it can be retried later.
				var todoUniqueKey = reference.Substring(1);
				_logger.LogDebug("Found unresolved TODO {}, resolving task...", todoUniqueKey);
				//var taskReference = await ResolveTaskAsync(todoUniqueKey, todo);
				//_logger.LogDebug($"Resolved TODO {todoUniqueKey} => task {taskReference}");
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
	//		  todo.reference || invariant(false, "Unexpected unidentified TODO marker")

	//	invariant(
	//	  !reference.startsWith("$"),
	//	  "Expected all TODO comments to be associated by now.",

	//	)

	//	const task = uncompletedTasks.find(t => t.taskReference === reference)

	//	if (!task)
	//		{
	//			log.warn(
	//			  "Cannot find a matching task for TODO comment with reference "{}"",
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
	//			log.info("TODO for task "{}" is gone -- completing task!", task.taskReference,)
	//	      // TODO [#5]: Isolate error when completing tasks
	//		  // Failure to complete a task should not prevent the action from progressing forward.
	//		  // We can simply skip processing this task for now.
	//		  // Since this script is designed to be idempotent, it can be retried later.
	//			await TaskManagementSystem.completeTask(task.taskReference)

	//	}
	//		else
	//		{
	//			// TODO [#8]: Test to remove later A
	//			log.info("TODO for task "{}" is gone TODO_ACTIONS_SHOULD_CLOSE_ISSUE_ON_DELETE is false, ignoring task.", task.taskReference,)

	//	}

	//		// This removes it from the code TODO tracker we want to keep this regardless. Reduces calls for when you don"t want auto close.
	//		await task.markAsCompleted()

	//  }
	//}

//	public async Task<string> resolveTask(string todoUniqueKey, IToDo todo)
//	{
//		var resolution = await DataStore.beginTaskResolution(
//		  todoUniqueKey,
//		  CodeRepository.repoContext.repositoryNodeId,
//		  todo,

//		)
//	  if ("existingTaskReference" in resolution) {
//			return resolution.existingTaskReference
//}
//const taskCreationLock = await resolution.acquireTaskCreationLock()
//	  _logger.LogDebug("Lock acquired. Now creating task for TODO {}.", todoUniqueKey)
//	  const {
//			title,
//	    body,
//	    state,
//	  } = TaskInformationGenerator.generateTaskInformationFromTodo(todo)
//	  const taskReference = await TaskManagementSystem.createTask({ title, body })
//	  taskCreationLock.finish(taskReference, state)
//	  return taskReference
//	}

}
