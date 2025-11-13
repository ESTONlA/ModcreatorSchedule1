# Release Process

This document describes the release process for Schedule 1 Modding Tool, including beta/nightly releases and stable releases.

## Overview

The project uses a dual-channel release system:
- **Beta Channel**: Frequent updates with latest features, may contain bugs
- **Stable Channel**: Tested releases, recommended for production use

## Versioning Scheme

We use semantic versioning with optional pre-release identifiers:
- Stable: `1.0.0`, `1.1.0`, `2.0.0`
- Beta: `1.0.0-beta.1`, `1.0.0-beta.2`, `1.1.0-beta.1`

## Beta Releases

Beta releases are created automatically when code is pushed to the `beta` branch.

### Creating a Beta Release

1. **Merge changes to beta branch**:
   ```bash
   git checkout beta
   git merge develop  # or your feature branch
   git push origin beta
   ```

2. **GitHub Actions will automatically**:
   - Build the project in Release configuration
   - Extract version from `Schedule1ModdingTool.csproj`
   - Create a GitHub Release with `beta` tag
   - Upload release artifacts
   - Mark release as pre-release

3. **Manual beta release** (via GitHub Actions):
   - Go to Actions → Release Beta → Run workflow
   - Optionally specify a version tag
   - Click "Run workflow"

### Beta Release Workflow

The `.github/workflows/release-beta.yml` workflow:
- Triggers on push to `beta` branch
- Can be manually triggered via workflow_dispatch
- Creates pre-release GitHub releases
- Uses version from project file (with `-beta` suffix if not present)

## Stable Releases

Stable releases are created when a version tag is pushed or when manually triggered via GitHub Actions. Pushing to `main` branch does **not** automatically trigger a release.

### Promoting Beta to Stable

1. **Merge beta to main and create version tag**:
   ```bash
   git checkout main
   git merge beta
   # Update version in Schedule1ModdingTool.csproj to remove -beta suffix
   git add ModcreatorSchedule1/Schedule1ModdingTool.csproj
   git commit -m "chore: promote beta to stable v1.0.0"
   git push origin main
   
   # Create and push version tag to trigger stable release
   git tag v1.0.0
   git push origin v1.0.0
   ```

2. **GitHub Actions will automatically**:
   - Build the project in Release configuration
   - Extract version from tag
   - Create a GitHub Release with version tag
   - Upload release artifacts
   - Mark as stable release (not pre-release)

### Manual Stable Release

1. **Create and push a version tag**:
   ```bash
   git tag v1.0.0
   git push origin v1.0.0
   ```

2. **Or use GitHub Actions**:
   - Go to Actions → Release Stable → Run workflow
   - Specify version tag
   - Click "Run workflow"

### Stable Release Workflow

The `.github/workflows/release-stable.yml` workflow:
- Triggers on version tags (e.g., `v1.0.0`) - **recommended method**
- Can be manually triggered via workflow_dispatch
- Does **not** trigger on push to `main` branch (to prevent accidental releases)
- Creates stable GitHub releases
- Uses version from tag, project file, or manual input

## Update Channels

Users can choose their update channel in Settings:
- **Stable**: Only receives stable releases
- **Beta**: Receives both beta and stable releases

### Channel Detection

- Users who install via beta installer are automatically on beta channel
- First release will be beta (default channel is Beta)
- Users can change channel in Settings → User Preferences → Update Channel

## Auto-Updater Configuration

The application uses AutoUpdater.NET with GitHub Releases integration:
- Update check URL: `https://github.com/ESTONlA/ModcreatorSchedule1/releases/latest`
- Checks for updates on startup (silent, non-blocking)
- Manual check available via Tools → Check for Updates
- Respects user's channel preference (Stable/Beta)

## Release Checklist

Before creating a stable release:

- [ ] All features are tested
- [ ] Version number is updated in `Schedule1ModdingTool.csproj`
- [ ] CHANGELOG.md is updated (if maintained)
- [ ] Beta branch is merged to main
- [ ] Version tag is created (if using tag-based release)
- [ ] Release notes are prepared

## GitHub Releases

Each release includes:
- Version tag (e.g., `v1.0.0` or `v1.0.0-beta.1`)
- Release notes (from CHANGELOG.md or auto-generated)
- Release artifacts (ZIP archive with published application)
- Pre-release flag (for beta releases)

## Troubleshooting

### Release not created

- Check GitHub Actions logs for errors
- Verify version format in project file
- Ensure GITHUB_TOKEN has release permissions

### Auto-updater not working

- Verify GitHub Releases are public
- Check update channel setting in user settings
- Ensure release assets are uploaded correctly
- Check application logs for update check errors

## Version Management

Version is stored in `Schedule1ModdingTool.csproj`:
```xml
<Version>1.0.0-beta.1</Version>
<AssemblyVersion>1.0.0.0</AssemblyVersion>
<FileVersion>1.0.0.0</FileVersion>
```

The `Version` property is used for:
- GitHub Releases
- Auto-updater version comparison
- Application version display

AssemblyVersion and FileVersion follow .NET conventions (major.minor.build.revision).

