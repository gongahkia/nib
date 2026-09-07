using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Summing.Entities;
using Summing.Diagnostics;
using Summing.Gameplay;
using Summing.Generation;
using Summing.History;
using Summing.Persistence;
using Summing.Player;
using Summing.Rendering;

if (Environment.GetCommandLineArgs().Contains("--verify-generation"))
{
    var quality = new Dictionary<GeneratorVariant, (ulong Fingerprint, int SolidTiles, int Ruins)>();
    foreach (var variant in Enum.GetValues<GeneratorVariant>())
    {
        var configuration = new WorldGenerationConfig { Seed = 9042026, Variant = variant };
        var firstWorld = WorldGeneratorRegistry.Generate(configuration);
        var firstHash = firstWorld.Terrain.Fingerprint();
        var secondHash = WorldGeneratorRegistry.Generate(configuration).Terrain.Fingerprint();
        var rawReport = TraversabilityValidator.Validate(firstWorld);
        var solidTiles = 0;
        for (var y = 0; y < firstWorld.Terrain.Height; y++)
            for (var x = 0; x < firstWorld.Terrain.Width; x++)
                if (firstWorld.Terrain.GetTile(x, y).Solid) solidTiles++;
        var ruinCount = firstWorld.Features.Count(feature =>
            feature.Kind is WorldFeatureKind.Ruin or WorldFeatureKind.ExposedMachine);
        var descents = firstWorld.RouteAnchors.Zip(firstWorld.RouteAnchors.Skip(1),
            (current, next) => next.Y > current.Y).Count(value => value);
        var lateral = firstWorld.RouteAnchors.Zip(firstWorld.RouteAnchors.Skip(1),
            (current, next) => Math.Abs(next.Y - current.Y) <= 1 && Math.Abs(next.X - current.X) >= 3).Count(value => value);
        quality[variant] = (firstHash, solidTiles, ruinCount);
        Console.WriteLine($"raw variant={variant} solvable={rawReport.Solvable} failure={rawReport.Failure}");
        Console.WriteLine($"shape variant={variant} solid={solidTiles} ruins={ruinCount} descents={descents} lateral={lateral}");
        var validated = ValidatedWorldGenerator.Generate(configuration);
        Console.WriteLine($"seed={configuration.Seed} variant={configuration.Variant} fingerprint={firstHash:x16} " +
            $"route={validated.Diagnostics.Traversability.CheckedTransitions} rejected={validated.Diagnostics.RejectedSeeds.Count}");
        if (firstHash != secondHash) throw new InvalidOperationException("generation is not deterministic");

        var rejectedCandidates = 0;
        for (var sample = 0; sample < 32; sample++)
        {
            var sampleSeed = unchecked(9042026L + sample * (long)0x61c8864680b583ebUL);
            var sampleConfiguration = new WorldGenerationConfig { Seed = sampleSeed, Variant = variant };
            var firstValidated = ValidatedWorldGenerator.Generate(sampleConfiguration);
            var secondValidated = ValidatedWorldGenerator.Generate(sampleConfiguration);
            if (!firstValidated.Diagnostics.Traversability.Solvable)
                throw new InvalidOperationException($"validated {variant} seed {sampleSeed} is not solvable");
            if (firstValidated.Configuration.Seed != secondValidated.Configuration.Seed ||
                firstValidated.Terrain.Fingerprint() != secondValidated.Terrain.Fingerprint())
                throw new InvalidOperationException($"validated {variant} seed {sampleSeed} is not deterministic");
            rejectedCandidates += firstValidated.Diagnostics.RejectedSeeds.Count;
        }
        Console.WriteLine($"sweep variant={variant} seeds=32 deterministic=True solvable=True rejected={rejectedCandidates}");
    }
    if (quality.Values.Select(item => item.Fingerprint).Distinct().Count() != quality.Count)
        throw new InvalidOperationException("generator variants produced identical authoritative worlds");
    if (quality[GeneratorVariant.Heightmap].SolidTiles <= quality[GeneratorVariant.Cellular].SolidTiles * 1.2f)
        throw new InvalidOperationException("heightmap variant lacks its broad-landform density distinction");
    if (quality[GeneratorVariant.Layered].Ruins <= Math.Max(quality[GeneratorVariant.Heightmap].Ruins,
            quality[GeneratorVariant.Cellular].Ruins))
        throw new InvalidOperationException("layered variant lacks its ruin-provenance distinction");
    foreach (var preset in DevelopmentSeedPresets.All)
    {
        var presetWorld = WorldGeneratorRegistry.Generate(new WorldGenerationConfig
        {
            Seed = preset.Seed,
            Variant = preset.Variant
        });
        var report = TraversabilityValidator.Validate(presetWorld);
        Console.WriteLine($"preset={preset.Name} seed={preset.Seed} variant={preset.Variant} solvable={report.Solvable}");
        if (!report.Solvable)
            throw new InvalidOperationException($"development preset {preset.Name} is not directly solvable: {report.Failure}");
    }
    return;
}

