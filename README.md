# GLTron Mobile

GLTron Mobile is a cross-platform lightcycle game built with MonoGame in C#.

> This repository uses the MonoGame projects in the solution as the **source of truth** for Android and iOS builds. The Android Studio and Xcode projects act as **wrappers** to trigger those builds and to help with IDE workflows.
>
> **Android Studio builds:** the Gradle tasks are configured to build and launch the **.NET MonoGame APK/AAB**. Java/Android APK outputs are intentionally removed to avoid confusion.
## Requirements

### Android (Android Studio / Gradle)
- Android Studio (latest stable)
- Android SDK 36+ (required)
- **JDK 17+** (Gradle 9 requires Java 17+)
- **.NET SDK 10** (required for MonoGame projects)
- `dotnet` available in PATH
- `adb` available in PATH
- Android SDK path set via `ANDROID_SDK_ROOT` or `ANDROID_HOME`

### iOS (Xcode)
- macOS with Xcode (latest stable)
- Apple developer signing configured
- **.NET SDK 10** (required for MonoGame projects)

## Install .NET 10 + MonoGame

### macOS (MacInCloud - user account only)
If you **cannot use Homebrew** or system folders, install .NET locally in your user home:

```bash
mkdir -p $HOME/dotnet
curl -L -o /tmp/dotnet-sdk-10.0.201-linux-x64.tar.gz https://builds.dotnet.microsoft.com/dotnet/Sdk/10.0.201/dotnet-sdk-10.0.201-linux-x64.tar.gz
tar zxf /tmp/dotnet-sdk-10.0.201-linux-x64.tar.gz -C $HOME/dotnet
export DOTNET_ROOT=$HOME/dotnet
export PATH=$PATH:$HOME/dotnet
```

Install MonoGame templates/tools (required for Android builds):

```bash
$DOTNET_ROOT/dotnet new install MonoGame.Templates.CSharp
$DOTNET_ROOT/dotnet workload install android
```

> macOS is required for iOS builds. Android builds also work on macOS.

### macOS (Administrator)
If you have admin privileges, you can install .NET with Homebrew:

```bash
brew install --cask dotnet-sdk
```

Install MonoGame templates/tools (required for Android builds):

```bash
dotnet new install MonoGame.Templates.CSharp
dotnet workload install android
```

### Linux (Ubuntu)
Install .NET 10 (manual tarball):

```bash
mkdir -p $HOME/dotnet
curl -L -o /tmp/dotnet-sdk-10.0.201-linux-x64.tar.gz https://builds.dotnet.microsoft.com/dotnet/Sdk/10.0.201/dotnet-sdk-10.0.201-linux-x64.tar.gz
tar zxf /tmp/dotnet-sdk-10.0.201-linux-x64.tar.gz -C $HOME/dotnet
export DOTNET_ROOT=$HOME/dotnet
export PATH=$PATH:$HOME/dotnet
```

Install MonoGame templates/tools (required for Android builds):

```bash
$DOTNET_ROOT/dotnet new install MonoGame.Templates.CSharp
$DOTNET_ROOT/dotnet workload install android
```

> Linux can build Android only (no iOS builds).

## Set SDK paths (Linux)
Add this to your shell profile (e.g. `~/.bashrc` or `~/.zshrc`):

```bash
export DOTNET_ROOT=$HOME/dotnet
export PATH="$PATH:$HOME/dotnet"
export ANDROID_SDK_ROOT=/path/to/Android/Sdk
export ANDROID_HOME=/path/to/Android/Sdk
```

Then reload your shell:
```bash
source ~/.bashrc
```
```
## Xcode Build (Wrapper)
Build iOS with the .NET project:
- `dotnet build GltronMobileGame.iOS/GltronMobileGame.iOS.csproj -c Debug -f net10.0-ios`

Then open **ios-xcode/GltronMobileGame.xcodeproj** in Xcode for signing/archiving.

> **iOS builds require an active Apple ID** with valid signing certificates and provisioning profiles.
