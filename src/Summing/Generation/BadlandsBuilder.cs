using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Summing.Core;
using Summing.History;
using Summing.World;
using Summing.World.Materials;

namespace Summing.Generation;

internal static class BadlandsBuilder
{
    public static GeneratedWorld Build(WorldGenerationConfig source, GeneratorVariant variant)
    {
        var configuration = Clone(source, variant);
        var parameters = configuration.Parameters;
        var world = new TileWorld(parameters.Width, parameters.Height);
        var random = new DeterministicRandom(configuration.Seed ^ ((long)variant * 0x51f15e5dL));
        var route = BuildMacroRoute(parameters, variant, random);
        var detours = BuildDetours(parameters, route, random);
        var protectedTiles = new HashSet<Point>();
        ProtectTravel(route, protectedTiles, true);
        foreach (var detour in detours) ProtectTravel(detour, protectedTiles, false);

        BuildFoundation(world, configuration.Seed, variant);
        BuildTravelFormation(world, configuration, variant, route, random, true);
        foreach (var detour in detours) BuildTravelFormation(world, configuration, variant, detour, random, false);
        BuildEscarpments(world, configuration, variant, random, route, protectedTiles);
        BuildGroundedOutcrops(world, configuration, variant, random, route, protectedTiles);
        BuildArches(world, configuration, variant, random, route, protectedTiles);
        CarveFractures(world, configuration, variant, random, protectedTiles);
        ApplyErosion(world, configuration, variant, protectedTiles);
        foreach (var detour in detours)
        {
            RestoreTravelSurfaces(world, configuration, variant, detour, false);
            ClearStandingCells(world, detour);
        }
        RestoreTravelSurfaces(world, configuration, variant, route, true);
        ClearStandingCells(world, route);

        var history = WorldHistoryGenerator.Generate(configuration.Seed);
        var features = PlaceSurfaceFeatures(world, configuration, variant, random, route, detours, history.Cultures.Count);
        var weather = BuildWeather(parameters, configuration.Seed);
        var spawnAnchor = route[0];
        var summitAnchor = route[^1];
        return new GeneratedWorld
        {
            Configuration = configuration,
            Terrain = world,
            Spawn = FootPosition(spawnAnchor),
            SummitBounds = new Rectangle((summitAnchor.X - 2) * GameConstants.TileSize,
                Math.Max(0, summitAnchor.Y - 4) * GameConstants.TileSize, 5 * GameConstants.TileSize, 4 * GameConstants.TileSize),
            RouteAnchors = route,
            Features = features,
            Weather = weather,
            History = history
        };
    }

    private static List<Point> BuildMacroRoute(BadlandsParameters parameters, GeneratorVariant variant,
        DeterministicRandom random)
    {
        var route = new List<Point>();
        var x = parameters.Width / 2;
        var y = parameters.Height - 7;
        var direction = random.Chance(0.5f) ? -1 : 1;
        var sinceDescent = 9;
        route.Add(new Point(x, y));
        while (y > 10)
        {
            var altitude = 1f - y / (float)parameters.Height;
            var descentChance = variant == GeneratorVariant.Cellular ? 0.1f : 0.065f;
            var traverseChance = variant == GeneratorVariant.Heightmap ? 0.28f : 0.2f;
            int verticalStep;
            if (sinceDescent >= 8 && y < parameters.Height - 18 && y > 18 && random.Chance(descentChance))
            {
                verticalStep = -random.Range(1, 3);
                sinceDescent = 0;
                if (random.Chance(0.55f)) direction *= -1;
            }
            else if (random.Chance(traverseChance))
            {
                verticalStep = random.Chance(0.65f) ? 0 : 1;
                sinceDescent++;
            }
            else
            {
                verticalStep = altitude < 0.34f ? 2 :
                    altitude < 0.67f ? random.Range(2, 4) : random.Range(2, 5);
                sinceDescent++;
            }

            var horizontalMaximum = altitude < 0.34f ? 4 : altitude < 0.67f ? 5 : 6;
            var horizontalMinimum = verticalStep <= 0 ? 3 : 2;
            if (random.Chance(variant == GeneratorVariant.Heightmap ? 0.16f : 0.24f)) direction *= -1;
            x += direction * random.Range(horizontalMinimum, horizontalMaximum + 1);
            if (x < 9)
            {
                x = 9 + random.Range(0, 4);
                direction = 1;
            }
            else if (x > parameters.Width - 10)
            {
                x = parameters.Width - 10 - random.Range(0, 4);
                direction = -1;
            }
            y = Math.Clamp(y - verticalStep, 6, parameters.Height - 7);
            x = AvoidVerticalConflict(route, x, y, direction, parameters.Width);
            AddDistinct(route, new Point(x, y));
        }

        while (y > 6)
        {
            var verticalStep = Math.Min(random.Range(2, 5), y - 6);
            x = Math.Clamp(x + direction * random.Range(2, 5), 9, parameters.Width - 10);
            y -= verticalStep;
            x = AvoidVerticalConflict(route, x, y, direction, parameters.Width);
            AddDistinct(route, new Point(x, y));
        }
        return route;
    }

