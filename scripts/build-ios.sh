#!/usr/bin/env bash
# Build the FNA iOS project for macOS
# Usage: ./scripts/build-ios.sh [-c Debug|Release] [-p Platform] [-t Target]
# Defaults: -c Release, -p iPhone, -t GltronMobileGame.iOS
# Platform options: iPhone (device), iPhoneSimulator (simulator)

set -euo pipefail

CONFIG="Release"
PLATFORM="iPhone"
PROJ_DIR="GltronMobileGame.iOS"
TFM="net9.0-ios"

while getopts ":c:p:t:" opt; do
  case $opt in
    c) CONFIG="$OPTARG" ;;
    p) PLATFORM="$OPTARG" ;;
    t) PROJ_DIR="$OPTARG" ;;
    *) echo "Unknown option -$OPTARG" ; exit 1 ;;
  esac
done

# Validate platform
if [[ "$PLATFORM" != "iPhone" && "$PLATFORM" != "iPhoneSimulator" ]]; then
  echo "Error: Platform must be 'iPhone' or 'iPhoneSimulator'"
  exit 1
fi

# Check if running on macOS
if [[ "$(uname)" != "Darwin" ]]; then
  echo "Error: iOS builds require macOS"
  exit 1
fi

# Check if iOS project exists
if [ ! -d "$PROJ_DIR" ]; then
  echo "Error: iOS project directory '$PROJ_DIR' not found."
  exit 1
fi

# Check if iOS workload is installed
echo "Checking iOS workload..."
if ! dotnet workload list | grep -q ios; then
  echo "Error: iOS workload not installed. Install with: dotnet workload install ios"
  exit 1
fi

echo "Building iOS project..."
echo "  Configuration: $CONFIG"
echo "  Platform: $PLATFORM"
echo "  Project: $PROJ_DIR"

# Use full solution that includes iOS project
SOLUTION_FILE="GltronMobile.Full.sln"
if [ ! -f "$SOLUTION_FILE" ]; then
  echo "Error: Solution '$SOLUTION_FILE' not found."
  exit 1
fi

# Check FNA dependencies
echo "Checking FNA dependencies..."
if [ ! -f "GltronMobileGame/FNA/lib/SDL2-CS/src/SDL2.cs" ]; then
  echo "FNA dependencies missing. Please run the setup script or CI will handle this."
  echo "Continuing with build - dependencies should be available..."
else
  echo "FNA dependencies found"
fi

# Restore solution
echo "Restoring solution..."
dotnet restore "$SOLUTION_FILE"

# FNA uses raw content files - no MGCB build needed
echo "FNA: Using raw content files (no MGCB processing required)"
echo "Content files will be bundled directly from Content/ directory"

echo "Building FNA solution..."
dotnet build "$SOLUTION_FILE" -c "$CONFIG"

# Build FNA iOS project with specific platform
echo "Building FNA iOS project for $PLATFORM..."
if [[ "$PLATFORM" == "iPhoneSimulator" ]]; then
  dotnet build "$PROJ_DIR" -c "$CONFIG" -f "$TFM" /p:Platform=iPhoneSimulator
else
  dotnet build "$PROJ_DIR" -c "$CONFIG" -f "$TFM" /p:Platform=iPhone
fi

# Find and report the output
echo ""
echo "🎉 FNA iOS build completed!"

# Look for IPA or app bundle
OUTPUT_DIR="$PROJ_DIR/bin/$CONFIG/$TFM"
if [[ "$PLATFORM" == "iPhoneSimulator" ]]; then
  OUTPUT_DIR="$OUTPUT_DIR-iossimulator"
else
  OUTPUT_DIR="$OUTPUT_DIR-ios"
fi

if [ -d "$OUTPUT_DIR" ]; then
  echo "Output directory: $OUTPUT_DIR"
  
  # Look for .app bundle
  APP_BUNDLE=$(find "$OUTPUT_DIR" -name "*.app" -type d 2>/dev/null | head -n1)
  if [ -n "$APP_BUNDLE" ]; then
    echo "App bundle: $APP_BUNDLE"
  fi
  
  # Look for .ipa file
  IPA_FILE=$(find "$OUTPUT_DIR" -name "*.ipa" -type f 2>/dev/null | head -n1)
  if [ -n "$IPA_FILE" ]; then
    echo "IPA file: $IPA_FILE"
  fi
  
  if [[ "$PLATFORM" == "iPhoneSimulator" ]]; then
    echo ""
    echo "To run in simulator:"
    echo "  1. Open Xcode and start a simulator"
    echo "  2. Drag the .app bundle to the simulator"
    echo "  3. Or use: xcrun simctl install booted \"$APP_BUNDLE\""
  else
    echo ""
    echo "To deploy to device:"
    echo "  1. Connect your iOS device"
    echo "  2. Use Xcode to deploy the app bundle"
    echo "  3. Or use deployment tools with proper provisioning"
  fi
else
  echo "Warning: Expected output directory not found: $OUTPUT_DIR"
  echo "Check build output above for errors."
fi

echo ""
echo "📝 iOS Deployment Notes:"
echo "• iOS deployment requires proper Apple Developer certificates and provisioning profiles"
echo "• FNA iOS frameworks (SDL2, OpenAL, Theoraplay) must be in Frameworks/ directory"
echo ""
echo "✅ FNA iOS features enabled:"
echo "• Raw content loading (no XNB files)"
echo "• Better .NET 9 compatibility"
echo "• Same XNA/MonoGame API compatibility"
