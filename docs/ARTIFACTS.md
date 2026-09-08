# Preview artifacts

The committed PNG files in `output/playwright/preview/` are deterministic browser reference renders of the static preview laboratory. They are not Ghostty or Neovim screenshots; Ghostty was available for configuration validation, but no compositor-level terminal capture was used. In particular, `shader-comparison-render.png` is visibly labelled as a simulation and does not claim to reproduce Ghostty's post-processing exactly.

The release-candidate renders were captured on Fedora Linux 43 with:

- Python 3.14.7 (`python3 -m http.server`)
- Node.js 22.22.2
- `playwright-cli` 0.1.19
- Chromium 151.0.7922.173
- a 1600 × 1100 CSS-pixel viewport and CSS-pixel screenshot scale

`playwright-cli.json` records the exact browser and viewport configuration used on the capture machine. Its absolute Chromium path is Fedora-specific; change only `executablePath` when reproducing on another system.

## Reproduce

From the repository root, start the local-only preview server:

```sh
python3 -m http.server 8765 --bind 127.0.0.1
```

In another shell, set the path to a `playwright-cli` executable or wrapper, then run:

```sh
cd output/playwright/preview
export PLAYWRIGHT_CLI_SESSION=quireveil-preview
playwright-cli open http://127.0.0.1:8765/preview/ --config playwright-cli.json
playwright-cli screenshot '#light-overview' --filename light-overview-render.png
playwright-cli screenshot '#dark-overview' --filename dark-overview-render.png
playwright-cli screenshot '#ansi-comparison' --filename ansi-comparison-render.png
playwright-cli screenshot '#syntax-detail' --filename syntax-detail-render.png
playwright-cli screenshot '#shader-comparison' --filename shader-comparison-render.png
playwright-cli close
```

The exact wrapper used for the committed files was `/home/gongahkia/.codex/skills/playwright/scripts/playwright_cli.sh`; the commands above use the portable executable name. No external request is made while the preview is running.

## Inspection record

Every committed PNG was inspected at native resolution. The browser reported no console warnings or errors, no element overflow in the five captured regions, and no request outside `127.0.0.1`. The font fell back to the platform monospace because `AtkynsonMono Nerd Font Mono` is optional and is not bundled.

| Artifact | Dimensions | Purpose |
| --- | ---: | --- |
| `light-overview-render.png` | 742 × 833 | light palette, editor, swatches, and terminal |
| `dark-overview-render.png` | 742 × 833 | dark palette, editor, swatches, and terminal |
| `ansi-comparison-render.png` | 1504 × 733 | normal and bright ANSI identities plus transcript |
| `syntax-detail-render.png` | 1504 × 686 | syntax hierarchy, selection, search, completion, diagnostics, and statusline |
| `shader-comparison-render.png` | 1504 × 406 | truthfully labelled shader-off/reference-simulation comparison |

The PNG dimensions include the element border, so they are two pixels larger than the corresponding CSS content box where a one-pixel border is present.
