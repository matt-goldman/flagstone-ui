# GitHub Copilot Setup Instructions

This document provides setup instructions for GitHub Copilot to work effectively with the Flagstone UI repository.

## Prerequisites

### Environment Limitations

- **GitHub Copilot only supports `ubuntu-latest`** runners
- This project targets multiple .NET MAUI platforms including Windows-specific targets
- For full compatibility, focus on **Android target framework** (`net9.0-android`) when building/testing in Copilot workflows

### Required Software

1. **.NET 9 SDK** (minimum version 9.0.100 as specified in `global.json`)
2. **MAUI workload** for .NET MAUI development
3. **Git LFS** for large file support

## Setup Steps

### Automatic Setup (Recommended)

This repository includes a GitHub Actions workflow (`.github/workflows/copilot-setup-steps.yml`) that **automatically configures the Copilot environment**. When GitHub Copilot starts working, it will:

1. Install .NET 9 SDK using the version specified in `global.json`
2. Install the MAUI workload
3. Enable Git LFS support
4. Restore project dependencies
5. Verify Android build capability

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

#### 2. Install MAUI Workload

```bash
dotnet workload install maui --ignore-failed-sources
```

#### 3. Configure Git LFS

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
- All builds target `net9.0-android` framework

#### For Ubuntu-Latest (Manual Development)

If working manually in ubuntu-latest environments:

```bash
# Restore packages
dotnet restore

# Build Android target only
dotnet build --framework net9.0-android --no-restore

# Test Android target only
dotnet test --framework net9.0-android --no-build
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
dotnet build --framework net9.0-android
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
- **Target Framework Selection**: When building manually in ubuntu-latest, always specify `--framework net9.0-android` to avoid platform compatibility issues
- **Windows/iOS Development**: Full Windows and iOS development requires Windows/macOS hosts respectively
- **Git LFS**: Configured for new binary assets; existing sample files are excluded from LFS to maintain compatibility
- **MAUI Workload**: Required even for test projects due to target framework compatibility
- **Setup Workflow Testing**: The copilot-setup-steps workflow can be manually tested via the repository's Actions tab

## Troubleshooting

### Common Issues

1. **SDK Version Mismatch**: Ensure .NET 9.0.100 or later is installed
2. **MAUI Workload Missing**: Run `dotnet workload install maui` if build fails
3. **Platform Target Errors**: Use `--framework net9.0-android` for ubuntu-latest builds
4. **Git LFS Not Configured**: Run `git lfs install` in the repository

### Solution File Structure

- **Main Solution**: `Flagstone.UI.sln` (contains all projects)
- **Target Frameworks**: `net9.0-android`, `net9.0-ios`, `net9.0-maccatalyst`, `net9.0-windows10.0.19041.0`
- **Test Projects**: Located in `tests/` directory
- **Source Projects**: Located in `src/` directory

## References

- [Repository README](README.md) - Full developer setup instructions
- [Copilot Instructions](.github/copilot-instructions.md) - AI agent guidance
- [Setup Workflow](.github/workflows/copilot-setup-steps.yml) - Automated environment configuration
- [Technical Documentation](docs/) - Architecture and implementation details
- [GitHub Copilot Environment Customization](https://docs.github.com/en/enterprise-cloud@latest/copilot/how-tos/use-copilot-agents/coding-agent/customize-the-agent-environment) - Official GitHub documentation