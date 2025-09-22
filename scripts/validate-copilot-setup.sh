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

# Check MAUI workload
echo "2. Checking MAUI workload..."
maui_installed=$(dotnet workload list | grep -c "maui")
if [[ $maui_installed -gt 0 ]]; then
    echo "   ✓ MAUI workload is installed"
else
    echo "   ✗ MAUI workload not found. Please run: dotnet workload install maui"
    exit 1
fi

echo ""

# Check Git LFS
echo "3. Checking Git LFS..."
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
echo "4. Testing solution restore..."
if dotnet restore Flagstone.UI.sln --verbosity quiet; then
    echo "   ✓ Solution restore successful"
else
    echo "   ✗ Solution restore failed"
    exit 1
fi

echo ""

# Test Android build (specific to ubuntu-latest/Copilot)
echo "5. Testing Android target build (ubuntu-latest compatible)..."
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