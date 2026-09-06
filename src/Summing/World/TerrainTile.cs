using System;
using Summing.World.Materials;

namespace Summing.World;

[Flags]
public enum TileFlags : byte
{
    None = 0,
    PlayerBuilt = 1,
    BrittleTriggered = 2,
    RuinTrace = 4,
    Ecology = 8
}

public struct TerrainTile
{
    public MaterialId Material { get; set; }
    public byte Damage { get; set; }
    public TileFlags Flags { get; set; }
    public short ProvenanceId { get; set; }
    public readonly bool Solid => MaterialCatalog.Get(Material).Solid;
}
