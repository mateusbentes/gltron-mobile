GLTron Mobile - Android and iOS build instructions (FNA)

Overview
This guide explains how to build and package GLTron Mobile for Android and iOS using FNA (FNA's Not Accurate) and .NET 9. FNA is a reimplementation of Microsoft XNA Game Studio 4.0 that provides better compatibility and easier deployment than MonoGame. The Android build works on Linux/Windows/macOS, while iOS build requires macOS with Xcode.

What you will get
- GLTron Mobile Android APK (or AAB) for Android devices
- GLTron Mobile iOS IPA for iOS devices (when built on macOS)
- FNA Android project (GltronMobileGame) with full source and direct activity management
- FNA iOS project (GltronMobileGame.iOS) ready for macOS builds
- HUD with FPS/score display, background music and sound effects
- Shared codebase between Android and iOS platforms
- Better .NET 9 compatibility and easier deployment than MonoGame

Prerequisites for Android
=========================
1) Install Android Studio and Android SDK
   - Install Android Studio and use the SDK Manager to install:
     • Android SDK Platform (API 34 recommended)
     • Android SDK Platform-Tools
     • Android SDK Build-Tools
   - Set ANDROID_HOME to your SDK location, typically:
     export ANDROID_HOME=$HOME/Android/Sdk
     export PATH="$ANDROID_HOME/platform-tools:$ANDROID_HOME/cmdline-tools/latest/bin:$PATH"
   - Alternatively, source the helper script:
     source ./scripts/setup-android-env.sh

2) Install .NET 9 SDK
   - Follow https://learn.microsoft.com/dotnet/core/install/linux
     export DOTNET_ROOT=$HOME/dotnet
     export PATH=$PATH:$HOME/dotnet
   - Verify: dotnet --info
   - Install Android workload for .NET (required for net9.0-android):
     dotnet workload install android

3) Install FNA and dependencies

    # FNA requires native libraries for audio, graphics, and input
    # On Ubuntu/Debian:
    sudo apt-get update
    sudo apt-get install libsdl2-dev libopenal-dev libtheoraplay-dev
    
    # On macOS (using Homebrew):
    brew install sdl2 openal-soft
    
    # On Windows, FNA will automatically download required DLLs
    
    # FNA is typically included as source or NuGet package in the project
    # No additional global tools needed - FNA handles content loading directly

FNA Setup for Android
====================
FNA requires additional setup for Android development:

1) Download FNA native libraries for Android
   # Use the automated script (recommended):
   ./scripts/download-fna-libs.sh
   
   # Or manually:
   # Create native libraries directory
   mkdir -p GltronMobileGame/lib/arm64-v8a
   mkdir -p GltronMobileGame/lib/armeabi-v7a
   
   # Download real FNA Android native libraries from:
   # - SDL2: https://github.com/libsdl-org/SDL/releases
   # - OpenAL: https://github.com/kcat/openal-soft/releases
   # Extract libSDL2.so, libopenal.so to the lib directories

2) Set up FNA environment variables (handled automatically by the app)
   export FNA_PLATFORM_BACKEND=SDL2
   export FNA_AUDIO_BACKEND=OpenAL
   export FNA_GRAPHICS_BACKEND=OpenGL

FNA Quick Setup
===============
Run the automated setup script:

    # Set up FNA C# dependencies
    ./scripts/setup-fna-deps.sh
    
    # This will:
    # - Download FNA C# bindings (SDL2-CS, FAudio, Theorafile)
    # - Set up FNA dependency structure
    # - Prepare for FNA builds

