using System.ComponentModel.DataAnnotations;

namespace Kompozap;

public class AppSettings
{
    public string? WorkingDirectory { get; init; }
    public DockerSettings Docker { get; init; } = new();
    public int? MaxPageSize { get; init; }
    public string LogsDirectory { get; init; } = ".logs";
    public bool AutoClose { get; init; } = true;
}

public class DockerSettings
{
    public string ComposePath { get; init; } = "docker-compose.yml";
    public ServiceSettings[] Services { get; init; } = [];
    public ImageTagSettings ImageTagSettings { get; init; } = new();
    public Dictionary<string, string> EnvironmentVariables { get; init; } = default!;

    [Required]
    public string BuildArguments { get; init; } = default!;

    [Required]
    public string PushArguments { get; init; } = default!;
}

public class ImageTagSettings
{
    public string EnvironmentVariableName { get; init; } = "IMAGE_TAG";
    public bool Generate { get; init; } = true;
    public string ImageTagFormat { get; init; } = "yyyyMMddHHmmss";
    public string TagReplacementComposeFilePath { get; init; } = default!;
}

public class ServiceSettings
{
    [Required]
    public string Name { get; init; } = default!;
    public bool Ignore { get; init; }
    public ServiceSettings[] Childern { get; init; } = [];
}