    private static List<List<Point>> BuildDetours(BadlandsParameters parameters, List<Point> route,
        DeterministicRandom random)
    {
        var detours = new List<List<Point>>();
        for (var startIndex = 8; startIndex + 8 < route.Count; startIndex += 18)
        {
            var endIndex = Math.Min(route.Count - 2, startIndex + random.Range(6, 9));
            var start = route[startIndex];
            var end = route[endIndex];
            var averageX = (start.X + end.X) * 0.5f;
            var side = averageX < parameters.Width * 0.5f ? 1f : -1f;
            if (random.Chance(0.25f)) side *= -1f;
            var controlX = Math.Clamp(averageX + side * random.Range(9, 15), 7f, parameters.Width - 8f);
            var verticalDistance = Math.Abs(start.Y - end.Y);
            var steps = Math.Max(8, verticalDistance / 2 + 3);
            var branch = new List<Point>();
            for (var step = 0; step <= steps; step++)
            {
                var t = step / (float)steps;
                var inverse = 1f - t;
                var x = inverse * inverse * start.X + 2f * inverse * t * controlX + t * t * end.X;
                var y = MathHelper.Lerp(start.Y, end.Y, t) + MathF.Sin(t * MathHelper.Pi) * 2.2f;
                AddDistinct(branch, new Point((int)MathF.Round(x), (int)MathF.Round(y)));
            }
            if (branch.Count >= 5) detours.Add(branch);
        }
        return detours;
    }

    private static void BuildFoundation(TileWorld world, long seed, GeneratorVariant variant)
    {
        for (var x = 0; x < world.Width; x++)
        {
            var broad = DeterministicRandom.Hash01(seed, x / 5, 0, 71);
            var top = world.Height - 7 + (int)MathF.Round((broad - 0.5f) * 2f);
            for (var y = top; y < world.Height; y++) SetGeologicalTile(world, seed, variant, x, y, y - top);
        }
    }

