(() => {
  "use strict";

  const palette = window.NIB_PALETTE;
  const comparisons = window.NIB_COMPARISONS;
  if (!palette || !comparisons) {
    throw new Error("Generated comparison data did not load");
  }

  const typescriptSample = [
    '<span class="tok-comment">// normalize one ledger record</span>',
    '<span class="tok-keyword">type</span> <span class="tok-type">Entry</span> <span class="tok-operator">=</span> <span class="tok-punctuation">{</span>',
    '  <span class="tok-property">title</span><span class="tok-punctuation">:</span> <span class="tok-type">string</span><span class="tok-punctuation">;</span>',
    '  <span class="tok-property">settled</span><span class="tok-punctuation">:</span> <span class="tok-type">boolean</span><span class="tok-punctuation">;</span>',
    '<span class="tok-punctuation">}</span><span class="tok-punctuation">;</span>',
    '',
    '<span class="tok-keyword">export function</span> <span class="tok-function">parseEntry</span><span class="tok-punctuation">(</span><span class="tok-property">raw</span><span class="tok-punctuation">:</span> <span class="tok-type">string</span><span class="tok-punctuation">)</span> <span class="tok-punctuation">{</span>',
    '  <span class="tok-control">if</span> <span class="tok-punctuation">(</span><span class="tok-operator">!</span><span class="tok-property">raw</span><span class="tok-punctuation">)</span> <span class="tok-control">throw</span> <span class="tok-keyword">new</span> <span class="tok-type">Error</span><span class="tok-punctuation">(</span><span class="tok-string">"empty"</span><span class="tok-punctuation">);</span>',
    '  <span class="tok-control">return</span> <span class="tok-special">JSON</span><span class="tok-punctuation">.</span><span class="tok-function">parse</span><span class="tok-punctuation">(</span><span class="tok-property">raw</span><span class="tok-punctuation">);</span>',
    '<span class="tok-punctuation">}</span> <span class="tok-function tok-error">parseEntry</span><span class="tok-punctuation">(</span><span class="tok-number">42</span><span class="tok-punctuation">);</span>',
  ];

  const accentOrder = ["red", "orange", "yellow", "green", "cyan", "blue", "purple", "magenta"];

  function nibMode(style) {
    const mode = palette.modes[style];
    return {
      variant: style,
      background: mode.background,
      surface: mode.surface.elevated,
      foreground: mode.foreground.primary,
      muted: mode.foreground.muted,
      accents: {
        red: mode.burgundy,
        orange: mode.rust,
        yellow: mode.amber,
        green: mode.moss.primary,
        cyan: mode.teal.primary,
        blue: mode.blue_ink.primary,
        purple: mode.violet,
        magenta: mode.burgundy,
      },
      roles: {
        keyword: mode.blue_ink.deep,
        control: mode.burgundy,
        type: mode.moss.primary,
        function: mode.blue_ink.bright,
        string: mode.teal.primary,
        number: mode.amber,
        property: mode.sepia,
        special: mode.violet,
      },
    };
  }

  const nib = {
    slug: "nib",
    name: "Nib",
    modes: { light: nibMode("light"), dark: nibMode("dark") },
  };

  function roleColors(mode) {
    return mode.roles || {
      keyword: mode.accents.blue,
      control: mode.accents.red,
      type: mode.accents.green,
      function: mode.accents.blue,
      string: mode.accents.cyan,
      number: mode.accents.yellow,
      property: mode.accents.orange,
      special: mode.accents.purple,
    };
  }

  function styleVariables(mode) {
    const values = {
      background: mode.background,
      surface: mode.surface,
      foreground: mode.foreground,
      muted: mode.muted,
      red: mode.accents.red,
      ...roleColors(mode),
    };
    return Object.entries(values).map(([name, value]) => `--reference-${name}:${value}`).join(";");
  }

  function modeColors(mode) {
    return [
      mode.background,
      mode.surface,
      mode.foreground,
      mode.muted,
      ...accentOrder.map((name) => mode.accents[name]),
    ];
  }

  function hexToOklab(hex) {
    const channels = [1, 3, 5].map((start) => Number.parseInt(hex.slice(start, start + 2), 16) / 255)
      .map((value) => (value <= 0.04045 ? value / 12.92 : ((value + 0.055) / 1.055) ** 2.4));
    const [red, green, blue] = channels;
    const light = Math.cbrt(0.4122214708 * red + 0.5363325363 * green + 0.0514459929 * blue);
    const medium = Math.cbrt(0.2119034982 * red + 0.6806995451 * green + 0.1073969566 * blue);
    const short = Math.cbrt(0.0883024619 * red + 0.2817188376 * green + 0.6299787005 * blue);
    return [
      0.2104542553 * light + 0.793617785 * medium - 0.0040720468 * short,
      1.9779984951 * light - 2.428592205 * medium + 0.4505937099 * short,
      0.0259040371 * light + 0.7827717662 * medium - 0.808675766 * short,
    ];
  }

  function similarity(first, second) {
    const firstColors = [...modeColors(first.modes.light), ...modeColors(first.modes.dark)];
    const secondColors = [...modeColors(second.modes.light), ...modeColors(second.modes.dark)];
    const distance = firstColors.reduce((sum, color, index) => {
      const left = hexToOklab(color);
      const right = hexToOklab(secondColors[index]);
      return sum + Math.hypot(left[0] - right[0], left[1] - right[1], left[2] - right[2]);
    }, 0) / firstColors.length;
    return Math.round(100 * Math.exp(-4 * distance));
  }

  function closestReference() {
    return comparisons.themes
      .map((theme) => ({ theme, score: similarity(nib, theme) }))
      .sort((left, right) => right.score - left.score)[0];
  }

  function swatches(mode) {
    return accentOrder.map((name) => {
      const color = mode.accents[name];
      return `<div class="reference-swatch" style="--swatch:${color}">
        <i aria-hidden="true"></i><span>${name}</span><small>${color}</small>
      </div>`;
    }).join("");
  }

  function themeCard(theme, style) {
    const mode = theme.modes[style];
    return `<article class="theme-card" data-theme="${theme.slug}" style="${styleVariables(mode)}">
      <header><strong>${theme.name}</strong><span>${mode.variant}</span></header>
      <pre class="reference-code">${typescriptSample.join("\n")}</pre>
      <div class="reference-swatches" aria-label="${theme.name} ${style} accent colours">${swatches(mode)}</div>
    </article>`;
  }

  const closest = closestReference();
  document.querySelector("#preview-title").textContent = `${nib.name} × ${closest.theme.name}`;
  document.querySelector("#similarity-badge").textContent = `${closest.score}% palette similarity`;
  document.querySelector("#comparison-board").innerHTML = ["light", "dark"].map((style) => `
    <section class="mode-row mode-row--${style}" aria-labelledby="${style}-title">
      <h2 id="${style}-title">${style}</h2>
      <div class="theme-pair">${themeCard(nib, style)}${themeCard(closest.theme, style)}</div>
    </section>
  `).join("");
})();
