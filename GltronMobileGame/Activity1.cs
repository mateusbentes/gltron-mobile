using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Microsoft.Xna.Framework;
using GltronMobileGame;
using System.Threading.Tasks;
using System.Threading;

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
        private const int INITIALIZATION_TIMEOUT_MS = 10000; // 10 seconds

        protected override void OnCreate(Bundle bundle)
        {
            Android.Util.Log.Info("GLTRON", "=== MONOGAME GAME1 INITIALIZATION ===");
            
            base.OnCreate(bundle);
            Android.Util.Log.Info("GLTRON", "Activity.OnCreate completed");

            // Show loading screen first
            ShowLoadingScreen();

            // Initialize game with timeout
            _ = Task.Run(async () => await InitializeGameWithTimeout());
        }

        private void ShowLoadingScreen()
        {
            var loadingView = new Android.Widget.LinearLayout(this)
            {
                Orientation = Android.Widget.Orientation.Vertical
            };
            loadingView.SetGravity(Android.Views.GravityFlags.Center);
            loadingView.SetBackgroundColor(Android.Graphics.Color.Black);

            var titleText = new Android.Widget.TextView(this)
            {
                Text = "GLTron Mobile",
                TextSize = 24
            };
            titleText.SetTextColor(Android.Graphics.Color.Cyan);
            titleText.Gravity = Android.Views.GravityFlags.Center;

            var loadingText = new Android.Widget.TextView(this)
            {
                Text = "Loading...",
                TextSize = 16
            };
            loadingText.SetTextColor(Android.Graphics.Color.White);
            loadingText.Gravity = Android.Views.GravityFlags.Center;

            var progressBar = new Android.Widget.ProgressBar(this);

            loadingView.AddView(titleText);
            loadingView.AddView(loadingText);
            loadingView.AddView(progressBar);

            SetContentView(loadingView);
        }

        private async Task InitializeGameWithTimeout()
        {
            try
            {
                using (var cts = new CancellationTokenSource(INITIALIZATION_TIMEOUT_MS))
                {
                    Android.Util.Log.Info("GLTRON", "Step 1: Creating Game1 instance...");
                    _game = new Game1();
                    Android.Util.Log.Info("GLTRON", "Step 2: Game1 created successfully!");
                    
                    // Check if cancelled
                    cts.Token.ThrowIfCancellationRequested();
                    
                    Android.Util.Log.Info("GLTRON", "Step 3: Starting game with Run()...");
                    
                    // Run game initialization on UI thread
                    RunOnUiThread(() => {
                        try
                        {
                            _game.Run();
                            Android.Util.Log.Info("GLTRON", "Step 4: Game.Run() completed!");
                        }
                        catch (System.Exception ex)
                        {
                            Android.Util.Log.Error("GLTRON", $"Game.Run() failed: {ex}");
                            ShowErrorScreen(ex);
                        }
                    });
                }
            }
            catch (OperationCanceledException)
            {
                Android.Util.Log.Error("GLTRON", "Game initialization timed out!");
                RunOnUiThread(() => ShowTimeoutScreen());
            }
            catch (System.Exception ex)
            {
                Android.Util.Log.Error("GLTRON", $"Game initialization failed: {ex}");
                RunOnUiThread(() => ShowErrorScreen(ex));
            }
        }

        private void ShowErrorScreen(System.Exception ex)
        {
            Android.Util.Log.Error("GLTRON", "=== MONOGAME INITIALIZATION EXCEPTION ===");
            Android.Util.Log.Error("GLTRON", $"EXCEPTION TYPE: {ex.GetType().FullName}");
            Android.Util.Log.Error("GLTRON", $"EXCEPTION MESSAGE: {ex.Message}");
            Android.Util.Log.Error("GLTRON", $"EXCEPTION STACK: {ex.StackTrace}");
            
            var errorView = new Android.Widget.TextView(this);
            errorView.Text = $"MonoGame Error:\n{ex.GetType().Name}\n{ex.Message}\n\nTap to retry";
            errorView.SetTextColor(Android.Graphics.Color.White);
            errorView.SetBackgroundColor(Android.Graphics.Color.Red);
            errorView.Gravity = Android.Views.GravityFlags.Center;
            errorView.SetPadding(20, 20, 20, 20);
            
            // Add tap to retry functionality
            errorView.Click += (s, e) => {
                Android.Util.Log.Info("GLTRON", "Retrying game initialization...");
                ShowLoadingScreen();
                _ = Task.Run(async () => await InitializeGameWithTimeout());
            };
            
            SetContentView(errorView);
        }

        private void ShowTimeoutScreen()
        {
            var timeoutView = new Android.Widget.TextView(this);
            timeoutView.Text = "Game initialization timed out.\nThis may indicate a compatibility issue.\n\nTap to retry";
            timeoutView.SetTextColor(Android.Graphics.Color.White);
            timeoutView.SetBackgroundColor(Android.Graphics.Color.DarkRed);
            timeoutView.Gravity = Android.Views.GravityFlags.Center;
            timeoutView.SetPadding(20, 20, 20, 20);
            
            // Add tap to retry functionality
            timeoutView.Click += (s, e) => {
                Android.Util.Log.Info("GLTRON", "Retrying after timeout...");
                ShowLoadingScreen();
                _ = Task.Run(async () => await InitializeGameWithTimeout());
            };
            
            SetContentView(timeoutView);
        }

        protected override void OnPause()
        {
            base.OnPause();
            Android.Util.Log.Info("GLTRON", "Activity1.OnPause");
        }

        protected override void OnResume()
        {
            base.OnResume();
            Android.Util.Log.Info("GLTRON", "Activity1.OnResume");
        }

        protected override void OnDestroy()
        {
            Android.Util.Log.Info("GLTRON", "Activity1.OnDestroy");
            _game?.Dispose();
            base.OnDestroy();
        }
    }
}
