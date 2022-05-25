

using OnboardRS.ToDoIssues.Business.Interfaces;

namespace OnboardRS.ToDoIssues.Tests.Models
{

	/// <summary>
	/// A mock file.
	/// </summary>
	public class ToDoMockFile : IToDoFile
	{
		public string FileName { get; set; }
		public IToDoFileContents Contents { get; set; }

		public ToDoMockFile(string fileName, string testContents)
		{
			FileName = fileName;
			Contents = new ToDoFileContents(testContents);
		}

		public Task SaveToDoFileAsync()
		{
			Contents.Changed = false;
			return Task.CompletedTask;
		}
	}
}
