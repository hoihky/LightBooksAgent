using Microsoft.AspNetCore.SignalR;

namespace LightBooksAgent.Web.Hubs;

public sealed class ActivityHub : Hub
{
    public async Task JoinRunGroup(string runId) =>
        await Groups.AddToGroupAsync(Context.ConnectionId, runId);

    public async Task LeaveRunGroup(string runId) =>
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, runId);
}
