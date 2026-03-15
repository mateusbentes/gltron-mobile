using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Microsoft.Xna.Framework;
using GltronMobileGame;

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
    public class Activity1 : AndroidGameActivity
    {
        private Game1 _game;

        protected override void OnCreate(Bundle bundle)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== MONOGAME GAME1 INITIALIZATION ===");
                System.Diagnostics.Debug.WriteLine("Activity.OnCreate starting...");
                
                base.OnCreate(bundle);
                System.Diagnostics.Debug.WriteLine("Activity.OnCreate completed");

                System.Diagnostics.Debug.WriteLine("Step 1: Creating Game1 instance...");
                
                // Create the game instance
                _game = new Game1();
                System.Diagnostics.Debug.WriteLine("Game1 instance created successfully");
                
                System.Diagnostics.Debug.WriteLine("Step 2: Getting game view from services...");
                var gameView = _game.Services.GetService(typeof(Android.Views.View));
                if (gameView == null)
                {
                    System.Diagnostics.Debug.WriteLine("ERROR: Game view service is null!");
                    throw new System.InvalidOperationException("Game view service not available");
                }
                System.Diagnostics.Debug.WriteLine("Game view service obtained successfully");
                
                System.Diagnostics.Debug.WriteLine("Step 3: Setting content view...");
                SetContentView((Android.Views.View)gameView);
                System.Diagnostics.Debug.WriteLine("Content view set successfully");
                
                System.Diagnostics.Debug.WriteLine("Step 4: Starting game loop...");
                _game.RunOneFrame();
                System.Diagnostics.Debug.WriteLine("Game loop started");
                
                System.Diagnostics.Debug.WriteLine("MonoGame initialized successfully!");
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("=== MONOGAME INITIALIZATION EXCEPTION ===");
                System.Diagnostics.Debug.WriteLine($"EXCEPTION TYPE: {ex.GetType().FullName}");
                System.Diagnostics.Debug.WriteLine($"EXCEPTION MESSAGE: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"EXCEPTION STACK: {ex.StackTrace}");
                
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
                System.Diagnostics.Debug.WriteLine("Error view displayed");
            }
            catch (System.Exception ex2)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to show error view: {ex2}");
            }
        }

        protected override void OnPause()
        {
            System.Diagnostics.Debug.WriteLine("Activity1.OnPause");
            base.OnPause();
        }

        protected override void OnResume()
        {
            System.Diagnostics.Debug.WriteLine("Activity1.OnResume");
            base.OnResume();
        }

        protected override void OnDestroy()
        {
            System.Diagnostics.Debug.WriteLine("GLTRON: Activity1.OnDestroy");

            try
            {
                _game?.Dispose();
                _game = null;
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GLTRON: Error disposing game: {ex}");
            }

            base.OnDestroy();
        }
    }
}
