using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OnboardRS.ToDoIssues.Business.Models;
using OnboardRS.ToDoIssues.Business.Models.Config;

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


	public Task ProcessRepoToDoActionsAsync(ToDoIssuesConfig config)
	{
		_logger.LogInformation(config.ToString());
		return Task.CompletedTask;
	}
}
