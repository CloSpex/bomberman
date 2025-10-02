using BombermanGame.Commands;
using BombermanGame.Events;
using BombermanGame.Factories;
using BombermanGame.Hubs;
using BombermanGame.Services;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000", "https://localhost:3000")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

// Register services
builder.Services.AddSingleton<IGameService, GameService>();
builder.Services.AddSingleton<IGameFactory, GameFactory>();
builder.Services.AddSingleton<ICommandHandler, GameCommandHandler>();
builder.Services.AddSingleton<IEventPublisher, EventPublisher>();

var app = builder.Build();

app.UseCors("AllowReactApp");
app.UseRouting();
app.MapControllers();
app.MapHub<GameHub>("/gamehub");

app.Run();