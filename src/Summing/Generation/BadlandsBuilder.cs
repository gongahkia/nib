using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Summing.Core;
using Summing.World;
using Summing.World.Materials;
using Summing.History;

namespace Summing.Generation;

internal static class BadlandsBuilder
{
    public static GeneratedWorld Build(WorldGenerationConfig source, GeneratorVariant variant)
    {
        var configuration = Clone(source, variant);
        var parameters = configuration.Parameters;
        var world = new TileWorld(parameters.Width, parameters.Height);
        var random = new DeterministicRandom(configuration.Seed ^ ((long)variant * 0x51f15e5dL));
        var route = BuildMacroRoute(parameters, random);
        var protectedTiles = new HashSet<Point>();
        ProtectRouteClearance(route, protectedTiles);

        BuildFoundation(world, configuration.Seed);
        for (var i = 0; i < route.Count; i++)
            BuildRouteFormation(world, configuration, variant, random, route, protectedTiles, i);
        BuildGeologicalForms(world, configuration, variant, random, protectedTiles);
        ApplyErosion(world, configuration, variant, protectedTiles);
        ClearRouteClearance(world, route);
        var history = WorldHistoryGenerator.Generate(configuration.Seed);
        var features = PlaceSurfaceFeatures(world, configuration, variant, random, route, history.Cultures.Count);
        var weather = BuildWeather(parameters, configuration.Seed);

        var spawnAnchor = route[0];
        var summitAnchor = route[^1];
        return new GeneratedWorld
        {
            Configuration = configuration,
            Terrain = world,
            Spawn = new Vector2((spawnAnchor.X + 0.5f) * GameConstants.TileSize, spawnAnchor.Y * GameConstants.TileSize),
            SummitBounds = new Rectangle((summitAnchor.X - 2) * GameConstants.TileSize,
                Math.Max(0, summitAnchor.Y - 4) * GameConstants.TileSize, 5 * GameConstants.TileSize, 4 * GameConstants.TileSize),
            RouteAnchors = route,
            Features = features,
            Weather = weather,
            History = history
        };
    }

    private static List<Point> BuildMacroRoute(BadlandsParameters parameters, DeterministicRandom random)
    {
        var route = new List<Point>();
        var x = parameters.Width / 2;
        var y = parameters.Height - 7;
        var direction = random.Chance(0.5f) ? -1 : 1;
        while (y > 7)
        {
            route.Add(new Point(x, y));
            var altitude = 1f - y / (float)parameters.Height;
            var verticalStep = altitude < 0.34f ? random.Range(3, 5) : altitude < 0.67f ? random.Range(3, 6) : random.Range(4, 6);
            var horizontalRange = altitude < 0.34f ? 5 : altitude < 0.67f ? 7 : 8;
            if (random.Chance(0.22f)) direction *= -1;
            x += direction * random.Range(2, horizontalRange + 1);
            if (x < 11) { x = 11 + random.Range(0, 5); direction = 1; }
            if (x > parameters.Width - 12) { x = parameters.Width - 12 - random.Range(0, 5); direction = -1; }
            y -= verticalStep;
        }
        route.Add(new Point(x, 6));
        return route;
    }

    private static void BuildFoundation(TileWorld world, long seed)
    {
        for (var y = world.Height - 7; y < world.Height; y++)
            for (var x = 0; x < world.Width; x++) SetGeologicalTile(world, seed, x, y, y - (world.Height - 7));
    }

    private static void BuildRouteFormation(TileWorld world, WorldGenerationConfig config, GeneratorVariant variant,
        DeterministicRandom random, List<Point> route, HashSet<Point> protectedTiles, int index)
    {
        var anchor = route[index];
        var altitude = 1f - anchor.Y / (float)world.Height;
        var halfWidth = altitude < 0.34f ? random.Range(5, 8) : altitude < 0.67f ? random.Range(4, 7) : random.Range(3, 6);
        var depth = variant == GeneratorVariant.Cellular ? random.Range(2, 5) : random.Range(2, 6);
        var attachToSide = variant == GeneratorVariant.Heightmap
            ? random.Chance(0.72f)
            : variant == GeneratorVariant.Layered && random.Chance(0.48f);
        var attachLeft = ((index / 3) & 1) == 0;
        var left = anchor.X - halfWidth;
        var right = anchor.X + halfWidth;
        if (attachToSide)
        {
            if (attachLeft) left = 0;
            else right = world.Width - 1;
        }

        for (var x = Math.Max(0, left); x <= Math.Min(world.Width - 1, right); x++)
        {
            var edgeDistance = Math.Min(x - left, right - x);
            var localDepth = depth + (edgeDistance > 3 ? random.Range(0, 3) : 0);
            for (var y = anchor.Y; y < Math.Min(world.Height, anchor.Y + localDepth); y++)
            {
                if (y != anchor.Y && protectedTiles.Contains(new Point(x, y))) continue;
                SetGeologicalTile(world, config.Seed, x, y, y - anchor.Y);
            }
        }

        for (var x = anchor.X - 3; x <= anchor.X + 3; x++)
        {
            protectedTiles.Add(new Point(x, anchor.Y));
            protectedTiles.Add(new Point(x, anchor.Y + 1));
        }

        if (index > 2 && index % 8 == 4)
        {
            var branchDirection = attachLeft ? 1 : -1;
            var branchX = Math.Clamp(anchor.X + branchDirection * random.Range(9, 14), 5, world.Width - 6);
            var branchY = Math.Min(world.Height - 8, anchor.Y + random.Range(1, 4));
            for (var x = branchX - 3; x <= branchX + 3; x++)
                for (var y = branchY; y < branchY + random.Range(2, 5); y++)
                    SetGeologicalTile(world, config.Seed, x, y, y - branchY);
        }
    }

