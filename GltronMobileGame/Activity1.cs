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
                
                // Check device compatibility first (non-blocking)
                try
                {
                    var compatibilityResult = FNACompatibilityChecker.CheckCompatibility(this);
                    if (!compatibilityResult.IsCompatible)
                    {
                        FNAHelper.LogError($"Device compatibility warning: OpenGL ES 3.0: {compatibilityResult.OpenGLESSupported}, Architecture: {compatibilityResult.ArchitectureSupported}, Android Version: {compatibilityResult.AndroidVersionSupported}");
                        // Continue anyway - let FNA decide if it can work
                    }
                    else
                    {
                        FNAHelper.LogInfo("Device compatibility check passed");
                    }
                }
                catch (System.Exception ex)
                {
                    FNAHelper.LogError($"Compatibility check failed, continuing anyway: {ex.Message}");
                }
                
                // CRITICAL: Set up FNA environment BEFORE calling base.OnCreate
                FNAHelper.SetupFNAEnvironment();
                
                // Call base.OnCreate
                base.OnCreate(bundle);
                FNAHelper.LogInfo("Base OnCreate completed");
                
                // CRITICAL: Initialize FNA platform for Android
                InitializeFNAPlatform();
                
                // Create the game instance
                FNAHelper.LogInfo("Creating Game1 instance...");
                _game = new Game1();
                FNAHelper.LogInfo("Game1 instance created successfully");
                
                // Start the game loop
                FNAHelper.LogInfo("Starting FNA game loop...");
                _game.Run();
                
                FNAHelper.LogInfo("FNA Activity initialized successfully!");
            }
            catch (System.Exception ex)
            {
                FNAHelper.LogError("=== FNA INITIALIZATION EXCEPTION ===");
                FNAHelper.LogError($"EXCEPTION TYPE: {ex.GetType().FullName}");
                FNAHelper.LogError($"EXCEPTION MESSAGE: {ex.Message}");
                if (ex.InnerException != null)
                {
                    FNAHelper.LogError($"INNER EXCEPTION: {ex.InnerException.GetType().FullName}");
                    FNAHelper.LogError($"INNER MESSAGE: {ex.InnerException.Message}");
                }
                FNAHelper.LogError($"EXCEPTION STACK: {ex.StackTrace}");
                
                ShowErrorScreen(ex);
            }
        }

        private void InitializeFNAPlatform()
        {
            try
            {
                FNAHelper.LogInfo("Initializing FNA platform for Android...");
                
                // Check if native libraries are available
                CheckNativeLibraries();
                
                // Initialize FNA platform manually if needed
                // This ensures FNA knows about the Android context
                var fnaLoggerType = System.Type.GetType("Microsoft.Xna.Framework.FNALoggerEXT, FNA");
                if (fnaLoggerType != null)
                {
                    var logInfoMethod = fnaLoggerType.GetMethod("LogInfo", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                    logInfoMethod?.Invoke(null, new object[] { "FNA Android platform initializing..." });
                }
                
                FNAHelper.LogInfo("FNA platform initialization completed");
            }
            catch (System.Exception ex)
            {
                FNAHelper.LogError($"FNA platform initialization failed: {ex}");
                throw new System.InvalidOperationException($"FNA platform initialization failed: {ex.Message}", ex);
            }
        }

        private void CheckNativeLibraries()
        {
            try
            {
                FNAHelper.LogInfo("Checking FNA native libraries...");
                
                // Check if we can load SDL2
                try
                {
                    System.Runtime.InteropServices.NativeLibrary.TryLoad("SDL2", out var sdl2Handle);
                    if (sdl2Handle != System.IntPtr.Zero)
                    {
                        FNAHelper.LogInfo("SDL2 library found");
                        System.Runtime.InteropServices.NativeLibrary.Free(sdl2Handle);
                    }
                    else
                    {
                        FNAHelper.LogError("SDL2 library not found - using placeholder");
                    }
                }
                catch (System.Exception ex)
                {
                    FNAHelper.LogError($"SDL2 check failed: {ex.Message}");
                }
                
                // Check if we can load OpenAL
                try
                {
                    System.Runtime.InteropServices.NativeLibrary.TryLoad("openal", out var openalHandle);
                    if (openalHandle != System.IntPtr.Zero)
                    {
                        FNAHelper.LogInfo("OpenAL library found");
                        System.Runtime.InteropServices.NativeLibrary.Free(openalHandle);
                    }
                    else
                    {
                        FNAHelper.LogError("OpenAL library not found - using placeholder");
                    }
                }
                catch (System.Exception ex)
                {
                    FNAHelper.LogError($"OpenAL check failed: {ex.Message}");
                }
                
                FNAHelper.LogInfo("Native library check completed");
            }
            catch (System.Exception ex)
            {
                FNAHelper.LogError($"Native library check failed: {ex}");
            }
        }



        private void ShowErrorScreen(System.Exception ex)
        {
            try
            {
                var errorView = new Android.Widget.TextView(this);
                
                string errorDetails = GetDetailedErrorMessage(ex);
                
                errorView.Text = $"GLTron Mobile - FNA Initialization Error\n\n" +
                               $"Error: {ex.GetType().Name}\n" +
                               $"Message: {ex.Message}\n\n" +
                               errorDetails + "\n\n" +
                               $"Device Info:\n" +
                               $"• Android {Android.OS.Build.VERSION.Release} (API {Android.OS.Build.VERSION.SdkInt})\n" +
                               $"• Model: {Android.OS.Build.Model}\n\n" +
                               $"Please restart the application.\n" +
                               $"If the problem persists, your device may not support FNA.";
                               
                errorView.SetTextColor(Android.Graphics.Color.White);
                errorView.SetBackgroundColor(Android.Graphics.Color.DarkRed);
                errorView.Gravity = Android.Views.GravityFlags.Center;
                errorView.SetPadding(20, 20, 20, 20);
                errorView.TextSize = 12f;
                SetContentView(errorView);
                
                FNAHelper.LogError("FNA error view displayed");
            }
            catch (System.Exception ex2)
            {
                FNAHelper.LogError($"Failed to show error view: {ex2}");
            }
        }

        private string GetDetailedErrorMessage(System.Exception ex)
        {
            if (ex is System.TypeInitializationException)
            {
                return "FNA Platform Initialization Failed:\n" +
                       "• SDL2 native library missing or incompatible\n" +
                       "• OpenAL audio library missing\n" +
                       "• OpenGL ES 3.0 not supported\n" +
                       "• Native library architecture mismatch";
            }
            else if (ex.Message.Contains("SDL"))
            {
                return "SDL2 Library Issue:\n" +
                       "• SDL2 native library not found\n" +
                       "• Incompatible SDL2 version\n" +
                       "• Missing ARM64/ARMv7 libraries";
            }
            else if (ex.Message.Contains("OpenAL") || ex.Message.Contains("Audio"))
            {
                return "Audio System Issue:\n" +
                       "• OpenAL library not found\n" +
                       "• Audio permissions missing\n" +
                       "• Audio hardware not supported";
            }
            else if (ex.Message.Contains("OpenGL") || ex.Message.Contains("Graphics"))
            {
                return "Graphics System Issue:\n" +
                       "• OpenGL ES 3.0 not supported\n" +
                       "• Graphics driver incompatible\n" +
                       "• Hardware acceleration disabled";
            }
            else
            {
                return "FNA Requirements:\n" +
                       "• OpenGL ES 3.0 support\n" +
                       "• SDL2 native libraries\n" +
                       "• OpenAL audio support\n" +
                       "• ARM64 or ARMv7 architecture";
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
