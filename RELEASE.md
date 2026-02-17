# Release Process

This document describes how to create and publish releases for FunctionalStateMachine.

## Overview

The project uses GitHub Actions for automated releases. When a tag is pushed with the format `vX.Y.Z`, the workflow:

1. Builds and tests the solution
2. Extracts version-specific release notes from CHANGELOG.md
3. Packs NuGet packages with the release notes embedded
4. Publishes to NuGet.org using Trusted Publishing (OIDC)
5. Creates a GitHub release with the packages attached

## Release Steps

### 1. Update CHANGELOG.md

Before creating a release, ensure CHANGELOG.md is updated with all changes for the new version:

```markdown
## [X.Y.Z] - YYYY-MM-DD

### Added
- New feature descriptions

### Changed
- Modified feature descriptions

### Fixed
- Bug fix descriptions
```

Follow the [Keep a Changelog](https://keepachangelog.com/) format.

### 2. Create and Push a Tag

Create a Git tag with the version number prefixed with `v`:

```bash
git tag -a v1.2.0 -m "Release v1.2.0"
git push origin v1.2.0
```

### 3. Automated Release

The GitHub Actions workflow (`.github/workflows/release.yml`) will:

1. Extract the version number from the tag (e.g., `v1.2.0` → `1.2.0`)
2. Run the `scripts/extract-changelog.ps1` script to extract release notes for that version
3. Build and test the solution
4. Pack the following NuGet packages with embedded release notes:
   - `FunctionalStateMachine.Core`
   - `FunctionalStateMachine.CommandRunner`
   - `FunctionalStateMachine.Diagrams`
5. Publish all packages to NuGet.org
6. Create a GitHub release with the packages attached

## Release Notes in NuGet

The release notes are automatically extracted from CHANGELOG.md and embedded in each NuGet package's `.nuspec` file. This means users viewing the package on NuGet.org will see the full changelog entry for that version instead of just "See CHANGELOG.md for release notes".

Additionally, the CHANGELOG.md file itself is included in each NuGet package, so users can view the complete project history.

## Trusted Publishing

The project uses [NuGet Trusted Publishing](https://devblogs.microsoft.com/nuget/introducing-nuget-trusted-publishing/) with OpenID Connect (OIDC) for authentication. This eliminates the need for long-lived API keys.

### Prerequisites

- The `NUGET_USERNAME` secret must be configured in the GitHub repository
- The NuGet.org account must have Trusted Publishing configured for this repository

## Testing Release Notes Extraction

You can test the changelog extraction script locally:

```bash
pwsh scripts/extract-changelog.ps1 -Version "1.2.0"
```

This will output the release notes for version 1.2.0 from CHANGELOG.md.

## Manual Package Build

To build packages locally with release notes:

```bash
# Extract release notes
pwsh scripts/extract-changelog.ps1 -Version "1.2.0" > release-notes.txt

# Build solution
dotnet build FunctionalStateMachine.sln --configuration Release

# Pack with release notes
RELEASE_NOTES=$(cat release-notes.txt)
dotnet pack src/FunctionalStateMachine.Core/FunctionalStateMachine.Core.csproj \
  --configuration Release \
  --no-build \
  --output ./artifacts \
  /p:PackageVersion=1.2.0 \
  /p:PackageReleaseNotes="$RELEASE_NOTES"
```

## Troubleshooting

### Release Notes Not Showing

If release notes don't appear in the NuGet package:

1. Verify the version exists in CHANGELOG.md with the exact format: `## [X.Y.Z] - YYYY-MM-DD`
2. Check the GitHub Actions workflow logs to see what was extracted
3. Inspect the `.nuspec` file inside the `.nupkg` (it's a ZIP file) to verify the `<releaseNotes>` element

### Version Not Found in Changelog

If the extract-changelog script reports "Version X.Y.Z not found":

1. Ensure CHANGELOG.md has an entry with the exact version number
2. Check that the version format matches: `## [X.Y.Z] - YYYY-MM-DD`
3. Verify there are no extra spaces or formatting issues

## See Also

- [CHANGELOG.md](CHANGELOG.md) - Complete project history
- [Keep a Changelog](https://keepachangelog.com/) - Changelog format specification
- [Semantic Versioning](https://semver.org/) - Version numbering scheme
- [NuGet Trusted Publishing](https://devblogs.microsoft.com/nuget/introducing-nuget-trusted-publishing/) - Authentication method