    private static void BuildGeologicalForms(TileWorld world, WorldGenerationConfig config, GeneratorVariant variant,
        DeterministicRandom random, HashSet<Point> protectedTiles)
    {
        var count = variant == GeneratorVariant.Cellular ? 34 : variant == GeneratorVariant.Layered ? 22 : 12;
        for (var i = 0; i < count; i++)
        {
            var cx = random.Range(3, world.Width - 3);
            var cy = random.Range(10, world.Height - 10);
            var radiusX = random.Range(2, variant == GeneratorVariant.Cellular ? 8 : 6);
            var radiusY = random.Range(2, 8);
            for (var y = cy - radiusY; y <= cy + radiusY; y++)
                for (var x = cx - radiusX; x <= cx + radiusX; x++)
                {
                    var nx = (x - cx) / (float)radiusX;
                    var ny = (y - cy) / (float)radiusY;
                    var irregularity = DeterministicRandom.Hash01(config.Seed, x, y, 11) * 0.34f;
                    if (nx * nx + ny * ny > 0.72f + irregularity || protectedTiles.Contains(new Point(x, y))) continue;
                    SetGeologicalTile(world, config.Seed, x, y, Math.Abs(y - cy));
                }
        }

        var archCount = variant == GeneratorVariant.Layered ? 7 : 3;
        for (var i = 0; i < archCount; i++)
        {
            var x = random.Range(8, world.Width - 15);
            var y = random.Range(16, world.Height - 20);
            var width = random.Range(6, 12);
            var height = random.Range(4, 9);
            for (var column = 0; column < 2; column++)
                for (var yy = y; yy < y + height; yy++)
                {
                    SetGeologicalTile(world, config.Seed, x + column, yy, column);
                    SetGeologicalTile(world, config.Seed, x + width - column, yy, column);
                }
            for (var xx = x; xx <= x + width; xx++) SetGeologicalTile(world, config.Seed, xx, y, 0);
        }
    }

    private static void ProtectRouteClearance(List<Point> route, HashSet<Point> protectedTiles)
    {
        foreach (var anchor in route)
            for (var x = anchor.X - 3; x <= anchor.X + 3; x++)
                for (var y = anchor.Y - 3; y <= anchor.Y + 1; y++) protectedTiles.Add(new Point(x, y));
    }

    private static void ClearRouteClearance(TileWorld world, List<Point> route)
    {
        // route clearance is a generation constraint, applied before validation; it is not a rejected-world repair
        var supports = new HashSet<Point>();
        foreach (var anchor in route)
            for (var x = anchor.X - 3; x <= anchor.X + 3; x++) supports.Add(new Point(x, anchor.Y));
        foreach (var anchor in route)
            for (var x = anchor.X - 2; x <= anchor.X + 2; x++)
                for (var y = anchor.Y - 3; y < anchor.Y; y++)
                    if (!supports.Contains(new Point(x, y))) world.SetTile(x, y, MaterialId.Air);
    }

    private static void ApplyErosion(TileWorld world, WorldGenerationConfig config, GeneratorVariant variant, HashSet<Point> protectedTiles)
    {
        var erosion = config.Parameters.Erosion * (variant == GeneratorVariant.Cellular ? 1.35f : variant == GeneratorVariant.Heightmap ? 0.6f : 1f);
        for (var y = 2; y < world.Height - 5; y++)
            for (var x = 2; x < world.Width - 2; x++)
            {
                if (!world.GetTile(x, y).Solid || protectedTiles.Contains(new Point(x, y))) continue;
                var exposed = !world.GetTile(x - 1, y).Solid || !world.GetTile(x + 1, y).Solid || !world.GetTile(x, y - 1).Solid;
                if (!exposed) continue;
                var noise = DeterministicRandom.Hash01(config.Seed, x / 2, y / 2, 29);
                if (noise < erosion * 0.11f) world.SetTile(x, y, MaterialId.Air);
            }
    }

