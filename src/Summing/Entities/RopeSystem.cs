using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Summing.Core;
using Summing.Input;
using Summing.Player;
using Summing.World;

namespace Summing.Entities;

public sealed class RopeSystem
{
    private const float AttachRange = 25f;
    private const float ClimbSpeed = 92f;
    private const int MaximumLengthTiles = 9;
    private const float MinimumAimAlignment = 0.9f;
    public const int MaximumThrowRangeTiles = 6;
    public const float MaximumThrowRange = MaximumThrowRangeTiles * GameConstants.TileSize;
    private readonly List<RopeEntity> _ropes = [];
    private float _noticeTimer;
    public IReadOnlyList<RopeEntity> Ropes => _ropes;
    public string Notice { get; private set; } = "";
    public bool NoticeVisible => _noticeTimer > 0f;
    public event Action<RopeEntity>? Placed;
    public event Action<RopeEntity>? Detached;
    public event Action<string>? PlacementRejected;

    public void Update(InputManager input, PlayerController player, PlayerInventory inventory, TileWorld world, float dt)
    {
        _noticeTimer = MathF.Max(0f, _noticeTimer - dt);
        RemoveDetachedRopes(world);
        if (input.Pressed(InputAction.Rope)) TryPlace(input, player, inventory, world);
        if (MathF.Abs(input.Move.Y) < 0.15f) return;
        foreach (var rope in _ropes)
        {
            if (MathF.Abs(player.Position.X - rope.X) > AttachRange ||
                player.Position.Y < rope.ClimbTop - 2f || player.Bounds.Top > rope.Bottom + 2f) continue;
            if (input.Move.Y < 0f && player.Position.Y <= rope.ClimbTop + ClimbSpeed * dt + 0.5f)
            {
                player.TryRopeMove(new Vector2(rope.ExitX, rope.ClimbTop), world);
                return;
            }
            var next = new Vector2(rope.X,
                Math.Clamp(player.Position.Y + input.Move.Y * ClimbSpeed * dt, rope.ClimbTop, rope.Bottom));
            player.TryRopeMove(next, world);
            return;
        }
    }

    public void Draw(SpriteBatch batch, Texture2D pixel)
    {
        foreach (var rope in _ropes) rope.Draw(batch, pixel);
    }

    private void TryPlace(InputManager input, PlayerController player, PlayerInventory inventory, TileWorld world)
    {
        if (!TryFindOverhang(input, player, world, out var rope) && !TryFindLedge(player, world, out rope))
        {
            Reject($"NO SOLID ANCHOR WITHIN {MaximumThrowRangeTiles} TILES", "no-anchor-in-range");
            return;
        }
        foreach (var existing in _ropes)
            if (existing.AnchorTile == rope.AnchorTile)
            {
                Reject("A ROPE IS ALREADY ANCHORED HERE", "duplicate-anchor");
                return;
            }
        if (!inventory.TryUseRope())
        {
            Reject("NO ROPES REMAIN", "empty-inventory");
            return;
        }

        _ropes.Add(rope);
        Placed?.Invoke(rope);
        player.ShowActionState(MovementState.RopeInteraction, 0.2f);
        Notice = rope.IsOverhangAnchor ? "ROPE CAUGHT THE OVERHANG" : "ROPE SECURED TO LEDGE";
        _noticeTimer = 1.5f;
    }

    private static bool TryFindOverhang(InputManager input, PlayerController player, TileWorld world,
        out RopeEntity rope)
    {
        rope = null!;
        var throwDirection = ResolveThrowDirection(input);
        var throwOrigin = player.Bounds.Center;
        var rangeOrigin = player.Position;
        var minimumX = Math.Max(0, TileWorld.WorldToTile(rangeOrigin.X - MaximumThrowRange));
        var maximumX = Math.Min(world.Width - 1, TileWorld.WorldToTile(rangeOrigin.X + MaximumThrowRange));
        var minimumY = Math.Max(0, TileWorld.WorldToTile(rangeOrigin.Y - MaximumThrowRange));
        var maximumY = Math.Min(world.Height - 2, TileWorld.WorldToTile(player.Bounds.Top - 1f));
        var bestScore = float.NegativeInfinity;

        for (var tileY = minimumY; tileY <= maximumY; tileY++)
            for (var tileX = minimumX; tileX <= maximumX; tileX++)
            {
                if (!world.GetTile(tileX, tileY).Solid || world.GetTile(tileX, tileY + 1).Solid) continue;
                var x = (tileX + 0.5f) * GameConstants.TileSize;
                var top = (tileY + 1f) * GameConstants.TileSize;
                var anchor = new Vector2(x, top);
                var footDistanceSquared = Vector2.DistanceSquared(rangeOrigin, anchor);
                if (footDistanceSquared > MaximumThrowRange * MaximumThrowRange) continue;
                var toAnchor = anchor - throwOrigin;
                if (toAnchor.Y >= -1f || toAnchor.LengthSquared() < 1f) continue;
                var alignment = Vector2.Dot(Vector2.Normalize(toAnchor), throwDirection);
                if (alignment < MinimumAimAlignment || !ThrowPathClear(world, throwOrigin, anchor)) continue;
                var climbTop = top + PlayerController.StandingBodyHeight;
                if (world.OverlapsSolid(new Aabb(x - PlayerController.BodyWidth * 0.5f, top + 0.01f,
                    PlayerController.BodyWidth, PlayerController.StandingBodyHeight))) continue;
                var bottom = FindRopeBottom(world, x, top);
                if (bottom - climbTop < GameConstants.TileSize) continue;

                var score = alignment * 1024f - MathF.Sqrt(footDistanceSquared);
                if (score <= bestScore) continue;
                bestScore = score;
                rope = new RopeEntity(x, top, bottom, climbTop, x, 0, new Point(tileX, tileY));
            }
        return rope != null;
    }

