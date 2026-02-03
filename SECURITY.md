# Security Policy

## Reporting a Vulnerability
If you discover a security vulnerability, please do not open a public issue.
Instead, contact the project maintainer privately with a clear description
and steps to reproduce the issue.

## Security Practices
- Sensitive information such as passwords and connection strings is not stored in the repository
- Development secrets are managed using .NET User Secrets or environment variables
- Authentication is handled using ASP.NET Core Identity
- Anti-forgery (CSRF) protection is enabled for state-changing requests
- Uploaded files are validated and restricted by size, type, and signature
- Uploaded content is excluded from version control

## Scope
This project is intended as a portfolio and learning project.
No production security guarantees are provided.
