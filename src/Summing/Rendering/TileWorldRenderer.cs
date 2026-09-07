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
    private readonly VisualPackManager _visualPacks;

    public TileWorldRenderer(Texture2D pixel, VisualPackManager visualPacks)
    {
        _pixel = pixel;
        _visualPacks = visualPacks;
    }

    public void Draw(SpriteBatch batch, TileWorld world, Vector2 cameraPosition, float worldZoom)
    {
        var visibleWidth = GameConstants.VirtualWidth / worldZoom;
        var visibleHeight = GameConstants.VirtualHeight / worldZoom;
        var left = Math.Max(0, TileWorld.WorldToTile(cameraPosition.X - visibleWidth * 0.55f));
        var right = Math.Min(world.Width - 1, TileWorld.WorldToTile(cameraPosition.X + visibleWidth * 0.55f));
        var top = Math.Max(0, TileWorld.WorldToTile(cameraPosition.Y - visibleHeight * 0.58f));
        var bottom = Math.Min(world.Height - 1, TileWorld.WorldToTile(cameraPosition.Y + visibleHeight * 0.58f));

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
                _visualPacks.DrawTerrainOverlay(batch, world, destination, definition, x, y);
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

    public void DrawSanitized(SpriteBatch batch, TileWorld world, Vector2 cameraPosition, float worldZoom)
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
            case MaterialId.OchreClay:
                batch.Draw(_pixel, new Rectangle(destination.X + 3, destination.Y + 6, 4, 2),
                    definition.AccentColor * 0.48f);
                batch.Draw(_pixel, new Rectangle(destination.Right - 9, destination.Bottom - 8, 6, 2), dark);
                break;
            case MaterialId.PaleChalk:
                batch.Draw(_pixel, new Rectangle(destination.X + 2, destination.Y + 8, 18, 1), dark);
                batch.Draw(_pixel, new Rectangle(destination.X + 7, destination.Y + 16, 14, 1),
                    definition.AccentColor * 0.56f);
                break;
            case MaterialId.BlueShale:
                for (var layer = 0; layer < 3; layer++)
                    batch.Draw(_pixel, new Rectangle(destination.X + 2 + layer * 2,
                        destination.Y + 5 + layer * 6, 18 - layer * 3, 1),
                        layer == 1 ? definition.AccentColor * 0.46f : dark);
                break;
            case MaterialId.Ironstone:
                batch.Draw(_pixel, new Rectangle(destination.X + 4 + (int)(hash % 4), destination.Y + 7, 5, 4),
                    definition.AccentColor * 0.58f);
                batch.Draw(_pixel, new Rectangle(destination.Right - 8, destination.Bottom - 7, 4, 3), dark);
                break;
            case MaterialId.AshClinker:
                batch.Draw(_pixel, new Rectangle(destination.X + 5, destination.Y + 6, 3, 3), dark);
                batch.Draw(_pixel, new Rectangle(destination.X + 15, destination.Y + 13, 4, 4), dark);
                batch.Draw(_pixel, new Rectangle(destination.X + 8, destination.Bottom - 5, 2, 2),
                    definition.AccentColor * 0.42f);
                break;
            case MaterialId.PetrifiedFiber:
                batch.Draw(_pixel, new Rectangle(destination.X + 5, destination.Y + 3, 2, 17), dark);
                batch.Draw(_pixel, new Rectangle(destination.X + 11, destination.Y + 6, 2, 15),
                    definition.AccentColor * 0.48f);
                batch.Draw(_pixel, new Rectangle(destination.X + 17, destination.Y + 3, 1, 13), dark);
                break;
            case MaterialId.MachineCeramic:
                batch.Draw(_pixel, new Rectangle(destination.X + 3, destination.Y + 3, 18, 1), dark);
                batch.Draw(_pixel, new Rectangle(destination.X + 3, destination.Bottom - 4, 18, 1), dark);
                batch.Draw(_pixel, new Rectangle(destination.X + 3, destination.Y + 3, 1, 17), dark);
                batch.Draw(_pixel, new Rectangle(destination.Right - 4, destination.Y + 3, 1, 17), dark);
                break;
            case MaterialId.CopperSalt:
                batch.Draw(_pixel, new Rectangle(destination.X + 6, destination.Y + 5, 3, 10),
                    definition.AccentColor * 0.58f);
                batch.Draw(_pixel, new Rectangle(destination.X + 3, destination.Y + 9, 9, 3),
                    definition.AccentColor * 0.4f);
                batch.Draw(_pixel, new Rectangle(destination.Right - 7, destination.Bottom - 8, 3, 5), dark);
                break;
            case MaterialId.WeatheredConcrete:
                batch.Draw(_pixel, new Rectangle(destination.X + 3, destination.Y + 7, 4, 3), dark);
                batch.Draw(_pixel, new Rectangle(destination.X + 13, destination.Y + 4, 5, 3),
                    definition.AccentColor * 0.42f);
                batch.Draw(_pixel, new Rectangle(destination.X + 9, destination.Bottom - 7, 3, 3), dark);
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
