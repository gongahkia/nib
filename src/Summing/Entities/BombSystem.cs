using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Summing.Core;
using Summing.Input;
using Summing.Player;
using Summing.World;

namespace Summing.Entities;

public sealed class BombSystem
{
    private const float TerrainBlastRadius = 72f;
    private const float CoreDamageRadius = 32f;
    private const float DamageRadius = 56f;
    private const float PlayerInfluenceRadius = 84f;
    private readonly List<BombEntity> _bombs = [];
    private readonly List<(Vector2 Position, float Time)> _blasts = [];
    public IReadOnlyList<BombEntity> Bombs => _bombs;
    public event Action<BombEntity>? Placed;
    public event Action<Explosion>? Exploded;
    public event Action<BombPlayerImpact>? PlayerBoosted;

    public void Update(InputManager input, PlayerController player, PlayerInventory inventory, TileWorld world, long frame, float dt)
    {
        if (input.Pressed(InputAction.Bomb) && inventory.TryUseBomb())
        {
            var throwDirection = input.Move.LengthSquared() > 0.15f ? input.Move : new Vector2(player.Facing, 0f);
            if (throwDirection.LengthSquared() > 1f) throwDirection.Normalize();
            var bomb = new BombEntity(FindClearSpawn(player, world),
                new Vector2(throwDirection.X * 155f, throwDirection.Y * 145f - 95f) + player.Velocity * 0.3f);
            _bombs.Add(bomb);
            Placed?.Invoke(bomb);
            player.ShowActionState(MovementState.BombUse, 0.18f);
        }

        for (var i = _bombs.Count - 1; i >= 0; i--)
        {
            var bomb = _bombs[i];
            if (!bomb.Update(world, dt)) continue;
            var destroyed = world.DamageCircle(bomb.Position, TerrainBlastRadius, 8, "bomb");
            var explosion = new Explosion(bomb.Position, TerrainBlastRadius, 2, destroyed, frame);
            var playerImpact = ApplyPlayerBlast(player, bomb.Position);
            if (playerImpact.RocketBoost) PlayerBoosted?.Invoke(playerImpact);
            _blasts.Add((bomb.Position, 0.22f));
            Exploded?.Invoke(explosion);
            _bombs.RemoveAt(i);
        }

        for (var i = _blasts.Count - 1; i >= 0; i--)
        {
            var blast = _blasts[i];
            blast.Time -= dt;
            if (blast.Time <= 0f) _blasts.RemoveAt(i);
            else _blasts[i] = blast;
        }
    }

    internal static BombPlayerImpact ApplyPlayerBlast(PlayerController player, Vector2 bombPosition)
    {
        var away = player.Bounds.Center - bombPosition;
        var distance = away.Length();
        if (distance >= PlayerInfluenceRadius) return default;
        if (distance < 1f) away = -Vector2.UnitY;
        else away /= distance;

        if (distance <= DamageRadius)
        {
            var damage = distance <= CoreDamageRadius ? 2 : 1;
            var scale = 1f - Math.Clamp(distance / DamageRadius, 0f, 1f);
            var impulse = away * (320f + 160f * scale);
            player.ApplyDamage(damage, impulse, "bomb");
            return new BombPlayerImpact(distance, damage, impulse, false);
        }

        if (player.Bounds.Center.Y <= bombPosition.Y + 8f)
        {
            away.Y = MathF.Min(away.Y, -0.45f);
            away.Normalize();
        }
        var edgeScale = 1f - Math.Clamp((distance - DamageRadius) /
            (PlayerInfluenceRadius - DamageRadius), 0f, 1f);
        var boost = away * MathHelper.Lerp(290f, 400f, edgeScale);
        player.ApplyBlastImpulse(boost);
        return new BombPlayerImpact(distance, 0, boost, true);
    }

    public void Draw(SpriteBatch batch, Texture2D pixel)
    {
        foreach (var bomb in _bombs) bomb.Draw(batch, pixel);
        foreach (var blast in _blasts)
        {
            var radius = (int)(TerrainBlastRadius * (1f - blast.Time / 0.22f));
            batch.Draw(pixel, new Rectangle((int)blast.Position.X - radius, (int)blast.Position.Y - 2, radius * 2, 4), new Color(229, 126, 76, 160));
            batch.Draw(pixel, new Rectangle((int)blast.Position.X - 2, (int)blast.Position.Y - radius, 4, radius * 2), new Color(237, 197, 124, 150));
        }
    }

    private static Vector2 FindClearSpawn(PlayerController player, TileWorld world)
    {
        Vector2[] candidates =
        [
            player.Bounds.Center + new Vector2(player.Facing * 12f, -3f),
            player.Bounds.Center + new Vector2(0f, -5f),
            player.Bounds.Center
        ];
        foreach (var candidate in candidates)
            if (!world.OverlapsSolid(new Aabb(candidate.X - 6f, candidate.Y - 6f, 12f, 12f))) return candidate;
        return player.Bounds.Center;
    }
}
