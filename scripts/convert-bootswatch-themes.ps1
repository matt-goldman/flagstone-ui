#!/usr/bin/env pwsh
# Script to download and convert Bootswatch themes to Flagstone UI

param(
    [string[]]$Themes = @("minty", "brite", "litera"),
    [string]$OutputDir = "samples/FlagstoneUI.SampleApp/Resources/Styles",
    [switch]$Debug
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Bootswatch Theme Converter" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Bootswatch version
$bootswatchVersion = "5.3.8"
$baseUrl = "https://raw.githubusercontent.com/thomaspark/bootswatch/v$bootswatchVersion/dist"

foreach ($theme in $Themes) {
    Write-Host "Processing theme: $theme" -ForegroundColor Yellow
    Write-Host "----------------------------------------"
    
    # Create temp directory for downloads
    $tempDir = "temp-$theme"
    New-Item -ItemType Directory -Path $tempDir -Force | Out-Null
    
    # Download _variables.scss
    $variablesUrl = "$baseUrl/$theme/_variables.scss"
    $variablesPath = Join-Path $tempDir "_variables.scss"
    Write-Host "  Downloading _variables.scss..." -NoNewline
    try {
        Invoke-WebRequest -Uri $variablesUrl -OutFile $variablesPath -UseBasicParsing
        Write-Host " ✓" -ForegroundColor Green
    }
    catch {
        Write-Host " ✗" -ForegroundColor Red
        Write-Host "  Error: $_" -ForegroundColor Red
        continue
    }
    
    # Download _bootswatch.scss
    $bootswatchUrl = "$baseUrl/$theme/_bootswatch.scss"
    $bootswatchPath = Join-Path $tempDir "_bootswatch.scss"
    Write-Host "  Downloading _bootswatch.scss..." -NoNewline
    try {
        Invoke-WebRequest -Uri $bootswatchUrl -OutFile $bootswatchPath -UseBasicParsing
        Write-Host " ✓" -ForegroundColor Green
    }
    catch {
        Write-Host " ✗" -ForegroundColor Red
        Write-Host "  Error: $_" -ForegroundColor Red
        continue
    }
    
    # Convert to Flagstone theme
    Write-Host "  Converting to Flagstone UI..." -NoNewline
    $themeTitleCase = (Get-Culture).TextInfo.ToTitleCase($theme.ToLower())
    $themeOutputDir = Join-Path $tempDir "output"
    
    $convertArgs = @(
        "run",
        "--project", "tools/FlagstoneUI.BootstrapConverter.Cli/FlagstoneUI.BootstrapConverter.Cli.csproj",
        "--",
        "convert",
        "-i", $variablesPath,
        "-i", $bootswatchPath,
        "-o", $themeOutputDir,
        "--namespace", "FlagstoneUI.SampleApp.Resources.Styles"
    )
    
    if ($Debug) {
        $convertArgs += "--debug"
    }
    
    try {
        $output = & dotnet @convertArgs 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Host " ✗" -ForegroundColor Red
            Write-Host "  Error during conversion:" -ForegroundColor Red
            Write-Host $output -ForegroundColor Red
            continue
        }
        Write-Host " ✓" -ForegroundColor Green
    }
    catch {
        Write-Host " ✗" -ForegroundColor Red
        Write-Host "  Error: $_" -ForegroundColor Red
        continue
    }
    
    # Copy generated files to output directory
    $tokensFile = Join-Path $themeOutputDir "Tokens.xaml"
    $targetFile = Join-Path $OutputDir "$themeTitleCase.xaml"
    
    if (Test-Path $tokensFile) {
        Write-Host "  Copying to $targetFile..." -NoNewline
        Copy-Item $tokensFile $targetFile -Force
        Write-Host " ✓" -ForegroundColor Green
    }
    else {
        Write-Host "  Warning: Tokens.xaml not found" -ForegroundColor Yellow
    }
    
    # Clean up temp directory
    Write-Host "  Cleaning up..." -NoNewline
    Remove-Item -Path $tempDir -Recurse -Force
    Write-Host " ✓" -ForegroundColor Green
    
    Write-Host ""
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Conversion complete!" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
