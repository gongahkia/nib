# Third-party references

Original research entries were accessed on **2026-09-09**; Firefox, Chromium,
Helium, Vim, Helix, and Sublime Text references were accessed on **2026-09-11**. Nib contains original
code, prose, generated artifacts, and original sRGB values. No external image,
font, or runtime resource is bundled or hotlinked.

## Technical primary sources

### Ghostty configuration reference

- **Creator/publisher:** Ghostty project
- **URL:** <https://ghostty.org/docs/config/reference>
- **Stated licence:** Ghostty source repository is MIT; the web page does not
  state a separate content licence.
- **AI disclosure:** none stated
- **Visual/technical lesson:** authoritative theme lookup, paired light/dark
  syntax, accepted color keys, repeated shader order, animation control,
  uniforms, `mainImage`, and shader failure behavior.
- **Exact reusable values:** API/configuration facts only; no theme colors.
- **Required attribution:** none identified; linked for verification.

### Ghostty color theme guide

- **Creator/publisher:** Ghostty project
- **URL:** <https://ghostty.org/docs/features/theme>
- **Stated licence:** Ghostty source repository is MIT; the web page does not
  state a separate content licence.
- **AI disclosure:** none stated
- **Visual/technical lesson:** custom-theme discovery and automatic paired
  mode behavior.
- **Exact reusable values:** configuration facts only.
- **Required attribution:** none identified; linked for verification.

### Neovim API, Tree-sitter, LSP, and diagnostic documentation

- **Creator/publisher:** Neovim project
- **URLs:** <https://neovim.io/doc/user/api/>,
  <https://neovim.io/doc/user/treesitter/>,
  <https://neovim.io/doc/user/lsp/>, and
  <https://neovim.io/doc/user/diagnostic/>
- **Stated licence:** Apache-2.0 in the Neovim source distribution.
- **AI disclosure:** none stated
- **Visual/technical lesson:** current highlight API semantics, standard
  capture names, semantic token naming/priority, and diagnostic group families.
- **Exact reusable values:** API and group names only; no theme colors.
- **Required attribution:** Apache-2.0 notices apply to copied source; none was
  copied. Links are retained as provenance.

### GNU Emacs custom-theme documentation

- **Creator/publisher:** Free Software Foundation / GNU Project
- **URLs:** <https://www.gnu.org/software/emacs/manual/html_node/elisp/Custom-Themes.html>
  and <https://www.gnu.org/s/emacs/manual/html_node/emacs/Custom-Themes.html>
- **Stated licence:** GNU Free Documentation License 1.3 or later for the GNU
  Emacs manuals.
- **AI disclosure:** none stated
- **Visual/technical lesson:** verified `deftheme`, `custom-theme-set-faces`,
  `custom-theme-set-variables`, `provide-theme`, `load-theme`, theme-file
  naming, and `custom-theme-load-path` behavior.
- **Exact reusable values:** API and file-format facts only; no theme colors.
- **Required attribution:** none for independently authored implementation;
  links are retained as provenance.

### Visual Studio Code theming and extension documentation

- **Creator/publisher:** Microsoft
- **URLs:** <https://code.visualstudio.com/api/extension-guides/color-theme>,
  <https://code.visualstudio.com/api/extension-capabilities/theming>,
  <https://code.visualstudio.com/api/references/theme-color>,
  <https://code.visualstudio.com/api/references/contribution-points#contributes.themes>,
  and <https://code.visualstudio.com/api/language-extensions/semantic-highlight-guide>
- **Stated licence:** Code - OSS reference source is MIT; the linked web pages
  do not state a separate page licence.
- **AI disclosure:** none stated
- **Visual/technical lesson:** verified theme contributions, `vs`/`vs-dark`
  UI kinds, workbench colors, TextMate `tokenColors`, semantic highlighting,
  token-selector syntax, modifier styling, and six/eight-digit RGB support.
- **Exact reusable values:** configuration identifiers and format facts only;
  no theme colors or implementation copied.
- **Required attribution:** none identified; links are retained as provenance.

### Cursor theme documentation

