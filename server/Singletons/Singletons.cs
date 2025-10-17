using BombermanGame.Models;
using System.Collections.Concurrent;

namespace BombermanGame.Singletons;


public sealed class GameConfiguration
{
    private static readonly Lazy<GameConfiguration> _instance =
        new Lazy<GameConfiguration>(() => new GameConfiguration());

    public static GameConfiguration Instance => _instance.Value;

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

    private GameConfiguration()
    {
        LoadConfiguration();
    }

    private void LoadConfiguration()
    {
        Console.WriteLine("[Singleton] GameConfiguration initialized with default settings");
    }

    public void UpdateMaxPlayers(int maxPlayers)
    {
        if (maxPlayers < 2 || maxPlayers > 8)
            throw new ArgumentException("Max players must be between 2 and 8");

        MaxPlayersPerRoom = maxPlayers;
        Console.WriteLine($"[Singleton] Max players updated to {maxPlayers}");
    }

    public void UpdateBombExplosionTime(int seconds)
    {
        if (seconds < 1 || seconds > 10)
            throw new ArgumentException("Bomb explosion time must be between 1 and 10 seconds");

        BombExplosionTimeSeconds = seconds;
        Console.WriteLine($"[Singleton] Bomb explosion time updated to {seconds}s");
    }

    public void UpdatePowerUpDropChance(double chance)
    {
        if (chance < 0 || chance > 1)
            throw new ArgumentException("Drop chance must be between 0 and 1");

        PowerUpDropChance = chance;
        Console.WriteLine($"[Singleton] Power-up drop chance updated to {chance:P0}");
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

        Console.WriteLine("[Singleton] Configuration reset to defaults");
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

    public void PrintConfiguration()
    {
        Console.WriteLine("\n=== Game Configuration (Singleton) ===");
        Console.WriteLine($"Max Players: {MaxPlayersPerRoom}");
        Console.WriteLine($"Bomb Explosion Time: {BombExplosionTimeSeconds}s");
        Console.WriteLine($"Power-up Drop Chance: {PowerUpDropChance:P0}");
        Console.WriteLine($"Board Size: {BoardWidth}x{BoardHeight}");
        Console.WriteLine($"Default Bomb Count: {DefaultBombCount}");
        Console.WriteLine($"Default Bomb Range: {DefaultBombRange}");
        Console.WriteLine($"Game Update Interval: {GameUpdateIntervalMs}ms");
        Console.WriteLine("=====================================\n");
    }
}

public sealed class GameStatistics
{
    private static readonly Lazy<GameStatistics> _instance =
        new Lazy<GameStatistics>(() => new GameStatistics());

    public static GameStatistics Instance => _instance.Value;

    private readonly ConcurrentDictionary<string, int> _playerWins = new();
    private readonly ConcurrentDictionary<string, int> _playerGamesPlayed = new();
    private readonly ConcurrentDictionary<string, int> _playerBombsPlaced = new();
    private readonly ConcurrentDictionary<string, int> _playerKills = new();

    private int _totalGames = 0;
    private int _totalBombsExploded = 0;
    private DateTime _startTime = DateTime.Now;

    private GameStatistics()
    {
        Console.WriteLine("[Singleton] GameStatistics initialized");
    }

    public void RecordGamePlayed(string playerId)
    {
        _playerGamesPlayed.AddOrUpdate(playerId, 1, (key, value) => value + 1);
        Interlocked.Increment(ref _totalGames);
    }

    public void RecordWin(string playerId)
    {
        _playerWins.AddOrUpdate(playerId, 1, (key, value) => value + 1);
    }

    public void RecordBombPlaced(string playerId)
    {
        _playerBombsPlaced.AddOrUpdate(playerId, 1, (key, value) => value + 1);
    }

    public void RecordBombExploded()
    {
        Interlocked.Increment(ref _totalBombsExploded);
    }

    public void RecordKill(string killerId)
    {
        _playerKills.AddOrUpdate(killerId, 1, (key, value) => value + 1);
    }

    public int GetPlayerWins(string playerId)
    {
        return _playerWins.TryGetValue(playerId, out var wins) ? wins : 0;
    }

    public int GetPlayerGamesPlayed(string playerId)
    {
        return _playerGamesPlayed.TryGetValue(playerId, out var games) ? games : 0;
    }

    public double GetPlayerWinRate(string playerId)
    {
        var games = GetPlayerGamesPlayed(playerId);
        if (games == 0) return 0;

        var wins = GetPlayerWins(playerId);
        return (double)wins / games;
    }

    public int GetTotalGames() => _totalGames;
    public int GetTotalBombsExploded() => _totalBombsExploded;
    public TimeSpan GetUptime() => DateTime.Now - _startTime;

    public void PrintStatistics()
    {
        Console.WriteLine("\n=== Game Statistics (Singleton) ===");
        Console.WriteLine($"Total Games Played: {_totalGames}");
        Console.WriteLine($"Total Bombs Exploded: {_totalBombsExploded}");
        Console.WriteLine($"Server Uptime: {GetUptime():hh\\:mm\\:ss}");
        Console.WriteLine($"Registered Players: {_playerGamesPlayed.Count}");

        if (_playerWins.Any())
        {
            var topPlayer = _playerWins.OrderByDescending(x => x.Value).First();
            Console.WriteLine($"Top Player: {topPlayer.Key} with {topPlayer.Value} wins");
        }

        Console.WriteLine("===================================\n");
    }

    public void Reset()
    {
        _playerWins.Clear();
        _playerGamesPlayed.Clear();
        _playerBombsPlaced.Clear();
        _playerKills.Clear();
        _totalGames = 0;
        _totalBombsExploded = 0;
        _startTime = DateTime.Now;

        Console.WriteLine("[Singleton] Statistics reset");
    }
}

public sealed class GameLogger
{
    private static readonly Lazy<GameLogger> _instance =
        new Lazy<GameLogger>(() => new GameLogger());

    public static GameLogger Instance => _instance.Value;

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

    private GameLogger()
    {
        Console.WriteLine("[Singleton] GameLogger initialized");
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
        Console.WriteLine("[Singleton] Logs cleared");
    }
}