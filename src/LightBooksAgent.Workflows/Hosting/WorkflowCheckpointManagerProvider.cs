using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Checkpointing;

namespace LightBooksAgent.Workflows.Hosting;

public sealed class WorkflowCheckpointManagerProvider
{
    public WorkflowCheckpointManagerProvider(string? checkpointDirectory = null)
    {
        var directory = checkpointDirectory
            ?? Path.Combine(AppContext.BaseDirectory, "data", "checkpoints");
        Directory.CreateDirectory(directory);
        Manager = CheckpointManager.CreateJson(new FileSystemJsonCheckpointStore(new DirectoryInfo(directory)));
    }

    public CheckpointManager Manager { get; }
}