    private static Vector2 ResolveThrowDirection(InputManager input)
    {
        var direction = input.Move.Y < -0.15f ? input.Move : input.Aim.Y < -0.15f ? input.Aim : -Vector2.UnitY;
        return Vector2.Normalize(direction);
    }

    private static bool ThrowPathClear(TileWorld world, Vector2 start, Vector2 end)
    {
        var delta = end - start;
        var distance = delta.Length();
        if (distance < 1f) return false;
        var direction = delta / distance;
        for (var travelled = 3f; travelled < distance - 1f; travelled += 3f)
        {
            var point = start + direction * travelled;
            if (world.GetTile(TileWorld.WorldToTile(point.X), TileWorld.WorldToTile(point.Y)).Solid) return false;
        }
        return true;
    }

    private static bool TryFindLedge(PlayerController player, TileWorld world, out RopeEntity rope)
    {
        rope = null!;
        var direction = player.Facing < 0 ? -1 : 1;
        if (!player.Grounded) return false;

        var supportX = TileWorld.WorldToTile(player.Position.X - direction * PlayerController.BodyWidth * 0.45f);
        var supportY = TileWorld.WorldToTile(player.Position.Y + 1f);
        var openX = supportX + direction;
        if (!world.Contains(supportX, supportY) || !world.Contains(openX, supportY) ||
            !world.GetTile(supportX, supportY).Solid || world.GetTile(supportX, supportY - 1).Solid ||
            world.GetTile(openX, supportY).Solid || world.GetTile(openX, supportY - 1).Solid) return false;

        var ledgeEdge = (supportX + (direction > 0 ? 1 : 0)) * GameConstants.TileSize;
        var x = ledgeEdge + direction * 10f;
        var top = supportY * GameConstants.TileSize;
        var bottom = FindRopeBottom(world, x, top);
        if (bottom - top < GameConstants.TileSize) return false;
        var exitX = x - direction * (GameConstants.TileSize * 0.5f + 10f);
        rope = new RopeEntity(x, top, bottom, top, exitX, direction, new Point(supportX, supportY));
        return true;
    }

    private static float FindRopeBottom(TileWorld world, float x, float top)
    {
        var bottom = MathF.Min(world.PixelHeight, top + GameConstants.TileSize * MaximumLengthTiles);
        var tileX = TileWorld.WorldToTile(x);
        var firstTileY = TileWorld.WorldToTile(top + 0.01f);
        var lastTileY = Math.Min(world.Height - 1, firstTileY + MaximumLengthTiles);
        for (var tileY = firstTileY; tileY <= lastTileY; tileY++)
            if (world.GetTile(tileX, tileY).Solid)
            {
                bottom = tileY * GameConstants.TileSize;
                break;
            }
        return bottom;
    }

    private void RemoveDetachedRopes(TileWorld world)
    {
        for (var index = _ropes.Count - 1; index >= 0; index--)
        {
            var rope = _ropes[index];
            if (world.GetTile(rope.AnchorTile.X, rope.AnchorTile.Y).Solid) continue;
            _ropes.RemoveAt(index);
            Detached?.Invoke(rope);
            Notice = "ROPE ANCHOR BROKE";
            _noticeTimer = 1.5f;
        }
    }

    private void Reject(string notice, string reason)
    {
        Notice = notice;
        _noticeTimer = 1.8f;
        PlacementRejected?.Invoke(reason);
    }
}
