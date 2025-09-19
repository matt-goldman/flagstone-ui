Write-Host "Flagstone UI - Development Environment Validation" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan

# Check .NET version
Write-Host "1. Checking .NET version..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version
    Write-Host "   ✓ .NET SDK version: $dotnetVersion" -ForegroundColor Green
    if ($dotnetVersion.StartsWith("9.")) {
        Write-Host "   ✓ .NET 9 detected" -ForegroundColor Green
    } else {
        Write-Host "   ⚠ Warning: Expected .NET 9, found $dotnetVersion" -ForegroundColor Yellow
    }
} catch {
    Write-Host "   ✗ .NET SDK not found. Please install .NET 9." -ForegroundColor Red
    exit 1
}

Write-Host ""

# Check MAUI workload
Write-Host "2. Checking MAUI workload..." -ForegroundColor Yellow
$workloads = dotnet workload list
if ($workloads -match "maui") {
    Write-Host "   ✓ MAUI workload is installed" -ForegroundColor Green
} else {
    Write-Host "   ✗ MAUI workload not found. Please run: dotnet workload install maui" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Test restore
Write-Host "3. Testing solution restore..." -ForegroundColor Yellow
try {
    dotnet restore Microsoft.Maui.sln --verbosity quiet
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   ✓ Solution restore successful" -ForegroundColor Green
    } else {
        Write-Host "   ✗ Solution restore failed" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "   ✗ Solution restore failed" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Test build
Write-Host "4. Testing solution build..." -ForegroundColor Yellow
try {
    dotnet build Microsoft.Maui.sln --no-restore --verbosity quiet
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   ✓ Solution build successful" -ForegroundColor Green
    } else {
        Write-Host "   ✗ Solution build failed" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "   ✗ Solution build failed" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "🎉 Development environment is properly configured!" -ForegroundColor Green
Write-Host "You can now work with the Flagstone UI solution." -ForegroundColor Green