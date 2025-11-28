#!/usr/bin/env bash
# Detect platform and show available build options
# Usage: ./scripts/detect-platform.sh

set -euo pipefail

echo "🔍 GLTron Mobile - Platform Detection and Build Options"
echo "======================================================="

# Detect OS
OS=$(uname)
case "$OS" in
  "Darwin")
    echo "Platform: macOS"
    echo "Available builds: ✅ Android, ✅ iOS"
    echo ""
    echo "Android build:"
    echo "  ./scripts/build-android-app.sh"
    echo ""
    echo "iOS build:"
    echo "  ./scripts/build-ios-app.sh -p iPhoneSimulator  # For simulator"
    echo "  ./scripts/build-ios-app.sh -p iPhone          # For device"
    ;;
  "Linux")
    echo "Platform: Linux"
    echo "Available builds: ✅ Android, ❌ iOS (requires macOS)"
    echo ""
    echo "Android build:"
    echo "  ./scripts/build-android-app.sh"
    echo ""
    echo "Note: iOS builds require macOS with Xcode and iOS workload"
    ;;
  "MINGW"*|"MSYS"*|"CYGWIN"*)
    echo "Platform: Windows"
    echo "Available builds: ✅ Android, ❌ iOS (requires macOS)"
    echo ""
    echo "Android build:"
    echo "  ./scripts/build-android-app.sh"
    echo ""
    echo "Note: iOS builds require macOS with Xcode and iOS workload"
    ;;
  *)
    echo "Platform: Unknown ($OS)"
    echo "Available builds: ⚠️  Android (untested), ❌ iOS (requires macOS)"
    ;;
esac

echo ""
echo "Verification commands:"
echo "  ./scripts/verify-build.sh     # Check Android setup"
if [[ "$OS" == "Darwin" ]]; then
  echo "  ./scripts/verify-ios.sh       # Check iOS setup"
fi

echo ""
echo "Solution files:"
echo "  GltronMobile.sln              # Android-only (works on all platforms)"
if [ -f "GltronMobile.Full.sln" ]; then
  echo "  GltronMobile.Full.sln         # Full multiplatform (macOS only)"
fi
