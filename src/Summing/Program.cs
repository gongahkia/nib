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

if (Environment.GetCommandLineArgs().Contains("--verify-generation"))
{
    foreach (var variant in Enum.GetValues<GeneratorVariant>())
    {
        var configuration = new WorldGenerationConfig { Seed = 9042026, Variant = variant };
        var firstWorld = WorldGeneratorRegistry.Generate(configuration);
        var firstHash = firstWorld.Terrain.Fingerprint();
        var secondHash = WorldGeneratorRegistry.Generate(configuration).Terrain.Fingerprint();
        var rawReport = TraversabilityValidator.Validate(firstWorld);
        Console.WriteLine($"raw variant={variant} solvable={rawReport.Solvable} failure={rawReport.Failure}");
        var validated = ValidatedWorldGenerator.Generate(configuration);
        Console.WriteLine($"seed={configuration.Seed} variant={configuration.Variant} fingerprint={firstHash:x16} " +
            $"route={validated.Diagnostics.Traversability.CheckedTransitions} rejected={validated.Diagnostics.RejectedSeeds.Count}");
        if (firstHash != secondHash) throw new InvalidOperationException("generation is not deterministic");
    }
    return;
}

if (Environment.GetCommandLineArgs().Contains("--verify-serialization"))
{
    var generated = WorldGeneratorRegistry.Generate(new WorldGenerationConfig());
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

var smokeRun = Environment.GetCommandLineArgs().Contains("--smoke-run");
var smokeTitle = Environment.GetCommandLineArgs().Contains("--smoke-title");
using var game = new Summing.Game1(smokeRun ? 240 : smokeTitle ? 30 : 0, smokeRun, smokeTitle);
game.Run();
