namespace Summing.Generation;

public sealed class BadlandsParameters
{
    public int Width { get; set; } = 88;
    public int Height { get; set; } = 220;
    public float Erosion { get; set; } = 0.32f;
    public float RuinDensity { get; set; } = 0.16f;
    public float EcologyDensity { get; set; } = 0.12f;
    public float WindStrength { get; set; } = 14f;
}

public sealed class WorldGenerationConfig
{
    public long Seed { get; set; } = 9042026;
    public string GeneratorVersion { get; set; } = WorldGeneratorRegistry.CurrentVersion;
    public GeneratorVariant Variant { get; set; } = GeneratorVariant.Layered;
    public BadlandsParameters Parameters { get; set; } = new();
}
