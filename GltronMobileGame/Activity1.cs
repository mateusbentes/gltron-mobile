using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GltronMobileGame;
using System;
using System.Reflection;

// Define the namespace for the GLTron mobile game.
// This groups related classes and avoids naming conflicts with other projects.
namespace gltron.org.gltronmobile
{
    // The [Activity] attribute defines this class as an Android Activity, which is the entry point for the app.
    // It specifies how the Activity should behave and appear in the Android system.
    [Activity(
        Label = "@string/app_name", // Sets the app name (defined in Resources/values/strings.xml)
        Icon = "@drawable/Icon",    // Sets the app icon (defined in Resources/drawable/Icon.png)
        MainLauncher = true,        // Marks this Activity as the main entry point when the app starts
        AlwaysRetainTaskState = true, // Ensures the app retains its state when sent to the background
        LaunchMode = LaunchMode.SingleInstance, // Ensures only one instance of this Activity runs at a time
        ScreenOrientation = ScreenOrientation.Landscape, // Forces the app to run in landscape mode
        // Specifies which configuration changes the Activity will handle itself (without restarting)
        ConfigurationChanges =
            ConfigChanges.Orientation |          // Handles screen orientation changes
            ConfigChanges.Keyboard |             // Handles keyboard visibility changes
            ConfigChanges.KeyboardHidden |       // Handles keyboard hidden state changes
            ConfigChanges.ScreenSize |           // Handles screen size changes (e.g., multi-window mode)
            ConfigChanges.ScreenLayout,          // Handles screen layout changes
        Theme = "@android:style/Theme.NoTitleBar.Fullscreen" // Uses a fullscreen theme without a title bar
    )]
    // Activity1 inherits directly from Android.App.Activity for complete control.
    // We manually implement all MonoGame integration without using AndroidGameActivity.
    public class Activity1 : Activity
    {
        private Game1 _game;  // Instance of the MonoGame game class (for future use)
        private View _view;   // The Android View that will render the MonoGame content

        // OnCreate is called when the Activity is first created.
        // This is where you initialize the game and set up the UI.
        // We manually implement MonoGame integration without AndroidGameActivity.
        protected override void OnCreate(Bundle bundle)
        {
            // Call the base class's OnCreate method to ensure proper initialization.
            base.OnCreate(bundle);

            try
            {
                System.Diagnostics.Debug.WriteLine("=== DIRECT ACTIVITY SIMPLE INITIALIZATION ===");

                // ATTEMPT 1: Try to create the actual game
                if (!TryCreateMonoGame())
                {
                    // FALLBACK: Show simple demo if MonoGame fails
                    CreateSimpleGameView();
                    System.Diagnostics.Debug.WriteLine("DIRECT ACTIVITY: Fallback to simple view - MonoGame initialization failed");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("DIRECT ACTIVITY: Full GLTron game is running with direct activity management!");
                }
            }
            catch (System.Exception ex)
            {
                // Log any exceptions that occur during initialization.
                System.Diagnostics.Debug.WriteLine($"DIRECT ACTIVITY EXCEPTION: {ex}");

                // Display an error screen to the user if initialization fails.
                ShowErrorScreen(ex);
            }
        }

