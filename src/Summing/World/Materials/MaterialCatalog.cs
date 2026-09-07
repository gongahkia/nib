using System.Collections.Generic;

namespace Summing.World.Materials;

public static class MaterialCatalog
{
    private static readonly IReadOnlyDictionary<MaterialId, MaterialDefinition> Definitions =
        new Dictionary<MaterialId, MaterialDefinition>
        {
            [MaterialId.Air] = new(MaterialId.Air, "air", false, 0, 0f, 0f, 0f, 0f, false, false,
                new(0, 0, 0, 0), new(0, 0, 0, 0)),
            [MaterialId.Loess] = new(MaterialId.Loess, "wind loess", true, 1, 0.66f, 0.85f, 0.15f, 0.08f, false, false,
                new(112, 73, 63), new(151, 98, 73)),
            [MaterialId.RedSandstone] = new(MaterialId.RedSandstone, "red sandstone", true, 1, 0.82f, 0.55f, 0.32f, 0.12f, true, true,
                new(111, 60, 61), new(161, 88, 70)),
            [MaterialId.BlackBasalt] = new(MaterialId.BlackBasalt, "black basalt", true, 2, 0.94f, 0.12f, 0.48f, 0.34f, true, true,
                new(48, 45, 55), new(78, 66, 69)),
            [MaterialId.FossilComposite] = new(MaterialId.FossilComposite, "fossil composite", true, 2, 0.77f, 0.38f, 0.24f, 0.43f, true, true,
                new(119, 106, 84), new(177, 151, 105)),
            [MaterialId.RuinAlloy] = new(MaterialId.RuinAlloy, "votive alloy", true, 2, 0.9f, 0.08f, 0.7f, 0.92f, true, true,
                new(61, 77, 81), new(112, 131, 124)),
            [MaterialId.BrittleMasonry] = new(MaterialId.BrittleMasonry, "brittle masonry", true, 1, 0.71f, 1f, 0.18f, 0.2f, true, true,
                new(98, 77, 72), new(190, 118, 83)),
            [MaterialId.SaltGlass] = new(MaterialId.SaltGlass, "salt glass", true, 1, 0.55f, 0.92f, 0.12f, 0.62f, false, false,
                new(122, 143, 139), new(181, 206, 184)),
            [MaterialId.OchreClay] = new(MaterialId.OchreClay, "ochre clay", true, 1, 0.72f, 0.78f, 0.18f, 0.09f, false, false,
                new(122, 79, 54), new(166, 111, 70)),
            [MaterialId.PaleChalk] = new(MaterialId.PaleChalk, "pale chalk", true, 1, 0.7f, 0.9f, 0.1f, 0.06f, true, false,
                new(148, 142, 124), new(201, 191, 158)),
            [MaterialId.BlueShale] = new(MaterialId.BlueShale, "blue shale", true, 2, 0.86f, 0.46f, 0.2f, 0.2f, true, true,
                new(58, 68, 76), new(88, 104, 111)),
            [MaterialId.Ironstone] = new(MaterialId.Ironstone, "ironstone", true, 2, 0.92f, 0.24f, 0.55f, 0.48f, true, true,
                new(78, 58, 57), new(137, 77, 63)),
            [MaterialId.AshClinker] = new(MaterialId.AshClinker, "ash clinker", true, 1, 0.68f, 0.82f, 0.76f, 0.18f, false, false,
                new(45, 47, 53), new(84, 77, 79)),
            [MaterialId.PetrifiedFiber] = new(MaterialId.PetrifiedFiber, "petrified fibre", true, 2, 0.81f, 0.43f, 0.2f, 0.27f, true, true,
                new(91, 77, 61), new(153, 126, 88)),
            [MaterialId.MachineCeramic] = new(MaterialId.MachineCeramic, "machine ceramic", true, 2, 0.88f, 0.31f, 0.82f, 0.08f, true, true,
                new(91, 94, 88), new(163, 157, 132)),
            [MaterialId.CopperSalt] = new(MaterialId.CopperSalt, "copper salt", true, 1, 0.62f, 0.88f, 0.16f, 0.87f, false, false,
                new(65, 93, 87), new(111, 164, 145)),
            [MaterialId.WeatheredConcrete] = new(MaterialId.WeatheredConcrete, "weathered concrete", true, 2, 0.84f, 0.5f, 0.31f, 0.29f, true, true,
                new(75, 72, 73), new(127, 116, 107))
        };

    public static MaterialDefinition Get(MaterialId id) => Definitions[id];
    public static IEnumerable<MaterialDefinition> All => Definitions.Values;
}
