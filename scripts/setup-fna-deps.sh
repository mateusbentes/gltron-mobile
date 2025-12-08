#!/usr/bin/env bash
# Setup FNA dependencies by downloading required C# bindings
# This replaces the git submodule approach

set -euo pipefail

echo "🎮 Setting up FNA dependencies..."

# Change to project root directory
cd "$(dirname "$0")/.."

# Check if FNA directory exists
if [ ! -d "GltronMobileGame/FNA" ]; then
    echo "❌ FNA directory not found. Please ensure FNA source is available."
    exit 1
fi

cd GltronMobileGame/FNA

# Create lib directory structure
echo "📁 Creating FNA lib directory structure..."
mkdir -p lib/SDL2-CS/src
mkdir -p lib/FAudio/csharp
mkdir -p lib/Theorafile/csharp
mkdir -p lib/SDL3-CS/SDL3

# Download SDL2-CS
echo "📥 Downloading SDL2-CS..."
if wget -q -O lib/SDL2-CS/src/SDL2.cs https://raw.githubusercontent.com/flibitijibibo/SDL2-CS/master/src/SDL2.cs; then
    echo "✅ SDL2-CS downloaded successfully"
else
    echo "❌ SDL2-CS download failed"
fi

# Download FAudio
echo "📥 Downloading FAudio..."
if wget -q -O lib/FAudio/csharp/FAudio.cs https://raw.githubusercontent.com/FNA-XNA/FAudio/master/csharp/FAudio.cs; then
    echo "✅ FAudio downloaded successfully"
else
    echo "❌ FAudio download failed"
fi

# Download Theorafile
echo "📥 Downloading Theorafile..."
if wget -q -O lib/Theorafile/csharp/Theorafile.cs https://raw.githubusercontent.com/FNA-XNA/Theorafile/master/csharp/Theorafile.cs; then
    echo "✅ Theorafile downloaded successfully"
else
    echo "❌ Theorafile download failed"
fi

# Download SDL3-CS (legacy)
echo "📥 Downloading SDL3-CS..."
if wget -q -O lib/SDL3-CS/SDL3/SDL3.Legacy.cs https://raw.githubusercontent.com/flibitijibibo/SDL3-CS/main/SDL3/SDL3.Legacy.cs; then
    echo "✅ SDL3-CS downloaded successfully"
else
    echo "❌ SDL3-CS download failed"
fi

echo ""
echo "🎉 FNA dependencies setup completed!"
echo ""
echo "📝 Downloaded files:"
find lib -name "*.cs" -type f | sort

echo ""
echo "✅ FNA is now ready for building!"
echo "   • SDL2 C# bindings available"
echo "   • FAudio C# bindings available"
echo "   • Theorafile C# bindings available"
echo "   • All dependencies resolved"
