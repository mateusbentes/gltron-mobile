using System;
using Android.App;
using Android.OS;
using Android.Views;
using Microsoft.Xna.Framework;
using GltronMobileGame;
using System.Reflection;

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
            base.OnCreate(savedInstanceState);

            // Enable fullscreen and keep the screen on while playing.
            Window?.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
            Window?.SetFlags(WindowManagerFlags.KeepScreenOn, WindowManagerFlags.KeepScreenOn);

            try
            {
                Android.Util.Log.Debug("GLTRON", "Setting up Android environment...");
                
                // Set the current activity in Android.App.Application context
                // This is what MonoGame's AndroidGamePlatform might be looking for
                if (Android.App.Application.Context == null)
                {
                    Android.Util.Log.Error("GLTRON", "Application.Context is null - this is likely the problem");
                }
                
                // Try to set the activity using Java interop
                try
                {
                    var javaClass = Java.Lang.Class.ForName("mono.MonoPackageManager");
                    if (javaClass != null)
                    {
                        Android.Util.Log.Debug("GLTRON", "Found MonoPackageManager class");
                    }
                }
                catch (Exception ex)
                {
                    Android.Util.Log.Debug("GLTRON", $"MonoPackageManager not found: {ex.Message}");
                }
                
                // Create AndroidGameView first to establish OpenGL context
                Android.Util.Log.Debug("GLTRON", "Creating AndroidGameView without Game first...");
                
                // Create the view first
                _gameView = new AndroidGameView(this, null);
                SetContentView(_gameView);
                
                Android.Util.Log.Debug("GLTRON", "AndroidGameView created and set as content view");
                
                // Now try to create the game in the OpenGL context
                Android.Util.Log.Debug("GLTRON", "Will create Game1 in OpenGL context...");
            }
            catch (Exception ex)
            {
                // If initialization fails, show a readable on-screen error instead of a silent crash.
                ShowErrorScreen(ex);
            }
        }

        /// <summary>
        /// Displays a simple centered error message if the game fails to initialize.
        /// This avoids black-screen crashes and gives immediate feedback to the user.
        /// </summary>
        private void ShowErrorScreen(Exception ex)
        {
            var errorView = new Android.Widget.TextView(this);
            errorView.Text = $"GLTron Mobile - Initialization Error\n\n{ex}";
            errorView.SetTextColor(Android.Graphics.Color.White);
            errorView.SetBackgroundColor(Android.Graphics.Color.DarkRed);
            errorView.Gravity = GravityFlags.Center;
            errorView.SetPadding(20, 20, 20, 20);

            SetContentView(errorView);
        }

        private void SetAndroidContext()
        {
            try
            {
                // Set the current activity for MonoGame's AndroidGamePlatform
                // This must be done BEFORE creating the Game instance
                
                // Try multiple approaches to set the Android context
                var gameType = typeof(Microsoft.Xna.Framework.Game);
                
                // Approach 1: Try to find and set Activity field
                var activityField = gameType.GetField("Activity", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (activityField != null)
                {
                    activityField.SetValue(null, this);
                    Android.Util.Log.Debug("GLTRON", "Set Game.Activity field");
                    return;
                }
                
                // Approach 2: Try AndroidGamePlatform static fields
                var platformType = System.Type.GetType("Microsoft.Xna.Framework.AndroidGamePlatform, MonoGame.Framework");
                if (platformType != null)
                {
                    var currentActivityField = platformType.GetField("_currentActivity", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                    if (currentActivityField != null)
                    {
                        currentActivityField.SetValue(null, this);
                        Android.Util.Log.Debug("GLTRON", "Set AndroidGamePlatform._currentActivity field");
                        return;
                    }
                }
                
                // Approach 3: Set Android.App.Application context
                if (Android.App.Application.Context == null)
                {
                    Android.Util.Log.Debug("GLTRON", "Application.Context is null - this might be the issue");
                }
                
                Android.Util.Log.Debug("GLTRON", "Could not set Android context via reflection");
            }
            catch (Exception ex)
            {
                Android.Util.Log.Error("GLTRON", $"Failed to set Android context: {ex.Message}");
            }
        }

        protected override void OnPause()
        {
            base.OnPause();

            // Pause rendering + GL thread safely.
            // This prevents crashes on home-button press or app minimization.
            _gameView?.Pause();
        }

        protected override void OnResume()
        {
            base.OnResume();

            // Resume rendering + GL thread.
            // This restores the MonoGame frame loop on returning to the app.
            _gameView?.Resume();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _gameView = null;
            _game?.Dispose();
            _game = null;
        }
    }
}
