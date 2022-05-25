
using MongoDB.Bson;
using MongoDB.Driver;
using OnboardRS.ToDoIssues.Business.Models.Mongo;
using OnboardRS.ToDoIssues.Business.Models.Tasks;

namespace OnboardRS.ToDoIssues.Business.Utilities;



public class MongoAgent
{
	public const string TODO_DATABASE_NAME = "ToDosCentralized";
	public const string TODO_COLLECTION_NAME = "ToDos";

	private readonly ILogger<MongoAgent> _logger;
	private readonly ToDoIssuesConfig _config;
	private IMongoClient? _mongoClient;
	private IMongoDatabase? _taskDatabse;
	private IMongoCollection<ToDoEntity>? _taskCollection;

	private IMongoClient MongoClient
	{
		get
		{
			if (null == _mongoClient)
			{
				_mongoClient = new MongoClient(_config.MongoDbUrl);
			}
			return _mongoClient;
		}
	}
	private IMongoDatabase MongoDatabase
	{
		get
		{
			if (null == _taskDatabse)
			{
				var mongoUrl = new MongoUrl(_config.MongoDbUrl);
				_taskDatabse = MongoClient.GetDatabase(TODO_DATABASE_NAME);
			}
			return _taskDatabse;
		}
	}

	private IMongoCollection<ToDoEntity> ToDoMongoCollection
	{
		get
		{
			if (null == _taskCollection)
			{
				_taskCollection = MongoDatabase.GetCollection<ToDoEntity>(TODO_COLLECTION_NAME);
			}
			return _taskCollection;
		}
	}


	public MongoAgent(ToDoIssuesConfig config, ILogger<MongoAgent> logger)
	{
		_config = config;
		_logger = logger;
	}

	public async Task<ToDoTaskResolutionModel> BeginTaskResolution(string todoUniqueKey, string repositoryId, IToDo todo)
	{
		var entity = todo.ToTodDoEntity(todoUniqueKey, repositoryId);

		// Ensure a task exists in the database.
		var upsertedEntity = await UpsertByIdAsync(entity);
		if (null == upsertedEntity)
		{
			throw new ApplicationException("Failed to upsert a task.");
		}

		if (upsertedEntity.IssueReference != null)
		{
			_logger.LogDebug($"Found already-existing identifier {upsertedEntity.IssueReference} for TODO {todoUniqueKey}.");
		}

		var result = new ToDoTaskResolutionModel(upsertedEntity);
		return result;
	}

	public async Task<ToDoEntity> AcquireTaskCreationLock(ToDoEntity model, string currentProcessId)
	{
		// Acquire a lock...
		_logger.LogDebug($"Acquiring lock for TODO {model.Id} (currentProcessId={currentProcessId}).");

		model.OwnerProcessId = currentProcessId;
		model.OwnerProcessTimestamp = DateTime.UtcNow;
		var lockedTask = await UpsertByLockOrIdAsync(model);

		if (null == lockedTask)
		{
			throw new ApplicationException("Failed to acquire a lock for this task.");

		}

		return lockedTask;
	}

	public async Task Finish(ToDoEntity model, string taskReference, IToDoTaskState state)
	{
		//// Associate
		//_logger.LogDebug($"Created task {taskReference} for TODO {model.Id}. Saving changes.");
		//model.TaskReference = 

		//	  await db.tasks.findOneAndUpdate(



		//           { $set: { taskReference: taskReference, hash: state.hash } },
		//         )
		//       },
	}

	public async Task<List<ToDoEntity>> FindAllTasksAsync()
	{
		var findCursor = await ToDoMongoCollection.FindAsync(Builders<ToDoEntity>.Filter.Empty);
		var tasks = await findCursor.ToListAsync();
		return tasks;

	}

	public async Task<List<ToDoEntity>> FindAllUncompletedTasksAsync(string repositoryId)
	{
		var filter = Builders<ToDoEntity>.Filter.Eq(x => x.RepositoryId, repositoryId);
		filter &= Builders<ToDoEntity>.Filter.Not(Builders<ToDoEntity>.Filter.Eq(x => x.Completed, false));
		filter &= Builders<ToDoEntity>.Filter.Not(Builders<ToDoEntity>.Filter.Eq(x => x.IssueReference, null));
		var findCursor = await ToDoMongoCollection.FindAsync(filter);
		var tasks = await findCursor.ToListAsync();
		return tasks;

	}

	public async Task MarkAsCompletedAsync(ToDoEntity model)
	{
		var filter = Builders<ToDoEntity>.Filter.Eq(x => x.Id, model.Id);
		var updater = Builders<ToDoEntity>.Update.Set(x => x.Completed, true);
		await ToDoMongoCollection.FindOneAndUpdateAsync(filter, updater);
	}


	public async Task UpdateStateAsync(ToDoEntity model, string newStateHash)
	{
		var filter = Builders<ToDoEntity>.Filter.Eq(x => x.Id, model.Id);
		var updater = Builders<ToDoEntity>.Update.Set(x => x.Hash, newStateHash);
		await ToDoMongoCollection.FindOneAndUpdateAsync(filter, updater);
	}

	public async Task<ToDoEntity?> UpsertByLockOrIdAsync(ToDoEntity model)
	{
		var filter = Builders<ToDoEntity>.Filter.Eq(x => x.Id, model.Id);
		filter |= (Builders<ToDoEntity>.Filter.Eq(x => x.OwnerProcessId, model.OwnerProcessId) | Builders<ToDoEntity>.Filter.Eq(x => x.OwnerProcessTimestamp, model.OwnerProcessTimestamp));
		var item = await UpsertAsync(filter, model);
		return item;
	}

	public async Task<ToDoEntity?> UpsertByIdAsync(ToDoEntity model)
	{
		var filter = Builders<ToDoEntity>.Filter.Eq(x => x.Id, model.Id);
		var item = await UpsertAsync(filter, model);
		return item;
	}

	private async Task<ToDoEntity?> UpsertAsync(FilterDefinition<ToDoEntity>? filter, ToDoEntity model)
	{
		var item = await ToDoMongoCollection.FindOneAndReplaceAsync(filter, model, new FindOneAndReplaceOptions<ToDoEntity, ToDoEntity>()
		{
			IsUpsert = true
		});
		return item;
	}
}
