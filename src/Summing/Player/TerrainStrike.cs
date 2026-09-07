using System;
using Microsoft.Xna.Framework;
using Summing.Core;
using Summing.Input;
using Summing.World;

namespace Summing.Player;

public sealed class TerrainStrike
{
    private const float StrikeDuration = 0.16f;
    private const float ImpactTime = 0.055f;
    private float _timer;
    private bool _impacted;
    private Point _targetTile;

    public bool Striking => _timer > 0f;
    public Point TargetTile => _targetTile;
    public event Action<TerrainChange>? Impact;

    public void Update(InputManager input, PlayerController player, TileWorld world, float dt)
    {
        if (_timer > 0f)
        {
            _timer -= dt;
            player.ShowActionState(MovementState.Punching, MathF.Max(_timer, 0.03f));
            if (!_impacted && _timer <= StrikeDuration - ImpactTime)
            {
                _impacted = true;
                var before = world.GetTile(_targetTile.X, _targetTile.Y);
                if (before.Solid)
                {
                    world.DamageTile(_targetTile.X, _targetTile.Y, 1, "punch");
                    Impact?.Invoke(new TerrainChange(_targetTile, before.Material, "punch", 1,
                        !world.GetTile(_targetTile.X, _targetTile.Y).Solid));
                }
            }
            return;
        }

        if (!input.Pressed(InputAction.Dig)) return;
        var direction = SelectDirection(input, player);
        _targetTile = AdjacentTargetTile(player.Bounds, direction);
        _timer = StrikeDuration;
        _impacted = false;
        player.ShowActionState(MovementState.Punching, StrikeDuration);
    }

    private static Vector2 SelectDirection(InputManager input, PlayerController player)
    {
        if (input.Move.LengthSquared() > 0.15f)
        {
            var direction = input.Move;
            if (MathF.Abs(direction.X) > MathF.Abs(direction.Y)) return new Vector2(Math.Sign(direction.X), 0f);
            return new Vector2(0f, Math.Sign(direction.Y));
        }
        return new Vector2(player.Facing, 0f);
    }

    private static Point AdjacentTargetTile(Aabb body, Vector2 direction)
    {
        if (direction.Y < 0f)
            return new Point(TileWorld.WorldToTile(body.Center.X), TileWorld.WorldToTile(body.Top) - 1);
        if (direction.Y > 0f)
            return new Point(TileWorld.WorldToTile(body.Center.X), TileWorld.WorldToTile(body.Bottom - 0.01f) + 1);
        if (direction.X < 0f)
            return new Point(TileWorld.WorldToTile(body.Left) - 1, TileWorld.WorldToTile(body.Center.Y));
        return new Point(TileWorld.WorldToTile(body.Right - 0.01f) + 1, TileWorld.WorldToTile(body.Center.Y));
    }
}