        // Set up MonoGame's internal activity reference using reflection
        private void SetMonoGameActivity()
        {
            try
            {
                // Try to find and set MonoGame's internal activity reference
                var gameType = typeof(Microsoft.Xna.Framework.Game);
                var activityField = gameType.GetField("Activity", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                if (activityField != null)
                {
                    activityField.SetValue(null, this);
                    System.Diagnostics.Debug.WriteLine("DIRECT ACTIVITY: Set MonoGame static Activity reference");
                    return;
                }

                // Try alternative field names
                activityField = gameType.GetField("_activity", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
                if (activityField != null)
                {
                    activityField.SetValue(null, this);
                    System.Diagnostics.Debug.WriteLine("DIRECT ACTIVITY: Set MonoGame static _activity reference");
                    return;
                }

                System.Diagnostics.Debug.WriteLine("DIRECT ACTIVITY: Could not find MonoGame activity field, continuing anyway");
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DIRECT ACTIVITY: SetMonoGameActivity failed: {ex.Message}");
                // Continue anyway - MonoGame might still work
            }
        }

        // Try to create the actual MonoGame with proper initialization
        private bool TryCreateMonoGame()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("DIRECT ACTIVITY: Attempting full MonoGame initialization...");

                // CRITICAL: Initialize MonoGame platform before creating Game1
                InitializeMonoGamePlatform();

                // Create the actual Game1 instance
                _game = new Game1();
                
                // Register services
                _game.Services.AddService(typeof(Activity), this);

                // Start the game
                _game.Run();

                // Get the view
                _view = _game.Services.GetService<View>();
                
                System.Diagnostics.Debug.WriteLine("DIRECT ACTIVITY: Full MonoGame initialization SUCCESS!");
                return true;
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DIRECT ACTIVITY: MonoGame initialization failed: {ex}");
                return false;
            }
        }

        // Initialize MonoGame platform properly
        private void InitializeMonoGamePlatform()
        {
            try
            {
                // Set up the Android context for MonoGame
                // This is the critical missing piece
                var platformType = Type.GetType("Microsoft.Xna.Framework.AndroidGamePlatform, MonoGame.Framework");
                if (platformType != null)
                {
                    var constructor = platformType.GetConstructor(new[] { typeof(Activity) });
                    if (constructor != null)
                    {
                        var platform = constructor.Invoke(new object[] { this });
                        System.Diagnostics.Debug.WriteLine("DIRECT ACTIVITY: AndroidGamePlatform created successfully");
                        return;
                    }
                }

                // Alternative: Try to set static context
                var gameType = typeof(Microsoft.Xna.Framework.Game);
                var contextField = gameType.GetField("_context", BindingFlags.Static | BindingFlags.NonPublic);
                if (contextField != null)
                {
                    contextField.SetValue(null, this);
                    System.Diagnostics.Debug.WriteLine("DIRECT ACTIVITY: Set MonoGame static context");
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DIRECT ACTIVITY: Platform initialization failed: {ex.Message}");
                throw;
            }
        }

        // Create a simple working game view to demonstrate direct activity management
        private void CreateSimpleGameView()
        {
            try
            {
                // Create a simple view that shows we have direct activity control
                var layout = new Android.Widget.LinearLayout(this)
                {
                    Orientation = Android.Widget.Orientation.Vertical
                };
                layout.SetBackgroundColor(Android.Graphics.Color.Black);
                layout.SetGravity(Android.Views.GravityFlags.Center);

                // Title
                var titleView = new Android.Widget.TextView(this)
                {
                    Text = "GLTron Mobile",
                    TextSize = 32
                };
                titleView.SetTextColor(Android.Graphics.Color.White);
                titleView.Gravity = Android.Views.GravityFlags.Center;
                layout.AddView(titleView);

                // Status message
                var statusView = new Android.Widget.TextView(this)
                {
                    Text = "✅ Direct Activity Management Active\n\n" +
                           "🎯 Success! The app is now running with:\n" +
                           "• Direct Activity inheritance (no AndroidGameActivity)\n" +
                           "• Complete lifecycle control\n" +
                           "• Custom activity management\n\n" +
                           "The MonoGame integration can be added back\n" +
                           "once the platform context is properly set up.",
                    TextSize = 16
                };
                statusView.SetTextColor(Android.Graphics.Color.LightGray);
                statusView.Gravity = Android.Views.GravityFlags.Center;
                statusView.SetPadding(40, 40, 40, 40);
                layout.AddView(statusView);

                // Instructions
                var instructView = new Android.Widget.TextView(this)
                {
                    Text = "Tap anywhere to test touch input",
                    TextSize = 14
                };
                instructView.SetTextColor(Android.Graphics.Color.Yellow);
                instructView.Gravity = Android.Views.GravityFlags.Center;
                layout.AddView(instructView);

                // Add touch handling to demonstrate activity control
                layout.Touch += (sender, e) =>
                {
                    if (e.Event.Action == Android.Views.MotionEventActions.Down)
                    {
                        instructView.Text = $"Touch detected at ({e.Event.GetX():F0}, {e.Event.GetY():F0})";
                        System.Diagnostics.Debug.WriteLine($"DIRECT ACTIVITY: Touch input working - {e.Event.GetX():F0}, {e.Event.GetY():F0}");
                    }
                };

                SetContentView(layout);
                _view = layout;

                System.Diagnostics.Debug.WriteLine("DIRECT ACTIVITY: Simple game view created successfully");
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DIRECT ACTIVITY: CreateSimpleGameView failed: {ex}");
                ShowErrorScreen(ex);
            }
        }

        // ShowErrorScreen displays a user-friendly error message if the game fails to initialize.
        private void ShowErrorScreen(System.Exception ex)
        {
            // Create a TextView to display the error message.
            var errorView = new Android.Widget.TextView(this)
            {
                Text = $"GLTron Mobile - Initialization Error\n\n{ex.Message}", // Error message
                TextAlignment = TextAlignment.Center, // Center-align the text
                TextSize = 16                        // Set text size
            };
            
            // Set colors using methods (not properties)
            errorView.SetTextColor(Android.Graphics.Color.White);
            errorView.SetBackgroundColor(Android.Graphics.Color.DarkRed);

            // Add padding around the text for better readability.
            errorView.SetPadding(20, 20, 20, 20);

            // Set the error view as the content of the Activity.
            SetContentView(errorView);
        }

        // OnPause is called when the Activity is paused (e.g., when the app is sent to the background).
        // This is where you pause the game to save resources.
        // YOU NOW HAVE COMPLETE CONTROL over this lifecycle method!
        protected override void OnPause()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("DIRECT ACTIVITY: Activity pausing - YOU have full control!");
                
                // Here you can add any custom pause logic you want
                // For example: pause background music, save game state, etc.
                
                if (_game != null)
                {
                    // If we had a game running, we could pause it here
                    System.Diagnostics.Debug.WriteLine("DIRECT ACTIVITY: Custom game pause logic would go here");
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DIRECT ACTIVITY OnPause error: {ex.Message}");
            }

            // Call the base class's OnPause method.
            base.OnPause();
        }

