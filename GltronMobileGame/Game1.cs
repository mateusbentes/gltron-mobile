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
    private Texture2D _whitePixel;

    public Game1()
    {
        try
        {
            Android.Util.Log.Info("GLTRON", "Game1 constructor start");
            
            _graphics = new GraphicsDeviceManager(this);
            _glTronGame = new GLTronGame();
            Content.RootDirectory = "Content";
            
            // Set up graphics for mobile landscape
            _graphics.IsFullScreen = true;
            _graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
            
            Android.Util.Log.Info("GLTRON", "Game1 constructor complete");
        }
        catch (System.Exception ex)
        {
            try { Android.Util.Log.Error("GLTRON", $"Game1 constructor failed: {ex}"); } catch { }
            throw;
        }
    }

    protected override void Initialize()
    {
        try
        {
            Android.Util.Log.Info("GLTRON", "Game1 Initialize start");
            
            // Apply graphics settings
            _graphics.ApplyChanges();
            
            // Log resolution info
            var viewport = GraphicsDevice.Viewport;
            Android.Util.Log.Info("GLTRON", $"Screen resolution: {viewport.Width}x{viewport.Height}");
            Android.Util.Log.Info("GLTRON", $"Aspect ratio: {(float)viewport.Width / viewport.Height}");
            
            // Initialize game with screen size
            _glTronGame.updateScreenSize(viewport.Width, viewport.Height);
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
            // Create a fallback - we'll draw without text for now
            _font = null;
        }

        if (_font != null)
        {
            _hud = new GltronMobileEngine.Video.HUD(_spriteBatch, _font);
            _glTronGame.tronHUD = _hud;
        }
        else
        {
            Android.Util.Log.Warn("GLTRON", "Running without HUD due to font loading failure");
        }

        // Initialize 3D graphics components
        try
        {
            Android.Util.Log.Info("GLTRON", "Initializing 3D graphics components");
            _worldGraphics = new GltronMobileEngine.Video.WorldGraphics(GraphicsDevice, Content);
            _worldGraphics.LoadContent(Content);
            _trailsRenderer = new GltronMobileEngine.Video.TrailsRenderer(GraphicsDevice);
            _trailsRenderer.LoadContent(Content);
            _camera = new GltronMobileEngine.Video.Camera(GraphicsDevice.Viewport);
            Android.Util.Log.Info("GLTRON", "3D graphics components initialized successfully");
        }
        catch (System.Exception ex)
        {
            try { Android.Util.Log.Error("GLTRON", $"3D graphics initialization failed: {ex}"); } catch { }
        }

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
                try 
                { 
                    Android.Util.Log.Info("GLTRON", $"Touch detected at: {touch.Position.X}, {touch.Position.Y}"); 
                    _glTronGame.addTouchEvent(touch.Position.X, touch.Position.Y, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
                } 
                catch { }
            }
        }

        // O TouchPanel.GetState() já está sendo usado para processar a entrada de toque.

        _glTronGame.RunGame(gameTime);

        // TODO: Add your update logic here

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        try
        {
            // Clear with dark background
            GraphicsDevice.Clear(Color.Black);

            // Always draw debug info
            _spriteBatch.Begin();
            
            try
            {
                var viewport = GraphicsDevice.Viewport;
                
                if (_font != null)
                {
                    _spriteBatch.DrawString(_font, $"Resolution: {viewport.Width}x{viewport.Height}", new Vector2(10, 10), Color.White);
                    _spriteBatch.DrawString(_font, $"Orientation: {(viewport.Width > viewport.Height ? "Landscape" : "Portrait")}", new Vector2(10, 30), Color.White);
                    
                    // Check if we're in menu state
                    if (_glTronGame.IsShowingMenu())
                    {
                        var centerX = viewport.Width / 2;
                        var centerY = viewport.Height / 2;
                        
                        var titleText = "GLTron Mobile";
                        var titleSize = _font.MeasureString(titleText);
                        _spriteBatch.DrawString(_font, titleText, new Vector2(centerX - titleSize.X/2, centerY - 100), Color.Cyan);
                        
                        var startText = "Tap anywhere to start";
                        var startSize = _font.MeasureString(startText);
                        _spriteBatch.DrawString(_font, startText, new Vector2(centerX - startSize.X/2, centerY - 50), Color.Yellow);
                        
                        var instructText = "Tap left/right to turn";
                        var instructSize = _font.MeasureString(instructText);
                        _spriteBatch.DrawString(_font, instructText, new Vector2(centerX - instructSize.X/2, centerY), Color.White);
                        
                        _spriteBatch.DrawString(_font, "Menu State: Active", new Vector2(10, 50), Color.Green);
                    }
                    else
                    {
                        _spriteBatch.DrawString(_font, "Game State: Running", new Vector2(10, 50), Color.Green);
                    }
                }
                else
                {
                    // Draw simple colored rectangles as fallback when font fails
                    // Create a 1x1 white texture for drawing rectangles
                    if (_whitePixel == null)
                    {
                        _whitePixel = new Texture2D(GraphicsDevice, 1, 1);
                        _whitePixel.SetData(new[] { Color.White });
                    }
                    
                    if (_glTronGame.IsShowingMenu())
                    {
                        // Draw menu indicator - cyan rectangle in center
                        _spriteBatch.Draw(_whitePixel, new Rectangle(viewport.Width/2 - 100, viewport.Height/2 - 50, 200, 100), Color.Cyan);
                    }
                    else
                    {
                        // Draw game indicator - green rectangle in corner
                        _spriteBatch.Draw(_whitePixel, new Rectangle(10, 10, 100, 50), Color.Green);
                    }
                }
            }
            catch (System.Exception ex)
            {
                try { Android.Util.Log.Error("GLTRON", $"SpriteBatch drawing error: {ex}"); } catch { }
            }
            
            _spriteBatch.End();

            // Draw 3D world only when not in menu and 3D components are available
            if (!_glTronGame.IsShowingMenu() && _worldGraphics != null && _camera != null && _trailsRenderer != null)
            {
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

                    // Draw HUD with real score if available
                    int score = 0;
                    try { score = _glTronGame.GetOwnPlayerScore(); } catch { }
                    _hud?.Draw(gameTime, score);
                }
                catch (System.Exception ex)
                {
                    try { Android.Util.Log.Error("GLTRON", $"3D rendering error: {ex}"); } catch { }
                }
            }

            // Run game logic rendering (win/lose logic)
            _glTronGame.RenderGame(GraphicsDevice);
        }
        catch (System.Exception ex)
        {
            try { Android.Util.Log.Error("GLTRON", $"Draw method error: {ex}"); } catch { }
        }

        base.Draw(gameTime);
    }
}
