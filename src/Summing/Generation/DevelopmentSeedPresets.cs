using System;
using System.Collections.Generic;

namespace Summing.Generation;

public sealed record DevelopmentSeedPreset(string Name, long Seed, GeneratorVariant Variant);

public static class DevelopmentSeedPresets
{
    public static readonly IReadOnlyList<DevelopmentSeedPreset> All =
    [
        new("BROAD ESCARPMENTS", 9042026, GeneratorVariant.Heightmap),
        new("FRACTURED NEEDLES", 27182818, GeneratorVariant.Cellular),
        new("BURIED PROCESSIONAL", 16180339, GeneratorVariant.Layered)
    ];

    public static int Find(WorldGenerationConfig configuration)
    {
        for (var index = 0; index < All.Count; index++)
            if (All[index].Seed == configuration.Seed && All[index].Variant == configuration.Variant) return index;
        return -1;
    }

    public static int Next(WorldGenerationConfig configuration) => (Find(configuration) + 1) % All.Count;

    public static void Apply(WorldGenerationConfig configuration, int index)
    {
        var preset = All[Math.Clamp(index, 0, All.Count - 1)];
        configuration.Seed = preset.Seed;
        configuration.Variant = preset.Variant;
    }

    public static string Label(WorldGenerationConfig configuration)
    {
        var index = Find(configuration);
        return index >= 0 ? $"{index + 1} {All[index].Name}" : "CUSTOM";
    }
}
