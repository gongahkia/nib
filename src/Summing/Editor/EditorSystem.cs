using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Summing.Camera;
using Summing.Core;
using Summing.Gameplay;
using Summing.Generation;
using Summing.Input;
using Summing.Rendering;
using Summing.World;
using Summing.World.Materials;

namespace Summing.Editor;

public sealed class EditorSystem
{
    private static readonly Keys[] PresetKeys = [Keys.D1, Keys.D2, Keys.D3];
    private readonly MaterialId[] _materials = Enum.GetValues<MaterialId>();
    private int _materialIndex = 2;
    private int? _movingFeature;
    private bool _typingSeed;
    private string _seedText = "";
    private float _noticeTimer;

    public bool Active { get; private set; }
    public EditorTool Tool { get; private set; } = EditorTool.Inspect;
    public Point CursorTile { get; private set; }
    public string Notice { get; private set; } = "";
    public MaterialId SelectedMaterial => _materials[_materialIndex];

    public void Update(InputManager input, Camera2D camera, Viewport viewport, GeneratedWorld generated,
        Difficulty difficulty, Action<WorldGenerationConfig> regenerate, Action<WorldGenerationConfig> toggleDifficulty,
        Action save, Action load, Action export, float dt)
    {
        _noticeTimer = MathF.Max(0f, _noticeTimer - dt);
        if (input.Pressed(InputAction.Debug))
        {
            Active = !Active;
            _movingFeature = null;
            SetNotice(Active ? "EDITOR ACTIVE" : "EDITOR CLOSED");
        }
        if (!Active) return;

        camera.Pan(input.Move * (input.KeyDown(Keys.LeftShift) ? 520f : 250f) * dt);
        var virtualMouse = new Vector2(input.MousePosition.X * GameConstants.VirtualWidth / (float)Math.Max(1, viewport.Width),
            input.MousePosition.Y * GameConstants.VirtualHeight / (float)Math.Max(1, viewport.Height));
        CursorTile = generated.Terrain.WorldToTile(camera.ScreenToWorld(virtualMouse));

        if (_typingSeed)
        {
            UpdateSeedTyping(input, generated, regenerate);
            return;
        }

        if (input.KeyPressed(Keys.Tab))
        {
            Tool = (EditorTool)(((int)Tool + 1) % Enum.GetValues<EditorTool>().Length);
            _movingFeature = null;
        }
        if (input.KeyPressed(Keys.OemOpenBrackets)) _materialIndex = (_materialIndex - 1 + _materials.Length) % _materials.Length;
        if (input.KeyPressed(Keys.OemCloseBrackets)) _materialIndex = (_materialIndex + 1) % _materials.Length;
        if (input.KeyPressed(Keys.T)) { _typingSeed = true; _seedText = generated.Configuration.Seed.ToString(); }
        if (input.KeyPressed(Keys.V))
        {
            var config = Copy(generated.Configuration);
            config.Variant = (GeneratorVariant)(((int)config.Variant + 1) % Enum.GetValues<GeneratorVariant>().Length);
            regenerate(config);
            SetNotice($"GENERATED {config.Variant}");
            return;
        }
        if (input.KeyPressed(Keys.F2)) { regenerate(Copy(generated.Configuration)); SetNotice("WORLD REGENERATED"); return; }
        if (input.KeyPressed(Keys.F3))
        {
            var config = Copy(generated.Configuration);
            config.Seed = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            regenerate(config);
            SetNotice($"RANDOM SEED {config.Seed}");
            return;
        }
        for (var index = 0; index < PresetKeys.Length; index++)
        {
            if (!input.KeyPressed(PresetKeys[index])) continue;
            var config = Copy(generated.Configuration);
            DevelopmentSeedPresets.Apply(config, index);
            regenerate(config);
            SetNotice($"PRESET {DevelopmentSeedPresets.All[index].Name}");
            return;
        }
        if (input.KeyPressed(Keys.P) || input.ButtonPressed(Buttons.RightShoulder))
        {
            var config = Copy(generated.Configuration);
            var index = DevelopmentSeedPresets.Next(config);
            DevelopmentSeedPresets.Apply(config, index);
            regenerate(config);
            SetNotice($"PRESET {DevelopmentSeedPresets.All[index].Name}");
            return;
        }
        if (input.KeyPressed(Keys.H))
        {
            toggleDifficulty(Copy(generated.Configuration));
            SetNotice($"DIFFICULTY {(difficulty == Difficulty.Easy ? Difficulty.Hard : Difficulty.Easy)}");
            return;
        }

        AdjustParameters(input, generated.Configuration.Parameters);
        var control = input.KeyDown(Keys.LeftControl) || input.KeyDown(Keys.RightControl);
        if (control && input.KeyPressed(Keys.S)) { save(); SetNotice("SAVED saves/editor-world.json"); }
        if (control && input.KeyPressed(Keys.L)) { load(); SetNotice("LOADED saves/editor-world.json"); return; }
        if (control && input.KeyPressed(Keys.E)) { export(); SetNotice("EXPORTED FULL WORLD JSON"); }

        if (input.MouseRightPressed) EraseTile(generated);
        if (!input.MouseLeftPressed) return;
        ApplyTool(generated);
    }

