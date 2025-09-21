#!/bin/bash

echo "Flagstone UI - Development Environment Validation"
echo "================================================"

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

# Test restore
echo "3. Testing solution restore..."
if dotnet restore Flagstone.UI.sln --verbosity quiet; then
    echo "   ✓ Solution restore successful"
else
    echo "   ✗ Solution restore failed"
    exit 1
fi

echo ""

# Test build
echo "4. Testing solution build..."
if dotnet build Flagstone.UI.sln --no-restore --verbosity quiet; then
    echo "   ✓ Solution build successful"
else
    echo "   ✗ Solution build failed"
    exit 1
fi

echo ""
echo "🎉 Development environment is properly configured!"
echo "You can now work with the Flagstone UI solution."