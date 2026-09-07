using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Summing.Camera;
using Summing.Entities;
using Summing.Gameplay;
using Summing.Generation;
using Summing.Input;
using Summing.Player;
using Summing.Rendering;
using Summing.World;

namespace Summing.Telemetry;

public sealed class PlaytestRecorder : IDisposable
{
    private sealed record ScreenshotRequest(long DueFrame, string Event, string Phase);
    private readonly StreamWriter _frames;
    private readonly StreamWriter _events;
    private readonly StreamWriter _screenshots;
    private readonly List<ScreenshotRequest> _pendingScreenshots = [];
    private readonly JsonSerializerOptions _json = new()
    {
        WriteIndented = true,
        IncludeFields = true,
        Converters = { new JsonStringEnumConverter() }
    };
    private readonly JsonSerializerOptions _jsonLine = new()
    {
        WriteIndented = false,
        IncludeFields = true,
        Converters = { new JsonStringEnumConverter() }
    };
    private long _frame;
    private int _screenshotSequence;
    private bool _disposed;
    private int _eventCount;

    public PlaytestRecorder(GeneratedWorld generated, Difficulty difficulty, InputBindings bindings,
        VisualSelectionSnapshot visualSelection)
    {
        var name = $"{DateTimeOffset.Now:yyyyMMdd-HHmmss-fff}-{generated.Configuration.Seed}";
        DirectoryPath = Path.Combine("playtests", name);
        var suffix = 1;
        while (Directory.Exists(DirectoryPath)) DirectoryPath = Path.Combine("playtests", $"{name}-{suffix++}");
        Directory.CreateDirectory(DirectoryPath);
        Directory.CreateDirectory(Path.Combine(DirectoryPath, "screenshots"));
        _frames = new StreamWriter(Path.Combine(DirectoryPath, "frames.csv"));
        _events = new StreamWriter(Path.Combine(DirectoryPath, "events.jsonl"));
        _screenshots = new StreamWriter(Path.Combine(DirectoryPath, "screenshots", "index.csv"));
        _frames.WriteLine("frame,time_s,input_down,input_pressed,input_released,move_x,move_y,aim_x,aim_y,player_x,player_y,body_width,body_height,velocity_x,velocity_y,movement_state,visual_state,camera_x,camera_y,lookahead_x,lookahead_y,shake_x,shake_y,shake_strength,grounded,ceiling,left_wall,right_wall,left_climbable,right_climbable,collision_normals,wall_stamina,dash_charges,coyote_used,jump_buffer_used,health,stunned,bombs,ropes,wind,nearby_tiles,route_anchor,altitude_band");
        _screenshots.WriteLine("frame,time_s,file,event,phase");
        File.WriteAllText(Path.Combine(DirectoryPath, "metadata.json"), JsonSerializer.Serialize(new
        {
            schemaVersion = 5,
            startedAtUtc = DateTimeOffset.UtcNow,
            seed = generated.Configuration.Seed,
            generator = WorldGeneratorRegistry.Identifier(generated.Configuration.Variant),
            generated.Configuration,
            difficulty,
            visualSelection,
            historyId = generated.History.Id,
            epoch = generated.History.EpochName,
            generated.Diagnostics,
            inputBindings = bindings.Actions,
            fixedUpdateHz = 60,
            cameraWorldZoom = Camera2D.WorldZoom,
            periodicScreenshotFrames = 600,
            eventScreenshotPhases = new[] { "event", "post-12-frames" },
            manualBookmark = "F8 on keyboard or right shoulder on gamepad",
            notes = "positions are world pixels; tile size and full starting world are in world-start.json"
        }, _json));
        RequestScreenshot("session-start", true);
    }

    public string DirectoryPath { get; private set; }
    public int EventCount => _eventCount;
    public bool IsFinished => _disposed;

    public void BeginFrame(long frame) => _frame = frame;

    public void RecordFrame(long frame, InputManager input, PlayerController player, Camera2D camera, TileWorld world,
        PlayerInventory inventory, GeneratedWorld generated)
    {
        _frame = frame;
        var actions = string.Join('|', Enum.GetValues<InputAction>().Where(input.Down));
        var pressed = string.Join('|', Enum.GetValues<InputAction>().Where(input.Pressed));
        var released = string.Join('|', Enum.GetValues<InputAction>().Where(input.Released));
        var contacts = new List<string>();
        if (player.Grounded) contacts.Add("0:-1");
        if (player.TouchingCeiling) contacts.Add("0:1");
        if (player.TouchingLeftWall) contacts.Add("1:0");
        if (player.TouchingRightWall) contacts.Add("-1:0");
        var normals = string.Join('|', contacts);
        var nearby = NearbyTiles(world, player);
        var routeAnchor = NearestRouteAnchor(generated, player.Position);
        var altitude = 1f - player.Position.Y / world.PixelHeight;
        var altitudeBand = altitude < 0.34f ? "lower" : altitude < 0.67f ? "middle" : "upper";
        _frames.WriteLine(string.Join(',', frame, F(frame / 60f), Csv(actions), Csv(pressed), Csv(released), F(input.Move.X), F(input.Move.Y),
            F(input.Aim.X), F(input.Aim.Y), F(player.Position.X), F(player.Position.Y), F(player.Bounds.Width),
            F(player.Bounds.Height), F(player.Velocity.X),
            F(player.Velocity.Y), player.State, player.VisualState, F(camera.Position.X), F(camera.Position.Y),
            F(camera.LookAhead.X), F(camera.LookAhead.Y), F(camera.ShakeOffset.X), F(camera.ShakeOffset.Y),
            F(camera.ShakeStrength), player.Grounded, player.TouchingCeiling, player.TouchingLeftWall,
            player.TouchingRightWall, player.LeftWallClimbable, player.RightWallClimbable, Csv(normals), F(player.WallStamina), player.DashCharges,
            player.UsedCoyoteThisFrame, player.UsedJumpBufferThisFrame, player.Health, player.Stunned, inventory.Bombs,
            inventory.Ropes, F(generated.WindAt(player.Position.Y)), Csv(nearby), routeAnchor, altitudeBand));
        if (frame % 120 == 0) _frames.Flush();
        if (frame % 600 == 0) RequestScreenshot("periodic", false);
    }