    public void DrawWorld(SpriteBatch batch, Texture2D pixel)
    {
        if (!Active) return;
        var destination = new Rectangle(CursorTile.X * GameConstants.TileSize, CursorTile.Y * GameConstants.TileSize,
            GameConstants.TileSize, GameConstants.TileSize);
        batch.Draw(pixel, new Rectangle(destination.X, destination.Y, destination.Width, 2), GamePalette.SaltCyan);
        batch.Draw(pixel, new Rectangle(destination.X, destination.Bottom - 2, destination.Width, 2), GamePalette.SaltCyan);
        batch.Draw(pixel, new Rectangle(destination.X, destination.Y, 2, destination.Height), GamePalette.SaltCyan);
        batch.Draw(pixel, new Rectangle(destination.Right - 2, destination.Y, 2, destination.Height), GamePalette.SaltCyan);
    }

    public void DrawOverlay(SpriteBatch batch, Texture2D pixel, PixelFont font, GeneratedWorld generated,
        Difficulty difficulty, string visualPack)
    {
        if (!Active) return;
        batch.Draw(pixel, new Rectangle(4, 67, 315, 130), new Color(5, 7, 12, 235));
        font.Draw(batch, "WORLD EDITOR", new Vector2(10, 73), GamePalette.SacredGold, 2);
        font.Draw(batch, $"TOOL {Format(Tool.ToString())}", new Vector2(10, 90), Color.White);
        font.Draw(batch, $"MATERIAL {Format(SelectedMaterial.ToString())}", new Vector2(10, 99), GamePalette.Oxide);
        font.Draw(batch, $"CURSOR {CursorTile.X},{CursorTile.Y}  SEED {generated.Configuration.Seed}", new Vector2(10, 108), GamePalette.UiMuted);
        font.Draw(batch, $"VARIANT {generated.Configuration.Variant}  EROSION {generated.Configuration.Parameters.Erosion:0.00}",
            new Vector2(10, 117), GamePalette.UiMuted);
        font.Draw(batch, $"DIFFICULTY {difficulty}  PRESET {DevelopmentSeedPresets.Label(generated.Configuration)}",
            new Vector2(10, 126), GamePalette.UiMuted);
        font.Draw(batch, $"ART {visualPack}", new Vector2(10, 135), GamePalette.SaltCyan);
        font.Draw(batch, $"RUINS {generated.Configuration.Parameters.RuinDensity:0.00}  WIND {generated.Configuration.Parameters.WindStrength:0}",
            new Vector2(10, 144), GamePalette.UiMuted);
        font.Draw(batch, "TAB TOOL  [ ] MATERIAL  LMB APPLY  RMB ERASE", new Vector2(10, 157), new Color(175, 181, 178));
        font.Draw(batch, "T SET SEED  V VARIANT  F2 REGEN  F3 RANDOM", new Vector2(10, 166), new Color(175, 181, 178));
        font.Draw(batch, "1 2 3/P PRESET  H DIFF  K VISUALS  M TEST VIEW", new Vector2(10, 175), new Color(175, 181, 178));
        font.Draw(batch, "- + EROSION  , . RUINS  CTRL S/L/E SAVE/LOAD/EXPORT", new Vector2(10, 184), new Color(175, 181, 178));
        if (_typingSeed)
        {
            batch.Draw(pixel, new Rectangle(318, 72, 308, 34), new Color(8, 10, 16, 245));
            font.Draw(batch, "TYPE SEED THEN ENTER", new Vector2(326, 79), GamePalette.SacredGold);
            font.Draw(batch, _seedText + "_", new Vector2(326, 91), Color.White);
        }
        if (_noticeTimer > 0f) font.Draw(batch, Notice, new Vector2(320, 114), GamePalette.SaltCyan);
    }

