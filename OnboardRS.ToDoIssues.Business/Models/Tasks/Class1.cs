

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


public class ToDoTask
{
	public ToDoTask(string taskReference, IToDoTaskState taskState)
	{
		TaskReference = taskReference;
		TaskState = taskState;
	}

	public string TaskReference { get; set; }
	public IToDoTaskState TaskState { get; set; }


}