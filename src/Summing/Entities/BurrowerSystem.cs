using System;
using System.Collections.Generic;
using Summing.Gameplay;
using Summing.Generation;
using Summing.Player;
using Summing.World;

namespace Summing.Entities;

public sealed class BurrowerSystem
{
    private readonly List<BurrowerEntity> _burrowers = [];
    private readonly DifficultyTuning _tuning;

    public BurrowerSystem(GeneratedWorld generated, DifficultyTuning tuning)
    {
        _tuning = tuning;
        foreach (var feature in generated.Features)
            if (feature.Kind == WorldFeatureKind.BurrowerSpawn) Add(feature);
    }

    public IReadOnlyList<BurrowerEntity> Burrowers => _burrowers;
    public event Action<string>? Event;

    public void Update(PlayerController player, TileWorld world, float dt)
    {
        foreach (var burrower in _burrowers)
            burrower.Update(player, world, _tuning.EnemySpeedMultiplier, _tuning.EnemyContactDamage, dt);
    }

    public void ApplyExplosion(Explosion explosion)
    {
        foreach (var burrower in _burrowers) burrower.ApplyExplosion(explosion);
        Event?.Invoke($"explosion:{(int)explosion.Position.X},{(int)explosion.Position.Y}");
    }

    private void Add(WorldFeature feature)
    {
        var burrower = new BurrowerEntity(feature);
        burrower.DugTile += tile => Event?.Invoke($"dig:{tile.X},{tile.Y}");
        burrower.Attacked += () => Event?.Invoke("attack");
        _burrowers.Add(burrower);
    }
}
