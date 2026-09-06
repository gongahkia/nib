using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Summing.Generation;
using Summing.History;
using Summing.World;
using Summing.World.Materials;

namespace Summing.Persistence;

public sealed class WorldSnapshot
{
    public int FormatVersion { get; set; } = 2;
    public DateTimeOffset SavedAtUtc { get; set; }
    public required WorldGenerationConfig Generator { get; set; }
    public required WorldGridSnapshot World { get; set; }
    public required List<MaterialSnapshot> Materials { get; set; }
    public required PositionSnapshot Spawn { get; set; }
    public required RectangleSnapshot Summit { get; set; }
    public required List<PointSnapshot> RouteAnchors { get; set; }
    public required List<FeatureSnapshot> Features { get; set; }
    public required List<WeatherBand> Weather { get; set; }
    public required WorldHistory History { get; set; }
    public required GenerationDiagnostics Diagnostics { get; set; }
    public required RuntimeSnapshot Runtime { get; set; }
}

public sealed record WorldGridSnapshot(int Width, int Height, int ChunkSize, int TileSize, List<ChunkSnapshot> Chunks);
public sealed record ChunkSnapshot(int X, int Y, int Revision, List<TileSnapshot> Tiles);
public sealed record TileSnapshot(int LocalX, int LocalY, MaterialId Material, byte Damage, TileFlags Flags, short ProvenanceId);
public sealed record MaterialSnapshot(MaterialId Id, string Name, bool Solid, int Hardness, float Friction, float Brittleness,
    float Heat, float Conductivity, bool Climbable, bool Structural);
public sealed record PositionSnapshot(float X, float Y);
public sealed record PointSnapshot(int X, int Y);
public sealed record RectangleSnapshot(int X, int Y, int Width, int Height);
public sealed record FeatureSnapshot(WorldFeatureKind Kind, float X, float Y, int Variant, short ProvenanceId);
public sealed record EntitySnapshot(string Id, string Type, float X, float Y, string State, Dictionary<string, string> Properties);
public sealed record RuntimeSnapshot(long Frame, string Difficulty, EntitySnapshot Player, int Bombs, int Ropes,
    List<EntitySnapshot> Entities, List<string> MutableTerrainEvents);
