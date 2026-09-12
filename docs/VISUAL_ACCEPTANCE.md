# Visual acceptance

This document records what has and has not been inspected as rendered pixels.
Structural checks remain in `make verify`; visual evidence is additive and must
never be inferred from a parser or headless load.

## Reference showcase

The dependency-free [`showcase`](../showcase/index.html) renders prose,
TypeScript, Python, diagnostics, diffs, foundation ramps, and ANSI colors from
the checked-in generated palette data. Its screenshots are browser reference
renders, not substitutes for native application captures.
Capture dimensions, environment, and pinned file hashes are recorded in
[`support/visual-evidence.json`](../support/visual-evidence.json).

| Environment | Mode | Viewport | Evidence | Status |
| --- | --- | --- | --- | --- |
| Fedora Linux 43, Chromium 151 | Light | 1440 × 1200 | [`nib-showcase-light.png`](../output/playwright/nib-showcase-light.png) | Pass |
| Fedora Linux 43, Chromium 151 | Dark | 1440 × 1200 | [`nib-showcase-dark.png`](../output/playwright/nib-showcase-dark.png) | Pass |

Acceptance requires readable prose and code, visible mode identity, distinct
surface elevation, non-hue diagnostic labels, separate line and inline diff
surfaces, complete 16-color ANSI output, and no horizontal page overflow at the
capture viewport.

## Priority native applications

| Target | Required scenarios | Light | Dark | Current evidence |
| --- | --- | --- | --- | --- |
| Neovim | Markdown, TypeScript, Python, Rust, JSON, diff, diagnostics | Pending | Pending | Headless runtime passes; screenshots absent |
| VS Code/Cursor | Same editor fixture set and workbench chrome | Pending | Pending | Structural checks pass; GUI unavailable |
| Obsidian | Reading view, source view, callouts, tables, graph controls | Pending | Pending | CSS and manifest checks pass |
| macOS Terminal | ANSI transcript and selection | Pending | Pending | No macOS host available |
| Ghostty | ANSI transcript, Neovim, selection, optional shaders off | Pending | Pending | Configuration runtime and shaders pass |
| Firefox | Toolbar, active/inactive tabs, private window | Pending | Pending | Manifest checks pass |
| Telegram Desktop | Chat list, conversation, composer, selection | Pending | Pending | Archive and variable checks pass |
| Discord loader | Channels, chat, composer, popovers, diagnostics | Pending | Pending | Experimental CSS; no loader capture |

No native port advances to **Verified** until both mode cells pass and link to
captures that identify application version, operating system, and fixture.

## Reproduce the reference captures

From the repository root, serve the files locally and use the bundled
Playwright CLI wrapper:

```sh
python3 -m http.server 4173
export PWCLI="${CODEX_HOME:-$HOME/.codex}/skills/playwright/scripts/playwright_cli.sh"
"$PWCLI" open http://127.0.0.1:4173/showcase/ --headed
"$PWCLI" snapshot
"$PWCLI" screenshot --filename output/playwright/nib-showcase-light.png --full-page
```

Switch to Chalkboard using the element reference from the current snapshot,
take the dark capture, then inspect both images at full resolution. Browser
screenshots are regenerated only after an intentional visual change; they are
not palette-generation outputs.
