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

    public void Update(Vector2 playerScreenPosition)
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
        var mouseAim = _mouse.Position.ToVector2() * 0.5f - playerScreenPosition;
        var requestedAim = rightStick.LengthSquared() > 0.16f ? rightStick : mouseAim;
        if (requestedAim.LengthSquared() > 0.01f) Aim = Vector2.Normalize(requestedAim);
    }

    public bool Down(InputAction action) => _down.Contains(action);
    public bool Pressed(InputAction action) => _pressed.Contains(action);
    public bool Released(InputAction action) => _released.Contains(action);

    private void SetMouseButton(InputAction action, ButtonState current, ButtonState previous)
    {
        if (current == ButtonState.Pressed) _down.Add(action);
        if (current == ButtonState.Pressed && previous == ButtonState.Released) _pressed.Add(action);
        if (current == ButtonState.Released && previous == ButtonState.Pressed) _released.Add(action);
    }
}
