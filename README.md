# GLTron Mobile

GLTron Mobile is a cross-platform lightcycle game built with MonoGame in C#.

> This repository uses the MonoGame projects in the solution as the **source of truth** for Android and iOS builds. The Android Studio and Xcode projects act as **wrappers** to trigger those builds and to help with IDE workflows.

## Requirements

### Android (Android Studio / Gradle)
- Android Studio (latest stable)
- Android SDK 34+
- JDK 11
- .NET SDK installed (same as used by the solution)
- `dotnet` available in PATH (or set `DOTNET` env / `-PdotnetPath=/path/to/dotnet`)
- `adb` available in PATH

### iOS (Xcode)
- macOS with Xcode (latest stable)
- Apple developer signing configured
- .NET SDK installed (same as used by the solution)

## Android Studio Build (Wrapper)
1. Open **android-studio/** in Android Studio.
2. Let Gradle sync.
3. Run configuration **GltronMobile (Gradle)**.

This will:
- Run `dotnet build GltronMobileGame/GltronAndroid.csproj -c Debug`
- Sync XNB/assemblies into `android-studio/app/src/main/assets`
- Install the generated APK via `adb install -r`

The APK is produced by .NET in:
- `GltronMobileGame/bin/Debug/net8.0-android/gltron.org.gltronmobile.apk`

## Xcode Build (Wrapper)
Build iOS with the .NET project:
- `dotnet build GltronMobileGame.iOS/GltronMobileGame.iOS.csproj -c Debug -f net8.0-ios`

Then open **ios-xcode/GltronMobileGame.xcodeproj** in Xcode for signing/archiving.

> The Xcode project is still a minimal shell; the actual build output comes from the .NET project.

## Game Code
The core game logic lives in:
- `GltronMobileEngine/`
- `GltronMobileGame/`

## License
See COPYING.
