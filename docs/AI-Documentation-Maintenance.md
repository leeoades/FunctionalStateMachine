# Maintaining AI Documentation

This document provides guidance for maintainers on keeping the AI-focused documentation up-to-date as new features are added to FunctionalStateMachine.

## AI Documentation Files

1. **`.copilot-instructions.md`** - Comprehensive guide for AI assistants in the repository
2. **`AI-USAGE-GUIDE.md`** - Condensed quick-start guide included in NuGet packages

## When to Update AI Documentation

Update the AI documentation files when:

- ✅ Adding a new feature or capability
- ✅ Changing existing API behavior
- ✅ Adding new builder methods or patterns
- ✅ Deprecating features
- ✅ Adding new packages
- ✅ Changing recommended patterns or best practices
- ✅ Fixing common pitfalls or issues

## Update Process

### 1. For New Features

When adding a new feature:

1. **Update Feature Documentation First**
   - Create or update the relevant file in `/docs`
   - Add entry to `/docs/index.md`
   - Update examples in `/samples` if applicable

2. **Update `.copilot-instructions.md`**
   - Add new pattern to "Common Patterns" section if it's a major feature
   - Update "Key Builder Methods" table if adding new builder methods
   - Add to "Execute Function Signatures" or "ModifyData Function Signatures" if adding new overloads
   - Update "Quick Reference: File Structure" if it affects project structure
   - Add to "Common Mistakes to Avoid" if there are common pitfalls
   - Update "Version Information" section to list the new feature

3. **Update `AI-USAGE-GUIDE.md`**
   - Add to "Key Patterns" section if it's a commonly-used feature
   - Update "Complete Example" if it demonstrates a fundamental pattern
   - Add to "Common Pitfalls" table if applicable
   - Update "When Updating for New Features" section if there are migration considerations

4. **Update `README.md`**
   - Add feature to appropriate section with example
   - Link to detailed documentation
   - Update feature list if it's a major addition

### 2. For API Changes

When modifying existing APIs:

1. **Update all examples** that use the changed API
2. **Update pattern descriptions** in both AI documentation files
3. **Add deprecation notes** if deprecating old patterns
4. **Update "Common Mistakes"** if the change prevents a common error

### 3. For Breaking Changes

When making breaking changes:

1. **Update CHANGELOG.md** with clear migration instructions
2. **Add migration notes** to both AI documentation files
3. **Update all examples** to use new patterns
4. **Consider adding a "Migration Guide"** section if the change is significant

## Documentation Structure Guidelines

### `.copilot-instructions.md` Structure

This file should maintain these sections in order:

1. **Overview** - Brief description of the library
2. **Package Installation** - How to install
3. **Core Type Pattern** - The fundamental type system
4. **Fluent Builder Pattern** - How to build state machines
5. **Common Patterns** - Numbered patterns for frequent use cases
6. **Command Dispatching** - Optional package usage
7. **Testing State Machines** - How to test
8. **Common Mistakes to Avoid** - Anti-patterns
9. **Data Access Patterns** - Function signatures
10. **Validation and Analysis** - Build-time checks
11. **Diagram Generation** - Optional tooling
12. **When to Use This Library** - Use cases
13. **Quick Reference: File Structure** - Project organization
14. **Version Information** - Feature list
15. **Further Documentation** - Links
16. **Quick Troubleshooting** - Common issues
17. **Summary Checklist** - Implementation steps

### `AI-USAGE-GUIDE.md` Structure

This file should maintain these sections in order:

1. **Package** - Quick install command
2. **Essential Type Pattern** - Core type definitions
3. **Builder Pattern** - Basic building example
4. **Execution** - How to fire and execute
5. **Key Patterns** - Most common patterns only
6. **Critical Rules** - Do's and don'ts
7. **Command Dispatching** - Optional package
8. **Execute Function Signatures** - Available overloads
9. **ModifyData Function Signatures** - Available overloads
10. **Common Pitfalls** - Table format
11. **Complete Example** - End-to-end example
12. **Documentation** - Links to more info
13. **Version Detection** - How to check versions
14. **When Updating for New Features** - Change process

