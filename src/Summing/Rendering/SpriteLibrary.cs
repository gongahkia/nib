using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Summing.Player;
using Summing.Generation;

namespace Summing.Rendering;

public sealed class SpriteLibrary : IDisposable
{
    private readonly VisualPackManager _visualPacks;
    private readonly Texture2D _locomotion;
    private readonly Texture2D _traversal;
    private readonly Texture2D _asepriteLocomotion;
    private readonly Texture2D _asepriteTraversal;
    private readonly Texture2D _ruins;
    private readonly Texture2D _relics;
    private readonly Texture2D _burrower;
    private readonly Texture2D _brittle;
    private readonly Dictionary<MovementState, (bool Traversal, int Frame)> _stateFrames = new()
    {
        [MovementState.Idle] = (false, 0),
        [MovementState.Run] = (false, 2),
        [MovementState.Takeoff] = (false, 6),
        [MovementState.AscendingJump] = (false, 7),
        [MovementState.Apex] = (false, 8),
        [MovementState.Falling] = (false, 9),
        [MovementState.Landing] = (false, 10),
        [MovementState.Crouch] = (true, 0),
        [MovementState.Crawl] = (true, 1),
        [MovementState.Slide] = (true, 2),
        [MovementState.LedgeHang] = (true, 3),
        [MovementState.Mantle] = (true, 4),
        [MovementState.WallCling] = (true, 5),
        [MovementState.WallJump] = (true, 6),
        [MovementState.Dash] = (true, 7),
        [MovementState.RopeInteraction] = (true, 8),
        [MovementState.Digging] = (true, 9),
        [MovementState.BombUse] = (true, 11),
        [MovementState.Hurt] = (true, 12),
        [MovementState.Stunned] = (true, 12),
        [MovementState.Death] = (true, 13)
    };

    public SpriteLibrary(GraphicsDevice graphicsDevice, VisualPackManager visualPacks)
    {
        _visualPacks = visualPacks;
        _locomotion = Load(graphicsDevice, "Sprites/player_locomotion.png");
        _traversal = Load(graphicsDevice, "Sprites/player_traversal.png");
        _asepriteLocomotion = Load(graphicsDevice, "Sprites/player_aseprite_locomotion.png");
        _asepriteTraversal = Load(graphicsDevice, "Sprites/player_aseprite_traversal.png");
        _ruins = Load(graphicsDevice, "Sprites/ruins.png");
        _relics = Load(graphicsDevice, "Sprites/relics.png");
        _burrower = Load(graphicsDevice, "Sprites/burrower.png");
        _brittle = Load(graphicsDevice, "Sprites/brittle_hazard.png");
    }

    public void DrawFeature(SpriteBatch batch, WorldFeature feature)
    {
        if (feature.Kind is WorldFeatureKind.Ruin or WorldFeatureKind.ExposedMachine or WorldFeatureKind.FossilArch)
        {
            var frame = Math.Abs(feature.Variant) % 4;
            batch.Draw(_ruins, new Rectangle((int)feature.Position.X - 24, (int)feature.Position.Y - 64, 48, 64),
                new Rectangle(frame * 48, 0, 48, 64), Color.White);
            return;
        }
        if (feature.Kind == WorldFeatureKind.RelicCandidate)
        {
            var frame = Math.Abs(feature.Variant) % 6;
            batch.Draw(_relics, new Rectangle((int)feature.Position.X - 12, (int)feature.Position.Y - 24, 24, 24),
                new Rectangle(frame * 24, 0, 24, 24), Color.White);
            return;
        }
        if (feature.Kind == WorldFeatureKind.Ecology)
        {
            var frame = 2 + Math.Abs(feature.Variant) % 3;
            batch.Draw(_relics, new Rectangle((int)feature.Position.X - 6, (int)feature.Position.Y - 12, 12, 12),
                new Rectangle(frame * 24, 0, 24, 24), new Color(132, 171, 149, 185));
        }
    }

    public void DrawRelic(SpriteBatch batch, Vector2 position, int variant)
    {
        var frame = Math.Abs(variant) % 6;
        batch.Draw(_relics, new Rectangle((int)position.X - 12, (int)position.Y - 24, 24, 24),
            new Rectangle(frame * 24, 0, 24, 24), Color.White);
    }

    public void DrawBurrower(SpriteBatch batch, Vector2 position, int frame, bool hurt)
    {
        var color = hurt ? GamePalette.Danger : Color.White;
        batch.Draw(_burrower, new Rectangle((int)position.X - 16, (int)position.Y - 12, 32, 24),
            new Rectangle(Math.Abs(frame) % 4 * 32, 0, 32, 24), color);
    }

    public void DrawBrittle(SpriteBatch batch, Vector2 topLeft, int frame)
    {
        batch.Draw(_brittle, new Rectangle((int)topLeft.X, (int)topLeft.Y, 24, 24),
            new Rectangle(Math.Abs(frame) % 4 * 24, 0, 24, 24), Color.White);
    }

    public void DrawPlayer(SpriteBatch batch, MovementState state, Vector2 feet, int facing, long frame, Color tint)
    {
        if (_visualPacks.DrawPlayer(batch, state, feet, facing, frame, tint)) return;

        var mapping = _stateFrames[state];
        var asepritePilgrim = _visualPacks.PlayerPack == PlayerVisualPackId.AsepritePilgrim;
        var texture = mapping.Traversal
            ? asepritePilgrim ? _asepriteTraversal : _traversal
            : asepritePilgrim ? _asepriteLocomotion : _locomotion;
        var sourceFrame = mapping.Frame;
        if (state == MovementState.Idle) sourceFrame += (int)(frame / 38 % 2);
        if (state == MovementState.Run) sourceFrame += (int)(frame / 5 % 4);
        if (state == MovementState.Digging) sourceFrame += (int)(frame / 5 % 2);
        var source = new Rectangle(sourceFrame * 24, 0, 24, 24);
        var destination = new Rectangle((int)MathF.Round(feet.X - 12f), (int)MathF.Round(feet.Y - 24f), 24, 24);
        var effects = facing < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        var silhouette = new Color(7, 9, 14, 210);
        batch.Draw(texture, new Rectangle(destination.X - 1, destination.Y, destination.Width, destination.Height), source,
            silhouette, 0f, Vector2.Zero, effects, 0f);
        batch.Draw(texture, new Rectangle(destination.X + 1, destination.Y, destination.Width, destination.Height), source,
            silhouette, 0f, Vector2.Zero, effects, 0f);
        batch.Draw(texture, new Rectangle(destination.X, destination.Y + 1, destination.Width, destination.Height), source,
            silhouette, 0f, Vector2.Zero, effects, 0f);
        batch.Draw(texture, destination, source, tint, 0f, Vector2.Zero, effects, 0f);
    }

    public void Dispose()
    {
        _locomotion.Dispose();
        _traversal.Dispose();
        _asepriteLocomotion.Dispose();
        _asepriteTraversal.Dispose();
        _ruins.Dispose();
        _relics.Dispose();
        _burrower.Dispose();
        _brittle.Dispose();
    }

    private static Texture2D Load(GraphicsDevice graphicsDevice, string relativePath)
    {
        var path = Path.Combine(AppContext.BaseDirectory, relativePath);
        using var stream = File.OpenRead(path);
        return Texture2D.FromStream(graphicsDevice, stream);
    }
}
