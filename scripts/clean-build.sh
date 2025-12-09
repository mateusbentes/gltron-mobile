#!/usr/bin/env bash
# Clean MonoGame Android project build artifacts
# Usage: ./scripts/clean-build.sh [-p ProjectDir] [-a] [-v]
# Options:
#   -p ProjectDir  : Specify project directory (default: GltronMobileGame)
#   -a            : Clean all projects (GltronMobileGame + GltronMobileEngine + GltronMobileGame.iOS)
#   -v            : Verbose output

set -euo pipefail

PROJ_DIR="GltronMobileGame"
CLEAN_ALL=false
VERBOSE=false

while getopts ":p:av" opt; do
  case $opt in
    p) PROJ_DIR="$OPTARG" ;;
    a) CLEAN_ALL=true ;;
    v) VERBOSE=true ;;
    *) echo "Unknown option -$OPTARG" ; exit 1 ;;
  esac
done

log() {
    if [ "$VERBOSE" = true ]; then
        echo "$1"
    fi
}

clean_project() {
    local project_path="$1"
    local project_name=$(basename "$project_path")
    
    echo "Cleaning $project_name..."
    
    if [ -d "$project_path" ]; then
        # Clean dotnet build artifacts
        if [ -f "$project_path/${project_name}.csproj" ]; then
            log "  Running dotnet clean..."
            if command -v dotnet &>/dev/null; then
                (cd "$project_path" && dotnet clean --verbosity minimal) || echo "  Warning: dotnet clean failed"
            else
                echo "  Warning: dotnet not found, skipping dotnet clean"
            fi
        fi
        
        # Remove bin and obj directories
        if [ -d "$project_path/bin" ]; then
            log "  Removing bin directory..."
            rm -rf "$project_path/bin"
        fi
        
        if [ -d "$project_path/obj" ]; then
            log "  Removing obj directory..."
            rm -rf "$project_path/obj"
        fi
        
        # Remove Content build artifacts
        if [ -d "$project_path/Content/bin" ]; then
            log "  Removing Content/bin directory..."
            rm -rf "$project_path/Content/bin"
        fi
        
        if [ -d "$project_path/Content/obj" ]; then
            log "  Removing Content/obj directory..."
            rm -rf "$project_path/Content/obj"
        fi
        
        # Remove Android-specific artifacts
        if [ -d "$project_path/Resources/bin" ]; then
            log "  Removing Resources/bin directory..."
            rm -rf "$project_path/Resources/bin"
        fi
        
        if [ -d "$project_path/Resources/obj" ]; then
            log "  Removing Resources/obj directory..."
            rm -rf "$project_path/Resources/obj"
        fi
        
        # Remove any APK/AAB files
        find "$project_path" -name "*.apk" -o -name "*.aab" 2>/dev/null | while read -r file; do
            log "  Removing $file..."
            rm -f "$file"
        done
        
        echo "  ✓ $project_name cleaned"
    else
        echo "  Warning: $project_path not found"
    fi
}

# Clean specified project
clean_project "$PROJ_DIR"

# Clean additional projects if requested
if [ "$CLEAN_ALL" = true ]; then
    clean_project "GltronMobileEngine"
    clean_project "GltronMobileGame.iOS"
fi

# Clean any global build artifacts in root
echo "Cleaning root directory artifacts..."
if [ -d "bin" ]; then
    log "  Removing root bin directory..."
    rm -rf "bin"
fi

if [ -d "obj" ]; then
    log "  Removing root obj directory..."
    rm -rf "obj"
fi

# Clean any temporary files
log "Removing temporary files..."
find . -name "*.tmp" -o -name "*.temp" -o -name "*~" 2>/dev/null | while read -r file; do
    log "  Removing $file..."
    rm -f "$file"
done

echo "✓ Build clean complete!"
echo ""
echo "To rebuild, run:"
echo "  ./scripts/build-android.sh -c Release"
echo ""
echo "To clean all projects next time, use:"
echo "  ./scripts/clean-build.sh -a -v"
