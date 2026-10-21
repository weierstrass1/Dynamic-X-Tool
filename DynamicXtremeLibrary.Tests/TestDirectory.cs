namespace DynamicXtremeLibrary.Tests;

internal sealed class TestDirectory : IDisposable
{
    public string Path { get; } = System.IO.Path.Combine(
        System.IO.Path.GetTempPath(),
        "DynamicXtremeTests",
        Guid.NewGuid().ToString("N"));

    public TestDirectory() => Directory.CreateDirectory(Path);

    public string CreateFile(string relativePath, string content)
    {
        string path = GetPath(relativePath);
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
        return path;
    }

    public string CreateFile(string relativePath, byte[] content)
    {
        string path = GetPath(relativePath);
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);
        File.WriteAllBytes(path, content);
        return path;
    }

    public string GetPath(string relativePath) => System.IO.Path.Combine(Path, relativePath);

    public void Dispose()
    {
        if (Directory.Exists(Path))
            Directory.Delete(Path, recursive: true);
    }
}

internal sealed class CurrentDirectoryScope : IDisposable
{
    private readonly string _original = Environment.CurrentDirectory;

    public CurrentDirectoryScope(string path) => Environment.CurrentDirectory = path;

    public void Dispose() => Environment.CurrentDirectory = _original;
}