    private static void BuildTravelFormation(TileWorld world, WorldGenerationConfig config, GeneratorVariant variant,
        IReadOnlyList<Point> anchors, DeterministicRandom random, bool primary)
    {
        for (var index = 0; index < anchors.Count - 1; index++)
        {
            var current = anchors[index];
            var next = anchors[index + 1];
            var steps = Math.Max(Math.Abs(next.X - current.X), Math.Abs(next.Y - current.Y));
            for (var step = 0; step <= steps; step++)
            {
                var t = steps == 0 ? 0f : step / (float)steps;
                var x = (int)MathF.Round(MathHelper.Lerp(current.X, next.X, t));
                var y = (int)MathF.Round(MathHelper.Lerp(current.Y, next.Y, t));
                var thickness = primary ? 3 : 2;
                for (var depth = 0; depth < thickness; depth++)
                    SetGeologicalTile(world, config.Seed, variant, x, y + depth, depth);
            }
        }

        for (var index = 0; index < anchors.Count; index++)
        {
            var anchor = anchors[index];
            const int topHalfWidth = 1;
            var approachDirection = index == 0 ? 0 : Math.Sign(anchor.X - anchors[index - 1].X);
            var depth = random.Range(variant == GeneratorVariant.Cellular ? 5 : 7,
                variant == GeneratorVariant.Heightmap ? 13 : 11);
            for (var down = 0; down <= depth; down++)
            {
                var expansion = variant == GeneratorVariant.Cellular ? down / 3 : down / 2;
                var irregular = (int)(DeterministicRandom.Hash(config.Seed, anchor.X, anchor.Y + down, 79) % 3) - 1;
                var extended = Math.Max(0, expansion + (down > 1 ? irregular : 0));
                var lean = variant == GeneratorVariant.Cellular
                    ? (int)MathF.Round((DeterministicRandom.Hash01(config.Seed, anchor.X, down, 83) - 0.5f) * 2f)
                    : 0;
                var leftExtent = topHalfWidth + (approachDirection < 0 ? extended : extended / 3);
                var rightExtent = topHalfWidth + (approachDirection > 0 ? extended : extended / 3);
                for (var x = anchor.X - leftExtent + lean; x <= anchor.X + rightExtent + lean; x++)
                    SetGeologicalTile(world, config.Seed, variant, x, anchor.Y + down, down);
            }
        }
    }

    private static void BuildEscarpments(TileWorld world, WorldGenerationConfig config, GeneratorVariant variant,
        DeterministicRandom random, IReadOnlyList<Point> route, HashSet<Point> protectedTiles)
    {
        var count = variant == GeneratorVariant.Heightmap ? 12 : variant == GeneratorVariant.Layered ? 9 : 6;
        for (var index = 0; index < count; index++)
        {
            var top = 10 + index * (world.Height - 24) / count + random.Range(-4, 5);
            var height = random.Range(13, variant == GeneratorVariant.Heightmap ? 28 : 23);
            var leftSide = ((index + random.Range(0, 2)) & 1) == 0;
            var maximumReach = random.Range(10, variant == GeneratorVariant.Heightmap ? 29 : 23);
            for (var y = Math.Max(2, top); y < Math.Min(world.Height - 7, top + height); y++)
            {
                var routeX = NearestRouteX(route, y);
                var row = y - top;
                var weathering = (int)MathF.Round(MathF.Sin((y + index * 7) * 0.47f) * 2f);
                var reach = maximumReach + row / 4 + weathering;
                if (leftSide)
                {
                    reach = Math.Min(reach, routeX - 7);
                    for (var x = 0; x <= reach; x++)
                        if (!protectedTiles.Contains(new Point(x, y)))
                            SetGeologicalTile(world, config.Seed, variant, x, y, row);
                }
                else
                {
                    var start = Math.Max(world.Width - 1 - reach, routeX + 7);
                    for (var x = start; x < world.Width; x++)
                        if (!protectedTiles.Contains(new Point(x, y)))
                            SetGeologicalTile(world, config.Seed, variant, x, y, row);
                }
            }
        }
    }

    private static void BuildGroundedOutcrops(TileWorld world, WorldGenerationConfig config, GeneratorVariant variant,
        DeterministicRandom random, IReadOnlyList<Point> route, HashSet<Point> protectedTiles)
    {
        var count = variant == GeneratorVariant.Cellular ? 34 : variant == GeneratorVariant.Layered ? 18 : 9;
        for (var index = 0; index < count; index++)
        {
            var reference = route[random.Range(2, route.Count - 2)];
            var x = Math.Clamp(reference.X + (random.Chance(0.5f) ? -1 : 1) * random.Range(6, 18), 3, world.Width - 4);
            if (!TryFindSurface(world, x, reference.Y + random.Range(-8, 10), out var surfaceY)) continue;
            var height = random.Range(3, variant == GeneratorVariant.Cellular ? 11 : 8);
            var baseHalfWidth = random.Range(1, variant == GeneratorVariant.Cellular ? 4 : 3);
            for (var rise = 1; rise <= height; rise++)
            {
                var halfWidth = Math.Max(0, baseHalfWidth - rise * baseHalfWidth / Math.Max(1, height));
                var lean = (int)MathF.Round(MathF.Sin((index * 5 + rise) * 0.8f));
                for (var column = -halfWidth; column <= halfWidth; column++)
                {
                    var tile = new Point(x + column + lean, surfaceY - rise);
                    if (protectedTiles.Contains(tile)) continue;
                    SetGeologicalTile(world, config.Seed, variant, tile.X, tile.Y, height - rise);
                }
            }
        }
    }

