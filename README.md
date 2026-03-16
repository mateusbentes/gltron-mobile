# GLTron Mobile

GLTron Mobile is a cross-platform lightcycle game built with MonoGame in C#.

> This repository uses the MonoGame projects in the solution as the **source of truth** for Android and iOS builds. The Android Studio and Xcode projects act as **wrappers** to trigger those builds and to help with IDE workflows.
>
> **Android Studio builds:** the Gradle tasks are configured to build and launch the **.NET MonoGame APK/AAB**. Java/Android APK outputs are intentionally removed to avoid confusion.
## Requirements

### Android (Android Studio / Gradle)
- Android Studio (latest stable)
- Android SDK 34+
- **JDK 17+** (Gradle 9 requires Java 17+)
- **.NET SDK 9** (required for MonoGame projects)
- `dotnet` available in PATH
- `adb` available in PATH
- Android SDK path set via `ANDROID_SDK_ROOT` or `ANDROID_HOME`

### iOS (Xcode)
- macOS with Xcode (latest stable)
- Apple developer signing configured
- **.NET SDK 9** (required for MonoGame projects)

## Install .NET 9 + MonoGame

### macOS (MacInCloud - user account only)
If you **cannot use Homebrew** or system folders, install .NET locally in your user home:

```bash
mkdir -p $HOME/dotnet
curl -L https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
bash /tmp/dotnet-install.sh --channel 9.0 --install-dir $HOME/dotnet
export DOTNET="$HOME/dotnet/dotnet"
export PATH="$HOME/dotnet:$PATH"
```
```

Install MonoGame templates/tools:

```bash
$DOTNET new install MonoGame.Templates.CSharp
```

### macOS (Administrator)
If you have admin privileges, you can install .NET with Homebrew:

```bash
brew install --cask dotnet-sdk
```

Install MonoGame templates/tools:

```bash
dotnet new install MonoGame.Templates.CSharp
```

### Linux (Ubuntu)
Install .NET 9:

```bash
sudo apt-get update
sudo apt-get install -y dotnet-sdk-9.0
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
## Xcode Build (Wrapper)
Build iOS with the .NET project:
- `dotnet build GltronMobileGame.iOS/GltronMobileGame.iOS.csproj -c Debug -f net9.0-ios`

Then open **ios-xcode/GltronMobileGame.xcodeproj** in Xcode for signing/archiving.

> **iOS builds require an active Apple ID** with valid signing certificates and provisioning profiles.
