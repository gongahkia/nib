using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Summing.Entities;
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
        var firstHash = WorldGeneratorRegistry.Generate(configuration).Terrain.Fingerprint();
        var secondHash = WorldGeneratorRegistry.Generate(configuration).Terrain.Fingerprint();
        Console.WriteLine($"seed={configuration.Seed} variant={configuration.Variant} fingerprint={firstHash:x16}");
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

using var game = new Summing.Game1();
game.Run();
