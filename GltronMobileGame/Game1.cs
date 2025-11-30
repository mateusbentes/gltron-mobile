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
    private Texture2D _menuBackground;
    
    // CRITICAL FIX: Swipe detection system
    private Vector2? _swipeStartPosition = null;
    private double _swipeStartTime = 0;
    private bool _swipeInProgress = false;
    private const float MIN_SWIPE_DISTANCE = 50f; // Minimum distance for a swipe
    private const double MAX_SWIPE_TIME = 1000; // Maximum time for a swipe (milliseconds)

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

            // Load menu background image (like original Java version)
            try
            {
                _menuBackground = Content.Load<Texture2D>("Assets/gltron_bitmap");
                System.Diagnostics.Debug.WriteLine("GLTRON: Menu background loaded successfully");
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GLTRON: Menu background load failed: {ex.Message}");
                _menuBackground = null;
            }

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
                
                // Set the grid size to match the game
                if (_glTronGame != null)
                {
                    _worldGraphics.SetGridSize(_glTronGame.GetGridSize());
                }
                _trailsRenderer = new GltronMobileEngine.Video.TrailsRenderer(GraphicsDevice);
                _trailsRenderer.LoadContent(Content);
                _camera = new GltronMobileEngine.Video.Camera(GraphicsDevice.Viewport);
                
                // CRITICAL FIX: Set proper camera projection for grid size
                if (_glTronGame != null)
                {
                    var viewport = GraphicsDevice.Viewport;
                    _camera.SetProjection(_glTronGame.GetGridSize(), viewport.AspectRatio);
                }
                
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

        // CRITICAL FIX: Process swipe gestures instead of simple taps
        ProcessSwipeInput(gameTime);

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
                        GraphicsDevice.Clear(Color.Black);
                        try
                        {
#if ANDROID
                            Android.Util.Log.Info("GLTRON", "Screen cleared with black for 3D game");
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

                    // Safe camera update - get real player position
                    var player = _glTronGame?.GetOwnPlayer();
                    Vector3 playerPos = Vector3.Zero;
                    if (player != null)
                    {
                        try
                        {
                            float x = player.getXpos();
                            float y = player.getYpos();
                            playerPos = new Vector3(x, 0, y); // X, Y=0, Z for GLTron coordinate system
                            
                            try
                            {
#if ANDROID
                                Android.Util.Log.Debug("GLTRON", $"Camera following player at: X={x:F2}, Y={y:F2}");
#endif
                            }
                            catch { }
                        }
                        catch (System.Exception ex)
                        {
                            playerPos = new Vector3(50f, 0f, 50f); // Default to arena center
                        }
                    }
                    else
                    {
                        playerPos = new Vector3(50f, 0f, 50f); // Default to arena center
                    }

                    try
                    {
                        // CRITICAL FIX: Use turn-interpolated camera like Java version
                        if (player != null)
                        {
                            try
                            {
                                int playerDirection = player.getDirection();
                                int lastDirection = player.getLastDirection();
                                long currentTime = (long)gameTime.TotalGameTime.TotalMilliseconds;
                                
                                // Get turn time from player (cast to concrete type)
                                long turnTime = 0;
                                if (player is GltronMobileEngine.Player concretePlayer)
                                {
                                    turnTime = concretePlayer.TurnTime;
                                }
                                
                                // Use smooth turn interpolation like Java version
                                _camera.UpdateWithTurn(playerPos, playerDirection, lastDirection, 
                                                     currentTime, turnTime, GltronMobileEngine.Player.TURN_LENGTH);
                                
                                try
                                {
#if ANDROID
                                    Android.Util.Log.Debug("GLTRON", $"Camera updated with turn interpolation - pos: {playerPos}, dir: {playerDirection}, lastDir: {lastDirection}");
#endif
                                }
                                catch { }
                            }
                            catch (System.Exception ex)
                            {
                                // Fallback to simple camera update
                                _camera.UpdateWithPlayerDirection(playerPos, player.getDirection(), gameTime);
                            }
                        }
                        else
                        {
                            // No player - use simple update
                            _camera.Update(playerPos, gameTime);
                        }
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
                        try
                        {
#if ANDROID
                            Android.Util.Log.Info("GLTRON", $"Camera View matrix: {_camera.View}");
                            Android.Util.Log.Info("GLTRON", $"Camera Projection matrix: {_camera.Projection}");
#endif
                        }
                        catch { }
                        
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

                    // CRITICAL FIX: Draw skybox first (like Java version)
                    try
                    {
                        // Disable depth testing for skybox (like Java version)
                        GraphicsDevice.DepthStencilState = DepthStencilState.None;
                        GraphicsDevice.RasterizerState = RasterizerState.CullNone;
                        _worldGraphics.DrawSkybox();
                        try
                        {
#if ANDROID
                            Android.Util.Log.Info("GLTRON", "Skybox drawn successfully");
#endif
                        }
                        catch { }
                    }
                    catch (System.Exception ex)
                    {
                        try
                        {
#if ANDROID
                            Android.Util.Log.Error("GLTRON", $"DrawSkybox FAILED: {ex.Message}");
#endif
                        }
                        catch { }
                    }

                    // Re-enable depth testing for world geometry
                    GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                    GraphicsDevice.RasterizerState = RasterizerState.CullNone; // Fix culling issues

                    // Draw floor
                    try
                    {
                        _worldGraphics.DrawFloor();
                        try
                        {
#if ANDROID
                            Android.Util.Log.Info("GLTRON", "Floor drawn successfully");
#endif
                        }
                        catch { }
                    }
                    catch (System.Exception ex)
                    {
                        try
                        {
#if ANDROID
                            Android.Util.Log.Error("GLTRON", $"DrawFloor FAILED: {ex.Message}");
#endif
                        }
                        catch { }
                    }

                    // Draw walls
                    var walls = _glTronGame?.GetWalls();
                    if (walls != null)
                    {
                        try
                        {
                            // Debug wall positions and grid size
                            try
                            {
#if ANDROID
                                float gridSize = _glTronGame?.GetGridSize() ?? 100f;
                                Android.Util.Log.Info("GLTRON", $"Arena grid size: {gridSize}");
                                
                                for (int i = 0; i < walls.Length && i < 4; i++)
                                {
                                    if (walls[i] != null)
                                    {
                                        Android.Util.Log.Info("GLTRON", $"Wall {i}: start=({walls[i].vStart.v[0]:F1},{walls[i].vStart.v[1]:F1}) dir=({walls[i].vDirection.v[0]:F1},{walls[i].vDirection.v[1]:F1})");
                                    }
                                }
#endif
                            }
                            catch { }
                            
                            _worldGraphics.DrawWalls(walls);
                            try
                            {
#if ANDROID
                                Android.Util.Log.Info("GLTRON", "Walls drawn successfully");
#endif
                            }
                            catch { }
                        }
                        catch (System.Exception ex)
                        {
                            try
                            {
#if ANDROID
                                Android.Util.Log.Error("GLTRON", $"DrawWalls FAILED: {ex.Message}");
#endif
                            }
                            catch { }
                        }
                    }
                    else
                    {
                        try
                        {
#if ANDROID
                            Android.Util.Log.Warn("GLTRON", "Walls array is null!");
#endif
                        }
                        catch { }
                    }

                    // Draw player trails and bikes for all 4 players
                    var players = _glTronGame?.GetPlayers();
                    if (players != null && _trailsRenderer != null)
                    {
                        for (int i = 0; i < players.Length && i < 4; i++) // Ensure we draw all 4 players
                        {
                            if (players[i] != null)
                            {
                                try
                                {
                                    // Draw trail first (behind bike)
                                    _trailsRenderer.DrawTrail(_worldGraphics, players[i]);
                                    
                                    // Draw bike (like Java version)
                                    if (players[i].getSpeed() > 0.0f || players[i].getExplode())
                                    {
                                        float x = players[i].getXpos();
                                        float y = players[i].getYpos();
                                        
                                        try
                                        {
#if ANDROID
                                            Android.Util.Log.Debug("GLTRON", $"Drawing player {i} bike at ({x:F1},{y:F1}) speed={players[i].getSpeed():F1}");
#endif
                                        }
                                        catch { }
                                        
                                        if (players[i].getExplode())
                                        {
                                            _worldGraphics.DrawExplosion(_worldGraphics.Effect, players[i]);
                                        }
                                        else
                                        {
                                            _worldGraphics.DrawBike(_worldGraphics.Effect, players[i]);
                                        }
                                    }
                                }
                                catch (System.Exception ex)
                                {
                                    try
                                    {
#if ANDROID
                                        Android.Util.Log.Error("GLTRON", $"Draw player {i} FAILED: {ex.Message}");
#endif
                                    }
                                    catch { }
                                }
                            }
                        }
                    }

                    // End 3D rendering
                    try
                    {
                        _worldGraphics.EndDraw();
                        try
                        {
#if ANDROID
                            Android.Util.Log.Info("GLTRON", "3D rendering completed successfully");
#endif
                        }
                        catch { }
                    }
                    catch (System.Exception ex)
                    {
                        try
                        {
#if ANDROID
                            Android.Util.Log.Error("GLTRON", $"EndDraw FAILED: {ex.Message}");
#endif
                        }
                        catch { }
                    }
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

            // STEP 2: Draw 2D UI overlay
            try
            {
                
                // Use proper blend mode for 2D over clear color
                _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);
                
                // PROPER MENU SYSTEM LIKE ORIGINAL JAVA VERSION
                if (_font != null)
                {
                    if (isInMenu)
                    {
                        // MENU: Draw background image first (like original Java drawSplash)
                        if (_menuBackground != null)
                        {
                            // Draw fullscreen background image
                            _spriteBatch.Draw(_menuBackground, new Rectangle(0, 0, viewport.Width, viewport.Height), Color.White);
                        }
                        
                        // Show instructions like original Java version
                        var centerX = viewport.Width / 2;
                        var centerY = viewport.Height / 2;
                        
                        // Main instruction - "Tap screen to Start"
                        var startText = "Tap screen to Start";
                        var startSize = _font.MeasureString(startText);
                        _spriteBatch.DrawString(_font, startText, 
                            new Vector2(centerX - startSize.X/2, centerY + 100), Color.White);
                        
                        // Secondary instruction
                        var instructText = "Swipe left/right to turn";
                        var instructSize = _font.MeasureString(instructText);
                        _spriteBatch.DrawString(_font, instructText, 
                            new Vector2(centerX - instructSize.X/2, centerY + 150), Color.White);
                        

                    }
                    else
                    {
                        // GAME: Show HUD elements like original Java version
                        // Note: HUD draws its own SpriteBatch, so we need to end ours first
                        _spriteBatch.End();
                        
                        if (_hud != null)
                        {
                            // Use the proper HUD system
                            int score = 0;
                            try { score = _glTronGame?.GetOwnPlayerScore() ?? 0; } catch { }
                            _hud.Draw(gameTime, score);
                        }
                        
                        // Restart SpriteBatch for additional UI elements
                        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);
                        
                        // Camera mode indicator and swipe instructions
                        if (_camera != null)
                        {
                            var cameraMode = _camera.GetCameraType().ToString();
                            _spriteBatch.DrawString(_font, $"Camera: {cameraMode}", new Vector2(viewport.Width - 200, viewport.Height - 50), Color.Cyan);
                            _spriteBatch.DrawString(_font, "Tap top-right to switch camera", new Vector2(viewport.Width - 300, 10), Color.Gray);
                        }
                        
                        // Swipe instructions
                        _spriteBatch.DrawString(_font, "◄ Swipe Left", new Vector2(10, viewport.Height - 80), Color.Yellow);
                        _spriteBatch.DrawString(_font, "Swipe Right ►", new Vector2(viewport.Width - 150, viewport.Height - 80), Color.Yellow);
                        
                        // Show swipe in progress
                        if (_swipeInProgress && _swipeStartPosition.HasValue)
                        {
                            // Draw swipe start indicator
                            var swipeColor = Color.Lime;
                            var swipeRect = new Rectangle((int)_swipeStartPosition.Value.X - 10, (int)_swipeStartPosition.Value.Y - 10, 20, 20);
                            _spriteBatch.Draw(_whitePixel, swipeRect, swipeColor);
                            
                            // Show swipe instruction
                            _spriteBatch.DrawString(_font, "Swiping...", new Vector2(_swipeStartPosition.Value.X - 40, _swipeStartPosition.Value.Y - 40), swipeColor);
                        }
                        

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
    
    private void ProcessSwipeInput(GameTime gameTime)
    {
        try
        {
            TouchCollection touchCollection = TouchPanel.GetState();
            var viewport = GraphicsDevice.Viewport;
            
            foreach (TouchLocation touch in touchCollection)
            {
                switch (touch.State)
                {
                    case TouchLocationState.Pressed:
                        // Start tracking swipe
                        _swipeStartPosition = touch.Position;
                        _swipeStartTime = gameTime.TotalGameTime.TotalMilliseconds;
                        _swipeInProgress = true;
                        
                        try
                        {
#if ANDROID
                            Android.Util.Log.Debug("GLTRON", $"Swipe started at: {touch.Position.X:F1}, {touch.Position.Y:F1}");
#endif
                        }
                        catch { }
                        
                        // Check for camera switch (top-right corner tap)
                        if (touch.Position.X > viewport.Width * 0.8f && touch.Position.Y < viewport.Height * 0.2f)
                        {
                            if (_camera != null)
                            {
                                var currentType = _camera.GetCameraType();
                                var newType = currentType == GltronMobileEngine.Video.CameraType.Follow ? 
                                             GltronMobileEngine.Video.CameraType.Bird : 
                                             GltronMobileEngine.Video.CameraType.Follow;
                                _camera.SetCameraType(newType);
                                
                                try
                                {
#if ANDROID
                                    Android.Util.Log.Info("GLTRON", $"Camera switched to: {newType}");
#endif
                                }
                                catch { }
                            }
                            _swipeInProgress = false; // Cancel swipe for camera switch
                        }
                        
                        // Handle menu tap (if in menu)
                        if (_glTronGame?.IsShowingMenu() == true)
                        {
                            _glTronGame?.addTouchEvent(touch.Position.X, touch.Position.Y, viewport.Width, viewport.Height);
                            _swipeInProgress = false; // Cancel swipe for menu
                        }
                        break;
                        
                    case TouchLocationState.Released:
                        if (_swipeInProgress && _swipeStartPosition.HasValue)
                        {
                            ProcessSwipeGesture(touch.Position, gameTime);
                        }
                        
                        // Reset swipe tracking
                        _swipeStartPosition = null;
                        _swipeInProgress = false;
                        break;
                        
                    case TouchLocationState.Moved:
                        // Optional: Could show swipe preview here
                        break;
                }
            }
            
            // Timeout check for swipes
            if (_swipeInProgress && _swipeStartPosition.HasValue)
            {
                double elapsedTime = gameTime.TotalGameTime.TotalMilliseconds - _swipeStartTime;
                if (elapsedTime > MAX_SWIPE_TIME)
                {
                    // Swipe took too long, cancel it
                    _swipeStartPosition = null;
                    _swipeInProgress = false;
                    
                    try
                    {
#if ANDROID
                        Android.Util.Log.Debug("GLTRON", "Swipe timed out");
#endif
                    }
                    catch { }
                }
            }
        }
        catch (System.Exception ex)
        {
            try
            {
#if ANDROID
                Android.Util.Log.Error("GLTRON", $"ProcessSwipeInput error: {ex}");
#endif
#pragma warning disable CS0168 // Variable is used in conditional compilation
            }
            catch { }
#pragma warning restore CS0168
        }
    }
    
    private void ProcessSwipeGesture(Vector2 endPosition, GameTime gameTime)
    {
        try
        {
            if (!_swipeStartPosition.HasValue) return;
            
            Vector2 swipeVector = endPosition - _swipeStartPosition.Value;
            float swipeDistance = swipeVector.Length();
            double swipeTime = gameTime.TotalGameTime.TotalMilliseconds - _swipeStartTime;
            
            try
            {
#if ANDROID
                Android.Util.Log.Debug("GLTRON", $"Swipe: distance={swipeDistance:F1}, time={swipeTime:F0}ms, vector=({swipeVector.X:F1},{swipeVector.Y:F1})");
#endif
            }
            catch { }
            
            // Check if swipe is long enough
            if (swipeDistance < MIN_SWIPE_DISTANCE)
            {
                try
                {
#if ANDROID
                    Android.Util.Log.Debug("GLTRON", "Swipe too short, treating as tap");
#endif
                }
                catch { }
                
                // Treat as tap if not in menu
                if (_glTronGame?.IsShowingMenu() == false)
                {
                    // For very short swipes, treat as tap to start game if in initial state
                    var viewport = GraphicsDevice.Viewport;
                    _glTronGame?.addTouchEvent(_swipeStartPosition.Value.X, _swipeStartPosition.Value.Y, viewport.Width, viewport.Height);
                }
                return;
            }
            
            // Determine swipe direction
            float horizontalComponent = System.Math.Abs(swipeVector.X);
            float verticalComponent = System.Math.Abs(swipeVector.Y);
            
            // Only process horizontal swipes for turning
            if (horizontalComponent > verticalComponent)
            {
                // Horizontal swipe
                if (swipeVector.X > 0)
                {
                    // Swipe right
                    ProcessTurnInput(GltronMobileEngine.Player.TURN_RIGHT);
                    
                    try
                    {
#if ANDROID
                        Android.Util.Log.Info("GLTRON", "SWIPE RIGHT - Turn Right");
#endif
                    }
                    catch { }
                }
                else
                {
                    // Swipe left
                    ProcessTurnInput(GltronMobileEngine.Player.TURN_LEFT);
                    
                    try
                    {
#if ANDROID
                        Android.Util.Log.Info("GLTRON", "SWIPE LEFT - Turn Left");
#endif
                    }
                    catch { }
                }
            }
            else
            {
                // Vertical swipe - could be used for other actions in the future
                try
                {
#if ANDROID
                    Android.Util.Log.Debug("GLTRON", "Vertical swipe detected (not used for turning)");
#endif
                }
                catch { }
            }
        }
        catch (System.Exception ex)
        {
            try
            {
#if ANDROID
                Android.Util.Log.Error("GLTRON", $"ProcessSwipeGesture error: {ex}");
#endif
#pragma warning disable CS0168 // Variable is used in conditional compilation
            }
            catch { }
#pragma warning restore CS0168
        }
    }
    
    private void ProcessTurnInput(int turnDirection)
    {
        try
        {
            if (_glTronGame?.IsShowingMenu() == true) return;
            
            // Send turn command to game
            var viewport = GraphicsDevice.Viewport;
            
            // Simulate the touch event that would cause a turn
            // Use center position and let the game logic handle the turn direction
            float centerX = turnDirection == GltronMobileEngine.Player.TURN_LEFT ? 
                           viewport.Width * 0.25f : viewport.Width * 0.75f;
            float centerY = viewport.Height * 0.5f;
            
            _glTronGame?.addTouchEvent(centerX, centerY, viewport.Width, viewport.Height);
        }
        catch (System.Exception ex)
        {
            try
            {
#if ANDROID
                Android.Util.Log.Error("GLTRON", $"ProcessTurnInput error: {ex}");
#endif
#pragma warning disable CS0168 // Variable is used in conditional compilation
            }
            catch { }
#pragma warning restore CS0168
        }
    }
}
