# Support and evidence

Nib distinguishes artifact availability from visual proof. A port is not called
verified merely because its file parses or a headless runtime accepts it.

| Tier | Meaning |
| --- | --- |
| **Verified** | Manually tested in the target application with a checked-in screenshot. |
| **Generated** | Its generated artifact passes applicable format, structural, contrast, or available runtime checks. |
| **Experimental** | It depends on a third-party loader, archived upstream, or a contract that is not stable enough for ordinary support. |

The machine-readable source is [`support/targets.json`](../support/targets.json).
At present, the canonical showcase is verified in Chromium. Native application
ports remain generated or experimental until their acceptance rows in
[`VISUAL_ACCEPTANCE.md`](VISUAL_ACCEPTANCE.md) have passing screenshots. This is
intentional: headless Neovim loading and Ghostty configuration validation are
valuable evidence, but neither proves the final pixels in a desktop window.

## Current coverage

| Area | Generated | Experimental | Principal missing evidence |
| --- | --- | --- | --- |
| Editors | Emacs, Helix, IntelliJ, Lite XL, Neovim, Obsidian, Sublime Text, Vim, VS Code/Cursor, Zed | — | Native light/dark screenshots and cross-version use |
| Terminals | Alacritty, Black Box, Ghostty, iTerm2, Kitty, Konsole, macOS Terminal, Warp, WezTerm, Windows Terminal, Xresources, Zellij | — | Native application captures on Linux, macOS, and Windows |
| Browsers | Firefox, Chromium, Helium | — | Loaded-extension captures and version coverage |
| Shell/tools | fish, fzf, tmux, Yazi | Pywal | Daily-use reports and host coverage |
| Messaging | Telegram Desktop | Discord, Slack | Loader/version coverage and native captures |
| Linux desktop | Dunst, i3, Waybar, Zathura | — | Host parsing, compositor combinations, and native captures |
| Frameworks | CSS custom properties, Tailwind CSS v4 | — | Downstream integration examples |
| Creative/data | GIMP, Matplotlib, R | — | Import/runtime checks in their host applications |

Marketplace publication, community maintenance, user reports, and long-term
daily use are deliberately not claimed. Those are ecosystem maturity signals,
not properties that repository generation can establish.
