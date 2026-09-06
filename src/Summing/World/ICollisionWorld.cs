using Microsoft.Xna.Framework;
using Summing.Core;

namespace Summing.World;

public interface ICollisionWorld
{
    bool OverlapsSolid(Aabb bounds);
    bool IsGrappleCompatible(Vector2 worldPosition);
    bool IsClimbable(Vector2 worldPosition);
    float FrictionAt(Vector2 worldPosition);
    bool RaycastGrapple(Vector2 origin, Vector2 direction, float maximumDistance, out Vector2 hit);
}
