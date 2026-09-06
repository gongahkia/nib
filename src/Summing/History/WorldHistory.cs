using System.Collections.Generic;

namespace Summing.History;

public sealed class WorldHistory
{
    public required string Id { get; init; }
    public required long Seed { get; init; }
    public required string EpochName { get; init; }
    public required List<GeneratedCulture> Cultures { get; init; }
    public required List<CultureRelation> Relations { get; init; }
    public required List<HistoricalEvent> Events { get; init; }
}

public sealed record GeneratedCulture(
    short Id,
    string Name,
    string Belief,
    string TechnologicalPractice,
    string Territory,
    string Symbol,
    string Fate);

public sealed record CultureRelation(
    short FirstCultureId,
    short SecondCultureId,
    string Disposition,
    string InheritedGrudge);

public sealed record HistoricalEvent(
    string Id,
    int YearsBeforePresent,
    string Kind,
    short PrimaryCultureId,
    short? SecondaryCultureId,
    string Summary,
    string EnvironmentalTrace);
