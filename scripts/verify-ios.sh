#!/usr/bin/env bash
# Verify iOS build setup for macOS
# Usage: ./scripts/verify-ios.sh [-v]

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

echo "🍎 GLTron Mobile - Verifying iOS build setup..."

# 1) Check if running on macOS
echo "1. Checking platform..."
if [[ "$(uname)" == "Darwin" ]]; then
    echo "  ✓ Running on macOS"
else
    echo "  ❌ iOS builds require macOS"
    exit 1
fi

# 2) Check .NET environment
echo "2. Checking .NET environment..."
if command -v dotnet &>/dev/null; then
    DOTNET_VERSION=$(dotnet --version)
    echo "  ✓ .NET found: $DOTNET_VERSION"
else
    echo "  ❌ .NET not found. Please install .NET 8 SDK"
    exit 1
fi

# 3) Check iOS workload
echo "3. Checking iOS workload..."
if dotnet workload list | grep -q ios; then
    echo "  ✓ iOS workload installed"
else
    echo "  ❌ iOS workload not found. Install with: dotnet workload install ios"
    exit 1
fi

# 4) Check Xcode
echo "4. Checking Xcode..."
if command -v xcodebuild &>/dev/null; then
    XCODE_VERSION=$(xcodebuild -version | head -n1)
    echo "  ✓ Xcode found: $XCODE_VERSION"
else
    echo "  ⚠️  Xcode not found. Install from App Store for device deployment"
fi

# 5) Check MGCB tool
echo "5. Checking MGCB tool..."
if command -v mgcb &>/dev/null || command -v dotnet-mgcb &>/dev/null || command -v mgcb-editor &>/dev/null; then
    echo "  ✓ MGCB tool found"
else
    echo "  ⚠️  MGCB tool not found. Install with: dotnet tool install -g dotnet-mgcb-editor"
fi

# 6) Check full solution
echo "6. Checking full solution..."
if [ -f "GltronMobile.Full.sln" ]; then
    echo "  ✓ Full solution found: GltronMobile.Full.sln"
else
    echo "  ❌ Full solution not found (GltronMobile.Full.sln)"
    exit 1
fi

# 7) Check iOS project
echo "7. Checking iOS project..."
if [ -f "GltronMobileGame.iOS/GltronMobileGame.iOS.csproj" ]; then
    echo "  ✓ iOS project found"
    
    # Check target framework
    if grep -q "net8.0-ios" "GltronMobileGame.iOS/GltronMobileGame.iOS.csproj"; then
        echo "  ✓ iOS target framework configured"
    else
        echo "  ❌ iOS target framework not properly configured"
    fi
    
    # Check MonoGame iOS package
    if grep -q "MonoGame.Framework.iOS" "GltronMobileGame.iOS/GltronMobileGame.iOS.csproj"; then
        echo "  ✓ MonoGame iOS package referenced"
    else
        echo "  ❌ MonoGame iOS package not found"
    fi
else
    echo "  ❌ iOS project not found"
    exit 1
fi

# 8) Check Info.plist
echo "8. Checking iOS configuration..."
if [ -f "GltronMobileGame.iOS/Info.plist" ]; then
    echo "  ✓ Info.plist found"
    
    if grep -q "CFBundleIdentifier" "GltronMobileGame.iOS/Info.plist"; then
        BUNDLE_ID=$(grep -A1 "CFBundleIdentifier" "GltronMobileGame.iOS/Info.plist" | tail -n1 | sed 's/.*<string>\(.*\)<\/string>.*/\1/')
        log "Bundle ID: $BUNDLE_ID"
        echo "  ✓ Bundle identifier configured"
    fi
else
    echo "  ❌ Info.plist not found"
fi

# 9) Check shared engine
echo "9. Checking shared engine..."
if [ -f "GltronMobileEngine/GltronMobileEngine.csproj" ]; then
    echo "  ✓ Shared engine found"
else
    echo "  ❌ Shared engine not found"
    exit 1
fi

# 10) Test content build for iOS
echo "10. Testing iOS content build..."
CONTENT_DIR="GltronMobileGame/Content"
if [ -f "$CONTENT_DIR/Content.mgcb" ] && command -v mgcb &>/dev/null; then
    log "Testing MGCB build for iOS..."
    if mgcb -@:"$CONTENT_DIR/Content.mgcb" /platform:iOS /outputDir:"$CONTENT_DIR/bin/iOS" /intermediateDir:"$CONTENT_DIR/obj/iOS" /quiet; then
        echo "  ✓ iOS content build test successful"
        if [ -d "$CONTENT_DIR/bin/iOS" ]; then
            XNB_COUNT=$(find "$CONTENT_DIR/bin/iOS" -name "*.xnb" | wc -l)
            echo "  ✓ Generated $XNB_COUNT XNB files for iOS"
        fi
    else
        echo "  ❌ iOS content build test failed"
    fi
else
    echo "  ⚠️  Skipping iOS content build test (MGCB not available or Content.mgcb not found)"
fi

echo ""
echo "🎯 iOS build verification complete!"
echo ""
echo "Next steps:"
echo "  1. If any ❌ errors above, fix them first"
echo "  2. Run: ./scripts/clean-build.sh -a"
echo "  3. For simulator: ./scripts/build-ios.sh -c Release -p iPhoneSimulator"
echo "  4. For device: ./scripts/build-ios.sh -c Release -p iPhone"
echo ""
echo "Note: Device deployment requires Apple Developer account and proper certificates."
