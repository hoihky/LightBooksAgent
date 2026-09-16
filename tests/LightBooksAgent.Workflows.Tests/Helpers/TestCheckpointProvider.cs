using LightBooksAgent.Workflows.Hosting;

namespace LightBooksAgent.Workflows.Tests.Helpers;

internal static class TestCheckpointProvider
{
    public static WorkflowCheckpointManagerProvider Create() =>
        new(Path.Combine(Path.GetTempPath(), "lightbooks-tests", Guid.NewGuid().ToString("N")));
}
