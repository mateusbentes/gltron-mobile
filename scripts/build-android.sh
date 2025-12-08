#!/usr/bin/env bash
# Build the FNA Android project
# Usage: ./scripts/build-android.sh [-c Debug|Release] [-p ProjectDir] [-f TargetFramework]
# Defaults: -c Release, -p GltronMobileGame, -f net9.0-android

set -euo pipefail

CONFIG="Release"
PROJ_DIR="GltronMobileGame"
TFM="net9.0-android"

while getopts ":c:p:f:" opt; do
  case $opt in
    c) CONFIG="$OPTARG" ;;
    p) PROJ_DIR="$OPTARG" ;;
    f) TFM="$OPTARG" ;;
    *) echo "Unknown option -$OPTARG" ; exit 1 ;;
  esac
done

if [ ! -d "$PROJ_DIR" ]; then
  echo "Project directory '$PROJ_DIR' not found. Ensure GltronMobileGame exists."
  exit 1
fi

# Determine which solution file to use
SOLUTION_FILE="GltronMobile.sln"
if [[ "$(uname)" == "Darwin" ]] && [ -f "GltronMobile.Full.sln" ]; then
  SOLUTION_FILE="GltronMobile.Full.sln"
  echo "Using full solution (includes iOS) on macOS"
else
  echo "Using Android-only solution"
fi

# Check FNA dependencies
echo "Checking FNA dependencies..."
if [ ! -f "GltronMobileGame/FNA/lib/SDL2-CS/src/SDL2.cs" ]; then
  echo "FNA dependencies missing. Please run the setup script or CI will handle this."
  echo "Continuing with build - dependencies should be available..."
else
  echo "FNA dependencies found"
fi

# Restore solution (includes FNA project reference)
echo "Restoring solution..."
dotnet restore "$SOLUTION_FILE"

# FNA uses raw content files - no MGCB build needed
echo "FNA: Using raw content files (no MGCB processing required)"
echo "Content files will be packaged directly from Content/ directory"

# Build FNA solution (engine + game) - Android targets only
echo "Building FNA solution for Android..."
dotnet build "$SOLUTION_FILE" -c "$CONFIG" -f net9.0-android

# Build the FNA Android project
echo "Building FNA Android project (TFM: $TFM)..."
dotnet build "$PROJ_DIR" -c "$CONFIG" -f "$TFM"

echo ""
echo "🎉 FNA Android build completed!"
echo ""
echo "📱 To deploy:"
echo "   adb install -r \"$(find \"$PROJ_DIR/bin/$CONFIG\" -type f -name '*.apk' -o -name '*.aab' 2>/dev/null | head -n1)\""
echo ""
echo "✅ FNA features enabled:"
echo "   • Direct activity management (no AndroidGameActivity)"
echo "   • Raw content loading (no XNB files)"
echo "   • Better .NET 9 compatibility"
