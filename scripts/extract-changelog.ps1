#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Extracts release notes for a specific version from CHANGELOG.md
.DESCRIPTION
    Parses CHANGELOG.md and extracts the section for the specified version,
    formatting it appropriately for NuGet package release notes.
.PARAMETER Version
    The version to extract (e.g., "1.0.0")
.PARAMETER ChangelogPath
    Path to the CHANGELOG.md file (defaults to CHANGELOG.md in the script's parent directory)
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$Version,
    
    [Parameter(Mandatory=$false)]
    [string]$ChangelogPath = ""
)

# Set default changelog path if not provided
if ([string]::IsNullOrEmpty($ChangelogPath)) {
    $scriptDir = Split-Path -Parent $PSCommandPath
    $ChangelogPath = Join-Path (Split-Path -Parent $scriptDir) "CHANGELOG.md"
}

# Verify the changelog file exists
if (-not (Test-Path $ChangelogPath)) {
    Write-Error "CHANGELOG.md not found at: $ChangelogPath"
    exit 1
}

# Read the changelog content
$content = Get-Content $ChangelogPath -Raw

# Build the regex pattern to match the version section
# Matches: ## [version] - date through to the next version header or end of file
$versionPattern = "(?s)## \[$([regex]::Escape($Version))\].*?(?=\n## \[|\z)"

# Extract the version section
if ($content -match $versionPattern) {
    $versionSection = $matches[0]
    
    # Remove the version header line (## [version] - date)
    $lines = $versionSection -split "`r?`n", 0, 'RegexMatch'
    $releaseNotes = ($lines | Select-Object -Skip 1) -join "`n"
    
    # Trim leading/trailing whitespace
    $releaseNotes = $releaseNotes.Trim()
    
    # Output the release notes
    Write-Output $releaseNotes
} else {
    Write-Warning "Version $Version not found in CHANGELOG.md"
    Write-Output "See https://github.com/leeoades/FunctionalStateMachine/blob/main/CHANGELOG.md for release notes"
}
