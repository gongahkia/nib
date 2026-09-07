using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Summing.Player;
using Summing.World;
using Summing.World.Materials;

namespace Summing.Rendering;

// Kept as the command-line startup preset. Runtime selection is split by player and environment below.
public enum VisualPackId
{
    SummingOriginal,
    GandalfOverworld
}

public enum PlayerVisualPackId
{
    SummingOriginal,
    AsepritePilgrim,
    GandalfMale,
    GandalfFemale,
    MerakintsugiSample
}

public enum EnvironmentVisualPackId
{
    SummingOriginal,
    GandalfOverworld,
    Adve,
    MonochromeCaves,
    PixelFantasyCaves,
    StoneRuins,
    DraculasManor
}

public enum TerrainSheetId
{
    FloorTiles1,
    FloorTiles2
}

public enum TerrainBandId
{
    Green,
    Autumn,
    Snow
}

public enum VisualSelectionOption
{
    PlayerPack,
    SkinSheet,
    LegSheet,
    TorsoSheet,
    FootSheet,
    HairSheet,
    EnvironmentPack,
    TerrainSheet,
    TerrainBand
}

public sealed record VisualSelectionSnapshot(
    PlayerVisualPackId PlayerPack,
    string SkinSheet,
    string LegSheet,
    string TorsoSheet,
    string FootSheet,
    string HairSheet,
    EnvironmentVisualPackId EnvironmentPack,
    TerrainSheetId TerrainSheet,
    TerrainBandId TerrainBand);

public readonly record struct VisualSelectionChange(bool Changed, string Notice, VisualSelectionOption Option);

public sealed class VisualPackManager : IDisposable
{
    private const string None = "NONE";
    private const int CharacterCellWidth = 80;
    private const int CharacterCellHeight = 64;
    private const int ImportedTileSize = 32;
    private static readonly Point[] PixelFantasyTopCells =
        [new(16, 560), new(144, 624), new(208, 624), new(272, 624), new(336, 624)];
    private static readonly Point[] PixelFantasyBottomCells =
        [new(144, 944), new(208, 944), new(304, 944), new(416, 944)];
    private static readonly int[] DraculasManorInteriorCells = [16, 17, 18, 19, 25];
    private readonly GraphicsDevice _graphicsDevice;
    private readonly string _selectionPath;
    private readonly bool _persistSelection;
    private readonly VisualPackFiles.GandalfFiles? _gandalfFiles;
    private readonly VisualPackFiles.MerakintsugiFiles? _merakintsugiFiles;
    private Texture2D? _environmentTexture;
    private Texture2D[] _gandalfCharacter = [];
    private Texture2D? _merakintsugiIdle;
    private Texture2D? _merakintsugiWalk;
    private string[] _skinChoices = [];
    private string[] _legChoices = [None];
    private string[] _torsoChoices = [None];
    private string[] _footChoices = [None];
    private string[] _hairChoices = [None];
    private string? _loadFailure;

    public VisualPackManager(GraphicsDevice graphicsDevice, VisualPackId requested,
        string selectionPath = "saves/visual-selection.json", bool loadSavedSelection = true,
        bool persistSelection = true)
    {
        _graphicsDevice = graphicsDevice;
        _selectionPath = selectionPath;
        _persistSelection = persistSelection;
        _gandalfFiles = VisualPackFiles.TryResolveGandalf(out var files) ? files : null;
        _merakintsugiFiles = VisualPackFiles.TryResolveMerakintsugi(out var merakintsugiFiles)
            ? merakintsugiFiles
            : null;
        PlayerPack = requested == VisualPackId.GandalfOverworld && GandalfCharacterAvailable
            ? PlayerVisualPackId.GandalfMale
            : PlayerVisualPackId.SummingOriginal;
        EnvironmentPack = requested == VisualPackId.GandalfOverworld && GandalfEnvironmentAvailable
            ? EnvironmentVisualPackId.GandalfOverworld
            : EnvironmentVisualPackId.SummingOriginal;

        VisualSelectionSnapshot? saved = null;
        if (loadSavedSelection) saved = LoadSelection();
        if (saved != null)
        {
            PlayerPack = saved.PlayerPack;
            EnvironmentPack = saved.EnvironmentPack;
            TerrainSheet = saved.TerrainSheet;
            TerrainBand = saved.TerrainBand;
        }
        NormalizePackAvailability();
        BuildCharacterChoices(saved == null);
        if (saved != null && UsesModularCharacter)
        {
            SkinSheet = SelectExisting(_skinChoices, saved.SkinSheet, SkinSheet);
            LegSheet = SelectExisting(_legChoices, saved.LegSheet, LegSheet);
            TorsoSheet = SelectExisting(_torsoChoices, saved.TorsoSheet, TorsoSheet);
            FootSheet = SelectExisting(_footChoices, saved.FootSheet, FootSheet);
            HairSheet = SelectExisting(_hairChoices, saved.HairSheet, HairSheet);
        }
        ReloadEnvironment();
        ReloadCharacter();
    }

