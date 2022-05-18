using OnboardRS.ToDoIssues.Business.Utilities;

namespace OnboardRS.ToDoIssues.GitHubAction.Extensions;

public static class ServiceCollectionExtensions
{
	internal static IServiceCollection AddGitHubActionServices(this IServiceCollection services)
	{
		services.AddScoped<ToDoIssueAgent>();
		services.AddLogging(options => {
			options.AddConsole()
			.AddDebug()
			.SetMinimumLevel(LogLevel.Trace);			
			});
		return services;
	}

}