    private void ApplyTool(GeneratedWorld generated)
    {
        if (!generated.Terrain.Contains(CursorTile.X, CursorTile.Y)) return;
        var position = new Vector2((CursorTile.X + 0.5f) * GameConstants.TileSize, CursorTile.Y * GameConstants.TileSize);
        switch (Tool)
        {
            case EditorTool.Inspect:
                var tile = generated.Terrain.GetTile(CursorTile.X, CursorTile.Y);
                var material = MaterialCatalog.Get(tile.Material);
                SetNotice($"{tile.Material} HARD {material.Hardness} FRICTION {material.Friction:0.00} " +
                    $"CLIMB {material.Climbable} STRUCT {material.Structural} PROV {tile.ProvenanceId}");
                break;
            case EditorTool.Paint:
                generated.Terrain.SetTile(CursorTile.X, CursorTile.Y, SelectedMaterial, TileFlags.PlayerBuilt);
                break;
            case EditorTool.Erase:
                EraseTile(generated);
                break;
            case EditorTool.PlaceBurrower:
                generated.Features.Add(new WorldFeature(WorldFeatureKind.BurrowerSpawn, position, 0));
                SetNotice("BURROWER FEATURE PLACED - RELOAD TO ACTIVATE");
                break;
            case EditorTool.PlaceBrittle:
                for (var x = CursorTile.X - 2; x <= CursorTile.X + 2; x++)
                    generated.Terrain.SetTile(x, CursorTile.Y, MaterialId.BrittleMasonry, TileFlags.PlayerBuilt);
                generated.Features.Add(new WorldFeature(WorldFeatureKind.BrittleSite, position, 0));
                SetNotice("BRITTLE FEATURE PLACED - RELOAD TO ACTIVATE");
                break;
            case EditorTool.PlaceRelic:
                generated.Features.Add(new WorldFeature(WorldFeatureKind.RelicCandidate, position, 0, 1));
                SetNotice("RELIC FEATURE PLACED - RELOAD TO ACTIVATE");
                break;
            case EditorTool.PlaceRuin:
                generated.Features.Add(new WorldFeature(WorldFeatureKind.Ruin, position, generated.Features.Count % 4, 1));
                break;
            case EditorTool.MoveEntity:
                MoveFeature(generated, position);
                break;
            case EditorTool.DeleteEntity:
                var nearest = FindNearestFeature(generated, position);
                if (nearest >= 0) generated.Features.RemoveAt(nearest);
                break;
        }
    }

