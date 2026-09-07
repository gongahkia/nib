using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Summing.Core;

namespace Summing.Entities;

public sealed class RopeEntity
{
    public RopeEntity(float x, float top, float bottom, float climbTop, float exitX, int ledgeDirection,
        Point anchorTile)
    {
        X = x;
        Top = top;
        Bottom = bottom;
        ClimbTop = climbTop;
        ExitX = exitX;
        LedgeDirection = ledgeDirection;
        AnchorTile = anchorTile;
    }

    public float X { get; }
    public float Top { get; }
    public float Bottom { get; }
    public float ClimbTop { get; }
    public float ExitX { get; }
    public int LedgeDirection { get; }
    public Point AnchorTile { get; }
    public bool IsOverhangAnchor => LedgeDirection == 0;

    public void Draw(SpriteBatch batch, Texture2D pixel)
    {
        batch.Draw(pixel, new Rectangle((int)X - 1, (int)Top, 3, (int)(Bottom - Top)), new Color(160, 126, 85));
        for (var y = (int)Top + 7; y < Bottom; y += 12)
            batch.Draw(pixel, new Rectangle((int)X - 3, y, 7, 2), new Color(203, 165, 105));

        if (!IsOverhangAnchor)
        {
            var anchorEdge = (AnchorTile.X + (LedgeDirection > 0 ? 1 : 0)) * GameConstants.TileSize;
            var left = (int)MathF.Min(anchorEdge, X);
            batch.Draw(pixel, new Rectangle(left, (int)Top, Math.Max(2, (int)MathF.Abs(X - anchorEdge)), 2),
                new Color(203, 165, 105));
        }
        batch.Draw(pixel, new Rectangle((int)X - 3, (int)Top - 2, 7, 5), new Color(226, 188, 121));
    }
}
