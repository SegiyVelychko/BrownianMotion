using BrownianMotion.Simulation.Contracts;
using BrownianMotion.Simulation.Demo;
using BrownianMotion.Simulation.Engine;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddTransient<IParticleFactory, ParticleFactory>();
builder.Services.AddTransient<IDeadlockDemo,    DeadlockDemo>();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
