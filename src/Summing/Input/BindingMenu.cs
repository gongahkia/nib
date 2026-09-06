using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Summing.Rendering;

namespace Summing.Input;

public sealed class BindingMenu
{
    private readonly InputAction[] _actions = Enum.GetValues<InputAction>();
    private int _selected;
    private bool _waiting;
    private bool _gamepad;

    public bool Update(InputManager input, string path)
    {
        if (_waiting)
        {
            var binding = input.Bindings.Actions[_actions[_selected]];
            if (!_gamepad && input.TryGetPressedKey(out var key))
            {
                input.Bindings.Rebind(_actions[_selected], key, binding.Button);
                input.Bindings.Save(path);
                _waiting = false;
            }
            else if (_gamepad && input.TryGetPressedButton(out var button))
            {
                input.Bindings.Rebind(_actions[_selected], binding.Key, button);
                input.Bindings.Save(path);
                _waiting = false;
            }
            return false;
        }

        if (input.Pressed(InputAction.Up)) _selected = (_selected - 1 + _actions.Length) % _actions.Length;
        if (input.Pressed(InputAction.Down)) _selected = (_selected + 1) % _actions.Length;
        if (input.KeyPressed(Keys.Tab) || input.Pressed(InputAction.Left) || input.Pressed(InputAction.Right))
            _gamepad = !_gamepad;
        if (input.KeyPressed(Keys.Enter) || input.Pressed(InputAction.Jump)) _waiting = true;
        return input.KeyPressed(Keys.Escape) || input.KeyPressed(Keys.B) || input.Pressed(InputAction.Pause);
    }

    public void Draw(SpriteBatch batch, Texture2D pixel, PixelFont font, InputBindings bindings)
    {
        batch.Draw(pixel, new Rectangle(0, 0, 640, 360), GamePalette.Void);
        font.Draw(batch, "REMAP CONTROLS", new Vector2(198, 30), GamePalette.SacredGold, 3);
        font.Draw(batch, $"EDITING {(_gamepad ? "GAMEPAD" : "KEYBOARD")}  LEFT RIGHT SWITCH DEVICE", new Vector2(160, 58), GamePalette.UiMuted);
        for (var index = 0; index < _actions.Length; index++)
        {
            var column = index / 8;
            var row = index % 8;
            var position = new Vector2(86 + column * 280, 86 + row * 25);
            var selected = index == _selected;
            if (selected) batch.Draw(pixel, new Rectangle((int)position.X - 8, (int)position.Y - 7, 246, 18), new Color(48, 45, 55));
            var binding = bindings.Actions[_actions[index]];
            font.Draw(batch, _actions[index].ToString(), position, selected ? GamePalette.SaltCyan : Color.White);
            font.Draw(batch, _gamepad ? binding.Button.ToString() : binding.Key.ToString(), position + new Vector2(112, 0), GamePalette.Bone);
        }
        var instruction = _waiting ? $"PRESS A {(_gamepad ? "GAMEPAD BUTTON" : "KEY")}" : "UP DOWN SELECT  ENTER OR A REBIND  START BACK";
        font.Draw(batch, instruction, new Vector2(160, 318), _waiting ? GamePalette.Danger : GamePalette.UiMuted);
    }
}
