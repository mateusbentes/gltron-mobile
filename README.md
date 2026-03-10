# GLTron Mobile

GLTron Mobile is a cross-platform lightcycle game built with MonoGame in C#.

> **Note**: The Android Studio and Xcode projects in this repository are **best-effort stubs**. They include simple sync hooks for assets/assemblies, but **do not yet embed MonoGame runtimes**. A fully working native Gradle/Xcode build requires bringing in the Mono runtime and MonoGame native libraries.

## Requirements

### Android (Android Studio / Gradle)
- Android Studio (latest stable)
- Android SDK 34+ (compileSdk 34)
- JDK 11

### iOS (Xcode)
- macOS with Xcode (latest stable)
- Apple developer signing configured

## Android Studio (Gradle) Build (Best-Effort)
1. Build the Android .NET project first so assets/assemblies exist:
   - `dotnet build GltronMobileGame/GltronAndroid.csproj -c Debug`
2. Open **android-studio/** in Android Studio.
3. Let Gradle sync.
4. Run the configuration **GltronMobile (Gradle)** (assembleDebug).

The Gradle project syncs:
- XNB content from `GltronMobileGame/Content/bin/Android/Content`
- Managed assemblies from `GltronMobileGame/bin/Debug/net8.0-android`

> You must still wire the Mono runtime / MonoGame native libs for a runnable APK.

## Xcode Build (Best-Effort)
1. Build the iOS .NET project first to produce assemblies/content:
   - `dotnet build GltronMobileGame.iOS/GltronMobileGame.iOS.csproj -c Debug -f net8.0-ios`
2. Open **ios-xcode/GltronMobileGame.xcodeproj** in Xcode.
3. Configure signing in Xcode.
4. Build/Run.

> The Xcode project is a minimal shell and does not yet link MonoGame runtime.

## Game Code
The core game logic lives in:
- `GltronMobileEngine/`
- `GltronMobileGame/`

These are MonoGame C# projects used in the original build system.

## License
See COPYING.
