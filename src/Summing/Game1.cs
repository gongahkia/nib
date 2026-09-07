using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Summing.Camera;
using Summing.Core;
using Summing.Entities;
using Summing.Editor;
using Summing.Gameplay;
using Summing.Generation;
using Summing.History;
using Summing.Input;
using Summing.Player;
using Summing.Persistence;
using Summing.Rendering;
using Summing.Telemetry;
using Summing.World;

namespace Summing;

public sealed class Game1 : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch = null!;
    private RenderTarget2D _scene = null!;
    private Texture2D _pixel = null!;
    private PixelFont _font = null!;
    private SpriteLibrary _sprites = null!;
    private VisualPackManager _visualPacks = null!;
    private AtmosphereRenderer _atmosphere = null!;
    private InputManager _input = null!;
    private TileWorld _world = null!;
    private GeneratedWorld _generated = null!;
    private TileWorldRenderer _tileRenderer = null!;
    private readonly Camera2D _camera = new();
    private readonly TerrainBreakEffects _terrainBreakEffects = new();
    private PlayerController _player = null!;
    private DigTool _digTool = null!;
    private BombSystem _bombSystem = null!;
    private RopeSystem _ropeSystem = null!;
    private PlayerInventory _inventory = null!;
    private Difficulty _difficulty = Difficulty.Easy;
    private long _frame;
    private ArchiveStore _archive = null!;
    private RelicSystem _relicSystem = null!;
    private BurrowerSystem _burrowerSystem = null!;
    private BrittleSystem _brittleSystem = null!;
    private readonly EditorSystem _editor = new();
    private readonly List<string> _terrainEvents = [];
    private PlaytestRecorder? _telemetry;
    private readonly long _autoExitFrame;
    private readonly bool _autoStart;
    private readonly bool _captureTitleSmoke;
    private readonly bool _captureVisualSelectorSmoke;
    private bool _visualSelectorSmokeCaptured;
    private readonly bool _loadSavedVisualSelection;
    private readonly bool _syntheticSmoke;
    private readonly BindingMenu _bindingMenu = new();
    private readonly VisualSelectionScreen _visualSelector = new();
    private readonly WorldGenerationConfig _menuConfig;
    private GamePhase _phase = GamePhase.Title;
    private float _phaseTimer;
    private bool _typingTitleSeed;
    private string _titleSeedText = "";
    private int _appearanceIndex;
    private VisualPackId _selectedVisualPack;
    private float _smoothedFps = 60f;
    private static readonly Color[] AppearanceTints = [Color.White, new Color(205, 232, 222), new Color(238, 191, 168)];
    private static readonly string[] AppearanceNames = ["BONE WRAPS", "SALT WRAPS", "OXIDE WRAPS"];
    private static readonly Keys[] TitlePresetKeys = [Keys.D1, Keys.D2, Keys.D3];

    public Game1(long autoExitFrame = 0, bool autoStart = true, bool captureTitleSmoke = false,
        WorldGenerationConfig? initialConfiguration = null, Difficulty initialDifficulty = Difficulty.Easy,
        VisualPackId initialVisualPack = VisualPackId.GandalfOverworld, bool captureVisualSelectorSmoke = false,
        bool loadSavedVisualSelection = true)
    {
        _autoExitFrame = autoExitFrame;
        _autoStart = autoStart;
        _captureTitleSmoke = captureTitleSmoke;
        _captureVisualSelectorSmoke = captureVisualSelectorSmoke;
        _loadSavedVisualSelection = loadSavedVisualSelection;
        _syntheticSmoke = autoExitFrame > 0 && autoStart;
        _menuConfig = initialConfiguration == null ? new WorldGenerationConfig() : CopyConfiguration(initialConfiguration);
        _difficulty = initialDifficulty;
        _selectedVisualPack = VisualPackFiles.IsAvailable(initialVisualPack)
            ? initialVisualPack
            : VisualPackId.SummingOriginal;
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = GameConstants.WindowWidth,
            PreferredBackBufferHeight = GameConstants.WindowHeight,
            SynchronizeWithVerticalRetrace = true
        };
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        IsFixedTimeStep = true;
        TargetElapsedTime = TimeSpan.FromSeconds(GameConstants.FixedDelta);
        Window.AllowUserResizing = true;
        Window.Title = "SUMMING — The Lorn Reach";
    }

    protected override void Initialize()
    {
        _input = new InputManager(InputBindings.LoadOrCreate("saves/bindings.json"));
        _archive = new ArchiveStore("archive/archive.json");
        LoadGeneratedWorld(ValidatedWorldGenerator.Generate(_menuConfig), false);
        if (_captureVisualSelectorSmoke) _visualSelector.Open();
        if (_autoExitFrame > 0 && _autoStart) StartRun(CopyConfiguration(_menuConfig));
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _scene = new RenderTarget2D(GraphicsDevice, GameConstants.VirtualWidth, GameConstants.VirtualHeight, false,
            SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData([Color.White]);
        _font = new PixelFont(_pixel);
        _visualPacks = new VisualPackManager(GraphicsDevice, _selectedVisualPack,
            loadSavedSelection: _loadSavedVisualSelection && _autoExitFrame == 0,
            persistSelection: _autoExitFrame == 0);
        _tileRenderer = new TileWorldRenderer(_pixel, _visualPacks);
        _sprites = new SpriteLibrary(GraphicsDevice, _visualPacks);
        _atmosphere = new AtmosphereRenderer(_pixel);
    }

    protected override void Update(GameTime gameTime)
    {
        var playerScreen = _camera.WorldToScreen(_player.Bounds.Center);
        _input.Update(playerScreen, new Vector2(GraphicsDevice.Viewport.Width / (float)GameConstants.VirtualWidth,
            GraphicsDevice.Viewport.Height / (float)GameConstants.VirtualHeight));
        _frame++;
        _telemetry?.BeginFrame(_frame);
        if (_syntheticSmoke) ApplySyntheticSmokeInput();
        if (_captureVisualSelectorSmoke) ApplySyntheticVisualSelectorInput();
        if (_autoExitFrame > 0 && _frame >= _autoExitFrame && _phase is GamePhase.Title or GamePhase.Binding &&
            (!_captureVisualSelectorSmoke || _visualSelectorSmokeCaptured)) Exit();
        _smoothedFps = MathHelper.Lerp(_smoothedFps,
            (float)(1.0 / Math.Max(0.0001, gameTime.ElapsedGameTime.TotalSeconds)), 0.03f);
        if (_phase == GamePhase.Binding)
        {
            if (_bindingMenu.Update(_input, "saves/bindings.json")) _phase = GamePhase.Title;
            base.Update(gameTime);
            return;
        }
        if (_visualSelector.Active)
        {
            if (_visualSelector.Update(_input, _visualPacks, out var change)) RecordVisualSelectionChange(change);
            base.Update(gameTime);
            return;
        }
        if (_input.KeyPressed(Keys.K) || _phase == GamePhase.Title && _input.ButtonPressed(Buttons.LeftShoulder))
        {
            _visualSelector.Open();
            base.Update(gameTime);
            return;
        }
        if (_phase == GamePhase.Title)
        {
            UpdateTitle();
            base.Update(gameTime);
            return;
        }
        if (_phase == GamePhase.Paused)
        {
            if (_input.Pressed(InputAction.Pause) || _input.Pressed(InputAction.Jump)) _phase = GamePhase.Playing;
            base.Update(gameTime);
            return;
        }
        if (_phase is GamePhase.Dead or GamePhase.Complete)
        {
            _phaseTimer += GameConstants.FixedDelta;
            if (_phaseTimer >= 0.4f && _telemetry is { IsFinished: false })
                _telemetry.Finish(_phase == GamePhase.Dead ? "death" : "summit");
            if (_phaseTimer >= 0.55f && (_input.KeyPressed(Microsoft.Xna.Framework.Input.Keys.Enter) || _input.Pressed(InputAction.Jump)))
            {
                if (_phase == GamePhase.Dead) RestartAfterDeath();
                else _phase = GamePhase.Title;
            }
            if (_autoExitFrame > 0 && _frame >= _autoExitFrame) Exit();
            base.Update(gameTime);
            return;
        }

        _editor.Update(_input, _camera, GraphicsDevice.Viewport, _generated, _difficulty, RegenerateWorld,
            ToggleDifficultyAndRegenerate, SaveEditorWorld, LoadEditorWorld, ExportWorld, GameConstants.FixedDelta);
        if (_editor.Active)
        {
            base.Update(gameTime);
            return;
        }
        if (_input.Pressed(InputAction.Pause))
        {
            _phase = GamePhase.Paused;
            base.Update(gameTime);
            return;
        }
        _player.Update(_input, _world, GameConstants.FixedDelta);
        var wind = _generated.WindAt(_player.Position.Y);
        _player.Velocity += new Vector2(wind * GameConstants.FixedDelta, 0f);
        _ropeSystem.Update(_input, _player, _inventory, _world, GameConstants.FixedDelta);
        _digTool.Update(_input, _player, _world, GameConstants.FixedDelta);
        _bombSystem.Update(_input, _player, _inventory, _world, _frame, GameConstants.FixedDelta);
        _relicSystem.Update(_player, GameConstants.FixedDelta);
        _burrowerSystem.Update(_player, _world, GameConstants.FixedDelta);
        _brittleSystem.Update(_player, _world, GameConstants.FixedDelta);
        _terrainBreakEffects.Update(GameConstants.FixedDelta);
        if (_player.Position.Y > _world.PixelHeight + 80f && _player.Alive)
            _player.ApplyDamage(99, Vector2.Zero, "void");
        var cameraTarget = new Vector2(
            Math.Clamp(_player.Position.X, _camera.VisibleWorldWidth * 0.5f,
                _world.PixelWidth - _camera.VisibleWorldWidth * 0.5f),
            Math.Clamp(_player.Position.Y - 25f, _camera.VisibleWorldHeight * 0.5f,
                _world.PixelHeight - _camera.VisibleWorldHeight * 0.5f));
        _camera.Update(cameraTarget, _player.Velocity, GameConstants.FixedDelta);
        if (_input.KeyPressed(Keys.F8) || _input.ButtonPressed(Buttons.RightShoulder))
            _telemetry?.RecordEvent("playtest-bookmark", new
            {
                _player.Position,
                tile = _world.WorldToTile(_player.Bounds.Center),
                routeAnchor = NearestRouteAnchorIndex(_player.Position),
                state = _player.VisualState
            });
        _telemetry?.RecordFrame(_frame, _input, _player, _camera, _world, _inventory, _generated);
        if (!_player.Alive)
        {
            _phase = GamePhase.Dead;
            _phaseTimer = 0f;
        }
        else if (_player.Bounds.Intersects(new Aabb(_generated.SummitBounds.X, _generated.SummitBounds.Y,
                     _generated.SummitBounds.Width, _generated.SummitBounds.Height)))
        {
            _telemetry?.RecordEvent("summit-completion", new { _player.Position, frame = _frame });
            _phase = GamePhase.Complete;
            _phaseTimer = 0f;
        }
        if (_autoExitFrame > 0 && _frame >= _autoExitFrame) Exit();
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.SetRenderTarget(_scene);
        GraphicsDevice.Clear(GamePalette.Void);
        if (_phase == GamePhase.Binding)
        {
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            _bindingMenu.Draw(_spriteBatch, _pixel, _font, _input.Bindings);
            _spriteBatch.End();
        }
        else if (_phase == GamePhase.Title)
        {
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            DrawTitle();
            _visualSelector.Draw(_spriteBatch, _pixel, _font, _sprites, _visualPacks, _frame,
                AppearanceTints[_appearanceIndex]);
            _spriteBatch.End();
        }
        else
        {
            _spriteBatch.Begin(transformMatrix: _camera.View, samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
            DrawBackdrop();
            _atmosphere.DrawWorldDither(_spriteBatch, _camera.Position, _frame, Camera2D.WorldZoom);
            _tileRenderer.Draw(_spriteBatch, _world, _camera.Position, Camera2D.WorldZoom);
            _terrainBreakEffects.Draw(_spriteBatch, _pixel);
            foreach (var feature in _generated.Features)
                if (feature.Kind != WorldFeatureKind.RelicCandidate) _sprites.DrawFeature(_spriteBatch, feature);
            DrawSummitMarker();
            _brittleSystem.Draw(_spriteBatch, _sprites);
            _relicSystem.Draw(_spriteBatch, _sprites);
            _ropeSystem.Draw(_spriteBatch, _pixel);
            _bombSystem.Draw(_spriteBatch, _pixel);
            _burrowerSystem.Draw(_spriteBatch, _sprites);
            _player.Draw(_spriteBatch, _sprites, _frame, AppearanceTints[_appearanceIndex]);
            _digTool.Draw(_spriteBatch, _pixel, _player);
            _editor.DrawWorld(_spriteBatch, _pixel);
            _spriteBatch.End();

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            DrawHud();
            _editor.DrawOverlay(_spriteBatch, _pixel, _font, _generated, _difficulty, _visualPacks.CurrentName);
            DrawPhaseOverlay();
            _visualSelector.Draw(_spriteBatch, _pixel, _font, _sprites, _visualPacks, _frame,
                AppearanceTints[_appearanceIndex]);
            _spriteBatch.End();
        }
        GraphicsDevice.SetRenderTarget(null);
        _telemetry?.CaptureDue(_scene);
        if (_captureTitleSmoke && _frame == 12)
        {
            Directory.CreateDirectory("artifacts");
            using var stream = File.Create("artifacts/title-smoke.png");
            _scene.SaveAsPng(stream, _scene.Width, _scene.Height);
        }
        if (_captureVisualSelectorSmoke && _frame >= 22 && !_visualSelectorSmokeCaptured)
        {
            Directory.CreateDirectory("artifacts");
            using var stream = File.Create("artifacts/visual-selector-smoke.png");
            _scene.SaveAsPng(stream, _scene.Width, _scene.Height);
            _visualSelectorSmokeCaptured = true;
        }

        GraphicsDevice.Clear(Color.Black);
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _spriteBatch.Draw(_scene, GraphicsDevice.Viewport.Bounds, Color.White);
        _spriteBatch.End();
        base.Draw(gameTime);
    }

    private void DrawTitle()
    {
        _spriteBatch.Draw(_pixel, new Rectangle(0, 0, 640, 360), GamePalette.DeepSky);
        for (var x = 0; x < 640; x += 22)
        {
            var height = 35 + Math.Abs((x * 13) % 95);
            _spriteBatch.Draw(_pixel, new Rectangle(x, 360 - height, 17, height), GamePalette.FarStone);
        }
        _spriteBatch.Draw(_pixel, new Rectangle(0, 280, 640, 80), new Color(10, 13, 20, 210));
        _font.Draw(_spriteBatch, "SUMMING", new Vector2(182, 40), GamePalette.Bone, 6);
        _font.Draw(_spriteBatch, "THE LORN REACH", new Vector2(244, 79), GamePalette.SacredGold);
        _font.Draw(_spriteBatch, "A SILENT ASCENT THROUGH THE REMAINS OF WEATHER", new Vector2(166, 102), GamePalette.UiMuted);
        _font.Draw(_spriteBatch, $"DIFFICULTY  {_difficulty}", new Vector2(210, 148), _difficulty == Difficulty.Easy ? GamePalette.SaltCyan : GamePalette.Danger, 2);
        _font.Draw(_spriteBatch, $"GENERATOR   {_menuConfig.Variant}", new Vector2(210, 171), Color.White, 2);
        _font.Draw(_spriteBatch, $"SEED        {_menuConfig.Seed}", new Vector2(210, 194), GamePalette.Bone, 2);
        _font.Draw(_spriteBatch, $"CLIMBER     {AppearanceNames[_appearanceIndex]}", new Vector2(210, 217), AppearanceTints[_appearanceIndex], 2);
        _font.Draw(_spriteBatch, $"PLAYER ART  {_visualPacks.PlayerName}", new Vector2(210, 240), GamePalette.SaltCyan);
        _font.Draw(_spriteBatch, $"WORLD ART   {_visualPacks.EnvironmentName}", new Vector2(210, 251), GamePalette.SaltCyan);
        _font.Draw(_spriteBatch, $"PRESET {DevelopmentSeedPresets.Label(_menuConfig)}", new Vector2(210, 263), GamePalette.SaltCyan);
        _font.Draw(_spriteBatch, "UP DOWN DIFFICULTY   LEFT RIGHT GENERATOR", new Vector2(187, 276), GamePalette.UiMuted);
        _font.Draw(_spriteBatch, "T SEED  R OR LS RANDOM  C OR Y CLIMBER", new Vector2(190, 287), GamePalette.UiMuted);
        _font.Draw(_spriteBatch, "1 2 3 OR RB PRESET   K OR LB VISUAL SELECTOR", new Vector2(174, 298), GamePalette.UiMuted);
        _font.Draw(_spriteBatch, "ENTER OR A TO ASCEND", new Vector2(224, 313), GamePalette.SacredGold, 2);
        _font.Draw(_spriteBatch, "B BINDINGS  F1 WORLD EDITOR DURING A RUN", new Vector2(195, 341), new Color(105, 116, 125));
        if (_typingTitleSeed)
        {
            _spriteBatch.Draw(_pixel, new Rectangle(154, 132, 332, 92), new Color(7, 9, 14, 245));
            _font.Draw(_spriteBatch, "ENTER WORLD SEED", new Vector2(218, 151), GamePalette.SacredGold, 2);
            _font.Draw(_spriteBatch, _titleSeedText + "_", new Vector2(218, 183), Color.White, 2);
            _font.Draw(_spriteBatch, "ENTER ACCEPTS  ESC CANCELS", new Vector2(236, 210), GamePalette.UiMuted);
        }
    }

    private void DrawSummitMarker()
    {
        var bounds = _generated.SummitBounds;
        var center = bounds.Center;
        _spriteBatch.Draw(_pixel, new Rectangle(center.X - 18, bounds.Bottom - 62, 36, 62), new Color(48, 45, 55));
        _spriteBatch.Draw(_pixel, new Rectangle(center.X - 4, bounds.Bottom - 92, 8, 34), GamePalette.SacredGold);
        _spriteBatch.Draw(_pixel, new Rectangle(center.X - 12, bounds.Bottom - 88, 24, 3), GamePalette.SaltCyan);
        _spriteBatch.Draw(_pixel, new Rectangle(center.X - 20, bounds.Bottom - 66, 40, 4), GamePalette.Bone);
    }

    private void DrawPhaseOverlay()
    {
        if (_phase is not (GamePhase.Paused or GamePhase.Dead or GamePhase.Complete)) return;
        _spriteBatch.Draw(_pixel, new Rectangle(0, 0, 640, 360), new Color(5, 7, 12, 190));
        if (_phase == GamePhase.Paused)
        {
            _font.Draw(_spriteBatch, "PAUSED", new Vector2(248, 126), GamePalette.Bone, 4);
            _font.Draw(_spriteBatch, "ESC START OR A TO RETURN", new Vector2(229, 181), GamePalette.UiMuted);
        }
        else if (_phase == GamePhase.Dead)
        {
            _font.Draw(_spriteBatch, "BODY LOST", new Vector2(208, 118), GamePalette.Danger, 4);
            _font.Draw(_spriteBatch, "THE ARCHIVE REMAINS", new Vector2(238, 170), GamePalette.SacredGold);
            _font.Draw(_spriteBatch, _difficulty == Difficulty.Easy ? "ENTER REPEATS THIS WORLD" : "ENTER ACCEPTS ANOTHER DEAD WORLD",
                new Vector2(_difficulty == Difficulty.Easy ? 228 : 205, 203), GamePalette.UiMuted);
        }
        else
        {
            _font.Draw(_spriteBatch, "SUMMIT REACHED", new Vector2(164, 115), GamePalette.SacredGold, 4);
            _font.Draw(_spriteBatch, $"ASCENT TIME {_frame / 60f:0.0} SECONDS", new Vector2(232, 172), GamePalette.Bone);
            _font.Draw(_spriteBatch, "ENTER RETURNS TO THE REACH", new Vector2(220, 204), GamePalette.UiMuted);
        }
    }

    private void DrawBackdrop()
    {
        _spriteBatch.Draw(_pixel, new Rectangle(-100, -100, _world.PixelWidth + 200, _world.PixelHeight + 200), GamePalette.DeepSky);
        DrawDistantRidge((int)_camera.Position.Y + 105, 92, new Color(20, 24, 37), 31);
        DrawDistantRidge((int)_camera.Position.Y + 145, 68, GamePalette.FarStone, 53);
        for (var x = -80; x < _world.PixelWidth + 80; x += 32)
        {
            var height = 30 + Math.Abs((x * 17) % 90);
            _spriteBatch.Draw(_pixel, new Rectangle(x, _world.PixelHeight - height - 30, 25, height), GamePalette.FarStone);
        }
    }

    private void DrawDistantRidge(int horizon, int step, Color color, int phase)
    {
        var viewLeft = (int)_camera.Position.X - (int)(_camera.VisibleWorldWidth * 0.5f) - step;
        var first = (int)MathF.Floor(viewLeft / (float)step) * step;
        for (var x = first; x < viewLeft + _camera.VisibleWorldWidth + step * 2; x += step)
        {
            var height = 34 + Math.Abs((x / step * 37 + phase) % 92);
            var width = step - 5;
            _spriteBatch.Draw(_pixel, new Rectangle(x, horizon - height, width, height + 240), color);
            _spriteBatch.Draw(_pixel, new Rectangle(x, horizon - height, width, 2), color == GamePalette.FarStone ? GamePalette.Stone : GamePalette.FarStone);
        }
    }

    private void UpdateTitle()
    {
        if (_typingTitleSeed)
        {
            var digits = new[] { Keys.D0, Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8, Keys.D9 };
            for (var index = 0; index < digits.Length; index++)
                if (_input.KeyPressed(digits[index]) && _titleSeedText.Length < 20) _titleSeedText += index;
            if (_input.KeyPressed(Keys.OemMinus) && _titleSeedText.Length == 0) _titleSeedText = "-";
            if (_input.KeyPressed(Keys.Back) && _titleSeedText.Length > 0) _titleSeedText = _titleSeedText[..^1];
            if (_input.KeyPressed(Keys.Escape)) { _typingTitleSeed = false; return; }
            if (_input.KeyPressed(Keys.Enter) && long.TryParse(_titleSeedText, out var seed))
            {
                _menuConfig.Seed = seed;
                _typingTitleSeed = false;
            }
            return;
        }

        if (_input.Pressed(InputAction.Up) || _input.Pressed(InputAction.Down))
            _difficulty = _difficulty == Difficulty.Easy ? Difficulty.Hard : Difficulty.Easy;
        if (_input.Pressed(InputAction.Left))
            _menuConfig.Variant = (GeneratorVariant)(((int)_menuConfig.Variant + 2) % 3);
        if (_input.Pressed(InputAction.Right))
            _menuConfig.Variant = (GeneratorVariant)(((int)_menuConfig.Variant + 1) % 3);
        if (_input.KeyPressed(Keys.T)) { _typingTitleSeed = true; _titleSeedText = _menuConfig.Seed.ToString(); }
        for (var index = 0; index < TitlePresetKeys.Length; index++)
            if (_input.KeyPressed(TitlePresetKeys[index])) DevelopmentSeedPresets.Apply(_menuConfig, index);
        if (_input.KeyPressed(Keys.P) || _input.ButtonPressed(Buttons.RightShoulder))
            DevelopmentSeedPresets.Apply(_menuConfig, DevelopmentSeedPresets.Next(_menuConfig));
        if (_input.KeyPressed(Keys.R) || _input.Pressed(InputAction.Rope))
            _menuConfig.Seed = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        if (_input.KeyPressed(Keys.C) || _input.Pressed(InputAction.Dig))
            _appearanceIndex = (_appearanceIndex + 1) % AppearanceTints.Length;
        if (_input.KeyPressed(Keys.B) || _input.Pressed(InputAction.Slide)) { _phase = GamePhase.Binding; return; }
        if (_input.KeyPressed(Keys.Escape)) { Exit(); return; }
        if (_input.KeyPressed(Keys.Enter) || _input.Pressed(InputAction.Jump)) StartRun(CopyConfiguration(_menuConfig));
    }

    private void ApplySyntheticSmokeInput()
    {
        var actions = new List<InputAction>();
        var move = _frame == 82 ? Vector2.UnitY : _frame is >= 12 and <= 155 ? Vector2.UnitX : Vector2.Zero;
        if (move.X > 0f) actions.Add(InputAction.Right);
        if (move.Y > 0f) actions.Add(InputAction.Down);
        if (_frame is >= 18 and <= 34) actions.Add(InputAction.Jump);
        if (_frame == 8) actions.Add(InputAction.Bomb);
        if (_frame == 42) actions.Add(InputAction.Dash);
        if (_frame == 82) actions.Add(InputAction.Dig);
        if (_frame == 108) actions.Add(InputAction.Slide);
        if (_frame == 126) actions.Add(InputAction.Rope);
        if (_frame == 180)
            RecordVisualSelectionChange(_visualPacks.Adjust(VisualSelectionOption.EnvironmentPack, 1));
        if (_frame == 190)
            RecordVisualSelectionChange(_visualPacks.Adjust(VisualSelectionOption.EnvironmentPack, 1));
        if (_frame == 195)
            RecordVisualSelectionChange(_visualPacks.Adjust(VisualSelectionOption.PlayerPack, 1));
        if (_frame == 200)
            RecordVisualSelectionChange(_visualPacks.Adjust(VisualSelectionOption.EnvironmentPack, 1));
        if (_frame == 205)
            RecordVisualSelectionChange(_visualPacks.Adjust(VisualSelectionOption.SkinSheet, 1));
        if (_frame == 210)
            RecordVisualSelectionChange(_visualPacks.Adjust(VisualSelectionOption.EnvironmentPack, 1));
        if (_frame == 215)
            RecordVisualSelectionChange(_visualPacks.Adjust(VisualSelectionOption.TorsoSheet, 1));
        if (_frame == 220)
            RecordVisualSelectionChange(_visualPacks.Adjust(VisualSelectionOption.EnvironmentPack, 1));
        if (_frame == 230)
            RecordVisualSelectionChange(_visualPacks.Adjust(VisualSelectionOption.EnvironmentPack, 1));
        _input.SetSyntheticState(move, Vector2.UnitX, actions.ToArray());
    }

    private void ApplySyntheticVisualSelectorInput()
    {
        var actions = _frame switch
        {
            2 or 6 or 10 or 20 => new[] { InputAction.Right },
            4 or 8 or 12 or 14 or 16 or 18 => new[] { InputAction.Down },
            _ => []
        };
        _input.SetSyntheticState(Vector2.Zero, Vector2.UnitX, actions);
    }

    private void StartRun(WorldGenerationConfig configuration)
    {
        var generated = ValidatedWorldGenerator.Generate(configuration);
        _menuConfig.Seed = generated.Configuration.Seed;
        _menuConfig.Variant = generated.Configuration.Variant;
        LoadGeneratedWorld(generated);
        _phase = GamePhase.Playing;
        _phaseTimer = 0f;
    }

    private void RestartAfterDeath()
    {
        var configuration = CopyConfiguration(_generated.Configuration);
        if (_difficulty == Difficulty.Hard)
            configuration.Seed = unchecked(configuration.Seed + (long)0x61c8864680b583ebUL);
        StartRun(configuration);
    }

    private void LoadGeneratedWorld(GeneratedWorld generated, bool startTelemetry = true)
    {
        _telemetry?.Finish("world-replaced");
        _telemetry = null;
        _frame = 0;
        _generated = generated;
        _world = generated.Terrain;
        _terrainBreakEffects.Clear();
        _terrainEvents.Clear();
        _world.TerrainChanged += change =>
        {
            _terrainBreakEffects.Emit(change);
            var shake = change.Destroyed ? change.Cause switch
            {
                "bomb" => 12f,
                "brittle-collapse" => 9f,
                "burrower" => 5f,
                _ => 7.5f
            } : change.Cause == "tool" ? 2.5f : 0f;
            if (shake > 0f) _camera.AddShake(shake);
            _terrainEvents.Add($"{_frame}:{change.Cause}:{change.Tile.X},{change.Tile.Y}:{change.Material}:{change.Damage}:{change.Destroyed}");
            if (_terrainEvents.Count > 512) _terrainEvents.RemoveAt(0);
            _telemetry?.RecordEvent(change.Destroyed ? "terrain-destroyed" : "terrain-damaged", change,
                change.Destroyed);
        };
        var tuning = DifficultyTuning.For(_difficulty);
        _player = new PlayerController(generated.Spawn, tuning.StartingHealth);
        _inventory = new PlayerInventory(tuning);
        _digTool = new DigTool();
        _bombSystem = new BombSystem();
        _ropeSystem = new RopeSystem();
        _relicSystem = new RelicSystem(generated, _archive);
        _burrowerSystem = new BurrowerSystem(generated, tuning);
        _brittleSystem = new BrittleSystem(generated, tuning);
        _bombSystem.Exploded += explosion =>
        {
            _camera.AddShake(14f);
            _burrowerSystem.ApplyExplosion(explosion);
            _telemetry?.RecordEvent("bomb-explosion", explosion);
        };
        _bombSystem.PlayerBoosted += impact => _telemetry?.RecordEvent("bomb-rocket-jump",
            new { _player.Position, impact.Distance, impact.Impulse, health = _player.Health });
        _bombSystem.Placed += bomb => _telemetry?.RecordEvent("bomb-use",
            new { bomb.Position, bomb.Velocity, bomb.Fuse }, false);
        _ropeSystem.Placed += rope => _telemetry?.RecordEvent("rope-use",
            new { rope.X, rope.Top, rope.Bottom, rope.LedgeDirection });
        _ropeSystem.PlacementRejected += reason => _telemetry?.RecordEvent("rope-placement-rejected",
            new { reason, _player.Position, facing = _player.Facing }, false);
        _player.StatusEvent += status => _telemetry?.RecordEvent(status.StartsWith("death", StringComparison.Ordinal)
            ? "death" : status.StartsWith("fall", StringComparison.Ordinal) ? "large-fall" : "damage", new { status, _player.Health });
        _player.Dashed += () => _telemetry?.RecordEvent("dash",
            new { _player.Position, _player.Velocity, _player.DashCharges });
        _digTool.Impact += change => _telemetry?.RecordEvent("tool-impact", change, change.Destroyed);
        _burrowerSystem.Event += value => _telemetry?.RecordEvent(value == "attack" ? "burrower-attack" : "burrower-interaction",
            new { value }, value == "attack");
        _brittleSystem.Triggered += tile => _telemetry?.RecordEvent("brittle-triggered", new { tile }, false);
        _brittleSystem.Collapsed += tile => _telemetry?.RecordEvent("brittle-collapse", new { tile });
        _relicSystem.Collected += relic => _telemetry?.RecordEvent("relic-collected", new { relic.Id, relic.Culture.Name });
        _camera.Snap(new Vector2(generated.Spawn.X, generated.Spawn.Y - 40f));
        if (startTelemetry)
        {
            var visualSelection = _visualPacks == null
                ? VisualPackManager.StartupSelection(_selectedVisualPack)
                : _visualPacks.Selection;
            _telemetry = new PlaytestRecorder(generated, _difficulty, _input.Bindings, visualSelection);
            SaveWorld(Path.Combine(_telemetry.DirectoryPath, "world-start.json"));
        }
    }

    private void RegenerateWorld(WorldGenerationConfig configuration) =>
        LoadGeneratedWorld(ValidatedWorldGenerator.Generate(configuration));

    private void ToggleDifficultyAndRegenerate(WorldGenerationConfig configuration)
    {
        _difficulty = _difficulty == Difficulty.Easy ? Difficulty.Hard : Difficulty.Easy;
        RegenerateWorld(configuration);
    }

    private void RecordVisualSelectionChange(VisualSelectionChange change)
    {
        if (!change.Changed || _telemetry is not { IsFinished: false }) return;
        _telemetry.RecordEvent("visual-selection-changed", new
        {
            option = change.Option,
            selection = _visualPacks.Selection
        });
    }

    private void SaveEditorWorld() => SaveWorld("saves/editor-world.json");

    private void LoadEditorWorld()
    {
        const string path = "saves/editor-world.json";
        if (File.Exists(path)) LoadGeneratedWorld(WorldSerializer.Load(path));
    }

    private void ExportWorld()
    {
        var path = $"exports/world-{_generated.Configuration.Seed}-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}.json";
        SaveWorld(path);
    }

    private void SaveWorld(string path) => WorldSerializer.Save(path, _generated, _player, _inventory,
        _burrowerSystem, _brittleSystem, _relicSystem, _ropeSystem, _bombSystem, _difficulty, _frame, _terrainEvents);

    private static WorldGenerationConfig CopyConfiguration(WorldGenerationConfig source) => new()
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

    protected override void UnloadContent()
    {
        _sprites.Dispose();
        _visualPacks.Dispose();
        _scene.Dispose();
        _pixel.Dispose();
        _spriteBatch.Dispose();
        base.UnloadContent();
    }

    protected override void OnExiting(object sender, ExitingEventArgs args)
    {
        _telemetry?.Finish("application-exit");
        base.OnExiting(sender, args);
    }

    private void DrawHud()
    {
        _spriteBatch.Draw(_pixel, new Rectangle(7, 7, 310, 51), new Color(6, 8, 13, 220));
        _font.Draw(_spriteBatch, $"STATE {Format(_player.VisualState.ToString())}", new Vector2(12, 12), new Color(226, 207, 158));
        _font.Draw(_spriteBatch, $"VEL {(int)_player.Velocity.X},{(int)_player.Velocity.Y}  DASH {_player.DashCharges}", new Vector2(12, 21), new Color(144, 158, 166));
        _font.Draw(_spriteBatch, $"WALL STAMINA {(int)(_player.WallStamina * 100f / PlayerController.WallStaminaMaximum)}%", new Vector2(12, 30), new Color(144, 158, 166));
        var target = _world.GetTile(_digTool.TargetTile.X, _digTool.TargetTile.Y);
        var targetName = target.Solid ? World.Materials.MaterialCatalog.Get(target.Material).Name : "air";
        _font.Draw(_spriteBatch, $"TOOL {targetName}", new Vector2(12, 39), new Color(169, 124, 94));
        _font.Draw(_spriteBatch, $"HEALTH {_player.Health}/{_player.MaximumHealth}  BOMB {_inventory.Bombs}  ROPE {_inventory.Ropes}",
            new Vector2(12, 48), new Color(205, 111, 92));
        _font.Draw(_spriteBatch, $"SEED {_generated.Configuration.Seed}  {Format(_generated.Configuration.Variant.ToString())}",
            new Vector2(350, 12), GamePalette.UiMuted);
        _font.Draw(_spriteBatch, $"ARCHIVE {_archive.Discoveries.Count}", new Vector2(520, 21), GamePalette.SacredGold);
        _font.Draw(_spriteBatch, $"VALID ROUTE {_generated.Diagnostics.Traversability.CheckedTransitions}  REJECTED {_generated.Diagnostics.RejectedSeeds.Count}",
            new Vector2(350, 30), _generated.Diagnostics.Traversability.Solvable ? GamePalette.SaltCyan : GamePalette.Danger);
        var altitude = Math.Clamp((int)((1f - _player.Position.Y / _world.PixelHeight) * 100f), 0, 100);
        _font.Draw(_spriteBatch, $"ALTITUDE {altitude}%  FPS {(int)_smoothedFps}", new Vector2(480, 39), GamePalette.UiMuted);
        if (_relicSystem.DiscoveryVisible && _relicSystem.LastDiscovery != null)
        {
            _spriteBatch.Draw(_pixel, new Rectangle(125, 300, 390, 24), new Color(7, 10, 15, 230));
            _font.Draw(_spriteBatch, _relicSystem.LastDiscovery, new Vector2(141, 309), GamePalette.SacredGold);
        }
        if (_ropeSystem.NoticeVisible)
            _font.Draw(_spriteBatch, _ropeSystem.Notice, new Vector2(12, 328), GamePalette.SacredGold);
        _font.Draw(_spriteBatch, "WASD MOVE  SPACE JUMP  SHIFT DASH  LMB DIG  R ROPE  Q BOMB/BOOST  K VISUALS", new Vector2(12, 342), new Color(131, 132, 139));
    }

    private static string Format(string value)
    {
        var result = "";
        foreach (var character in value)
        {
            if (char.IsUpper(character) && result.Length > 0) result += " ";
            result += character;
        }
        return result;
    }

    private int NearestRouteAnchorIndex(Vector2 position)
    {
        var tile = _world.WorldToTile(position);
        var nearest = 0;
        var best = int.MaxValue;
        for (var index = 0; index < _generated.RouteAnchors.Count; index++)
        {
            var anchor = _generated.RouteAnchors[index];
            var distance = Math.Abs(anchor.X - tile.X) + Math.Abs(anchor.Y - tile.Y);
            if (distance >= best) continue;
            best = distance;
            nearest = index;
        }
        return nearest;
    }
}
