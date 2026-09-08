# Zed guide

`zed/` is a theme-only Zed development extension. Its generated family contains
exactly `nib-light` and `nib-dark`, declares the current v0.2.0 theme schema,
uses opaque surfaces, and includes editor/UI roles, syntax styles, diagnostics,
version-control states, and terminal ANSI 0–15. It has no Rust component,
dependencies, telemetry, or runtime network access.

## Install a local checkout

Open Zed's Extensions page, choose **Install Dev Extension**, and select the
repository's `zed/` directory—the directory containing `extension.toml`, not
`zed/themes/`. Open the Theme Selector with `ctrl-k ctrl-t` on Linux/Windows or
`cmd-k cmd-t` on macOS, then select `nib-light` or `nib-dark`.

For automatic system appearance, add this to your own Zed settings after the
development extension is installed:

```json
{
  "theme": {
    "mode": "system",
    "light": "nib-light",
    "dark": "nib-dark"
  }
}
```

Nib never edits Zed settings. The theme is font-neutral; use any installed
buffer and UI fonts you prefer.

## Remove

Remove the Nib development extension from Zed's Extensions page and delete any
theme-setting lines you added. No repository files need to be copied into
Zed's internal extension directory.

## Compatibility and verification

Zed does not publish a stable application-version matrix for theme schema
v0.2.0, so Nib states compatibility by schema rather than inventing a minimum
editor version. Release verification validates `zed/themes/nib.json` against
the official `https://zed.dev/schema/themes/v0.2.0.json` schema, while the
offline suite checks the manifest, schema declaration, exact names and
appearances, required UI/syntax roles, opaque background, canonical six-digit
colors, and ANSI mapping.

Zed is not installed in the release environment, so loading and interactive
rendering are documented manual checks and are explicitly reported as skipped.
