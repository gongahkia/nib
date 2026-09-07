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
    private readonly List<RopeEntity> _ropes = [];
    private float _noticeTimer;
    public IReadOnlyList<RopeEntity> Ropes => _ropes;
    public string Notice { get; private set; } = "";
    public bool NoticeVisible => _noticeTimer > 0f;
    public event Action<RopeEntity>? Placed;
    public event Action<string>? PlacementRejected;

    public void Update(InputManager input, PlayerController player, PlayerInventory inventory, TileWorld world, float dt)
    {
        _noticeTimer = MathF.Max(0f, _noticeTimer - dt);
        if (input.Pressed(InputAction.Rope)) TryPlace(player, inventory, world);
        if (MathF.Abs(input.Move.Y) < 0.15f) return;
        foreach (var rope in _ropes)
        {
            if (MathF.Abs(player.Position.X - rope.X) > AttachRange ||
                player.Position.Y < rope.Top - 2f || player.Bounds.Top > rope.Bottom + 2f) continue;
            if (input.Move.Y < 0f && player.Position.Y <= rope.Top + ClimbSpeed * dt + 0.5f)
            {
                var ledgeCenter = rope.X - rope.LedgeDirection * (GameConstants.TileSize * 0.5f + 10f);
                player.TryRopeMove(new Vector2(ledgeCenter, rope.Top), world);
                return;
            }
            var next = new Vector2(rope.X,
                Math.Clamp(player.Position.Y + input.Move.Y * ClimbSpeed * dt, rope.Top, rope.Bottom));
            player.TryRopeMove(next, world);
            return;
        }
    }

    public void Draw(SpriteBatch batch, Texture2D pixel)
    {
        foreach (var rope in _ropes) rope.Draw(batch, pixel);
    }

    private void TryPlace(PlayerController player, PlayerInventory inventory, TileWorld world)
    {
        if (!TryFindLedge(player, world, out var x, out var top, out var bottom, out var direction))
        {
            Reject("ROPE NEEDS A LEDGE", "requires-ledge");
            return;
        }
        foreach (var existing in _ropes)
            if (MathF.Abs(existing.X - x) < 2f && MathF.Abs(existing.Top - top) < 2f)
            {
                Reject("A ROPE IS ALREADY HERE", "duplicate-ledge");
                return;
            }
        if (!inventory.TryUseRope())
        {
            Reject("NO ROPES REMAIN", "empty-inventory");
            return;
        }

        var rope = new RopeEntity(x, top, bottom, direction);
        _ropes.Add(rope);
        Placed?.Invoke(rope);
        player.ShowActionState(MovementState.RopeInteraction, 0.2f);
        Notice = "ROPE SECURED TO LEDGE";
        _noticeTimer = 1.5f;
    }

    private static bool TryFindLedge(PlayerController player, TileWorld world, out float x, out float top,
        out float bottom, out int direction)
    {
        x = top = bottom = 0f;
        direction = player.Facing < 0 ? -1 : 1;
        if (!player.Grounded) return false;

        var supportX = TileWorld.WorldToTile(player.Position.X - direction * PlayerController.BodyWidth * 0.45f);
        var supportY = TileWorld.WorldToTile(player.Position.Y + 1f);
        var openX = supportX + direction;
        if (!world.Contains(supportX, supportY) || !world.Contains(openX, supportY) ||
            !world.GetTile(supportX, supportY).Solid || world.GetTile(supportX, supportY - 1).Solid ||
            world.GetTile(openX, supportY).Solid || world.GetTile(openX, supportY - 1).Solid) return false;

        var ledgeEdge = (supportX + (direction > 0 ? 1 : 0)) * GameConstants.TileSize;
        x = ledgeEdge + direction * 10f;
        top = supportY * GameConstants.TileSize;
        bottom = MathF.Min(world.PixelHeight, top + GameConstants.TileSize * MaximumLengthTiles);
        for (var tileY = supportY + 1; tileY <= Math.Min(world.Height - 1, supportY + MaximumLengthTiles); tileY++)
            if (world.GetTile(openX, tileY).Solid)
            {
                bottom = tileY * GameConstants.TileSize;
                break;
            }
        return bottom - top >= GameConstants.TileSize;
    }

    private void Reject(string notice, string reason)
    {
        Notice = notice;
        _noticeTimer = 1.8f;
        PlacementRejected?.Invoke(reason);
    }
}
