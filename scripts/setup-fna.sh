#!/bin/bash
# Setup FNA for GLTron Mobile (Android and iOS)

set -e

echo "🎮 Setting up FNA for GLTron Mobile..."

# Change to project root directory
cd "$(dirname "$0")/.."

# Check if FNA directory already exists
if [ -d "GltronMobileGame/FNA" ]; then
    echo "✅ FNA directory already exists"
    read -p "Do you want to update FNA? (y/n): " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        echo "🔄 Updating FNA..."
        cd GltronMobileGame/FNA
        git pull
        cd ../..
    fi
else
    echo "📥 Downloading FNA source..."
    cd GltronMobileGame
    git clone https://github.com/FNA-XNA/FNA.git
    cd ..
fi

echo "🔧 Setting up FNA native libraries for Android..."

# Create native library directories for Android
mkdir -p GltronMobileGame/lib/arm64-v8a
mkdir -p GltronMobileGame/lib/armeabi-v7a
mkdir -p GltronMobileGame/lib/x86_64

echo "🍎 Setting up FNA native libraries for iOS..."

# Create native library directories for iOS
mkdir -p GltronMobileGame.iOS/Frameworks

echo "📋 FNA setup completed!"
echo ""
echo "📝 Next steps:"
echo ""
echo "🤖 For Android:"
echo "1. Download FNA native libraries for Android from:"
echo "   https://github.com/FNA-XNA/FNA/releases"
echo "2. Extract the following files to GltronMobileGame/lib/:"
echo "   - libSDL2.so (to arm64-v8a, armeabi-v7a, x86_64)"
echo "   - libopenal.so (to arm64-v8a, armeabi-v7a, x86_64)"
echo "   - libtheoraplay.so (to arm64-v8a, armeabi-v7a, x86_64)"
echo "3. Build: dotnet build GltronMobileGame/GltronAndroid.csproj"
echo ""
echo "🍎 For iOS (macOS only):"
echo "1. Download FNA iOS frameworks from:"
echo "   https://github.com/FNA-XNA/FNA/releases"
echo "2. Extract frameworks to GltronMobileGame.iOS/Frameworks/:"
echo "   - SDL2.framework"
echo "   - OpenAL.framework"
echo "   - Theoraplay.framework"
echo "3. Build: dotnet build GltronMobileGame.iOS/GltronMobileGame.iOS.csproj"
echo ""
echo "🎯 FNA provides:"
echo "✅ Better .NET 9 compatibility"
echo "✅ Direct activity management (no AndroidGameActivity needed)"
echo "✅ Easier deployment and debugging"
echo "✅ Same XNA/MonoGame API compatibility"
