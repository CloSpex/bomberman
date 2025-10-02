using BombermanGame.Models;

namespace BombermanGame.PowerUps;

public interface IPowerUpEffect
{
    void ApplyEffect(Player player);
    string GetDescription();
}