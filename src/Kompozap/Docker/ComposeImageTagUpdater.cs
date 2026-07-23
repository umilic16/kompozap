namespace Kompozap.Docker;

internal static class ComposeImageTagUpdater
{
    private const string ImagePrefix = "image:";
    internal static void UpdateTags(string composeFilePath, HashSet<string> selectedImages, string tag)
    {
        var lines = File.ReadAllLines(composeFilePath);

        for (var i = 0; i < lines.Length; i++)
        {
            var imageIndex = lines[i].IndexOf(ImagePrefix, StringComparison.Ordinal);

            if (imageIndex == -1)
                continue;

            var imageStartIndex = imageIndex + ImagePrefix.Length;

            while (imageStartIndex < lines[i].Length && lines[i][imageStartIndex] == ' ')
                imageStartIndex++;

            var imageEndIndex = lines[i].Length;

            for (var j = imageStartIndex; j < lines[i].Length; j++)
            {
                if (char.IsWhiteSpace(lines[i][j]) || lines[i][j] == '#')
                {
                    imageEndIndex = j;
                    break;
                }
            }

            var imageReference = lines[i][imageStartIndex..imageEndIndex];
            var currentImage = ComposeServiceParser.GetImagePath(imageReference);

            if (!selectedImages.Contains(currentImage))
                continue;

            var newImage = ReplaceTag(imageReference, tag);

            lines[i] = lines[i][..imageStartIndex] + newImage + lines[i][imageEndIndex..];
        }

        File.WriteAllLines(composeFilePath, lines);
    }


    private static string ReplaceTag(string image, string tag)
    {
        var lastSlash = image.LastIndexOf('/');
        var lastColon = image.LastIndexOf(':');

        if (lastColon < lastSlash)
            return $"{image}:{tag}";

        return $"{image[..lastColon]}:{tag}";
    }
}