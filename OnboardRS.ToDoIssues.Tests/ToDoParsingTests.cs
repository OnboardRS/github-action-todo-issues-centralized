using OnboardRS.ToDoIssues.Business;

namespace OnboardRS.ToDoIssues.Tests;

[TestClass]
public class ToDoParsingTests
{

	public string InheritedParserTestCase01String = @$"

        // {ToDoConstants.TASK_MARKER}: Item 1

        // {ToDoConstants.TASK_MARKER}: Item 2
        // Body

        // {ToDoConstants.TASK_MARKER}: Item 3
        //
        // Extended body

        // Not part of {ToDoConstants.TASK_MARKER}

        /*
         * {ToDoConstants.TASK_MARKER}: Item 4
         * Body
         *
         * Extended body
         */

        <!--
          - {ToDoConstants.TASK_MARKER}: Item 5
          - Body
          -
          - Extended body

          Not part of TODO
          -->
        
          # {ToDoConstants.TASK_MARKER}: Item 6
          # Body
          #
          # Extended body
          #-
          # Not part of TODO
";

	[TestMethod]
	public async Task InheritedParserTestCase01ParsesToDos()
	{
		var file = new ToDoMockFile("main.js", InheritedParserTestCase01String);
		var result = await file.ParseToDosAsync();
		Assert.AreEqual(6, result.Count, "Parse Length");
		for (var index = 0; index < result.Count; index++)
		{
			var toDo = result[index];
			Assert.AreEqual(file, toDo.ToDoFile, $"File not assigned. Index {index}");
			var expectedTitle = $"Item {index + 1}";
			Assert.AreEqual(expectedTitle, toDo.Title, $"Title not assigned. Index {index}");
		}
		Assert.AreEqual("", result[0].Body);
		Assert.AreEqual("Body", result[1].Body);
		Assert.AreEqual("Extended body", result[2].Body);
		Assert.AreEqual("Body\n\nExtended body", result[3].Body);
		Assert.AreEqual("Body\n\nExtended body", result[4].Body);
		Assert.AreEqual("Body\n\nExtended body", result[5].Body);
	}


	private string InheritedParserTestCase02String = @$"		// {ToDoConstants.TASK_MARKER} [#1]: Item 1

		// {ToDoConstants.TASK_MARKER} [$wow]: Item 2

	// {ToDoConstants.TASK_MARKER} [todo-actions#1]: Item 3

	// {ToDoConstants.TASK_MARKER} [https://github.com/dtinth/todo-actions/issues/1]: Item 4";

	[TestMethod]
	public async Task InheritedParserTestCase02DetectsReference()
	{
		var file = new ToDoMockFile("main.js", InheritedParserTestCase02String);
		var result = await file.ParseToDosAsync();
		Assert.AreEqual(4, result.Count, "Parse Length");
		Assert.AreEqual("#1", result[0].Reference);
		Assert.AreEqual("$wow", result[1].Reference);
		Assert.AreEqual("todo-actions#1", result[2].Reference);
		Assert.AreEqual("https://github.com/dtinth/todo-actions/issues/1", result[3].Reference);
	}


	private string InheritedParserTestCase03String = @$"        // {ToDoConstants.TASK_MARKER}:
        // Title
        // Body";

	[TestMethod]
	public async Task InheritedParserTestCase03AllowsNextLineTitle()
	{
		var file = new ToDoMockFile("main.js", InheritedParserTestCase03String);
		var result = await file.ParseToDosAsync();
		Assert.AreEqual(1, result.Count, "Parse Length");
		Assert.AreEqual("Title", result[0].Title);
		Assert.AreEqual("Body", result[0].Body);
	}

	[TestMethod]
	[DataRow($"// {ToDoConstants.TASK_MARKER} [#14]: Dogs", true, "//", "#14", "Dogs")]
	[DataRow($"// {ToDoConstants.TASK_MARKER}: Item 1", true, "//", "", "Item 1")]
	[DataRow($"// {ToDoConstants.TASK_MARKER}: Item 2", true, "//", "", "Item 2")]
	[DataRow($"// {ToDoConstants.TASK_MARKER}: Item 3", true, "//", "", "Item 3")]
	[DataRow($" * {ToDoConstants.TASK_MARKER}: Item 4", true, "*", "", "Item 4")]
	[DataRow($" - {ToDoConstants.TASK_MARKER}: Item 5", true, "-", "", "Item 5")]
	[DataRow($"# {ToDoConstants.TASK_MARKER}: Item 6", true, "#", "", "Item 6")]

	public void ParseLineForToDoTest(string testString, bool isMatch, string expectedPrefix, string expectedReference, string expectedSuffix)
	{
		var result = testString.ParseLineForToDo();
		if (isMatch)
		{
			Assert.IsNotNull(result, testString);
			Assert.AreEqual(testString.Trim(), result.Groups[0].Value.Trim(), testString);
			Assert.AreEqual(expectedPrefix.Trim(), result.Groups[1].Value.Trim(), testString);
			Assert.AreEqual(expectedReference.Trim(), result.Groups[2].Value.Trim(), testString);
			Assert.AreEqual(expectedSuffix.Trim(), result.Groups[3].Value.Trim(), testString);
		}
		else
		{
			Assert.IsNull(result, testString);
		}
	}
}