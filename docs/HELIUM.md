# Helium themes

Nib ships separate generated light and dark static themes for Helium:

- `helium/nib-light/manifest.json`
- `helium/nib-dark/manifest.json`

Helium uses Chromium's theme format, which does not provide Firefox-style paired `theme` and `dark_theme` entries in one package. Install the variant you want to use. Both packages are Manifest V3, contain only RGB theme values, request no permissions, run no code, and load no remote resources.

## Load an unpacked theme

1. Open `helium://extensions`.
2. Enable **Developer mode**.
3. Choose **Load unpacked**.
4. Select either `helium/nib-light` or `helium/nib-dark` from this repository.

Loading the other directory switches variants. To remove Nib, open `helium://settings/appearance` and reset or replace the active theme.

## Package locally

Helium can package either directory into a CRX using its Chromium command-line interface. Use a temporary copy if you do not want the generated CRX and private key beside the source directory.

## Current Helium limitation

The manifests theme the frame, tabs, toolbar, bookmark text, controls, and supported new-tab roles. As of Helium 0.16.6.1, some Chromium-derived internal pages such as Settings, Extensions, and Downloads can still retain their default colors. Helium tracks this as an approved [upstream theming request](https://github.com/imputnet/helium/issues/106); an extension theme cannot safely override privileged internal pages.

The implementation follows Chromium's [theme creation guide](https://chromium.googlesource.com/chromium/src.git/+/main/docs/theme_creation_guide.md) and current [theme property definitions](https://chromium.googlesource.com/chromium/src/+/main/chrome/browser/themes/theme_properties.h).
