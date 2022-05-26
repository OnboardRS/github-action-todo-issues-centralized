﻿using System.Diagnostics;

namespace OnboardRS.ToDoIssues.Business.Utilities.Extensions;

public static class ShellHelper
{
	public static async Task<string> RunBashCommandAsync (this string cmd, ILogger logger)
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
			process.Start();
			var output = await process.StandardOutput.ReadToEndAsync();
			await process.WaitForExitAsync();
			return output;
		}
		catch (Exception e)
		{
			logger.LogError(e, "Command {} failed", cmd);
			var errorOutput = await process.StandardError.ReadToEndAsync();
			throw new ApplicationException($"Bash error: {errorOutput}", e);
		}
	}
}