    public PlayerVisualPackId PlayerPack { get; private set; }
    public EnvironmentVisualPackId EnvironmentPack { get; private set; }
    public TerrainSheetId TerrainSheet { get; private set; } = TerrainSheetId.FloorTiles2;
    public TerrainBandId TerrainBand { get; private set; } = TerrainBandId.Autumn;
    public string SkinSheet { get; private set; } = "PACK CONTROLLED";
    public string LegSheet { get; private set; } = None;
    public string TorsoSheet { get; private set; } = None;
    public string FootSheet { get; private set; } = None;
    public string HairSheet { get; private set; } = None;
    public bool GandalfCharacterAvailable => _gandalfFiles is { } files && Directory.Exists(files.CharacterDirectory);
    public bool MerakintsugiCharacterAvailable => _merakintsugiFiles is not null;
    public bool GandalfEnvironmentAvailable => EnvironmentAvailable(EnvironmentVisualPackId.GandalfOverworld);
    public string? LoadFailure => _loadFailure;
    public int AvailablePlayerPackCount => Enum.GetValues<PlayerVisualPackId>().Count(PlayerAvailable);
    public int AvailableEnvironmentPackCount => Enum.GetValues<EnvironmentVisualPackId>().Count(EnvironmentAvailable);
    public string PlayerName => DisplayName(PlayerPack);
    public string EnvironmentName => EnvironmentPack switch
    {
        EnvironmentVisualPackId.GandalfOverworld =>
            $"GANDALF {DisplayName(TerrainSheet)} {TerrainBand.ToString().ToUpperInvariant()}",
        EnvironmentVisualPackId.Adve => $"ADVE {AdveBandName()}",
        EnvironmentVisualPackId.MonochromeCaves => "MONOCHROME CAVES",
        EnvironmentVisualPackId.PixelFantasyCaves => "PIXEL FANTASY CAVES",
        EnvironmentVisualPackId.StoneRuins => "STONE RUINS",
        EnvironmentVisualPackId.DraculasManor => "DRACULA'S MANOR",
        _ => "SUMMING ORIGINAL"
    };
    public string CurrentName => $"P {PlayerName}  E {EnvironmentName}";
    public VisualSelectionSnapshot Selection => new(PlayerPack, SkinSheet, LegSheet, TorsoSheet, FootSheet,
        HairSheet, EnvironmentPack, TerrainSheet, TerrainBand);

    public static string DisplayName(VisualPackId id) => id switch
    {
        VisualPackId.GandalfOverworld => "GANDALF OVERWORLD",
        _ => "SUMMING ORIGINAL"
    };

    public static string DisplayName(PlayerVisualPackId id) => id switch
    {
        PlayerVisualPackId.AsepritePilgrim => "ASHEN PILGRIM",
        PlayerVisualPackId.GandalfMale => "GANDALF MALE",
        PlayerVisualPackId.GandalfFemale => "GANDALF FEMALE",
        PlayerVisualPackId.MerakintsugiSample => "MERAKINTSUGI SAMPLE",
        _ => "SUMMING ORIGINAL"
    };

    public static string DisplayName(EnvironmentVisualPackId id) => id switch
    {
        EnvironmentVisualPackId.GandalfOverworld => "GANDALF OVERWORLD",
        EnvironmentVisualPackId.Adve => "ADVE",
        EnvironmentVisualPackId.MonochromeCaves => "MONOCHROME CAVES",
        EnvironmentVisualPackId.PixelFantasyCaves => "PIXEL FANTASY CAVES",
        EnvironmentVisualPackId.StoneRuins => "STONE RUINS",
        EnvironmentVisualPackId.DraculasManor => "DRACULA'S MANOR",
        _ => "SUMMING ORIGINAL"
    };

    public static string DisplayName(TerrainSheetId id) => id == TerrainSheetId.FloorTiles1
        ? "FLOOR TILES 1"
        : "FLOOR TILES 2";

    public static VisualSelectionSnapshot StartupSelection(VisualPackId preset) =>
        preset == VisualPackId.GandalfOverworld
            ? new VisualSelectionSnapshot(PlayerVisualPackId.GandalfMale, "Male Skin1.png", "Pants.png",
                "Shirt v2.png", "Boots.png", "Male Hair1.png", EnvironmentVisualPackId.GandalfOverworld,
                TerrainSheetId.FloorTiles2, TerrainBandId.Autumn)
            : new VisualSelectionSnapshot(PlayerVisualPackId.SummingOriginal, "PACK CONTROLLED", "PACK CONTROLLED",
                "PACK CONTROLLED", "PACK CONTROLLED", "PACK CONTROLLED", EnvironmentVisualPackId.SummingOriginal,
                TerrainSheetId.FloorTiles2, TerrainBandId.Autumn);

    public string ValueName(VisualSelectionOption option) => option switch
    {
        VisualSelectionOption.PlayerPack => PlayerName,
        VisualSelectionOption.SkinSheet => !UsesModularCharacter ? "PACK CONTROLLED" : SkinSheet,
        VisualSelectionOption.LegSheet => !UsesModularCharacter ? "PACK CONTROLLED" : LegSheet,
        VisualSelectionOption.TorsoSheet => !UsesModularCharacter ? "PACK CONTROLLED" : TorsoSheet,
        VisualSelectionOption.FootSheet => !UsesModularCharacter ? "PACK CONTROLLED" : FootSheet,
        VisualSelectionOption.HairSheet => !UsesModularCharacter ? "PACK CONTROLLED" : HairSheet,
        VisualSelectionOption.EnvironmentPack => DisplayName(EnvironmentPack),
        VisualSelectionOption.TerrainSheet => EnvironmentPack != EnvironmentVisualPackId.GandalfOverworld
            ? "PACK CONTROLLED"
            : DisplayName(TerrainSheet),
        VisualSelectionOption.TerrainBand => EnvironmentPack switch
        {
            EnvironmentVisualPackId.GandalfOverworld => TerrainBand.ToString().ToUpperInvariant(),
            EnvironmentVisualPackId.Adve => AdveBandName(),
            _ => "PACK CONTROLLED"
        },
        _ => ""
    };

