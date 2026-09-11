The honest verdict: Nib is not shallow, and it should not be shelved. It is already unusually deep as an engineered theme, but it is still immature as a general colour system and public-facing product.

Flexoki is vertically deep: coherent colour theory, reusable ramps, clear usage rules, visual proof, and years of ecosystem adoption. Nib is horizontally deep: many ports, detailed semantic mappings, accessibility checks, deterministic generation, and strong maintenance infrastructure.

The next move should not be another batch of ports. Nib needs consolidation, visual validation, and a more formal foundation palette.

## Where each project is deep

| Dimension | Flexoki | Nib | Honest assessment |
| --- | --- | --- | --- |
| Underlying colour system | Excellent | Incomplete | Flexoki clearly wins |
| Semantic application roles | Strong but compact | Very strong | Nib is arguably deeper |
| Accessibility engineering | Moderate/implicit | Excellent | Nib clearly wins |
| Port breadth | Very broad | Broad | Flexoki still leads |
| Port consistency | Mixed community implementations | Centrally generated | Nib has the better architecture |
| Real application testing | Mature and visible | Limited | Flexoki clearly wins |
| Visual documentation | Excellent | Sparse | Largest visible Nib weakness |
| Community/distribution maturity | Established since 2023 | Early `0.1.0` project | Flexoki clearly wins |
| Personal preference evidence | Designer judgment | Blind personal testing | Nib has unusually good evidence for your own use |

## 1. Flexoki is a colour system; Nib is currently a semantic theme system

This is the biggest substantive difference.

Flexoki has:

- A 15-value neutral foundation from paper to black.
- Eight accent families.
- Thirteen steps per accent, from `50` through `950`.
- A total of 119 reusable foundation colours.
- Explicit rules saying light interfaces generally use the darker `600` accents and dark interfaces use the lighter `400` accents.
- A compact mapping from those foundations into background, UI, text, syntax, and diagnostic roles.

