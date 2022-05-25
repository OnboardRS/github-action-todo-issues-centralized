
using MongoDB.Bson;
using MongoDB.Driver;
using OnboardRS.ToDoIssues.Business.Models.Mongo;

namespace OnboardRS.ToDoIssues.Business.Utilities;



public class MongoAgent
{
	public const string TODO_DATABASE_NAME = "ToDosCentralized";
	public const string TODO_COLLECTION_NAME = "ToDos";

	private readonly ILogger<MongoAgent> _logger;
	private readonly ToDoIssuesConfig _config;
	private IMongoClient? _mongoClient;
	private IMongoDatabase? _taskDatabse;
	private IMongoCollection<MongoTodDoModel>? _taskCollection;

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

	private IMongoCollection<MongoTodDoModel> ToDoMongoCollection
	{
		get
		{
			if (null == _taskCollection)
			{
				_taskCollection = MongoDatabase.GetCollection<MongoTodDoModel>(TODO_COLLECTION_NAME);
			}
			return _taskCollection;
		}
	}


	public MongoAgent(ToDoIssuesConfig config, ILogger<MongoAgent> logger)
	{
		_config = config;
		_logger = logger;
	}

	//public async Task<TaskResolutionProcedure> beginTaskResolution(string todoUniqueKey, string repositoryId, IToDo todo)
	//	{
	//	const db = await getMongoDb()
	//  const _id = ObjectId.createFromHexString(todoUniqueKey)

	//  // Ensure a task exists in the database.
	//	const task = await db.tasks.findOneAndUpdate(

	//	{ _id: _id },
	//    {
	//      $setOnInsert:
	//		{
	//_id: _id,
	//        repositoryId: repositoryId,
	//        taskReference: null,
	//        createdAt: new Date(),
	//        ownerProcessId: null,
	//        ownerProcessTimestamp: null,
	//      },
	//    },
	//    { upsert: true, returnOriginal: false },
	//  )
	//  if (!task.value)
	//{
	//	throw new Error("Failed to upsert a task.")
	//  }
	//if (task.value.taskReference)
	//{
	//	__loggerger._loggerDebug(
	//	  "Found already-existing identifier {} for TODO {}.",
	//	  task.value.taskReference,
	//	  todoUniqueKey,


	//	)


	//	return { existingTaskReference: task.value.taskReference }
	//}

	//return {
	//	async acquireTaskCreationLock()
	//	{
	//		// Acquire a lock...
	//		__loggerger._loggerDebug(
	//		  "Acquiring lock for TODO {} (currentProcessId={}).",
	//		  todoUniqueKey,
	//		  currentProcessId,


	//		)


	//	  const lockedTask = await db.tasks.findOneAndUpdate(


	//		{
	//_id: _id,
	//          $or:
	//			[

	//			{ ownerProcessTimestamp: null },
	//            { ownerProcessTimestamp: { $lt: new Date(Date.now() - 60e3) } },
	//          ],
	//        },
	//        {
	//          $set:
	//			{
	//ownerProcessId: currentProcessId,
	//            ownerProcessTimestamp: new Date(),
	//          },
	//        },
	//        { returnOriginal: false },
	//      )
	//      if (!lockedTask.value)
	//	{
	//		throw new Error("Failed to acquire a lock for this task.")

	//	  }
	//	return {
	//		async finish(taskReference, state)
	//		{
	//			// Associate
	//			__loggerger._loggerDebug(
	//			  "Created task {} for TODO {}. Saving changes.",
	//			  taskReference,
	//			  todoUniqueKey,


	//			)


	//		  await db.tasks.findOneAndUpdate(


	//			{ _id: _id },
	//            { $set: { taskReference: taskReference, hash: state.hash } },
	//          )
	//        },
	//      }
	//},
	//  }
	//}

	public async Task<List<MongoTodDoModel>> FindAllTasksAsync()
	{
		var findCursor = await ToDoMongoCollection.FindAsync(Builders<MongoTodDoModel>.Filter.Empty);
		var tasks = await findCursor.ToListAsync();
		return tasks;

	}

	public async Task<List<MongoTodDoModel>> FindAllUncompletedTasksAsync(string repositoryId)
	{
		var filter = Builders<MongoTodDoModel>.Filter.Eq(x => x.RepositoryId, repositoryId);
		filter &= Builders<MongoTodDoModel>.Filter.Not(Builders<MongoTodDoModel>.Filter.Eq(x => x.Completed, false));
		filter &= Builders<MongoTodDoModel>.Filter.Not(Builders<MongoTodDoModel>.Filter.Eq(x => x.TaskReference, null));
		var findCursor = await ToDoMongoCollection.FindAsync(filter);
		var tasks = await findCursor.ToListAsync();
		return tasks;

	}

	public async Task MarkAsCompletedAsync(MongoTodDoModel model)
	{
		var filter = Builders<MongoTodDoModel>.Filter.Eq(x => x.Id, model.Id);
		var updater = Builders<MongoTodDoModel>.Update.Set(x => x.Completed, true);
		await ToDoMongoCollection.FindOneAndUpdateAsync(filter, updater);
	}


	public async Task UpdateStateAsync(MongoTodDoModel model, string newStateHash)
	{
		var filter = Builders<MongoTodDoModel>.Filter.Eq(x => x.Id, model.Id);
		var updater = Builders<MongoTodDoModel>.Update.Set(x => x.Hash, newStateHash);
		await ToDoMongoCollection.FindOneAndUpdateAsync(filter, updater);
	}
}
