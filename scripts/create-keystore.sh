#!/bin/bash

# GLTron Mobile - Android Keystore Creation Script
# Creates a keystore for signing Android App Bundles (AAB) and APKs

set -e  # Exit on any error

# Configuration
KEYSTORE_NAME="gltron-release.keystore"
KEY_ALIAS="gltron-release-key"
VALIDITY_DAYS=10000  # ~27 years
KEY_SIZE=2048
ALGORITHM="RSA"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}🔐 GLTron Mobile - Android Keystore Creator${NC}"
echo "=================================================="
echo ""

# Check if keytool is available
if ! command -v keytool &> /dev/null; then
    echo -e "${RED}❌ ERROR: keytool not found${NC}"
    echo "Please install Java Development Kit (JDK)"
    echo "On Ubuntu/Debian: sudo apt install openjdk-11-jdk"
    echo "On macOS: brew install openjdk@11"
    exit 1
fi

# Navigate to project root (one level up from scripts)
cd "$(dirname "$0")/.."

echo -e "${YELLOW}📋 Keystore Configuration:${NC}"
echo "  File name: $KEYSTORE_NAME"
echo "  Key alias: $KEY_ALIAS"
echo "  Algorithm: $ALGORITHM"
echo "  Key size: $KEY_SIZE bits"
echo "  Validity: $VALIDITY_DAYS days (~27 years)"
echo ""

# Check if keystore already exists
if [ -f "$KEYSTORE_NAME" ]; then
    echo -e "${YELLOW}⚠️  WARNING: Keystore already exists!${NC}"
    echo "File: $(pwd)/$KEYSTORE_NAME"
    echo ""
    read -p "Do you want to overwrite it? (y/N): " -n 1 -r
    echo ""
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        echo "Operation cancelled."
        exit 0
    fi
    echo ""
fi

echo -e "${BLUE}🔑 Creating Android keystore...${NC}"
echo "You will be prompted for:"
echo "1. Keystore password (remember this!)"
echo "2. Key password (can be same as keystore password)"
echo "3. Your details (name, organization, etc.)"
echo ""

# Create the keystore
keytool -genkey -v \
    -keystore "$KEYSTORE_NAME" \
    -alias "$KEY_ALIAS" \
    -keyalg "$ALGORITHM" \
    -keysize $KEY_SIZE \
    -validity $VALIDITY_DAYS \
    -storetype JKS

if [ $? -eq 0 ]; then
    echo ""
    echo -e "${GREEN}✅ SUCCESS: Android keystore created!${NC}"
    echo ""
    echo -e "${BLUE}📁 Keystore Details:${NC}"
    echo "  Location: $(pwd)/$KEYSTORE_NAME"
    echo "  Alias: $KEY_ALIAS"
    echo "  Type: JKS (Java KeyStore)"
    echo ""
    echo -e "${YELLOW}⚠️  IMPORTANT SECURITY NOTES:${NC}"
    echo "1. 🔒 Keep this keystore file SAFE - back it up securely"
    echo "2. 🔑 Remember your passwords - they cannot be recovered"
    echo "3. 📱 You'll need this keystore for ALL future app updates"
    echo "4. 🚫 Never commit this keystore to version control"
    echo "5. 🔐 Store passwords in a secure password manager"
    echo ""
    echo -e "${GREEN}📋 Next Steps:${NC}"
    echo "1. Run: ./scripts/sign-aab.sh to create signed AAB"
    echo "2. Upload the signed AAB to Google Play Console"
    echo ""
    
    # Create a .gitignore entry if it doesn't exist
    if [ ! -f .gitignore ]; then
        echo "*.keystore" > .gitignore
        echo "Created .gitignore to exclude keystore files"
    elif ! grep -q "*.keystore" .gitignore; then
        echo "*.keystore" >> .gitignore
        echo "Added *.keystore to .gitignore"
    fi
    
else
    echo ""
    echo -e "${RED}❌ ERROR: Failed to create keystore${NC}"
    echo "Please check the error messages above and try again"
    exit 1
fi
