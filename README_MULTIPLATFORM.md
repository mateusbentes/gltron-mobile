GLTron Mobile - Android and iOS build instructions (MonoGame)

Overview
This guide explains how to build and package GLTron Mobile for Android and iOS using MonoGame and .NET 8. The Android build works on Linux/Windows/macOS, while iOS build requires macOS with Xcode. It uses ready-to-run scripts placed under scripts/ for Android.

What you will get
- GLTron Mobile Android APK (or AAB) for Android devices
- GLTron Mobile iOS IPA for iOS devices (when built on macOS)
- MonoGame Android project (GltronMobileGame) with full source
- MonoGame iOS project (GltronMobileGame.iOS) ready for macOS builds
- HUD with FPS/score display, background music and sound effects
- Shared codebase between Android and iOS platforms

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

2) Install .NET 8 SDK
   - Follow https://learn.microsoft.com/dotnet/core/install/linux
     export DOTNET_ROOT=$HOME/dotnet
     export PATH=$PATH:$HOME/dotnet
   - Verify: dotnet --info
   - Install Android workload for .NET (required for net8.0-android):
     dotnet workload install android

3) Install MGCB command-line tool and MonoGame templates

    # Install MGCB first so scripts can call `mgcb`
    dotnet tool install -g dotnet-mgcb
    # Ensure .NET global tools (like dotnet-mgcb) are available
    export PATH="$PATH:/home/mateus/.dotnet/tools"
    # Optional: MGCB Editor UI and MonoGame templates
    dotnet tool install -g dotnet-mgcb-editor
    dotnet new --install MonoGame.Templates.CSharp
    # ensure global tools are on PATH (add to your shell profile)
    export PATH="$HOME/.dotnet/tools:$PATH"
    # verify MGCB is available
    mgcb --version

Android Build Steps
===================
Option A: One-shot end-to-end script (recommended)
- Make scripts executable (once):
  chmod +x scripts/*.sh
- Verify setup: ./scripts/verify-build.sh
- Run the app builder (builds with Release configuration by default):
  ./scripts/build-android-app.sh
- On success, the script prints the APK/AAB path and an adb install command.

Option B: Manual steps (advanced)
1) Create a MonoGame Android project:
   dotnet new mgandroid -n GltronMobileGame
2) Copy game code and content from GltronMobileEngine into GltronMobileGame
   - Files: Game1.cs, GLTronGame.cs, Player.cs, Segment.cs, Vec.cs
   - Folders: Video/*, Sound/*, Content/*
3) Ensure Content/Content.mgcb contains font/audio entries (already configured in this repo) and is built for Android
   - Build manually:
     mgcb -@:"GltronMobileGame/Content/Content.mgcb" /platform:Android \
          /outputDir:"GltronMobileGame/Content/bin/Android" \
          /intermediateDir:"GltronMobileGame/Content/obj/Android"
4) Build APK:
   dotnet build GltronMobileGame -c Release -f net8.0-android

iOS Build Instructions (macOS only)
===================================
Prerequisites for iOS:
1) macOS with Xcode installed
2) .NET 8 SDK with iOS workload:
   dotnet workload install ios
3) MonoGame templates and MGCB tool (same as Android prerequisites)

Building for iOS:
Option A: Using build scripts (recommended)
- Verify iOS setup: ./scripts/verify-ios.sh
- Build for iOS Simulator: ./scripts/build-ios-app.sh -p iPhoneSimulator
- Build for iOS Device: ./scripts/build-ios-app.sh -p iPhone

Option B: Manual dotnet commands
1) Build for iOS Simulator:
   dotnet build GltronMobileGame.iOS -c Release -f net8.0-ios /p:Platform=iPhoneSimulator
2) Build for iOS Device:
   dotnet build GltronMobileGame.iOS -c Release -f net8.0-ios /p:Platform=iPhone

Note: iOS builds require proper Apple Developer certificates for device deployment.

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
  • Install .NET 8 SDK and ensure dotnet is on PATH.
- mgcb or mgcb-editor not found
  • Install MGCB editor tool and add ~/.dotnet/tools to PATH.
- ANDROID_HOME not set / sdkmanager/adb not found
  • Set ANDROID_HOME to your SDK path or source scripts/setup-android-env.sh.
- No APK/AAB after build
  • Check build output in GltronMobileGame/bin/<Config>/, review dotnet build logs for errors.
- Content not found at runtime
  • Ensure Content.mgcb built for platform and content files exist under Content/bin/<Platform>.

Multiplatform Notes
==================
- This repo contains both Android (GltronMobileGame) and iOS (GltronMobileGame.iOS) MonoGame projects
- Both projects share the same game engine code (GltronMobileEngine) and content assets
- The original Java Android project under GlTron/ is kept for reference only
- iOS builds require macOS with Xcode and proper Apple Developer setup
- Content pipeline automatically builds for the target platform (Android/iOS)

Solution Files:
- GltronMobile.sln: Android-only solution (works on Linux/Windows/macOS)
- GltronMobile.Full.sln: Full multiplatform solution including iOS (macOS only)

Platform Detection:
Run ./scripts/detect-platform.sh to see available build options for your platform.

Support
=======
If you want, ask me to run the one-shot builder once your Android SDK and .NET are installed, and I'll verify the APK generation for you. For iOS builds, ensure you're on macOS with the iOS workload installed.
