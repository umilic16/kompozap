using Spectre.Console;

namespace Kompozap.Docker;

internal static class ComposeRunner
{
    public static async Task<bool> Build(AppSettings settings, string[] services)
    {
        var arguments = BuildArguments(settings.Docker.BuildArguments, services);
        return await Run("Building Docker images", arguments, settings);
    }


    public static async Task<bool> Push(AppSettings settings, string[] services)
    {
        var arguments = BuildArguments(settings.Docker.PushArguments, services);
        return await Run("Pushing Docker images", arguments, settings);
    }


    private static string BuildArguments(string baseArguments, string[] services)
    {
        var serviceArguments = string.Join(" ", services.Select(QuoteArgument));

        return string.IsNullOrWhiteSpace(serviceArguments)
            ? baseArguments
            : $"{baseArguments} {serviceArguments}";
    }


    private static async Task<bool> Run(string title, string arguments, AppSettings settings)
    {
        AnsiConsole.Write(new Rule($"[{Theme.Primary}]{title}[/]").RuleStyle(Theme.Primary));

        var exitCode = await CommandHelper.RunStreaming(
            "docker",
            arguments,
            settings.Docker.EnvironmentVariables,
            line =>
            {
                AnsiConsole.MarkupLine(Markup.Escape(line.Trim()));
            });

        var success = exitCode == 0;
        if (success)
        {
            AnsiConsole.MarkupLineInterpolated($"[{Theme.Success}][bold]{title}[/] completed successfully.[/]");
        }
        else
        {
            AnsiConsole.MarkupLineInterpolated($"[{Theme.Error}][bold]{title}[/] failed.[/]");
        }
        return success;
    }


    private static string QuoteArgument(string value)
    {
        if (!value.Contains(' '))
            return value;

        return $"\"{value}\"";
    }
}
