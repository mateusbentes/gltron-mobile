# GLTron Mobile

GLTron Mobile is a cross-platform lightcycle game built with MonoGame in C#.

> This repository uses the MonoGame projects in the solution as the **source of truth** for Android and iOS builds. The Android Studio and Xcode projects act as **wrappers** to trigger those builds and to help with IDE workflows.

## Requirements

### Android (Android Studio / Gradle)
- Android Studio (latest stable)
- Android SDK 34+
- **JDK 17+** (Gradle 9 requires Java 17+)
- **.NET SDK 8** (required for MonoGame projects)
- `dotnet` available in PATH
- `adb` available in PATH
- Android SDK path set via `ANDROID_SDK_ROOT` or `ANDROID_HOME`

### iOS (Xcode)
- macOS with Xcode (latest stable)
- Apple developer signing configured
- **.NET SDK 8** (required for MonoGame projects)

## Install .NET 8 + MonoGame

### macOS (MacInCloud)
1. Install .NET 8:

```bash
brew install --cask dotnet-sdk
```

2. Install MonoGame templates/tools:

```bash
dotnet new install MonoGame.Templates.CSharp
```

3. Verify:

```bash
dotnet --version
```

### Linux (Ubuntu)
Install .NET 8:

```bash
sudo apt-get update
sudo apt-get install -y dotnet-sdk-8.0
```

Install MonoGame templates/tools:

```bash
dotnet new install MonoGame.Templates.CSharp
```

## Set SDK paths (Linux)
Add this to your shell profile (e.g. `~/.bashrc` or `~/.zshrc`):

```bash
export DOTNET=/path/to/dotnet
export PATH="/path/to:$PATH"
export ANDROID_SDK_ROOT=/path/to/Android/Sdk
export ANDROID_HOME=/path/to/Android/Sdk
```

Then reload your shell:
```bash
source ~/.bashrc
```

## Android Studio Build (Wrapper)
1. Open **android-studio/** in Android Studio.
2. Let Gradle sync.
3. Run configuration **GltronMobile (Gradle)**.

This will:
- Run `dotnet build GltronMobileGame/GltronAndroid.csproj -c Debug`
- Sync XNB/assemblies into `android-studio/app/src/main/assets`
- Install the generated APK via `adb install -r`
- Launch the real MonoGame activity via `adb shell am start -n gltron.org.gltronmobile/crc6407c82ebe7ef5f924.Activity1`

The APK is produced by .NET in:
- `GltronMobileGame/bin/Debug/net8.0-android/` (APK name may vary)

## Xcode Build (Wrapper)
Build iOS with the .NET project:
- `dotnet build GltronMobileGame.iOS/GltronMobileGame.iOS.csproj -c Debug -f net8.0-ios`

Then open **ios-xcode/GltronMobileGame.xcodeproj** in Xcode for signing/archiving.

> If Xcode reports a script phase error, ensure `dotnet` is available in PATH or set `DOTNET` in Xcode build environment.
See COPYING.
