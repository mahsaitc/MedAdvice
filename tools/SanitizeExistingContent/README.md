# SanitizeExistingContent

One-time cleanup for rich-text columns that were written **before** inbound HTML
sanitization was added to the admin insert actions.

Rows created from that point on are sanitized on write, and the three detail views
also sanitize on render, so this tool is not required for the site to be safe. It
exists to clean the stored data itself, so the raw column values are trustworthy for
any future consumer (exports, a rewritten view, an API).

## What it touches

| Table     | Column       |
|-----------|--------------|
| `Advices` | `AdviceText` |
| `Blogs`   | `BlogText`   |
| `Doctors` | `DrDetails`  |

Sanitization is performed by `MedAdvice.Services.HtmlContentSanitizer`, linked
directly from the web project rather than copied, so the result is byte-identical
to what the running site produces.

## Running it

Dry run first — reports row counts and how many rows would change, writes nothing:

```
dotnet run --project tools/SanitizeExistingContent -- --connection "<connection string>"
```

Commit the changes:

```
dotnet run --project tools/SanitizeExistingContent -- --connection "<connection string>" --apply
```

The connection string may also come from the `ConnectionStrings__MedAdviceDbConnection`
environment variable instead of `--connection`.

Exit codes: `0` success, `1` database error, `2` no connection string supplied.

## Before running against real data

1. Take a database backup. The tool overwrites columns in place with no undo.
2. Run without `--apply` and check the reported counts look sane.
3. Only then re-run with `--apply`.

This project is intentionally **not** part of `MedAdvice.sln`, so it never affects
the web application's build.
