using System.Diagnostics;

namespace Kompozap;

internal static class CommandHelper
{
    internal static async Task<int> RunStreaming(string command, string arguments, IDictionary<string, string>? environmentVariables, Action<string> onOutput)
    {
        using var process = new Process();

        process.StartInfo = new ProcessStartInfo
        {
            FileName = command,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        if (environmentVariables is not null)
        {
            foreach (var variable in environmentVariables)
            {
                process.StartInfo.Environment[variable.Key] = variable.Value;
            }
        }

        process.OutputDataReceived += (_, e) =>
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
                onOutput(e.Data);
        };

        process.ErrorDataReceived += (_, e) =>
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
                onOutput(e.Data);
        };

        process.Start();

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();

        return process.ExitCode;
    }
}