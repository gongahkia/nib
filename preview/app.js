(() => {
  "use strict";

  const palette = window.NIB_PALETTE;
  const comparisons = window.NIB_COMPARISONS;
  if (!palette || !comparisons) {
    throw new Error("Generated comparison data did not load");
  }

  const samples = {
    typescript: [
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
    ],
    rust: [
      '<span class="tok-comment">// update one ledger entry</span>',
      '<span class="tok-keyword">struct</span> <span class="tok-type">Entry</span> <span class="tok-punctuation">{</span>',
      '  <span class="tok-property">title</span><span class="tok-punctuation">:</span> <span class="tok-type">String</span><span class="tok-punctuation">,</span>',
      '  <span class="tok-property">settled</span><span class="tok-punctuation">:</span> <span class="tok-type">bool</span><span class="tok-punctuation">,</span>',
      '<span class="tok-punctuation">}</span>',
      '',
      '<span class="tok-keyword">fn</span> <span class="tok-function">write_entry</span><span class="tok-punctuation">(</span><span class="tok-property">entry</span><span class="tok-punctuation">:</span> <span class="tok-operator">&amp;</span><span class="tok-type">Entry</span><span class="tok-punctuation">)</span> <span class="tok-operator">-&gt;</span> <span class="tok-type">Result</span><span class="tok-operator">&lt;</span><span class="tok-punctuation">()</span><span class="tok-punctuation">,</span> <span class="tok-type">Error</span><span class="tok-operator">&gt;</span> <span class="tok-punctuation">{</span>',
      '  <span class="tok-function">println!</span><span class="tok-punctuation">(</span><span class="tok-string">"{}"</span><span class="tok-punctuation">,</span> <span class="tok-property">entry</span><span class="tok-punctuation">.</span><span class="tok-property">title</span><span class="tok-punctuation">);</span>',
      '  <span class="tok-control">return</span> <span class="tok-type">Ok</span><span class="tok-punctuation">(());</span>',
      '<span class="tok-punctuation">}</span> <span class="tok-function tok-error">write_entry</span><span class="tok-punctuation">(</span><span class="tok-number">42</span><span class="tok-punctuation">);</span>',
    ],
    python: [
      '<span class="tok-comment"># immutable ledger record</span>',
      '<span class="tok-keyword">from</span> <span class="tok-type">dataclasses</span> <span class="tok-keyword">import</span> <span class="tok-function">dataclass</span>',
      '',
      '<span class="tok-special">@dataclass</span><span class="tok-punctuation">(</span><span class="tok-property">frozen</span><span class="tok-operator">=</span><span class="tok-keyword">True</span><span class="tok-punctuation">)</span>',
      '<span class="tok-keyword">class</span> <span class="tok-type">Entry</span><span class="tok-punctuation">:</span>',
      '  <span class="tok-property">title</span><span class="tok-punctuation">:</span> <span class="tok-type">str</span>',
      '  <span class="tok-property">settled</span><span class="tok-punctuation">:</span> <span class="tok-type">bool</span> <span class="tok-operator">=</span> <span class="tok-keyword">False</span>',
      '',
      '<span class="tok-keyword">def</span> <span class="tok-function">parse_entry</span><span class="tok-punctuation">(</span><span class="tok-property">raw</span><span class="tok-punctuation">:</span> <span class="tok-type">str</span><span class="tok-punctuation">)</span> <span class="tok-operator">-&gt;</span> <span class="tok-type">Entry</span><span class="tok-punctuation">:</span>',
      '  <span class="tok-control">return</span> <span class="tok-type">Entry</span><span class="tok-punctuation">(</span><span class="tok-string">"field note"</span><span class="tok-punctuation">)</span> <span class="tok-comment"># 42</span>',
    ],
    lua: [
      '<span class="tok-comment">-- construct one ledger entry</span>',
      '<span class="tok-keyword">local</span> <span class="tok-type">Ledger</span> <span class="tok-operator">=</span> <span class="tok-punctuation">{}</span>',
      '<span class="tok-type">Ledger</span><span class="tok-punctuation">.</span><span class="tok-property">__index</span> <span class="tok-operator">=</span> <span class="tok-type">Ledger</span>',
      '',
      '<span class="tok-keyword">function</span> <span class="tok-type">Ledger</span><span class="tok-punctuation">:</span><span class="tok-function">new</span><span class="tok-punctuation">(</span><span class="tok-property">title</span><span class="tok-punctuation">)</span>',
      '  <span class="tok-control">if</span> <span class="tok-property">title</span> <span class="tok-operator">==</span> <span class="tok-string">""</span> <span class="tok-control">then</span>',
      '    <span class="tok-function">error</span><span class="tok-punctuation">(</span><span class="tok-string">"title required"</span><span class="tok-punctuation">)</span>',
      '  <span class="tok-control">end</span>',
      '  <span class="tok-control">return</span> <span class="tok-function">setmetatable</span><span class="tok-punctuation">({</span> <span class="tok-property">title</span> <span class="tok-operator">=</span> <span class="tok-property">title</span> <span class="tok-punctuation">},</span> <span class="tok-type">Ledger</span><span class="tok-punctuation">)</span>',
      '<span class="tok-control">end</span> <span class="tok-number">42</span>',
    ],
    diff: [
      '<span class="tok-comment">diff --git a/theme.lua b/theme.lua</span>',
      '<span class="tok-special">index 6cc2..83ad 100644</span>',
      '<span class="tok-keyword">--- a/theme.lua</span>',
      '<span class="tok-keyword">+++ b/theme.lua</span>',
      '<span class="tok-special">@@ -4,7 +4,8 @@</span>',
      '<span class="tok-deleted">- local timeout = 30</span>',
      '<span class="tok-added">+ local timeout = 45</span>',
      '<span class="tok-added">+ local retries = 3</span>',
      '',
      '<span class="tok-control">return</span> <span class="tok-punctuation">{</span> <span class="tok-property">timeout</span><span class="tok-punctuation">,</span> <span class="tok-property">retries</span> <span class="tok-punctuation">}</span>',
    ],
    markdown: [
      '<span class="tok-function"># Field notes</span>',
      '',
      '<span class="tok-comment">&gt; Records are immutable after settlement.</span>',
      '',
      '<span class="tok-keyword">## Checks</span>',
      '',
      '<span class="tok-property">- [x]</span> parse entries',
      '<span class="tok-property">- [x]</span> validate input',
      '<span class="tok-property">- [ ]</span> <span class="tok-string">write report</span>',
      '<span class="tok-special">`make test`</span>',
    ],
  };

  const accentOrder = ["red", "orange", "yellow", "green", "cyan", "blue", "purple", "magenta"];
  const escape = (text) => String(text)
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;")
    .replaceAll('"', "&quot;");

  function nibMode(style) {
    const mode = palette.modes[style];
    return {
      variant: style === "light" ? "Light" : "Dark",
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

  const themes = [
    {
      slug: "nib",
      name: "Nib",
      source: "../palette/palette.json",
      local: true,
      modes: { light: nibMode("light"), dark: nibMode("dark") },
    },
    ...comparisons.themes,
  ];

  function styleVariables(mode) {
    const roles = mode.roles || {
      keyword: mode.accents.blue,
      control: mode.accents.red,
      type: mode.accents.green,
      function: mode.accents.blue,
      string: mode.accents.cyan,
      number: mode.accents.yellow,
      property: mode.accents.orange,
      special: mode.accents.purple,
    };
    const values = {
      background: mode.background,
      surface: mode.surface,
      foreground: mode.foreground,
      muted: mode.muted,
      red: mode.accents.red,
      green: mode.accents.green,
      keyword: roles.keyword,
      control: roles.control,
      type: roles.type,
      function: roles.function,
      string: roles.string,
      number: roles.number,
      property: roles.property,
      special: roles.special,
    };
    return Object.entries(values).map(([name, value]) => `--reference-${name}:${value}`).join(";");
  }

  function swatches(mode) {
    return accentOrder.map((name) => {
      const color = mode.accents[name];
      return `<div class="reference-swatch" style="--swatch:${color}"><i></i><span>${name}</span><small>${color}</small></div>`;
    }).join("");
  }

  function modeMarkup(style, mode) {
    return `<section class="theme-mode" data-mode-card="${style}" style="${styleVariables(mode)}" aria-label="${escape(mode.variant)} palette">
      <header class="mode-bar"><strong>${escape(mode.variant)}</strong><span class="base-values">BG ${mode.background} · TEXT ${mode.foreground}</span></header>
      <pre class="reference-code" data-reference-code></pre>
      <div class="reference-swatches" aria-label="Accent colours">${swatches(mode)}</div>
    </section>`;
  }

  function renderThemes() {
    document.querySelector("#comparison-grid").innerHTML = themes.map((theme, index) => `<article class="theme-card" id="compare-${escape(theme.slug)}">
      <header>
        <div class="theme-identity"><span class="theme-index">${theme.local ? "Current" : String(index).padStart(2, "0")}</span><h2>${escape(theme.name)}</h2></div>
        <a class="source-link" href="${escape(theme.source)}"${theme.local ? "" : ' target="_blank" rel="noreferrer"'}>Source ↗</a>
      </header>
      <div class="theme-modes">${modeMarkup("light", theme.modes.light)}${modeMarkup("dark", theme.modes.dark)}</div>
    </article>`).join("");
  }

  function renderCode() {
    const markup = samples[document.querySelector("#sample-select").value].join("\n");
    document.querySelectorAll("[data-reference-code]").forEach((node) => { node.innerHTML = markup; });
  }

  function setMode(mode) {
    const grid = document.querySelector("#comparison-grid");
    grid.classList.toggle("show-light", mode === "light");
    grid.classList.toggle("show-dark", mode === "dark");
    document.querySelectorAll(".mode-button").forEach((button) => {
      const active = button.dataset.mode === mode;
      button.classList.toggle("is-active", active);
      button.setAttribute("aria-pressed", String(active));
    });
    const sample = document.querySelector("#sample-select").selectedOptions[0].text;
    const visible = mode === "both" ? "Both modes" : `${mode[0].toUpperCase()}${mode.slice(1)} mode`;
    document.querySelector("#control-status").textContent = `${visible} visible. ${sample} sample selected.`;
  }

  renderThemes();
  renderCode();
  setMode("both");

  document.querySelectorAll(".mode-button").forEach((button) => button.addEventListener("click", () => setMode(button.dataset.mode)));
  document.querySelector("#sample-select").addEventListener("change", () => {
    renderCode();
    setMode(document.querySelector(".mode-button.is-active").dataset.mode);
  });
})();
