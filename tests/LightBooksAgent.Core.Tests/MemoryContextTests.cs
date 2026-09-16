using LightBooksAgent.Core.Models;

namespace LightBooksAgent.Core.Tests;

public class MemoryContextTests
{
    [Fact]
    public void FormattedBlock_IsEmpty_WhenNoLessons()
    {
        var context = new MemoryContext();
        Assert.Equal(string.Empty, context.FormattedBlock);
    }

    [Fact]
    public void FormattedBlock_IncludesLessons()
    {
        var context = new MemoryContext
        {
            Lessons = ["Use step-by-step code examples", "Include comparison tables"]
        };

        Assert.Contains("step-by-step", context.FormattedBlock);
        Assert.Contains("comparison tables", context.FormattedBlock);
    }
}