- **Creator/publisher:** Anysphere / Cursor
- **URL:** <https://docs.cursor.com/en/configuration/themes>
- **Stated licence:** no separate documentation licence stated on the page.
- **AI disclosure:** none stated
- **Visual/technical lesson:** Cursor inherits VS Code theming and can consume
  a VS Code-compatible color-theme extension, allowing one generated artifact
  instead of a divergent duplicate.
- **Exact reusable values:** compatibility fact only; no theme colors.
- **Required attribution:** none identified; linked for verification.

### Zed theme and extension documentation

- **Creator/publisher:** Zed Industries
- **URLs:** <https://zed.dev/docs/extensions/themes>,
  <https://zed.dev/docs/extensions/developing-extensions>,
  <https://zed.dev/docs/extensions/publishing/prerequisites>,
  <https://zed.dev/docs/appearance>, and
  <https://zed.dev/schema/themes/v0.2.0.json>
- **Stated licence:** Zed source is primarily GPL-3.0-or-later with marked
  Apache-2.0 components; the linked web pages do not state a separate page
  licence.
- **AI disclosure:** none stated
- **Visual/technical lesson:** verified theme-family structure, v0.2.0 JSON
  schema, `extension.toml`, theme-only extension layout, appearance names,
  UI/syntax/terminal fields, development installation, and paired system-mode
  settings.
- **Exact reusable values:** schema identifiers and format facts only; no
  upstream theme colors or source code copied.
- **Required attribution:** none for independently authored theme data; links
  are retained as provenance.

### Firefox static-theme documentation

- **Creator/publisher:** Mozilla
- **URLs:** <https://developer.mozilla.org/en-US/docs/Mozilla/Add-ons/WebExtensions/manifest.json/theme>,
  <https://developer.mozilla.org/en-US/docs/Mozilla/Add-ons/WebExtensions/manifest.json/dark_theme>,
  <https://developer.mozilla.org/en-US/docs/Mozilla/Add-ons/WebExtensions/manifest.json/browser_specific_settings>,
  and <https://extensionworkshop.com/documentation/themes/static-themes/>
- **Stated licence:** documentation terms vary by Mozilla property; no prose or
  example implementation was copied.
- **AI disclosure:** none stated.
- **Visual/technical lesson:** verified paired static-theme keys, supported
  browser UI color roles, Manifest V3 add-on IDs, signing, and the explicit
  no-data declaration required for new AMO submissions.
- **Exact reusable values:** manifest and color-role identifiers only; no theme
  colors.
- **Required attribution:** none identified for the independently authored
  manifest; links are retained as provenance.

### Chromium and Helium theming

- **Creator/publisher:** Chromium project and Imput
- **URLs:** <https://chromium.googlesource.com/chromium/src.git/+/main/docs/theme_creation_guide.md>,
  <https://chromium.googlesource.com/chromium/src/+/main/chrome/browser/themes/theme_properties.h>,
  <https://github.com/imputnet/helium>, and
  <https://github.com/imputnet/helium/issues/106>
- **Stated licence:** Chromium source is BSD-3-Clause; Helium's original work is
  GPL-3.0 and imported Chromium work retains its original licence.
- **AI disclosure:** none stated.
- **Visual/technical lesson:** verified RGB-array static theme manifests,
  currently overwritable Chromium UI roles, Helium compatibility, and Helium's
  known incomplete theming of some privileged internal pages.
- **Exact reusable values:** manifest and color-role identifiers only; no theme
  colors.
- **Required attribution:** source links are retained as technical provenance;
  no implementation was copied.

### Vim, Helix, and Sublime Text theming

- **Creator/publisher:** Vim project, Helix project, and Sublime HQ
- **URLs:** <https://github.com/vim/colorschemes>,
  <https://docs.helix-editor.com/themes.html>,
  <https://www.sublimetext.com/docs/color_schemes.html>, and
  <https://www.sublimetext.com/docs/scope_naming.html>
- **Stated licence:** Vim's maintained colorschemes repository is Vim-licensed;
  Helix source is MPL-2.0; no separate licence is stated on the Sublime Text
  documentation pages.
- **AI disclosure:** none stated.
- **Visual/technical lesson:** verified Vim highlight and terminal-palette
  conventions, Helix TOML palettes and current UI/syntax/diff/diagnostic
  scopes, and Sublime Text color-scheme globals, variables, baseline scopes,
  and diff markers.
