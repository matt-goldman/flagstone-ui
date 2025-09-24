# GitHub Copilot Setup Instructions

This document provides setup instructions for GitHub Copilot to work effectively with the Flagstone UI repository.

## Prerequisites

### Environment Limitations

- **GitHub Copilot only supports `ubuntu-latest`** runners
- This project targets multiple .NET MAUI platforms including Windows-specific targets
- For full compatibility, focus on **Android target framework** (`net10.0-android`) when building/testing in Copilot workflows

### Required Software

1. **.NET 9 SDK** (minimum version 9.0.100 as specified in `global.json`)
2. **MAUI workload** for .NET MAUI development
3. **Git LFS** for large file support

## Setup Steps

### Automatic Setup (Recommended)

This repository includes a GitHub Actions workflow (`.github/workflows/copilot-setup-steps.yml`) that **automatically configures the Copilot environment**. When GitHub Copilot starts working, it will:

1. Install .NET 9 SDK using the version specified in `global.json`
2. Install Linux dependencies (OpenJDK 17, set JAVA_HOME)
3. Install the MAUI Android workload (Linux-specific)
4. Install Android SDK dependencies automatically
5. Enable Git LFS support
6. Restore project dependencies
7. Verify Android build capability

**No manual setup is required** when using GitHub Copilot - the environment will be configured automatically.

### Manual Setup (For Reference)

If you need to set up the environment manually for development or testing:

#### 1. Install .NET 9 SDK

```bash
# Download and install .NET 9 SDK
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --version 9.0.100
export PATH="$HOME/.dotnet:$PATH"
```

#### 2. Install Linux Dependencies for .NET MAUI

**Java SDK (OpenJDK 17):**
```bash
# Install OpenJDK 17 (required for Android development)
sudo apt-get update
sudo apt-get install -y openjdk-17-jdk

# Set JAVA_HOME environment variable
export JAVA_HOME=/usr/lib/jvm/java-17-openjdk-amd64
echo 'export JAVA_HOME=/usr/lib/jvm/java-17-openjdk-amd64' >> ~/.bashrc
```

#### 3. Install MAUI Android Workload

```bash
# Linux uses maui-android workload (not the full maui workload)
dotnet workload install maui-android --ignore-failed-sources
```

#### 4. Install Android SDK Dependencies

```bash
# Create directory for Android SDK
mkdir -p $HOME/android-sdk
export ANDROID_HOME=$HOME/android-sdk
echo 'export ANDROID_HOME=$HOME/android-sdk' >> ~/.bashrc

# Use InstallAndroidDependencies target with a specific Android-capable project
dotnet build src/FlagstoneUI.Core/FlagstoneUI.Core.csproj -t:InstallAndroidDependencies --framework net10.0-android \
  -p:AndroidSdkDirectory=$HOME/android-sdk \
  -p:JavaSdkDirectory=$JAVA_HOME \
  -p:AcceptAndroidSdkLicenses=True
```

#### 5. Configure Git LFS

```bash
# Install Git LFS
sudo apt-get update
sudo apt-get install git-lfs

# Initialize LFS in the repository
git lfs install
```

**Note**: The repository includes `.gitattributes` configuration for LFS support, but existing sample files are excluded from LFS to avoid conflicts. LFS will be used for new binary assets added to the repository.

### 4. Platform-Specific Build Instructions

#### For GitHub Copilot (Automatic)

When using GitHub Copilot, the environment is **automatically configured** via the setup workflow. Copilot will use Android-specific commands by default:

- Dependencies are pre-restored
- Android TFM is validated during setup
- All builds target `net10.0-android` framework

#### For Ubuntu-Latest (Manual Development)

If working manually in ubuntu-latest environments:

```bash
# Restore packages
dotnet restore

# Build Android target only
dotnet build --framework net10.0-android --no-restore

# Test Android target only
dotnet test --framework net10.0-android --no-build
```

#### For Cross-Platform Development

When working locally with full platform support:

```bash
# Full restore and build
dotnet restore Flagstone.UI.sln
dotnet build Flagstone.UI.sln

# Run all tests
dotnet test Flagstone.UI.sln
```