    public void RecordEvent(string type, object data, bool capture = true)
    {
        _eventCount++;
        _events.WriteLine(JsonSerializer.Serialize(new { frame = _frame, timeSeconds = _frame / 60f, type, data }, _jsonLine));
        _events.Flush();
        if (capture) RequestScreenshot(type, true);
    }

    public void CaptureDue(RenderTarget2D scene)
    {
        for (var index = _pendingScreenshots.Count - 1; index >= 0; index--)
        {
            var request = _pendingScreenshots[index];
            if (request.DueFrame > _frame) continue;
            var safeEvent = new string(request.Event.Select(character => char.IsLetterOrDigit(character) ? character : '-').ToArray()).Trim('-');
            var fileName = $"{_screenshotSequence++:D5}-f{_frame:D8}-{safeEvent}-{request.Phase}.png";
            var relative = Path.Combine("screenshots", fileName);
            using var stream = File.Create(Path.Combine(DirectoryPath, relative));
            scene.SaveAsPng(stream, scene.Width, scene.Height);
            _screenshots.WriteLine($"{_frame},{F(_frame / 60f)},{relative},{Csv(request.Event)},{request.Phase}");
            _screenshots.Flush();
            _pendingScreenshots.RemoveAt(index);
        }
    }

    public void Finish(string outcome)
    {
        if (_disposed) return;
        File.WriteAllText(Path.Combine(DirectoryPath, "summary.json"), JsonSerializer.Serialize(new
        {
            endedAtUtc = DateTimeOffset.UtcNow,
            finalFrame = _frame,
            durationSeconds = _frame / 60f,
            outcome,
            eventCount = _eventCount,
            screenshotCount = _screenshotSequence
        }, _json));
        Dispose();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _frames.Dispose();
        _events.Dispose();
        _screenshots.Dispose();
    }

    private void RequestScreenshot(string eventName, bool withPost)
    {
        QueueScreenshot(_frame, eventName, "event");
        if (withPost) QueueScreenshot(_frame + 12, eventName, "post");
    }

    private void QueueScreenshot(long dueFrame, string eventName, string phase)
    {
        var index = _pendingScreenshots.FindIndex(request => request.DueFrame == dueFrame && request.Phase == phase);
        if (index < 0)
        {
            _pendingScreenshots.Add(new ScreenshotRequest(dueFrame, eventName, phase));
            return;
        }
        var current = _pendingScreenshots[index];
        var names = current.Event.Split('+', StringSplitOptions.RemoveEmptyEntries);
        if (!names.Contains(eventName, StringComparer.Ordinal))
            _pendingScreenshots[index] = current with { Event = $"{current.Event}+{eventName}" };
    }

    private static string NearbyTiles(TileWorld world, PlayerController player)
    {
        var center = world.WorldToTile(player.Bounds.Center);
        var rows = new List<string>();
        for (var y = center.Y - 2; y <= center.Y + 2; y++)
        {
            var row = new List<string>();
            for (var x = center.X - 3; x <= center.X + 3; x++)
            {
                var tile = world.GetTile(x, y);
                row.Add($"{x}:{y}:{tile.Material}:{tile.Damage}");
            }
            rows.Add(string.Join('|', row));
        }
        return string.Join('/', rows);
    }

    private static int NearestRouteAnchor(GeneratedWorld generated, Vector2 position)
    {
        var tile = generated.Terrain.WorldToTile(position);
        var nearest = 0;
        var best = int.MaxValue;
        for (var index = 0; index < generated.RouteAnchors.Count; index++)
        {
            var anchor = generated.RouteAnchors[index];
            var distance = Math.Abs(anchor.X - tile.X) + Math.Abs(anchor.Y - tile.Y);
            if (distance >= best) continue;
            best = distance;
            nearest = index;
        }
        return nearest;
    }

    private static string F(float value) => value.ToString("0.###", CultureInfo.InvariantCulture);
    private static string Csv(string value) => $"\"{value.Replace("\"", "\"\"")}\"";
}
