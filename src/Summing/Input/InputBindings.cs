using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework.Input;

namespace Summing.Input;

public sealed class ActionBinding
{
    public Keys Key { get; set; }
    public Buttons Button { get; set; }
}

public sealed class InputBindings
{
    public Dictionary<InputAction, ActionBinding> Actions { get; set; } = Defaults();

    public static InputBindings LoadOrCreate(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
        if (File.Exists(path))
        {
            try
            {
                return JsonSerializer.Deserialize<InputBindings>(File.ReadAllText(path), JsonOptions()) ?? new InputBindings();
            }
            catch (JsonException)
            {
                // invalid hand-edited bindings fall back to a playable default
            }
        }
        var bindings = new InputBindings();
        bindings.Save(path);
        return bindings;
    }

    public void Save(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
        File.WriteAllText(path, JsonSerializer.Serialize(this, JsonOptions()));
    }

    public void Rebind(InputAction action, Keys key, Buttons button) =>
        Actions[action] = new ActionBinding { Key = key, Button = button };

    private static JsonSerializerOptions JsonOptions() => new()
    {
        WriteIndented = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    private static Dictionary<InputAction, ActionBinding> Defaults() => new()
    {
        [InputAction.Left] = new() { Key = Keys.A, Button = Buttons.DPadLeft },
        [InputAction.Right] = new() { Key = Keys.D, Button = Buttons.DPadRight },
        [InputAction.Up] = new() { Key = Keys.W, Button = Buttons.DPadUp },
        [InputAction.Down] = new() { Key = Keys.S, Button = Buttons.DPadDown },
        [InputAction.Jump] = new() { Key = Keys.Space, Button = Buttons.A },
        [InputAction.Dash] = new() { Key = Keys.LeftShift, Button = Buttons.X },
        [InputAction.Grab] = new() { Key = Keys.LeftControl, Button = Buttons.LeftShoulder },
        [InputAction.Grapple] = new() { Key = Keys.G, Button = Buttons.RightShoulder },
        [InputAction.Dig] = new() { Key = Keys.F, Button = Buttons.Y },
        [InputAction.Slide] = new() { Key = Keys.C, Button = Buttons.B },
        [InputAction.Rope] = new() { Key = Keys.R, Button = Buttons.LeftStick },
        [InputAction.Bomb] = new() { Key = Keys.Q, Button = Buttons.RightStick },
        [InputAction.Interact] = new() { Key = Keys.E, Button = Buttons.A },
        [InputAction.Debug] = new() { Key = Keys.F1, Button = Buttons.Back },
        [InputAction.Pause] = new() { Key = Keys.Escape, Button = Buttons.Start }
    };
}
