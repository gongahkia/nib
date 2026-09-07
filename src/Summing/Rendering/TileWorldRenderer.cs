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
                DrawMaterialTexture(batch, destination, tile, definition, x, y);
                DrawExposedEdges(batch, world, destination, definition, x, y);
                if (tile.Damage > 0)
                {
                    var progress = tile.Damage / (float)Math.Max(1, definition.Hardness);
                    var crack = Math.Clamp((int)MathF.Round(5f + progress * 13f), 5, 18);
                    var crackColor = Color.Lerp(GamePalette.Void, definition.BaseColor, 0.12f);
                    batch.Draw(_pixel, new Rectangle(destination.Center.X, destination.Y + 3, 1, crack), crackColor);
                    batch.Draw(_pixel, new Rectangle(destination.Center.X - 4, destination.Y + 8, 5, 1), crackColor);
                    if (progress > 0.45f)
                        batch.Draw(_pixel, new Rectangle(destination.Center.X, destination.Y + 13, 6, 1), crackColor);
                }
            }

        foreach (var chunk in world.Chunks.Values)
        {
            if (chunk.Dirty) chunk.MarkClean();
        }
    }

    private void DrawExposedEdges(SpriteBatch batch, TileWorld world, Rectangle destination,
        MaterialDefinition definition, int x, int y)
    {
        if (!world.GetTile(x, y - 1).Solid)
            batch.Draw(_pixel, new Rectangle(destination.X, destination.Y, destination.Width, 3), definition.AccentColor);
        if (!world.GetTile(x - 1, y).Solid)
            batch.Draw(_pixel, new Rectangle(destination.X, destination.Y + 3, 2, destination.Height - 3),
                definition.AccentColor * 0.52f);
        if (!world.GetTile(x + 1, y).Solid)
            batch.Draw(_pixel, new Rectangle(destination.Right - 2, destination.Y + 3, 2, destination.Height - 3),
                Color.Lerp(definition.BaseColor, GamePalette.Void, 0.38f));
        if (!world.GetTile(x, y + 1).Solid)
            batch.Draw(_pixel, new Rectangle(destination.X + 2, destination.Bottom - 2, destination.Width - 4, 2),
                Color.Lerp(definition.BaseColor, GamePalette.Void, 0.3f));
    }

    private void DrawMaterialTexture(SpriteBatch batch, Rectangle destination, TerrainTile tile,
        MaterialDefinition definition, int x, int y)
    {
        var hash = unchecked((uint)(x * 73856093 ^ y * 19349663));
        var dark = Color.Lerp(definition.BaseColor, GamePalette.Void, 0.34f);
        switch (tile.Material)
        {
            case MaterialId.Loess:
                batch.Draw(_pixel, new Rectangle(destination.X + 3, destination.Y + 7 + (int)(hash % 5), 8, 1),
                    definition.AccentColor * 0.32f);
                batch.Draw(_pixel, new Rectangle(destination.Right - 7, destination.Bottom - 7, 2, 2),
                    definition.AccentColor * 0.52f);
                break;
            case MaterialId.RedSandstone:
                batch.Draw(_pixel, new Rectangle(destination.X + 2, destination.Y + 8, 13 + (int)(hash % 7), 1),
                    definition.AccentColor * 0.44f);
                batch.Draw(_pixel, new Rectangle(destination.X + 8, destination.Y + 17, 14, 1), dark);
                break;
            case MaterialId.BlackBasalt:
                batch.Draw(_pixel, new Rectangle(destination.X + 6 + (int)(hash % 8), destination.Y + 5, 2, 12), dark);
                batch.Draw(_pixel, new Rectangle(destination.X + 4, destination.Y + 14, 7, 2),
                    definition.AccentColor * 0.34f);
                break;
            case MaterialId.FossilComposite:
                var fossilX = 4 + (int)(hash % 5);
                var fossilY = 7 + (int)((hash >> 4) % 7);
                batch.Draw(_pixel, new Rectangle(destination.X + fossilX, destination.Y + fossilY, 11, 2),
                    definition.AccentColor * 0.48f);
                if ((hash & 1) == 0)
                    batch.Draw(_pixel, new Rectangle(destination.X + fossilX + 3, destination.Y + fossilY - 3, 2, 8),
                        definition.AccentColor * 0.42f);
                else
                    batch.Draw(_pixel, new Rectangle(destination.X + fossilX + 7, destination.Y + fossilY - 2, 2, 6), dark);
                break;
            case MaterialId.RuinAlloy:
                batch.Draw(_pixel, new Rectangle(destination.X + 4, destination.Y + 3, 2, destination.Height - 6), dark);
                batch.Draw(_pixel, new Rectangle(destination.Right - 7, destination.Y + 6, 3, 3),
                    definition.AccentColor * 0.72f);
                batch.Draw(_pixel, new Rectangle(destination.Right - 7, destination.Bottom - 8, 3, 3),
                    definition.AccentColor * 0.48f);
                break;
            case MaterialId.BrittleMasonry:
                batch.Draw(_pixel, new Rectangle(destination.Center.X, destination.Y + 4, 1, 14), dark);
                batch.Draw(_pixel, new Rectangle(destination.Center.X - 5, destination.Y + 9, 6, 1), dark);
                batch.Draw(_pixel, new Rectangle(destination.Center.X, destination.Y + 15, 5, 1), dark);
                break;
            case MaterialId.SaltGlass:
                batch.Draw(_pixel, new Rectangle(destination.X + 4, destination.Y + 4, 3, 3),
                    definition.AccentColor * 0.7f);
                batch.Draw(_pixel, new Rectangle(destination.X + 10, destination.Y + 9, 2, 9), dark);
                batch.Draw(_pixel, new Rectangle(destination.X + 12, destination.Y + 9, 6, 2),
                    definition.AccentColor * 0.42f);
                break;
        }

        if ((tile.Flags & TileFlags.RuinTrace) != 0)
        {
            batch.Draw(_pixel, new Rectangle(destination.X + 8, destination.Y + 6, 8, 2), GamePalette.SacredGold * 0.65f);
            batch.Draw(_pixel, new Rectangle(destination.X + 11, destination.Y + 4, 2, 8), GamePalette.SacredGold * 0.65f);
        }
        else if ((tile.Flags & TileFlags.Ecology) != 0)
        {
            batch.Draw(_pixel, new Rectangle(destination.X + 9, destination.Y + 2, 2, 7), GamePalette.SaltCyan * 0.58f);
            batch.Draw(_pixel, new Rectangle(destination.X + 5, destination.Y + 3, 5, 2), GamePalette.SaltCyan * 0.46f);
        }
        else if ((hash & 3) == 0)
        {
            batch.Draw(_pixel, new Rectangle(destination.Right - 5, destination.Bottom - 5, 2, 1),
                definition.AccentColor * 0.35f);
        }
    }
}
