using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Summing.Player;
using Summing.World;
using Summing.World.Materials;

namespace Summing.Rendering;

public enum VisualPackId
{
    SummingOriginal,
    GandalfOverworld
}

public sealed class VisualPackManager : IDisposable
{
    private const int CharacterCellWidth = 80;
    private const int CharacterCellHeight = 64;
    private const int ImportedTileSize = 32;
    private readonly Texture2D? _gandalfTerrain;
    private readonly Texture2D[] _gandalfCharacter;

    public VisualPackManager(GraphicsDevice graphicsDevice, VisualPackId requested)
    {
        Texture2D? terrain = null;
        Texture2D[] character = [];
        string? loadFailure = null;
        if (VisualPackFiles.TryResolveGandalf(out var files))
        {
            var loadedCharacter = new List<Texture2D>();
            try
            {
                terrain = Load(graphicsDevice, files.Terrain, 288, 288, removeFlatInterior: true);
                loadedCharacter.Add(Load(graphicsDevice, files.Skin, 800, 448));
                loadedCharacter.Add(Load(graphicsDevice, files.Pants, 800, 448));
                loadedCharacter.Add(Load(graphicsDevice, files.Shirt, 800, 448));
                loadedCharacter.Add(Load(graphicsDevice, files.Boots, 800, 448));
                loadedCharacter.Add(Load(graphicsDevice, files.Hair, 800, 448));
                character = loadedCharacter.ToArray();
            }
            catch (Exception exception) when (exception is IOException or InvalidOperationException or ArgumentException)
            {
                terrain?.Dispose();
                terrain = null;
                foreach (var texture in loadedCharacter) texture.Dispose();
                character = [];
                loadFailure = exception.Message;
            }
        }

        _gandalfTerrain = terrain;
        _gandalfCharacter = character;
        LoadFailure = loadFailure;

        Current = requested == VisualPackId.GandalfOverworld && GandalfAvailable
            ? requested
            : VisualPackId.SummingOriginal;
    }

    public VisualPackId Current { get; private set; }
    public bool GandalfAvailable => _gandalfTerrain != null && _gandalfCharacter.Length == 5;
    public string? LoadFailure { get; }
    public string CurrentName => DisplayName(Current);

    public static string DisplayName(VisualPackId id) => id switch
    {
        VisualPackId.GandalfOverworld => "GANDALF OVERWORLD",
        _ => "SUMMING ORIGINAL"
    };

    public string Cycle()
    {
        if (!GandalfAvailable)
        {
            Current = VisualPackId.SummingOriginal;
            return LoadFailure == null
                ? "GANDALF OVERWORLD NOT INSTALLED"
                : "GANDALF OVERWORLD COULD NOT LOAD";
        }

        Current = Current == VisualPackId.SummingOriginal
            ? VisualPackId.GandalfOverworld
            : VisualPackId.SummingOriginal;
        return $"ART PACK {CurrentName}";
    }

    public bool DrawTerrainOverlay(SpriteBatch batch, TileWorld world, Rectangle destination,
        MaterialDefinition material, int x, int y)
    {
        if (Current != VisualPackId.GandalfOverworld || _gandalfTerrain == null) return false;

        var leftExposed = !world.GetTile(x - 1, y).Solid;
        var rightExposed = !world.GetTile(x + 1, y).Solid;
        var topExposed = !world.GetTile(x, y - 1).Solid;
        var bottomExposed = !world.GetTile(x, y + 1).Solid;
        var sourceX = leftExposed ? 0 : rightExposed ? 2 : 1;
        var sourceY = topExposed ? 6 : bottomExposed ? 8 : 7;
        var tint = Color.Lerp(Color.White, material.BaseColor, 0.2f);
        DrawImportedTile(batch, destination, sourceX, sourceY, tint);

        // One-cell pillars and shelves expose both opposing faces. The atlas stores those faces separately.
        if (leftExposed && rightExposed) DrawImportedTile(batch, destination, 2, sourceY, tint);
        if (topExposed && bottomExposed) DrawImportedTile(batch, destination, sourceX, 8, tint);
        return true;
    }

    public bool DrawPlayer(SpriteBatch batch, MovementState state, Vector2 feet, int facing, long frame, Color tint)
    {
        if (Current != VisualPackId.GandalfOverworld || !GandalfAvailable) return false;

        var pose = ResolvePose(state, frame);
        var source = new Rectangle(pose.Column * CharacterCellWidth, pose.Row * CharacterCellHeight,
            CharacterCellWidth, CharacterCellHeight);
        // Half-scale preserves the source pixels while keeping the visible figure approximately one 24px tile tall.
        var destination = new Rectangle((int)MathF.Round(feet.X - 20f), (int)MathF.Round(feet.Y - 32f), 40, 32);
        if (state is MovementState.Crouch or MovementState.Crawl)
            destination = new Rectangle(destination.X + 5, destination.Bottom - 22, 30, 22);
        else if (state == MovementState.Slide)
            destination = new Rectangle(destination.X - 2, destination.Bottom - 22, 44, 22);

        // The imported source faces left by default, opposite Summing's original player sheet.
        var effects = facing > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        var silhouette = new Color(7, 9, 14, 220);
        DrawCharacterLayers(batch, destination with { X = destination.X - 1 }, source, silhouette, effects);
        DrawCharacterLayers(batch, destination with { X = destination.X + 1 }, source, silhouette, effects);
        DrawCharacterLayers(batch, destination with { Y = destination.Y + 1 }, source, silhouette, effects);
        DrawCharacterLayers(batch, destination, source, Color.Lerp(Color.White, tint, 0.18f), effects);
        return true;
    }