- **Exact reusable values:** format and scope identifiers only; no upstream
  theme colors or implementation were copied.
- **Required attribution:** links are retained as technical provenance for the
  independently authored generated themes.

### Plugin highlight definitions

- **Creator/publisher:** the respective upstream projects
- **URLs:** <https://github.com/nvim-telescope/telescope.nvim>,
  <https://github.com/hrsh7th/nvim-cmp>,
  <https://github.com/lewis6991/gitsigns.nvim>,
  <https://github.com/folke/which-key.nvim>,
  <https://github.com/folke/trouble.nvim>,
  <https://github.com/folke/noice.nvim>,
  <https://github.com/folke/snacks.nvim>, and
  <https://github.com/nvim-lualine/lualine.nvim>
- **Stated licence:** see each linked repository (MIT at access time for the
  implementations consulted).
- **AI disclosure:** none stated
- **Visual/technical lesson:** current public highlight-group names and safe
  fallback relationships.
- **Exact reusable values:** group names only; no source code or theme colors.
- **Required attribution:** none for independently authored group definitions;
  links identify the verification sources.

## Blind-audit reference themes

The completed personal blind audit compared Nib with the following themes under
normalized token roles. Its temporary frontend and copied comparison values
were removed after palette approval; this source list remains as methodological
provenance.

| Theme | Canonical source | Stated licence |
| --- | --- | --- |
| Flexoki | <https://stephango.com/flexoki> and <https://github.com/kepano/flexoki> | MIT |
| Solarized | <https://github.com/altercation/solarized> | MIT |
| Everforest | <https://github.com/sainnhe/everforest/blob/master/palette.md> | MIT |
| Rosé Pine | <https://github.com/rose-pine/neovim/blob/main/lua/rose-pine/palette.lua> | MIT |
| Kanagawa | <https://github.com/rebelot/kanagawa.nvim/blob/master/lua/kanagawa/colors.lua> | MIT |
| Gruvbox | <https://github.com/morhetz/gruvbox/blob/master/colors/gruvbox.vim> | MIT |
| PaperColor | <https://github.com/NLKNguyen/papercolor-theme/blob/master/colors/PaperColor.vim> | MIT |
| Mélange | <https://github.com/savq/melange-nvim> | MIT |
| Ayu | <https://github.com/ayu-theme/ayu-colors> | MIT |
| Tokyo Night | <https://github.com/folke/tokyonight.nvim> | Apache-2.0 |
| Catppuccin | <https://github.com/catppuccin/catppuccin> | MIT |
| Modus | <https://github.com/protesilaos/modus-themes/blob/main/modus-themes.el> | GPL-3.0 |

- **Exact reusable values:** none remain in the repository.
- **Changes:** the audit normalized token roles and used identical code solely
  for the completed comparison.
- **Required attribution:** upstream names and source links are retained in this
  ledger.
- **AI disclosure:** none stated by the upstream projects.

## Fountain-ink references

### Pilot Iroshizuku web catalog

- **Creator/publisher:** Pilot Corporation
- **URL:** <https://webcatalog.pilot.co.jp/products/DispDetail.do?itemID=t000100002552&searchTypeParam=freeWordSearch&searchValue=%E8%89%B2%E5%BD%A9%E9%9B%AB&volumeName=00004>
- **Stated licence:** all-rights-reserved commercial product page; no reuse
  licence stated.
- **AI disclosure:** none stated
- **Visual lesson:** Shin-kai/Tsuki-yo blue depth, Asa-gao/Kon-peki clarity,
  Ku-jaku/Shin-ryoku green range, Yama-budo wine, and Murasaki-shikibu violet.
- **Exact reusable values:** inspiration only; no swatch value or image used.
- **Required attribution:** factual source identification only.

### LAMY T 52 ink and ink collection

- **Creator/publisher:** C. Josef Lamy GmbH
- **URLs:** <https://www.lamy.com/en-us/p/lamy-t-52-ink/52899168551246> and
  <https://www.lamyshop.se/en/collections/ink>
- **Stated licence:** all-rights-reserved commercial pages; no reuse licence
  stated.
- **AI disclosure:** none stated
- **Visual lesson:** blue-black and Benitoite as sober anchors; Amazonite,
  Petrol, Peridot, Azurite, Dark Lilac, Topaz, and Sepia as semantic families.