    private static void BuildArches(TileWorld world, WorldGenerationConfig config, GeneratorVariant variant,
        DeterministicRandom random, IReadOnlyList<Point> route, HashSet<Point> protectedTiles)
    {
        var count = variant == GeneratorVariant.Layered ? 7 : variant == GeneratorVariant.Heightmap ? 4 : 3;
        for (var index = 0; index < count; index++)
        {
            var anchor = route[Math.Clamp(6 + index * route.Count / Math.Max(1, count + 1), 3, route.Count - 4)];
            var halfWidth = random.Range(3, 6);
            var height = random.Range(5, 9);
            var top = anchor.Y - height;
            for (var rise = 1; rise < height; rise++)
            {
                PlaceFormationTile(world, config.Seed, variant, anchor.X - halfWidth, anchor.Y - rise,
                    height - rise, protectedTiles, MaterialId.FossilComposite);
                PlaceFormationTile(world, config.Seed, variant, anchor.X + halfWidth, anchor.Y - rise,
                    height - rise, protectedTiles, MaterialId.FossilComposite);
            }
            for (var x = anchor.X - halfWidth; x <= anchor.X + halfWidth; x++)
                for (var thickness = 0; thickness < 2; thickness++)
                    PlaceFormationTile(world, config.Seed, variant, x, top + thickness, thickness,
                        protectedTiles, MaterialId.FossilComposite);
        }
    }

    private static void CarveFractures(TileWorld world, WorldGenerationConfig config, GeneratorVariant variant,
        DeterministicRandom random, HashSet<Point> protectedTiles)
    {
        var count = variant == GeneratorVariant.Cellular ? 34 : variant == GeneratorVariant.Layered ? 20 : 9;
        for (var index = 0; index < count; index++)
        {
            var x = random.Range(3, world.Width - 3);
            var y = random.Range(8, world.Height - 12);
            var length = random.Range(4, variant == GeneratorVariant.Cellular ? 15 : 11);
            var drift = random.Chance(0.5f) ? -1 : 1;
            for (var step = 0; step < length; step++)
            {
                if (step > 0 && random.Chance(0.38f)) x += drift;
                if (random.Chance(0.12f)) drift *= -1;
                var tile = new Point(x, y + step);
                if (world.Contains(tile.X, tile.Y) && !protectedTiles.Contains(tile))
                    world.SetTile(tile.X, tile.Y, MaterialId.Air);
            }
        }
    }

    private static void ApplyErosion(TileWorld world, WorldGenerationConfig config, GeneratorVariant variant,
        HashSet<Point> protectedTiles)
    {
        var strength = config.Parameters.Erosion * (variant == GeneratorVariant.Cellular ? 1.25f :
            variant == GeneratorVariant.Heightmap ? 0.7f : 1f);
        var passes = variant == GeneratorVariant.Cellular ? 3 : 2;
        for (var pass = 0; pass < passes; pass++)
        {
            var removals = new List<Point>();
            for (var y = 2; y < world.Height - 6; y++)
                for (var x = 1; x < world.Width - 1; x++)
                {
                    var point = new Point(x, y);
                    if (!world.GetTile(x, y).Solid || protectedTiles.Contains(point)) continue;
                    var neighbors = 0;
                    if (world.GetTile(x - 1, y).Solid) neighbors++;
                    if (world.GetTile(x + 1, y).Solid) neighbors++;
                    if (world.GetTile(x, y - 1).Solid) neighbors++;
                    if (world.GetTile(x, y + 1).Solid) neighbors++;
                    if (neighbors >= 4) continue;
                    var threshold = strength * (neighbors <= 1 ? 0.42f : neighbors == 2 ? 0.17f : 0.045f);
                    if (DeterministicRandom.Hash01(config.Seed, x, y, 97 + pass) < threshold) removals.Add(point);
                }
            foreach (var point in removals) world.SetTile(point.X, point.Y, MaterialId.Air);
        }
    }

