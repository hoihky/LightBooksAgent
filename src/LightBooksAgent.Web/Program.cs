using LightBooksAgent.Agents;
using LightBooksAgent.Application;
using LightBooksAgent.Infrastructure;
using LightBooksAgent.Workflows;
using LightBooksAgent.Infrastructure.Persistence;
using LightBooksAgent.Web.Components;
using LightBooksAgent.Web.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSignalR();

builder.Services.AddLightBooksApplication(builder.Configuration);
builder.Services.AddLightBooksInfrastructure(builder.Configuration);
builder.Services.AddLightBooksAgents();
builder.Services.AddLightBooksWorkflows();

var app = builder.Build();

await DbInitializer.InitializeAsync(app.Services);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapHub<ActivityHub>("/hubs/activity");
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
