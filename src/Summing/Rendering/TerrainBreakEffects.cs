using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Summing.Core;
using Summing.World;
using Summing.World.Materials;

namespace Summing.Rendering;

public sealed class TerrainBreakEffects
{
    private sealed class Debris
    {
        public required Vector2 Position;
        public required Vector2 Velocity;
        public required Color Color;
        public required float Life;
        public required float MaximumLife;
        public required int Size;
    }

    private sealed class ImpactFlash
    {
        public required Vector2 Position;
        public required Color Color;
        public required float Life;
        public required float MaximumLife;
        public required float Radius;
    }

    private readonly List<Debris> _debris = [];
    private readonly List<ImpactFlash> _flashes = [];

    public int ActiveDebris => _debris.Count;

    public void Clear()
    {
        _debris.Clear();
        _flashes.Clear();
    }

    public void Emit(TerrainChange change)
    {
        var definition = MaterialCatalog.Get(change.Material);
        var center = new Vector2((change.Tile.X + 0.5f) * GameConstants.TileSize,
            (change.Tile.Y + 0.5f) * GameConstants.TileSize);
        var count = change.Destroyed ? change.Cause switch
        {
            "bomb" => 12,
            "brittle-collapse" => 10,
            "burrower" => 7,
            _ => 16
        } : change.Cause == "punch" ? 4 : 1;
        var force = change.Destroyed ? change.Cause == "bomb" ? 178f : 126f : 54f;
        for (var index = 0; index < count; index++)
        {
            var hash = Hash(change.Tile.X, change.Tile.Y, index, change.Cause.Length);
            var angle = (hash & 0xffff) / 65535f * MathHelper.TwoPi;
            var speed = force * (0.48f + ((hash >> 16) & 0xff) / 255f * 0.72f);
            var direction = new Vector2(MathF.Cos(angle), MathF.Sin(angle) * 0.72f - 0.42f);
            var life = 0.28f + ((hash >> 24) & 0xff) / 255f * (change.Destroyed ? 0.52f : 0.18f);
            _debris.Add(new Debris
            {
                Position = center + direction * 3f,
                Velocity = direction * speed,
                Color = (index & 1) == 0 ? definition.BaseColor : definition.AccentColor,
                Life = life,
                MaximumLife = life,
                Size = change.Destroyed && index % 4 == 0 ? 4 : index % 3 == 0 ? 3 : 2
            });
        }

        _flashes.Add(new ImpactFlash
        {
            Position = center,
            Color = definition.AccentColor,
            Life = change.Destroyed ? 0.2f : 0.1f,
            MaximumLife = change.Destroyed ? 0.2f : 0.1f,
            Radius = change.Destroyed ? 19f : 9f
        });
        if (_debris.Count > 420) _debris.RemoveRange(0, _debris.Count - 420);
        if (_flashes.Count > 48) _flashes.RemoveRange(0, _flashes.Count - 48);
    }

    public void Update(float dt)
    {
        for (var index = _debris.Count - 1; index >= 0; index--)
        {
            var particle = _debris[index];
            particle.Life -= dt;
            if (particle.Life <= 0f)
            {
                _debris.RemoveAt(index);
                continue;
            }
            particle.Velocity += new Vector2(0f, 390f * dt);
            particle.Velocity *= 1f - 1.8f * dt;
            particle.Position += particle.Velocity * dt;
        }
        for (var index = _flashes.Count - 1; index >= 0; index--)
        {
            _flashes[index].Life -= dt;
            if (_flashes[index].Life <= 0f) _flashes.RemoveAt(index);
        }
    }

    public void Draw(SpriteBatch batch, Texture2D pixel)
    {
        foreach (var particle in _debris)
        {
            var alpha = Math.Clamp(particle.Life / particle.MaximumLife, 0f, 1f);
            batch.Draw(pixel, new Rectangle((int)MathF.Round(particle.Position.X), (int)MathF.Round(particle.Position.Y),
                particle.Size, particle.Size), particle.Color * alpha);
        }
        foreach (var flash in _flashes)
        {
            var progress = 1f - flash.Life / flash.MaximumLife;
            var radius = (int)MathF.Round(MathHelper.Lerp(3f, flash.Radius, progress));
            var color = flash.Color * (1f - progress);
            batch.Draw(pixel, new Rectangle((int)flash.Position.X - radius, (int)flash.Position.Y - 1, radius * 2, 3), color);
            batch.Draw(pixel, new Rectangle((int)flash.Position.X - 1, (int)flash.Position.Y - radius, 3, radius * 2), color);
        }
    }

    private static uint Hash(int x, int y, int index, int salt)
    {
        var value = unchecked((uint)(x * 73856093 ^ y * 19349663 ^ index * 83492791 ^ salt * 265443576));
        value ^= value >> 16;
        value *= 0x7feb352d;
        value ^= value >> 15;
        return value;
    }
}