    private static void ProtectTravel(IReadOnlyList<Point> anchors, HashSet<Point> protectedTiles, bool primary)
    {
        foreach (var anchor in anchors)
        {
            var landingHalfWidth = primary ? 2 : 1;
            for (var x = anchor.X - landingHalfWidth; x <= anchor.X + landingHalfWidth; x++)
                protectedTiles.Add(new Point(x, anchor.Y));
            for (var x = anchor.X - 1; x <= anchor.X + 1; x++)
                for (var y = anchor.Y - 2; y < anchor.Y; y++) protectedTiles.Add(new Point(x, y));
        }
    }

    private static void RestoreTravelSurfaces(TileWorld world, WorldGenerationConfig config, GeneratorVariant variant,
        IReadOnlyList<Point> anchors, bool primary)
    {
        foreach (var anchor in anchors)
            for (var x = anchor.X - 1; x <= anchor.X + 1; x++)
                for (var y = anchor.Y - 2; y < anchor.Y; y++) world.SetTile(x, y, MaterialId.Air);

        for (var index = 0; index < anchors.Count; index++)
        {
            var anchor = anchors[index];
            var altitude = 1f - anchor.Y / (float)world.Height;
            var landingHalfWidth = primary ? altitude < 0.34f ? 3 : altitude < 0.67f ? 2 : 1 : 2;
            if (variant == GeneratorVariant.Heightmap && primary) landingHalfWidth++;
            var material = RouteSurfaceMaterial(config.Seed, anchor, altitude);
            for (var x = anchor.X - landingHalfWidth; x <= anchor.X + landingHalfWidth; x++)
                world.SetTile(x, anchor.Y, material);
        }
    }

    private static void ClearStandingCells(TileWorld world, IReadOnlyList<Point> anchors)
    {
        foreach (var anchor in anchors) world.SetTile(anchor.X, anchor.Y - 1, MaterialId.Air);
    }

    private static List<WorldFeature> PlaceSurfaceFeatures(TileWorld world, WorldGenerationConfig config,
        GeneratorVariant variant, DeterministicRandom random, List<Point> route, List<List<Point>> detours,
        int cultureCount)
    {
        var features = new List<WorldFeature>();
        for (var index = 4; index < route.Count - 3; index++)
        {
            var anchor = route[index];
            var altitude = 1f - anchor.Y / (float)world.Height;
            var ruinChance = config.Parameters.RuinDensity * (variant == GeneratorVariant.Layered ? 1.7f :
                variant == GeneratorVariant.Heightmap ? 0.7f : 0.9f);
            if (random.Chance(ruinChance))
            {
                var provenance = (short)random.Range(1, cultureCount + 1);
                var ruinAnchor = new Point(Math.Clamp(anchor.X + (index % 2 == 0 ? 3 : -3), 2, world.Width - 3), anchor.Y);
                EmbedRuinFragment(world, config.Seed, ruinAnchor, index % 4, provenance);
                features.Add(new WorldFeature(index % 4 == 0 ? WorldFeatureKind.ExposedMachine : WorldFeatureKind.Ruin,
                    FootPosition(ruinAnchor), random.Range(0, 4), provenance));
            }
            if (altitude > 0.16f && random.Chance(config.Parameters.EcologyDensity * (0.45f + altitude)))
            {
                var ecologyAnchor = new Point(Math.Clamp(anchor.X + (index % 2 == 0 ? -2 : 2), 1, world.Width - 2), anchor.Y);
                MarkSurfaceFlag(world, ecologyAnchor, TileFlags.Ecology, 0);
                features.Add(new WorldFeature(WorldFeatureKind.Ecology, FootPosition(ecologyAnchor), random.Range(0, 3)));
            }
            if (altitude > 0.28f && index % (altitude > 0.67f ? 13 : 19) == 9)
                features.Add(new WorldFeature(WorldFeatureKind.BurrowerSpawn, FootPosition(anchor), 0));
        }

        for (var index = 0; index < detours.Count; index++)
        {
            var detour = detours[index];
            var relicAnchor = detour[detour.Count / 2];
            features.Add(new WorldFeature(WorldFeatureKind.RelicCandidate, FootPosition(relicAnchor), index % 6,
                (short)random.Range(1, cultureCount + 1)));
            var brittleAnchor = detour[Math.Clamp(detour.Count * 2 / 3, 1, detour.Count - 2)];
            var altitude = 1f - brittleAnchor.Y / (float)world.Height;
            if (altitude < 0.28f) continue;
            for (var x = brittleAnchor.X - 2; x <= brittleAnchor.X + 2; x++)
                world.SetTile(x, brittleAnchor.Y, MaterialId.BrittleMasonry);
            features.Add(new WorldFeature(WorldFeatureKind.BrittleSite,
                new Vector2((brittleAnchor.X + 0.5f) * GameConstants.TileSize,
                    brittleAnchor.Y * GameConstants.TileSize), index % 4));
        }

        if (features.TrueForAll(feature => feature.Kind != WorldFeatureKind.BurrowerSpawn))
        {
            var anchor = route[route.Count * 2 / 3];
            features.Add(new WorldFeature(WorldFeatureKind.BurrowerSpawn, FootPosition(anchor), 0));
        }
        return features;
    }

