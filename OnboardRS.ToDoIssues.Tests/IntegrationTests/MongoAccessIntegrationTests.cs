using System;

namespace OnboardRS.ToDoIssues.Tests;

[TestClass]
public class MongoAccessIntegrationTests : BaseIntegrationTests
{
	private const string _existingId = "adaf5884a8404b8483f31a6e";
	private const string _existingIssueReference = "-7";

	[TestInitialize]
	public void SetUp()
	{
		BaseSetUp();
	}

	[TestMethod]
	public async Task FindToDoByIssueIdAsyncMissingTest()
	{

		var entity = await MongoAgent.FindToDoByIssueIdAsync(Guid.NewGuid().ToString().Replace("-", string.Empty));
		Assert.IsNull(entity);
	}

	[TestMethod]
	public async Task FindToDoByIssueIdAsyncTest()
	{

		var entity = await MongoAgent.FindToDoByIssueIdAsync(_existingId);
		Assert.IsNotNull(entity);
		Assert.AreEqual(_existingIssueReference, entity.IssueReference);
	}

	[TestMethod]
	public async Task FindToDoByIssueReferenceAsyncTest()
	{
		var entity = await MongoAgent.FindToDoByIssueReferenceAsync(_existingIssueReference);
		Assert.IsNotNull(entity);
		Assert.AreEqual(_existingId, entity.Id.ToString());
	}

	[TestMethod]
	public async Task MongoUnassociatedCreateTest()
	{
		var toDo = GetDefaultTestToDo();
		var stubIssueReference = Guid.NewGuid().ToString();
		toDo.IssueReference = "$" + stubIssueReference;
		var lockedEntity = await MongoAgent.AcquireToDoCreationLock(toDo);
		Assert.IsNotNull(lockedEntity);
	}
}