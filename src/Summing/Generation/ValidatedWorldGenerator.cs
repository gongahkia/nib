using System;
using System.Collections.Generic;

namespace Summing.Generation;

public sealed record GenerationDiagnostics(IReadOnlyList<long> RejectedSeeds, TraversabilityReport Traversability);

public static class ValidatedWorldGenerator
{
    public static GeneratedWorld Generate(WorldGenerationConfig requested, int maximumAttempts = 32)
    {
        var rejected = new List<long>();
        var candidateConfig = Copy(requested);
        for (var attempt = 0; attempt < maximumAttempts; attempt++)
        {
            var candidate = WorldGeneratorRegistry.Generate(candidateConfig);
            var report = TraversabilityValidator.Validate(candidate);
            if (report.Solvable)
            {
                candidate.Diagnostics = new GenerationDiagnostics(rejected, report);
                return candidate;
            }
            rejected.Add(candidateConfig.Seed);
            candidateConfig.Seed = unchecked(candidateConfig.Seed + (long)0x61c8864680b583ebUL);
        }
        throw new InvalidOperationException($"failed to generate a solvable world after {maximumAttempts} complete candidates");
    }

    private static WorldGenerationConfig Copy(WorldGenerationConfig source) => new()
    {
        Seed = source.Seed,
        GeneratorVersion = source.GeneratorVersion,
        Variant = source.Variant,
        Parameters = new BadlandsParameters
        {
            Width = source.Parameters.Width, Height = source.Parameters.Height, Erosion = source.Parameters.Erosion,
            RuinDensity = source.Parameters.RuinDensity, EcologyDensity = source.Parameters.EcologyDensity,
            WindStrength = source.Parameters.WindStrength
        }
    };
}
