using Spectre.Console;

namespace Kompozap.Docker;

internal sealed class ImagePathGroup(string name)
{
    public string Name { get; } = name;

    public Dictionary<string, ImagePathGroup> SubGroups { get; } = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, ImageSelection> Images { get; } = new(StringComparer.OrdinalIgnoreCase);

    public void AddImage(string imageName, ComposeService service)
    {
        if (!Images.TryGetValue(imageName, out var image))
        {
            // leaf nodes
            image = new ImageSelection
            {
                DisplayName = imageName,
                ImagePath = service.ImagePath
            };

            Images.Add(imageName, image);
        }

        image.Services.Add(service.Name);
    }

    internal int AddToPrompt(MultiSelectionPrompt<ImageSelection> prompt, ISelectionItem<ImageSelection>? parent)
    {
        int total = 1;

        // group nodes
        var groupChoice = new ImageSelection
        {
            DisplayName = Name,
            ImagePath = "" // ImagePath not needed here
        };

        var node = parent is null
            ? prompt.AddChoice(groupChoice)
            : parent.AddChild(groupChoice);

        prompt.Select(groupChoice);

        foreach (var subgroup in SubGroups.Values.OrderBy(x => x.Name))
        {
            total += subgroup.AddToPrompt(prompt, node);
        }

        foreach (var image in Images.Values.OrderBy(x => x.DisplayName))
        {
            node.AddChild(image);
            prompt.Select(image);
            total++;
        }

        return total;
    }
}
