# Package Assets

This directory contains assets used for NuGet packages and GitHub repository.

## Files

- `icon.png` - Package icon (128x128px) used in NuGet.org listings

## Icon Design

The icon represents a state machine with interconnected nodes forming a flow, symbolizing:
- **Transitions**: Arrows showing state flow
- **Functional**: Clean, mathematical representation
- **Commands**: Output-focused design

## Usage

The icon is referenced in package metadata via Directory.Build.props:

```xml
<PackageIcon>icon.png</PackageIcon>
```

## Creating the Icon

If you need to regenerate the icon, use any vector graphics tool:

1. Canvas: 128x128px
2. Style: Modern, minimal, tech-focused
3. Colors: Blue gradient (#0066CC to #00AAFF)
4. Export: PNG with transparent background
5. Optimize: Use ImageOptim or similar

## License

The icon follows the same MIT license as the project.
