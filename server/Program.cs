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

builder.Services.AddSingleton<IGameService, GameService>();
builder.Services.AddSingleton<IGameFactory, GameFactory>();
builder.Services.AddSingleton<ICommandHandler, GameCommandHandler>();
builder.Services.AddSingleton<IEventPublisher, EventPublisher>();

builder.Services.AddSingleton<IGameElementFactory, StandardGameElementFactory>();

builder.Services.AddSingleton<IPlayerBuilder, PlayerBuilder>();
builder.Services.AddSingleton<IGameRoomBuilder, GameRoomBuilder>();

builder.Services.AddSingleton<PrototypeManager>();

builder.Services.AddSingleton<IGameRoomRepository, InMemoryGameRoomRepository>();
builder.Services.AddSingleton<IGameDataService, GameRoomAdapter>();

builder.Services.AddSingleton<IGameFacade, GameFacade>();

builder.Services.AddSingleton<IGameRenderer, JsonGameRenderer>();

var app = builder.Build();

app.UseCors("AllowReactApp");
app.UseRouting();
app.MapControllers();
app.MapHub<GameHub>("/gamehub");

app.Run();