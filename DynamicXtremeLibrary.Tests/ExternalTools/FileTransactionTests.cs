using DynamicXtremeLibrary.ExternalTools;

namespace DynamicXtremeLibrary.Tests.ExternalTools;

public sealed class FileTransactionTests
{
    [Fact]
    public void DisposeWithoutCommit_RestoresModifiedFileAndRemovesCreatedFileAndDirectories()
    {
        using TestDirectory temp = new();
        string existing = temp.CreateFile("existing.txt", "original");
        string created = temp.GetPath(Path.Combine("new", "nested", "created.txt"));

        using (FileTransaction transaction = new())
        {
            transaction.WriteAllText(existing, "changed");
            transaction.WriteAllText(created, "created");
            Assert.Equal("changed", File.ReadAllText(existing));
            Assert.True(File.Exists(created));
        }

        Assert.Equal("original", File.ReadAllText(existing));
        Assert.False(File.Exists(created));
        Assert.False(Directory.Exists(temp.GetPath("new")));
    }

    [Fact]
    public void Commit_PreservesAllChanges()
    {
        using TestDirectory temp = new();
        string path = temp.GetPath("committed.txt");

        using (FileTransaction transaction = new())
        {
            transaction.WriteAllText(path, "value");
            transaction.Commit();
        }

        Assert.Equal("value", File.ReadAllText(path));
    }

    [Fact]
    public void MultipleWrites_RollBackToStateBeforeFirstWrite()
    {
        using TestDirectory temp = new();
        string path = temp.CreateFile("data.bin", [1, 2]);

        using (FileTransaction transaction = new())
        {
            transaction.WriteAllBytes(path, [3]);
            transaction.WriteAllBytes(path, [4, 5]);
        }

        Assert.Equal([1, 2], File.ReadAllBytes(path));
    }

    [Fact]
    public void WriteAllBytes_CreatesBackupFromOriginalAndRollbackRestoresBackupState()
    {
        using TestDirectory temp = new();
        string path = temp.CreateFile("game.smc", [1, 2, 3]);
        string backup = temp.CreateFile("game.bak", [9]);

        using (FileTransaction transaction = new())
        {
            transaction.WriteAllBytes(path, [4, 5], backup);
            Assert.Equal([1, 2, 3], File.ReadAllBytes(backup));
        }

        Assert.Equal([1, 2, 3], File.ReadAllBytes(path));
        Assert.Equal([9], File.ReadAllBytes(backup));
    }

    [Fact]
    public void CopyFile_OverwritesDestinationAndRollsItBack()
    {
        using TestDirectory temp = new();
        string source = temp.CreateFile("source.txt", "new");
        string destination = temp.CreateFile("destination.txt", "old");

        using (FileTransaction transaction = new())
        {
            transaction.CopyFile(source, destination);
            Assert.Equal("new", File.ReadAllText(destination));
        }

        Assert.Equal("old", File.ReadAllText(destination));
    }

    [Fact]
    public void CopyFile_RejectsMissingSource()
    {
        using TestDirectory temp = new();
        using FileTransaction transaction = new();

        Assert.Throws<FileNotFoundException>(() => transaction.CopyFile(
            temp.GetPath("missing"), temp.GetPath("destination")));
    }

    [Fact]
    public void Rollback_IsIdempotent()
    {
        using TestDirectory temp = new();
        string path = temp.GetPath("new.txt");
        using FileTransaction transaction = new();
        transaction.WriteAllText(path, "value");

        transaction.Rollback();
        transaction.Rollback();

        Assert.False(File.Exists(path));
    }
}
