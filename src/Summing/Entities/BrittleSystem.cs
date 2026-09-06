using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Summing.Gameplay;
using Summing.Generation;
using Summing.Player;
using Summing.Rendering;
using Summing.World;
using Summing.World.Materials;

namespace Summing.Entities;

public sealed class BrittleSystem
{
    private readonly List<BrittleStructure> _structures = [];

    public BrittleSystem(GeneratedWorld generated, DifficultyTuning tuning)
    {
        foreach (var feature in generated.Features)
        {
            if (feature.Kind != WorldFeatureKind.BrittleSite) continue;
            _structures.Add(new BrittleStructure(generated.Terrain.WorldToTile(feature.Position), generated.Terrain,
                1.05f * tuning.HazardDelayMultiplier));
        }
        generated.Terrain.TerrainChanged += OnTerrainChanged;
    }

    public IReadOnlyList<BrittleStructure> Structures => _structures;
    public event Action<Point>? Collapsed;
    public event Action<Point>? Triggered;

    public void Update(PlayerController player, TileWorld world, float dt)
    {
        foreach (var structure in _structures)
        {
            if (!structure.Triggered && structure.PlayerStanding(player.Bounds))
            {
                structure.Trigger();
                if (structure.Tiles.Count > 0) Triggered?.Invoke(structure.Tiles[0]);
            }
            if (structure.Update(world, dt) && structure.Tiles.Count > 0) Collapsed?.Invoke(structure.Tiles[0]);
        }
    }

    public void Draw(SpriteBatch batch, SpriteLibrary sprites)
    {
        foreach (var structure in _structures)
            foreach (var tile in structure.Tiles)
            {
                if (structure.Collapsed) continue;
                sprites.DrawBrittle(batch, new Vector2(tile.X * Core.GameConstants.TileSize,
                    tile.Y * Core.GameConstants.TileSize), structure.CrackFrame);
            }
    }

    private void OnTerrainChanged(TerrainChange change)
    {
        if (change.Material != MaterialId.BrittleMasonry) return;
        foreach (var structure in _structures)
        {
            if (!structure.Contains(change.Tile)) continue;
            if (change.Cause is "bomb" or "burrower") structure.Trigger(change.Cause == "bomb" ? 0.08f : 0.28f);
            if (change.Destroyed) structure.Trigger(0.08f);
        }
    }
}