    public bool OptionAvailable(VisualSelectionOption option) => option switch
    {
        VisualSelectionOption.SkinSheet or VisualSelectionOption.LegSheet or VisualSelectionOption.TorsoSheet or
            VisualSelectionOption.FootSheet or VisualSelectionOption.HairSheet =>
            UsesModularCharacter && GandalfCharacterAvailable,
        VisualSelectionOption.TerrainSheet => EnvironmentPack == EnvironmentVisualPackId.GandalfOverworld &&
            GandalfEnvironmentAvailable,
        VisualSelectionOption.TerrainBand => EnvironmentPack is EnvironmentVisualPackId.GandalfOverworld or
            EnvironmentVisualPackId.Adve,
        _ => true
    };

    public VisualSelectionChange Adjust(VisualSelectionOption option, int direction)
    {
        if (direction == 0) return new VisualSelectionChange(false, "", option);
        var before = Selection;
        switch (option)
        {
            case VisualSelectionOption.PlayerPack:
                PlayerPack = NextPlayerPack(PlayerPack, direction);
                BuildCharacterChoices(true);
                ReloadCharacter();
                break;
            case VisualSelectionOption.SkinSheet:
                if (OptionAvailable(option)) SkinSheet = Cycle(_skinChoices, SkinSheet, direction);
                ReloadCharacter();
                break;
            case VisualSelectionOption.LegSheet:
                if (OptionAvailable(option)) LegSheet = Cycle(_legChoices, LegSheet, direction);
                ReloadCharacter();
                break;
            case VisualSelectionOption.TorsoSheet:
                if (OptionAvailable(option)) TorsoSheet = Cycle(_torsoChoices, TorsoSheet, direction);
                ReloadCharacter();
                break;
            case VisualSelectionOption.FootSheet:
                if (OptionAvailable(option)) FootSheet = Cycle(_footChoices, FootSheet, direction);
                ReloadCharacter();
                break;
            case VisualSelectionOption.HairSheet:
                if (OptionAvailable(option)) HairSheet = Cycle(_hairChoices, HairSheet, direction);
                ReloadCharacter();
                break;
            case VisualSelectionOption.EnvironmentPack:
                EnvironmentPack = NextEnvironmentPack(EnvironmentPack, direction);
                ReloadEnvironment();
                break;
            case VisualSelectionOption.TerrainSheet:
                if (OptionAvailable(option)) TerrainSheet = CycleEnum(TerrainSheet, direction);
                ReloadEnvironment();
                break;
            case VisualSelectionOption.TerrainBand:
                if (OptionAvailable(option)) TerrainBand = CycleEnum(TerrainBand, direction);
                break;
        }

        var changed = before != Selection;
        if (changed) SaveSelection();
        var notice = changed ? $"{OptionLabel(option)}  {ValueName(option)}" : OptionAvailable(option)
            ? "NO OTHER INSTALLED CHOICE"
            : "SELECT AN IMPORTED PACK FIRST";
        return new VisualSelectionChange(changed, notice, option);
    }

    public bool DrawTerrainOverlay(SpriteBatch batch, TileWorld world, Rectangle destination,
        MaterialDefinition material, int x, int y)
    {
        if (_environmentTexture == null) return false;

        return EnvironmentPack switch
        {
            EnvironmentVisualPackId.GandalfOverworld => DrawGandalfTile(batch, world, destination, material, x, y),
            EnvironmentVisualPackId.Adve => DrawAdveTile(batch, world, destination, material, x, y),
            EnvironmentVisualPackId.MonochromeCaves =>
                DrawMonochromeTile(batch, world, destination, material, x, y),
            EnvironmentVisualPackId.PixelFantasyCaves =>
                DrawPixelFantasyTile(batch, world, destination, material, x, y),
            EnvironmentVisualPackId.StoneRuins => DrawStoneRuinsTile(batch, world, destination, material, x, y),
            EnvironmentVisualPackId.DraculasManor =>
                DrawDraculasManorTile(batch, world, destination, material, x, y),
            _ => false
        };
    }

    private bool DrawGandalfTile(SpriteBatch batch, TileWorld world, Rectangle destination,
        MaterialDefinition material, int x, int y)
    {

        var leftExposed = !world.GetTile(x - 1, y).Solid;
        var rightExposed = !world.GetTile(x + 1, y).Solid;
        var topExposed = !world.GetTile(x, y - 1).Solid;
        var bottomExposed = !world.GetTile(x, y + 1).Solid;
        var sourceX = leftExposed ? 0 : rightExposed ? 2 : 1;
        var sourceY = (int)TerrainBand * 6 + (topExposed ? 0 : bottomExposed ? 2 : 1);
        var tint = Color.Lerp(Color.White, material.BaseColor, 0.2f);
        DrawImportedTile(batch, destination, sourceX, sourceY, tint);

        if (leftExposed && rightExposed) DrawImportedTile(batch, destination, 2, sourceY, tint);
        if (topExposed && bottomExposed)
            DrawImportedTile(batch, destination, sourceX, (int)TerrainBand * 6 + 2, tint);
        return true;
    }

