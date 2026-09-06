namespace Summing.Generation;

public interface IBiomeGenerator
{
    string Id { get; }
    GeneratorVariant Variant { get; }
    GeneratedWorld Generate(WorldGenerationConfig configuration);
}

public sealed class HeightmapBadlandsGenerator : IBiomeGenerator
{
    public string Id => "badlands-heightmap-v1";
    public GeneratorVariant Variant => GeneratorVariant.Heightmap;
    public GeneratedWorld Generate(WorldGenerationConfig configuration) => BadlandsBuilder.Build(configuration, Variant);
}

public sealed class CellularBadlandsGenerator : IBiomeGenerator
{
    public string Id => "badlands-cellular-v1";
    public GeneratorVariant Variant => GeneratorVariant.Cellular;
    public GeneratedWorld Generate(WorldGenerationConfig configuration) => BadlandsBuilder.Build(configuration, Variant);
}

public sealed class LayeredBadlandsGenerator : IBiomeGenerator
{
    public string Id => "badlands-layered-v1";
    public GeneratorVariant Variant => GeneratorVariant.Layered;
    public GeneratedWorld Generate(WorldGenerationConfig configuration) => BadlandsBuilder.Build(configuration, Variant);
}
