using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Summing.Generation;
using Summing.History;
using Summing.Player;

namespace Summing.Entities;

public sealed class RelicSystem
{
    private readonly List<RelicEntity> _relics = [];
    private readonly ArchiveStore _archive;
    private readonly WorldHistory _history;
    private float _discoveryTimer;

    public RelicSystem(GeneratedWorld generated, ArchiveStore archive)
    {
        _archive = archive;
        _history = generated.History;
        var index = 0;
        foreach (var feature in generated.Features)
        {
            if (feature.Kind != WorldFeatureKind.RelicCandidate) continue;
            var historyEvent = _history.Events[Math.Abs(feature.Variant + index * 3) % _history.Events.Count];
            var culture = _history.Cultures.Find(item => item.Id == feature.ProvenanceId) ?? _history.Cultures[0];
            _relics.Add(new RelicEntity($"{_history.Id}-relic-{index:D2}", feature, historyEvent, culture));
            index++;
        }
    }

    public IReadOnlyList<RelicEntity> Relics => _relics;
    public string? LastDiscovery { get; private set; }
    public bool DiscoveryVisible => _discoveryTimer > 0f;
    public event Action<RelicEntity>? Collected;

    public void Update(PlayerController player, float dt)
    {
        _discoveryTimer = MathF.Max(0f, _discoveryTimer - dt);
        foreach (var relic in _relics)
        {
            if (relic.Collected || Vector2.DistanceSquared(player.Bounds.Center, relic.Position) > 30f * 30f) continue;
            relic.Collect();
            var entry = new ArchiveEntry(relic.Id, _history.Id, _history.Seed, _history.EpochName,
                relic.Culture.Name, $"{relic.Culture.Name}: {relic.HistoryEvent.Kind}", relic.HistoryEvent.Summary,
                relic.HistoryEvent.EnvironmentalTrace, DateTimeOffset.UtcNow);
            _archive.Add(entry);
            LastDiscovery = $"ARCHIVE: {relic.Culture.Name} / {relic.HistoryEvent.Kind}";
            _discoveryTimer = 4.5f;
            Collected?.Invoke(relic);
        }
    }

}
