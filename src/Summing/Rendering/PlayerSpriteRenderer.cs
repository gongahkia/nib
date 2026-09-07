using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Summing.Player;

namespace Summing.Rendering;

public sealed class PlayerSpriteRenderer : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly Dictionary<string, LoadedClip> _clips = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Texture2D> _textures = new(StringComparer.Ordinal);
    private readonly Dictionary<MovementState, PlayerStateProfile> _states = [];
    private readonly Rectangle _crop;
    private readonly Vector2 _footAnchor;
    private readonly float _scale;
    private readonly int _authoredFacing;

    public PlayerSpriteRenderer(GraphicsDevice graphicsDevice)
    {
        var (profile, profilePath) = LoadActiveProfile();
        var profileDirectory = Path.GetDirectoryName(profilePath)!;

        _crop = new Rectangle(profile.Crop.X, profile.Crop.Y, profile.Crop.Width, profile.Crop.Height);
        _footAnchor = new Vector2(profile.FootAnchor.X, profile.FootAnchor.Y);
        _scale = profile.Scale;
        _authoredFacing = profile.AuthoredFacing;

        foreach (var (name, clipProfile) in profile.Clips)
        {
            var texturePath = ResolveContainedPath(profileDirectory, clipProfile.File);
            if (!_textures.TryGetValue(texturePath, out var texture))
            {
                if (!File.Exists(texturePath))
                    throw new FileNotFoundException($"required player sprite sheet is missing: {texturePath}", texturePath);
                using var stream = File.OpenRead(texturePath);
                texture = Texture2D.FromStream(graphicsDevice, stream);
                _textures.Add(texturePath, texture);
            }

            var expectedWidth = clipProfile.Frames * profile.CellWidth;
            if (texture.Width != expectedWidth || texture.Height != profile.CellHeight)
                throw new InvalidDataException($"player sheet {clipProfile.File} must be {expectedWidth}x" +
                    $"{profile.CellHeight}, found {texture.Width}x{texture.Height}");
            _clips.Add(name, new LoadedClip(texture, profile.CellWidth));
        }

        foreach (var state in Enum.GetValues<MovementState>())
        {
            var binding = profile.States[state.ToString()];
            _states.Add(state, binding);
        }
    }

    public void Draw(SpriteBatch batch, PlayerController player, long frame)
    {
        var binding = _states[player.VisualState];
        var clip = _clips[binding.Clip];
        var animationFrame = binding.FirstFrame;
        if (binding.FrameCount > 1)
            animationFrame += (int)(frame / binding.TicksPerFrame % binding.FrameCount);

        var source = _crop;
        source.X += animationFrame * clip.CellStride;
        var flip = Math.Sign(player.Facing) != _authoredFacing;
        var effects = flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        var origin = FacingAnchor(_crop, _footAnchor, flip);
        var position = new Vector2(MathF.Round(player.Position.X), MathF.Round(player.Position.Y));
        batch.Draw(clip.Texture, position, source, Color.White, 0f, origin, _scale, effects, 0f);
    }

    public void Dispose()
    {
        foreach (var texture in _textures.Values) texture.Dispose();
    }

    public static string ActiveProfileId() => LoadActiveProfile().Profile.Id;

    internal static Vector2 FacingAnchor(Rectangle crop, Vector2 footAnchor, bool flipped) =>
        flipped ? new Vector2(crop.Width - footAnchor.X, footAnchor.Y) : footAnchor;

    private static void ValidateProfile(PlayerSpriteProfile profile, string path)
    {
        if (profile.SchemaVersion != 1 || string.IsNullOrWhiteSpace(profile.Id))
            throw new InvalidDataException($"unsupported player sprite profile: {path}");
        if (profile.CellWidth <= 0 || profile.CellHeight <= 0 || profile.Scale <= 0f)
            throw new InvalidDataException($"invalid cell size or scale in player sprite profile: {path}");
        if (profile.Crop.Width <= 0 || profile.Crop.Height <= 0 || profile.Crop.X < 0 || profile.Crop.Y < 0 ||
            profile.Crop.X + profile.Crop.Width > profile.CellWidth ||
            profile.Crop.Y + profile.Crop.Height > profile.CellHeight)
            throw new InvalidDataException($"player sprite crop lies outside its frame: {path}");
        if (profile.FootAnchor.X < 0f || profile.FootAnchor.X > profile.Crop.Width ||
            profile.FootAnchor.Y < 0f || profile.FootAnchor.Y > profile.Crop.Height)
            throw new InvalidDataException($"player sprite foot anchor lies outside its crop: {path}");
        if (profile.AuthoredFacing is not (-1 or 1))
            throw new InvalidDataException($"authoredFacing must be -1 or 1: {path}");
        if (!NearlyEqual(profile.Collision.StandingWidth, PlayerController.BodyWidth) ||
            !NearlyEqual(profile.Collision.StandingHeight, PlayerController.StandingBodyHeight) ||
            !NearlyEqual(profile.Collision.CrouchingHeight, PlayerController.CrouchingBodyHeight))
            throw new InvalidDataException($"player sprite profile collision contract does not match PlayerController: {path}");

        if (profile.Clips.Count == 0)
            throw new InvalidDataException($"player sprite profile contains no clips: {path}");
        foreach (var (name, clip) in profile.Clips)
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(clip.File) || clip.Frames <= 0)
                throw new InvalidDataException($"player sprite profile contains an invalid clip: {path}");

        foreach (var state in Enum.GetValues<MovementState>())
        {
            if (!profile.States.TryGetValue(state.ToString(), out var binding))
                throw new InvalidDataException($"player sprite profile is missing state {state}: {path}");
            if (!profile.Clips.TryGetValue(binding.Clip, out var clip))
                throw new InvalidDataException($"state {state} references missing clip {binding.Clip}: {path}");
            if (binding.FirstFrame < 0 || binding.FrameCount <= 0 || binding.TicksPerFrame <= 0 ||
                binding.FirstFrame + binding.FrameCount > clip.Frames)
                throw new InvalidDataException($"state {state} has an invalid frame range: {path}");
        }
    }

    private static bool NearlyEqual(float left, float right) => MathF.Abs(left - right) < 0.01f;

    private static (PlayerSpriteProfile Profile, string Path) LoadActiveProfile()
    {
        var playerRoot = Path.Combine(AppContext.BaseDirectory, "Sprites", "Player");
        var selectionPath = Path.Combine(playerRoot, "active-player.json");
        var selection = ReadJson<ActivePlayerProfile>(selectionPath);
        if (selection.SchemaVersion != 1 || string.IsNullOrWhiteSpace(selection.Profile))
            throw new InvalidDataException($"invalid active player sprite selection: {selectionPath}");
        var profilePath = ResolveContainedPath(playerRoot, selection.Profile);
        var profile = ReadJson<PlayerSpriteProfile>(profilePath);
        ValidateProfile(profile, profilePath);
        return (profile, profilePath);
    }

    private static T ReadJson<T>(string path)
    {
        if (!File.Exists(path)) throw new FileNotFoundException($"player sprite configuration is missing: {path}", path);
        return JsonSerializer.Deserialize<T>(File.ReadAllText(path), JsonOptions) ??
            throw new InvalidDataException($"player sprite configuration is empty: {path}");
    }

    private static string ResolveContainedPath(string root, string relativePath)
    {
        var fullRoot = Path.GetFullPath(root) + Path.DirectorySeparatorChar;
        var fullPath = Path.GetFullPath(Path.Combine(root, relativePath));
        if (!fullPath.StartsWith(fullRoot, StringComparison.Ordinal))
            throw new InvalidDataException($"player sprite path escapes its asset directory: {relativePath}");
        return fullPath;
    }

    private sealed record LoadedClip(Texture2D Texture, int CellStride);
}

