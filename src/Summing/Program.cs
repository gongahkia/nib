using System;
using System.Linq;
using Summing.Generation;

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

using var game = new Summing.Game1();
game.Run();
