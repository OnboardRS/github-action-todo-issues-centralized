using Microsoft.Extensions.Logging;
using OnboardRS.ToDoIssues.Business.Utilities;

namespace OnboardRS.ToDoIssues.Business.Models.Config;

public class ToDoIssuesConfig : BaseReflectedToStringObject
{
	public string GitHubAccessToken { get; set; }
	public string MongoDbName { get; set; } = MongoAgent.TODO_DATABASE_NAME;
	public string MongoDbCollectionName { get; set; } = MongoAgent.TODO_COLLECTION_NAME;
	public string MongoDbUrl { get; set; }
	public string IssueLabel { get; set; }
	public RepoInfoModel CodeRepoInfoModel { get; set; }
	public RepoInfoModel IssueRepoInfoModel { get; set; }

	public List<string> ExcludeList { get; set; } = new()
	{
		"README.md"
	};

	public ToDoIssuesConfig(string gitHubAccessToken, string mongoDbUrl, string issueLabel, RepoInfoModel codeRepoInfoModel, RepoInfoModel issueRepoInfoModel)
	{
		GitHubAccessToken = gitHubAccessToken;
		MongoDbUrl = mongoDbUrl;
		IssueLabel = issueLabel;
		CodeRepoInfoModel = codeRepoInfoModel;
		IssueRepoInfoModel = issueRepoInfoModel;
	}

	public void ValidateInputs()
	{
		AssertNotNullOrNotEmpty(nameof(GitHubAccessToken), GitHubAccessToken);
		AssertNotNullOrNotEmpty(nameof(MongoDbUrl), MongoDbUrl);
		AssertNotNullOrNotEmpty(nameof(IssueLabel), IssueLabel);
		CodeRepoInfoModel.ValidateInputs();
		IssueRepoInfoModel.ValidateInputs();
	}
}

public abstract class BaseConfig : BaseReflectedToStringObject
{
	protected BaseConfig(ILogger logger)
	{
		Logger = logger;
	}

	protected ILogger Logger { get; set; }

	public string GetExpectedEnvironmentVariable(string variableName)
	{
		var result = Environment.GetEnvironmentVariable(variableName);
		if (null == result)
		{
			var message = $"Missing environment variable with key [{variableName}]";
			Logger.LogError(message);
			throw new ApplicationException(message);
		}

		return result;
	}

	public string? GetEnvironmentVariable(string variableName, string? defaultValue = null, bool suppressValueInLogs = false)
	{
		Logger.LogDebug($"Trying to pull environment variable with key [{variableName}]");
		var result = Environment.GetEnvironmentVariable(variableName);
		if (null == result)
		{
			Logger.LogDebug($"Environment variable with key [{variableName}] not found. Defaulting to [{defaultValue}]");
			result = defaultValue;
		}
		else if (!suppressValueInLogs)
		{
			Logger.LogDebug($"Environment variable with key [{variableName}] returning value of [{result}]");
		}

		return result;
	}

	public int GetEnvironmentVariableAsInt(string variableName)
	{
		var stringValue = GetExpectedEnvironmentVariable(variableName);
		if (int.TryParse(stringValue, out var result))
		{
			return result;
		}

		var message = $"Environment variable with key {variableName} and value {stringValue} is not parseable to an integer.";
		Logger.LogError(message);
		throw new ApplicationException(message);
	}

	public int GetEnvironmentVariableAsIntWithDefault(string variableName, int defaultValue)
	{
		var stringValue = GetEnvironmentVariable(variableName);
		if (int.TryParse(stringValue, out var result))
		{
			return result;
		}

		return defaultValue;
	}

	protected string? GetCachedStringKeyedEnvironmentValue(ref string? backingField, string variableName)
	{
		if (string.IsNullOrWhiteSpace(backingField))
		{
			backingField = GetEnvironmentVariable(variableName);
		}
		return backingField;
	}

	protected string GetExpectedCachedStringKeyedEnvironmentValue(ref string? backingField, string variableName)
	{
		if (string.IsNullOrWhiteSpace(backingField))
		{
			backingField = GetExpectedEnvironmentVariable(variableName);
		}
		return backingField;
	}

	protected string GetCachedStringKeyedEnvironmentValueWithDefault(ref string? backingField, string variableName, string defaultValue)
	{
		if (string.IsNullOrWhiteSpace(backingField))
		{
			backingField = GetEnvironmentVariable(variableName);
		}

		string defaultedResult;
		if (null == backingField)
		{
			Logger.LogWarning($"Missing environment variable [{variableName}]. Using default of [{defaultValue}]");
			defaultedResult = defaultValue;
		}
		else
		{
			defaultedResult = backingField;
		}

		backingField = defaultedResult;
		return backingField;
	}
}
