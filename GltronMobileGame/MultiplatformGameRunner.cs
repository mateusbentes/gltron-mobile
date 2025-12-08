using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#if ANDROID
using Android.App;
using Android.Views;
#elif IOS
using Foundation;
using UIKit;
#endif

namespace GltronMobileGame
{
    /// <summary>
    /// Multiplatform game runner that bypasses the problematic FNA Game constructor
    /// Works on Android, iOS, Windows, Linux, macOS
    /// </summary>
    public class MultiplatformGameRunner
    {
        private Game1 _game;
        private bool _isRunning = false;
        
        // Platform-specific context
#if ANDROID
        private Activity _activity;
#elif IOS
        private UIViewController _viewController;
#else
        private object _platformContext; // For desktop platforms
#endif
        
        // Swipe detection system
        private Vector2? _swipeStartPosition = null;
        private double _swipeStartTime = 0;
        private bool _swipeInProgress = false;
        private const float MIN_SWIPE_DISTANCE = 30f;
        private const double MAX_SWIPE_TIME = 500;

        // Platform-agnostic constructor
#if ANDROID
        public MultiplatformGameRunner(Activity activity)
        {
            _activity = activity;
            InitializeCommon();
        }
#elif IOS
        public MultiplatformGameRunner(UIViewController viewController)
        {
            _viewController = viewController;
            InitializeCommon();
        }
#else
        public MultiplatformGameRunner(object platformContext = null)
        {
            _platformContext = platformContext;
            InitializeCommon();
        }
#endif

        private void InitializeCommon()
        {
            try
            {
                LogInfo("MultiplatformGameRunner constructor starting...");
                
                // The actual Game1 creation will be deferred to Initialize() method
                // to avoid the hanging constructor issue
                
                LogInfo("MultiplatformGameRunner constructor completed - Game1 creation deferred");
            }
            catch (System.Exception ex)
            {
                LogError($"MultiplatformGameRunner constructor failed: {ex}");
                throw;
            }
        }

        public void Initialize()
        {
            try
            {
                LogInfo("MultiplatformGameRunner Initialize starting...");
                
                // Create Game1 instance here where it's safer
                if (_game == null)
                {
                    LogInfo("Creating Game1 instance in Initialize()...");
                    _game = new Game1();
                    LogInfo("Game1 instance created successfully!");
                }
                
                LogInfo("MultiplatformGameRunner Initialize completed");
            }
            catch (System.Exception ex)
            {
                LogError($"MultiplatformGameRunner Initialize failed: {ex}");
                throw;
            }
        }

        public void LoadContent()
        {
            try
            {
                LogInfo("MultiplatformGameRunner LoadContent starting...");
                
                // Game1 handles its own content loading through its lifecycle
                // We don't need to do anything here
                
                LogInfo("MultiplatformGameRunner LoadContent completed");
            }
            catch (System.Exception ex)
            {
                LogError($"MultiplatformGameRunner LoadContent failed: {ex}");
                throw;
            }
        }

        public void Update(GameTime gameTime)
        {
            try
            {
                // Let Game1 handle its own update cycle
                // We don't need to do anything here - Game1.Run() handles everything
            }
            catch (System.Exception ex)
            {
                LogError($"MultiplatformGameRunner Update failed: {ex}");
            }
        }

        public void Draw(GameTime gameTime)
        {
            try
            {
                // Let Game1 handle its own draw cycle
                // We don't need to do anything here - Game1.Run() handles everything
            }
            catch (System.Exception ex)
            {
                LogError($"MultiplatformGameRunner Draw failed: {ex}");
            }
        }

        public void Run()
        {
            try
            {
                LogInfo("MultiplatformGameRunner Run starting...");
                
                // Initialize and create Game1
                Initialize();
                LoadContent();
                
                // Run the actual Game1 instance
                if (_game != null)
                {
                    LogInfo("Starting Game1.Run()...");
                    _game.Run();
                    LogInfo("Game1.Run() completed");
                }
                else
                {
                    LogError("Game1 instance is null - cannot run game");
                    throw new System.InvalidOperationException("Game1 instance is null");
                }
                
                LogInfo("MultiplatformGameRunner completed successfully!");
            }
            catch (System.Exception ex)
            {
                LogError($"MultiplatformGameRunner Run failed: {ex}");
                throw;
            }
        }

        public void Dispose()
        {
            try
            {
                LogInfo("MultiplatformGameRunner disposing...");
                
                // Dispose Game1 instance
                _game?.Dispose();
                _game = null;
                
                LogInfo("MultiplatformGameRunner disposed");
            }
            catch (System.Exception ex)
            {
                LogError($"MultiplatformGameRunner dispose failed: {ex}");
            }
        }

        // Platform-agnostic logging
        private void LogInfo(string message)
        {
            System.Diagnostics.Debug.WriteLine($"GLTRON: {message}");
            
#if ANDROID
            try { Android.Util.Log.Info("GLTRON", message); } catch { }
#elif IOS
            try { Foundation.NSLog($"GLTRON: {message}"); } catch { }
#else
            Console.WriteLine($"GLTRON: {message}");
#endif
        }

        private void LogError(string message)
        {
            System.Diagnostics.Debug.WriteLine($"GLTRON: ERROR - {message}");
            
#if ANDROID
            try { Android.Util.Log.Error("GLTRON", message); } catch { }
#elif IOS
            try { Foundation.NSLog($"GLTRON: ERROR - {message}"); } catch { }
#else
            Console.WriteLine($"GLTRON: ERROR - {message}");
#endif
        }

        // Platform-specific input handling
        public void HandleTouchInput(float x, float y, int screenWidth, int screenHeight)
        {
            try
            {
                _glTronGame?.addTouchEvent(x, y, screenWidth, screenHeight);
            }
            catch (System.Exception ex)
            {
                LogError($"HandleTouchInput failed: {ex}");
            }
        }

        // Platform-specific lifecycle methods
        public void OnPause()
        {
            try
            {
                LogInfo("MultiplatformGameRunner paused");
                // Pause game logic here
            }
            catch (System.Exception ex)
            {
                LogError($"OnPause failed: {ex}");
            }
        }

        public void OnResume()
        {
            try
            {
                LogInfo("MultiplatformGameRunner resumed");
                // Resume game logic here
            }
            catch (System.Exception ex)
            {
                LogError($"OnResume failed: {ex}");
            }
        }

        // Platform detection
        public static string GetCurrentPlatform()
        {
#if ANDROID
            return "Android";
#elif IOS
            return "iOS";
#elif WINDOWS
            return "Windows";
#elif LINUX
            return "Linux";
#elif MACOS
            return "macOS";
#else
            return "Unknown";
#endif
        }
    }
}
