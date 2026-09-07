using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Summing.Input;
using Summing.Player;

namespace Summing.Rendering;

public sealed class VisualSelectionScreen
{
    private static readonly VisualSelectionOption[] Rows = Enum.GetValues<VisualSelectionOption>();
    private int _row;
    private string _notice = "PLAYER AND WORLD ART ARE INDEPENDENT";

    public bool Active { get; private set; }

    public void Open() => Active = true;

    public bool Update(InputManager input, VisualPackManager packs, out VisualSelectionChange change)
    {
        change = default;
        if (input.KeyPressed(Keys.K) || input.KeyPressed(Keys.Escape) || input.KeyPressed(Keys.Enter) ||
            input.ButtonPressed(Buttons.LeftShoulder) || input.ButtonPressed(Buttons.B) ||
            input.ButtonPressed(Buttons.Start))
        {
            Active = false;
            return false;
        }

        if (input.Pressed(InputAction.Up)) _row = Wrap(_row - 1, Rows.Length);
        if (input.Pressed(InputAction.Down)) _row = Wrap(_row + 1, Rows.Length);
        var direction = input.Pressed(InputAction.Left) ? -1 : input.Pressed(InputAction.Right) ? 1 : 0;
        if (direction == 0) return false;

        change = packs.Adjust(Rows[_row], direction);
        _notice = change.Notice;
        return change.Changed;
    }

    public void Draw(SpriteBatch batch, Texture2D pixel, PixelFont font, SpriteLibrary sprites,
        VisualPackManager packs, long frame, Color playerTint)
    {
        if (!Active) return;
        batch.Draw(pixel, new Rectangle(0, 0, 640, 360), new Color(4, 6, 11, 225));
        batch.Draw(pixel, new Rectangle(18, 17, 604, 326), new Color(9, 12, 19, 252));
        DrawBorder(batch, pixel, new Rectangle(18, 17, 604, 326), GamePalette.Stone);
        font.Draw(batch, "VISUAL SELECTOR", new Vector2(34, 30), GamePalette.SacredGold, 3);
        font.Draw(batch, "LIVE PREVIEW  PRESENTATION ONLY", new Vector2(358, 36), GamePalette.UiMuted);

        for (var index = 0; index < Rows.Length; index++)
        {
            var option = Rows[index];
            var y = 66 + index * 22;
            var selected = index == _row;
            var available = packs.OptionAvailable(option);
            if (selected) batch.Draw(pixel, new Rectangle(30, y - 4, 365, 17), new Color(50, 55, 67, 220));
            font.Draw(batch, selected ? ">" : " ", new Vector2(34, y), GamePalette.SacredGold);
            font.Draw(batch, VisualPackManager.OptionLabel(option), new Vector2(47, y),
                available ? GamePalette.Bone : GamePalette.UiMuted);
            font.Draw(batch, Shorten(packs.ValueName(option), 31), new Vector2(153, y),
                selected ? GamePalette.SaltCyan : available ? Color.White : GamePalette.UiMuted);
        }

        batch.Draw(pixel, new Rectangle(421, 64, 168, 105), new Color(15, 18, 27));
        DrawBorder(batch, pixel, new Rectangle(421, 64, 168, 105), GamePalette.FarStone);
        sprites.DrawPlayer(batch, MovementState.Run, new Vector2(505, 145), 1, frame, playerTint);
        font.Draw(batch, Shorten(packs.PlayerName, 24), new Vector2(434, 153), GamePalette.SaltCyan);

        var terrainArea = new Rectangle(433, 192, 144, 80);
        batch.Draw(pixel, terrainArea, GamePalette.DeepSky);
        if (!packs.DrawEnvironmentPreview(batch, terrainArea)) DrawOriginalEnvironment(batch, pixel, terrainArea);
        DrawBorder(batch, pixel, terrainArea, GamePalette.FarStone);
        font.Draw(batch, Shorten(packs.EnvironmentName, 24), new Vector2(434, 280), GamePalette.SaltCyan);

        font.Draw(batch, Shorten(_notice, 64), new Vector2(34, 278), GamePalette.SacredGold);
        font.Draw(batch, "UP DOWN SELECT   LEFT RIGHT CHANGE", new Vector2(34, 303), GamePalette.Bone);
        font.Draw(batch, "K ENTER ESC OR B CLOSES   CHOICES SAVE AUTOMATICALLY", new Vector2(34, 319), GamePalette.UiMuted);
    }

    private static void DrawOriginalEnvironment(SpriteBatch batch, Texture2D pixel, Rectangle area)
    {
        var colors = new[]
        {
            new Color(151, 87, 70), new Color(90, 72, 75), new Color(57, 59, 69),
            new Color(89, 72, 75), new Color(119, 76, 67), new Color(48, 45, 55)
        };
        const int cell = 40;
        for (var y = 0; y < 2; y++)
            for (var x = 0; x < 3; x++)
            {
                var destination = new Rectangle(area.X + 12 + x * cell, area.Y + y * cell, cell, cell);
                batch.Draw(pixel, destination, colors[x + y * 3]);
                batch.Draw(pixel, new Rectangle(destination.X, destination.Y, destination.Width, 3),
                    Color.Lerp(colors[x + y * 3], Color.White, 0.28f));
            }
    }

    private static void DrawBorder(SpriteBatch batch, Texture2D pixel, Rectangle rectangle, Color color)
    {
        batch.Draw(pixel, new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, 1), color);
        batch.Draw(pixel, new Rectangle(rectangle.X, rectangle.Bottom - 1, rectangle.Width, 1), color);
        batch.Draw(pixel, new Rectangle(rectangle.X, rectangle.Y, 1, rectangle.Height), color);
        batch.Draw(pixel, new Rectangle(rectangle.Right - 1, rectangle.Y, 1, rectangle.Height), color);
    }

    private static string Shorten(string value, int length) => value.Length <= length
        ? value
        : value[..(length - 2)] + "..";

    private static int Wrap(int value, int count) => (value % count + count) % count;
}
