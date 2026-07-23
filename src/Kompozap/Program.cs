using System.Reflection;
using Kompozap;
using Kompozap.Docker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Spectre.Console;
using YamlDotNet.RepresentationModel;

AppSettings appSettings = new();

try
{
    var builder = Host.CreateApplicationBuilder(args);

    builder.Services.AddOptions<AppSettings>()
                    .Bind(builder.Configuration)
                    .ValidateDataAnnotations()
                    .ValidateOnStart();

    var app = builder.Build();
    appSettings = app.Services.GetRequiredService<IOptions<AppSettings>>().Value;
    var figlet = new FigletText("Kompozap").Centered().Color(Theme.Primary);
    var fullVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "Unknown";
    var semVersion = fullVersion.Split('+')[0];
    var versionText = new Markup($"[dim]Version [bold {Theme.Primary}]{semVersion}[/][/]\n").Centered();
    AnsiConsole.Write(figlet);
    AnsiConsole.Write(versionText);
    AnsiConsole.Record();
    if (!string.IsNullOrWhiteSpace(appSettings.WorkingDirectory))
    {
        Directory.SetCurrentDirectory(appSettings.WorkingDirectory);
        AnsiConsole.MarkupLineInterpolated($"Working directory set to: {appSettings.WorkingDirectory}");
    }
    using var composeStream = new StreamReader(appSettings.Docker.ComposePath);
    var yaml = new YamlStream();
    yaml.Load(composeStream);
    AnsiConsole.MarkupLineInterpolated($"Successfully loaded: {appSettings.Docker.ComposePath}");
    var root = (YamlMappingNode)yaml.Documents[0].RootNode;
    var services = ((YamlMappingNode)root["services"]).Children;

    var imageTree = new ImageTree();
    var composeServices = services.Parse(appSettings);
    foreach (var service in composeServices)
    {
        imageTree.AddService(service);
    }

    var prompt = imageTree.BuildPrompt(appSettings.MaxPageSize);
    prompt.HighlightStyle(Theme.Primary);
    AnsiConsole.MarkupLine($"\n[bold {Theme.Primary}]Kompozap[/] is ready!");
    var selected = await AnsiConsole.PromptAsync(prompt);

    var selectedServices = selected.SelectMany(x => x.Services)
                                   .Distinct(StringComparer.OrdinalIgnoreCase)
                                   .ToArray();

    var selectedImages = selected.Select(x => x.ImagePath).ToHashSet(StringComparer.OrdinalIgnoreCase);

    string? generatedTag = null;
    if (appSettings.Docker.ImageTagSettings.Generate)
    {
        var tagSettings = appSettings.Docker.ImageTagSettings;

        generatedTag = DateTime.Now.ToString(tagSettings.ImageTagFormat);
        appSettings.Docker.EnvironmentVariables[tagSettings.EnvironmentVariableName] = generatedTag;
    }

    if (await ComposeRunner.Build(appSettings, selectedServices)
        && await ComposeRunner.Push(appSettings, selectedServices))
    {
        if (generatedTag is not null)
        {
            AnsiConsole.MarkupLineInterpolated($"[{Theme.Success}]Published images[/] with tag [bold ${Theme.Primary}]{generatedTag}[/].");
            var tagReplacementComposeFilePath = appSettings.Docker.ImageTagSettings.TagReplacementComposeFilePath;
            ComposeImageTagUpdater.UpdateTags(tagReplacementComposeFilePath, selectedImages, generatedTag);
            AnsiConsole.MarkupLineInterpolated($"[{Theme.Success}]Updated image tags[/] in [${Theme.Primary}]{Markup.Escape(tagReplacementComposeFilePath)}[/].");
        }
        AnsiConsole.MarkupLine($"[${Theme.Primary}]Done![/]");
    }
    else
    {
        await ExportLog(appSettings.LogsDirectory);
    }
}
catch (Exception ex)
{
    AnsiConsole.MarkupLine($"[{Theme.Error}]Kompozap encountered an unexpected error.[/]");
    AnsiConsole.WriteException(ex);
    await ExportLog(appSettings.LogsDirectory);
}

if (!appSettings.AutoClose && Environment.UserInteractive && !Console.IsOutputRedirected && !Console.IsInputRedirected)
{
    AnsiConsole.Markup("[dim]Press any key to exit...[/]");
    Console.ReadKey(intercept: true);
}

static async Task ExportLog(string directory)
{
    try
    {
        var logContent = AnsiConsole.ExportText();
        var logDirectory = Directory.CreateDirectory(directory);
        var logFilePath = Path.Combine(logDirectory.FullName, $"{DateTime.Now:yyyyMMddHHmmss}.txt");

        await File.WriteAllTextAsync(logFilePath, logContent);
        AnsiConsole.MarkupLineInterpolated($"[dim]Logs exported to:[/] [white link]{Markup.Escape(logFilePath)}[/]");
    }
    catch (Exception ex)
    {
        AnsiConsole.MarkupLine($"[${Theme.Error}]Failed to export error log.[/]");
        AnsiConsole.WriteException(ex);
    }
    finally
    {
        Environment.ExitCode = 1;
    }
}
