# iOS Build Setup for FNA

## Current Status

The iOS build workflow is currently **disabled** (`if: false`) because it requires proper FNA iOS frameworks that are not easily downloadable from public sources.

## To Enable iOS Builds

### Option 1: Manual Framework Setup

1. **Obtain FNA iOS Frameworks**:
   - Build FNA iOS frameworks from source
   - Or obtain them from FNA community/releases
   - Required frameworks: `SDL2.framework`, `OpenAL.framework`, `Theoraplay.framework`

2. **Add Frameworks to Repository**:
   ```bash
   # Create frameworks directory
   mkdir -p GltronMobileGame.iOS/Frameworks
   
   # Copy real frameworks (replace placeholders)
   cp -r /path/to/SDL2.framework GltronMobileGame.iOS/Frameworks/
   cp -r /path/to/OpenAL.framework GltronMobileGame.iOS/Frameworks/
   cp -r /path/to/Theoraplay.framework GltronMobileGame.iOS/Frameworks/
   ```

3. **Enable Workflow**:
   - Edit `.github/workflows/ios-build.yml`
   - Change `if: false` to `if: true` or remove the line entirely

### Option 2: Build Frameworks in CI

Modify the iOS workflow to build FNA frameworks from source during CI (more complex but automated).

### Option 3: Use Package Manager

If FNA iOS frameworks become available through a package manager (CocoaPods, Swift Package Manager), update the workflow to use that approach.

## Current Placeholder Setup

The current workflow creates placeholder frameworks with basic `Info.plist` files to allow the build to complete without errors, but the resulting app will not be functional.

## Testing iOS Builds Locally

To test iOS builds locally:

1. Run the setup script: `./scripts/setup-fna-deps.sh`
2. Ensure you have proper FNA iOS frameworks
3. Build with: `./scripts/build-ios.sh -c Debug -p iPhoneSimulator`

## Notes

- iOS builds require macOS and Xcode
- iPhone Simulator builds don't require Apple Developer certificates
- Device builds require proper code signing setup
- FNA iOS support may have specific requirements not covered here
