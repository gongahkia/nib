using System.Collections.Generic;

namespace Summing.World.Materials;

public static class MaterialCatalog
{
    private static readonly IReadOnlyDictionary<MaterialId, MaterialDefinition> Definitions =
        new Dictionary<MaterialId, MaterialDefinition>
        {
            [MaterialId.Air] = new(MaterialId.Air, "air", false, 0, 0f, 0f, 0f, 0f, false, false, false,
                new(0, 0, 0, 0), new(0, 0, 0, 0)),
            [MaterialId.Loess] = new(MaterialId.Loess, "wind loess", true, 1, 0.66f, 0.85f, 0.15f, 0.08f, false, false, false,
                new(112, 73, 63), new(151, 98, 73)),
            [MaterialId.RedSandstone] = new(MaterialId.RedSandstone, "red sandstone", true, 2, 0.82f, 0.55f, 0.32f, 0.12f, true, true, true,
                new(111, 60, 61), new(161, 88, 70)),
            [MaterialId.BlackBasalt] = new(MaterialId.BlackBasalt, "black basalt", true, 5, 0.94f, 0.12f, 0.48f, 0.34f, true, true, true,
                new(48, 45, 55), new(78, 66, 69)),
            [MaterialId.FossilComposite] = new(MaterialId.FossilComposite, "fossil composite", true, 3, 0.77f, 0.38f, 0.24f, 0.43f, true, true, true,
                new(119, 106, 84), new(177, 151, 105)),
            [MaterialId.RuinAlloy] = new(MaterialId.RuinAlloy, "votive alloy", true, 7, 0.9f, 0.08f, 0.7f, 0.92f, true, true, true,
                new(61, 77, 81), new(112, 131, 124)),
            [MaterialId.BrittleMasonry] = new(MaterialId.BrittleMasonry, "brittle masonry", true, 1, 0.71f, 1f, 0.18f, 0.2f, true, true, true,
                new(98, 77, 72), new(190, 118, 83)),
            [MaterialId.SaltGlass] = new(MaterialId.SaltGlass, "salt glass", true, 2, 0.55f, 0.92f, 0.12f, 0.62f, false, false, false,
                new(122, 143, 139), new(181, 206, 184))
        };

    public static MaterialDefinition Get(MaterialId id) => Definitions[id];
    public static IEnumerable<MaterialDefinition> All => Definitions.Values;
}
