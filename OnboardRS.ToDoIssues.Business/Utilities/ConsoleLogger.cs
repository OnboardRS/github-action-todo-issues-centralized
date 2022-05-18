using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace OnboardRS.ToDoIssues.Business.Utilities;

public class ConsoleLogger : ILogger, IDisposable
{
	public ConsoleLogger()
	{
		LogLevel = LogLevel.Debug;
	}

	public ConsoleLogger(LogLevel logLevel)
	{
		LogLevel = logLevel;
	}

	public LogLevel LogLevel { get; set; }

	public void Dispose()
	{
		GC.SuppressFinalize(this);
	}

	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
	{
		var message = string.Empty;
		message += formatter(state, exception);

		Console.WriteLine($"{logLevel} - {message}");
		Debug.WriteLine($"{logLevel} - {message}");
	}

	public bool IsEnabled(LogLevel logLevel)
	{
		var shouldLog = LogLevel.None != logLevel && logLevel >= LogLevel;
		return shouldLog;
	}

	public IDisposable BeginScope<TState>(TState state)
	{
		return this;
	}
}