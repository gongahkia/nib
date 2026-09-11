# Nib

Nib is a coordinated light/dark theme family for [Ghostty](https://ghostty.org/), [Neovim](https://neovim.io/), Vim, Helix, GNU Emacs, VS Code, Cursor, Zed, Sublime Text, Firefox, and Helium. Its light mode places deep blue-black fountain-pen ink on cool-neutral paper; its dark sibling uses soft ivory ink on near-black chalkboard charcoal. Moss leads the non-blue syntax colors, followed by peacock teal, restrained burgundy, rust, violet, amber, sepia, and graphite.

The core themes are opaque, dependency-free, and complete without shaders. No normal foreground or background is pure black or pure white.

## Quick start

### Neovim

Put the repository on Neovim's runtime path, then use the automatic entry point:

```lua
vim.o.background = "dark" -- or "light"
vim.cmd.colorscheme("nib")
```

No `setup()` call is required. Explicit entry points are also available:

```vim
:colorscheme nib-light
:colorscheme nib-dark
```

A local-checkout Lazy.nvim specification is:

```lua
{
  dir = vim.fn.expand("~/src/nib"),
  lazy = false,
  priority = 1000,
  config = function()
    require("nib").setup({
      style = "auto",
      transparent = false,
      italics = true,
      terminal_colors = true,
      integrations = {
        -- telescope = false,
      },
    })
  end,
}
```

For a manual package install, copy or symlink the checkout into a `start` directory under Neovim's data path, for example `~/.local/share/nvim/site/pack/themes/start/nib` on Linux. The [Neovim guide](docs/NEOVIM.md) documents loading, options, integrations, semantics, and troubleshooting.

### Ghostty

Preview the safe installer without changing any configuration:

```sh
python3 scripts/install_ghostty.py
```

Install only the two theme files with an explicit action:

```sh
python3 scripts/install_ghostty.py --apply
```

Then add this verified Ghostty 1.3 paired-theme syntax to your own configuration:

```ini
theme = light:nib-light,dark:nib-dark
window-theme = system
background-opacity = 1
```

The installer does not edit `~/.config/ghostty/config`, load a shader, or overwrite an existing theme unless `--force` is explicit. Linux, macOS, symlink, backup, uninstall, tmux, and SSH instructions are in the [Ghostty guide](docs/GHOSTTY.md).

### Emacs

Copy the two generated custom-theme files into a directory on
`custom-theme-load-path`, then load one exact variant:

```elisp
(add-to-list 'custom-theme-load-path
             (expand-file-name "themes" user-emacs-directory))
(load-theme 'nib-dark t) ; or nib-light
```

The [Emacs guide](docs/EMACS.md) covers local installation, switching,
terminal colors, removal, supported faces, and batch verification.

### VS Code and Cursor

The theme-only extension under `vscode/` works in both editors. It contributes
exactly `nib-light` and `nib-dark`, with TextMate and semantic-token coverage.
Package it locally without publishing:

```sh
cd vscode
npx --yes @vscode/vsce@3.6.2 package --no-dependencies \
  --out nib-color-theme-0.1.0.vsix
```

Install the VSIX in either editor and choose the variant through
**Preferences: Color Theme**. See the [VS Code and Cursor guide](docs/VSCODE.md)
for development-host, install, and uninstall details.

### Zed

From Zed's Extensions page, choose **Install Dev Extension** and select the
repository's `zed/` directory. The [Zed guide](docs/ZED.md) includes exact
selection, paired system-mode settings, removal, and compatibility notes.

### Vim

Copy `vim/colors/*.vim` to `~/.vim/colors/`, enable `termguicolors`, set
`background`, and load `colorscheme nib`. Explicit light and dark entry points
are also included. See the [Vim guide](docs/VIM.md).

### Helix

Copy `helix/nib-*.toml` to `~/.config/helix/themes/`, then set
`theme = "nib-dark"` or switch with `:theme nib-light`. See the
[Helix guide](docs/HELIX.md).

### Sublime Text

Copy both files under `sublime/` into Sublime's `Packages/User` directory and
select **Nib Light** or **Nib Dark** as the color scheme. See the
[Sublime Text guide](docs/SUBLIME.md).

### Firefox

Load [`firefox/manifest.json`](firefox/manifest.json) as a temporary add-on from
`about:debugging` for local testing. A single permission-free static theme
contains the paired light and dark definitions and follows Firefox's color
scheme. Permanent installation requires Mozilla signing. See the
[Firefox guide](docs/FIREFOX.md).

### Helium

Open `helium://extensions`, enable developer mode, and load either
`helium/nib-light/` or `helium/nib-dark/` unpacked. Chromium themes cannot pair
both modes in one package, so the variants remain separate. See the
[Helium guide](docs/HELIUM.md) for installation, packaging, removal, and the
current limitation on Chromium internal pages.

## Setup API

The API intentionally stays small:

```lua
require("nib").setup({
  style = "auto", -- "auto", "light", or "dark"
  transparent = false,
  italics = true,
  terminal_colors = true,
  integrations = {
    -- set a supported integration to false to omit its groups
  },
})
```

`style = "auto"` follows `vim.o.background`. The runtime clears prior highlights before every load, defines terminal colors 0–15 unless disabled, and sets `vim.g.colors_name` to the entry point in use. Transparency affects editor backgrounds; measured contrast applies only to the default opaque surfaces.

Nib defines harmless groups for nvim-treesitter, Telescope, nvim-cmp, Gitsigns, WhichKey, Trouble, Noice, Snacks, and lualine without importing those plugins. Tree-sitter is the dependable syntax baseline. LSP semantic types refine it conservatively, while modifiers emphasize state through weight, underline, italics, or strikethrough rather than server-dependent recoloring.

## Optional static shaders

Two opt-in Ghostty shader chains add procedural paper grain followed by restrained ink feathering. The daily preset is nearly imperceptible; the showcase preset is deliberately stronger. Neither animates, uses external textures, or participates in the default install.

```ini
custom-shader = /absolute/path/to/nib/ghostty/shaders/paper-grain-daily.glsl
custom-shader = /absolute/path/to/nib/ghostty/shaders/ink-feather-daily.glsl
custom-shader-animation = false
```

Ghostty post-processing affects the whole rendered surface, including Neovim chrome. If a shader causes a black surface, remove every `custom-shader = ...` line from another terminal or editor and restart Ghostty. Read the complete [shader guide](docs/SHADERS.md) before opting in.

## Design and accessibility

One canonical [palette](palette/palette.json), validated by its [schema](palette/schema.json) and frozen by an explicit [palette lock](palette/lock.json), generates the Neovim, Vim, Helix, Emacs, VS Code/Cursor, Zed, and Sublime Text themes; the Ghostty, Firefox, and Helium themes; and the machine-readable palette and audit reports. Physical inks and external palettes informed relationships only; none supplied an “exact” Nib screen color.

Principal text targets 7:1 contrast where aesthetically reasonable. Meaningful text requires 4.5:1, and relevant boundaries/non-text indicators require 3:1. Comments pass the ordinary-text target in both modes. Protanopia, deuteranopia, and tritanopia simulations are regression-tested, while critical states also use letters, signs, undercurls, weight, strikethrough, or distinct surface tints. These scoped checks are not blanket accessibility certification.

Read the [palette rationale](docs/PALETTE.md), [blind-audit synthesis](docs/BLIND_AUDIT.md), [accessibility report](docs/ACCESSIBILITY.md), and generated [contrast](docs/generated/CONTRAST.md), [ANSI](docs/generated/ANSI.md), and [color-vision](docs/generated/COLOR_VISION.md) tables.

## Supported versions

- Neovim 0.10 or newer. The release suite passes under Neovim 0.10.4 and 0.11.6.
- Ghostty 1.3.0 or newer for automatic paired light/dark selection. Theme validation passes under Ghostty 1.3.1.
- GNU Emacs 27.1 or newer. Batch validation runs when Emacs is available.
- VS Code `^1.85.0`, and Cursor versions compatible with that VS Code theme-extension API.
- Zed versions supporting the current theme schema v0.2.0; Zed does not publish a stable app-version mapping for that schema.
- Vim 9.1 was used for headless loading validation. Exact colors require `termguicolors`.
- Helix versions supporting the documented TOML theme format and current scope names.
- Sublime Text 3.1 or newer for the `.sublime-color-scheme` format.
- Firefox 140 or newer. The paired static theme declares no data collection and requires no permissions or scripts.
- Helium 0.16.6.1 was used for local packaging validation. The two variants use the standard Chromium Manifest V3 theme format.
- A truecolor terminal is recommended for Neovim. The 16-color Ghostty palette remains intentionally conventional for remote and degraded sessions.

The runtime Lua has no dependencies and performs no network requests. Development checks use Python 3.11 or newer; Node.js, Ghostty, and `glslc` add checks when present as documented in the [development guide](docs/DEVELOPMENT.md).

## Verification

Run the complete local suite:

```sh
make verify
```

It checks the palette lock, schema, required roles, color format, generation drift, contrast, semantic and ANSI distinguishability, color-vision regressions, Neovim, Vim, and Emacs loading behavior, VS Code/Cursor, Zed, Helix, Sublime Text, Firefox, and Helium package structure, Ghostty structure and runtime validation when installed, shader structure and compilation when `glslc` is installed, fixtures, documentation links, syntax, and `git diff --check`. Optional application runtimes are reported as skipped when unavailable.

## Troubleshooting

- If `:colorscheme nib` is not found, confirm the repository root—not its `lua/` directory—is on `runtimepath`.
- If automatic Neovim mode looks wrong, set `vim.o.background` before invoking the colorscheme or use an explicit entry point.
- If Ghostty cannot find a theme, confirm the files are named exactly `nib-light` and `nib-dark` under `$XDG_CONFIG_HOME/ghostty/themes` or `~/.config/ghostty/themes`.
- If remote colors differ, verify truecolor and terminfo on the remote host; fixed RGB output bypasses the ANSI palette.
- If generated files differ, edit only `palette/palette.json`, then run `make generate` and review every generated change.

## Provenance and licence

Research sources, licences, AI disclosures, reuse status, and non-affiliation are recorded in [THIRD_PARTY_REFERENCES.md](THIRD_PARTY_REFERENCES.md). Nib does not bundle palette assets or fonts. Original code and documentation are available under the [MIT License](LICENSE).
