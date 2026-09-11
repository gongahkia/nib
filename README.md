# Nib

Nib is a coordinated light/dark theme family for [Ghostty](https://ghostty.org/), [Neovim](https://neovim.io/), [GNU Emacs](https://www.gnu.org/software/emacs/), [VS Code](https://code.visualstudio.com/), [Cursor](https://www.cursor.com/), and [Zed](https://zed.dev/). Its light mode places deep blue-black fountain-pen ink on warm ivory; its dark sibling uses cool ivory ink on near-black navy. Moss leads the non-blue syntax colors, followed by peacock teal, restrained burgundy, rust, violet, amber, sepia, and graphite.

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

## Comparison preview

The static site consumes generated palette CSS and JSON-shaped JavaScript. It has no framework, telemetry, account, CDN, or runtime network dependency:

```sh
make preview
# open http://127.0.0.1:8765/preview/
```

It runs a blind preference audit between Nib and twelve reference themes using the same TypeScript sample and normalized token roles. Opponent order and A/B placement are shuffled, identities stay hidden, and each light or dark choice opens a mode-specific comment field. Only one comment across the two modes is required, keeping the remaining matchups rapid. Progress survives reloads in browser-local storage; completed answers can be copied or exported as JSON with the hidden identity key for later synthesis. Reference values and upstream links live in [`palette/comparisons.json`](palette/comparisons.json).

## Design and accessibility

One canonical [palette](palette/palette.json), validated by its [schema](palette/schema.json), generates the Neovim palettes and entry points, Ghostty themes, Emacs custom themes, VS Code/Cursor extension themes, Zed extension theme, Nib preview data, machine export, contrast table, ANSI table, and color-vision report. The separate comparison data does not feed any Nib theme implementation. Physical inks and external palettes informed relationships only; none supplied an “exact” Nib screen color.

Principal text targets 7:1 contrast where aesthetically reasonable. Meaningful text requires 4.5:1, and relevant boundaries/non-text indicators require 3:1. Comments pass the ordinary-text target in both modes. Protanopia, deuteranopia, and tritanopia simulations are regression-tested, while critical states also use letters, signs, undercurls, weight, strikethrough, or distinct surface tints. These scoped checks are not blanket accessibility certification.

Read the [palette rationale](docs/PALETTE.md), [accessibility report](docs/ACCESSIBILITY.md), and generated [contrast](docs/generated/CONTRAST.md), [ANSI](docs/generated/ANSI.md), and [color-vision](docs/generated/COLOR_VISION.md) tables.

## Supported versions

- Neovim 0.10 or newer. The release suite passes under Neovim 0.10.4 and 0.11.6.
- Ghostty 1.3.0 or newer for automatic paired light/dark selection. Theme validation passes under Ghostty 1.3.1.
- GNU Emacs 27.1 or newer. Batch validation runs when Emacs is available.
- VS Code `^1.85.0`, and Cursor versions compatible with that VS Code theme-extension API.
- Zed versions supporting the current theme schema v0.2.0; Zed does not publish a stable app-version mapping for that schema.
- A truecolor terminal is recommended for Neovim. The 16-color Ghostty palette remains intentionally conventional for remote and degraded sessions.

The runtime Lua has no dependencies and performs no network requests. Development checks use Python 3.11 or newer; Node.js, Ghostty, and `glslc` add checks when present as documented in the [development guide](docs/DEVELOPMENT.md).

## Verification

Run the complete local suite:

```sh
make verify
```

It checks the schema, required roles, color format, generation drift, contrast, semantic and ANSI distinguishability, color-vision regressions, Neovim and Emacs loading behavior, VS Code/Cursor and Zed extension structure, Ghostty structure and runtime validation when installed, shader structure and compilation when `glslc` is installed, comparison preview data, fixtures, documentation links, syntax, and `git diff --check`. Optional editor runtimes are reported as skipped when unavailable.

## Troubleshooting

- If `:colorscheme nib` is not found, confirm the repository root—not its `lua/` directory—is on `runtimepath`.
- If automatic Neovim mode looks wrong, set `vim.o.background` before invoking the colorscheme or use an explicit entry point.
- If Ghostty cannot find a theme, confirm the files are named exactly `nib-light` and `nib-dark` under `$XDG_CONFIG_HOME/ghostty/themes` or `~/.config/ghostty/themes`.
- If remote colors differ, verify truecolor and terminfo on the remote host; fixed RGB output bypasses the ANSI palette.
- If generated files differ, edit only `palette/palette.json`, then run `make generate` and review every generated change.

## Provenance and licence

Research sources, licences, AI disclosures, reuse status, and non-affiliation are recorded in [THIRD_PARTY_REFERENCES.md](THIRD_PARTY_REFERENCES.md). Nib does not bundle palette assets or fonts. Original code and documentation are available under the [MIT License](LICENSE).
