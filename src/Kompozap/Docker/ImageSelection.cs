namespace Kompozap.Docker;

internal sealed class ImageSelection
{
    public required string DisplayName { get; init; }
    public required string ImagePath { get; init; }
    public HashSet<string> Services { get; } = new(StringComparer.OrdinalIgnoreCase);
}
