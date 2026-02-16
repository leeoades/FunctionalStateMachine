# Publishing Documentation to GitHub Wiki

## Why No Automated Wiki Publishing?

GitHub wikis cannot be automatically initialized via GitHub Actions or the GitHub API. A wiki repository (`<repo>.wiki.git`) only exists after you manually create at least one page through the GitHub web interface.

### The Problem

- GitHub Actions cannot create a wiki repository programmatically
- The `git clone` operation fails with "Repository not found" until the wiki is manually initialized
- There is no GitHub REST API endpoint for creating or initializing wikis

### Previous Attempt

A GitHub Action workflow (`.github/workflows/publish-wiki.yml`) was created to automatically publish the `docs/` folder to the wiki, but it failed because the wiki repository didn't exist. This workflow has been removed.

## Alternative Approaches

Since automated wiki publishing isn't practical, here are recommended alternatives:

### Option 1: GitHub Pages (Recommended)

GitHub Pages provides automated, searchable documentation hosting with better tooling support.

**Advantages:**
- Fully automated via GitHub Actions
- Better SEO (search engines index Pages but not wikis under 500 stars)
- Version control and PR workflow
- Supports custom domains and themes
- No manual initialization required

**Setup:**
```yaml
# .github/workflows/publish-docs.yml
name: Publish Documentation

on:
  push:
    branches: [main]
    paths: ['docs/**']

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Deploy to GitHub Pages
        uses: peaceiris/actions-gh-pages@v3
        with:
          github_token: ${{ secrets.GITHUB_TOKEN }}
          publish_dir: ./docs
```

### Option 2: Manual Wiki Setup

If you prefer to use GitHub wikis:

1. **Initialize the wiki manually:**
   - Go to your repository's Wiki tab
   - Click "Create the first page"
   - Add any content and save

2. **Set up automated publishing:**
   ```bash
   # Clone the wiki repository
   git clone https://github.com/your-org/your-repo.wiki.git
   
   # Copy docs and push
   cp -r docs/*.md your-repo.wiki/
   cd your-repo.wiki
   git add .
   git commit -m "Update wiki"
   git push
   ```

3. **Add the workflow back:**
   - The workflow can work once the wiki is manually initialized
   - Note: This still requires the wiki to exist before first run

### Option 3: Link to Docs Folder

The simplest approach - keep documentation in the `docs/` folder and link to it from the README.

**Advantages:**
- Zero setup required
- Version control included
- PR review for documentation changes
- Works immediately

**Implementation:**
```markdown
## Documentation

Browse the documentation in the [docs/](docs/) folder:
- [Commands vs Side Effects](docs/commands-vs-effects.md)
- [Fluent Configuration](docs/fluent-configuration.md)
- [Guards](docs/guards.md)
- [And more...](docs/)
```

## Current State

The documentation is currently available in the [`docs/`](../docs/) folder and linked from the main README. This provides immediate access without any additional setup.

If you want to enable wiki publishing in the future, you must first manually initialize the wiki through the GitHub web interface before any automated workflow can function.
