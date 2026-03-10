# GLTron Mobile

GLTron Mobile is a cross-platform lightcycle game built with MonoGame in C#.

> **Note**: The Android Studio and Xcode projects in this repository are **stub templates** created to give IDEs a starting point. They are **not a complete MonoGame build pipeline**. You will need to wire MonoGame frameworks/bindings into these projects to build successfully.

## Requirements

### Android (Android Studio / Gradle)
- Android Studio (latest stable)
- Android SDK 34+ (compileSdk 34)
- JDK 11
- Gradle (Android Studio manages this)

### iOS (Xcode)
- macOS with Xcode (latest stable)
- Apple developer signing configured

## Android Studio (Gradle) Build (Stub)
1. Open **android-studio/** in Android Studio.
2. Let Gradle sync.
3. Run the configuration **GltronMobile (Gradle)** (assembleDebug).

> The project contains placeholder Activity/Manifest and no MonoGame integration yet.

## Xcode Build (Stub)
1. Open **ios-xcode/GltronMobileGame.xcodeproj** in Xcode.
2. Configure signing in Xcode.
3. Build/Run.

> The Xcode project is a minimal shell and does not yet link MonoGame.

## Game Code
The core game logic lives in:
- `GltronMobileEngine/`
- `GltronMobileGame/`

These are MonoGame C# projects used in the original build system.

## License
See COPYING.
