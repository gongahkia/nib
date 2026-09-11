# Firefox theme

Nib ships as one generated Firefox static theme in `firefox/manifest.json`. Its `theme` and `dark_theme` entries contain the approved light and dark palettes, so Firefox follows its active color scheme without an extension script.

The package requests no permissions, runs no code, loads no remote resources, and declares that it collects no data. Firefox 140 or newer is required.

## Test locally

1. Open `about:debugging#/runtime/this-firefox`.
2. Choose **Load Temporary Add-on**.
3. Select `firefox/manifest.json` from this repository.

A temporary add-on remains installed only until Firefox restarts. The browser chrome, built-in sidebars, popups, and new-tab surfaces use Nib roles. The theme also declares the matching preferred content color scheme, but it does not recolor arbitrary websites.

## Package and install permanently

Firefox themes must be signed before permanent installation. Create an archive with `manifest.json` at its root, then submit it to [addons.mozilla.org](https://addons.mozilla.org/developers/) for listed or self-distributed signing:

```sh
cd firefox
zip -9 ../nib-firefox-0.1.0.zip manifest.json
```

Increase the canonical version before submitting an update. After signing, install the resulting XPI through Firefox or the Add-ons Manager.

## Disable or remove

Open `about:addons`, select **Themes**, then disable or remove **Nib**. Firefox restores the previously active theme.

The generated manifest follows Mozilla's [static theme](https://extensionworkshop.com/documentation/themes/static-themes/), [`theme`](https://developer.mozilla.org/en-US/docs/Mozilla/Add-ons/WebExtensions/manifest.json/theme), [`dark_theme`](https://developer.mozilla.org/en-US/docs/Mozilla/Add-ons/WebExtensions/manifest.json/dark_theme), and [browser-specific settings](https://developer.mozilla.org/en-US/docs/Mozilla/Add-ons/WebExtensions/manifest.json/browser_specific_settings) documentation.
