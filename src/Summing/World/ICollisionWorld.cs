using Microsoft.Xna.Framework;
using Summing.Core;

namespace Summing.World;

public interface ICollisionWorld
{
    bool OverlapsSolid(Aabb bounds);
    bool IsClimbable(Vector2 worldPosition);
    float FrictionAt(Vector2 worldPosition);
}
