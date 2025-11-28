#!/usr/bin/env bash
# Verify Android build setup and content pipeline
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

echo "🔍 Verifying Android build setup..."

# Check .NET environment
echo "1. Checking .NET environment..."
if command -v dotnet &>/dev/null; then
    DOTNET_VERSION=$(dotnet --version)
    echo "  ✓ .NET found: $DOTNET_VERSION"
else
    echo "  ❌ .NET not found. Please install .NET 8 SDK"
    exit 1
fi

# Check Android workload
echo "2. Checking Android workload..."
if dotnet workload list | grep -q android; then
    echo "  ✓ Android workload installed"
else
    echo "  ⚠️  Android workload not found. Install with: dotnet workload install android"
fi

# Check MGCB tool
echo "3. Checking MGCB tool..."
if command -v mgcb &>/dev/null || command -v dotnet-mgcb &>/dev/null; then
    echo "  ✓ MGCB tool found"
else
    echo "  ⚠️  MGCB tool not found. Install with: dotnet tool install -g dotnet-mgcb"
fi

# Check project structure
echo "4. Checking project structure..."
if [ -f "GltronAndroid/GltronAndroid.csproj" ]; then
    echo "  ✓ Android project found"
else
    echo "  ❌ Android project not found"
    exit 1
fi

if [ -f "GltronAndroid/Content/Content.mgcb" ]; then
    echo "  ✓ Content.mgcb found"
    
    # Check platform setting
    if grep -q "/platform:Android" "GltronAndroid/Content/Content.mgcb"; then
        echo "  ✓ Content.mgcb configured for Android"
    else
        echo "  ❌ Content.mgcb not configured for Android platform"
        exit 1
    fi
else
    echo "  ❌ Content.mgcb not found"
    exit 1
fi

# Check essential content files
echo "5. Checking content files..."
CONTENT_DIR="GltronAndroid/Content"

if [ -f "$CONTENT_DIR/Fonts/Default.spritefont" ]; then
    echo "  ✓ Default font found"
else
    echo "  ❌ Default font missing"
fi

# Check for audio files
AUDIO_COUNT=0
for audio in "Assets/game_engine.ogg" "Assets/game_crash.ogg" "Assets/song_revenge_of_cats.ogg"; do
    if [ -f "$CONTENT_DIR/$audio" ]; then
        log "Found $audio"
        ((AUDIO_COUNT++))
    fi
done
echo "  ✓ Found $AUDIO_COUNT/3 audio files"

# Check Activity1.cs
echo "6. Checking Activity configuration..."
if [ -f "GltronAndroid/Activity1.cs" ]; then
    if grep -q "AndroidGameActivity" "GltronAndroid/Activity1.cs"; then
        echo "  ✓ Activity1 properly configured"
    else
        echo "  ❌ Activity1 not properly configured"
    fi
else
    echo "  ❌ Activity1.cs not found"
fi

# Check for duplicate files (should be removed)
echo "7. Checking for duplicate files..."
DUPLICATES=0
for file in "GLTronGame.cs" "Player.cs" "Vec.cs" "Segment.cs"; do
    if [ -f "GltronAndroid/$file" ]; then
        echo "  ⚠️  Duplicate file found: GltronAndroid/$file (should be removed)"
        ((DUPLICATES++))
    fi
done

if [ $DUPLICATES -eq 0 ]; then
    echo "  ✓ No duplicate files found"
fi

# Test content build (dry run)
echo "8. Testing content build..."
if command -v mgcb &>/dev/null; then
    log "Testing MGCB build..."
    if mgcb -@:"GltronAndroid/Content/Content.mgcb" /platform:Android /outputDir:"GltronAndroid/Content/bin/Android" /intermediateDir:"GltronAndroid/Content/obj/Android" /quiet; then
        echo "  ✓ Content build test successful"
        
        # Check if XNB files were created
        if [ -d "GltronAndroid/Content/bin/Android" ]; then
            XNB_COUNT=$(find "GltronAndroid/Content/bin/Android" -name "*.xnb" | wc -l)
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
echo "  3. Run: ./scripts/build-android.sh -c Debug"
echo "  4. Deploy with: adb install -r path/to/generated.apk"