    public void Dispose()
    {
        _gandalfTerrain?.Dispose();
        foreach (var texture in _gandalfCharacter) texture.Dispose();
    }

    private void DrawImportedTile(SpriteBatch batch, Rectangle destination, int column, int row, Color tint)
    {
        var source = new Rectangle(column * ImportedTileSize, row * ImportedTileSize,
            ImportedTileSize, ImportedTileSize);
        batch.Draw(_gandalfTerrain!, destination, source, tint);
    }

    private void DrawCharacterLayers(SpriteBatch batch, Rectangle destination, Rectangle source, Color tint,
        SpriteEffects effects)
    {
        foreach (var texture in _gandalfCharacter)
            batch.Draw(texture, destination, source, tint, 0f, Vector2.Zero, effects, 0f);
    }

    private static (int Row, int Column) ResolvePose(MovementState state, long frame) => state switch
    {
        MovementState.Idle => (0, (int)(frame / 12 % 5)),
        MovementState.Run => (2, (int)(frame / 4 % 8)),
        MovementState.Takeoff => (3, 0),
        MovementState.AscendingJump => (3, Math.Min(2, (int)(frame / 6 % 4))),
        MovementState.Apex => (3, 3),
        MovementState.Falling => (4, (int)(frame / 7 % 4)),
        MovementState.Landing => (0, 1),
        MovementState.Crouch => (6, 2),
        MovementState.Crawl => (6, 3 + (int)(frame / 9 % 2)),
        MovementState.Slide => (6, 6),
        MovementState.LedgeHang => (4, 0),
        MovementState.Mantle => (3, 2),
        MovementState.WallCling => (4, (int)(frame / 9 % 4)),
        MovementState.WallJump => (3, 1),
        MovementState.Dash => (2, (int)(frame / 3 % 8)),
        MovementState.RopeInteraction => (4, (int)(frame / 9 % 4)),
        MovementState.Digging => (5, (int)(frame / 4 % 6)),
        MovementState.BombUse => (5, 1 + (int)(frame / 5 % 4)),
        MovementState.Hurt => (6, 1),
        MovementState.Stunned => (6, 3),
        MovementState.Death => (6, (int)(frame / 7 % 10)),
        _ => (0, 0)
    };

    private static Texture2D Load(GraphicsDevice graphicsDevice, string path, int minimumWidth, int minimumHeight,
        bool removeFlatInterior = false)
    {
        using var stream = File.OpenRead(path);
        var texture = Texture2D.FromStream(graphicsDevice, stream);
        if (texture.Width < minimumWidth || texture.Height < minimumHeight)
        {
            var actual = $"{texture.Width}x{texture.Height}";
            texture.Dispose();
            throw new InvalidDataException($"Art pack texture '{path}' is {actual}; expected at least {minimumWidth}x{minimumHeight}.");
        }
        if (!removeFlatInterior) return texture;

        var pixels = new Color[texture.Width * texture.Height];
        texture.GetData(pixels);
        for (var index = 0; index < pixels.Length; index++)
            if (pixels[index] == new Color(19, 17, 22, 255)) pixels[index] = Color.Transparent;
        texture.SetData(pixels);
        return texture;
    }
}

public static class VisualPackFiles
{
    public readonly record struct GandalfFiles(string Terrain, string Skin, string Pants, string Shirt,
        string Boots, string Hair);

    private const string PlatformerDirectory =
        "third_party/art/local-only/gandalfhardcore-platformer/GandalfHardcore FREE Platformer Assets";
    private const string CharacterDirectory =
        "third_party/art/local-only/gandalfhardcore-character/GandalfHardcore Character Asset Pack";

    public static bool IsAvailable(VisualPackId id) => id == VisualPackId.SummingOriginal || TryResolveGandalf(out _);

    public static bool TryResolveGandalf(out GandalfFiles files)
    {
        foreach (var root in CandidateRoots())
        {
            var platformer = Path.Combine(root, PlatformerDirectory);
            var character = Path.Combine(root, CharacterDirectory);
            var candidate = new GandalfFiles(
                Path.Combine(platformer, "Floor Tiles2.png"),
                Path.Combine(character, "Character skin colors", "Male Skin1.png"),
                Path.Combine(character, "Male Clothing", "Pants.png"),
                Path.Combine(character, "Male Clothing", "Shirt v2.png"),
                Path.Combine(character, "Male Clothing", "Boots.png"),
                Path.Combine(character, "Male Hair", "Male Hair1.png"));
            if (AllFilesExist(candidate))
            {
                files = candidate;
                return true;
            }
        }

        files = default;
        return false;
    }

    private static IEnumerable<string> CandidateRoots()
    {
        var configured = Environment.GetEnvironmentVariable("SUMMING_ART_ROOT");
        if (!string.IsNullOrWhiteSpace(configured)) yield return Path.GetFullPath(configured);

        var visited = new HashSet<string>(StringComparer.Ordinal);
        foreach (var origin in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            var directory = new DirectoryInfo(origin);
            for (var depth = 0; directory != null && depth < 12; depth++, directory = directory.Parent)
                if (visited.Add(directory.FullName)) yield return directory.FullName;
        }
    }

    private static bool AllFilesExist(GandalfFiles files) =>
        File.Exists(files.Terrain) && File.Exists(files.Skin) && File.Exists(files.Pants) &&
        File.Exists(files.Shirt) && File.Exists(files.Boots) && File.Exists(files.Hair);
}
