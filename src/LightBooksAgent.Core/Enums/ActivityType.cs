namespace LightBooksAgent.Core.Enums;

public enum ActivityType
{
    AgentStarted = 0,
    AgentCompleted = 1,
    AgentError = 2,
    ToolInvoked = 3,
    UrlFetched = 4,
    LlmCall = 5,
    MemoryRetrieved = 6,
    MemoryStored = 7,
    HitlGateReached = 8,
    HitlResolved = 9,
    WorkflowStepChanged = 10,
    Information = 11
}
