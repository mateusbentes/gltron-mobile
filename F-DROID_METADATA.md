# F-Droid Metadata for GLTron Mobile

This document outlines the F-Droid metadata structure and requirements for GLTron Mobile.

## ✅ Completed Metadata Files

### Required Files Created:
- ✅ `metadata/en-US/short_description.txt` (42 characters)
- ✅ `metadata/en-US/full_description.txt` (comprehensive description)
- ✅ `metadata/en-US/images/icon.png` (copied from app resources)
- 🔄 `metadata/en-US/images/phoneScreenshots/` (directory created, screenshots needed)

## 📋 F-Droid Submission Checklist

### Metadata Requirements:
- ✅ Short description (30-50 characters, no period)
- ✅ Full description with features and controls
- ✅ App icon (PNG format)
- ⏳ Phone screenshots (need to capture from actual gameplay)

### Technical Requirements:
- ✅ Open source code (GPL-3.0 license)
- ✅ No proprietary dependencies
- ✅ No ads or tracking
- ✅ No in-app purchases
- ✅ Builds with standard Android SDK
- ✅ MonoGame framework (FOSS)

### App Information:
- **Package Name**: `gltron.org.gltronmobile`
- **Version**: 4.0 (versionCode: 4)
- **License**: GPL-3.0
- **Categories**: Games
- **Min SDK**: 24 (Android 7.0)
- **Target SDK**: 36 (Android 13)

## 📱 Screenshots Needed

To complete F-Droid submission, capture these screenshots:

1. **Main Menu** - Show title, "Tap to Start", keyboard controls info
2. **Gameplay** - Active game with light cycles, trails, arena
3. **Action Scene** - Multiple players, complex trail patterns
4. **Game Over** - Score display, restart options

### Screenshot Specifications:
- **Format**: PNG
- **Resolution**: 1920x1080 (landscape) or 1080x1920 (portrait)
- **Content**: Clear UI, good lighting, representative gameplay
- **Naming**: `screenshot1.png`, `screenshot2.png`, etc.

## 🚀 Next Steps for F-Droid Submission

1. **Capture Screenshots**:
   ```bash
   # Connect Android device with USB debugging
   adb shell screencap -p /sdcard/gltron_menu.png
   adb pull /sdcard/gltron_menu.png metadata/en-US/images/phoneScreenshots/screenshot1.png
   ```

2. **Test Build**:
   ```bash
   ./scripts/build-android.sh
   # Verify APK builds successfully
   ```

3. **Verify Metadata**:
   - Check all files are in correct locations
   - Validate description formatting
   - Ensure icon is proper resolution

4. **Submit to F-Droid**:
   - Fork F-Droid data repository
   - Add GLTron Mobile metadata
   - Create merge request

## 📄 File Structure

```
metadata/
└── en-US/
    ├── short_description.txt
    ├── full_description.txt
    └── images/
        ├── icon.png
        └── phoneScreenshots/
            ├── screenshot1.png
            ├── screenshot2.png
            ├── screenshot3.png
            └── screenshot4.png
```

## 🎯 F-Droid Benefits

Publishing on F-Droid will provide:
- **FOSS Community Access** - Reach users who prefer open source apps
- **No Google Play Dependencies** - Alternative distribution channel
- **Privacy Focused** - No tracking or analytics required
- **Global Reach** - Available worldwide without restrictions
- **Community Trust** - F-Droid's reputation for quality FOSS apps

GLTron Mobile is well-suited for F-Droid as it's completely open source, has no ads, no tracking, and provides classic gaming entertainment.
