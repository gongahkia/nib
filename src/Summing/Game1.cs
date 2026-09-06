using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Summing.Camera;
using Summing.Core;
using Summing.Entities;
using Summing.Gameplay;
using Summing.Generation;
using Summing.History;
using Summing.Input;
using Summing.Player;
using Summing.Rendering;
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
    private AtmosphereRenderer _atmosphere = null!;
    private InputManager _input = null!;
    private TileWorld _world = null!;
    private GeneratedWorld _generated = null!;
    private TileWorldRenderer _tileRenderer = null!;
    private readonly Camera2D _camera = new();
    private PlayerController _player = null!;
    private readonly DigTool _digTool = new();
    private readonly BombSystem _bombSystem = new();
    private readonly RopeSystem _ropeSystem = new();
    private PlayerInventory _inventory = null!;
    private readonly Difficulty _difficulty = Difficulty.Easy;
    private long _frame;
    private ArchiveStore _archive = null!;
    private RelicSystem _relicSystem = null!;
    private BurrowerSystem _burrowerSystem = null!;
    private BrittleSystem _brittleSystem = null!;

    public Game1()
    {
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
        Window.Title = "SUMMING — movement laboratory";
    }

    protected override void Initialize()
    {
        _input = new InputManager(InputBindings.LoadOrCreate("saves/bindings.json"));
        _generated = WorldGeneratorRegistry.Generate(new WorldGenerationConfig());
        _world = _generated.Terrain;
        _archive = new ArchiveStore("archive/archive.json");
        _relicSystem = new RelicSystem(_generated, _archive);
        var tuning = DifficultyTuning.For(_difficulty);
        _player = new PlayerController(_generated.Spawn, tuning.StartingHealth);
        _inventory = new PlayerInventory(tuning);
        _burrowerSystem = new BurrowerSystem(_generated, tuning);
        _brittleSystem = new BrittleSystem(_generated, tuning);
        _bombSystem.Exploded += _burrowerSystem.ApplyExplosion;
        _camera.Snap(new Vector2(_generated.Spawn.X, _generated.Spawn.Y - 40f));
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
        _tileRenderer = new TileWorldRenderer(_pixel);
        _sprites = new SpriteLibrary(GraphicsDevice);
        _atmosphere = new AtmosphereRenderer(_pixel);
    }

    protected override void Update(GameTime gameTime)
    {
        var playerScreen = _camera.WorldToScreen(_player.Bounds.Center);
        _input.Update(playerScreen);
        _frame++;
        if (_input.Pressed(InputAction.Pause)) Exit();
        _player.Update(_input, _world, GameConstants.FixedDelta);
        var wind = _generated.WindAt(_player.Position.Y);
        _player.Velocity += new Vector2(wind * GameConstants.FixedDelta, 0f);
        _ropeSystem.Update(_input, _player, _inventory, _world, GameConstants.FixedDelta);
        _digTool.Update(_input, _player, _world, GameConstants.FixedDelta);
        _bombSystem.Update(_input, _player, _inventory, _world, _frame, GameConstants.FixedDelta);
        _relicSystem.Update(_player, GameConstants.FixedDelta);
        _burrowerSystem.Update(_player, _world, GameConstants.FixedDelta);
        _brittleSystem.Update(_player, _world, GameConstants.FixedDelta);
        if (_player.Position.Y > _world.PixelHeight + 80f)
            _player.Reset(new Vector2(7f * GameConstants.TileSize, 15f * GameConstants.TileSize));
        var cameraTarget = new Vector2(
            Math.Clamp(_player.Position.X, GameConstants.VirtualWidth * 0.5f, _world.PixelWidth - GameConstants.VirtualWidth * 0.5f),
            Math.Clamp(_player.Position.Y - 25f, GameConstants.VirtualHeight * 0.5f, _world.PixelHeight - GameConstants.VirtualHeight * 0.5f));
        _camera.Update(cameraTarget, _player.Velocity, GameConstants.FixedDelta);
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.SetRenderTarget(_scene);
        GraphicsDevice.Clear(new Color(10, 13, 20));
        _spriteBatch.Begin(transformMatrix: _camera.View, samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
        DrawBackdrop();
        _atmosphere.DrawWorldDither(_spriteBatch, _camera.Position, _frame);
        _tileRenderer.Draw(_spriteBatch, _world, _camera.Position);
        foreach (var feature in _generated.Features)
            if (feature.Kind != WorldFeatureKind.RelicCandidate) _sprites.DrawFeature(_spriteBatch, feature);
        _brittleSystem.Draw(_spriteBatch, _sprites);
        _relicSystem.Draw(_spriteBatch, _sprites);
        _ropeSystem.Draw(_spriteBatch, _pixel);
        _bombSystem.Draw(_spriteBatch, _pixel);
        _burrowerSystem.Draw(_spriteBatch, _sprites);
        _player.Draw(_spriteBatch, _pixel, _sprites, _frame);
        _digTool.Draw(_spriteBatch, _pixel, _player);
        _spriteBatch.End();

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        DrawHud();
        _spriteBatch.End();
        GraphicsDevice.SetRenderTarget(null);

        GraphicsDevice.Clear(Color.Black);
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _spriteBatch.Draw(_scene, GraphicsDevice.Viewport.Bounds, Color.White);
        _spriteBatch.End();
        base.Draw(gameTime);
    }

    private void DrawBackdrop()
    {
        _spriteBatch.Draw(_pixel, new Rectangle(-100, -100, _world.PixelWidth + 200, _world.PixelHeight + 200), GamePalette.DeepSky);
        for (var x = -80; x < _world.PixelWidth + 80; x += 32)
        {
            var height = 30 + Math.Abs((x * 17) % 90);
            _spriteBatch.Draw(_pixel, new Rectangle(x, _world.PixelHeight - height - 30, 25, height), GamePalette.FarStone);
        }
    }

    protected override void UnloadContent()
    {
        _sprites.Dispose();
        _scene.Dispose();
        _pixel.Dispose();
        _spriteBatch.Dispose();
        base.UnloadContent();
    }

    private void DrawHud()
    {
        _spriteBatch.Draw(_pixel, new Rectangle(7, 7, 250, 39), new Color(6, 8, 13, 220));
        _font.Draw(_spriteBatch, $"STATE {Format(_player.VisualState.ToString())}", new Vector2(12, 12), new Color(226, 207, 158));
        _font.Draw(_spriteBatch, $"VEL {(int)_player.Velocity.X},{(int)_player.Velocity.Y}  DASH {_player.DashCharges}", new Vector2(12, 21), new Color(144, 158, 166));
        _font.Draw(_spriteBatch, $"WALL {(int)(_player.WallStamina * 100f / PlayerController.WallStaminaMaximum)}%  GRAPPLE {(_player.GrappleAttached ? "ON" : "OFF")}", new Vector2(12, 30), new Color(144, 158, 166));
        var target = _world.GetTile(_digTool.TargetTile.X, _digTool.TargetTile.Y);
        var targetName = target.Solid ? World.Materials.MaterialCatalog.Get(target.Material).Name : "air";
        _font.Draw(_spriteBatch, $"TOOL {targetName}", new Vector2(12, 39), new Color(169, 124, 94));
        _font.Draw(_spriteBatch, $"HEALTH {_player.Health}/{_player.MaximumHealth}  BOMB {_inventory.Bombs}  ROPE {_inventory.Ropes}",
            new Vector2(12, 48), new Color(205, 111, 92));
        _font.Draw(_spriteBatch, $"SEED {_generated.Configuration.Seed}  {Format(_generated.Configuration.Variant.ToString())}",
            new Vector2(350, 12), GamePalette.UiMuted);
        _font.Draw(_spriteBatch, $"ARCHIVE {_archive.Discoveries.Count}", new Vector2(520, 21), GamePalette.SacredGold);
        if (_relicSystem.DiscoveryVisible && _relicSystem.LastDiscovery != null)
        {
            _spriteBatch.Draw(_pixel, new Rectangle(125, 300, 390, 24), new Color(7, 10, 15, 230));
            _font.Draw(_spriteBatch, _relicSystem.LastDiscovery, new Vector2(141, 309), GamePalette.SacredGold);
        }
        _font.Draw(_spriteBatch, "WASD MOVE  SPACE JUMP  SHIFT DASH  LMB DIG  RMB GRAPPLE", new Vector2(12, 342), new Color(131, 132, 139));
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
}
