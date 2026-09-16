namespace LightBooksAgent.Core.Enums;

public enum AgentRunStatus
{
    Idle = 0,
    Running = 1,
    Paused = 2,
    Stopped = 3,
    WaitingForHuman = 4,
    Completed = 5,
    Error = 6
}
