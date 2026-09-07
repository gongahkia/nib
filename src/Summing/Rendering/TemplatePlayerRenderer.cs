using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Summing.Player;

namespace Summing.Rendering;

public sealed class TemplatePlayerRenderer : IDisposable
{
    private const int SourceCellSize = 128;
    private static readonly Rectangle FrameCrop = new(42, 36, 78, 48);
    private static readonly Vector2 FootPivot = new(22f, 44f);
    private const float DrawScale = 0.5f;
    private readonly Dictionary<string, AnimationStrip> _strips;

    public TemplatePlayerRenderer(GraphicsDevice graphicsDevice)
    {
        _strips = new Dictionary<string, AnimationStrip>(StringComparer.Ordinal)
        {
            ["idle"] = Load(graphicsDevice, "idle", 10, 8),
            ["walk"] = Load(graphicsDevice, "walk", 10, 6),
            ["run"] = Load(graphicsDevice, "run", 10, 4),
            ["jump"] = Load(graphicsDevice, "jump", 6, 5),
            ["fall"] = Load(graphicsDevice, "fall", 4, 6),
            ["fall_loop"] = Load(graphicsDevice, "fall_loop", 3, 8),
            ["combo_1"] = Load(graphicsDevice, "combo_1", 3, 4),
            ["combo_1_end"] = Load(graphicsDevice, "combo_1_end", 4, 5)
        };
    }

    public void Draw(SpriteBatch batch, PlayerController player, long frame)
    {
        var selection = Select(player.VisualState, frame);
        var strip = _strips[selection.Strip];
        var source = FrameCrop;
        source.X += selection.Frame * SourceCellSize;
        var effects = player.Facing < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        var position = new Vector2(MathF.Round(player.Position.X), MathF.Round(player.Position.Y));
        batch.Draw(strip.Texture, position, source, Color.White, 0f, FootPivot, DrawScale, effects, 0f);
    }

    public void Dispose()
    {
        foreach (var strip in _strips.Values) strip.Texture.Dispose();
    }

    private static (string Strip, int Frame) Select(MovementState state, long frame) => state switch
    {
        MovementState.Idle => ("idle", Cycle(frame, 10, 8)),
        MovementState.Run => ("run", Cycle(frame, 10, 4)),
        MovementState.Takeoff => ("jump", 0),
        MovementState.AscendingJump => ("jump", Math.Min(3, Cycle(frame, 4, 5))),
        MovementState.Apex => ("jump", 4),
        MovementState.Falling => ("fall_loop", Cycle(frame, 3, 8)),
        MovementState.Landing => ("fall", 3),
        MovementState.Crouch => ("jump", 0),
        MovementState.Crawl => ("walk", Cycle(frame, 10, 7)),
        MovementState.Slide => ("run", 7),
        MovementState.LedgeHang => ("fall", 3),
        MovementState.Mantle => ("jump", 2),
        MovementState.WallCling => ("fall_loop", Cycle(frame, 3, 8)),
        MovementState.WallJump => ("jump", 2),
        MovementState.Dash => ("run", Cycle(frame, 10, 3)),
        MovementState.RopeInteraction => ("fall", 3),
        MovementState.RopeJump => ("jump", 3),
        MovementState.Digging => ("combo_1", Cycle(frame, 3, 4)),
        MovementState.BombUse => ("combo_1_end", Cycle(frame, 4, 5)),
        MovementState.Hurt => ("fall", 1),
        MovementState.Stunned => ("fall", 2),
        MovementState.Death => ("fall", 3),
        _ => ("idle", 0)
    };

    private static int Cycle(long frame, int count, int ticksPerFrame) =>
        (int)(frame / ticksPerFrame % count);

    private static AnimationStrip Load(GraphicsDevice graphicsDevice, string name, int expectedFrames,
        int ticksPerFrame)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Sprites", "Player", "TemplateFree",
            $"male_hero_template-{name}.png");
        if (!File.Exists(path)) throw new FileNotFoundException($"required Template Free player sheet is missing: {path}", path);
        using var stream = File.OpenRead(path);
        var texture = Texture2D.FromStream(graphicsDevice, stream);
        if (texture.Height != SourceCellSize || texture.Width != expectedFrames * SourceCellSize)
        {
            texture.Dispose();
            throw new InvalidDataException($"Template Free sheet {name} must be {expectedFrames * SourceCellSize}x" +
                $"{SourceCellSize}, found {texture.Width}x{texture.Height}");
        }
        return new AnimationStrip(texture, expectedFrames, ticksPerFrame);
    }

    private sealed record AnimationStrip(Texture2D Texture, int FrameCount, int TicksPerFrame);
}
