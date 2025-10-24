using BombermanGame.Commands;
using BombermanGame.Events;
using BombermanGame.Factories;
using BombermanGame.Hubs;
using BombermanGame.Services;
using BombermanGame.Builders;
using BombermanGame.Prototypes;
using BombermanGame.Adapters;
using BombermanGame.Facades;
using BombermanGame.Bridges;
using BombermanGame.Decorators;
using BombermanGame.Singletons;
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


var config = GameConfiguration.Instance;
var statistics = GameStatistics.Instance;
var logger = GameLogger.Instance;

config.UpdatePowerUpDropChance(0.3);
logger.LogInfo("Startup", "Game configuration loaded");

builder.Services.AddSingleton<IGameFactory, GameFactory>();

builder.Services.AddSingleton<ICommandHandler, GameCommandHandler>();

builder.Services.AddSingleton<IEventPublisher, EventPublisher>();
builder.Services.AddSingleton<GameEventLogger>();

builder.Services.AddTransient<IPlayerBuilder, PlayerBuilder>();
builder.Services.AddTransient<IGameRoomBuilder, GameRoomBuilder>();

builder.Services.AddSingleton<PrototypeManager>();

builder.Services.AddSingleton<IGameRoomRepository, InMemoryGameRoomRepository>();
builder.Services.AddSingleton<IGameDataService, GameRoomAdapter>();

builder.Services.AddSingleton<IGameFacade, GameFacade>();

builder.Services.AddSingleton<IGameRenderer, JsonGameRenderer>();

builder.Services.AddSingleton<PlayerDecoratorManager>();

builder.Services.AddSingleton<IGameService, GameService>();


var app = builder.Build();

var eventPublisher = app.Services.GetRequiredService<IEventPublisher>();
var eventLogger = app.Services.GetRequiredService<GameEventLogger>();
eventPublisher.Subscribe<PlayerJoinedEvent>(eventLogger);
eventPublisher.Subscribe<GameStartedEvent>(eventLogger);
eventPublisher.Subscribe<BombExplodedEvent>(eventLogger);

app.UseCors("AllowReactApp");
app.UseRouting();
app.MapControllers();
app.MapHub<GameHub>("/gamehub");


logger.LogInfo("Startup", "All systems operational - Server ready!");

app.Run();