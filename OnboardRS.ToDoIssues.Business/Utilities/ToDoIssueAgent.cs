using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OnboardRS.ToDoIssues.Business.Models;

namespace OnboardRS.ToDoIssues.Business.Utilities;

public class ToDoIssueAgent
{
	private readonly ILogger<ToDoIssueAgent> _logger;

	public ToDoIssueAgent(ILogger<ToDoIssueAgent> logger)
	{
		_logger = logger;
		_logger.LogInformation("Test information log via logger.");
		Console.WriteLine("Test console write line.");
	}


	public Task ProcessRepoToDoActionsAsync(RepoInfoModel codeRepo, RepoInfoModel issueRepo)
	{
		_logger.LogInformation("Code Repo:");
		_logger.LogInformation(codeRepo.ToString());
		_logger.LogInformation("Issue Repo:");
		_logger.LogInformation(issueRepo.ToString());
		return Task.CompletedTask;
	}
}
