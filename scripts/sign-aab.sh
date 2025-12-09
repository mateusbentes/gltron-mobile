#!/bin/bash

# GLTron Mobile - Android App Bundle (AAB) Signing Script
# Builds and signs AAB for Google Play Store distribution

set -e  # Exit on any error

# Configuration
KEYSTORE_NAME="gltron-release.keystore"
KEY_ALIAS="gltron-release-key"
PROJECT_FILE="GltronMobileGame/GltronAndroid.csproj"
BUILD_CONFIG="Release"
TARGET_FRAMEWORK="net8.0-android"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}📱 GLTron Mobile - AAB Signing Script${NC}"
echo "============================================="
echo ""

# Navigate to project root (one level up from scripts)
cd "$(dirname "$0")/.."

# Check if keystore exists
if [ ! -f "$KEYSTORE_NAME" ]; then
    echo -e "${RED}❌ ERROR: Keystore not found!${NC}"
    echo "Expected location: $(pwd)/$KEYSTORE_NAME"
    echo ""
    echo -e "${YELLOW}💡 Solution:${NC}"
    echo "Run: ./scripts/create-keystore.sh first"
    exit 1
fi

# Check if project file exists
if [ ! -f "$PROJECT_FILE" ]; then
    echo -e "${RED}❌ ERROR: Project file not found!${NC}"
    echo "Expected location: $(pwd)/$PROJECT_FILE"
    exit 1
fi

# Check if dotnet is available
if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}❌ ERROR: .NET SDK not found${NC}"
    echo "Please install .NET 8 SDK"
    exit 1
fi

echo -e "${BLUE}🔍 Build Configuration:${NC}"
echo "  Project: $PROJECT_FILE"
echo "  Configuration: $BUILD_CONFIG"
echo "  Target Framework: $TARGET_FRAMEWORK"
echo "  Keystore: $KEYSTORE_NAME"
echo "  Key Alias: $KEY_ALIAS"
echo ""

# Prompt for keystore password
echo -e "${YELLOW}🔑 Keystore Authentication${NC}"
read -s -p "Enter keystore password: " KEYSTORE_PASSWORD
echo ""
read -s -p "Enter key password (press Enter if same as keystore): " KEY_PASSWORD
echo ""

# Use keystore password for key password if not provided
if [ -z "$KEY_PASSWORD" ]; then
    KEY_PASSWORD="$KEYSTORE_PASSWORD"
fi

echo ""
echo -e "${BLUE}🧹 Cleaning previous builds...${NC}"
dotnet clean "$PROJECT_FILE" -c "$BUILD_CONFIG" --verbosity quiet

echo -e "${BLUE}🔨 Building signed Android App Bundle (AAB)...${NC}"
echo "This may take a few minutes..."
echo ""

# Build the signed AAB
dotnet publish "$PROJECT_FILE" \
    -c "$BUILD_CONFIG" \
    -f "$TARGET_FRAMEWORK" \
    -p:AndroidPackageFormat=aab \
    -p:AndroidKeyStore=true \
    -p:AndroidSigningKeyStore="$(pwd)/$KEYSTORE_NAME" \
    -p:AndroidSigningKeyAlias="$KEY_ALIAS" \
    -p:AndroidSigningKeyPass="$KEY_PASSWORD" \
    -p:AndroidSigningStorePass="$KEYSTORE_PASSWORD" \
    --verbosity normal

if [ $? -eq 0 ]; then
    echo ""
    echo -e "${GREEN}✅ SUCCESS: Signed AAB created!${NC}"
    echo ""
    
    # Find the generated AAB file
    AAB_PATH=$(find . -name "*.aab" -path "*/bin/$BUILD_CONFIG/*" | head -1)
    
    if [ -n "$AAB_PATH" ]; then
        AAB_SIZE=$(du -h "$AAB_PATH" | cut -f1)
        echo -e "${BLUE}📦 AAB Details:${NC}"
        echo "  File: $AAB_PATH"
        echo "  Size: $AAB_SIZE"
        echo ""
        
        # Verify the AAB is signed
        if command -v aapt &> /dev/null; then
            echo -e "${BLUE}🔍 Verifying AAB...${NC}"
            aapt dump badging "$AAB_PATH" | grep "package:" | head -1
            echo ""
        fi
        
        echo -e "${GREEN}🚀 Ready for Google Play Store!${NC}"
        echo ""
        echo -e "${YELLOW}📋 Next Steps:${NC}"
        echo "1. 🌐 Go to Google Play Console (play.google.com/console)"
        echo "2. 📤 Upload this AAB file: $AAB_PATH"
        echo "3. 📝 Fill in store listing details"
        echo "4. 🎯 Submit for review"
        echo ""
        echo -e "${BLUE}💡 Tips:${NC}"
        echo "• AAB files are smaller and more efficient than APKs"
        echo "• Google Play will generate optimized APKs for each device"
        echo "• Keep your keystore safe for future updates"
        
    else
        echo -e "${YELLOW}⚠️  WARNING: Could not locate the generated AAB file${NC}"
        echo "Check the build output directory manually:"
        echo "GltronMobileGame/bin/$BUILD_CONFIG/$TARGET_FRAMEWORK/publish/"
    fi
    
else
    echo ""
    echo -e "${RED}❌ ERROR: Build failed${NC}"
    echo ""
    echo -e "${YELLOW}🔧 Troubleshooting:${NC}"
    echo "1. Check that your keystore password is correct"
    echo "2. Verify the keystore file is not corrupted"
    echo "3. Ensure you have the latest .NET SDK"
    echo "4. Check the build output above for specific errors"
    echo ""
    echo -e "${BLUE}💡 Common Solutions:${NC}"
    echo "• Run: dotnet clean && dotnet restore"
    echo "• Check Android SDK is properly installed"
    echo "• Verify keystore was created correctly"
    exit 1
fi

# Clear sensitive variables
unset KEYSTORE_PASSWORD
unset KEY_PASSWORD
