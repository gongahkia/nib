using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Summing.Core;
using Summing.World.Materials;

namespace Summing.World;

public sealed class TileWorld : ICollisionWorld
{
    private readonly Dictionary<ChunkCoordinate, TileChunk> _chunks = [];

    public TileWorld(int width, int height)
    {
        Width = width;
        Height = height;
        for (var cy = 0; cy < (height + GameConstants.ChunkSize - 1) / GameConstants.ChunkSize; cy++)
            for (var cx = 0; cx < (width + GameConstants.ChunkSize - 1) / GameConstants.ChunkSize; cx++)
                _chunks[new ChunkCoordinate(cx, cy)] = new TileChunk(new ChunkCoordinate(cx, cy));
    }

    public int Width { get; }
    public int Height { get; }
    public int PixelWidth => Width * GameConstants.TileSize;
    public int PixelHeight => Height * GameConstants.TileSize;
    public IReadOnlyDictionary<ChunkCoordinate, TileChunk> Chunks => _chunks;
    public event Action<TerrainChange>? TerrainChanged;

    public TerrainTile GetTile(int x, int y)
    {
        if (x < 0 || x >= Width || y >= Height)
            return new TerrainTile { Material = MaterialId.BlackBasalt };
        if (y < 0) return default;
        var (chunk, localX, localY) = Locate(x, y);
        return chunk.Get(localX, localY);
    }

    public void SetTile(int x, int y, MaterialId material, TileFlags flags = TileFlags.None, short provenanceId = 0)
    {
        if (!Contains(x, y)) return;
        var (chunk, localX, localY) = Locate(x, y);
        chunk.Set(localX, localY, new TerrainTile { Material = material, Flags = flags, ProvenanceId = provenanceId });
    }

    public void SetTile(int x, int y, TerrainTile tile)
    {
        if (!Contains(x, y)) return;
        var (chunk, localX, localY) = Locate(x, y);
        chunk.Set(localX, localY, tile);
    }

    public bool DamageTile(int x, int y, int damage, string cause)
    {
        if (!Contains(x, y)) return false;
        var tile = GetTile(x, y);
        var material = MaterialCatalog.Get(tile.Material);
        if (!material.Solid || damage <= 0) return false;
        var accumulated = tile.Damage + damage;
        var destroyed = accumulated >= material.Hardness;
        if (destroyed)
            SetTile(x, y, MaterialId.Air);
        else
        {
            tile.Damage = (byte)Math.Min(byte.MaxValue, accumulated);
            SetTile(x, y, tile);
        }
        TerrainChanged?.Invoke(new TerrainChange(new Point(x, y), material.Id, cause, damage, destroyed));
        return destroyed;
    }

    public int DamageCircle(Vector2 center, float radius, int damage, string cause)
    {
        var minX = WorldToTile(center.X - radius);
        var maxX = WorldToTile(center.X + radius);
        var minY = WorldToTile(center.Y - radius);
        var maxY = WorldToTile(center.Y + radius);
        var destroyed = 0;
        for (var y = minY; y <= maxY; y++)
            for (var x = minX; x <= maxX; x++)
            {
                var tileCenter = new Vector2((x + 0.5f) * GameConstants.TileSize, (y + 0.5f) * GameConstants.TileSize);
                if (Vector2.DistanceSquared(center, tileCenter) <= radius * radius && DamageTile(x, y, damage, cause)) destroyed++;
            }
        return destroyed;
    }

    public bool OverlapsSolid(Aabb bounds)
    {
        var minX = WorldToTile(bounds.Left + 0.01f);
        var maxX = WorldToTile(bounds.Right - 0.01f);
        var minY = WorldToTile(bounds.Top + 0.01f);
        var maxY = WorldToTile(bounds.Bottom - 0.01f);
        for (var y = minY; y <= maxY; y++)
            for (var x = minX; x <= maxX; x++)
                if (GetTile(x, y).Solid) return true;
        return false;
    }

    public bool IsGrappleCompatible(Vector2 worldPosition)
    {
        var tile = GetTile(WorldToTile(worldPosition.X), WorldToTile(worldPosition.Y));
        return tile.Solid && MaterialCatalog.Get(tile.Material).GrappleCompatible;
    }

    public bool IsClimbable(Vector2 worldPosition)
    {
        var tile = GetTile(WorldToTile(worldPosition.X), WorldToTile(worldPosition.Y));
        return tile.Solid && MaterialCatalog.Get(tile.Material).Climbable;
    }

    public float FrictionAt(Vector2 worldPosition)
    {
        var tile = GetTile(WorldToTile(worldPosition.X), WorldToTile(worldPosition.Y));
        return tile.Solid ? MaterialCatalog.Get(tile.Material).Friction : 1f;
    }

    public bool RaycastGrapple(Vector2 origin, Vector2 direction, float maximumDistance, out Vector2 hit)
    {
        for (var distance = 3f; distance <= maximumDistance; distance += 3f)
        {
            var sample = origin + direction * distance;
            if (!IsGrappleCompatible(sample)) continue;
            hit = sample;
            return true;
        }
        hit = default;
        return false;
    }

    public Point WorldToTile(Vector2 position) => new(WorldToTile(position.X), WorldToTile(position.Y));
    public static int WorldToTile(float coordinate) => (int)MathF.Floor(coordinate / GameConstants.TileSize);
    public bool Contains(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

    public ulong Fingerprint()
    {
        const ulong offset = 14695981039346656037UL;
        const ulong prime = 1099511628211UL;
        var hash = offset;
        for (var y = 0; y < Height; y++)
            for (var x = 0; x < Width; x++)
            {
                var tile = GetTile(x, y);
                hash ^= (byte)tile.Material;
                hash *= prime;
                hash ^= (byte)tile.Flags;
                hash *= prime;
                hash ^= unchecked((ushort)tile.ProvenanceId);
                hash *= prime;
            }
        return hash;
    }

    public static TileWorld CreateMovementTest()
    {
        var world = new TileWorld(52, 22);
        Fill(world, 0, 18, 52, 4, MaterialId.RedSandstone);
        Fill(world, 0, 0, 2, 22, MaterialId.BlackBasalt);
        Fill(world, 50, 0, 2, 22, MaterialId.BlackBasalt);
        Fill(world, 4, 15, 8, 3, MaterialId.Loess);
        Fill(world, 14, 12, 3, 6, MaterialId.FossilComposite);
        Fill(world, 20, 16, 6, 2, MaterialId.RedSandstone);
        Fill(world, 29, 10, 2, 8, MaterialId.RuinAlloy);
        Fill(world, 35, 14, 7, 4, MaterialId.BrittleMasonry);
        Fill(world, 44, 8, 2, 10, MaterialId.BlackBasalt);
        for (var x = 5; x < 48; x += 4) world.SetTile(x, 17, MaterialId.SaltGlass);
        return world;
    }

    private (TileChunk Chunk, int LocalX, int LocalY) Locate(int x, int y)
    {
        var coordinate = new ChunkCoordinate(x / GameConstants.ChunkSize, y / GameConstants.ChunkSize);
        return (_chunks[coordinate], x % GameConstants.ChunkSize, y % GameConstants.ChunkSize);
    }

    private static void Fill(TileWorld world, int x, int y, int width, int height, MaterialId material)
    {
        for (var tileY = y; tileY < y + height; tileY++)
            for (var tileX = x; tileX < x + width; tileX++) world.SetTile(tileX, tileY, material);
    }
}
