using BombermanGame.Singletons;

namespace BombermanGame.Tests;

[TestFixture]
public class SingletonTests
{
    [Test]
    public void GameConfiguration_ShouldBeThreadSafe()
    {
        const int threadCount = 100;
        var instances = new GameConfiguration[threadCount];
        Parallel.For(0, threadCount, i => { instances[i] = GameConfiguration.Instance; });
        var distinct = instances.Distinct().Count();
        Assert.That(1, Is.EqualTo(distinct), "Multiple instances were created!");
    }

    [Test]
    public void GameLogger_ShouldBeThreadSafe()
    {
        const int threadCount = 100;
        var instances = new GameLogger[threadCount];
        Parallel.For(0, threadCount, i => { instances[i] = GameLogger.Instance; });
        var distinct = instances.Distinct().Count();
        Assert.That(1, Is.EqualTo(distinct), "Multiple instances were created!");
    }
}



