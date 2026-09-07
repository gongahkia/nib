using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Summing.Core;
using Summing.Input;
using Summing.World;

namespace Summing.Player;

public sealed class DigTool
{
    private const float SwingDuration = 0.28f;
    private const float ImpactTime = 0.12f;
    private float _timer;
    private bool _impacted;
    private Point _targetTile;
    private Vector2 _direction = Vector2.UnitX;

    public bool Swinging => _timer > 0f;
    public Point TargetTile => _targetTile;
    public event Action<TerrainChange>? Impact;

    public void Update(InputManager input, PlayerController player, TileWorld world, float dt)
    {
        if (_timer > 0f)
        {
            _timer -= dt;
            player.ShowActionState(MovementState.Digging, MathF.Max(_timer, 0.03f));
            if (!_impacted && _timer <= SwingDuration - ImpactTime)
            {
                _impacted = true;
                var before = world.GetTile(_targetTile.X, _targetTile.Y);
                if (before.Solid)
                {
                    world.DamageTile(_targetTile.X, _targetTile.Y, 1, "tool");
                    Impact?.Invoke(new TerrainChange(_targetTile, before.Material, "tool", 1,
                        !world.GetTile(_targetTile.X, _targetTile.Y).Solid));
                }
            }
            return;
        }

        if (!input.Pressed(InputAction.Dig)) return;
        _direction = SelectDirection(input, player);
        _targetTile = AdjacentTargetTile(player.Bounds, _direction);
        _timer = SwingDuration;
        _impacted = false;
        player.ShowActionState(MovementState.Digging, SwingDuration);
    }

    public void Draw(SpriteBatch batch, Texture2D pixel, PlayerController player)
    {
        if (!Swinging) return;
        var progress = 1f - _timer / SwingDuration;
        var angleOffset = MathHelper.Lerp(-0.9f, 0.45f, MathF.Min(1f, progress * 1.4f));
        var baseAngle = MathF.Atan2(_direction.Y, _direction.X);
        var direction = new Vector2(MathF.Cos(baseAngle + angleOffset), MathF.Sin(baseAngle + angleOffset));
        var start = player.Bounds.Center;
        var end = start + direction * 28f;
        DrawLine(batch, pixel, start, end, new Color(194, 177, 137), 3f);
        batch.Draw(pixel, new Rectangle((int)end.X - 3, (int)end.Y - 3, 7, 7), new Color(80, 94, 94));
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

    private static void DrawLine(SpriteBatch batch, Texture2D pixel, Vector2 start, Vector2 end, Color color, float width)
    {
        var delta = end - start;
        batch.Draw(pixel, start, null, color, MathF.Atan2(delta.Y, delta.X), Vector2.Zero,
            new Vector2(delta.Length(), width), SpriteEffects.None, 0f);
    }
}
