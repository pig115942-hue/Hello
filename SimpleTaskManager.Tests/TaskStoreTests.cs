using Xunit;

namespace SimpleTaskManager.Tests;

public class TaskStoreTests
{
    [Fact]
    public void SavesAndLoadsTasks()
    {
        var file = Path.Combine(Path.GetTempPath(), $"tasks-{Guid.NewGuid():N}.json");
        try
        {
            var store = new TaskStore(file);
            var tasks = new List<TaskItem> { new() { Title = "Buy milk" }, new() { Title = "Done", IsCompleted = true } };
            store.Save(tasks);
            var loaded = store.Load();
            Assert.Equal(2, loaded.Count);
            Assert.Equal("Buy milk", loaded[0].Title);
            Assert.True(loaded[1].IsCompleted);
        }
        finally { if (File.Exists(file)) File.Delete(file); }
    }

    [Fact]
    public void MissingFileReturnsEmptyList()
    {
        var file = Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}.json");
        var store = new TaskStore(file);
        Assert.Empty(store.Load());
    }
}
