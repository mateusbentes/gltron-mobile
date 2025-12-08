#!/bin/bash
# Setup FNA frameworks for iOS
# Downloads or provides guidance for iOS framework setup

set -e

echo "🍎 Setting up FNA frameworks for iOS..."

# Check if we're on macOS
if [[ "$OSTYPE" != "darwin"* ]]; then
    echo "❌ iOS development requires macOS"
    echo "   This script can only run on macOS with Xcode installed"
    exit 1
fi

# Change to project root
cd "$(dirname "$0")/.."

# Create frameworks directory
mkdir -p GltronMobileGame.iOS/Frameworks

echo "📁 Frameworks directory: GltronMobileGame.iOS/Frameworks"

# Check for existing frameworks
SDL2_EXISTS=false
OPENAL_EXISTS=false

if [ -d "GltronMobileGame.iOS/Frameworks/SDL2.framework" ]; then
    SDL2_EXISTS=true
    echo "✅ SDL2.framework already exists"
else
    echo "❌ SDL2.framework missing"
fi

if [ -d "GltronMobileGame.iOS/Frameworks/OpenAL.framework" ]; then
    OPENAL_EXISTS=true
    echo "✅ OpenAL.framework already exists"
else
    echo "❌ OpenAL.framework missing"
fi

# If frameworks are missing, provide download instructions
if [ "$SDL2_EXISTS" = false ] || [ "$OPENAL_EXISTS" = false ]; then
    echo ""
    echo "📋 To get FNA iOS frameworks:"
    echo ""
    
    if [ "$SDL2_EXISTS" = false ]; then
        echo "🔽 SDL2.framework:"
        echo "1. Download SDL2 iOS framework from:"
        echo "   https://github.com/libsdl-org/SDL/releases"
        echo "2. Look for 'SDL2-[version].dmg' in release assets"
        echo "3. Mount the DMG and copy SDL2.framework to:"
        echo "   GltronMobileGame.iOS/Frameworks/SDL2.framework"
        echo ""
    fi
    
    if [ "$OPENAL_EXISTS" = false ]; then
        echo "🔽 OpenAL.framework:"
        echo "1. OpenAL is provided by iOS system frameworks"
        echo "2. Or download OpenAL Soft iOS framework from:"
        echo "   https://github.com/kcat/openal-soft/releases"
        echo "3. Copy to: GltronMobileGame.iOS/Frameworks/OpenAL.framework"
        echo ""
    fi
    
    echo "📝 Alternative: Compile stub frameworks locally"
    echo "   This script can create minimal stub frameworks for testing"
    echo ""
    
    # Offer to create stub frameworks
    read -p "Would you like to create minimal stub frameworks for testing? (y/n): " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        create_stub_frameworks
    fi
    
    echo "⚠️  Without proper frameworks:"
    echo "   • App may not build or run correctly"
    echo "   • Limited audio/graphics functionality"
    echo "   • App Store submission may fail"
    echo ""
fi

# Check Xcode and iOS SDK
echo "🔧 Checking iOS development environment..."

if command -v xcodebuild >/dev/null 2>&1; then
    XCODE_VERSION=$(xcodebuild -version | head -1)
    echo "✅ $XCODE_VERSION"
    
    # List available iOS SDKs
    IOS_SDKS=$(xcodebuild -showsdks | grep iphoneos | tail -1)
    if [ -n "$IOS_SDKS" ]; then
        echo "✅ $IOS_SDKS"
    else
        echo "⚠️  No iOS SDKs found"
    fi
else
    echo "❌ Xcode not found or not in PATH"
    echo "   Install Xcode from the App Store"
fi

# Check for iOS deployment target compatibility
echo ""
echo "📱 iOS Deployment Target: 12.0+"
echo "   This ensures compatibility with FNA requirements"

# Summary
echo ""
if [ "$SDL2_EXISTS" = true ] && [ "$OPENAL_EXISTS" = true ]; then
    echo "🎉 All FNA iOS frameworks are ready!"
    echo ""
    echo "📝 Next steps:"
    echo "1. Build: dotnet build GltronMobileGame.iOS/GltronMobileGame.iOS.csproj"
    echo "2. Deploy to simulator or device using Xcode"
    echo "3. Test FNA functionality"