    private bool DrawAdveTile(SpriteBatch batch, TileWorld world, Rectangle destination,
        MaterialDefinition material, int x, int y)
    {
        var leftExposed = !world.GetTile(x - 1, y).Solid;
        var rightExposed = !world.GetTile(x + 1, y).Solid;
        var topExposed = !world.GetTile(x, y - 1).Solid;
        var bottomExposed = !world.GetTile(x, y + 1).Solid;
        var primaryOrigin = TerrainBand switch
        {
            TerrainBandId.Green => new Point(8, 64),
            TerrainBandId.Snow => new Point(8, 120),
            _ => new Point(8, 16)
        };
        var structuralOrigin = TerrainBand switch
        {
            TerrainBandId.Green => new Point(72, 64),
            TerrainBandId.Snow => new Point(72, 120),
            _ => new Point(72, 16)
        };
        var origin = material.Structural ? structuralOrigin : primaryOrigin;
        var hash = TileHash(x, y);
        var column = leftExposed ? 0 : rightExposed ? 4 : 1 + (int)(hash % 3);
        var row = topExposed ? 0 : bottomExposed ? 4 : 1 + (int)((hash >> 4) % 3);
        var source = new Rectangle(origin.X + column * 8, origin.Y + row * 8, 8, 8);
        var tint = Color.Lerp(Color.White, material.BaseColor, 0.06f);
        batch.Draw(_environmentTexture!, destination, source, tint);
        return true;
    }

    private bool DrawMonochromeTile(SpriteBatch batch, TileWorld world, Rectangle destination,
        MaterialDefinition material, int x, int y)
    {
        var leftExposed = !world.GetTile(x - 1, y).Solid;
        var rightExposed = !world.GetTile(x + 1, y).Solid;
        var topExposed = !world.GetTile(x, y - 1).Solid;
        var bottomExposed = !world.GetTile(x, y + 1).Solid;
        var hash = TileHash(x, y);
        var cell = topExposed ? 3 : bottomExposed ? 18 : leftExposed ? 12 : rightExposed ? 23 :
            (hash & 1) == 0 ? 48 : 49;
        DrawEightPixelCell(batch, destination, cell, Color.Lerp(Color.White, material.AccentColor, 0.52f) * 0.58f);
        return true;
    }

    private bool DrawPixelFantasyTile(SpriteBatch batch, TileWorld world, Rectangle destination,
        MaterialDefinition material, int x, int y)
    {
        var topExposed = !world.GetTile(x, y - 1).Solid;
        var bottomExposed = !world.GetTile(x, y + 1).Solid;
        if (!topExposed && !bottomExposed) return false;

        var hash = TileHash(x, y);
        var sources = topExposed ? PixelFantasyTopCells : PixelFantasyBottomCells;
        var point = sources[(int)(hash % (uint)sources.Length)];
        var source = new Rectangle(point.X, point.Y, 16, 16);
        batch.Draw(_environmentTexture!, destination, source, Color.Lerp(Color.White, material.BaseColor, 0.1f),
            0f, Vector2.Zero, SpriteEffects.None, 0f);
        return true;
    }

    private bool DrawStoneRuinsTile(SpriteBatch batch, TileWorld world, Rectangle destination,
        MaterialDefinition material, int x, int y)
    {
        var tile = world.GetTile(x, y);
        if ((tile.Flags & TileFlags.RuinTrace) == 0 && tile.Material is not (MaterialId.RuinAlloy or
            MaterialId.BrittleMasonry or MaterialId.MachineCeramic or MaterialId.WeatheredConcrete)) return false;

        var leftExposed = !world.GetTile(x - 1, y).Solid;
        var rightExposed = !world.GetTile(x + 1, y).Solid;
        var topExposed = !world.GetTile(x, y - 1).Solid;
        var bottomExposed = !world.GetTile(x, y + 1).Solid;
        var hash = TileHash(x, y);
        var cell = topExposed ? 8 + (int)(hash % 3) : bottomExposed ? 74 + (int)(hash % 2) :
            leftExposed ? 19 : rightExposed ? 34 : tile.Material switch
            {
                MaterialId.BrittleMasonry => (hash & 1) == 0 ? 17 : 18,
                MaterialId.MachineCeramic => (hash & 1) == 0 ? 44 : 46,
                MaterialId.RuinAlloy => (hash & 1) == 0 ? 29 : 37,
                _ => (hash & 1) == 0 ? 24 : 32
            };
        DrawEightPixelCell(batch, destination, cell, Color.Lerp(Color.White, material.BaseColor, 0.04f) * 0.92f);
        return true;
    }

    private bool DrawDraculasManorTile(SpriteBatch batch, TileWorld world, Rectangle destination,
        MaterialDefinition material, int x, int y)
    {
        var leftExposed = !world.GetTile(x - 1, y).Solid;
        var rightExposed = !world.GetTile(x + 1, y).Solid;
        var topExposed = !world.GetTile(x, y - 1).Solid;
        var bottomExposed = !world.GetTile(x, y + 1).Solid;
        var hash = TileHash(x, y);
        var cell = topExposed ? 8 + (int)(hash & 1) : bottomExposed ? 56 : leftExposed ? 10 :
            rightExposed ? 11 : DraculasManorInteriorCells[(int)(hash % DraculasManorInteriorCells.Length)];
        DrawEightPixelCell(batch, destination, cell, Color.Lerp(Color.White, material.BaseColor, 0.05f) * 0.94f);
        return true;
    }

    private void DrawEightPixelCell(SpriteBatch batch, Rectangle destination, int cell, Color tint)
    {
        var columns = _environmentTexture!.Width / 8;
        var source = new Rectangle(cell % columns * 8, cell / columns * 8, 8, 8);
        batch.Draw(_environmentTexture, destination, source, tint);
    }

