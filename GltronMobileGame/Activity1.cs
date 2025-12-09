using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Microsoft.Xna.Framework;
using GltronMobileGame;
using System.Reflection;

namespace gltron.org.gltronmobile
{
    [Activity(
        Label = "@string/app_name",
        Icon = "@drawable/Icon",
        MainLauncher = true,
        AlwaysRetainTaskState = true,
        LaunchMode = LaunchMode.SingleInstance,
        ScreenOrientation = ScreenOrientation.SensorLandscape,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize | ConfigChanges.ScreenLayout,
        Theme = "@android:style/Theme.NoTitleBar.Fullscreen"
    )]
    public class Activity1 : Activity
    {
        private Game1 _game;

        protected override void OnCreate(Bundle bundle)
        {
            try
            {
                Android.Util.Log.Info("GLTRON", "=== MONOGAME GAME1 INITIALIZATION ===");
                Android.Util.Log.Info("GLTRON", "Activity.OnCreate starting...");
                
                base.OnCreate(bundle);
                Android.Util.Log.Info("GLTRON", "Activity.OnCreate completed");

                Android.Util.Log.Info("GLTRON", "Step 1: Setting up Android environment...");
                
                // Try multiple approaches to set the Android context
                try
                {
                    // Approach 1: Try AndroidGamePlatform._currentActivity
                    var platformType = System.Type.GetType("Microsoft.Xna.Framework.AndroidGamePlatform, MonoGame.Framework");
                    if (platformType != null)
                    {
                        Android.Util.Log.Info("GLTRON", "Found AndroidGamePlatform type");
                        
                        // Try different field names
                        string[] fieldNames = { "_currentActivity", "currentActivity", "Activity", "_activity", "Context", "_context" };
                        foreach (var fieldName in fieldNames)
                        {
                            var field = platformType.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
                            if (field != null)
                            {
                                field.SetValue(null, this);
                                Android.Util.Log.Info("GLTRON", $"Set {fieldName} field successfully");
                                break;
                            }
                        }
                        
                        // Try properties too
                        string[] propertyNames = { "CurrentActivity", "Activity", "Context" };
                        foreach (var propName in propertyNames)
                        {
                            var prop = platformType.GetProperty(propName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
                            if (prop != null && prop.CanWrite)
                            {
                                prop.SetValue(null, this);
                                Android.Util.Log.Info("GLTRON", $"Set {propName} property successfully");
                                break;
                            }
                        }
                    }
                    
                    // Approach 2: Try setting Android.App.Application.Context
                    if (Android.App.Application.Context == null)
                    {
                        Android.Util.Log.Info("GLTRON", "Application.Context is null, trying to set it");
                        // This might not work, but worth trying
                    }
                    else
                    {
                        Android.Util.Log.Info("GLTRON", "Application.Context is available");
                    }
                    
                    // Approach 3: Set current activity in a global way
                    Java.Lang.JavaSystem.SetProperty("mono.android.activity", this.GetType().FullName);
                    
                }
                catch (System.Exception ex)
                {
                    Android.Util.Log.Error("GLTRON", $"Setup failed: {ex.Message}");
                }
                
                Android.Util.Log.Info("GLTRON", "Step 2: Creating Game1 instance...");
                
                // Create the game instance
                _game = new Game1();
                Android.Util.Log.Info("GLTRON", "Game1 instance created successfully");
                
                Android.Util.Log.Info("GLTRON", "Step 3: Running game...");
                _game.Run();
                Android.Util.Log.Info("GLTRON", "Game started successfully");
                
                Android.Util.Log.Info("GLTRON", "MonoGame initialized successfully!");
            }
            catch (System.Exception ex)
            {
                Android.Util.Log.Error("GLTRON", "=== MONOGAME INITIALIZATION EXCEPTION ===");
                Android.Util.Log.Error("GLTRON", $"EXCEPTION TYPE: {ex.GetType().FullName}");
                Android.Util.Log.Error("GLTRON", $"EXCEPTION MESSAGE: {ex.Message}");
                Android.Util.Log.Error("GLTRON", $"EXCEPTION STACK: {ex.StackTrace}");
                
                ShowErrorScreen(ex);
            }
        }



        private void ShowErrorScreen(System.Exception ex)
        {
            try
            {
                var errorView = new Android.Widget.TextView(this);
                errorView.Text = $"GLTron Mobile - Initialization Error\n\n" +
                               $"Error Type: {ex.GetType().Name}\n" +
                               $"Message: {ex.Message}\n\n" +
                               $"Please restart the application.\n" +
                               $"If the problem persists, try restarting your device.";
                errorView.SetTextColor(Android.Graphics.Color.White);
                errorView.SetBackgroundColor(Android.Graphics.Color.DarkRed);
                errorView.Gravity = Android.Views.GravityFlags.Center;
                errorView.SetPadding(20, 20, 20, 20);
                SetContentView(errorView);
                Android.Util.Log.Info("GLTRON", "Error view displayed");
            }
            catch (System.Exception ex2)
            {
                Android.Util.Log.Error("GLTRON", $"Failed to show error view: {ex2}");
            }
        }

        protected override void OnPause()
        {
            Android.Util.Log.Info("GLTRON", "Activity1.OnPause");
            base.OnPause();
        }

        protected override void OnResume()
        {
            Android.Util.Log.Info("GLTRON", "Activity1.OnResume");
            base.OnResume();
        }

        protected override void OnDestroy()
        {
            Android.Util.Log.Info("GLTRON", "Activity1.OnDestroy");
            
            try
            {
                _game?.Dispose();
                _game = null;
            }
            catch (System.Exception ex)
            {
                Android.Util.Log.Error("GLTRON", $"Error disposing game: {ex}");
            }
            
            base.OnDestroy();
        }
    }
}
