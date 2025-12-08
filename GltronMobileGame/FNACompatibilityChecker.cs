using System;
using Android.Content;
using Android.Content.PM;

namespace GltronMobileGame
{
    /// <summary>
    /// Checks if the Android device supports FNA requirements
    /// </summary>
    public static class FNACompatibilityChecker
    {
        /// <summary>
        /// Checks if the device meets FNA requirements
        /// </summary>
        public static FNACompatibilityResult CheckCompatibility(Context context)
        {
            var result = new FNACompatibilityResult();
            
            try
            {
                FNAHelper.LogInfo("Checking FNA compatibility...");
                
                // Check OpenGL ES version
                result.OpenGLESSupported = CheckOpenGLESSupport(context);
                
                // Check architecture
                result.ArchitectureSupported = CheckArchitectureSupport();
                
                // Check Android version
                result.AndroidVersionSupported = CheckAndroidVersionSupport();
                
                // Audio support - assume true for most devices
                result.AudioSupported = true;
                
                result.IsCompatible = result.OpenGLESSupported && 
                                    result.ArchitectureSupported && 
                                    result.AndroidVersionSupported && 
                                    result.AudioSupported;
                
                LogCompatibilityResult(result);
                
                return result;
            }
            catch (System.Exception ex)
            {
                FNAHelper.LogError($"Compatibility check failed: {ex}");
                result.IsCompatible = false;
                result.ErrorMessage = ex.Message;
                return result;
            }
        }
        
        private static bool CheckOpenGLESSupport(Context context)
        {
            try
            {
                var packageManager = context.PackageManager;
                
                // Check for OpenGL ES 3.0 support through feature info
                var featureInfos = packageManager.GetSystemAvailableFeatures();
                bool hasOpenGLES30 = false;
                
                foreach (var featureInfo in featureInfos)
                {
                    if (featureInfo.Name == null) // OpenGL ES feature
                    {
                        // OpenGL ES 3.0 = 0x30000
                        if (featureInfo.ReqGlEsVersion >= 0x30000)
                        {
                            hasOpenGLES30 = true;
                            FNAHelper.LogInfo($"OpenGL ES version: {featureInfo.ReqGlEsVersion:X}");
                            break;
                        }
                    }
                }
                
                FNAHelper.LogInfo($"OpenGL ES 3.0 support: {hasOpenGLES30}");
                return hasOpenGLES30;
            }
            catch (System.Exception ex)
            {
                FNAHelper.LogError($"OpenGL ES check failed: {ex}");
                return false;
            }
        }
        
        private static bool CheckArchitectureSupport()
        {
            try
            {
                var supportedAbis = Android.OS.Build.SupportedAbis;
                bool hasArm64 = false;
                bool hasArmv7 = false;
                
                foreach (var abi in supportedAbis)
                {
                    if (abi == "arm64-v8a")
                        hasArm64 = true;
                    else if (abi == "armeabi-v7a")
                        hasArmv7 = true;
                }
                
                bool supported = hasArm64 || hasArmv7;
                FNAHelper.LogInfo($"Architecture support: {supported} (ARM64: {hasArm64}, ARMv7: {hasArmv7})");
                return supported;
            }
            catch (System.Exception ex)
            {
                FNAHelper.LogError($"Architecture check failed: {ex}");
                return false;
            }
        }
        
        private static bool CheckAndroidVersionSupport()
        {
            try
            {
                // FNA requires Android API 24+ (Android 7.0)
                bool supported = Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.N;
                FNAHelper.LogInfo($"Android version support: {supported} (API {Android.OS.Build.VERSION.SdkInt})");
                return supported;
            }
            catch (System.Exception ex)
            {
                FNAHelper.LogError($"Android version check failed: {ex}");
                return false;
            }
        }
        

        
        private static void LogCompatibilityResult(FNACompatibilityResult result)
        {
            FNAHelper.LogInfo("=== FNA Compatibility Check Results ===");
            FNAHelper.LogInfo($"Overall Compatible: {result.IsCompatible}");
            FNAHelper.LogInfo($"OpenGL ES 3.0: {result.OpenGLESSupported}");
            FNAHelper.LogInfo($"Architecture: {result.ArchitectureSupported}");
            FNAHelper.LogInfo($"Android Version: {result.AndroidVersionSupported}");
            FNAHelper.LogInfo($"Audio: {result.AudioSupported}");
            if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                FNAHelper.LogError($"Error: {result.ErrorMessage}");
            }
            FNAHelper.LogInfo("=== End Compatibility Check ===");
        }
    }
    
    /// <summary>
    /// Result of FNA compatibility check
    /// </summary>
    public class FNACompatibilityResult
    {
        public bool IsCompatible { get; set; }
        public bool OpenGLESSupported { get; set; }
        public bool ArchitectureSupported { get; set; }
        public bool AndroidVersionSupported { get; set; }
        public bool AudioSupported { get; set; }
        public string ErrorMessage { get; set; }
    }
}
