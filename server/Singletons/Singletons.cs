using BombermanGame.Models;
using System.Collections.Concurrent;

namespace BombermanGame.Singletons;

public sealed class GameConfiguration
{
    private static GameConfiguration _instance;
    private static readonly object _lock = new object();

    public static GameConfiguration Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                        _instance = new GameConfiguration();
                }
            }
            return _instance;
        }
    }

    private GameConfiguration() { }

    public int MaxPlayersPerRoom { get; private set; } = 4;
    public int BombExplosionTimeSeconds { get; private set; } = 3;
    public int ExplosionDurationSeconds { get; private set; } = 1;
    public double PowerUpDropChance { get; private set; } = 0.3;
    public int DefaultBombCount { get; private set; } = 1;
    public int DefaultBombRange { get; private set; } = 2;
    public int DefaultMovementSpeed { get; private set; } = 1;
    public int GameUpdateIntervalMs { get; private set; } = 100;

    public int BoardWidth { get; private set; } = 15;
    public int BoardHeight { get; private set; } = 13;
    public double DestructibleWallChance { get; private set; } = 0.6;

    private readonly string[] _playerColors = { "#ff0000", "#0000ff", "#00ff00", "#ffff00" };
    public string[] PlayerColors => _playerColors;

    private readonly (int X, int Y)[] _spawnPositions = { (1, 1), (13, 1), (1, 11), (13, 11) };
    public (int X, int Y)[] SpawnPositions => _spawnPositions;

    public void UpdateMaxPlayers(int maxPlayers)
    {
        if (maxPlayers < 2 || maxPlayers > 8)
            throw new ArgumentException("Max players must be between 2 and 8");

        MaxPlayersPerRoom = maxPlayers;
        Console.WriteLine($" Max players updated to {maxPlayers}");
    }

    public void UpdateBombExplosionTime(int seconds)
    {
        if (seconds < 1 || seconds > 10)
            throw new ArgumentException("Bomb explosion time must be between 1 and 10 seconds");

        BombExplosionTimeSeconds = seconds;
        Console.WriteLine($" Bomb explosion time updated to {seconds}s");
    }

    public void UpdatePowerUpDropChance(double chance)
    {
        if (chance < 0 || chance > 1)
            throw new ArgumentException("Drop chance must be between 0 and 1");

        PowerUpDropChance = chance;
        Console.WriteLine($" Power-up drop chance updated to {chance:P0}");
    }

    public void ResetToDefaults()
    {
        MaxPlayersPerRoom = 4;
        BombExplosionTimeSeconds = 3;
        ExplosionDurationSeconds = 1;
        PowerUpDropChance = 0.3;
        DefaultBombCount = 1;
        DefaultBombRange = 2;
        DefaultMovementSpeed = 1;
        GameUpdateIntervalMs = 100;
        BoardWidth = 15;
        BoardHeight = 13;
        DestructibleWallChance = 0.6;

        Console.WriteLine("Configuration reset to defaults");
    }

    public string GetPlayerColor(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= _playerColors.Length)
            return "#ffffff";

        return _playerColors[playerIndex];
    }

    public (int X, int Y) GetSpawnPosition(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= _spawnPositions.Length)
            return (1, 1);

        return _spawnPositions[playerIndex];
    }

}

public sealed class GameLogger
{

    private static GameLogger _instance;
    private static readonly object _lock = new();

    public static GameLogger Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                        _instance = new GameLogger();
                }
            }
            return _instance;
        }
    }

    private GameLogger() { }

    private readonly ConcurrentQueue<LogEntry> _logQueue = new();
    private readonly int _maxLogEntries = 1000;

    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }

    private class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public LogLevel Level { get; set; }
        public string Category { get; set; } = "";
        public string Message { get; set; } = "";
    }
    public void LogDebug(string category, string message)
    {
        Log(LogLevel.Debug, category, message);
    }

    public void LogInfo(string category, string message)
    {
        Log(LogLevel.Info, category, message);
    }

    public void LogWarning(string category, string message)
    {
        Log(LogLevel.Warning, category, message);
    }

    public void LogError(string category, string message)
    {
        Log(LogLevel.Error, category, message);
    }

    private void Log(LogLevel level, string category, string message)
    {
        var entry = new LogEntry
        {
            Timestamp = DateTime.Now,
            Level = level,
            Category = category,
            Message = message
        };

        _logQueue.Enqueue(entry);

        while (_logQueue.Count > _maxLogEntries)
        {
            _logQueue.TryDequeue(out _);
        }

        var color = level switch
        {
            LogLevel.Error => ConsoleColor.Red,
            LogLevel.Warning => ConsoleColor.Yellow,
            LogLevel.Info => ConsoleColor.Green,
            _ => ConsoleColor.Gray
        };

        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine($"[{entry.Timestamp:HH:mm:ss}] [{level}] [{category}] {message}");
        Console.ForegroundColor = originalColor;
    }

    public List<string> GetRecentLogs(int count = 10)
    {
        return _logQueue
            .TakeLast(count)
            .Select(e => $"[{e.Timestamp:HH:mm:ss}] [{e.Level}] [{e.Category}] {e.Message}")
            .ToList();
    }

    public void ClearLogs()
    {
        _logQueue.Clear();
        Console.WriteLine("Logs cleared");
    }
}