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
                System.Diagnostics.Debug.WriteLine("GLTRON: Initializing 3D graphics components");
                _worldGraphics = new GltronMobileEngine.Video.WorldGraphics(GraphicsDevice, Content);
                _worldGraphics.LoadContent(Content);
                _trailsRenderer = new GltronMobileEngine.Video.TrailsRenderer(GraphicsDevice);
                _trailsRenderer.LoadContent(Content);
                _camera = new GltronMobileEngine.Video.Camera(GraphicsDevice.Viewport);
                System.Diagnostics.Debug.WriteLine("GLTRON: 3D graphics components initialized successfully");
                
                // Platform-specific logging if available
                try
                {
#if ANDROID
                    Android.Util.Log.Info("GLTRON", "3D graphics components initialized successfully");
#endif
                }
                catch { /* Ignore platform-specific logging errors */ }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GLTRON: 3D graphics initialization failed: {ex}");
                try
                {
#if ANDROID
                    Android.Util.Log.Error("GLTRON", $"3D graphics initialization failed: {ex}");
#endif
                }
                catch { /* Ignore platform-specific logging errors */ }
                // Continue without 3D graphics
            }

            // Initialize sound (non-critical)
            try
            {
                System.Diagnostics.Debug.WriteLine("GLTRON: Attempting to initialize sound system...");
                SoundManager.Instance.Initialize(Content);
                System.Diagnostics.Debug.WriteLine("GLTRON: Sound system initialized, starting music...");
                SoundManager.Instance.PlayMusic(true, 0.5f);
                System.Diagnostics.Debug.WriteLine("GLTRON: Sound initialized and music started successfully");
                
                // Platform-specific logging if available
                try
                {
#if ANDROID
                    Android.Util.Log.Info("GLTRON", "Sound initialized and music started");
#endif
                }
                catch { /* Ignore platform-specific logging errors */ }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GLTRON: Sound initialization failed: {ex.Message}");
                System.Diagnostics.Debug.WriteLine("GLTRON: Continuing without sound - game will still work");
                
                try
                {
#if ANDROID
                    Android.Util.Log.Warn("GLTRON", $"Sound init failed: {ex.Message}");
                    Android.Util.Log.Info("GLTRON", "Continuing without sound - game will still work");
#endif
                }
                catch { /* Ignore platform-specific logging errors */ }
                // Continue without sound - this is non-critical
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

            // Use Android logging for better visibility
            try
            {
#if ANDROID
                Android.Util.Log.Info("GLTRON", $"Draw - isInMenu: {isInMenu}, _worldGraphics: {_worldGraphics != null}, _camera: {_camera != null}");
#endif
            }
            catch { }

            // STEP 1: DRAW 3D FIRST (correct rendering order)
            if (!isInMenu && _worldGraphics != null && _camera != null)
            {
                try
                {
#if ANDROID
                    Android.Util.Log.Info("GLTRON", "Entering 3D rendering section");
#endif
                }
                catch { }
            }
            else
            {
                try
                {
#if ANDROID
                    Android.Util.Log.Info("GLTRON", $"Skipping 3D rendering - isInMenu: {isInMenu}, _worldGraphics: {_worldGraphics != null}, _camera: {_camera != null}");
#endif
                }
                catch { }
            }
            
            if (!isInMenu && _worldGraphics != null && _camera != null)
            {
                try
                {
                    try
                    {
#if ANDROID
                        Android.Util.Log.Info("GLTRON", "Setting up 3D rendering pipeline");
#endif
                    }
                    catch { }
                    
                    // Reset pipeline state for 3D rendering
                    GraphicsDevice.BlendState = BlendState.Opaque;
                    GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                    GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

                    // Try simple clear first (Android compatibility)
                    try
                    {
                        GraphicsDevice.Clear(Color.Red);
                        try
                        {
#if ANDROID
                            Android.Util.Log.Info("GLTRON", "Screen cleared with dark blue - simple clear worked");
#endif
                        }
                        catch { }
                    }
                    catch (System.Exception ex)
                    {
                        try
                        {
#if ANDROID
                            Android.Util.Log.Error("GLTRON", $"Simple clear failed: {ex.Message}");
#endif
                        }
                        catch { }
                        
                        // Fallback: try basic clear
                        try
                        {
                            GraphicsDevice.Clear(Color.Red);
                            try
                            {
#if ANDROID
                                Android.Util.Log.Info("GLTRON", "Fallback red clear worked");
#endif
                            }
                            catch { }
                        }
                        catch (System.Exception ex2)
                        {
                            try
                            {
#if ANDROID
                                Android.Util.Log.Error("GLTRON", $"All clear methods failed: {ex2.Message}");
#endif
                            }
                            catch { }
                        }
                    }

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

                    try
                    {
                        _camera.Update(playerPos, gameTime);
                        try
                        {
#if ANDROID
                            Android.Util.Log.Info("GLTRON", $"Camera updated successfully - playerPos: {playerPos}");
#endif
                        }
                        catch { }
                    }
                    catch (System.Exception ex)
                    {
                        try
                        {
#if ANDROID
                            Android.Util.Log.Error("GLTRON", $"Camera update failed: {ex.Message}");
#endif
                        }
                        catch { }
                        return; // Skip 3D rendering if camera fails
                    }

                    // Begin 3D rendering
                    try
                    {
                        _worldGraphics.BeginDraw(_camera.View, _camera.Projection);
                        try
                        {
#if ANDROID
                            Android.Util.Log.Info("GLTRON", "WorldGraphics.BeginDraw called successfully");
#endif
                        }
                        catch { }
                    }
                    catch (System.Exception ex)
                    {
                        try
                        {
#if ANDROID
                            Android.Util.Log.Error("GLTRON", $"WorldGraphics.BeginDraw FAILED: {ex.Message}");
#endif
                        }
                        catch { }
                        return; // Skip 3D rendering if BeginDraw fails
                    }

                    // SKIP ALL 3D DRAWING TO TEST CLEAR COLOR VISIBILITY
                    try
                    {
#if ANDROID
                        Android.Util.Log.Info("GLTRON", "SKIPPING ALL 3D DRAWING - TESTING RED CLEAR COLOR");
#endif
                    }
                    catch { }
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
                // Clear screen for menu - use black like original
                GraphicsDevice.Clear(Color.Black);
                try
                {
#if ANDROID
                    Android.Util.Log.Info("GLTRON", "Menu mode - cleared with black background");
#endif
                }
                catch { }
            }

            // STEP 2: TEST 2D UI ONLY (NO 3D)
            try
            {
                try
                {
#if ANDROID
                    Android.Util.Log.Info("GLTRON", "TESTING 2D UI ONLY - NO 3D DRAWING");
#endif
                }
                catch { }
                
                // Use proper blend mode for 2D over clear color
                _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);
                
                // PROPER MENU SYSTEM LIKE ORIGINAL JAVA VERSION
                if (_font != null)
                {
                    if (isInMenu)
                    {
                        // MENU: Show instructions like original Java version
                        var centerX = viewport.Width / 2;
                        var centerY = viewport.Height / 2;
                        
                        // Main instruction - "Tap screen to Start"
                        var startText = "Tap screen to Start";
                        var startSize = _font.MeasureString(startText);
                        _spriteBatch.DrawString(_font, startText, 
                            new Vector2(centerX - startSize.X/2, centerY - 50), Color.White);
                        
                        // Secondary instruction
                        var instructText = "Tap left/right to turn";
                        var instructSize = _font.MeasureString(instructText);
                        _spriteBatch.DrawString(_font, instructText, 
                            new Vector2(centerX - instructSize.X/2, centerY + 20), Color.White);
                        
                        // Title
                        var titleText = "GL TRON";
                        var titleSize = _font.MeasureString(titleText);
                        _spriteBatch.DrawString(_font, titleText, 
                            new Vector2(centerX - titleSize.X/2, centerY - 150), Color.Cyan);
                        
                        try
                        {
#if ANDROID
                            Android.Util.Log.Info("GLTRON", "Drawing menu instructions on black background");
#endif
                        }
                        catch { }
                    }
                    else
                    {
                        // GAME: Show HUD elements like original
                        // Score
                        int score = 0;
                        try { score = _glTronGame?.GetOwnPlayerScore() ?? 0; } catch { }
                        _spriteBatch.DrawString(_font, $"Score: {score}", new Vector2(10, 10), Color.Yellow);
                        
                        // Game status
                        _spriteBatch.DrawString(_font, "Game Running", new Vector2(10, 40), Color.White);
                        
                        try
                        {
#if ANDROID
                            Android.Util.Log.Info("GLTRON", "Drawing game HUD on red background");
#endif
                        }
                        catch { }
                    }
                }
                else
                {
                    // Fallback when no font available
                    _whitePixel ??= new Texture2D(GraphicsDevice, 1, 1);
                    if (_whitePixel.IsDisposed || _whitePixel.GraphicsDevice == null)
                    {
                        _whitePixel = new Texture2D(GraphicsDevice, 1, 1);
                        _whitePixel.SetData(new[] { Color.White });
                    }
                    
                    if (isInMenu)
                    {
                        // Menu indicator - white rectangle in center
                        _spriteBatch.Draw(_whitePixel, new Rectangle(viewport.Width/2 - 100, viewport.Height/2 - 25, 200, 50), Color.White);
                    }
                    else
                    {
                        // Game indicator - yellow rectangle in corner
                        _spriteBatch.Draw(_whitePixel, new Rectangle(10, 10, 100, 30), Color.Yellow);
                    }
                }
                
                _spriteBatch.End();
            }
            catch (System.Exception ex)
            {
                try { Android.Util.Log.Error("GLTRON", $"2D UI rendering error: {ex}"); } catch { }
            }

            // STEP 3: SKIP GAME LOGIC RENDERING FOR NOW
            try
            {
#if ANDROID
                Android.Util.Log.Info("GLTRON", "SKIPPING game logic rendering to test UI only");
#endif
            }
            catch { }
            
            // SKIP: _glTronGame?.RenderGame(GraphicsDevice);
        }
        catch (System.Exception ex)
        {
            try { Android.Util.Log.Error("GLTRON", $"Draw method error: {ex}"); } catch { }
        }

        try
        {
#if ANDROID
            Android.Util.Log.Info("GLTRON", "About to call base.Draw(gameTime)");
#endif
        }
        catch { }

        base.Draw(gameTime);
        
        try
        {
#if ANDROID
            Android.Util.Log.Info("GLTRON", "Completed base.Draw(gameTime)");
#endif
        }
        catch { }
    }
}
