#!/usr/bin/env bash
# Build the iOS project for macOS
# Usage: ./scripts/build-ios.sh [-c Debug|Release] [-p Platform] [-t Target]
# Defaults: -c Release, -p iPhone, -t GltronMobileGame.iOS
# Platform options: iPhone (device), iPhoneSimulator (simulator)

set -euo pipefail

CONFIG="Release"
PLATFORM="iPhone"
PROJ_DIR="GltronMobileGame.iOS"
TFM="net8.0-ios"

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

# Restore solution
echo "Restoring solution..."
dotnet restore "$SOLUTION_FILE"

# Build content
if [ -f "$PROJ_DIR/Content/Content.mgcb" ]; then
  echo "Building content (iOS platform)..."

  CONTENT_FILE="$PROJ_DIR/Content/Content.mgcb"
  OUT_DIR="$PROJ_DIR/Content/bin/iOS/Content"
  OBJ_DIR="$PROJ_DIR/Content/obj/iOS"

  mkdir -p "$OUT_DIR" "$OBJ_DIR"

  mgcb -r /@:"$CONTENT_FILE" /platform:iOS /outputDir:"$OUT_DIR" /intermediateDir:"$OBJ_DIR"

  echo "Content built. (not synchronizing Assets)"
fi

echo "Building solution..."
dotnet build "$SOLUTION_FILE" -c "$CONFIG"

# Build iOS project with specific platform
echo "Building iOS project for $PLATFORM..."
if [[ "$PLATFORM" == "iPhoneSimulator" ]]; then
  dotnet build "$PROJ_DIR" -c "$CONFIG" -f "$TFM" /p:Platform=iPhoneSimulator
else
  dotnet build "$PROJ_DIR" -c "$CONFIG" -f "$TFM" /p:Platform=iPhone
fi

# Find and report the output
echo ""
echo "Build complete!"

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
echo "Note: iOS deployment requires proper Apple Developer certificates and provisioning profiles."