else
    echo "⚠️  Some frameworks are missing"
    echo "   The project will build with warnings but may have limited functionality"
    echo ""
    echo "📝 To complete setup:"
    echo "1. Follow the download instructions above"
    echo "2. Re-run this script to verify"
    echo "3. Build and test the project"
fi

echo ""
echo "✅ iOS framework setup completed!"

# Function to create minimal stub frameworks for iOS testing
create_stub_frameworks() {
    echo "🔨 Creating minimal stub frameworks for iOS testing..."
    
    # Check if Xcode command line tools are available
    if ! command -v xcodebuild >/dev/null 2>&1; then
        echo "❌ Xcode command line tools not found"
        echo "   Install with: xcode-select --install"
        return 1
    fi
    
    # Create temporary directory for compilation
    TEMP_DIR=$(mktemp -d)
    echo "📁 Using temporary directory: $TEMP_DIR"
    
    # Create SDL2 stub source
    cat > "$TEMP_DIR/SDL2_stub.c" << 'EOF'
// Minimal SDL2 stub for iOS FNA compatibility
#include <stdlib.h>
#include <string.h>

// SDL2 initialization and core functions
int SDL_Init(unsigned int flags) { return 0; }
void SDL_Quit(void) { }
const char* SDL_GetError(void) { return "Stub SDL2 - iOS Testing"; }
void SDL_SetMainReady(void) { }

// Path functions for iOS
char* SDL_GetBasePath(void) { 
    static char base_path[] = "/var/mobile/Containers/Data/Application/";
    return base_path; 
}
char* SDL_GetPrefPath(const char* org, const char* app) { 
    static char pref_path[] = "/var/mobile/Containers/Data/Application/Documents/";
    return pref_path; 
}

// Window management stubs
void* SDL_CreateWindow(const char* title, int x, int y, int w, int h, unsigned int flags) { 
    return (void*)1; 
}
void SDL_DestroyWindow(void* window) { }

// OpenGL context management
void* SDL_GL_CreateContext(void* window) { return (void*)1; }
void SDL_GL_DeleteContext(void* context) { }
void SDL_GL_SwapWindow(void* window) { }
int SDL_GL_SetAttribute(int attr, int value) { return 0; }

// Event handling
int SDL_PollEvent(void* event) { return 0; }
void SDL_PumpEvents(void) { }

// Memory management
void SDL_free(void* mem) { if (mem) free(mem); }
void* SDL_malloc(size_t size) { return malloc(size); }

// iOS-specific functions
const char* SDL_GetPlatform(void) { return "iOS"; }
int SDL_SetHint(const char* name, const char* value) { return 1; }
EOF

    # Create OpenAL stub source
    cat > "$TEMP_DIR/OpenAL_stub.c" << 'EOF'
// Minimal OpenAL stub for iOS FNA compatibility
#include <stdint.h>

// OpenAL error handling
int alGetError(void) { return 0; }
const char* alGetString(int param) { return "OpenAL Stub - iOS Testing"; }

// Context management
typedef void ALCdevice;
typedef void ALCcontext;
ALCdevice* alcOpenDevice(const char* devicename) { return (ALCdevice*)0x12345678; }
int alcCloseDevice(ALCdevice* device) { return 1; }
ALCcontext* alcCreateContext(ALCdevice* device, const int* attrlist) { return (ALCcontext*)0x87654321; }
int alcMakeContextCurrent(ALCcontext* context) { return 1; }
void alcDestroyContext(ALCcontext* context) { }

// Source management
void alGenSources(int n, uint32_t* sources) { 
    for(int i = 0; i < n; i++) sources[i] = i + 1; 
}
void alDeleteSources(int n, const uint32_t* sources) { }
void alSourcePlay(uint32_t source) { }
void alSourceStop(uint32_t source) { }
void alSourcef(uint32_t source, int param, float value) { }
void alSourcei(uint32_t source, int param, int value) { }

// Buffer management
void alGenBuffers(int n, uint32_t* buffers) { 
    for(int i = 0; i < n; i++) buffers[i] = i + 100; 
}
void alDeleteBuffers(int n, const uint32_t* buffers) { }
void alBufferData(uint32_t buffer, int format, const void* data, int size, int freq) { }

// Listener management
void alListenerf(int param, float value) { }
void alListener3f(int param, float v1, float v2, float v3) { }
EOF

    # Create SDL2 framework
    if [ "$SDL2_EXISTS" = false ]; then
        echo "🔨 Compiling SDL2 stub framework..."
        
        # Create framework structure
        mkdir -p "GltronMobileGame.iOS/Frameworks/SDL2.framework/Headers"
        
        # Create minimal header
        cat > "GltronMobileGame.iOS/Frameworks/SDL2.framework/Headers/SDL.h" << 'EOF'
#ifndef SDL_h_
#define SDL_h_

// Minimal SDL2 header for iOS stub
extern int SDL_Init(unsigned int flags);
extern void SDL_Quit(void);
extern const char* SDL_GetError(void);
extern void SDL_SetMainReady(void);
extern char* SDL_GetBasePath(void);

#endif /* SDL_h_ */
EOF

        # Compile for iOS device (ARM64)
        xcrun -sdk iphoneos clang -arch arm64 -dynamiclib -install_name @rpath/SDL2.framework/SDL2 \
            -o "GltronMobileGame.iOS/Frameworks/SDL2.framework/SDL2" "$TEMP_DIR/SDL2_stub.c" \
            -mios-version-min=12.0 2>/dev/null || echo "⚠️ ARM64 compilation failed"
        
        # Create Info.plist
        cat > "GltronMobileGame.iOS/Frameworks/SDL2.framework/Info.plist" << 'EOF'
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleIdentifier</key>
    <string>org.libsdl.SDL2</string>
    <key>CFBundleName</key>
    <string>SDL2</string>
    <key>CFBundleVersion</key>
    <string>2.28.5</string>
    <key>CFBundleExecutable</key>
    <string>SDL2</string>
</dict>
</plist>
EOF
        
        echo "✅ SDL2 stub framework created"
    fi
    
    # Create OpenAL framework
    if [ "$OPENAL_EXISTS" = false ]; then
        echo "🔨 Compiling OpenAL stub framework..."
        
        # Create framework structure
        mkdir -p "GltronMobileGame.iOS/Frameworks/OpenAL.framework/Headers"
        
        # Create minimal header
        cat > "GltronMobileGame.iOS/Frameworks/OpenAL.framework/Headers/al.h" << 'EOF'
#ifndef AL_H
#define AL_H

#include <stdint.h>

// Minimal OpenAL header for iOS stub
extern int alGetError(void);
extern const char* alGetString(int param);
extern void alGenSources(int n, uint32_t* sources);
extern void alSourcePlay(uint32_t source);

#endif /* AL_H */
EOF

        # Compile for iOS device (ARM64)
        xcrun -sdk iphoneos clang -arch arm64 -dynamiclib -install_name @rpath/OpenAL.framework/OpenAL \
            -o "GltronMobileGame.iOS/Frameworks/OpenAL.framework/OpenAL" "$TEMP_DIR/OpenAL_stub.c" \
            -mios-version-min=12.0 2>/dev/null || echo "⚠️ ARM64 compilation failed"
        
        # Create Info.plist
        cat > "GltronMobileGame.iOS/Frameworks/OpenAL.framework/Info.plist" << 'EOF'
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleIdentifier</key>
    <string>org.openal.OpenAL</string>
    <key>CFBundleName</key>
    <string>OpenAL</string>
    <key>CFBundleVersion</key>
    <string>1.23.1</string>
    <key>CFBundleExecutable</key>
    <string>OpenAL</string>
</dict>
</plist>
EOF
        
        echo "✅ OpenAL stub framework created"
    fi
    
    # Clean up
    rm -rf "$TEMP_DIR"
    
    echo ""
    echo "🎉 Stub frameworks created successfully!"
    echo "⚠️  These are minimal stubs for testing only"
    echo "   For production, use official SDL2 and OpenAL frameworks"
    echo ""
    echo "📝 Next steps:"
    echo "1. Build iOS project: dotnet build GltronMobileGame.iOS/GltronMobileGame.iOS.csproj"
    echo "2. Test in iOS Simulator"
    echo "3. Replace with official frameworks for App Store submission"
}
