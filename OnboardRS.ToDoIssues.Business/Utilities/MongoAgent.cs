
using System.Diagnostics;
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
	private IMongoDatabase? _taskDataBase;
	private IMongoCollection<ToDoEntity>? _taskCollection;



	public string ProcessId
	{
		get
		{
			return Process.GetCurrentProcess().Id.ToString();
		}
	}

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
			if (null == _taskDataBase)
			{
				var mongoUrl = new MongoUrl(_config.MongoDbUrl);
				_taskDataBase = MongoClient.GetDatabase(_config.MongoDbName);
				ValidateDataBaseConnection();


			}
			return _taskDataBase;
		}
	}

	private IMongoCollection<ToDoEntity> ToDoMongoCollection
	{
		get
		{
			if (null == _taskCollection)
			{
				var exists = CollectionExistsAsync(_config.MongoDbCollectionName).Result;
				if (!exists)
				{
					MongoDatabase.CreateCollection(_config.MongoDbCollectionName);
				}

				_taskCollection = MongoDatabase.GetCollection<ToDoEntity>(_config.MongoDbCollectionName);
			}
			return _taskCollection;
		}
	}


	public MongoAgent(ToDoIssuesConfig config, ILogger<MongoAgent> logger)
	{
		_config = config;
		_logger = logger;
	}

	public void ValidateDataBaseConnection()
	{
		var doc = MongoDatabase.RunCommandAsync((Command<BsonDocument>)"{ping:1}").Result;
		if (null == doc)
		{
			throw new ApplicationException($"Could not connect to DB {_config.MongoDbName}");
		}
	}

	public async Task<bool> CollectionExistsAsync(string collectionName)
	{
		var filter = new BsonDocument("name", collectionName);
		//filter by collection name
		var collections = await MongoDatabase.ListCollectionsAsync(new ListCollectionsOptions { Filter = filter });
		//check for existence
		return await collections.AnyAsync();
	}

	public async Task<List<ToDoPersistanceModel>> GetExistingToDoComparisonModelsAsync()
	{
		var comparisonModels = new List<ToDoPersistanceModel>();
		var items = await FindAllUncompletedToDosAsync();
		foreach (var toDoEntity in items)
		{
			var comparisonModel = new ToDoPersistanceModel(toDoEntity);
			comparisonModels.Add(comparisonModel);
		}

		return comparisonModels;
	}

	/// <summary>
	/// This method is to be called when trying to turn a code stub into a persisted stub.
	/// </summary>
	/// <param name="toDo"></param>
	/// <returns></returns>
	public async Task<ToDoEntity?> AcquireTaskCreationLock(IToDo toDo)
	{
		if (null == toDo.IssueReference)
		{
			_logger.LogDebug($"Cannot acquire create log for null {nameof(toDo.IssueReference)}");
			return null;
		}
		if (!toDo.IssueReference.StartsWith(ToDoConstants.STUB_REFERENCE_MARKER))
		{
			_logger.LogDebug($"Cannot acquire create log for reference that doesn't start with {ToDoConstants.STUB_REFERENCE_MARKER}");
			return null;
		}

		var stubIssueReference = toDo.IssueReference.Substring(1);
		var existing = await FindToDoByIssueReferenceAsync(stubIssueReference);
		if (null != existing && existing.OwnerProcessTimestamp.HasValue && DateTime.UtcNow.Subtract(existing.OwnerProcessTimestamp.Value).TotalMinutes >= 90)
		{
			_logger.LogDebug($"Create lock already exists for issue reference {toDo.IssueReference}");
			return null;
		}

		var entity = existing ?? ToCreatableEntity(toDo, stubIssueReference, _config.CodeRepoInfoModel.Name);
		entity.OwnerProcessTimestamp = DateTime.UtcNow;
		entity.OwnerProcessId = ProcessId;
		var lockedEntity = await UpsertByIdAsync(entity);
		return lockedEntity;
	}

	public async Task ReleaseTaskCreationLock(ToDoEntity toDo)
	{
		toDo.OwnerProcessTimestamp = null;
		toDo.OwnerProcessId = null;
		await UpsertByIdAsync(toDo);
	}

	public static ToDoEntity ToCreatableEntity(IToDo todo, string stubIssueReference, string repositoryId)
	{
		var objectId = new ObjectId(Guid.NewGuid().ToString().Replace("-", string.Empty));
		var model = new ToDoEntity(objectId, repositoryId)
		{
			Completed = null,
			CreatedAt = DateTime.UtcNow,
			Hash = todo.GetToDoHash(),
			OwnerProcessId = null,
			OwnerProcessTimestamp = null,
			IssueReference = stubIssueReference
		};
		return model;
	}

	public async Task<ToDoEntity?> FindToDoByIssueIdAsync(string? id)
	{
		if (string.IsNullOrWhiteSpace(id))
		{
			return null;
		}

		var filter = Builders<ToDoEntity>.Filter.Eq(x => x.Id, new ObjectId(id));
		var findCursor = await ToDoMongoCollection.FindAsync(filter);
		var entity = await findCursor.FirstOrDefaultAsync();
		return entity;

	}

	public async Task<ToDoEntity?> FindToDoByIssueReferenceAsync(string? issueReference)
	{
		if (string.IsNullOrWhiteSpace(issueReference))
		{
			return null;
		}

		var filter = Builders<ToDoEntity>.Filter.Eq(x => x.IssueReference, issueReference);
		var findCursor = await ToDoMongoCollection.FindAsync(filter);
		var entity = await findCursor.FirstOrDefaultAsync();
		return entity;

	}

	public async Task<List<ToDoEntity>> FindAllTasksAsync()
	{
		var findCursor = await ToDoMongoCollection.FindAsync(Builders<ToDoEntity>.Filter.Empty);
		var tasks = await findCursor.ToListAsync();
		return tasks;

	}

	public async Task<List<ToDoEntity>> FindAllUncompletedToDosAsync()
	{
		var filter = Builders<ToDoEntity>.Filter.Eq(x => x.RepositoryId, _config.CodeRepoInfoModel.Name);
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
		var options = new FindOneAndReplaceOptions<ToDoEntity, ToDoEntity>
		{
			IsUpsert = true,
			ReturnDocument = ReturnDocument.After
		};
		var item = await ToDoMongoCollection.FindOneAndReplaceAsync(filter, model, options);
		return item;
	}
}
