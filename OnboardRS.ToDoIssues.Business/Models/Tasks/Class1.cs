

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

public class ToDoPersistanceModel
{
	public ToDoIssueModel? ToDoIssueModel { get; set; }
	public ToDoEntity ToDoEntity { get; set; }
	public string? DbIssueHash { get; set; }
	public string? DbIssueReference { get; set; }

	public ToDoPersistanceModel(ToDoEntity toDoEntity)
	{
		ToDoEntity = toDoEntity;
		DbIssueHash = toDoEntity.Hash;
		DbIssueReference = toDoEntity.IssueReference;
	}
}


public class ToDoIssueModel
{
	public ToDoIssueModel(string issueNumber, string title, string body)
	{

		IssueNumber = issueNumber;
		Title = title;
		Body = body;
	}

	public string Title { get; set; }
	public string Body { get; set; }
	public string IssueNumber{ get; set; }


}