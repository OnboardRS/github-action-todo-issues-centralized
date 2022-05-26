using System.Diagnostics;

namespace OnboardRS.ToDoIssues.Business.Utilities.Extensions;

public static class ShellHelper
{
	public static async Task<string> RunBashCommandAsync(this string cmd, ILogger logger)
	{
		var escapedArgs = cmd.Replace("\"", "\\\"");
		var process = new Process
		{
			StartInfo = new ProcessStartInfo
			{
				FileName = "bash",
				Arguments = $"-c \"{escapedArgs}\"",
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true
			},
			EnableRaisingEvents = true
		};

		try
		{
			logger.LogInformation($"{nameof(RunBashCommandAsync)}: Running command [{cmd}]");
			process.Start();
			var output = await process.StandardOutput.ReadToEndAsync();
			var errorOutput = await process.StandardError.ReadToEndAsync();
			logger.LogInformation($"{nameof(RunBashCommandAsync)} Standard Output:\n{output}");
			if (!string.IsNullOrWhiteSpace(errorOutput))
			{
				logger.LogInformation($"{nameof(RunBashCommandAsync)} Error Output:\n{errorOutput}");
			}
			await process.WaitForExitAsync();

			return output;
		}
		catch (Exception e)
		{
			logger.LogError(e, $"{nameof(RunBashCommandAsync)}: Command [{cmd}] failed");
			var errorOutput = await process.StandardError.ReadToEndAsync();
			throw new ApplicationException($"{nameof(RunBashCommandAsync)} Bash error: {errorOutput}", e);
		}
	}
}