    private static uint TileHash(int x, int y) => unchecked((uint)(x * 73856093 ^ y * 19349663));

    public bool DrawPlayer(SpriteBatch batch, MovementState state, Vector2 feet, int facing, long frame, Color tint)
    {
        if (PlayerPack == PlayerVisualPackId.MerakintsugiSample)
            return DrawMerakintsugiPlayer(batch, state, feet, facing, frame, tint);
        if (PlayerPack is PlayerVisualPackId.SummingOriginal or PlayerVisualPackId.AsepritePilgrim ||
            _gandalfCharacter.Length == 0) return false;

        var pose = ResolvePose(state, frame);
        var source = new Rectangle(pose.Column * CharacterCellWidth, pose.Row * CharacterCellHeight,
            CharacterCellWidth, CharacterCellHeight);
        var destination = new Rectangle((int)MathF.Round(feet.X - 20f), (int)MathF.Round(feet.Y - 32f), 40, 32);
        if (state is MovementState.Crouch or MovementState.Crawl)
            destination = new Rectangle(destination.X + 5, destination.Bottom - 22, 30, 22);
        else if (state == MovementState.Slide)
            destination = new Rectangle(destination.X - 2, destination.Bottom - 22, 44, 22);

        var effects = facing > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        var silhouette = new Color(7, 9, 14, 220);
        DrawCharacterLayers(batch, destination with { X = destination.X - 1 }, source, silhouette, effects);
        DrawCharacterLayers(batch, destination with { X = destination.X + 1 }, source, silhouette, effects);
        DrawCharacterLayers(batch, destination with { Y = destination.Y + 1 }, source, silhouette, effects);
        DrawCharacterLayers(batch, destination, source, Color.Lerp(Color.White, tint, 0.18f), effects);
        return true;
    }

    private bool DrawMerakintsugiPlayer(SpriteBatch batch, MovementState state, Vector2 feet, int facing,
        long frame, Color tint)
    {
        if (_merakintsugiIdle == null || _merakintsugiWalk == null) return false;
        var idle = state is MovementState.Idle or MovementState.Landing;
        var source = idle
            ? new Rectangle((int)(frame / 8 % 10) * 46, 0, 46, 55)
            : new Rectangle((int)(frame / 4 % 24) % 4 * 45, (int)(frame / 4 % 24) / 4 * 58, 45, 58);
        var texture = idle ? _merakintsugiIdle : _merakintsugiWalk;
        var destination = new Rectangle((int)MathF.Round(feet.X - 11f), (int)MathF.Round(feet.Y - 28f), 22, 28);
        if (state is MovementState.Crouch or MovementState.Crawl or MovementState.Slide)
            destination = new Rectangle(destination.X, destination.Bottom - 20, destination.Width, 20);
        // the source sheet faces left; mirror it when the controller faces right
        var effects = facing > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        var silhouette = new Color(7, 9, 14, 220);
        batch.Draw(texture, destination with { X = destination.X - 1 }, source, silhouette, 0f, Vector2.Zero,
            effects, 0f);
        batch.Draw(texture, destination with { X = destination.X + 1 }, source, silhouette, 0f, Vector2.Zero,
            effects, 0f);
        batch.Draw(texture, destination with { Y = destination.Y + 1 }, source, silhouette, 0f, Vector2.Zero,
            effects, 0f);
        batch.Draw(texture, destination, source, Color.Lerp(Color.White, tint, 0.12f), 0f, Vector2.Zero, effects, 0f);
        return true;
    }

    public bool DrawEnvironmentPreview(SpriteBatch batch, Rectangle area)
    {
        if (_environmentTexture == null) return false;
        var source = EnvironmentPack switch
        {
            EnvironmentVisualPackId.GandalfOverworld => new Rectangle(0, (int)TerrainBand * 6 * 32, 96, 64),
            EnvironmentVisualPackId.Adve => new Rectangle(8, TerrainBand switch
            {
                TerrainBandId.Green => 64,
                TerrainBandId.Snow => 120,
                _ => 16
            }, 40, 40),
            EnvironmentVisualPackId.MonochromeCaves => new Rectangle(0, 0, 64, 64),
            EnvironmentVisualPackId.PixelFantasyCaves => new Rectangle(0, 560, 640, 400),
            EnvironmentVisualPackId.StoneRuins => new Rectangle(0, 0, 64, 88),
            EnvironmentVisualPackId.DraculasManor => new Rectangle(0, 0, 64, 104),
            _ => Rectangle.Empty
        };
        if (source == Rectangle.Empty) return false;
        batch.Draw(_environmentTexture, area, source, Color.White);
        return true;
    }

    public void Dispose()
    {
        _environmentTexture?.Dispose();
        foreach (var texture in _gandalfCharacter) texture.Dispose();
        _merakintsugiIdle?.Dispose();
        _merakintsugiWalk?.Dispose();
    }

    public static string OptionLabel(VisualSelectionOption option) => option switch
    {
        VisualSelectionOption.PlayerPack => "PLAYER PACK",
        VisualSelectionOption.SkinSheet => "BODY SPRITE",
        VisualSelectionOption.LegSheet => "LEGS SPRITE",
        VisualSelectionOption.TorsoSheet => "TORSO SPRITE",
        VisualSelectionOption.FootSheet => "FEET SPRITE",
        VisualSelectionOption.HairSheet => "HAIR SPRITE",
        VisualSelectionOption.EnvironmentPack => "WORLD PACK",
        VisualSelectionOption.TerrainSheet => "TILESET",
        VisualSelectionOption.TerrainBand => "TILESET BAND",
        _ => option.ToString().ToUpperInvariant()
    };

