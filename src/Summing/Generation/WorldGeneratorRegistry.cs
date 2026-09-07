using System.Collections.Generic;

namespace Summing.Generation;

public static class WorldGeneratorRegistry
{
    public const string CurrentVersion = "badlands-v3";
    private static readonly IReadOnlyDictionary<GeneratorVariant, IBiomeGenerator> Generators =
        new Dictionary<GeneratorVariant, IBiomeGenerator>
        {
            [GeneratorVariant.Heightmap] = new HeightmapBadlandsGenerator(),
            [GeneratorVariant.Cellular] = new CellularBadlandsGenerator(),
            [GeneratorVariant.Layered] = new LayeredBadlandsGenerator()
        };

    public static GeneratedWorld Generate(WorldGenerationConfig configuration) => Generators[configuration.Variant].Generate(configuration);
    public static string Identifier(GeneratorVariant variant) => Generators[variant].Id;
}
