using System;
using Android.App;
using Android.OS;
using Android.Views;
using Microsoft.Xna.Framework;
using GltronMobileGame;

namespace gltron.org.gltronmobile
{
    [Activity(
        Label = "GLTron Mobile",
        MainLauncher = true,
        Theme = "@android:style/Theme.NoTitleBar.Fullscreen",
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape
    )]
    public class Activity1 : Activity
    {
        private Game1 _game;
        private AndroidGameView _gameView;

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                // Enable edge-to-edge fullscreen
                Window?.SetFlags(
                    WindowManagerFlags.Fullscreen,
                    WindowManagerFlags.Fullscreen
                );

                Window?.SetFlags(
                    WindowManagerFlags.KeepScreenOn,
                    WindowManagerFlags.KeepScreenOn
                );

                // Create the game instance
                _game = new Game1();

                // Debug: Check specific services we're interested in
                Android.Util.Log.Debug("GLTRON", "=== Checking MonoGame Services ===");
                
                var viewService = _game.Services.GetService(typeof(View));
                Android.Util.Log.Debug("GLTRON", $"View service: {viewService?.GetType().FullName ?? "NULL"}");
                
                var androidViewService = _game.Services.GetService(typeof(Android.Views.View));
                Android.Util.Log.Debug("GLTRON", $"Android.Views.View service: {androidViewService?.GetType().FullName ?? "NULL"}");
                
                // Try common MonoGame Android view types
                var gameViewService = _game.Services.GetService(System.Type.GetType("Microsoft.Xna.Framework.AndroidGameView"));
                Android.Util.Log.Debug("GLTRON", $"AndroidGameView service: {gameViewService?.GetType().FullName ?? "NULL"}");

                // Try to get the view from MonoGame services
                var gameView = _game.Services.GetService(typeof(View)) as View;

                if (gameView != null)
                {
                    Android.Util.Log.Debug("GLTRON", "SUCCESS: Using view from services");
                    SetContentView(gameView);
                    _game.Run();
                }
                else
                {
                    Android.Util.Log.Debug("GLTRON", "EXPECTED: No view available from services - MonoGame .NET 9 behavior");
                    Android.Util.Log.Debug("GLTRON", "Creating custom AndroidGameView");
                    
                    // Create our custom AndroidGameView for MonoGame .NET 9
                    try
                    {
                        _gameView = new AndroidGameView(this, _game);
                        Android.Util.Log.Debug("GLTRON", "Custom AndroidGameView created successfully");
                        
                        SetContentView(_gameView);
                        
                        Android.Util.Log.Debug("GLTRON", "AndroidGameView set as content view");
                        
                        // Note: We don't call _game.Run() here because our AndroidGameView
                        // handles the game loop through OpenGL rendering callbacks
                    }
                    catch (Exception viewEx)
                    {
                        Android.Util.Log.Error("GLTRON", $"Failed to create custom AndroidGameView: {viewEx.Message}");
                        throw new InvalidOperationException($"Custom AndroidGameView creation failed: {viewEx.Message}", viewEx);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorScreen(ex);
            }
        }

        private void ShowErrorScreen(Exception ex)
        {
            try
            {
                var errorView = new Android.Widget.TextView(this);
                errorView.Text = $"GLTron Mobile - Initialization Error\n\n" +
                               $"Error Type: {ex.GetType().Name}\n" +
                               $"Message: {ex.Message}\n\n" +
                               $"Please restart the application.";
                errorView.SetTextColor(Android.Graphics.Color.White);
                errorView.SetBackgroundColor(Android.Graphics.Color.DarkRed);
                errorView.Gravity = Android.Views.GravityFlags.Center;
                errorView.SetPadding(20, 20, 20, 20);
                SetContentView(errorView);
            }
            catch (Exception)
            {
                // Ignore secondary errors
            }
        }

        protected override void OnPause()
        {
            base.OnPause();
            _gameView?.Pause();
        }

        protected override void OnResume()
        {
            base.OnResume();
            _gameView?.Resume();
        }

        protected override void OnDestroy()
        {
            try
            {
                _gameView = null;
                _game?.Dispose();
                _game = null;
            }
            catch (Exception ex)
            {
                // Log error if needed
            }
            
            base.OnDestroy();
        }
    }
}
