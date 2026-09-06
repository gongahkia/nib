using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Summing.Core;
using Summing.World;
using Summing.World.Materials;

namespace Summing.Entities;

public sealed class BrittleStructure
{
    private readonly List<Point> _tiles;
    private readonly float _delay;

    public BrittleStructure(Point start, TileWorld world, float delay)
    {
        _delay = delay;
        _tiles = [];
        for (var x = start.X - 2; x <= start.X + 2; x++)
            if (world.GetTile(x, start.Y).Material == MaterialId.BrittleMasonry) _tiles.Add(new Point(x, start.Y));
    }

    public IReadOnlyList<Point> Tiles => _tiles;
    public bool Triggered { get; private set; }
    public bool Collapsed { get; private set; }
    public float Remaining { get; private set; }
    public int CrackFrame => !Triggered ? 0 : Math.Clamp((int)((1f - Remaining / _delay) * 4f), 0, 3);

    public bool Contains(Point tile) => _tiles.Contains(tile);

    public void Trigger(float urgency = 1f)
    {
        if (Collapsed) return;
        var requested = _delay * Math.Clamp(urgency, 0.08f, 1f);
        if (!Triggered || requested < Remaining) Remaining = requested;
        Triggered = true;
    }

    public bool PlayerStanding(Aabb playerBounds)
    {
        foreach (var tile in _tiles)
        {
            var tileBounds = new Aabb(tile.X * GameConstants.TileSize, tile.Y * GameConstants.TileSize,
                GameConstants.TileSize, GameConstants.TileSize);
            if (playerBounds.Bottom <= tileBounds.Top + 4f && playerBounds.Bottom >= tileBounds.Top - 3f &&
                playerBounds.Right > tileBounds.Left && playerBounds.Left < tileBounds.Right) return true;
        }
        return false;
    }

    public bool Update(TileWorld world, float dt)
    {
        if (!Triggered || Collapsed) return false;
        Remaining -= dt;
        if (Remaining > 0f) return false;
        foreach (var tile in _tiles)
        {
            if (world.GetTile(tile.X, tile.Y).Material == MaterialId.BrittleMasonry)
                world.DamageTile(tile.X, tile.Y, 20, "brittle-collapse");
        }
        Collapsed = true;
        return true;
    }
}
