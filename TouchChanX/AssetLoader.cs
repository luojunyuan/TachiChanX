namespace TouchChanX;

internal static class AssetLoader
{
    private const string Prefix = "TouchChanX.Assets";

    private static Stream GetImageStream(string fileName)
    {
        string resourcePath = $"{Prefix}.{fileName}";

        Stream? stream = typeof(AssetLoader).Assembly.GetManifestResourceStream(resourcePath);

        return stream ?? throw new FileNotFoundException(resourcePath);
    }

    public static Stream KleeHires => GetImageStream("klee_hires.png");
}
