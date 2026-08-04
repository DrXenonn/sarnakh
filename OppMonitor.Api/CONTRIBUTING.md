# Contributing to StarterKit.Api

This is a **personal-use template project** — these are internal guidelines, not an open-source policy.

## Commit Message Format

Follow [Conventional Commits](https://www.conventionalcommits.org/) loosely:

```
<type>: <description>

[optional body]
```

### Types

| Type | Use when… |
|------|-----------|
| `feat` | Adding a new feature / API endpoint |
| `fix` | Fixing a bug |
| `refactor` | Rewriting code without changing behavior |
| `docs` | Updating `AGENTS.md`, `README.md`, or in-code docs |
| `chore` | Build scripts, tooling, `.gitignore`, etc. |
| `deps` | Adding / updating / removing NuGet packages |
| `config` | Changing `appsettings*.json`, launch profiles, or cfg behavior |

### Examples

```
feat: setup SQLite + EF Core with initial migration
fix: correct swagger route
chore: add .gitattributes
```

## Branching Model

Work directly on `main`. If experimenting with a risky change, branch with a short descriptive name like `feat/rate-limiting` or `fix/refresh-token-bug`.

## Before Committing

1. **Build**: `dotnet build`
2. **Run**: `dotnet run` — confirm the app starts without errors
3. **Check files**: make sure no sensitive data (connection strings, secrets, JWT keys) are being committed

## Updating the Template

When a feature is complete and stable, update `AGENTS.md` to reflect new packages, endpoints, or configuration so future sessions know the current state.
