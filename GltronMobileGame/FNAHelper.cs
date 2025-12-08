using System;

namespace GltronMobileGame
{
    /// <summary>
    /// FNA initialization helper for Android and iOS
    /// Handles FNA environment setup for mobile platforms
    /// </summary>
    public static class FNAHelper
    {
        /// <summary>
        /// Sets up FNA environment variables for mobile platforms
        /// </summary>
        public static void SetupFNAEnvironment()
        {
            try
            {
                LogInfo("Setting up FNA environment variables...");
                
                // Core FNA backend configuration
                System.Environment.SetEnvironmentVariable("FNA_PLATFORM_BACKEND", "SDL2");
                System.Environment.SetEnvironmentVariable("FNA_AUDIO_BACKEND", "OpenAL");
                System.Environment.SetEnvironmentVariable("FNA_GRAPHICS_BACKEND", "OpenGL");
                
#if ANDROID
                // Android-specific OpenGL settings
                System.Environment.SetEnvironmentVariable("FNA_OPENGL_FORCE_ES3", "1");
                System.Environment.SetEnvironmentVariable("FNA_OPENGL_FORCE_COMPATIBILITY_PROFILE", "0");
                
                // Android touch/mouse settings
                System.Environment.SetEnvironmentVariable("SDL_ANDROID_SEPARATE_MOUSE_AND_TOUCH", "1");
                System.Environment.SetEnvironmentVariable("SDL_TOUCH_MOUSE_EVENTS", "0");
                
                LogInfo("Android-specific FNA settings applied");
#elif IOS
                // iOS-specific settings
                System.Environment.SetEnvironmentVariable("FNA_OPENGL_FORCE_ES3", "1");
                System.Environment.SetEnvironmentVariable("FNA_OPENGL_FORCE_COMPATIBILITY_PROFILE", "0");
                
                LogInfo("iOS-specific FNA settings applied");
#endif
                
                LogInfo("FNA environment variables set successfully");
            }
            catch (System.Exception ex)
            {
                LogError($"FNA environment setup failed: {ex}");
                throw; // Re-throw as this is critical for FNA initialization
            }
        }

        /// <summary>
        /// Mobile platform logging helper
        /// </summary>
        public static void LogInfo(string message)
        {
            System.Diagnostics.Debug.WriteLine($"GLTRON-FNA: {message}");
            
#if ANDROID
            try { Android.Util.Log.Info("GLTRON-FNA", message); } catch { }
#elif IOS
            try { Foundation.NSLog($"GLTRON-FNA: {message}"); } catch { }
#endif
        }

        /// <summary>
        /// Mobile platform error logging helper
        /// </summary>
        public static void LogError(string message)
        {
            System.Diagnostics.Debug.WriteLine($"GLTRON-FNA: ERROR - {message}");
            
#if ANDROID
            try { Android.Util.Log.Error("GLTRON-FNA", message); } catch { }
#elif IOS
            try { Foundation.NSLog($"GLTRON-FNA: ERROR - {message}"); } catch { }
#endif
        }

        /// <summary>
        /// Gets current mobile platform
        /// </summary>
        public static string GetPlatform()
        {
#if ANDROID
            return "Android";
#elif IOS
            return "iOS";
#else
            return "Unknown";
#endif
        }
    }
}