Its author explains the relationship to subtractive ink, additive displays, Oklab, perceptual lightness, chroma and the deliberate decision to retain some artistic irregularity. The extended ramps increase intensity non-linearly to imitate diluted pigment retaining character. That is genuine design thinking, not website padding. [Flexoki’s complete rationale and palette](https://stephango.com/flexoki).

Nib contains 114 distinct locked colours—almost the same raw quantity—but they are structured very differently:

- 56 colours belong only to light mode.
- 58 belong only to dark mode.
- The two modes share no exact colour values.
- Most colours are attached directly to semantic jobs: selection, diff surfaces, ANSI slots, borders, search, cursor, diagnostics and so on.
- Blue, moss and teal have partial internal variation, while burgundy, rust, violet, amber and the other families do not have complete reusable ramps.

That makes [Nib’s palette](/Users/gabrielongtemasek/Desktop/personal/nib/palette/palette.json) excellent for generating the applications already understood by the repository, but weaker for designing a new, complex interface. If a future port needs five burgundy surfaces, three hover levels or a subdued amber border, there is no systematic scale to choose from.

This is not evidence that Nib’s colours are bad. It means the abstraction begins one layer too high.

## 2. Nib has stronger engineering discipline

Nib’s implementation machinery is substantially more rigorous than Flexoki’s central repository:

- 77 generated artifacts come from one locked palette.
- The approved palette is pinned by SHA-256.
- There are schema, contrast, colour-vision, ANSI, structural, runtime and deterministic-generation checks.
- Nib currently passes all 19 verification groups.
- Port mappings are first-party and updated through one generator.
- Critical states use shape, signs, undercurls and surface changes rather than hue alone.

That work lives in [generate.py](/Users/gabrielongtemasek/Desktop/personal/nib/scripts/generate.py), [verify.py](/Users/gabrielongtemasek/Desktop/personal/nib/tests/verify.py:1067), and the generated [contrast report](/Users/gabrielongtemasek/Desktop/personal/nib/docs/generated/CONTRAST.md).

Flexoki’s included generator currently generates iTerm2 files; much of its broader application support consists of independently authored community ports and externally maintained repositories. That ecosystem model has produced impressive breadth, but it does not offer Nib’s single-source reproducibility. [Flexoki’s generator entry point](https://github.com/kepano/flexoki/blob/main/_generators/src/index.ts), [Flexoki’s community port catalogue](https://github.com/kepano/flexoki).

So Nib should not imitate Flexoki’s repository architecture. Nib’s architecture is already better for preventing drift.

## 3. Nib’s accessibility work is more concrete

Flexoki describes itself as high-contrast and perceptually calibrated. Its principal text is indeed extremely strong. But applying the same contrast calculation used by Nib reveals some intentional low-contrast roles:

- Light `tx-3`, mapped to comments: approximately `2.00:1`.
- Dark `tx-3`: approximately `2.61:1`.
- Light yellow: `3.39:1`.
- Light green: `4.39:1`.
- Light cyan: `4.43:1`.
- Dark red: `4.42:1`.

That does not make Flexoki broadly inaccessible—the exact requirement depends on role, size and presentation—but some colours it maps to ordinary syntax text do not meet a strict `4.5:1` threshold.

Nib’s meaningful syntax colours and comments all clear `4.5:1`; principal text reaches `13.01:1` light and `12.70:1` dark. It also evaluates colour-vision convergence and records where redundant indicators are required.

This is one area where Nib should retain its own philosophy rather than copy Flexoki.

## 4. Flexoki’s support is broader, but Nib’s “supported” claim needs better evidence

Flexoki still covers areas Nib does not:

- GTK, KDE system colours, Qt, i3, Waybar, Dunst and other desktop components.
- Drafts, Ulysses, Typora, Standard Notes and NetNewsWire.
- Affinity, GIMP, Clip Studio Paint, Figma, Matplotlib and R.
- Framework packages such as VitePress, Starlight and theme.sh.

Its README also links several independently maintained implementations rather than pretending everything is maintained in the main repository. [Current Flexoki ports](https://github.com/kepano/flexoki#ports).

Nib now has very good editor, terminal, browser, command-line and messaging coverage. But most of the newest files have only format and structural validation. They have not accumulated:

- Real daily use.
- Screenshots from the target application.
- Testing across application versions and operating systems.
- Reports from other users.
- Marketplace or theme-gallery releases.

The Obsidian comparison makes this especially visible. Nib has a correct paired CSS package. Flexoki has a standalone theme and is also integrated into Minimal, an Obsidian project with over 1,500 commits, extensive plugin support, configurable modes and desktop/mobile usage. [Flexoki Obsidian](https://github.com/kepano/flexoki-obsidian), [Minimal](https://github.com/kepano/obsidian-minimal).

Nib supports Obsidian technically. Flexoki supports an Obsidian ecosystem.

## 5. The visual/presentation gap is enormous

Flexoki’s site functions as its canonical specification:

- Interactive light/dark switching.
- Copyable swatches.
- Syntax examples.
- Base and extended palettes.
- An explanation of the design problem.
- Semantic mappings.
- Port catalogue.
- Changelog.

That presentation helps other people understand how to apply the palette without asking the author. It is part documentation, part validation and part contributor API.

Nib currently has no screenshots or image previews anywhere in the repository. The prose is detailed, but a visitor cannot immediately answer:

- What does Nib actually look like?
- Does it work for prose as well as TypeScript?
- How do diagnostics and diffs look?
- How consistent are Neovim, Obsidian, VS Code and Terminal?
- Are light and dark recognizably the same family?
- Does the fountain-pen premise appear visually, or only in the description?

There are also two small signs of documentation drift:

- [CONTRIBUTING.md](/Users/gabrielongtemasek/Desktop/personal/nib/CONTRIBUTING.md:8) still tells contributors to inspect removed preview modes.
- [SHADERS.md](/Users/gabrielongtemasek/Desktop/personal/nib/docs/SHADERS.md:66) calls screenshot artifacts authoritative even though none currently exist.

Those are easy fixes, but they demonstrate why visual QA needs a defined home.

## 6. Nib is conceptually distinct—but its wording risks making it feel derivative

The colours themselves are distinct:

- Nib’s light surface is cooler and greyer.
- Its principal ink is blue-black rather than Flexoki’s warm near-black.
- Its dark mode is green-neutral chalkboard charcoal rather than Flexoki’s warm black.
- Nib’s syntax accents are deliberately quieter.
- Nib has a stronger hierarchy around blue-black, moss and teal.

However, both projects currently introduce themselves with nearly the same “inky colour scheme for prose/reading, writing, and code” formulation. Nib’s README structure was also intentionally patterned after Flexoki.

That makes Nib seem more derivative on first contact than its actual colours justify.

Nib should lean harder into what is uniquely its own:

- Fountain-pen line density rather than printing ink.
- Blue-black as the structural ink.
- Diluted washes for selection and diff surfaces.
- Pooled/saturated ink for focus and diagnostics.
- Moss, sepia and graphite as writing-material colours.
- Cool-neutral paper and chalkboard charcoal.
- Low-glare long-session comfort proven through your blind audit.

## What I recommend

Do not revise the approved colours immediately. Deepen the system around them in this order:

1. Create a foundation palette beneath the current semantic palette.

   Establish neutral, blue-black, moss, teal, burgundy, rust, violet, amber, sepia and graphite scales. Preserve current values as anchors rather than inventing a fake mathematical origin story. Add missing steps through deliberate OKLCH work and visual review.

2. Separate the architecture into layers.

   ```text
   foundation ramps
       → light/dark aliases
           → semantic roles
               → application ports
   ```

   Nib currently combines the middle three layers inside one file.

3. Build a canonical showcase—not another comparison frontend.

   It should show both modes, copyable swatches, prose, several programming languages, diffs, diagnostics, ANSI output and a few real application captures. This would replace the missing visual evidence without reviving the completed blind-test interface.

4. Introduce support tiers.

   - Verified: manually tested in-app with screenshots.
   - Generated: format and structural checks pass.
   - Experimental: private APIs or third-party patchers.

   At present “available” obscures these important differences.

5. Run a visual acceptance pass before adding more ports.

   Prioritize Neovim, VS Code/Cursor, Obsidian, macOS Terminal, Ghostty, Firefox, Telegram and one Discord loader. Test both modes with Markdown/prose, TypeScript, Python, Rust, JSON, diffs, diagnostics and terminal ANSI output.

6. Only then expand into desktop and creative ecosystems.

   GTK/KDE/Qt and creative palette exports will benefit enormously from proper foundation ramps. Building them now would force more one-off colour invention.

The concise conclusion is: **Nib is already deeper than Flexoki in verification and semantic engineering, but Flexoki is much deeper as a foundational palette, visually proven product, and mature ecosystem.** Nib needs vertical consolidation now—not more horizontal support.
