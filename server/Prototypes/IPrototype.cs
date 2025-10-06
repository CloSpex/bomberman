using BombermanGame.Models;
using BombermanGame.Strategies;

namespace BombermanGame.Prototypes;

public interface IPrototype<T>
{
    T Clone();
}
