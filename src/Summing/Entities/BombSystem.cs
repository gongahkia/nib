using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Summing.Input;
using Summing.Player;
using Summing.World;

namespace Summing.Entities;

public sealed class BombSystem
{
    private readonly List<BombEntity> _bombs = [];
    private readonly List<(Vector2 Position, float Time)> _blasts = [];
    public IReadOnlyList<BombEntity> Bombs => _bombs;
    public event Action<BombEntity>? Placed;
    public event Action<Explosion>? Exploded;

    public void Update(InputManager input, PlayerController player, PlayerInventory inventory, TileWorld world, long frame, float dt)
    {
        if (input.Pressed(InputAction.Bomb) && inventory.TryUseBomb())
        {
            var throwDirection = input.Move.LengthSquared() > 0.15f ? input.Move : new Vector2(player.Facing, 0f);
            if (throwDirection.LengthSquared() > 1f) throwDirection.Normalize();
            var bomb = new BombEntity(player.Bounds.Center + new Vector2(player.Facing * 12f, -3f),
                new Vector2(throwDirection.X * 155f, throwDirection.Y * 145f - 95f) + player.Velocity * 0.3f);
            _bombs.Add(bomb);
            Placed?.Invoke(bomb);
            player.ShowActionState(MovementState.BombUse, 0.18f);
        }

        for (var i = _bombs.Count - 1; i >= 0; i--)
        {
            var bomb = _bombs[i];
            if (!bomb.Update(world, dt)) continue;
            const float radius = 72f;
            var destroyed = world.DamageCircle(bomb.Position, radius, 8, "bomb");
            var explosion = new Explosion(bomb.Position, radius, 2, destroyed, frame);
            var distance = Vector2.Distance(player.Bounds.Center, bomb.Position);
            if (distance < radius + 12f)
            {
                var away = player.Bounds.Center - bomb.Position;
                if (away.LengthSquared() < 1f) away = -Vector2.UnitY;
                away.Normalize();
                var scale = 1f - Math.Clamp(distance / (radius + 12f), 0f, 1f);
                player.ApplyDamage(explosion.Damage, away * (240f + 210f * scale), "bomb");
            }
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

    public void Draw(SpriteBatch batch, Texture2D pixel)
    {
        foreach (var bomb in _bombs) bomb.Draw(batch, pixel);
        foreach (var blast in _blasts)
        {
            var radius = (int)(72f * (1f - blast.Time / 0.22f));
            batch.Draw(pixel, new Rectangle((int)blast.Position.X - radius, (int)blast.Position.Y - 2, radius * 2, 4), new Color(229, 126, 76, 160));
            batch.Draw(pixel, new Rectangle((int)blast.Position.X - 2, (int)blast.Position.Y - radius, 4, radius * 2), new Color(237, 197, 124, 150));
        }
    }
}
