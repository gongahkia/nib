# VS Code and Cursor guide

`vscode/` is a complete VS Code color-theme extension containing exactly
`nib-light` and `nib-dark`. Cursor officially inherits VS Code theme support,
so the same extension is the single generated artifact for both editors; no
second, drift-prone Cursor palette is maintained.

The extension targets the VS Code `^1.85.0` engine range. It defines workbench
and editor chrome, selection and search, diagnostics, diffs, Git decorations,
completion and hover surfaces, terminal ANSI 0–15, a TextMate syntax baseline,
and conservative semantic-token refinements. It has no executable extension
code, dependencies, telemetry, or runtime network access.

## Try a local checkout

With VS Code installed, open an Extension Development Host from the repository
root:

```sh
code --extensionDevelopmentPath="$PWD/vscode"
```

Run **Preferences: Color Theme** and select `nib-light` or `nib-dark`. Cursor
users can install the packaged VSIX below and use the same command.

## Package and install locally

The packaging command is development-only and downloads the pinned `vsce`
tool when it is not already cached. It does not publish the extension:

```sh
cd vscode
npx --yes @vscode/vsce@3.6.2 package --no-dependencies \
  --out nib-color-theme-0.1.0.vsix
```

Install into VS Code from the repository root:

```sh
code --install-extension vscode/nib-color-theme-0.1.0.vsix
```

For Cursor, run **Extensions: Install from VSIX...** and choose the same file.
Then select either exact theme name with **Preferences: Color Theme**. Nib does
not change settings automatically and does not require the canonical preview
font.

## Remove

Use the Extensions view in either editor and uninstall **Nib**, or remove the
local VS Code extension from the command line:

```sh
code --uninstall-extension local-nib.nib-color-theme
```

Delete any VSIX you created separately. Packaging output is intentionally not
committed.

## Verification limits

`make verify` validates the extension manifest, exact variant names, engine
range, required UI and syntax coverage, semantic selectors, six-digit color
format, canonical-palette ownership, button contrast, and ANSI mapping. A VSIX
was also packaged during release verification. VS Code and Cursor are not
installed in the release environment, so interactive rendering remains a
manual check; this is reported as skipped rather than presented as runtime
validation.
