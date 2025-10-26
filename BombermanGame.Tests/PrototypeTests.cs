using System.Runtime.CompilerServices;
using BombermanGame.Builders;
using BombermanGame.Models;
using BombermanGame.Prototypes;
using BombermanGame.Strategies;

namespace BombermanGame.Tests
{
    [TestFixture]
    public class PrototypeTests
    {
        private PrototypeManager _prototypeManager;
        private Player _playerTemplate;
        private PlayerBuilder _playerBuilder = new();
        private GameBoard _boardTemplate;

        [SetUp]
        public void Setup()
        {
            _prototypeManager = new PrototypeManager();
            _playerTemplate = _playerBuilder
                .WithId("1")
                .WithName("Alice")
                .WithMovementStrategy(new NormalMovementStrategy())
                .Build();
            _prototypeManager.RegisterPlayerPrototype("hero", _playerTemplate);

            // Board template
            _boardTemplate = new GameBoard();
            _prototypeManager.RegisterBoardPrototype("level1", _boardTemplate);
        }

        [Test]
        public void Player_ShallowAndDeepCopies_HaveExpectedMemory()
        {
            var shallowPlayer = _prototypeManager.CreatePlayerPreview("hero");
            var deepPlayer = _prototypeManager.CreatePlayerFromPrototype("hero");

            // Top-level objects
            Assert.That(RuntimeHelpers.GetHashCode(_playerTemplate),
                Is.Not.EqualTo(RuntimeHelpers.GetHashCode(shallowPlayer)));
            Assert.That(RuntimeHelpers.GetHashCode(_playerTemplate),
                Is.Not.EqualTo(RuntimeHelpers.GetHashCode(deepPlayer)));

            // Movement strategy
            Assert.That(RuntimeHelpers.GetHashCode(_playerTemplate.MovementStrategy),
                Is.EqualTo(RuntimeHelpers.GetHashCode(shallowPlayer.MovementStrategy)));
            Assert.That(RuntimeHelpers.GetHashCode(_playerTemplate.MovementStrategy),
                Is.Not.EqualTo(RuntimeHelpers.GetHashCode(deepPlayer.MovementStrategy)));
        }

        [Test]
        public void GameBoard_ShallowAndDeepCopies_HaveExpectedMemory()
        {
            var shallowBoard = _prototypeManager.CreateBoardSnapshot("level1");
            var deepBoard = _prototypeManager.CreateBoardFromPrototype("level1");

            // Top-level objects
            Assert.That(
                RuntimeHelpers.GetHashCode(_boardTemplate),
                Is.Not.EqualTo(RuntimeHelpers.GetHashCode(shallowBoard))
                );
            Assert.That(
                RuntimeHelpers.GetHashCode(_boardTemplate),
                Is.Not.EqualTo(RuntimeHelpers.GetHashCode(deepBoard))
                );

            // Grid array references
            Assert.That(
                RuntimeHelpers.GetHashCode(_boardTemplate.Grid),
                Is.EqualTo(RuntimeHelpers.GetHashCode(shallowBoard.Grid))
            );
            Assert.That(
                RuntimeHelpers.GetHashCode(_boardTemplate.Grid),
                Is.Not.EqualTo(RuntimeHelpers.GetHashCode(deepBoard.Grid))
            );
        }
    }
}