internal sealed class ActivePlayerProfile
{
    public int SchemaVersion { get; init; }
    public string Profile { get; init; } = string.Empty;
}

internal sealed class PlayerSpriteProfile
{
    public int SchemaVersion { get; init; }
    public string Id { get; init; } = string.Empty;
    public int CellWidth { get; init; }
    public int CellHeight { get; init; }
    public PlayerSpriteRectangle Crop { get; init; } = new();
    public PlayerSpritePoint FootAnchor { get; init; } = new();
    public float Scale { get; init; }
    public int AuthoredFacing { get; init; }
    public PlayerCollisionProfile Collision { get; init; } = new();
    public Dictionary<string, PlayerClipProfile> Clips { get; init; } = [];
    public Dictionary<string, PlayerStateProfile> States { get; init; } = [];
}

internal sealed class PlayerSpriteRectangle
{
    public int X { get; init; }
    public int Y { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
}

internal sealed class PlayerSpritePoint
{
    public float X { get; init; }
    public float Y { get; init; }
}

internal sealed class PlayerCollisionProfile
{
    public float StandingWidth { get; init; }
    public float StandingHeight { get; init; }
    public float CrouchingHeight { get; init; }
}

internal sealed class PlayerClipProfile
{
    public string File { get; init; } = string.Empty;
    public int Frames { get; init; }
}

internal sealed class PlayerStateProfile
{
    public string Clip { get; init; } = string.Empty;
    public int FirstFrame { get; init; }
    public int FrameCount { get; init; }
    public int TicksPerFrame { get; init; }
}