    private static void EmbedRuinFragment(TileWorld world, long seed, Point anchor, int style, short provenance)
    {
        switch (style)
        {
            case 0:
                for (var depth = 0; depth < 5; depth++) SetRuinTile(world, anchor.X, anchor.Y + depth, provenance);
                for (var rise = 1; rise <= 2; rise++)
                    if (DeterministicRandom.Hash01(seed, anchor.X, anchor.Y - rise, 109) > 0.32f)
                        SetRuinTile(world, anchor.X, anchor.Y - rise, provenance);
                break;
            case 1:
                for (var x = anchor.X - 4; x <= anchor.X + 1; x++)
                    if (x != anchor.X - 1) SetRuinTile(world, x, anchor.Y, provenance);
                SetRuinTile(world, anchor.X - 4, anchor.Y - 1, provenance);
                break;
            case 2:
                for (var x = anchor.X - 1; x <= anchor.X + 1; x++)
                    for (var depth = 0; depth < 3; depth++) SetRuinTile(world, x, anchor.Y + depth, provenance);
                break;
            default:
                for (var x = anchor.X - 3; x <= anchor.X + 3; x++)
                    if ((x + anchor.Y) % 3 != 0) SetRuinTile(world, x, anchor.Y + 2, provenance);
                break;
        }
    }

    private static void SetRuinTile(TileWorld world, int x, int y, short provenance)
    {
        if (!world.Contains(x, y)) return;
        world.SetTile(x, y, MaterialId.RuinAlloy, TileFlags.RuinTrace, provenance);
    }