Android Build Steps
===================
Option A: Development builds (recommended for testing)
- Make scripts executable (once):
  chmod +x scripts/*.sh
- Clean build artifacts (if needed):
  ./scripts/clean-fna-build.sh
- Set up FNA dependencies (first time only):
  ./scripts/setup-fna-deps.sh
- Download FNA native libraries:
  ./scripts/download-fna-libs.sh
- Build debug APK:
  ./scripts/build-android.sh -c Debug
- Build release APK:
  ./scripts/build-android.sh -c Release

Option B: Production builds (for Google Play Store)
- Clean build artifacts (if needed):
  ./scripts/clean-fna-build.sh
- Set up FNA dependencies (first time only):
  ./scripts/setup-fna-deps.sh
- Download FNA native libraries:
  ./scripts/download-fna-libs.sh
- Create keystore for signing (one time only):
  ./scripts/create-keystore.sh
- Build production APK and AAB (signed):
  ./scripts/build-production-fna.sh -k gltron-release.keystore -a gltron-release-key
- Or build just AAB for Play Store:
  ./scripts/build-production-fna.sh -t aab -k gltron-release.keystore -a gltron-release-key
- Or use the dedicated AAB signing script:
  ./scripts/sign-aab.sh

Option C: Manual steps (advanced)
1) Clean build artifacts (if needed):
   ./scripts/clean-fna-build.sh
2) Set up FNA dependencies:
   ./scripts/setup-real-fna-deps.sh
3) Download FNA native libraries:
   ./scripts/download-fna-libs.sh
4) Build APK manually:
   dotnet build GltronMobileGame -c Release -f net9.0-android36.0
5) For production, add signing parameters:
   dotnet publish GltronMobileGame/GltronAndroid.csproj -c Release -f net9.0-android36.0 \
     -p:AndroidPackageFormat=aab -p:AndroidKeyStore=true \
     -p:AndroidSigningKeyStore=gltron-release.keystore \
     -p:AndroidSigningKeyAlias=gltron-release-key

iOS Build Instructions (macOS only)
===================================
Prerequisites for iOS:
1) macOS with Xcode installed
2) .NET 9 SDK with iOS workload:
   dotnet workload install ios
3) FNA native frameworks (set up automatically with setup script)

Building for iOS:
Option A: Using build scripts (recommended)
- Set up FNA dependencies (first time only):
  ./scripts/setup-fna-deps.sh
- Build for iOS Simulator: ./scripts/build-ios.sh -c Debug -p iPhoneSimulator
- Build for iOS Device: ./scripts/build-ios.sh -c Release -p iPhone

Option B: Manual dotnet commands
1) Build for iOS Simulator:
   dotnet build GltronMobileGame.iOS -c Release -f net9.0-ios /p:Platform=iPhoneSimulator
2) Build for iOS Device:
   dotnet build GltronMobileGame.iOS -c Release -f net9.0-ios /p:Platform=iPhone

Note: iOS builds are currently disabled in CI. See docs/ios-setup.md for setup instructions.

Note: iOS builds require proper Apple Developer certificates for device deployment.

Production Builds for Google Play Store
=======================================
For releasing to Google Play Store, use the production build system:

1) Create a signing keystore (one time setup):
   ./scripts/create-keystore.sh
   # Follow prompts to create gltron-release.keystore
   # Keep the keystore file and passwords safe!

2) Build signed production APK and AAB:
   ./scripts/build-production-fna.sh -k gltron-release.keystore -a gltron-release-key
   
3) Or build just AAB (recommended for Play Store):
   ./scripts/build-production-fna.sh -t aab -k gltron-release.keystore -a gltron-release-key

4) Alternative: Use dedicated AAB signing script:
   ./scripts/sign-aab.sh

Production builds include:
- Code shrinking with ProGuard
- IL trimming for smaller APK size
- Optimized FNA runtime
- Digital signing for Play Store
- Support for both APK and AAB formats

Deploying to device/emulator
============================
Android:
- Connect a device with USB debugging enabled or start an emulator.
- Install APK (example):
  adb install -r "GltronMobileGame/bin/Release/com.companyname.gltronandroid-Signed.apk"
  # Path may differ; use the path printed by the build script.

iOS:
- Use Xcode to deploy to simulator or device
- Or use command line tools with proper provisioning profiles

Troubleshooting
===============
- dotnet: command not found
  • Install .NET 9 SDK and ensure dotnet is on PATH.
- FNA dependencies missing (CS0234 errors)
  • Run ./scripts/setup-fna-deps.sh to download FNA C# bindings.
- ANDROID_HOME not set / sdkmanager/adb not found
  • Set ANDROID_HOME to your SDK path or source scripts/setup-android-env.sh.
- FNA platform initialization failed: SDL2 native library missing or incompatible
  • Run ./scripts/download-real-fna-libs.sh to download proper native libraries.
  • Ensure ANDROID_NDK_ROOT is set: export ANDROID_NDK_ROOT=/home/mateus/Android/Sdk/ndk/29.0.14206865
  • Verify libraries with ./scripts/verify-native-libs.sh
- OpenAL audio missing / OpenGL ES 3.0 not supported
  • Check AndroidManifest.xml has proper OpenGL ES fallback configuration.
  • Ensure device supports at least OpenGL ES 2.0.
- Native library architecture mismatch
  • Delete old stub libraries: rm -rf GltronMobileGame/lib/
  • Re-run ./scripts/download-real-fna-libs.sh with proper NDK setup.
- No APK/AAB after build
  • Check build output in GltronMobileGame/bin/<Config>/, review dotnet build logs for errors.
- TypeInitializationException at runtime
  • Run ./scripts/download-fna-libs.sh to set up FNA native libraries.
  • For full functionality, replace stub libraries with real SDL2/OpenAL libraries.
- Content not found at runtime
  • FNA uses raw content files - ensure PNG/OGG/TTF files exist in Content/Assets/.
- Production build signing errors
  • Verify keystore exists and passwords are correct.
- Build cache issues or GUID mismatches
  • Run ./scripts/clean-fna-build.sh to clean all build artifacts and caches.
- FNA platform initialization failed
  • Ensure FNA native libraries are properly installed with ./scripts/download-fna-libs.sh
  • Check device compatibility (OpenGL ES 3.0, ARM64/ARMv7 architecture).

Multiplatform Notes
==================
- This repo contains both Android (GltronMobileGame) and iOS (GltronMobileGame.iOS) FNA projects
- Both projects share the same game engine code (GltronMobileEngine) and content assets
- FNA uses raw content files (PNG, OGG, TTF) - no XNB conversion needed
- The original Java Android project under GlTron/ is kept for reference only
- iOS builds require macOS with Xcode and proper Apple Developer setup
- FNA provides better .NET 9 compatibility than MonoGame

Solution Files:
- GltronMobile.sln: Android-only solution (works on Linux/Windows/macOS)
- GltronMobile.Full.sln: Full multiplatform solution including iOS (macOS only)

Platform Detection:
Run ./scripts/detect-platform.sh to see available build options for your platform.

FNA Features
============
This project uses FNA (FNA's Not Accurate) instead of MonoGame for better compatibility:

✅ **Better .NET 9 Support**: FNA works seamlessly with .NET 9
✅ **Raw Content Loading**: No XNB conversion needed - use PNG, OGG, TTF directly
✅ **Native Integration**: Direct SDL2/OpenAL integration for better performance
✅ **Easier Deployment**: Simplified build process and dependency management
✅ **XNA4 Compatibility**: Drop-in replacement for XNA/MonoGame code
✅ **Cross-Platform**: Same codebase works on Android, iOS, and desktop

Support
=======
- For build issues, check the troubleshooting section above
- For FNA-specific questions, see: https://fna-xna.github.io/docs/
- For production builds, ensure you have a valid keystore for signing
- iOS builds require macOS with Xcode and the iOS workload installed
