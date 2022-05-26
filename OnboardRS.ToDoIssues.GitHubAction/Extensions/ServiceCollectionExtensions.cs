using OnboardRS.ToDoIssues.Business.Interfaces;
using OnboardRS.ToDoIssues.Business.Utilities;

namespace OnboardRS.ToDoIssues.GitHubAction.Extensions;

public static class ServiceCollectionExtensions
{
	internal static IServiceCollection AddGitHubActionServices(this IServiceCollection services, string[] args)
	{
		var logger = new ConsoleLogger(LogLevel.Debug);
		ActionInputs? actionInputs = null;
		try
		{
			var parser = Default.ParseArguments(() => new ActionInputs(), args);
			parser.WithNotParsed(
			                     errors =>
			                     {
				                     logger.LogError(string.Join(IToDoFileContents.UNIX_LINE_ENDING, errors.Select(error => error.ToString())));
				                     Environment.Exit(ExitCodes.ACTION_PARSE_ERROR);
			                     });

			actionInputs = parser.Value;
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
			Environment.Exit(ExitCodes.ACTION_PARSE_EXCEPTION);
		}

		var config = actionInputs.ToToDoIssuesConfig();
		try
		{
			config.ValidateInputs();
		}
		catch (ApplicationException e)
		{
			logger.LogError(e, e.Message);
			logger.LogError("Inputs invalid, shutting down.");
			Environment.Exit(ExitCodes.ACTION_CONFIG_ERROR);
		}

		services.AddSingleton(actionInputs);
		services.AddSingleton(config);
		services.AddTransient<MongoAgent>();
		services.AddTransient<GitHubAgent>();
		services.AddTransient<ToDoIssueAgent>();
		services.AddLogging(options =>
		                    {
			                    options.AddConsole()
				                    .AddDebug()
				                    .SetMinimumLevel(LogLevel.Trace);
		                    });
		return services;
	}
}