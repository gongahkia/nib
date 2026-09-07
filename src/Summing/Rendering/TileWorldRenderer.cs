using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Summing.Core;
using Summing.World;

namespace Summing.Rendering;

public sealed class TileWorldRenderer(Texture2D pixel)
{
    private readonly Texture2D _pixel = pixel;

    public void Draw(SpriteBatch batch, TileWorld world, Vector2 cameraPosition, float worldZoom)
    {
        var visibleWidth = GameConstants.VirtualWidth / worldZoom;
        var visibleHeight = GameConstants.VirtualHeight / worldZoom;
        var left = Math.Max(0, TileWorld.WorldToTile(cameraPosition.X - visibleWidth * 0.55f));
        var right = Math.Min(world.Width - 1, TileWorld.WorldToTile(cameraPosition.X + visibleWidth * 0.55f));
        var top = Math.Max(0, TileWorld.WorldToTile(cameraPosition.Y - visibleHeight * 0.58f));
        var bottom = Math.Min(world.Height - 1, TileWorld.WorldToTile(cameraPosition.Y + visibleHeight * 0.58f));
        var fill = new Color(72, 75, 82);
        var seam = new Color(29, 31, 37);
        var exposed = new Color(151, 156, 164);

        for (var y = top; y <= bottom; y++)
            for (var x = left; x <= right; x++)
            {
                var tile = world.GetTile(x, y);
                if (!tile.Solid) continue;
                var destination = new Rectangle(x * GameConstants.TileSize, y * GameConstants.TileSize,
                    GameConstants.TileSize, GameConstants.TileSize);
                batch.Draw(_pixel, destination, fill);
                batch.Draw(_pixel, new Rectangle(destination.Right - 1, destination.Y, 1, destination.Height), seam);
                batch.Draw(_pixel, new Rectangle(destination.X, destination.Bottom - 1, destination.Width, 1), seam);
                if (!world.GetTile(x, y - 1).Solid)
                    batch.Draw(_pixel, new Rectangle(destination.X, destination.Y, destination.Width, 2), exposed);
                if (tile.Damage > 0)
                {
                    batch.Draw(_pixel, new Rectangle(destination.Center.X, destination.Y + 5, 1, 13), seam);
                    batch.Draw(_pixel, new Rectangle(destination.Center.X - 4, destination.Y + 10, 5, 1), seam);
                }
            }

        foreach (var chunk in world.Chunks.Values)
            if (chunk.Dirty) chunk.MarkClean();
    }
}
