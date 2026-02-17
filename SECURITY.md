# Security Policy

## Supported Versions

We actively support the following versions of Functional State Machine with security updates:

| Version | Supported          |
| ------- | ------------------ |
| 1.x.x   | :white_check_mark: |
| < 1.0   | :x:                |

## Reporting a Vulnerability

We take the security of Functional State Machine seriously. If you discover a security vulnerability, please follow these steps:

### 1. Do Not Disclose Publicly

Please **do not** create a public GitHub issue for security vulnerabilities. This helps protect users who haven't yet had a chance to update.

### 2. Report Privately

Report security vulnerabilities by emailing the maintainers or using GitHub's private vulnerability reporting feature:

- **GitHub Security Advisories**: Use the "Report a vulnerability" button in the Security tab
- **Email**: [Contact the repository owner through GitHub]

### 3. Include Details

When reporting a vulnerability, please include:

- **Description**: A clear description of the vulnerability
- **Impact**: What could an attacker achieve?
- **Reproduction**: Steps to reproduce the issue
- **Version**: Which version(s) are affected?
- **Mitigation**: Any potential workarounds or mitigations

### Example Report

```
Subject: Security Vulnerability in State Machine Validation

Description: A specially crafted state machine configuration can bypass 
validation checks, potentially leading to runtime errors.

Impact: Applications could crash or enter invalid states during execution.

Reproduction:
1. Create a state machine with...
2. Configure transition with...
3. Call Build() and observe...

Affected Versions: 1.0.0 - 1.1.0

Proposed Fix: Add additional validation in the Build() method to...
```

## Response Timeline

- **Acknowledgment**: We'll acknowledge receipt within 48 hours
- **Assessment**: We'll assess the vulnerability within 5 business days
- **Fix Development**: We'll work on a fix based on severity
- **Release**: Security fixes are released as soon as possible
- **Disclosure**: Public disclosure after fix is available and users have had reasonable time to update (typically 30 days)

## Security Best Practices

When using Functional State Machine:

### 1. Input Validation

Always validate data and triggers from untrusted sources before passing them to the state machine:

```csharp
// Don't do this with untrusted input
var trigger = (MyTrigger)untrustedInput;
machine.Fire(trigger, currentState, currentData);

// Do this instead
if (TryParseTrigger(untrustedInput, out var trigger))
{
    machine.Fire(trigger, currentState, currentData);
}
```

### 2. Command Execution

Be careful when executing commands returned from the state machine. Ensure proper authorization and validation:

```csharp
var (newState, newData, commands) = machine.Fire(trigger, state, data);

foreach (var command in commands)
{
    // Validate command before execution
    if (IsAuthorized(command) && IsValid(command))
    {
        await dispatcher.DispatchAsync(command);
    }
}
```

### 3. State Persistence

When persisting state:

- **Encrypt sensitive data** in state storage
- **Validate state** before loading into state machine
- **Use secure storage** mechanisms
- **Implement access controls** on state storage

### 4. Dependency Updates

- Keep the library updated to the latest version
- Monitor security advisories
- Use tools like Dependabot to track updates

## Known Security Considerations

### Type Safety

The library uses .NET's type system for safety, but be aware:

- **Reflection**: The static analysis uses reflection to discover trigger types
- **Serialization**: Be cautious when serializing/deserializing state machine configurations
- **Dynamic Behavior**: Guard conditions and data modifications can execute arbitrary code

### No Built-in Authorization

The library doesn't include authorization mechanisms. Implement authorization in your command runners:

```csharp
public class SecureCommandRunner : ICommandRunner<MyCommand>
{
    private readonly IAuthorizationService _authService;

    public async Task RunAsync(MyCommand command)
    {
        // Implement authorization
        if (!await _authService.IsAuthorizedAsync(command))
        {
            throw new UnauthorizedAccessException();
        }

        // Execute command
        await ExecuteCommandAsync(command);
    }
}
```

## Security Updates

Security updates are published:

1. As NuGet packages with patch version bumps
2. In GitHub Security Advisories
3. In the CHANGELOG.md under the relevant version
4. In GitHub Releases with security labels

## Acknowledgments

We appreciate security researchers who responsibly disclose vulnerabilities. Contributors who report valid security issues will be acknowledged in:

- The security advisory (if they consent)
- The CHANGELOG for the fixed version
- A SECURITY.md acknowledgments section (below)

### Security Contributors

<!-- List of security contributors will appear here -->

Thank you for helping keep Functional State Machine secure!
