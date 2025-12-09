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

                Android.Util.Log.Info("GLTRON", "Step 1: Setting up Android context for MonoGame...");
                
                // Set up Android context using reflection before creating Game instance
                SetupMonoGameAndroidContext();
                
                Android.Util.Log.Info("GLTRON", "Step 2: Creating Game1 instance...");
                
                // Create the game instance
                _game = new Game1();
                Android.Util.Log.Info("GLTRON", "Game1 instance created successfully");
                
                Android.Util.Log.Info("GLTRON", "Step 3: Getting game view from services...");
                var gameView = _game.Services.GetService(typeof(Android.Views.View));
                if (gameView == null)
                {
                    Android.Util.Log.Error("GLTRON", "ERROR: Game view service is null!");
                    throw new System.InvalidOperationException("Game view service not available");
                }
                Android.Util.Log.Info("GLTRON", "Game view service obtained successfully");
                
                Android.Util.Log.Info("GLTRON", "Step 4: Setting content view...");
                SetContentView((Android.Views.View)gameView);
                Android.Util.Log.Info("GLTRON", "Content view set successfully");
                
                Android.Util.Log.Info("GLTRON", "Step 5: Starting game loop...");
                _game.RunOneFrame();
                Android.Util.Log.Info("GLTRON", "Game loop started");
                
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

        private void SetupMonoGameAndroidContext()
        {
            try
            {
                Android.Util.Log.Info("GLTRON", "Attempting to set Android context via reflection...");
                
                // Try multiple approaches to set the Android context
                
                // Approach 1: Look for AndroidGamePlatform static fields/properties
                var androidGamePlatformType = System.Type.GetType("Microsoft.Xna.Framework.AndroidGamePlatform");
                if (androidGamePlatformType != null)
                {
                    Android.Util.Log.Info("GLTRON", "Found AndroidGamePlatform type");
                    
                    // Try to find Activity field/property
                    var activityField = androidGamePlatformType.GetField("Activity", BindingFlags.Public | BindingFlags.Static);
                    if (activityField != null)
                    {
                        activityField.SetValue(null, this);
                        Android.Util.Log.Info("GLTRON", "Set Activity via AndroidGamePlatform.Activity field");
                        return;
                    }
                    
                    var activityProperty = androidGamePlatformType.GetProperty("Activity", BindingFlags.Public | BindingFlags.Static);
                    if (activityProperty != null && activityProperty.CanWrite)
                    {
                        activityProperty.SetValue(null, this);
                        Android.Util.Log.Info("GLTRON", "Set Activity via AndroidGamePlatform.Activity property");
                        return;
                    }
                }
                
                // Approach 2: Look for Game class static fields/properties
                var gameType = typeof(Microsoft.Xna.Framework.Game);
                var gameActivityField = gameType.GetField("Activity", BindingFlags.Public | BindingFlags.Static);
                if (gameActivityField != null)
                {
                    gameActivityField.SetValue(null, this);
                    Android.Util.Log.Info("GLTRON", "Set Activity via Game.Activity field");
                    return;
                }
                
                var gameActivityProperty = gameType.GetProperty("Activity", BindingFlags.Public | BindingFlags.Static);
                if (gameActivityProperty != null && gameActivityProperty.CanWrite)
                {
                    gameActivityProperty.SetValue(null, this);
                    Android.Util.Log.Info("GLTRON", "Set Activity via Game.Activity property");
                    return;
                }
                
                // Approach 3: Look for any Android-related static context setters
                var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    if (assembly.FullName.Contains("MonoGame") || assembly.FullName.Contains("Microsoft.Xna.Framework"))
                    {
                        var types = assembly.GetTypes();
                        foreach (var type in types)
                        {
                            if (type.Name.Contains("Android"))
                            {
                                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);
                                foreach (var method in methods)
                                {
                                    if (method.Name.Contains("SetActivity") || method.Name.Contains("Initialize"))
                                    {
                                        var parameters = method.GetParameters();
                                        if (parameters.Length == 1 && parameters[0].ParameterType == typeof(Activity))
                                        {
                                            method.Invoke(null, new object[] { this });
                                            Android.Util.Log.Info("GLTRON", $"Set Activity via {type.Name}.{method.Name}");
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                
                Android.Util.Log.Info("GLTRON", "No suitable method found to set Android context");
            }
            catch (System.Exception ex)
            {
                Android.Util.Log.Error("GLTRON", $"Error setting up Android context: {ex.Message}");
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
