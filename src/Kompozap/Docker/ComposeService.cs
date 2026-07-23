namespace Kompozap.Docker;

internal sealed class ComposeService
{
    public required string Name { get; init; }
    public required string ImagePath { get; init; }
}