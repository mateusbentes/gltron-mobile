#!/usr/bin/env bash
# Build the solution (Engine + Android Game) for the new architecture
# Usage: ./scripts/build-android.sh [-c Debug|Release] [-p ProjectDir] [-f TargetFramework]
# Defaults: -c Release, -p GltronMobileGame, -f net8.0-android

set -euo pipefail

CONFIG="Release"
PROJ_DIR="GltronMobileGame"
TFM="net8.0-android"

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

# Restore solution
echo "Restoring solution..."
dotnet restore "$SOLUTION_FILE"

# Build content if MGCB present
if [ -f "$PROJ_DIR/Content/Content.mgcb" ]; then
  echo "Building content (Android platform)..."
  
  CONTENT_FILE="$PROJ_DIR/Content/Content.mgcb"
  OUT_DIR="$PROJ_DIR/Content/bin/Android/Content"
  OBJ_DIR="$PROJ_DIR/Content/obj/Android"

  mkdir -p "$OUT_DIR" "$OBJ_DIR"

  mgcb -r /@:"$CONTENT_FILE" /platform:Android /outputDir:"$OUT_DIR" /intermediateDir:"$OBJ_DIR"

  echo "Content built. (not synchronizing Assets)"
fi

# Build via solution (engine + game)
echo "Building solution..."
dotnet build "$SOLUTION_FILE" -c "$CONFIG"

# Optionally also build the game project for a specific TFM if needed
echo "Building Android project (explicit TFM: $TFM)..."
dotnet build "$PROJ_DIR" -c "$CONFIG" -f "$TFM"

echo "Done. To deploy, use your IDE or: adb install -r \"$(find \"$PROJ_DIR/bin/$CONFIG\" -type f -name '*.apk' -o -name '*.aab' 2>/dev/null | head -n1)\""