- **Exact reusable values:** inspiration only; no swatch value or image used.
- **Required attribution:** factual source identification only.

### Sailor blue-black and Manyo ink

- **Creator/publisher:** The Sailor Pen Co., Ltd.
- **URLs:** <https://en.sailor.co.jp/product/13-1007/> and
  <https://en.sailor.co.jp/product/13-2009/>
- **Stated licence:** all-rights-reserved commercial pages; no reuse licence
  stated.
- **AI disclosure:** none stated
- **Visual lesson:** a cool blue-black foundation and restrained density/
  dual-shading relationships.
- **Exact reusable values:** inspiration only; no swatch value or image used.
- **Required attribution:** factual source identification only.

### Diamine 30 ml fountain-pen ink collection

- **Creator/publisher:** Diamine Inks
- **URL:** <https://www.diamineinks.co.uk/collections/diamine-30ml-fountain-pen-ink>
- **Stated licence:** all-rights-reserved commercial page; no reuse licence
  stated.
- **AI disclosure:** none stated
- **Visual lesson:** Oxford Blue, Oxblood, Ancient Copper, Lady Grey, and Honey
  Burst support a compact blue/wine/rust/graphite/amber vocabulary.
- **Exact reusable values:** inspiration only; no swatch value or image used.
- **Required attribution:** factual source identification only.

### Pelikan Edelstein ink collection

- **Creator/publisher:** Pelikan Vertriebsgesellschaft mbH & Co. KG
- **URL:** <https://www.pelikan-passion.com/images/international/assets/catalogs/fine-writing-instruments-current-catalog-en.pdf>
- **Stated licence:** all-rights-reserved catalog; no reuse licence stated.
- **AI disclosure:** none stated
- **Visual lesson:** Olivine, Aquamarine, Smoky Quartz, Amethyst, Garnet, and
  Amber reinforce muted green/teal/grey/violet/red/ochre roles.
- **Exact reusable values:** inspiration only; no swatch value or image used.
- **Required attribution:** factual source identification only.

## CC0 palette references

The following source pages name **Paleto** as author, state **CC0** with no
attribution required, and identify the work as **AI Assisted**. Nib gives
credit voluntarily. Every palette was inspiration only: no download, asset,
or exact value was copied, so there is no required attribution.

| Source title | Direct URL | Page AI disclosure | Relevant visual lesson |
| --- | --- | --- | --- |
| Paleto — Deep Sea | <https://paleto.itch.io/paleto-deep-sea> | Graphics | ocean void, cyan/coral counterpoint, pale beams |
| Paleto — Water | <https://paleto.itch.io/paleto-water> | Code, graphics, sounds | teal depth, green steps, restrained foam light |
| Paleto — Green | <https://paleto.itch.io/paleto-green> | Code, graphics, sounds | canopy/moss hierarchy, bark, birch calibration |
| Paleto — Stone | <https://paleto.itch.io/paleto-stone> | Code, graphics, sounds | cave dark, warm slate/earth, bleached stone |
| Paleto — Sky | <https://paleto.itch.io/paleto-sky> | Code, graphics, sounds | midnight-to-cloud span and warm dawn accent |
| Paleto — Western / Desert | <https://paleto.itch.io/paleto-western-desert> | Graphics | ochre, leather, mesa rust, cactus green |
| Paleto — Bloom | <https://paleto.itch.io/paleto-bloom> | Code, graphics, sounds | controlled rose, lavender, and gold on green |
| Paleto — Dead City | <https://paleto.itch.io/paleto-dead-city> | Graphics | ash, rubble, rust, dried red, dry separation |
| Paleto — Fantasy RPG | <https://paleto.itch.io/paleto-fantasy-rpg> | Graphics | warm earth/gold balanced against arcane blue |

## Non-affiliation

Nib is an independent software theme. It is not affiliated with,
sponsored by, endorsed by, or licensed by Pilot Corporation, C. Josef Lamy
GmbH, The Sailor Pen Co., Diamine Inks, Pelikan, Paleto, itch.io, Ghostty,
Neovim, GNU/FSF, Microsoft, Visual Studio Code, Cursor/Anysphere, Zed
Industries, or the plugin projects listed above. Manufacturer and product
names appear only for factual research attribution.
