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

static async Task StartToDoIssueProcessAsync(ActionInputs inputs, IHost host)
{
	var logger = Get<ILogger<Program>>(host);
	logger.LogInformation("Program: Test information log via logger.");
	Console.WriteLine("Program: Test console write line.");

	logger.LogInformation($"{ToDoConstants.TASK_MARKER} Label:");
	logger.LogInformation(inputs.IssueLabel);

	var toDoIssueAgent = Get<ToDoIssueAgent>(host);
	await toDoIssueAgent.ProcessRepoToDoActionsAsync();

	//using ProjectWorkspace workspace = Get<ProjectWorkspace>(host);
	//using CancellationTokenSource tokenSource = new();

	//Console.CancelKeyPress += delegate
	//{
	//	tokenSource.Cancel();
	//};

	//var projectAnalyzer = Get<ProjectMetricDataAnalyzer>(host);

	//Matcher matcher = new();
	//matcher.AddIncludePatterns(new[] { "**/*.csproj", "**/*.vbproj" });

	//Dictionary<string, CodeAnalysisMetricData> metricData = new(StringComparer.OrdinalIgnoreCase);
	//var projects = matcher.GetResultsInFullPath(inputs.Directory);

	//foreach (var project in projects)
	//{
	//	var metrics =
	//		await projectAnalyzer.AnalyzeAsync(
	//			workspace, project, tokenSource.Token);

	//	foreach (var (path, metric) in metrics)
	//	{
	//		metricData[path] = metric;
	//	}
	//}

	//var updatedMetrics = false;
	//var title = "";
	//StringBuilder summary = new();
	//if (metricData is { Count: > 0 })
	//{
	//	var fileName = "CODE_METRICS.md";
	//	var fullPath = Path.Combine(inputs.Directory, fileName);
	//	var logger = Get<ILoggerFactory>(host).CreateLogger(nameof(StartAnalysisAsync));
	//	var fileExists = File.Exists(fullPath);

	//	logger.LogInformation(
	//		$"{(fileExists ? "Updating" : "Creating")} {fileName} markdown file with latest code metric data.");

	//	summary.AppendLine(
	//		title = $"{(fileExists ? "Updated" : "Created")} {fileName} file, analyzed metrics for {metricData.Count} projects.");

	//	foreach (var (path, _) in metricData)
	//	{
	//		summary.AppendLine($"- *{path}*");
	//	}

	//	var contents = metricData.ToMarkDownBody(inputs);
	//	await File.WriteAllTextAsync(
	//		fullPath,
	//		contents,
	//		tokenSource.Token);

	//	updatedMetrics = true;
	//}
	//else
	//{
	//	summary.Append("No metrics were determined.");
	//}

	//// https://docs.github.com/actions/reference/workflow-commands-for-github-actions#setting-an-output-parameter
	//Console.WriteLine($"::set-output name=updated-metrics::{updatedMetrics}");
	//Console.WriteLine($"::set-output name=summary-title::{title}");
	//Console.WriteLine($"::set-output name=summary-details::{summary}");

	Environment.Exit(0);
}

var logger = new ConsoleLogger(LogLevel.Debug);
var parser = Default.ParseArguments(() => new ActionInputs(), args);
parser.WithNotParsed(
	errors =>
	{
		logger.LogError(string.Join(IToDoFileContents.UNIX_LINE_ENDING, errors.Select(error => error.ToString()))); ;
		Environment.Exit(2);
	});

var actionInputs = parser.Value;
var actionInputsLogger = Get<ILogger<ActionInputs>>(host);
actionInputs.LogInputs(actionInputsLogger);
await StartToDoIssueProcessAsync(actionInputs, host);
await host.RunAsync();