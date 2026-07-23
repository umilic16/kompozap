using Spectre.Console;

namespace Kompozap.Docker;

internal sealed class ImageTree
{
    public Dictionary<string, ImagePathGroup> Groups { get; } = new(StringComparer.OrdinalIgnoreCase);

    public HashSet<ImageSelection> RootImages { get; } = [];

    public void AddService(ComposeService service)
    {
        var segments = service.ImagePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length == 1)
        {
            var existing = RootImages.FirstOrDefault(x => x.ImagePath == service.ImagePath);

            if (existing is null)
            {
                existing = new ImageSelection
                {
                    DisplayName = segments[0],
                    ImagePath = service.ImagePath
                };

                RootImages.Add(existing);
            }

            existing.Services.Add(service.Name);
            return;
        }

        var imageName = segments[^1];
        var path = segments[..^1];

        var current = Groups;

        ImagePathGroup? group = null;

        foreach (var segment in path)
        {
            if (!current.TryGetValue(segment, out group))
            {
                group = new ImagePathGroup(segment);
                current[segment] = group;
            }

            current = group.SubGroups;
        }

        group!.AddImage(imageName, service);
    }

    public MultiSelectionPrompt<ImageSelection> BuildPrompt(int? pageSize)
    {
        var prompt = new MultiSelectionPrompt<ImageSelection>()
            .Title("Select services:")
            .WrapAround()
            .UseConverter(x => x.DisplayName);

        int total = 0;

        foreach (var image in RootImages.OrderBy(x => x.DisplayName))
        {
            prompt.AddChoice(image);
            prompt.Select(image);
            total++;
        }

        foreach (var group in Groups.Values.OrderBy(x => x.Name))
        {
            total += group.AddToPrompt(prompt, null);
        }

        prompt.PageSize(Math.Max(3, Math.Min(pageSize ?? total, total)));

        return prompt;
    }
}
