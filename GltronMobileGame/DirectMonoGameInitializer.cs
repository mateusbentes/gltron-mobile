using Android.App;
using Android.Views;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Reflection;

namespace GltronMobileGame
{
    /// <summary>
    /// Initializes MonoGame properly for direct Activity management
    /// This replaces the functionality that AndroidGameActivity normally provides
    /// </summary>
    public static class DirectMonoGameInitializer
    {
        public static bool InitializeMonoGameForActivity(Activity activity)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("DirectMonoGameInitializer: Starting MonoGame initialization...");

                // Step 1: Set up the Android platform context
                if (!SetupAndroidPlatformContext(activity))
                {
                    System.Diagnostics.Debug.WriteLine("DirectMonoGameInitializer: Failed to setup platform context");
                    return false;
                }

                // Step 2: Initialize MonoGame's Android platform
                if (!InitializeAndroidGamePlatform(activity))
                {
                    System.Diagnostics.Debug.WriteLine("DirectMonoGameInitializer: Failed to initialize Android platform");
                    return false;
                }

                System.Diagnostics.Debug.WriteLine("DirectMonoGameInitializer: MonoGame initialization completed successfully");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DirectMonoGameInitializer: Initialization failed: {ex}");
                return false;
            }
        }

        private static bool SetupAndroidPlatformContext(Activity activity)
        {
            try
            {
                // Try to set MonoGame's static activity reference
                var gameType = typeof(Game);
                
                // Method 1: Look for Activity property/field
                var activityProperty = gameType.GetProperty("Activity", BindingFlags.Static | BindingFlags.Public);
                if (activityProperty != null && activityProperty.CanWrite)
                {
                    activityProperty.SetValue(null, activity);
                    System.Diagnostics.Debug.WriteLine("DirectMonoGameInitializer: Set Game.Activity property");
                    return true;
                }

                var activityField = gameType.GetField("Activity", BindingFlags.Static | BindingFlags.Public);
                if (activityField != null)
                {
                    activityField.SetValue(null, activity);
                    System.Diagnostics.Debug.WriteLine("DirectMonoGameInitializer: Set Game.Activity field");
                    return true;
                }

                // Method 2: Look for context field
                var contextField = gameType.GetField("_context", BindingFlags.Static | BindingFlags.NonPublic);
                if (contextField != null)
                {
                    contextField.SetValue(null, activity);
                    System.Diagnostics.Debug.WriteLine("DirectMonoGameInitializer: Set Game._context field");
                    return true;
                }

                System.Diagnostics.Debug.WriteLine("DirectMonoGameInitializer: No static activity/context field found");
                return true; // Continue anyway
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DirectMonoGameInitializer: SetupAndroidPlatformContext failed: {ex}");
                return false;
            }
        }

        private static bool InitializeAndroidGamePlatform(Activity activity)
        {
            try
            {
                // Try to find and initialize AndroidGamePlatform
                var monoGameAssembly = Assembly.GetAssembly(typeof(Game));
                if (monoGameAssembly == null)
                {
                    System.Diagnostics.Debug.WriteLine("DirectMonoGameInitializer: Could not get MonoGame assembly");
                    return false;
                }

                // Look for AndroidGamePlatform class
                var platformType = monoGameAssembly.GetType("Microsoft.Xna.Framework.AndroidGamePlatform");
                if (platformType == null)
                {
                    // Try alternative names
                    platformType = monoGameAssembly.GetType("Microsoft.Xna.Framework.Android.AndroidGamePlatform");
                }

                if (platformType != null)
                {
                    // Try to find Initialize method
                    var initMethod = platformType.GetMethod("Initialize", BindingFlags.Static | BindingFlags.Public);
                    if (initMethod != null)
                    {
                        initMethod.Invoke(null, new object[] { activity });
                        System.Diagnostics.Debug.WriteLine("DirectMonoGameInitializer: Called AndroidGamePlatform.Initialize()");
                        return true;
                    }

                    // Try to create platform instance
                    var constructor = platformType.GetConstructor(new[] { typeof(Activity) });
                    if (constructor != null)
                    {
                        var platform = constructor.Invoke(new object[] { activity });
                        System.Diagnostics.Debug.WriteLine("DirectMonoGameInitializer: Created AndroidGamePlatform instance");
                        return true;
                    }
                }

                System.Diagnostics.Debug.WriteLine("DirectMonoGameInitializer: AndroidGamePlatform not found or no suitable methods");
                return true; // Continue anyway
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DirectMonoGameInitializer: InitializeAndroidGamePlatform failed: {ex}");
                return false;
            }
        }

        public static Game1 CreateGame1WithProperInitialization(Activity activity)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("DirectMonoGameInitializer: Creating Game1 with proper initialization...");

                // Initialize MonoGame platform first
                if (!InitializeMonoGameForActivity(activity))
                {
                    throw new InvalidOperationException("Failed to initialize MonoGame platform");
                }

                // Create Game1 instance
                var game = new Game1();
                
                // Register activity in services
                game.Services.AddService(typeof(Activity), activity);

                System.Diagnostics.Debug.WriteLine("DirectMonoGameInitializer: Game1 created successfully with platform initialization");
                return game;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DirectMonoGameInitializer: CreateGame1WithProperInitialization failed: {ex}");
                throw;
            }
        }
    }
}
