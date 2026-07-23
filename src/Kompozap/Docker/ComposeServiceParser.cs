using YamlDotNet.Helpers;
using YamlDotNet.RepresentationModel;

namespace Kompozap.Docker;

internal static partial class ComposeServiceParser
{
    internal static List<ComposeService> Parse(this IOrderedDictionary<YamlNode, YamlNode> services, AppSettings appSettings)
    {
        var ignoredServices = appSettings.Docker.Services.Where(x => x.Ignore)
                                                         .Select(x => x.Name)
                                                         .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var composeServices = new List<ComposeService>();

        foreach (var service in services)
        {
            var serviceName = ((YamlScalarNode)service.Key).Value;

            if (serviceName is null || ignoredServices.Contains(serviceName))
                continue;

            var serviceDetails = (YamlMappingNode)service.Value;

            if (!serviceDetails.Children.TryGetValue(new YamlScalarNode("image"), out var imageNode))
                continue;

            var image = ((YamlScalarNode)imageNode).Value;

            if (string.IsNullOrWhiteSpace(image))
                continue;

            composeServices.Add(new ComposeService
            {
                Name = serviceName,
                ImagePath = GetImagePath(image)
            });
        }

        return composeServices;
    }

    internal static string GetImagePath(string? rawImage)
    {
        if (string.IsNullOrWhiteSpace(rawImage))
            return string.Empty;

        var image = rawImage.Trim();

        // Remove compose variable prefix
        var variableEnd = image.IndexOf('}');
        if (image.StartsWith("${", StringComparison.Ordinal) && variableEnd >= 0)
        {
            image = image[(variableEnd + 1)..];
        }

        // Remove tag
        var lastSlash = image.LastIndexOf('/');
        var lastColon = image.LastIndexOf(':');

        if (lastColon > lastSlash)
        {
            image = image[..lastColon];
        }

        // Remove registry
        var firstSlash = image.IndexOf('/');

        if (firstSlash > 0 &&
            (image[..firstSlash].Contains('.') ||
             image[..firstSlash].Contains(':') ||
             image[..firstSlash] == "localhost"))
        {
            image = image[(firstSlash + 1)..];
        }

        return image;
    }
}
