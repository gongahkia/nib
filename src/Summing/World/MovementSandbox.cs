using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Summing.Core;

namespace Summing.World;

public sealed class MovementSandbox : ICollisionWorld
{
    private readonly List<Rectangle> _solids =
    [
        new(0, 320, 1200, 80), new(80, 260, 180, 60), new(310, 220, 72, 100),
        new(440, 280, 120, 40), new(590, 190, 48, 130), new(700, 250, 160, 70),
        new(900, 145, 48, 175), new(1010, 230, 150, 90), new(-40, 0, 40, 400), new(1200, 0, 40, 400)
    ];

    public bool OverlapsSolid(Aabb bounds)
    {
        foreach (var solid in _solids)
        {
            if (bounds.Intersects(new Aabb(solid.X, solid.Y, solid.Width, solid.Height))) return true;
        }
        return false;
    }

    public bool IsGrappleCompatible(Vector2 worldPosition)
    {
        foreach (var solid in _solids)
        {
            if (solid.Contains(worldPosition)) return true;
        }
        return false;
    }

    public bool RaycastGrapple(Vector2 origin, Vector2 direction, float maximumDistance, out Vector2 hit)
    {
        for (var distance = 4f; distance <= maximumDistance; distance += 4f)
        {
            var sample = origin + direction * distance;
            if (!IsGrappleCompatible(sample)) continue;
            hit = sample;
            return true;
        }
        hit = default;
        return false;
    }

    public void Draw(SpriteBatch batch, Texture2D pixel)
    {
        foreach (var solid in _solids)
        {
            batch.Draw(pixel, solid, new Color(64, 53, 58));
            batch.Draw(pixel, new Rectangle(solid.X, solid.Y, solid.Width, 3), new Color(126, 91, 73));
        }
    }
}
