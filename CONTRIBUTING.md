# Contributing to Functional State Machine

Thank you for your interest in contributing! This document provides guidelines and instructions for contributing to this project.

## Code of Conduct

This project adheres to a Code of Conduct that all contributors are expected to follow. Please be respectful and constructive in all interactions.

## Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download) or later
- Git
- A code editor (Visual Studio, VS Code, Rider, etc.)

### Building the Project

```bash
# Clone the repository
git clone https://github.com/leeoades/FunctionalStateMachine.git
cd FunctionalStateMachine

# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run tests
dotnet test
```

## How to Contribute

### Updating AI Documentation

When adding features or making API changes, please update the AI-focused documentation:

- **`.copilot-instructions.md`** - Comprehensive guide for AI assistants
- **`AI-USAGE-GUIDE.md`** - Quick reference guide (included in NuGet packages)

See [docs/AI-Documentation-Maintenance.md](docs/AI-Documentation-Maintenance.md) for detailed guidance on keeping AI documentation current.

### Reporting Bugs

Before creating a bug report, please check existing issues to avoid duplicates. When creating a bug report, include:

- A clear, descriptive title
- Steps to reproduce the issue
- Expected behavior
- Actual behavior
- .NET version and OS information
- Code samples demonstrating the issue

### Suggesting Enhancements

Enhancement suggestions are welcome! Please provide:

- A clear description of the enhancement
- Use cases and benefits
- Example API or usage patterns
- Any potential drawbacks or concerns

### Pull Requests

1. **Fork the repository** and create a branch from `main`
2. **Make your changes** following the coding conventions below
3. **Add tests** for any new functionality
4. **Update documentation** if needed (README.md, docs/)
5. **Run tests** to ensure everything passes
6. **Commit your changes** with clear, descriptive messages
7. **Push to your fork** and submit a pull request

#### Pull Request Guidelines

- Keep changes focused and atomic
- Follow existing code style and conventions
- Include unit tests for new features
- Update CHANGELOG.md for notable changes
- Ensure all tests pass
- Keep commits clean and well-documented

## Coding Conventions

### C# Style

- Use C# 9+ features (records, init properties, pattern matching)
- Prefer immutability and functional patterns
- Use explicit types rather than `var` for clarity
- Follow standard .NET naming conventions
- Enable nullable reference types

### Testing

- Write tests using xUnit
- Follow the Arrange-Act-Assert pattern
- Test both happy paths and edge cases
- Use descriptive test names: `GivenX_WhenY_ThenZ`

### Documentation

- Update inline XML documentation for public APIs
- Add examples to README.md for major features
- Update relevant documentation in `/docs` folder
- Keep CHANGELOG.md updated with notable changes

## Project Structure

```
FunctionalStateMachine/
├── src/                     # Library source code
│   ├── Core/               # Core state machine implementation
│   ├── CommandRunner/      # DI-based command execution
│   ├── CommandRunner.Generator/  # Source generators
│   └── Diagrams/           # Mermaid diagram generation
├── test/                    # Unit tests
├── samples/                 # Example applications
├── docs/                    # Feature documentation
└── scripts/                 # Build and release scripts
```

## Development Workflow

### Adding a New Feature

1. **Create an issue** describing the feature
2. **Discuss the approach** with maintainers
3. **Implement the feature** with tests
4. **Update documentation** (README, docs/)
5. **Add CHANGELOG entry** under `[Unreleased]`
6. **Submit pull request** for review

### Fixing a Bug

1. **Write a failing test** that reproduces the bug
2. **Fix the bug** with minimal changes
3. **Verify the test passes**
4. **Add CHANGELOG entry** under `[Unreleased]`
5. **Submit pull request** with bug description

## Dependency Management

### Dependabot Auto-Merge

This repository uses Dependabot to keep dependencies up to date. To reduce maintenance burden, Dependabot PRs for **minor** and **patch** updates are automatically merged when all tests pass.

**How it works:**
- Dependabot creates PRs for NuGet and GitHub Actions updates weekly
- Minor (0.x.0) and patch (0.0.x) updates are auto-merged after CI passes
- Major version updates (x.0.0) require manual review due to potential breaking changes

**Security:**
- Auto-merge only applies to version updates, not security vulnerabilities
- All updates run the full CI test suite before merging
- Failed tests block auto-merge

## Release Process

Releases are managed by maintainers following semantic versioning:

- **Patch** (0.0.x): Bug fixes, documentation updates
- **Minor** (0.x.0): New features, backward-compatible changes
- **Major** (x.0.0): Breaking API changes

## Questions?

If you have questions or need help, please:

- Check the [documentation](docs/)
- Review [existing issues](https://github.com/leeoades/FunctionalStateMachine/issues)
- Open a new issue with the `question` label

## License

By contributing, you agree that your contributions will be licensed under the MIT License.
