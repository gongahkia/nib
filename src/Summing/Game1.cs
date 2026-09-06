using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Summing.Camera;
using Summing.Core;
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
    private InputManager _input = null!;
    private readonly TileWorld _world = TileWorld.CreateMovementTest();
    private TileWorldRenderer _tileRenderer = null!;
    private readonly Camera2D _camera = new();
    private PlayerController _player = null!;
    private readonly DigTool _digTool = new();

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
        _player = new PlayerController(new Vector2(7f * GameConstants.TileSize, 15f * GameConstants.TileSize));
        _camera.Snap(new Vector2(320f, 348f));
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
    }

    protected override void Update(GameTime gameTime)
    {
        var playerScreen = _camera.WorldToScreen(_player.Bounds.Center);
        _input.Update(playerScreen);
        if (_input.Pressed(InputAction.Pause)) Exit();
        _player.Update(_input, _world, GameConstants.FixedDelta);
        _digTool.Update(_input, _player, _world, GameConstants.FixedDelta);
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
        _tileRenderer.Draw(_spriteBatch, _world, _camera.Position);
        _player.Draw(_spriteBatch, _pixel);
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
        _spriteBatch.Draw(_pixel, new Rectangle(-100, 0, 1400, 400), new Color(13, 17, 27));
        for (var x = -80; x < 1300; x += 32)
        {
            var height = 30 + Math.Abs((x * 17) % 90);
            _spriteBatch.Draw(_pixel, new Rectangle(x, 320 - height, 25, height), new Color(25, 28, 38));
        }
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
