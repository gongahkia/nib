using System;
using Summing.Core;
using Summing.World.Materials;

namespace Summing.World;

public readonly record struct ChunkCoordinate(int X, int Y);

public sealed class TileChunk
{
    private readonly TerrainTile[] _tiles = new TerrainTile[GameConstants.ChunkSize * GameConstants.ChunkSize];

    public TileChunk(ChunkCoordinate coordinate)
    {
        Coordinate = coordinate;
    }

    public ChunkCoordinate Coordinate { get; }
    public bool Dirty { get; private set; } = true;
    public int Revision { get; private set; }

    public TerrainTile Get(int localX, int localY) => _tiles[localY * GameConstants.ChunkSize + localX];

    public void Set(int localX, int localY, TerrainTile tile)
    {
        var index = localY * GameConstants.ChunkSize + localX;
        if (_tiles[index].Material == tile.Material && _tiles[index].Damage == tile.Damage &&
            _tiles[index].Flags == tile.Flags && _tiles[index].ProvenanceId == tile.ProvenanceId) return;
        _tiles[index] = tile;
        Dirty = true;
        Revision++;
    }

    public void MarkClean() => Dirty = false;
    public ReadOnlySpan<TerrainTile> Tiles => _tiles;
}
