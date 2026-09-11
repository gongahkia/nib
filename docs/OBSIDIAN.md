# Obsidian

Nib includes one paired Obsidian app theme. Obsidian applies the light or dark
variables in `theme.css` according to the app's selected base colour scheme.

## Install from this repository

1. In the target vault, create `.obsidian/themes/Nib/` if it does not exist.
2. Copy `obsidian/Nib/manifest.json` and `obsidian/Nib/theme.css` into that
   directory. `versions.json` is release metadata and may be copied with them.
3. Reload Obsidian, then open **Settings → Appearance → Themes** and select
   **Nib**.
4. Select Obsidian's light or dark base colour scheme, or let it follow the
   operating system.

From the repository root on macOS or Linux, the copy can be performed with:

```sh
mkdir -p "/absolute/path/to/vault/.obsidian/themes/Nib"
cp obsidian/Nib/{manifest.json,theme.css,versions.json} \
  "/absolute/path/to/vault/.obsidian/themes/Nib/"
```

The generated CSS uses Obsidian's documented variables and low-specificity
`.theme-light` and `.theme-dark` scopes. It contains no fonts, images, remote
imports, or network resources.

## Distribution status

The checked-in folder is directly installable, but Nib has not been submitted
to Obsidian's community theme gallery. Its manifest currently requires
Obsidian 1.10.6 or newer. Re-run `make generate` after a deliberate palette or
package-version change; do not edit the generated files directly.
