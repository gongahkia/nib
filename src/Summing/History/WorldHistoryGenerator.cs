using System;
using System.Collections.Generic;
using Summing.Generation;

namespace Summing.History;

public static class WorldHistoryGenerator
{
    private static readonly string[] NameRoots =
        ["Avar", "Cair", "Eid", "Ilyr", "Keth", "Namar", "Orun", "Sere", "Thren", "Ulan", "Vael", "Ysil"];
    private static readonly string[] NameForms =
        ["Accord", "Choir", "Concord", "Keepers", "Ledger", "Measure", "Meridian", "Pact", "Remnant", "Synod", "Witnesses"];
    private static readonly string[] Beliefs =
    [
        "memory is a material debt that must be returned to the earth",
        "weathered machines retain the final intentions of their makers",
        "altitude measures the distance between an act and its consequence",
        "human breath belongs to the vanished reservoir sky",
        "stone records every name but reveals them in the wrong order",
        "the oldest instruments are ancestors whose functions must remain veiled",
        "migration repeats an ancient circuit broken by the collapse"
    ];
    private static readonly string[] Practices =
    [
        "sealed pressure engines inside votive masonry",
        "encoded census records as mineral firing patterns",
        "grew conductive salt glass along exposed foundations",
        "used fossil conduits as both aqueducts and legal boundaries",
        "repaired atmospheric machinery through hereditary rites",
        "cut migration bearings into cliff faces with oxidised alloy",
        "measured structural fatigue as a calendar of obligations"
    ];
    private static readonly string[] Territories =
        ["lower dust shelves", "western ravines", "black upper scarps", "salt-glass terraces", "reservoir ruins", "windward mesas"];
    private static readonly string[] Symbols =
        ["divided ring", "three descending cuts", "closed eye lattice", "broken vertical measure", "hollow hand", "paired horizon"];
    private static readonly string[] Fates =
    [
        "dispersed during the long pressure failure",
        "absorbed by a successor that erased its spoken name",
        "abandoned the Reach after its reservoir rites ceased to work",
        "survived only as marks copied by later travellers",
        "collapsed after generations of disputed boundary repairs",
        "vanished during an upward migration whose destination is unknown"
    ];
    private static readonly string[] Dispositions = ["ritual alliance", "contested inheritance", "cold exchange", "open conflict", "uneasy custodianship"];
    private static readonly string[] Grudges =
    [
        "an unreturned atmospheric instrument",
        "the redrawing of a fossil-water boundary",
        "a forbidden repair made during an eclipse season",
        "the burial of shared records beneath a false monument",
        "a migration passage sealed without warning"
    ];
    private static readonly (string Kind, string Verb, string Trace)[] EventForms =
    [
        ("migration", "crossed the exposed shelves after the lower air failed", "parallel bearing cuts overwritten at different heights"),
        ("conflict", "claimed a pressure organ already held as an ancestor", "alloy repairs interrupted by deliberate impact scars"),
        ("collapse", "lost the method that kept the reservoir seals compliant", "walls bowed outward around fossilised valve housings"),
        ("compact", "exchanged safe ascent routes for custody of buried records", "two incompatible symbols joined by a later mortar line"),
        ("schism", "divided over whether a functioning machine could be sacred", "matching masonry split between polished and shattered faces"),
        ("alteration", "fired a new mineral skin across the older cliff works", "thin glass strata covering eroded names"),
        ("abandonment", "sealed its final instruments and departed into upper weather", "door-like stone planes without accessible chambers")
    ];

    public static WorldHistory Generate(long seed)
    {
        var random = new DeterministicRandom(seed ^ unchecked((long)0xa7139e25c4d18f2bUL));
        var cultureCount = random.Range(3, 6);
        var cultures = new List<GeneratedCulture>(cultureCount);
        var usedNames = new HashSet<string>();
        for (short id = 1; id <= cultureCount; id++)
        {
            string name;
            do name = $"{NameRoots[random.Range(0, NameRoots.Length)]} {NameForms[random.Range(0, NameForms.Length)]}";
            while (!usedNames.Add(name));
            cultures.Add(new GeneratedCulture(id, name, Pick(Beliefs, random), Pick(Practices, random),
                Pick(Territories, random), Pick(Symbols, random), Pick(Fates, random)));
        }

        var relations = new List<CultureRelation>();
        for (short first = 1; first <= cultureCount; first++)
        for (short second = (short)(first + 1); second <= cultureCount; second++)
            relations.Add(new CultureRelation(first, second, Pick(Dispositions, random), Pick(Grudges, random)));

        var events = new List<HistoricalEvent>();
        var years = random.Range(840, 1600);
        for (var index = 0; index < random.Range(7, 11); index++)
        {
            years += random.Range(90, 720);
            var form = EventForms[random.Range(0, EventForms.Length)];
            var primary = (short)random.Range(1, cultureCount + 1);
            short? secondary = form.Kind is "conflict" or "compact" ? (short)random.Range(1, cultureCount + 1) : null;
            if (secondary == primary) secondary = (short)(primary % cultureCount + 1);
            var primaryName = cultures[primary - 1].Name;
            var secondaryText = secondary.HasValue ? $" against the claims of {cultures[secondary.Value - 1].Name}" : "";
            events.Add(new HistoricalEvent($"{seed:x16}-event-{index:D2}", years, form.Kind, primary, secondary,
                $"{primaryName} {form.Verb}{secondaryText}.", form.Trace));
        }

        return new WorldHistory
        {
            Id = $"world-{unchecked((ulong)seed):x16}",
            Seed = seed,
            EpochName = $"The {NameRoots[random.Range(0, NameRoots.Length)]} {Pick(new[] { "Recession", "Silence", "Exposure", "Severance", "Ascent" }, random)}",
            Cultures = cultures,
            Relations = relations,
            Events = events
        };
    }

    private static T Pick<T>(IReadOnlyList<T> values, DeterministicRandom random) => values[random.Range(0, values.Count)];
}
