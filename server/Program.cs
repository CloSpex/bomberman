using BombermanGame.Commands;
using BombermanGame.Events;
using BombermanGame.Factories;
using BombermanGame.Hubs;
using BombermanGame.Services;
using BombermanGame.Builders;
using BombermanGame.Prototypes;
using BombermanGame.Bridges;
using BombermanGame.Decorators;
using BombermanGame.Singletons;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

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


var config = GameConfiguration.Instance;
var logger = GameLogger.Instance;

config.UpdatePowerUpDropChance(0.3);
logger.LogInfo("Startup", "Game configuration loaded");


builder.Services.AddSingleton<ICommandHandler, GameCommandHandler>();

builder.Services.AddSingleton<IEventPublisher, EventPublisher>();

builder.Services.AddTransient<IPlayerBuilder, PlayerBuilder>();
builder.Services.AddTransient<IGameRoomBuilder, GameRoomBuilder>();

builder.Services.AddSingleton<PrototypeManager>();

builder.Services.AddSingleton<IGameRenderer, JsonGameRenderer>();

builder.Services.AddSingleton<PlayerDecoratorManager>();

builder.Services.AddSingleton<IGameService, GameService>();


var app = builder.Build();

var eventPublisher = app.Services.GetRequiredService<IEventPublisher>();

app.UseCors("AllowReactApp");
app.UseRouting();
app.MapHub<GameHub>("/gamehub");


logger.LogInfo("Startup", "All systems operational - Server ready!");

app.Run();