    private static List<WorldFeature> PlaceSurfaceFeatures(TileWorld world, WorldGenerationConfig config,
        GeneratorVariant variant, DeterministicRandom random, List<Point> route, int cultureCount)
    {
        var features = new List<WorldFeature>();
        for (var i = 3; i < route.Count - 2; i++)
        {
            var anchor = route[i];
            var altitude = 1f - anchor.Y / (float)world.Height;
            if (random.Chance(config.Parameters.RuinDensity * (variant == GeneratorVariant.Layered ? 1.5f : 0.75f)))
            {
                var provenance = (short)random.Range(1, cultureCount + 1);
                var position = new Vector2((anchor.X + random.Range(-3, 4) + 0.5f) * GameConstants.TileSize,
                    anchor.Y * GameConstants.TileSize);
                features.Add(new WorldFeature(i % 4 == 0 ? WorldFeatureKind.ExposedMachine : WorldFeatureKind.Ruin,
                    position, random.Range(0, 4), provenance));
                var tracePoint = world.WorldToTile(position + new Vector2(0f, 2f));
                var traceTile = world.GetTile(tracePoint.X, tracePoint.Y);
                if (traceTile.Solid)
                {
                    traceTile.Flags |= TileFlags.RuinTrace;
                    traceTile.ProvenanceId = provenance;
                    world.SetTile(tracePoint.X, tracePoint.Y, traceTile);
                }
            }
            if (altitude > 0.18f && random.Chance(config.Parameters.EcologyDensity * (0.4f + altitude)))
                features.Add(new WorldFeature(WorldFeatureKind.Ecology,
                    new Vector2((anchor.X + random.Range(-4, 5)) * GameConstants.TileSize, anchor.Y * GameConstants.TileSize),
                    random.Range(0, 3)));
            if (i % 11 == 6)
                features.Add(new WorldFeature(WorldFeatureKind.RelicCandidate,
                    new Vector2((anchor.X + (i % 2 == 0 ? 6 : -6)) * GameConstants.TileSize, (anchor.Y - 1) * GameConstants.TileSize),
                    i % 6, (short)random.Range(1, cultureCount + 1)));
            if (i % 17 == 9)
                features.Add(new WorldFeature(WorldFeatureKind.BurrowerSpawn,
                    new Vector2(anchor.X * GameConstants.TileSize, (anchor.Y - 1) * GameConstants.TileSize), 0));
            if (i % 9 == 5)
            {
                features.Add(new WorldFeature(WorldFeatureKind.BrittleSite,
                    new Vector2((anchor.X + 2) * GameConstants.TileSize, anchor.Y * GameConstants.TileSize), 0));
                for (var x = anchor.X; x < anchor.X + 4 && x < world.Width; x++)
                    world.SetTile(x, anchor.Y, MaterialId.BrittleMasonry);
            }
        }
        return features;
    }

    private static List<WeatherBand> BuildWeather(BadlandsParameters parameters, long seed)
    {
        var third = parameters.Height / 3;
        var direction = DeterministicRandom.Hash01(seed, 0, 0, 41) > 0.5f ? 1f : -1f;
        return
        [
            new WeatherBand(third * 2, parameters.Height, direction * parameters.WindStrength * 0.15f, "sheltered dust"),
            new WeatherBand(third, third * 2 - 1, -direction * parameters.WindStrength * 0.45f, "crosswind"),
            new WeatherBand(0, third - 1, direction * parameters.WindStrength, "summit exposure")
        ];
    }

    private static void SetGeologicalTile(TileWorld world, long seed, int x, int y, int depth)
    {
        if (!world.Contains(x, y)) return;
        var altitude = 1f - y / (float)world.Height;
        var band = (y / 12 + (int)(DeterministicRandom.Hash(seed, x / 5, 0, 3) % 3)) % 6;
        var material = depth == 0 && band <= 1 ? MaterialId.Loess : band switch
        {
            0 or 1 => MaterialId.RedSandstone,
            2 => altitude > 0.6f ? MaterialId.SaltGlass : MaterialId.FossilComposite,
            3 or 4 => MaterialId.BlackBasalt,
            _ => MaterialId.FossilComposite
        };
        world.SetTile(x, y, material);
    }

    private static WorldGenerationConfig Clone(WorldGenerationConfig source, GeneratorVariant variant) => new()
    {
        Seed = source.Seed,
        GeneratorVersion = source.GeneratorVersion,
        Variant = variant,
        Parameters = new BadlandsParameters
        {
            Width = source.Parameters.Width,
            Height = source.Parameters.Height,
            Erosion = source.Parameters.Erosion,
            RuinDensity = source.Parameters.RuinDensity,
            EcologyDensity = source.Parameters.EcologyDensity,
            WindStrength = source.Parameters.WindStrength
        }
    };
}
