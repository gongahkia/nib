using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Xna.Framework;
using Summing.Core;
using Summing.Entities;
using Summing.Gameplay;
using Summing.Generation;
using Summing.Player;
using Summing.World;
using Summing.World.Materials;

namespace Summing.Persistence;

public static class WorldSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static void Save(string path, GeneratedWorld generated, PlayerController player, PlayerInventory inventory,
        BurrowerSystem burrowers, BrittleSystem brittle, RelicSystem relics, RopeSystem ropes, BombSystem bombs,
        Difficulty difficulty, long frame, IReadOnlyList<string> terrainEvents)
    {
        var snapshot = Capture(generated, player, inventory, burrowers, brittle, relics, ropes, bombs, difficulty, frame, terrainEvents);
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
        var temporary = path + ".tmp";
        File.WriteAllText(temporary, JsonSerializer.Serialize(snapshot, Options));
        File.Move(temporary, path, true);
    }

    public static GeneratedWorld Load(string path)
    {
        var snapshot = JsonSerializer.Deserialize<WorldSnapshot>(File.ReadAllText(path), Options) ??
            throw new InvalidDataException("world snapshot is empty");
        if (snapshot.FormatVersion != 1) throw new InvalidDataException($"unsupported world snapshot version {snapshot.FormatVersion}");
        var world = new TileWorld(snapshot.World.Width, snapshot.World.Height);
        foreach (var chunk in snapshot.World.Chunks)
        foreach (var tile in chunk.Tiles)
        {
            var x = chunk.X * snapshot.World.ChunkSize + tile.LocalX;
            var y = chunk.Y * snapshot.World.ChunkSize + tile.LocalY;
            world.SetTile(x, y, new TerrainTile
            {
                Material = tile.Material,
                Damage = tile.Damage,
                Flags = tile.Flags,
                ProvenanceId = tile.ProvenanceId
            });
        }
        return new GeneratedWorld
        {
            Configuration = snapshot.Generator,
            Terrain = world,
            Spawn = new Vector2(snapshot.Spawn.X, snapshot.Spawn.Y),
            SummitBounds = new Rectangle(snapshot.Summit.X, snapshot.Summit.Y, snapshot.Summit.Width, snapshot.Summit.Height),
            RouteAnchors = snapshot.RouteAnchors.ConvertAll(point => new Point(point.X, point.Y)),
            Features = snapshot.Features.ConvertAll(feature => new WorldFeature(feature.Kind,
                new Vector2(feature.X, feature.Y), feature.Variant, feature.ProvenanceId)),
            Weather = snapshot.Weather,
            History = snapshot.History
        };
    }

    private static WorldSnapshot Capture(GeneratedWorld generated, PlayerController player, PlayerInventory inventory,
        BurrowerSystem burrowers, BrittleSystem brittle, RelicSystem relics, RopeSystem ropes, BombSystem bombs,
        Difficulty difficulty, long frame, IReadOnlyList<string> terrainEvents)
    {
        var chunks = new List<ChunkSnapshot>();
        foreach (var (coordinate, chunk) in generated.Terrain.Chunks)
        {
            var tiles = new List<TileSnapshot>();
            for (var localY = 0; localY < GameConstants.ChunkSize; localY++)
            for (var localX = 0; localX < GameConstants.ChunkSize; localX++)
            {
                var tile = chunk.Get(localX, localY);
                tiles.Add(new TileSnapshot(localX, localY, tile.Material, tile.Damage, tile.Flags, tile.ProvenanceId));
            }
            chunks.Add(new ChunkSnapshot(coordinate.X, coordinate.Y, chunk.Revision, tiles));
        }

        var entities = new List<EntitySnapshot>();
        var index = 0;
        foreach (var burrower in burrowers.Burrowers)
            entities.Add(new EntitySnapshot($"burrower-{index++}", "burrower", burrower.Position.X, burrower.Position.Y,
                burrower.Alerted ? "alerted" : "dormant", new() { ["health"] = burrower.Health.ToString() }));
        index = 0;
        foreach (var relic in relics.Relics)
            entities.Add(new EntitySnapshot(relic.Id, "relic", relic.Position.X, relic.Position.Y,
                relic.Collected ? "collected" : "uncollected", new() { ["culture"] = relic.Culture.Name, ["event"] = relic.HistoryEvent.Id }));
        index = 0;
        foreach (var rope in ropes.Ropes)
            entities.Add(new EntitySnapshot($"rope-{index++}", "placed-rope", rope.X, rope.Top, "placed",
                new() { ["bottom"] = rope.Bottom.ToString("0.###") }));
        index = 0;
        foreach (var bomb in bombs.Bombs)
            entities.Add(new EntitySnapshot($"bomb-{index++}", "bomb", bomb.Position.X, bomb.Position.Y, "fuse",
                new() { ["fuse"] = bomb.Fuse.ToString("0.###") }));
        index = 0;
        foreach (var structure in brittle.Structures)
        {
            var tile = structure.Tiles.Count > 0 ? structure.Tiles[0] : Point.Zero;
            entities.Add(new EntitySnapshot($"brittle-{index++}", "brittle-structure",
                tile.X * GameConstants.TileSize, tile.Y * GameConstants.TileSize,
                structure.Collapsed ? "collapsed" : structure.Triggered ? "triggered" : "stable",
                new() { ["remaining"] = structure.Remaining.ToString("0.###") }));
        }

        var playerEntity = new EntitySnapshot("player", "player", player.Position.X, player.Position.Y,
            player.VisualState.ToString(), new()
            {
                ["velocityX"] = player.Velocity.X.ToString("0.###"), ["velocityY"] = player.Velocity.Y.ToString("0.###"),
                ["health"] = player.Health.ToString(), ["grounded"] = player.Grounded.ToString(),
                ["wallStamina"] = player.WallStamina.ToString("0.###"), ["dashCharges"] = player.DashCharges.ToString(),
                ["grappleAttached"] = player.GrappleAttached.ToString()
            });

        var materialSnapshots = new List<MaterialSnapshot>();
        foreach (var material in MaterialCatalog.All)
            materialSnapshots.Add(new MaterialSnapshot(material.Id, material.Name, material.Solid, material.Hardness,
                material.Friction, material.Brittleness, material.Heat, material.Conductivity, material.Climbable,
                material.GrappleCompatible, material.Structural));
        return new WorldSnapshot
        {
            SavedAtUtc = DateTimeOffset.UtcNow,
            Generator = generated.Configuration,
            World = new WorldGridSnapshot(generated.Terrain.Width, generated.Terrain.Height, GameConstants.ChunkSize,
                GameConstants.TileSize, chunks),
            Materials = materialSnapshots,
            Spawn = new PositionSnapshot(generated.Spawn.X, generated.Spawn.Y),
            Summit = new RectangleSnapshot(generated.SummitBounds.X, generated.SummitBounds.Y,
                generated.SummitBounds.Width, generated.SummitBounds.Height),
            RouteAnchors = generated.RouteAnchors.ConvertAll(point => new PointSnapshot(point.X, point.Y)),
            Features = generated.Features.ConvertAll(feature => new FeatureSnapshot(feature.Kind,
                feature.Position.X, feature.Position.Y, feature.Variant, feature.ProvenanceId)),
            Weather = generated.Weather,
            History = generated.History,
            Runtime = new RuntimeSnapshot(frame, difficulty.ToString(), playerEntity, inventory.Bombs, inventory.Ropes,
                entities, new List<string>(terrainEvents))
        };
    }
}
