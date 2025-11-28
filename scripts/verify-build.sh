#!/usr/bin/env bash
# Verify Android build setup and content pipeline for the new solution-based architecture
# Usage: ./scripts/verify-build.sh [-v]

set -euo pipefail

VERBOSE=false

while getopts ":v" opt; do
  case $opt in
    v) VERBOSE=true ;;
    *) echo "Unknown option -$OPTARG" ; exit 1 ;;
  esac
done

log() {
    if [ "$VERBOSE" = true ]; then
        echo "  $1"
    fi
}

echo "🔍 GLTron Mobile - Verifying Android build setup..."

# 1) Check .NET environment
echo "1. Checking .NET environment..."
if command -v dotnet &>/dev/null; then
    DOTNET_VERSION=$(dotnet --version)
    echo "  ✓ .NET found: $DOTNET_VERSION"
else
    echo "  ❌ .NET not found. Please install .NET 8 SDK"
    exit 1
fi

# 2) Check Android workload
echo "2. Checking Android workload..."
if dotnet workload list | grep -q android; then
    echo "  ✓ Android workload installed"
else
    echo "  ⚠️  Android workload not found. Install with: dotnet workload install android"
fi

# 3) Check MGCB tool
echo "3. Checking MGCB tool..."
if command -v mgcb &>/dev/null || command -v dotnet-mgcb &>/dev/null || command -v mgcb-editor &>/dev/null; then
    echo "  ✓ MGCB tool found"
else
    echo "  ⚠️  MGCB tool not found. Install with: dotnet tool install -g dotnet-mgcb-editor"
fi

# 4) Check solution and projects
echo "4. Checking solution and projects..."
if [ -f "GltronMobile.sln" ]; then
    echo "  ✓ Android solution found: GltronMobile.sln"
else
    echo "  ❌ Android solution not found (GltronMobile.sln)"
    exit 1
fi

if [ -f "GltronMobile.Full.sln" ]; then
    echo "  ✓ Full solution found: GltronMobile.Full.sln (includes iOS)"
else
    echo "  ⚠️  Full solution not found (iOS builds will not be available)"
fi

if [ -f "GltronMobileEngine/GltronMobileEngine.csproj" ]; then
    echo "  ✓ Engine project found"
else
    echo "  ❌ Engine project not found"
    exit 1
fi

if [ -f "GltronMobileGame/GltronAndroid.csproj" ]; then
    echo "  ✓ Android game project found"
else
    echo "  ❌ Android game project not found"
    exit 1
fi

# 5) Check content
echo "5. Checking content..."
CONTENT_DIR="GltronMobileGame/Content"
if [ -f "$CONTENT_DIR/Content.mgcb" ]; then
    echo "  ✓ Content.mgcb found"
else
    echo "  ❌ Content.mgcb not found at $CONTENT_DIR/Content.mgcb"
    exit 1
fi

if [ -f "$CONTENT_DIR/Fonts/Default.spritefont" ]; then
    echo "  ✓ Default font found"
else
    echo "  ❌ Default font missing"
fi

# Audio files count (informational)
AUDIO_COUNT=0
for audio in "Assets/game_engine.ogg" "Assets/game_crash.ogg" "Assets/song_revenge_of_cats.ogg"; do
    if [ -f "$CONTENT_DIR/$audio" ]; then
        log "Found $audio"
        ((AUDIO_COUNT++))
    fi
done
echo "  ✓ Found $AUDIO_COUNT/3 audio files"

# 6) Check Activity1.cs configuration
echo "6. Checking Activity configuration..."
if [ -f "GltronMobileGame/Activity1.cs" ]; then
    if grep -q "AndroidGameActivity" "GltronMobileGame/Activity1.cs"; then
        echo "  ✓ Activity1 properly configured"
    else
        echo "  ❌ Activity1 not properly configured"
    fi
else
    echo "  ❌ Activity1.cs not found"
fi

# 7) Test content build (dry run)
echo "7. Testing content build..."
if command -v mgcb &>/dev/null; then
    log "Testing MGCB build..."
    if mgcb -@:"$CONTENT_DIR/Content.mgcb" /platform:Android /outputDir:"$CONTENT_DIR/bin/Android" /intermediateDir:"$CONTENT_DIR/obj/Android" /quiet; then
        echo "  ✓ Content build test successful"
        if [ -d "$CONTENT_DIR/bin/Android" ]; then
            XNB_COUNT=$(find "$CONTENT_DIR/bin/Android" -name "*.xnb" | wc -l)
            echo "  ✓ Generated $XNB_COUNT XNB files"
        fi
    else
        echo "  ❌ Content build test failed"
    fi
else
    echo "  ⚠️  Skipping content build test (MGCB not available)"
fi

echo ""
echo "🎯 Build verification complete!"
echo ""
echo "Next steps:"
echo "  1. If any ❌ errors above, fix them first"
echo "  2. Run: ./scripts/clean-build.sh -a"
echo "  3. Run: ./scripts/build-android.sh -c Release"
echo "  4. Deploy with: adb install -r path/to/generated.apk"
