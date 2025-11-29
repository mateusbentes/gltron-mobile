using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace GltronMobileEngine;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private GLTronGame _glTronGame;
    private SpriteBatch _spriteBatch;
    private SpriteFont _font;
    private GltronMobileEngine.Video.HUD _hud;

    public Game1()
    {
        _glTronGame = new GLTronGame();
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here
        _glTronGame.initialiseGame();
        TouchPanel.EnabledGestures = GestureType.Tap;

        base.Initialize();
    }

    protected override void LoadContent()
    {
        try
        {
            Android.Util.Log.Info("GLTRON", "LoadContent start");
        }
        catch { }

        _spriteBatch = new SpriteBatch(GraphicsDevice);
        try
        {
            _font = Content.Load<SpriteFont>("Fonts/Default");
            Android.Util.Log.Info("GLTRON", "SpriteFont loaded");
        }
        catch (System.Exception ex)
        {
            try { Android.Util.Log.Error("GLTRON", $"SpriteFont load failed: {ex}"); } catch { }
            throw;
        }

        _hud = new GltronMobileEngine.Video.HUD(_spriteBatch, _font);
        _glTronGame.tronHUD = _hud;

        // Initialize sound and start music
        try
        {
            GltronMobileEngine.Sound.SoundManager.Instance.Initialize(Content);
            GltronMobileEngine.Sound.SoundManager.Instance.PlayMusic(true, 0.5f);
            Android.Util.Log.Info("GLTRON", "Sound initialized and music started");
        }
        catch (System.Exception ex)
        {
            try { Android.Util.Log.Error("GLTRON", $"Sound init failed: {ex}"); } catch { }
        }
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // Processar entrada de toque
        TouchCollection touchCollection = TouchPanel.GetState();
        foreach (TouchLocation touch in touchCollection)
        {
            if (touch.State == TouchLocationState.Pressed)
            {
                _glTronGame.addTouchEvent(touch.Position.X, touch.Position.Y, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            }
        }

        // O TouchPanel.GetState() já está sendo usado para processar a entrada de toque.

        _glTronGame.RunGame(gameTime);

        // TODO: Add your update logic here

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        // Ensure a visible background
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _glTronGame.RenderGame(GraphicsDevice);

        // Draw HUD with real score if available
        int score = 0;
        try { score = _glTronGame.GetOwnPlayerScore(); } catch { }
        _hud?.Draw(gameTime, score);

        try { Android.Util.Log.Debug("GLTRON", "Draw tick"); } catch { }

        base.Draw(gameTime);
    }
}
