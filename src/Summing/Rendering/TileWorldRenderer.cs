using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Summing.Core;
using Summing.World;
using Summing.World.Materials;

namespace Summing.Rendering;

public sealed class TileWorldRenderer
{
    private readonly Texture2D _pixel;
    public TileWorldRenderer(Texture2D pixel) => _pixel = pixel;

    public void Draw(SpriteBatch batch, TileWorld world, Vector2 cameraPosition)
    {
        var left = Math.Max(0, TileWorld.WorldToTile(cameraPosition.X - GameConstants.VirtualWidth * 0.55f));
        var right = Math.Min(world.Width - 1, TileWorld.WorldToTile(cameraPosition.X + GameConstants.VirtualWidth * 0.55f));
        var top = Math.Max(0, TileWorld.WorldToTile(cameraPosition.Y - GameConstants.VirtualHeight * 0.58f));
        var bottom = Math.Min(world.Height - 1, TileWorld.WorldToTile(cameraPosition.Y + GameConstants.VirtualHeight * 0.58f));

        for (var y = top; y <= bottom; y++)
            for (var x = left; x <= right; x++)
            {
                var tile = world.GetTile(x, y);
                if (!tile.Solid) continue;
                var definition = MaterialCatalog.Get(tile.Material);
                var destination = new Rectangle(x * GameConstants.TileSize, y * GameConstants.TileSize,
                    GameConstants.TileSize, GameConstants.TileSize);
                batch.Draw(_pixel, destination, definition.BaseColor);

                var exposedTop = !world.GetTile(x, y - 1).Solid;
                if (exposedTop) batch.Draw(_pixel, new Rectangle(destination.X, destination.Y, destination.Width, 3), definition.AccentColor);
                if (((x + y) & 1) == 0)
                    batch.Draw(_pixel, new Rectangle(destination.X + 5, destination.Y + 9, 2, 2), definition.AccentColor * 0.55f);
                if (((x * 7 + y * 11) % 5) == 0)
                    batch.Draw(_pixel, new Rectangle(destination.Right - 6, destination.Bottom - 5, 3, 1), definition.AccentColor * 0.42f);
                if (tile.Damage > 0)
                {
                    var crack = Math.Min(12, tile.Damage * 2 + 2);
                    batch.Draw(_pixel, new Rectangle(destination.Center.X, destination.Y + 4, 1, crack), new Color(19, 20, 25));
                    batch.Draw(_pixel, new Rectangle(destination.Center.X - 3, destination.Y + 8, 4, 1), new Color(19, 20, 25));
                }
            }

        foreach (var chunk in world.Chunks.Values)
        {
            if (chunk.Dirty) chunk.MarkClean();
        }
    }
}
