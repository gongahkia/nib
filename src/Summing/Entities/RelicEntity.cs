using Microsoft.Xna.Framework;
using Summing.Generation;
using Summing.History;

namespace Summing.Entities;

public sealed class RelicEntity
{
    public RelicEntity(string id, WorldFeature source, HistoricalEvent historyEvent, GeneratedCulture culture)
    {
        Id = id;
        Position = source.Position;
        Variant = source.Variant;
        HistoryEvent = historyEvent;
        Culture = culture;
    }

    public string Id { get; }
    public Vector2 Position { get; }
    public int Variant { get; }
    public HistoricalEvent HistoryEvent { get; }
    public GeneratedCulture Culture { get; }
    public bool Collected { get; private set; }
    public void Collect() => Collected = true;
}
