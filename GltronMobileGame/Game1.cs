using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using GltronMobileEngine.Sound;

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
            // Use platform-agnostic logging for multiplatform support
            System.Diagnostics.Debug.WriteLine("GLTRON: Game1 constructor start");
            
            // CRITICAL: Initialize GraphicsDeviceManager first - this must succeed
            _graphics = new GraphicsDeviceManager(this);
            if (_graphics == null)
            {
                throw new System.InvalidOperationException("Failed to create GraphicsDeviceManager");
            }
            
            // CRITICAL: Set Content.RootDirectory before any content operations
            Content.RootDirectory = "Content";
            
            // Set up graphics for mobile landscape (multiplatform compatible)
            _graphics.IsFullScreen = true;
            _graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
            
            // Set reasonable default resolution for mobile devices
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            
            // CRITICAL: Create GLTronGame in constructor to avoid null reference issues
            _glTronGame = new GLTronGame();
            if (_glTronGame == null)
            {
                throw new System.InvalidOperationException("Failed to create GLTronGame instance");
            }
            
            // CRITICAL: Don't access GraphicsDevice here - it doesn't exist yet!
            
            System.Diagnostics.Debug.WriteLine("GLTRON: Game1 constructor complete");
        }
        catch (System.Exception ex)
        {
            // Platform-agnostic error logging
            System.Diagnostics.Debug.WriteLine($"GLTRON: Game1 constructor failed: {ex}");
            throw;
        }
    }

    protected override void Initialize()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("GLTRON: Game1 Initialize start");
            
            // CRITICAL: Check graphics manager exists
            if (_graphics == null)
            {
                var error = "GraphicsDeviceManager is null in Initialize!";
                System.Diagnostics.Debug.WriteLine($"GLTRON: ERROR - {error}");
                throw new System.InvalidOperationException(error);
            }
            
            // Apply graphics settings with retry logic for platform stability
            int applyRetries = 3;
            for (int i = 0; i < applyRetries; i++)
            {
                try
                {
                    _graphics.ApplyChanges();
                    break;
                }
                catch (System.Exception ex) when (i < applyRetries - 1)
                {
                    System.Diagnostics.Debug.WriteLine($"GLTRON: ApplyChanges attempt {i + 1} failed: {ex.Message}");
                    System.Threading.Thread.Sleep(100); // Brief delay before retry
                }
            }
            
            // CRITICAL: Check GraphicsDevice exists after ApplyChanges
            if (GraphicsDevice == null)
            {
                var error = "GraphicsDevice is null after ApplyChanges!";
                System.Diagnostics.Debug.WriteLine($"GLTRON: ERROR - {error}");
                throw new System.InvalidOperationException(error);
            }
            
            // Log resolution info (multiplatform compatible)
            var viewport = GraphicsDevice.Viewport;
            System.Diagnostics.Debug.WriteLine($"GLTRON: Screen resolution: {viewport.Width}x{viewport.Height}");
            System.Diagnostics.Debug.WriteLine($"GLTRON: Preferred resolution: {_graphics.PreferredBackBufferWidth}x{_graphics.PreferredBackBufferHeight}");
            System.Diagnostics.Debug.WriteLine($"GLTRON: Aspect ratio: {(float)viewport.Width / viewport.Height}");
            
            // Platform-specific logging if available
            try
            {
#if ANDROID
                Android.Util.Log.Info("GLTRON", $"Screen resolution: {viewport.Width}x{viewport.Height}");
                Android.Util.Log.Info("GLTRON", $"Preferred resolution: {_graphics.PreferredBackBufferWidth}x{_graphics.PreferredBackBufferHeight}");
                Android.Util.Log.Info("GLTRON", $"Aspect ratio: {(float)viewport.Width / viewport.Height}");
#endif
            }
            catch { /* Ignore platform-specific logging errors */ }
            
            // Initialize game with screen size - GLTronGame was created in constructor
            if (_glTronGame == null)
            {
                throw new System.InvalidOperationException("GLTronGame is null in Initialize - constructor failed");
            }

            try
            {
                System.Diagnostics.Debug.WriteLine("GLTRON: Initializing GLTronGame with screen size");
                _glTronGame.updateScreenSize(viewport.Width, viewport.Height);
                _glTronGame.initialiseGame();
                System.Diagnostics.Debug.WriteLine("GLTRON: GLTronGame initialized successfully");
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GLTRON: GLTronGame initialization failed: {ex}");
                throw new System.InvalidOperationException($"GLTronGame initialization failed: {ex.Message}", ex);
            }
            
            // REMOVED: TouchPanel.EnabledGestures = GestureType.Tap;
            // This conflicts with TouchPanel.GetState() on Android
            
            System.Diagnostics.Debug.WriteLine("GLTRON: Game1 Initialize complete");
            
            // Platform-specific logging if available
            try
            {
#if ANDROID
                Android.Util.Log.Info("GLTRON", "Game1 Initialize complete");
#endif
            }
            catch { /* Ignore platform-specific logging errors */ }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GLTRON: Initialize error: {ex}");
            
            try
            {
#if ANDROID
                Android.Util.Log.Error("GLTRON", $"Initialize error: {ex}");
#endif
            }
            catch { /* Ignore platform-specific logging errors */ }
            
            throw; // Re-throw critical initialization errors
        }

        base.Initialize();
    }

    protected override void LoadContent()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("GLTRON: LoadContent start");
            
            // CRITICAL: Check GraphicsDevice exists before using it
            if (GraphicsDevice == null)
            {
                System.Diagnostics.Debug.WriteLine("GLTRON: ERROR - GraphicsDevice is null in LoadContent!");
                throw new System.InvalidOperationException("GraphicsDevice is null in LoadContent!");
            }

            // CRITICAL: Initialize SpriteBatch first
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            System.Diagnostics.Debug.WriteLine("GLTRON: SpriteBatch created");

            // CRITICAL: Create white pixel texture for fallback rendering
            _whitePixel = new Texture2D(GraphicsDevice, 1, 1);
            _whitePixel.SetData(new[] { Color.White });
            System.Diagnostics.Debug.WriteLine("GLTRON: White pixel texture created");

            // Try to load font (non-critical)
            try
            {
                _font = Content.Load<SpriteFont>("Fonts/Default");
                Android.Util.Log.Info("GLTRON", "SpriteFont loaded");
            }
            catch (System.Exception ex)
            {
                try { Android.Util.Log.Error("GLTRON", $"SpriteFont load failed: {ex}"); } catch { }
                _font = null; // Continue without font
            }

            // Initialize HUD if font loaded
            if (_font != null)
            {
                _hud = new GltronMobileEngine.Video.HUD(_spriteBatch, _font);
                _glTronGame.tronHUD = _hud;
                Android.Util.Log.Info("GLTRON", "HUD initialized");
            }
            else
            {
                Android.Util.Log.Warn("GLTRON", "Running without HUD due to font loading failure");
            }

            // Initialize 3D graphics components (non-critical)
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
                // Continue without 3D graphics
            }

            // Initialize sound (non-critical)
            try
            {
                SoundManager.Instance.Initialize(Content);
                SoundManager.Instance.PlayMusic(true, 0.5f);
                Android.Util.Log.Info("GLTRON", "Sound initialized and music started");
            }
            catch (System.Exception ex)
            {
                try { Android.Util.Log.Error("GLTRON", $"Sound init failed: {ex}"); } catch { }
                // Continue without sound
            }

            Android.Util.Log.Info("GLTRON", "LoadContent completed successfully");
        }
        catch (System.Exception ex)
        {
            try { Android.Util.Log.Error("GLTRON", $"LoadContent failed: {ex}"); } catch { }
            throw; // Critical failure
        }
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // CRITICAL: Check GraphicsDevice before using it
        if (GraphicsDevice == null)
        {
            return;
        }

        // Processar entrada de toque
        TouchCollection touchCollection = TouchPanel.GetState();
        foreach (TouchLocation touch in touchCollection)
        {
            if (touch.State == TouchLocationState.Pressed)
            {
                try 
                { 
                    Android.Util.Log.Info("GLTRON", $"Touch detected at: {touch.Position.X}, {touch.Position.Y}"); 
                    _glTronGame?.addTouchEvent(touch.Position.X, touch.Position.Y, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
                } 
                catch (System.Exception ex)
                {
                    try { Android.Util.Log.Error("GLTRON", $"Touch event error: {ex}"); } catch { }
                }
            }
        }

        // Run game logic with null check
        try
        {
            _glTronGame?.RunGame(gameTime);
        }
        catch (System.Exception ex)
        {
            try { Android.Util.Log.Error("GLTRON", $"RunGame error: {ex}"); } catch { }
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        try
        {
            // CRITICAL: Check GraphicsDevice before any rendering
            if (GraphicsDevice == null)
            {
                Android.Util.Log.Error("GLTRON", "GraphicsDevice is null in Draw!");
                return;
            }

            // CRITICAL: Check SpriteBatch before using it
            if (_spriteBatch == null)
            {
                Android.Util.Log.Error("GLTRON", "SpriteBatch is null in Draw!");
                return;
            }

            var viewport = GraphicsDevice.Viewport;
            bool isInMenu = _glTronGame?.IsShowingMenu() == true;

            // STEP 1: DRAW 3D FIRST (correct rendering order)
            if (!isInMenu && _worldGraphics != null && _camera != null)
            {
                try
                {
                    // Reset pipeline state for 3D rendering
                    GraphicsDevice.BlendState = BlendState.Opaque;
                    GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                    GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

                    // Clear includes depth buffer (CRITICAL for Android)
                    GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1f, 0);

                    // Safe camera update
                    var player = _glTronGame?.GetOwnPlayer();
                    Vector3 playerPos = Vector3.Zero;
                    if (player != null)
                    {
                        try
                        {
                            playerPos = new Vector3(player.getXpos(), 0, player.getYpos());
                        }
                        catch
                        {
                            playerPos = Vector3.Zero; // Fallback if player coordinates not ready
                        }
                    }

                    _camera.Update(playerPos, gameTime);

                    // Begin 3D rendering
                    _worldGraphics.BeginDraw(_camera.View, _camera.Projection);

                    // Draw floor
                    _worldGraphics.DrawFloor();

                    // Draw walls
                    var walls = _glTronGame?.GetWalls();
                    if (walls != null)
                    {
                        _worldGraphics.DrawWalls(walls);
                    }

                    // Draw player trails
                    var players = _glTronGame?.GetPlayers();
                    if (players != null && _trailsRenderer != null)
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
                    // Fallback: clear screen if 3D fails
                    GraphicsDevice.Clear(Color.Black);
                }
            }
            else
            {
                // Clear screen for menu or when 3D not available
                GraphicsDevice.Clear(Color.Black);
            }

            // STEP 2: DRAW 2D UI/HUD ON TOP
            try
            {
                _spriteBatch.Begin();
                
                if (_font != null)
                {
                    // Debug info
                    _spriteBatch.DrawString(_font, $"Resolution: {viewport.Width}x{viewport.Height}", new Vector2(10, 10), Color.White);
                    _spriteBatch.DrawString(_font, $"Orientation: {(viewport.Width > viewport.Height ? "Landscape" : "Portrait")}", new Vector2(10, 30), Color.White);
                    
                    if (isInMenu)
                    {
                        // Menu UI
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
                        
                        // Draw HUD with real score if available
                        int score = 0;
                        try { score = _glTronGame?.GetOwnPlayerScore() ?? 0; } catch { }
                        _hud?.Draw(gameTime, score);
                    }
                }
                else
                {
                    // Fallback rendering when font fails
                    _whitePixel ??= new Texture2D(GraphicsDevice, 1, 1);
                    if (_whitePixel.IsDisposed || _whitePixel.GraphicsDevice == null)
                    {
                        _whitePixel = new Texture2D(GraphicsDevice, 1, 1);
                        _whitePixel.SetData(new[] { Color.White });
                    }
                    
                    if (isInMenu)
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
                
                _spriteBatch.End();
            }
            catch (System.Exception ex)
            {
                try { Android.Util.Log.Error("GLTRON", $"2D UI rendering error: {ex}"); } catch { }
            }

            // STEP 3: Run game logic rendering (win/lose logic)
            try
            {
                _glTronGame?.RenderGame(GraphicsDevice);
            }
            catch (System.Exception ex)
            {
                try { Android.Util.Log.Error("GLTRON", $"Game logic rendering error: {ex}"); } catch { }
            }
        }
        catch (System.Exception ex)
        {
            try { Android.Util.Log.Error("GLTRON", $"Draw method error: {ex}"); } catch { }
        }

        base.Draw(gameTime);
    }
}
