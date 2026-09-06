using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Summing.Input;

public sealed class InputManager
{
    private KeyboardState _previousKeyboard;
    private KeyboardState _keyboard;
    private GamePadState _previousPad;
    private GamePadState _pad;
    private MouseState _previousMouse;
    private MouseState _mouse;
    private readonly HashSet<InputAction> _down = [];
    private readonly HashSet<InputAction> _pressed = [];
    private readonly HashSet<InputAction> _released = [];

    public InputManager(InputBindings bindings) => Bindings = bindings;

    public InputBindings Bindings { get; }
    public Vector2 Move { get; private set; }
    public Vector2 Aim { get; private set; } = Vector2.UnitX;
    public Point MousePosition => _mouse.Position;
    public bool GamePadConnected => _pad.IsConnected;

    public void Update(Vector2 playerScreenPosition, Vector2 displayScale)
    {
        _previousKeyboard = _keyboard;
        _previousPad = _pad;
        _previousMouse = _mouse;
        _keyboard = Keyboard.GetState();
        _pad = GamePad.GetState(PlayerIndex.One, GamePadDeadZone.IndependentAxes);
        _mouse = Mouse.GetState();
        _down.Clear();
        _pressed.Clear();
        _released.Clear();

        foreach (var (action, binding) in Bindings.Actions)
        {
            var down = _keyboard.IsKeyDown(binding.Key) || _pad.IsButtonDown(binding.Button);
            var wasDown = _previousKeyboard.IsKeyDown(binding.Key) || _previousPad.IsButtonDown(binding.Button);
            if (down) _down.Add(action);
            if (down && !wasDown) _pressed.Add(action);
            if (!down && wasDown) _released.Add(action);
        }

        SetMouseButton(InputAction.Dig, _mouse.LeftButton, _previousMouse.LeftButton);
        SetMouseButton(InputAction.Grapple, _mouse.RightButton, _previousMouse.RightButton);

        var digital = new Vector2(
            (Down(InputAction.Right) ? 1f : 0f) - (Down(InputAction.Left) ? 1f : 0f),
            (Down(InputAction.Down) ? 1f : 0f) - (Down(InputAction.Up) ? 1f : 0f));
        var stick = new Vector2(_pad.ThumbSticks.Left.X, -_pad.ThumbSticks.Left.Y);
        Move = stick.LengthSquared() > 0.09f ? stick : digital;
        if (Move.LengthSquared() > 1f) Move.Normalize();

        var rightStick = new Vector2(_pad.ThumbSticks.Right.X, -_pad.ThumbSticks.Right.Y);
        var mouseAim = new Vector2(_mouse.X / MathF.Max(0.01f, displayScale.X),
            _mouse.Y / MathF.Max(0.01f, displayScale.Y)) - playerScreenPosition;
        var requestedAim = rightStick.LengthSquared() > 0.16f ? rightStick : mouseAim;
        if (requestedAim.LengthSquared() > 0.01f) Aim = Vector2.Normalize(requestedAim);
    }

    public bool Down(InputAction action) => _down.Contains(action);
    public bool Pressed(InputAction action) => _pressed.Contains(action);
    public bool Released(InputAction action) => _released.Contains(action);
    public bool KeyDown(Keys key) => _keyboard.IsKeyDown(key);
    public bool KeyPressed(Keys key) => _keyboard.IsKeyDown(key) && !_previousKeyboard.IsKeyDown(key);
    public bool MouseLeftPressed => _mouse.LeftButton == ButtonState.Pressed && _previousMouse.LeftButton == ButtonState.Released;
    public bool MouseRightPressed => _mouse.RightButton == ButtonState.Pressed && _previousMouse.RightButton == ButtonState.Released;

    public bool TryGetPressedKey(out Keys key)
    {
        foreach (var candidate in _keyboard.GetPressedKeys())
        {
            if (_previousKeyboard.IsKeyUp(candidate)) { key = candidate; return true; }
        }
        key = Keys.None;
        return false;
    }

    public bool TryGetPressedButton(out Buttons button)
    {
        Buttons[] candidates =
        [
            Buttons.A, Buttons.B, Buttons.X, Buttons.Y, Buttons.LeftShoulder, Buttons.RightShoulder,
            Buttons.LeftStick, Buttons.RightStick, Buttons.DPadUp, Buttons.DPadDown, Buttons.DPadLeft,
            Buttons.DPadRight, Buttons.Start, Buttons.Back
        ];
        foreach (var candidate in candidates)
        {
            if (_pad.IsButtonDown(candidate) && _previousPad.IsButtonUp(candidate)) { button = candidate; return true; }
        }
        button = default;
        return false;
    }

    public void SetSyntheticState(Vector2 move, Vector2 aim, params InputAction[] downActions)
    {
        var previous = new HashSet<InputAction>(_down);
        _down.Clear();
        _pressed.Clear();
        _released.Clear();
        foreach (var action in downActions)
        {
            _down.Add(action);
            if (!previous.Contains(action)) _pressed.Add(action);
        }
        foreach (var action in previous)
            if (!_down.Contains(action)) _released.Add(action);
        Move = move;
        if (aim.LengthSquared() > 0.01f) Aim = Vector2.Normalize(aim);
    }

    private void SetMouseButton(InputAction action, ButtonState current, ButtonState previous)
    {
        if (current == ButtonState.Pressed) _down.Add(action);
        if (current == ButtonState.Pressed && previous == ButtonState.Released) _pressed.Add(action);
        if (current == ButtonState.Released && previous == ButtonState.Pressed) _released.Add(action);
    }
}
