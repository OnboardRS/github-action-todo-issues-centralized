using OnboardRS.ToDoIssues.Business;
using OnboardRS.ToDoIssues.Business.Interfaces;
using OnboardRS.ToDoIssues.Business.Models;
using OnboardRS.ToDoIssues.Business.Utilities;
using OnboardRS.ToDoIssues.GitHubAction.Extensions;


using IHost host = Host.CreateDefaultBuilder(args)
					   .ConfigureServices((_, services) => services.AddGitHubActionServices(args))
					   .Build();

static TService Get<TService>(IHost host)
	where TService : notnull
{
	return host.Services.GetRequiredService<TService>();
}

static async Task StartToDoIssueProcessAsync(IHost host)
{
	var toDoIssueAgent = Get<ToDoIssueAgent>(host);
	await toDoIssueAgent.ProcessRepoToDoActionsAsync();
	Environment.Exit(ExitCodes.NORMAL);
}


await StartToDoIssueProcessAsync(host);
await host.RunAsync();