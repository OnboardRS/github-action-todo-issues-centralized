using Microsoft.Extensions.Logging;

namespace OnboardRS.ToDoIssues.Business.Models;

public abstract class ToDoIssuesConfig : BaseReflectedToStringObject
{
	public const string ASPNETCORE_ENVIRONMENT_KEY = "ASPNETCORE_ENVIRONMENT";

	private string? _aspNetCoreEnvironment;

	protected ToDoIssuesConfig(ILogger logger)
	{
		Logger = logger;
		Logger.LogInformation($"Application config created with ${ASPNETCORE_ENVIRONMENT_KEY} of [{AspNetCoreEnvironment}]");
	}

	protected ILogger Logger { get; set; }

	public string? AspNetCoreEnvironment
	{
		get
		{
			if (string.IsNullOrWhiteSpace(_aspNetCoreEnvironment))
			{
				_aspNetCoreEnvironment = GetEnvironmentVariable(ASPNETCORE_ENVIRONMENT_KEY);
			}

			return _aspNetCoreEnvironment;
		}
		set => _aspNetCoreEnvironment = value;
	}

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
