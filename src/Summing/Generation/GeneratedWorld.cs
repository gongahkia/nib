using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Summing.Core;
using Summing.World;
using Summing.History;

namespace Summing.Generation;

public enum WorldFeatureKind
{
    Ruin,
    ExposedMachine,
    FossilArch,
    Ecology,
    RelicCandidate,
    BurrowerSpawn,
    BrittleSite
}

public sealed record WorldFeature(WorldFeatureKind Kind, Vector2 Position, int Variant, short ProvenanceId = 0);
public sealed record WeatherBand(int TopTile, int BottomTile, float Wind, string Exposure);

public sealed class GeneratedWorld
{
    public required WorldGenerationConfig Configuration { get; init; }
    public required TileWorld Terrain { get; init; }
    public required Vector2 Spawn { get; init; }
    public required Rectangle SummitBounds { get; init; }
    public required List<Point> RouteAnchors { get; init; }
    public required List<WorldFeature> Features { get; init; }
    public required List<WeatherBand> Weather { get; init; }
    public required WorldHistory History { get; init; }

    public float WindAt(float worldY)
    {
        var tileY = TileWorld.WorldToTile(worldY);
        foreach (var band in Weather)
            if (tileY >= band.TopTile && tileY <= band.BottomTile) return band.Wind;
        return 0f;
    }
}