    private static void MarkSurfaceFlag(TileWorld world, Point point, TileFlags flag, short provenance)
    {
        var tile = world.GetTile(point.X, point.Y);
        if (!tile.Solid) return;
        tile.Flags |= flag;
        tile.ProvenanceId = provenance;
        world.SetTile(point.X, point.Y, tile);
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

    private static MaterialId RouteSurfaceMaterial(long seed, Point anchor, float altitude)
    {
        var variation = DeterministicRandom.Hash01(seed, anchor.X / 4, anchor.Y / 6, 113);
        if (altitude < 0.3f) return variation < 0.55f ? MaterialId.Loess : MaterialId.RedSandstone;
        if (altitude < 0.68f) return variation < 0.7f ? MaterialId.RedSandstone : MaterialId.FossilComposite;
        return variation < 0.52f ? MaterialId.FossilComposite : MaterialId.BlackBasalt;
    }

    private static void SetGeologicalTile(TileWorld world, long seed, GeneratorVariant variant, int x, int y, int depth)
    {
        if (!world.Contains(x, y)) return;
        var altitude = 1f - y / (float)world.Height;
        var bandThickness = variant == GeneratorVariant.Heightmap ? 14 : variant == GeneratorVariant.Layered ? 10 : 8;
        var warp = (int)(DeterministicRandom.Hash(seed, x / (variant == GeneratorVariant.Cellular ? 3 : 7), 0, 3) % 3);
        var band = Math.Abs(y / bandThickness + warp) % 6;
        MaterialId material;
        if (variant == GeneratorVariant.Cellular)
        {
            var cell = DeterministicRandom.Hash01(seed, x / 3, y / 3, 127);
            material = cell < 0.14f && altitude > 0.38f ? MaterialId.SaltGlass :
                cell < 0.35f ? MaterialId.FossilComposite :
                cell > 0.82f ? MaterialId.BlackBasalt : MaterialId.RedSandstone;
        }
        else
        {
            material = band switch
            {
                0 or 1 => MaterialId.RedSandstone,
                2 => altitude > 0.58f ? MaterialId.SaltGlass : MaterialId.FossilComposite,
                3 or 4 => MaterialId.BlackBasalt,
                _ => MaterialId.FossilComposite
            };
        }
        if (depth == 0 && altitude < 0.46f) material = MaterialId.Loess;
        world.SetTile(x, y, material);
    }

    private static void PlaceFormationTile(TileWorld world, long seed, GeneratorVariant variant, int x, int y,
        int depth, HashSet<Point> protectedTiles, MaterialId? material = null)
    {
        var point = new Point(x, y);
        if (!world.Contains(x, y) || protectedTiles.Contains(point)) return;
        if (material.HasValue) world.SetTile(x, y, material.Value);
        else SetGeologicalTile(world, seed, variant, x, y, depth);
    }

    private static bool TryFindSurface(TileWorld world, int x, int preferredY, out int surfaceY)
    {
        for (var distance = 0; distance <= 18; distance++)
        {
            var above = preferredY - distance;
            if (above > 1 && above < world.Height && world.GetTile(x, above).Solid && !world.GetTile(x, above - 1).Solid)
            {
                surfaceY = above;
                return true;
            }
            var below = preferredY + distance;
            if (below > 1 && below < world.Height && world.GetTile(x, below).Solid && !world.GetTile(x, below - 1).Solid)
            {
                surfaceY = below;
                return true;
            }
        }
        surfaceY = 0;
        return false;
    }

    private static int NearestRouteX(IReadOnlyList<Point> route, int y)
    {
        var nearest = route[0];
        var distance = Math.Abs(nearest.Y - y);
        for (var index = 1; index < route.Count; index++)
        {
            var candidateDistance = Math.Abs(route[index].Y - y);
            if (candidateDistance >= distance) continue;
            nearest = route[index];
            distance = candidateDistance;
        }
        return nearest.X;
    }

    private static Vector2 FootPosition(Point anchor) => new((anchor.X + 0.5f) * GameConstants.TileSize,
        anchor.Y * GameConstants.TileSize);

    private static void AddDistinct(List<Point> points, Point point)
    {
        if (points.Count == 0 || points[^1] != point) points.Add(point);
    }

    private static int AvoidVerticalConflict(IReadOnlyList<Point> route, int x, int y, int direction, int worldWidth)
    {
        for (var attempt = 0; attempt < 7; attempt++)
        {
            var conflict = false;
            for (var index = 0; index < route.Count; index++)
                if (route[index].X == x && Math.Abs(route[index].Y - y) <= 1)
                {
                    conflict = true;
                    break;
                }
            if (!conflict) return x;
            x = Math.Clamp(x + direction, 9, worldWidth - 10);
            if (x is 9 || x == worldWidth - 10) direction *= -1;
        }
        return x;
    }

    private static WorldGenerationConfig Clone(WorldGenerationConfig source, GeneratorVariant variant) => new()
    {
        Seed = source.Seed,
        GeneratorVersion = WorldGeneratorRegistry.CurrentVersion,
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
