using MongoDB.Bson;

namespace OnboardRS.ToDoIssues.Business.Models.Mongo;



public class MongoTodDoModel
{
	/// <summary>
	/// Globally-unique ID for the task.
	/// </summary>
	public ObjectId Id { get; set; }

	/// <summary>
	/// String identifying the repository.
	/// This should be stable, i.e.does not change even though project is renamed.
	/// </summary>
	public string RepositoryId { get; set; }

	/// <summary>
	/// The identifier of the associated task.
	/// </summary>

	public string? TaskReference { get; set; }

	/// <summary>
	/// `true` if Issue is completed.
	/// </summary>
	public bool? Completed { get; set; }

	/// <summary>
	/// When the task is created.
	/// </summary>
	public DateTime CreatedAt { get; set; }

	/// <summary>
	/// ID of the process creating it.
	/// </summary>
	public string? OwnerProcessId { get; set; }

	/// <summary>
	/// Timestamp at which the lock was acquired.
	/// </summary>
	public DateTime? OwnerProcessTimestamp { get; set; }

	/// <summary>
	/// Hash of the task body contents
	/// </summary>
	public string? Hash { get; set; }

	public MongoTodDoModel(ObjectId id, string repositoryId)
	{
		Id = id;
		RepositoryId = repositoryId;
		CreatedAt = DateTime.UtcNow;
	}
}

//type TaskResolutionProcedure =
//  | { existingTaskReference: string }
//  | { acquireTaskCreationLock(): Promise<TaskCreationLock> }

//type TaskCreationLock = {
//  finish(taskReference: string, state: ITaskState): Promise<void>
//}

//type Task = {
//  taskReference: string
//  state: ITaskState
//  markAsCompleted(): Promise<void>
//  updateState(newState: ITaskState): Promise<void>
//}