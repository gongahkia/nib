# Neovim guide

Quireveil is a dependency-free Lua colorscheme for Neovim 0.10 and newer. The release suite currently exercises it under Neovim 0.11.6 with `nvim --headless -u NONE`.

## Entry points

The default colorscheme follows the existing `background` option:

```lua
vim.o.background = "light"
vim.cmd.colorscheme("quireveil")
```

The explicit siblings set `background` themselves:

```vim
:colorscheme quireveil-light
:colorscheme quireveil-dark
```

Every load clears old highlights, resets syntax when active, reapplies the complete group table with `vim.api.nvim_set_hl()`, and sets `vim.g.colors_name`. This makes switching and reloading deterministic rather than additive.

## Configuration

Call `setup()` only when changing defaults:

```lua
require("quireveil").setup({
  style = "auto",
  transparent = false,
  italics = true,
  terminal_colors = true,
  integrations = {
    telescope = false,
    -- cmp = false,
    -- gitsigns = false,
    -- which_key = false,
    -- trouble = false,
    -- noice = false,
    -- snacks = false,
  },
})
```

`setup()` validates its small option surface and immediately loads the selected style. Setting `terminal_colors = false` removes `vim.g.terminal_color_0` through `15`; it does not alter an already-running terminal job's own state. Transparency removes the normal and non-current background fill, so the opaque contrast report no longer describes the composite result.

## Syntax and semantic policy

Legacy syntax groups provide a fallback. Current Tree-sitter capture groups form the main language-independent baseline, including variable/member, function/method, type, keyword subfamilies, strings, comments, markup, tags, and diff captures.

LSP semantic groups map standard token types to that baseline. Functions and methods retain the strongest blue-ink accent, types and classes use moss, strings use teal, and punctuation/operators stay graphite. Semantic modifiers primarily add stable non-hue information:

- `deprecated` uses strikethrough.
- `readonly` uses weight and selected type/modifier combinations link to constants.
- `documentation` follows the italics preference.
- `defaultLibrary` uses underline or a builtin link.

Neovim applies Tree-sitter captures at its normal capture priority and layers LSP type, modifier, and type/modifier groups above that baseline. Servers can publish inconsistent legends, so Quireveil deliberately avoids broad modifier-specific recoloring.

## UI and diagnostics

The runtime covers normal/non-current and floating windows, separators, popups, completion, line numbers, cursor lines, status/tab lines, search, selection, matching brackets, folds, spell checking, diagnostics, diffs, Git signs, Markdown, help, embedded terminals, quickfix, and location-list-related groups.

Diagnostics use text, virtual text, floating text, signs, and severity-specific undercurls. Recommended sign text is `E`, `W`, `I`, and `H`; sign glyphs are configured by the user's diagnostic setup because a colorscheme should not override diagnostic behavior.

## Integrations

Definitions for Telescope, nvim-cmp, Gitsigns, WhichKey, Trouble, Noice, and Snacks are enabled by default. They are only highlight declarations and do not `require()` plugin modules. A lualine theme is available as:

```lua
require("lualine").setup({ options = { theme = "quireveil" } })
```

Plugin group names were checked against upstream source on the access date in the [research ledger](../THIRD_PARTY_REFERENCES.md). Plugins may add groups later without breaking the base theme.

## Manual installation

On Linux, one package-style symlink is sufficient:

```sh
mkdir -p ~/.local/share/nvim/site/pack/themes/start
ln -s /absolute/path/to/quireveil ~/.local/share/nvim/site/pack/themes/start/quireveil
```

Use the corresponding `stdpath('data')/site/pack/themes/start` location on other systems. Remove only that symlink or copied directory to uninstall. Quireveil never writes to Neovim configuration or data directories itself.
