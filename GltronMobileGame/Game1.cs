using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace GltronMobileGame;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private GLTronGame _glTronGame;
    private SpriteBatch _spriteBatch;
    private SpriteFont _font;
    private GltronMobileEngine.Video.HUD _hud;
    private GltronMobileEngine.Video.WorldGraphics _worldGraphics;
    private GltronMobileEngine.Video.TrailsRenderer _trailsRenderer;
    private GltronMobileEngine.Video.Camera _camera;

    public Game1()
    {
        _glTronGame = new GLTronGame();
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        try
        {
            Android.Util.Log.Info("GLTRON", "Game1 Initialize start");
            _glTronGame.initialiseGame();
            TouchPanel.EnabledGestures = GestureType.Tap;
            Android.Util.Log.Info("GLTRON", "Game1 Initialize complete");
        }
        catch (System.Exception ex)
        {
            try { Android.Util.Log.Error("GLTRON", $"Initialize error: {ex}"); } catch { }
        }

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

        // Initialize 3D graphics components
        _worldGraphics = new GltronMobileEngine.Video.WorldGraphics(GraphicsDevice, Content);
        _worldGraphics.LoadContent(Content);
        
        _trailsRenderer = new GltronMobileEngine.Video.TrailsRenderer(GraphicsDevice);
        _trailsRenderer.LoadContent(Content);
        
        _camera = new GltronMobileEngine.Video.Camera(GraphicsDevice.Viewport);

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
        // Clear with dark background
        GraphicsDevice.Clear(Color.Black);

        // Check if we're in menu state
        if (_glTronGame.IsShowingMenu())
        {
            // Draw simple menu
            _spriteBatch.Begin();
            var centerX = GraphicsDevice.Viewport.Width / 2;
            var centerY = GraphicsDevice.Viewport.Height / 2;
            
            var titleText = "GLTron Mobile";
            var titleSize = _font.MeasureString(titleText);
            _spriteBatch.DrawString(_font, titleText, new Vector2(centerX - titleSize.X/2, centerY - 100), Color.Cyan);
            
            var startText = "Tap anywhere to start";
            var startSize = _font.MeasureString(startText);
            _spriteBatch.DrawString(_font, startText, new Vector2(centerX - startSize.X/2, centerY - 50), Color.Yellow);
            
            var instructText = "Tap left/right to turn";
            var instructSize = _font.MeasureString(instructText);
            _spriteBatch.DrawString(_font, instructText, new Vector2(centerX - instructSize.X/2, centerY), Color.White);
            
            _spriteBatch.End();
        }
        else
        {
            // Game is running - draw 3D world
            try
            {
                // Update camera to follow player
                var playerPos = Vector3.Zero;
                if (_glTronGame.GetOwnPlayer() != null)
                {
                    var player = _glTronGame.GetOwnPlayer();
                    playerPos = new Vector3(player.getXpos(), 0, player.getYpos());
                }
                _camera.Update(playerPos, gameTime);

                // Begin 3D rendering
                _worldGraphics.BeginDraw(_camera.View, _camera.Projection);

                // Draw floor
                _worldGraphics.DrawFloor();

                // Draw walls
                var walls = _glTronGame.GetWalls();
                if (walls != null)
                {
                    _worldGraphics.DrawWalls(walls);
                }

                // Draw player trails
                var players = _glTronGame.GetPlayers();
                if (players != null)
                {
                    for (int i = 0; i < players.Length; i++)
                    {
                        if (players[i] != null)
                        {
                            _trailsRenderer.DrawTrail(_worldGraphics, players[i]);
                        }
                    }
                }

                // End 3D rendering
                _worldGraphics.EndDraw();
            }
            catch (System.Exception ex)
            {
                try { Android.Util.Log.Error("GLTRON", $"3D rendering error: {ex}"); } catch { }
            }

            // Draw HUD with real score if available
            int score = 0;
            try { score = _glTronGame.GetOwnPlayerScore(); } catch { }
            _hud?.Draw(gameTime, score);
        }

        // Run game logic rendering (win/lose logic)
        _glTronGame.RenderGame(GraphicsDevice);

        base.Draw(gameTime);
    }
}
