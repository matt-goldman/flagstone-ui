#!/bin/bash

echo "Flagstone UI - Copilot Environment Validation (ubuntu-latest)"
echo "============================================================"

# Check .NET version
echo "1. Checking .NET version..."
dotnet_version=$(dotnet --version 2>/dev/null)
if [[ $? -eq 0 ]]; then
    echo "   ✓ .NET SDK version: $dotnet_version"
    if [[ $dotnet_version == 9.* ]]; then
        echo "   ✓ .NET 9 detected"
    else
        echo "   ⚠ Warning: Expected .NET 9, found $dotnet_version"
    fi
else
    echo "   ✗ .NET SDK not found. Please install .NET 9."
    exit 1
fi

echo ""

# Check Java SDK
echo "2. Checking Java SDK..."
if command -v java &> /dev/null; then
    java_version=$(java -version 2>&1 | head -n 1)
    echo "   ✓ Java is installed: $java_version"
    if [[ -n "$JAVA_HOME" ]]; then
        echo "   ✓ JAVA_HOME is set: $JAVA_HOME"
    else
        echo "   ⚠ JAVA_HOME environment variable not set"
    fi
else
    echo "   ✗ Java not found. Please install OpenJDK 17."
    exit 1
fi

echo ""

# Check MAUI Android workload
echo "3. Checking MAUI Android workload..."
maui_installed=$(dotnet workload list | grep -c "maui-android")
if [[ $maui_installed -gt 0 ]]; then
    echo "   ✓ MAUI Android workload is installed"
else
    echo "   ✗ MAUI Android workload not found. Please run: dotnet workload install maui-android"
    exit 1
fi

echo ""

# Check Android SDK
echo "4. Checking Android SDK..."
if [[ -n "$ANDROID_HOME" ]]; then
    echo "   ✓ ANDROID_HOME is set: $ANDROID_HOME"
    if [[ -d "$ANDROID_HOME" ]]; then
        echo "   ✓ Android SDK directory exists"
    else
        echo "   ⚠ Android SDK directory does not exist"
    fi
else
    echo "   ⚠ ANDROID_HOME environment variable not set"
fi

echo ""

# Check Git LFS
echo "5. Checking Git LFS..."
if command -v git-lfs &> /dev/null; then
    echo "   ✓ Git LFS is installed"
    if git lfs env &> /dev/null; then
        echo "   ✓ Git LFS is initialized"
    else
        echo "   ⚠ Git LFS not initialized. Run: git lfs install"
    fi
else
    echo "   ✗ Git LFS not found. Please install git-lfs package"
    exit 1
fi

echo ""

# Test restore
echo "6. Testing solution restore..."
if dotnet restore Flagstone.UI.sln --verbosity quiet; then
    echo "   ✓ Solution restore successful"
else
    echo "   ✗ Solution restore failed"
    exit 1
fi

echo ""

# Test Android build (specific to ubuntu-latest/Copilot)
echo "7. Testing Android target build (ubuntu-latest compatible)..."
if dotnet build --framework net9.0-android --no-restore --verbosity quiet; then
    echo "   ✓ Android target build successful"
else
    echo "   ✗ Android target build failed"
    echo "   Note: This is the only target framework supported on ubuntu-latest"
    exit 1
fi

echo ""
echo "🎉 Copilot environment is properly configured!"
echo "Note: For full cross-platform development, use Windows or macOS hosts."
echo "In Copilot workflows, always use: dotnet build --framework net9.0-android"