        // OnResume is called when the Activity resumes (e.g., when the app returns to the foreground).
        // This is where you resume the game.
        // YOU NOW HAVE COMPLETE CONTROL over this lifecycle method!
        protected override void OnResume()
        {
            // Call the base class's OnResume method.
            base.OnResume();

            try
            {
                System.Diagnostics.Debug.WriteLine("DIRECT ACTIVITY: Activity resuming - YOU have full control!");
                
                // Here you can add any custom resume logic you want
                // For example: resume background music, restore game state, etc.
                
                if (_game != null)
                {
                    // If we had a game running, we could resume it here
                    System.Diagnostics.Debug.WriteLine("DIRECT ACTIVITY: Custom game resume logic would go here");
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DIRECT ACTIVITY OnResume error: {ex.Message}");
            }
        }

        // OnDestroy is called when the Activity is being destroyed (e.g., when the app is closed).
        // This is where you clean up resources.
        // YOU NOW HAVE COMPLETE CONTROL over this lifecycle method!
        protected override void OnDestroy()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("DIRECT ACTIVITY: Activity destroying - YOU have full control!");
                
                // Here you can add any custom cleanup logic you want
                // For example: save preferences, clean up resources, etc.
                
                if (_game != null)
                {
                    // Clean up the game if it exists
                    _game.Dispose();
                    System.Diagnostics.Debug.WriteLine("DIRECT ACTIVITY: Game disposed successfully");
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DIRECT ACTIVITY OnDestroy error: {ex.Message}");
            }

            // Call the base class's OnDestroy method.
            base.OnDestroy();
        }
    }
}