    private void MoveFeature(GeneratedWorld generated, Vector2 position)
    {
        if (_movingFeature == null)
        {
            var nearest = FindNearestFeature(generated, position);
            if (nearest >= 0) { _movingFeature = nearest; SetNotice("ENTITY SELECTED - CHOOSE DESTINATION"); }
            return;
        }
        var feature = generated.Features[_movingFeature.Value];
        generated.Features[_movingFeature.Value] = feature with { Position = position };
        _movingFeature = null;
        SetNotice("ENTITY MOVED");
    }

    private static int FindNearestFeature(GeneratedWorld generated, Vector2 position)
    {
        var result = -1;
        var best = 3f * GameConstants.TileSize * 3f * GameConstants.TileSize;
        for (var i = 0; i < generated.Features.Count; i++)
        {
            var distance = Vector2.DistanceSquared(position, generated.Features[i].Position);
            if (distance >= best) continue;
            best = distance;
            result = i;
        }
        return result;
    }

    private void EraseTile(GeneratedWorld generated) => generated.Terrain.SetTile(CursorTile.X, CursorTile.Y, MaterialId.Air);

    private static void AdjustParameters(InputManager input, BadlandsParameters parameters)
    {
        if (input.KeyPressed(Keys.OemMinus)) parameters.Erosion = Math.Clamp(parameters.Erosion - 0.02f, 0f, 1f);
        if (input.KeyPressed(Keys.OemPlus)) parameters.Erosion = Math.Clamp(parameters.Erosion + 0.02f, 0f, 1f);
        if (input.KeyPressed(Keys.OemComma)) parameters.RuinDensity = Math.Clamp(parameters.RuinDensity - 0.02f, 0f, 1f);
        if (input.KeyPressed(Keys.OemPeriod)) parameters.RuinDensity = Math.Clamp(parameters.RuinDensity + 0.02f, 0f, 1f);
        if (input.KeyPressed(Keys.OemSemicolon)) parameters.WindStrength = Math.Clamp(parameters.WindStrength - 1f, 0f, 40f);
        if (input.KeyPressed(Keys.OemQuotes)) parameters.WindStrength = Math.Clamp(parameters.WindStrength + 1f, 0f, 40f);
    }

    private void UpdateSeedTyping(InputManager input, GeneratedWorld generated, Action<WorldGenerationConfig> regenerate)
    {
        var digits = new[] { Keys.D0, Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8, Keys.D9 };
        for (var index = 0; index < digits.Length; index++)
            if (input.KeyPressed(digits[index]) && _seedText.Length < 20) _seedText += index;
        if (input.KeyPressed(Keys.OemMinus) && _seedText.Length == 0) _seedText = "-";
        if (input.KeyPressed(Keys.Back) && _seedText.Length > 0) _seedText = _seedText[..^1];
        if (input.KeyPressed(Keys.Escape)) { _typingSeed = false; return; }
        if (!input.KeyPressed(Keys.Enter)) return;
        _typingSeed = false;
        if (!long.TryParse(_seedText, out var seed)) { SetNotice("INVALID SEED"); return; }
        var config = Copy(generated.Configuration);
        config.Seed = seed;
        regenerate(config);
        SetNotice($"GENERATED SEED {seed}");
    }

    private void SetNotice(string notice)
    {
        Notice = notice;
        _noticeTimer = 3f;
    }

    private static WorldGenerationConfig Copy(WorldGenerationConfig source) => new()
    {
        Seed = source.Seed,
        GeneratorVersion = source.GeneratorVersion,
        Variant = source.Variant,
        Parameters = new BadlandsParameters
        {
            Width = source.Parameters.Width,
            Height = source.Parameters.Height,
            Erosion = source.Parameters.Erosion,
            RuinDensity = source.Parameters.RuinDensity,
            EcologyDensity = source.Parameters.EcologyDensity,
            WindStrength = source.Parameters.WindStrength
        }
    };

    private static string Format(string text)
    {
        var result = "";
        foreach (var character in text)
        {
            if (char.IsUpper(character) && result.Length > 0) result += " ";
            result += character;
        }
        return result;
    }
}