if (Environment.GetCommandLineArgs().Contains("--verify-serialization"))
{
    var generated = WorldGeneratorRegistry.Generate(new WorldGenerationConfig());
    generated.Terrain.SetTile(1, 1, Summing.World.Materials.MaterialId.BlackBasalt);
    generated.Terrain.DamageTile(1, 1, 2, "serialization-verification");
    var tuning = DifficultyTuning.For(Difficulty.Easy);
    var player = new PlayerController(generated.Spawn, tuning.StartingHealth);
    var inventory = new PlayerInventory(tuning);
    var archive = new ArchiveStore(Path.Combine(Path.GetTempPath(), "summing-verification-archive.json"));
    var burrowers = new BurrowerSystem(generated, tuning);
    var brittle = new BrittleSystem(generated, tuning);
    var relics = new RelicSystem(generated, archive);
    var ropes = new RopeSystem();
    var bombs = new BombSystem();
    var path = Path.GetTempFileName();
    try
    {
        WorldSerializer.Save(path, generated, player, inventory, burrowers, brittle, relics, ropes, bombs,
            Difficulty.Easy, 0, new List<string>());
        var loaded = WorldSerializer.Load(path);
        if (loaded.Terrain.Fingerprint() != generated.Terrain.Fingerprint() || loaded.History.Id != generated.History.Id)
            throw new InvalidDataException("world JSON round-trip changed authoritative data");
        if (loaded.Terrain.GetTile(1, 1).Damage != 2)
            throw new InvalidDataException("world JSON round-trip lost mutable tile damage");
        if (loaded.Configuration.Seed != generated.Configuration.Seed ||
            loaded.Configuration.Variant != generated.Configuration.Variant ||
            loaded.Configuration.GeneratorVersion != generated.Configuration.GeneratorVersion ||
            loaded.Spawn != generated.Spawn || loaded.SummitBounds != generated.SummitBounds ||
            !loaded.RouteAnchors.SequenceEqual(generated.RouteAnchors) ||
            loaded.Features.Count != generated.Features.Count || loaded.Weather.Count != generated.Weather.Count)
            throw new InvalidDataException("world JSON round-trip changed generation, traversal, or placement data");
        Console.WriteLine($"roundtrip=ok bytes={new FileInfo(path).Length} fingerprint={loaded.Terrain.Fingerprint():x16}");
    }
    finally
    {
        File.Delete(path);
    }
    return;
}

if (Environment.GetCommandLineArgs().Contains("--verify-systems"))
{
    HeadlessVerification.RunSystems();
    return;
}

var arguments = Environment.GetCommandLineArgs();
var smokeRun = arguments.Contains("--smoke-run");
var smokePeriodic = arguments.Contains("--smoke-periodic");
var smokeTitle = arguments.Contains("--smoke-title");
var initialConfiguration = new WorldGenerationConfig();
var initialDifficulty = Difficulty.Easy;
var initialVisualPack = VisualPackId.GandalfOverworld;
foreach (var argument in arguments)
{
    if (argument.StartsWith("--variant=", StringComparison.OrdinalIgnoreCase) &&
        Enum.TryParse<GeneratorVariant>(argument[10..], true, out var variant))
        initialConfiguration.Variant = variant;
    if (argument.StartsWith("--seed=", StringComparison.OrdinalIgnoreCase) &&
        long.TryParse(argument[7..], out var seed)) initialConfiguration.Seed = seed;
    if (argument.StartsWith("--difficulty=", StringComparison.OrdinalIgnoreCase) &&
        Enum.TryParse<Difficulty>(argument[13..], true, out var difficulty)) initialDifficulty = difficulty;
    if (argument.StartsWith("--art-pack=", StringComparison.OrdinalIgnoreCase) &&
        Enum.TryParse<VisualPackId>(argument[11..], true, out var visualPack)) initialVisualPack = visualPack;
}
using var game = new Summing.Game1(smokeRun ? 240 : smokePeriodic ? 620 : smokeTitle ? 30 : 0,
    smokeRun || smokePeriodic, smokeTitle, initialConfiguration, initialDifficulty, initialVisualPack);
game.Run();
