# Security Policy

## Supported Versions

Flagstone UI is currently in early development (pre-1.0). Security updates will be provided for the following versions:

| Version | Supported          |
| ------- | ------------------ |
| 0.x.x   | :white_check_mark: |

## Reporting a Vulnerability

If you discover a security vulnerability in Flagstone UI, please report it privately to help us address it quickly and responsibly.

### How to Report

**Do not create a public issue for security vulnerabilities.**

Instead, please:

1. **Email**: Send details to [matt@matt-goldman.net](mailto:matt@matt-goldman.net)
2. **Subject**: Include "Flagstone UI Security" in the subject line
3. **Details**: Provide as much information as possible:
   - Description of the vulnerability
   - Steps to reproduce
   - Potential impact
   - Suggested mitigation (if any)

### What to Expect

- **Acknowledgment**: We will acknowledge receipt within 48 hours
- **Initial Assessment**: We will provide an initial assessment within 5 business days
- **Updates**: We will keep you informed of progress
- **Resolution**: We aim to resolve security issues within 30 days
- **Credit**: With your permission, we will credit you in the release notes

### Responsible Disclosure

We ask that you:

- Give us reasonable time to fix the issue before public disclosure
- Do not exploit the vulnerability for malicious purposes
- Do not access data that doesn't belong to you
- Do not perform actions that could harm the service or other users

### Security Best Practices

When using Flagstone UI:

- Always use the latest stable version
- Follow secure coding practices in your applications
- Report suspicious behavior or potential vulnerabilities
- Keep your .NET runtime and dependencies up to date

## Security Considerations

### Theme Security

- Custom themes should not execute arbitrary code
- Resource dictionaries should only contain styling resources
- Avoid using converters or behaviors that access external resources

### Control Implementation

- Controls follow MAUI security best practices
- No sensitive data is logged or exposed
- Platform handlers only modify visual appearance, not behavior

## Contact

For security-related questions or concerns:

- **Security Issues**: [matt@matt-goldman.net](mailto:matt@matt-goldman.net)
- **General Questions**: Create a GitHub issue or discussion

Thank you for helping keep Flagstone UI and its users safe!