## Writing Style Guidelines

### For AI Assistants

- ✅ Use imperative mood: "Add trigger type" not "You should add trigger type"
- ✅ Provide concrete examples for every pattern
- ✅ Include type signatures and parameter names
- ✅ Show both simple and complex variants
- ✅ Use consistent naming in examples (MyState, MyTrigger, MyData, MyCommand)
- ✅ Explain *why* a pattern exists, not just *how* to use it
- ✅ Include anti-patterns with ❌ markers
- ✅ Include best practices with ✅ markers
- ✅ Use code blocks with `csharp` language identifier
- ✅ Keep examples self-contained and copyable

### For Patterns

When documenting a pattern:

1. **Title** - Clear, descriptive name
2. **Example** - Working code snippet
3. **Explanation** - Key behaviors or notes in bold
4. **Use case** - When to apply this pattern

Example:
```markdown
### Pattern N: Pattern Name

[code example]

**Key behavior description.** Additional context.
```

## Validation Checklist

Before committing AI documentation updates:

- [ ] Build solution successfully: `dotnet build`
- [ ] All code examples are syntactically correct
- [ ] All internal links work (e.g., to /docs files)
- [ ] Examples use consistent naming conventions
- [ ] New features are mentioned in both AI documentation files
- [ ] CHANGELOG.md is updated with the feature
- [ ] Feature documentation exists in `/docs`
- [ ] Version Information section lists the new feature

## Testing AI Documentation

To validate that AI documentation is effective:

1. **Use with an AI Assistant**
   - Share the documentation with an AI coding assistant
   - Ask it to create a state machine using a new feature
   - Verify it follows the documented patterns correctly

2. **Check Examples**
   - Copy examples into a test project
   - Verify they compile without errors
   - Run examples to ensure they work as described

3. **Review Coverage**
   - Compare feature list in documentation with actual features in code
   - Ensure all public APIs are documented
   - Verify all patterns in `/docs` are reflected in AI docs

## Example: Adding a New Feature

Let's say you're adding a new feature called "Parallel Transitions" that allows multiple state changes at once:

### Step 1: Create Feature Documentation

Create `/docs/Parallel-Transitions.md`:
```markdown
# Parallel Transitions

Parallel transitions allow a state to transition to multiple target states simultaneously...

[Full documentation with examples]
```

Update `/docs/index.md`:
```markdown
- [Parallel transitions](Parallel-Transitions.md)
```

### Step 2: Update `.copilot-instructions.md`

Add to "Key Builder Methods" table:
```markdown
| `.ParallelTransitionTo(states)` | Multiple target states | Inside `.On<>()` |
```

Add to "Common Patterns":
```markdown
### Pattern N: Parallel Transitions

[code example with explanation]
```

Add to "Version Information":
```markdown
- Parallel transitions
```

### Step 3: Update `AI-USAGE-GUIDE.md`

Add to "Key Patterns":
```markdown
### Parallel Transitions
[brief example]
```

### Step 4: Update README.md

Add section with example and link to docs.

### Step 5: Update CHANGELOG.md

Add entry under unreleased or current version.

### Step 6: Build and Validate

```bash
dotnet build
dotnet pack
# Test with AI assistant
```

## Keeping Documentation Discoverable

Ensure AI assistants can find the documentation:

1. **Reference in README.md** - Main entry point should link to AI docs
2. **Include in Packages** - AI-USAGE-GUIDE.md in NuGet package via Directory.Build.props
3. **Repository Root** - Keep `.copilot-instructions.md` at root for IDE integration
4. **Link from docs/index.md** - Make discoverable from main docs

## Questions?

For questions about AI documentation maintenance, open an issue or discussion on GitHub.
