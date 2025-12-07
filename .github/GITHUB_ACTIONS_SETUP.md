# GitHub Actions Setup for GLTron Mobile

## 🚀 What This Workflow Does

- **✅ Builds Debug APK** on every push/PR
- **✅ Builds Signed AAB** on version tags (v1.0, v2.0, etc.)
- **✅ Creates GitHub Releases** automatically
- **✅ Uploads artifacts** for download
- **✅ Caches dependencies** for faster builds

## 🔐 Required GitHub Secrets

To enable signed releases, add these secrets to your GitHub repository:

### 1. Go to Repository Settings
```
Your Repo → Settings → Secrets and variables → Actions → New repository secret
```

### 2. Add These Secrets:

| Secret Name | Description | How to Get |
|-------------|-------------|------------|
| `KEYSTORE_BASE64` | Your keystore file encoded in Base64 | See instructions below |
| `KEYSTORE_PASSWORD` | Your keystore password | From when you created keystore |
| `KEY_ALIAS` | Your key alias | Usually `gltron-release-key` |
| `KEY_PASSWORD` | Your key password | From when you created keystore |

## 📝 How to Get KEYSTORE_BASE64

### Option 1: Using Command Line
```bash
# Navigate to your project root
cd /home/mateus/gltron-mobile

# Encode your keystore to Base64
base64 -w 0 gltron-release.keystore > keystore-base64.txt

# Copy the content of keystore-base64.txt to GitHub secret
cat keystore-base64.txt
```

### Option 2: Using Scripts
```bash
# Create the keystore first (if you haven't)
./scripts/create-keystore.sh

# Then encode it
base64 -w 0 gltron-release.keystore | pbcopy  # macOS
base64 -w 0 gltron-release.keystore | xclip -selection clipboard  # Linux
```

## 🏷️ How to Trigger Releases

### Automatic Debug Builds
```bash
# Any push to main/master triggers debug build
git push origin main
```

### Automatic Release Builds
```bash
# Create and push a version tag
git tag v2.0
git push origin v2.0

# This will:
# 1. Build signed AAB
# 2. Create GitHub release
# 3. Upload AAB to release
```

## 📁 Build Artifacts

### Debug Builds
- **Location**: Actions → Build → Artifacts
- **File**: `gltron-mobile-debug-apk`
- **Use**: Testing and development

### Release Builds
- **Location**: Releases page
- **File**: `gltron-mobile-release-aab`
- **Use**: Google Play Store upload

## 🔧 Workflow Features

### ✅ What Works Out of the Box
- Debug APK builds
- Dependency caching
- Multi-platform support
- Artifact uploads

### 🔐 What Needs Secrets
- Signed AAB builds
- Automatic releases
- Google Play Store ready files

### 🚫 What's Optional
- All secrets (workflow works without them)
- Signed builds (unsigned builds still work)

## 🛠️ Customization

### Change Build Configuration
Edit `.github/workflows/android-build.yml`:

```yaml
# Build different configurations
-c Debug    # Change to Release, etc.

# Build different targets
-f net8.0-android    # Change framework if needed

# Add custom parameters
-p:CustomProperty=Value
```

### Change Trigger Conditions
```yaml
on:
  push:
    branches: [ "main", "develop" ]  # Add more branches
    tags: [ "v*", "release-*" ]      # Different tag patterns
```

## 🔍 Troubleshooting

### Build Fails
1. Check .NET 8 compatibility
2. Verify Android SDK setup
3. Check project file paths

### Signing Fails
1. Verify keystore Base64 encoding
2. Check secret names match exactly
3. Verify passwords are correct

### No Releases Created
1. Push must be a tag starting with 'v'
2. Check GITHUB_TOKEN permissions
3. Verify tag format: `v1.0`, `v2.0`, etc.

## 📋 Quick Setup Checklist

- [ ] Replace old Windows workflow with Android workflow
- [ ] Create keystore: `./scripts/create-keystore.sh`
- [ ] Encode keystore to Base64
- [ ] Add all 4 secrets to GitHub
- [ ] Push code to trigger first build
- [ ] Create version tag for first release

## 🎯 Benefits

- **🚀 Automated builds** on every commit
- **📦 Ready-to-upload AABs** for Play Store
- **🔐 Secure keystore handling**
- **📱 Professional CI/CD pipeline**
- **⚡ Fast builds with caching**
