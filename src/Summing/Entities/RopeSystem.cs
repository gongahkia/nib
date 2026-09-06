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
    private readonly List<RopeEntity> _ropes = [];
    public IReadOnlyList<RopeEntity> Ropes => _ropes;

    public void Update(InputManager input, PlayerController player, PlayerInventory inventory, TileWorld world, float dt)
    {
        if (input.Pressed(InputAction.Rope) && inventory.TryUseRope()) Place(player, world);
        if (!input.Down(InputAction.Grab) || MathF.Abs(input.Move.Y) < 0.15f) return;
        foreach (var rope in _ropes)
        {
            if (MathF.Abs(player.Position.X - rope.X) > 15f || player.Position.Y < rope.Top || player.Bounds.Top > rope.Bottom) continue;
            var next = new Vector2(MathHelper.Lerp(player.Position.X, rope.X, 0.35f),
                player.Position.Y + input.Move.Y * 92f * dt);
            player.TryRopeMove(next, world);
            return;
        }
    }

    public void Draw(SpriteBatch batch, Texture2D pixel)
    {
        foreach (var rope in _ropes) rope.Draw(batch, pixel);
    }

    private void Place(PlayerController player, TileWorld world)
    {
        var tileX = TileWorld.WorldToTile(player.Position.X);
        var playerTileY = TileWorld.WorldToTile(player.Bounds.Top);
        var anchorY = playerTileY;
        for (var step = 1; step <= 7; step++)
        {
            var candidate = playerTileY - step;
            if (world.GetTile(tileX, candidate).Solid) { anchorY = candidate + 1; break; }
            anchorY = candidate;
        }
        var x = (tileX + 0.5f) * GameConstants.TileSize;
        var top = anchorY * GameConstants.TileSize;
        var bottom = MathF.Min(world.PixelHeight, top + GameConstants.TileSize * 9f);
        _ropes.Add(new RopeEntity(x, top, bottom));
        player.ShowActionState(MovementState.RopeInteraction, 0.2f);
    }
}
