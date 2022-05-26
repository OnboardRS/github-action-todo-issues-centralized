using MongoDB.Bson;

namespace OnboardRS.ToDoIssues.Business.Models;

public class ToDoEntity : BaseReflectedToStringObject
{
	public ToDoEntity(ObjectId id, string repositoryId)
	{
		Id = id;
		RepositoryId = repositoryId;
		CreatedAt = DateTime.UtcNow;
	}

	/// <summary>
	///     Globally-unique ID for the issue.
	/// </summary>
	public ObjectId Id { get; set; }

	/// <summary>
	///     String identifying the repository.
	///     This should be stable, i.e.does not change even though project is renamed.
	/// </summary>
	public string RepositoryId { get; set; }

	/// <summary>
	///     The identifier of the associated issue.
	/// </summary>

	public string? IssueReference { get; set; }

	/// <summary>
	///     `true` if Issue is completed.
	/// </summary>
	public bool? Completed { get; set; }

	/// <summary>
	///     When the issue is created.
	/// </summary>
	public DateTime CreatedAt { get; set; }

	/// <summary>
	///     ID of the process creating it.
	/// </summary>
	public string? OwnerProcessId { get; set; }

	/// <summary>
	///     Timestamp at which the lock was acquired.
	/// </summary>
	public DateTime? OwnerProcessTimestamp { get; set; }

	/// <summary>
	///     Hash of the issue body contents
	/// </summary>
	public string? Hash { get; set; }
}