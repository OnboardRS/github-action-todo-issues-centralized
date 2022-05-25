

using OnboardRS.ToDoIssues.Business.Models.Mongo;

namespace OnboardRS.ToDoIssues.Business.Models.Tasks;

//public class ToDoTaskResolutionProcedure =
//	| { existingTaskReference: string }
//| { acquireTaskCreationLock(): Promise<TaskCreationLock> }

//public class ToDoTaskCreationLock = {
//	                        finish(taskReference: string, state: ITaskState): Promise<void>
//                        }

//public class ToDoTaskTask = {

//	            taskReference: string
//	            state: ITaskState
//	            markAsCompleted(): Promise<void>
//	            updateState(newState: ITaskState): Promise<void>
//            }

public class ToDoTaskResolutionModel
{
	public ToDoIssueModel? ToDoIssueModel { get; set; }
	public ToDoEntity ToDoEntity { get; set; }
	public string? IssueHash { get; set; }
	public string? ExistingIssueReference { get; set; }

	public ToDoTaskResolutionModel(ToDoEntity toDoEntity)
	{
		ToDoEntity = toDoEntity;
	}
}


public class ToDoIssueModel
{
	public ToDoIssueModel(string hash, string title, string body)
	{
		IssueHash = hash;
		Title = title;
		Body = body;
	}

	public string Title { get; set; }
	public string Body { get; set; }
	public string IssueHash { get; set; }


}