## Validation

### Quick Environment Check

```bash
# Verify .NET version
dotnet --version

# Verify MAUI workload
dotnet workload list | grep maui

# Verify Git LFS
git lfs version

# Test project restore
dotnet restore

# Test Android build
dotnet build --framework net10.0-android
```

### Using Validation Scripts

The repository includes a Copilot-specific validation script for ubuntu-latest environments:

```bash
# Copilot/ubuntu-latest specific validation
./scripts/validate-copilot-setup.sh
```

For full cross-platform development environments:

```bash
# Linux/macOS (full platform support)
./scripts/validate-setup.sh

# Note: Standard validation scripts may fail on ubuntu-latest due to platform limitations
```

## Important Notes

- **Automatic Environment Setup**: GitHub Copilot uses `.github/workflows/copilot-setup-steps.yml` to automatically configure the development environment
- **Linux-Specific Requirements**: Ubuntu-latest requires OpenJDK 17, Android SDK, and `maui-android` workload (not full `maui`)
- **Target Framework Selection**: When building manually in ubuntu-latest, always specify `--framework net10.0-android` to avoid platform compatibility issues
- **Windows/iOS Development**: Full Windows and iOS development requires Windows/macOS hosts respectively
- **Git LFS**: Configured for new binary assets; existing sample files are excluded from LFS to maintain compatibility
- **MAUI Workload**: Required even for test projects due to target framework compatibility
- **Setup Workflow Testing**: The copilot-setup-steps workflow can be manually tested via the repository's Actions tab

## Troubleshooting

### Common Issues

1. **SDK Version Mismatch**: Ensure .NET 9.0.100 or later is installed
2. **MAUI Workload Missing**: Run `dotnet workload install maui-android` (not `maui`) on Linux
3. **Java SDK Missing**: Install OpenJDK 17 and set JAVA_HOME environment variable
4. **Android SDK Issues**: Use InstallAndroidDependencies target for automatic setup
5. **Platform Target Errors**: Use `--framework net10.0-android` for ubuntu-latest builds
6. **Git LFS Not Configured**: Run `git lfs install` in the repository

### Linux-Specific Issues

1. **Permission Errors**: Use `sudo` for package installations but not for dotnet commands
2. **JAVA_HOME Not Found**: Ensure path `/usr/lib/jvm/java-17-openjdk-amd64` exists after OpenJDK installation
3. **Android SDK License**: The InstallAndroidDependencies target handles license acceptance automatically
4. **Workload Installation Fails**: Use `--ignore-failed-sources` flag with workload install commands
5. **Windows Targeting Error**: Use specific Android-capable projects (e.g., `src/FlagstoneUI.Core/FlagstoneUI.Core.csproj`) for InstallAndroidDependencies target to avoid Windows-specific test projects

### Solution File Structure

- **Main Solution**: `Flagstone.UI.sln` (contains all projects)
- **Target Frameworks**: `net10.0-android`, `net10.0-ios`, `net10.0-maccatalyst`, `net10.0-windows10.0.19041.0`
- **Test Projects**: Located in `tests/` directory
- **Source Projects**: Located in `src/` directory

## References

- [Repository README](README.md) - Full developer setup instructions
- [Copilot Instructions](.github/copilot-instructions.md) - AI agent guidance
- [Setup Workflow](.github/workflows/copilot-setup-steps.yml) - Automated environment configuration
- [Technical Documentation](docs/) - Architecture and implementation details
- [GitHub Copilot Environment Customization](https://docs.github.com/en/enterprise-cloud@latest/copilot/how-tos/use-copilot-agents/coding-agent/customize-the-agent-environment) - Official GitHub documentation
- [.NET MAUI Installation on Linux](https://learn.microsoft.com/en-us/dotnet/maui/get-started/installation?view=net-maui-9.0&tabs=visual-studio-code) - Microsoft documentation
- [.NET Core Installation on Linux](https://learn.microsoft.com/en-us/dotnet/core/install/linux) - Linux-specific .NET setup guidance