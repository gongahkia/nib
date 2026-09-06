using Microsoft.Xna.Framework;

namespace Summing.Entities;

public sealed record Explosion(Vector2 Position, float Radius, int Damage, int DestroyedTiles, long Frame);

public interface IExplosionTarget
{
    bool Alive { get; }
    Vector2 Position { get; }
    void ApplyExplosion(Explosion explosion);
}
