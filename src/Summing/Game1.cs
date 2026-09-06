using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Summing;

public class Game1 : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch = null!;
    private Texture2D _pixel = null!;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
        _graphics.SynchronizeWithVerticalRetrace = true;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        IsFixedTimeStep = true;
        TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 60.0);
        Window.Title = "SUMMING — vertical slice";
    }

    protected override void Initialize()
    {
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData([Color.White]);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
        {
            Exit();
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(10, 13, 20));
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _spriteBatch.Draw(_pixel, new Rectangle(0, 540, 1280, 180), new Color(31, 35, 46));
        _spriteBatch.Draw(_pixel, new Rectangle(90, 170, 38, 370), new Color(72, 59, 61));
        _spriteBatch.Draw(_pixel, new Rectangle(116, 258, 210, 282), new Color(46, 44, 52));
        _spriteBatch.Draw(_pixel, new Rectangle(980, 95, 56, 445), new Color(58, 48, 54));
        _spriteBatch.Draw(_pixel, new Rectangle(730, 370, 275, 170), new Color(40, 40, 49));
        _spriteBatch.Draw(_pixel, new Rectangle(620, 500, 18, 40), new Color(213, 177, 92));
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
