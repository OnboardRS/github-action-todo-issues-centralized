namespace OnboardRS.ToDoIssues.Business.Utilities;

public class ToDoIssueAgent
{
	private readonly ILogger<ToDoIssueAgent> _logger;
	private ToDoIssuesConfig? _toDoIssuesConfig;

	private ToDoIssuesConfig ToDoIssuesConfig
	{
		get
		{
			if (null == _toDoIssuesConfig)
			{
				string message = $"Cannot use {nameof(ToDoIssuesConfig)} in class {nameof(ToDoIssueAgent)} until {nameof(SetToDoIssuesConfig)} as been called.";
				_logger.LogError(message);
				throw new ApplicationException(message);
			}

			return _toDoIssuesConfig;
		}
	}

	public ToDoIssueAgent(ILogger<ToDoIssueAgent> logger)
	{
		_logger = logger;
	}

	public void SetToDoIssuesConfig(ToDoIssuesConfig config)
	{
		_toDoIssuesConfig = config;

		//Show configuration values.
		_logger.LogInformation(config.ToString());
	}


	public async Task ProcessRepoToDoActionsAsync()
	{

		_logger.LogInformation("Search for files with TODO tags...");
		var toDoFiles = await GetToDoFilesFromRepositoryAsync();

		var todoComments = new List<IToDo>();
		foreach (var toDoFile in toDoFiles)
		{
			// TODO: Implement ignoring paths
			if (toDoFile.FileName == "README.md")
			{
				continue;
			}
			
			//List<ToDoModel> todos = 
			//	const todos = TodoParser.parseTodos(file)

			//	_logger.LogInformation('%s: %s found', file.fileName, todos.length)

			//	todoComments.push(...todos)
			//}
		}


		//_logger.LogInformation('Total TODOs found: %s', todoComments.length)
		//const todosWithoutReference = todoComments.filter(todo => !todo.reference)
		//_logger.LogInformation('TODOs without references: %s', todosWithoutReference.length)

		//if (todosWithoutReference.length > 0)
		//{
		//	for (const todo of todosWithoutReference) {
		//		todo.reference = `$${ new ObjectId().toHexString()}`
		//	}
		//	await saveChanges('Collect TODO comments')
		//}

		//// Every TODO must have a reference by now.
		//for (const todo of todoComments) {
		//	invariant(
		//	          todo.reference,
		//	          'TODO "%s" at %s must have a reference by now!',
		//	          todo.title,
		//	          todo.file.fileName,

		//	         )
		//}

		//// Update all the tasks according to the TODO state.
		//const associated = await TaskUpdater.ensureAllTodosAreAssociated(todoComments)
		//await saveChanges('Update TODO references: ' + associated.join(', '))

		//// Reconcile all tasks
		//await TaskUpdater.reconcileTasks(todoComments)
		
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

		//		_logger.LogInformation('Files changed: %s', changedFiles.length)

		//		if (changedFiles.length === 0)
		//		{
		//			return

		//		}
		//		for (const file of changedFiles) {
		//			file.save()

		//		}
		//		execFileSync('git', ['add', ...changedFiles.map(file => file.fileName)])

		//		execFileSync('git', ['commit', '-m', commitMessage], {
		//			stdio: 'inherit',
		//		})
		//		if (!process.env.GITHUB_TOKEN)
		//		{
		//			throw `Maybe you forgot to enable the GITHUB_TOKEN secret?`

		//		}
		//		execSync(
		//		         'git push "https://x-access-token:$GITHUB_TOKEN@github.com/$GITHUB_REPOSITORY.git" HEAD:"$GITHUB_REF"',

		//		{ stdio: 'inherit' },
		//		)
	}

}