    private void NormalizePackAvailability()
    {
        if (!PlayerAvailable(PlayerPack))
            PlayerPack = PlayerVisualPackId.SummingOriginal;
        if (!EnvironmentAvailable(EnvironmentPack))
            EnvironmentPack = EnvironmentVisualPackId.SummingOriginal;
        if (!Enum.IsDefined(TerrainSheet)) TerrainSheet = TerrainSheetId.FloorTiles2;
        if (!Enum.IsDefined(TerrainBand)) TerrainBand = TerrainBandId.Autumn;
    }

    private void BuildCharacterChoices(bool resetDefaults = false)
    {
        if (!UsesModularCharacter || _gandalfFiles is not { } files)
        {
            _skinChoices = [];
            _legChoices = _torsoChoices = _footChoices = _hairChoices = [None];
            SkinSheet = "PACK CONTROLLED";
            LegSheet = TorsoSheet = FootSheet = HairSheet = "PACK CONTROLLED";
            return;
        }

        var male = PlayerPack == PlayerVisualPackId.GandalfMale;
        var prefix = male ? "Male" : "Female";
        var skinDirectory = Path.Combine(files.CharacterDirectory, "Character skin colors");
        var clothingDirectory = Path.Combine(files.CharacterDirectory, $"{prefix} Clothing");
        var hairDirectory = Path.Combine(files.CharacterDirectory, $"{prefix} Hair");
        _skinChoices = Files(skinDirectory, name => name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
        _legChoices = WithNone(Files(clothingDirectory, name => male
            ? name.Contains("Pants", StringComparison.OrdinalIgnoreCase) || name.Contains("Underwear", StringComparison.OrdinalIgnoreCase)
            : name.Contains("Panties", StringComparison.OrdinalIgnoreCase) || name.Contains("Skirt", StringComparison.OrdinalIgnoreCase)));
        _torsoChoices = WithNone(Files(clothingDirectory, name => male
            ? name.Contains("Shirt", StringComparison.OrdinalIgnoreCase)
            : name.Contains("Corset", StringComparison.OrdinalIgnoreCase)));
        _footChoices = WithNone(Files(clothingDirectory, name =>
            name.Contains("Boot", StringComparison.OrdinalIgnoreCase) || name.Contains("Shoe", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Sock", StringComparison.OrdinalIgnoreCase)));
        _hairChoices = WithNone(Files(hairDirectory, _ => true));

        var currentSkin = resetDefaults ? "" : SkinSheet;
        var currentLegs = resetDefaults ? "" : LegSheet;
        var currentTorso = resetDefaults ? "" : TorsoSheet;
        var currentFeet = resetDefaults ? "" : FootSheet;
        var currentHair = resetDefaults ? "" : HairSheet;
        SkinSheet = SelectExisting(_skinChoices, currentSkin, $"{prefix} Skin1.png");
        LegSheet = SelectExisting(_legChoices, currentLegs, male ? "Pants.png" : "Skirt.png");
        TorsoSheet = SelectExisting(_torsoChoices, currentTorso, male ? "Shirt v2.png" : "Corset v2.png");
        FootSheet = SelectExisting(_footChoices, currentFeet, "Boots.png");
        HairSheet = SelectExisting(_hairChoices, currentHair, $"{prefix} Hair1.png");
    }

    private void ReloadEnvironment()
    {
        _environmentTexture?.Dispose();
        _environmentTexture = null;
        if (EnvironmentPack == EnvironmentVisualPackId.SummingOriginal) return;
        try
        {
            if (!VisualPackFiles.TryResolveEnvironment(EnvironmentPack, TerrainSheet, out var asset))
            {
                EnvironmentPack = EnvironmentVisualPackId.SummingOriginal;
                return;
            }
            _environmentTexture = Load(_graphicsDevice, asset.Path, asset.MinimumWidth, asset.MinimumHeight,
                asset.RemoveFlatInterior);
        }
        catch (Exception exception) when (exception is IOException or InvalidOperationException or ArgumentException)
        {
            _loadFailure = exception.Message;
            EnvironmentPack = EnvironmentVisualPackId.SummingOriginal;
        }
    }

    private void ReloadCharacter()
    {
        ReplaceCharacter([]);
        ReplaceMerakintsugiCharacter(null, null);
        if (PlayerPack is PlayerVisualPackId.SummingOriginal or PlayerVisualPackId.AsepritePilgrim) return;
        if (PlayerPack == PlayerVisualPackId.MerakintsugiSample && _merakintsugiFiles is { } merakintsugiFiles)
        {
            Texture2D? idle = null;
            Texture2D? walk = null;
            try
            {
                idle = Load(_graphicsDevice, merakintsugiFiles.Idle, 460, 55);
                walk = Load(_graphicsDevice, merakintsugiFiles.Walk, 180, 348);
                ReplaceMerakintsugiCharacter(idle, walk);
                return;
            }
            catch (Exception exception) when (exception is IOException or InvalidOperationException or ArgumentException)
            {
                idle?.Dispose();
                walk?.Dispose();
                _loadFailure = exception.Message;
                PlayerPack = PlayerVisualPackId.SummingOriginal;
                BuildCharacterChoices();
                return;
            }
        }
        if (_gandalfFiles is not { } files)
        {
            PlayerPack = PlayerVisualPackId.SummingOriginal;
            BuildCharacterChoices();
            return;
        }

        var prefix = PlayerPack == PlayerVisualPackId.GandalfMale ? "Male" : "Female";
        var paths = new List<string>
        {
            Path.Combine(files.CharacterDirectory, "Character skin colors", SkinSheet)
        };
        AddLayer(paths, Path.Combine(files.CharacterDirectory, $"{prefix} Clothing"), LegSheet);
        AddLayer(paths, Path.Combine(files.CharacterDirectory, $"{prefix} Clothing"), TorsoSheet);
        AddLayer(paths, Path.Combine(files.CharacterDirectory, $"{prefix} Clothing"), FootSheet);
        AddLayer(paths, Path.Combine(files.CharacterDirectory, $"{prefix} Hair"), HairSheet);

        var loaded = new List<Texture2D>();
        try
        {
            foreach (var path in paths) loaded.Add(Load(_graphicsDevice, path, 800, 448));
            ReplaceCharacter(loaded.ToArray());
        }
        catch (Exception exception) when (exception is IOException or InvalidOperationException or ArgumentException)
        {
            foreach (var texture in loaded) texture.Dispose();
            _loadFailure = exception.Message;
            PlayerPack = PlayerVisualPackId.SummingOriginal;
            BuildCharacterChoices();
            ReplaceCharacter([]);
        }
    }

    private void ReplaceCharacter(Texture2D[] replacement)
    {
        foreach (var texture in _gandalfCharacter) texture.Dispose();
        _gandalfCharacter = replacement;
    }

    private void ReplaceMerakintsugiCharacter(Texture2D? idle, Texture2D? walk)
    {
        _merakintsugiIdle?.Dispose();
        _merakintsugiWalk?.Dispose();
        _merakintsugiIdle = idle;
        _merakintsugiWalk = walk;
    }

    private void DrawImportedTile(SpriteBatch batch, Rectangle destination, int column, int row, Color tint)
    {
        var source = new Rectangle(column * ImportedTileSize, row * ImportedTileSize,
            ImportedTileSize, ImportedTileSize);
        batch.Draw(_environmentTexture!, destination, source, tint);
    }

    private void DrawCharacterLayers(SpriteBatch batch, Rectangle destination, Rectangle source, Color tint,
        SpriteEffects effects)
    {
        foreach (var texture in _gandalfCharacter)
            batch.Draw(texture, destination, source, tint, 0f, Vector2.Zero, effects, 0f);
    }

    private PlayerVisualPackId NextPlayerPack(PlayerVisualPackId current, int direction)
    {
        var choices = Enum.GetValues<PlayerVisualPackId>().Where(PlayerAvailable).ToArray();
        return Cycle(choices, current, direction);
    }

    private bool PlayerAvailable(PlayerVisualPackId id) => id switch
    {
        PlayerVisualPackId.GandalfMale or PlayerVisualPackId.GandalfFemale => GandalfCharacterAvailable,
        PlayerVisualPackId.MerakintsugiSample => MerakintsugiCharacterAvailable,
        _ => true
    };

    private bool UsesModularCharacter =>
        PlayerPack is PlayerVisualPackId.GandalfMale or PlayerVisualPackId.GandalfFemale;

    private EnvironmentVisualPackId NextEnvironmentPack(EnvironmentVisualPackId current, int direction)
    {
        var choices = Enum.GetValues<EnvironmentVisualPackId>()
            .Where(EnvironmentAvailable)
            .ToArray();
        return Cycle(choices, current, direction);
    }

    private static bool EnvironmentAvailable(EnvironmentVisualPackId id) =>
        id == EnvironmentVisualPackId.SummingOriginal ||
        VisualPackFiles.TryResolveEnvironment(id, TerrainSheetId.FloorTiles2, out _);

    private string AdveBandName() => TerrainBand switch
    {
        TerrainBandId.Green => "VERDIGRIS",
        TerrainBandId.Snow => "SALT",
        _ => "OXIDE"
    };

    private VisualSelectionSnapshot? LoadSelection()
    {
        try
        {
            if (!File.Exists(_selectionPath)) return null;
            return JsonSerializer.Deserialize<VisualSelectionSnapshot>(File.ReadAllText(_selectionPath), JsonOptions());
        }
        catch (Exception exception) when (exception is IOException or JsonException or UnauthorizedAccessException)
        {
            _loadFailure = $"Visual selection could not load: {exception.Message}";
            return null;
        }
    }

    private void SaveSelection()
    {
        if (!_persistSelection) return;
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_selectionPath) ?? ".");
            File.WriteAllText(_selectionPath, JsonSerializer.Serialize(Selection, JsonOptions()));
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            _loadFailure = $"Visual selection could not save: {exception.Message}";
        }
    }

    private static JsonSerializerOptions JsonOptions() => new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private static string[] Files(string directory, Func<string, bool> predicate)
    {
        if (!Directory.Exists(directory)) return [];
        return Directory.EnumerateFiles(directory, "*.png", SearchOption.TopDirectoryOnly)
            .Select(Path.GetFileName)
            .Where(name => name != null && predicate(name))
            .Cast<string>()
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string[] WithNone(string[] choices) => [None, .. choices];

    private static string SelectExisting(string[] choices, string selected, string fallback)
    {
        if (choices.Contains(selected, StringComparer.OrdinalIgnoreCase))
            return choices.First(choice => choice.Equals(selected, StringComparison.OrdinalIgnoreCase));
        if (choices.Contains(fallback, StringComparer.OrdinalIgnoreCase))
            return choices.First(choice => choice.Equals(fallback, StringComparison.OrdinalIgnoreCase));
        return choices.FirstOrDefault() ?? None;
    }

    private static string Cycle(string[] choices, string current, int direction)
    {
        if (choices.Length == 0) return current;
        var index = Array.FindIndex(choices, choice => choice.Equals(current, StringComparison.OrdinalIgnoreCase));
        if (index < 0) index = 0;
        return choices[Wrap(index + Math.Sign(direction), choices.Length)];
    }

    private static T Cycle<T>(T[] choices, T current, int direction) where T : struct, Enum
    {
        var index = Array.IndexOf(choices, current);
        if (index < 0) index = 0;
        return choices[Wrap(index + Math.Sign(direction), choices.Length)];
    }

    private static T CycleEnum<T>(T current, int direction) where T : struct, Enum =>
        Cycle(Enum.GetValues<T>(), current, direction);

    private static int Wrap(int value, int count) => (value % count + count) % count;

    private static void AddLayer(List<string> paths, string directory, string file)
    {
        if (file != None) paths.Add(Path.Combine(directory, file));
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
    public readonly record struct GandalfFiles(string PlatformerDirectory, string CharacterDirectory,
        string Terrain1, string Terrain2);
    public readonly record struct MerakintsugiFiles(string CharacterDirectory, string Idle, string Walk);
    public readonly record struct EnvironmentAsset(string Path, int MinimumWidth, int MinimumHeight,
        bool RemoveFlatInterior = false);

    private const string PlatformerDirectory =
        "third_party/art/local-only/gandalfhardcore-platformer/GandalfHardcore FREE Platformer Assets";
    private const string CharacterDirectory =
        "third_party/art/local-only/gandalfhardcore-character/GandalfHardcore Character Asset Pack";
    private const string AdveTiles = "third_party/art/cc0/adve/tiles.png";
    private const string MonochromeTiles = "third_party/art/cc0/monochrome-caves/bw_tiles.png";
    private const string PixelFantasyTiles = "third_party/art/local-only/pixel-fantasy-caves/mainlev_build.png";
    private const string StoneRuinsTiles = "third_party/art/local-only/stone-ruins/tiles.png";
    private const string DraculasManorTiles = "third_party/art/cc0/draculas-manor/lilspook_tiles.png";
    private const string MerakintsugiDirectory = "third_party/art/local-only/merakintsugi-sample";

    public static bool IsAvailable(VisualPackId id) => id == VisualPackId.SummingOriginal || TryResolveGandalf(out _);

    public static bool TryResolveEnvironment(EnvironmentVisualPackId id, TerrainSheetId sheet,
        out EnvironmentAsset asset)
    {
        if (id == EnvironmentVisualPackId.SummingOriginal)
        {
            asset = default;
            return true;
        }

        foreach (var root in CandidateRoots())
        {
            asset = id switch
            {
                EnvironmentVisualPackId.GandalfOverworld => new EnvironmentAsset(
                    Path.Combine(root, PlatformerDirectory,
                        sheet == TerrainSheetId.FloorTiles1 ? "Floor Tiles1.png" : "Floor Tiles2.png"),
                    288, 576, true),
                EnvironmentVisualPackId.Adve => new EnvironmentAsset(Path.Combine(root, AdveTiles), 88, 136),
                EnvironmentVisualPackId.MonochromeCaves =>
                    new EnvironmentAsset(Path.Combine(root, MonochromeTiles), 64, 64),
                EnvironmentVisualPackId.PixelFantasyCaves =>
                    new EnvironmentAsset(Path.Combine(root, PixelFantasyTiles), 640, 960),
                EnvironmentVisualPackId.StoneRuins =>
                    new EnvironmentAsset(Path.Combine(root, StoneRuinsTiles), 64, 88),
                EnvironmentVisualPackId.DraculasManor =>
                    new EnvironmentAsset(Path.Combine(root, DraculasManorTiles), 64, 104),
                _ => default
            };
            if (!string.IsNullOrEmpty(asset.Path) && File.Exists(asset.Path)) return true;
        }

        asset = default;
        return false;
    }

    public static bool TryResolveGandalf(out GandalfFiles files)
    {
        foreach (var root in CandidateRoots())
        {
            var platformer = Path.Combine(root, PlatformerDirectory);
            var character = Path.Combine(root, CharacterDirectory);
            var candidate = new GandalfFiles(platformer, character,
                Path.Combine(platformer, "Floor Tiles1.png"),
                Path.Combine(platformer, "Floor Tiles2.png"));
            if (Directory.Exists(character) && File.Exists(candidate.Terrain1) && File.Exists(candidate.Terrain2))
            {
                files = candidate;
                return true;
            }
        }

        files = default;
        return false;
    }

    public static bool TryResolveMerakintsugi(out MerakintsugiFiles files)
    {
        foreach (var root in CandidateRoots())
        {
            var character = Path.Combine(root, MerakintsugiDirectory);
            var candidate = new MerakintsugiFiles(character,
                Path.Combine(character, "idle", "sprite sheets", "idle.png"),
                Path.Combine(character, "walk", "sprite sheets", "walk.png"));
            if (File.Exists(candidate.Idle) && File.Exists(candidate.Walk))
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
}
