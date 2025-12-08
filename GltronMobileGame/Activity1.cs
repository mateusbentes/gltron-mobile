using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Microsoft.Xna.Framework;
using GltronMobileGame;
using System;

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
                FNAHelper.LogInfo("=== FNA ANDROID ACTIVITY STARTING ===");
                FNAHelper.LogInfo("Activity.OnCreate starting...");
                
                // Call base.OnCreate first
                base.OnCreate(bundle);
                FNAHelper.LogInfo("Base OnCreate completed");
                
                // Set up FNA environment variables before creating the game
                FNAHelper.SetupFNAEnvironment();
                
                // Create the game instance
                FNAHelper.LogInfo("Creating Game1 instance...");
                _game = new Game1();
                FNAHelper.LogInfo("Game1 instance created successfully");
                
                // FNA handles view creation and management internally
                // We just need to start the game loop
                FNAHelper.LogInfo("Starting FNA game loop...");
                _game.Run();
                
                FNAHelper.LogInfo("FNA Activity initialized successfully!");
            }
            catch (System.Exception ex)
            {
                FNAHelper.LogError("=== FNA INITIALIZATION EXCEPTION ===");
                FNAHelper.LogError($"EXCEPTION TYPE: {ex.GetType().FullName}");
                FNAHelper.LogError($"EXCEPTION MESSAGE: {ex.Message}");
                FNAHelper.LogError($"EXCEPTION STACK: {ex.StackTrace}");
                
                ShowErrorScreen(ex);
            }
        }



        private void ShowErrorScreen(System.Exception ex)
        {
            try
            {
                var errorView = new Android.Widget.TextView(this);
                errorView.Text = $"GLTron Mobile - FNA Initialization Error\n\n" +
                               $"Error Type: {ex.GetType().Name}\n" +
                               $"Message: {ex.Message}\n\n" +
                               $"FNA requires:\n" +
                               $"• OpenGL ES 3.0 support\n" +
                               $"• SDL2 native libraries\n" +
                               $"• OpenAL audio support\n\n" +
                               $"Please restart the application.\n" +
                               $"If the problem persists, your device may not support FNA requirements.";
                errorView.SetTextColor(Android.Graphics.Color.White);
                errorView.SetBackgroundColor(Android.Graphics.Color.DarkRed);
                errorView.Gravity = Android.Views.GravityFlags.Center;
                errorView.SetPadding(20, 20, 20, 20);
                errorView.TextSize = 14f;
                SetContentView(errorView);
                
                FNAHelper.LogError("FNA error view displayed");
            }
            catch (System.Exception ex2)
            {
                FNAHelper.LogError($"Failed to show error view: {ex2}");
            }
        }

        protected override void OnPause()
        {
            FNAHelper.LogInfo("Activity1.OnPause - FNA handles pause automatically");
            base.OnPause();
            // FNA handles game pausing automatically when the activity pauses
        }

        protected override void OnResume()
        {
            FNAHelper.LogInfo("Activity1.OnResume - FNA handles resume automatically");
            base.OnResume();
            // FNA handles game resuming automatically when the activity resumes
        }

        protected override void OnDestroy()
        {
            FNAHelper.LogInfo("Activity1.OnDestroy - Disposing FNA game");
            
            try
            {
                _game?.Dispose();
                _game = null;
                FNAHelper.LogInfo("FNA game disposed successfully");
            }
            catch (System.Exception ex)
            {
                FNAHelper.LogError($"Error disposing FNA game: {ex}");
            }
            
            base.OnDestroy();
        }
    }
}
