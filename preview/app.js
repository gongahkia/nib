(() => {
  "use strict";

  const palette = window.QUIREVEIL_PALETTE;
  if (!palette) {
    throw new Error("Generated palette data did not load");
  }

  const samples = {
    typescript: [
      '<span class="tok-comment">// a small ledger with deliberate failure states</span>',
      '<span class="tok-keyword">type</span> <span class="tok-type">Entry</span> <span class="tok-operator">=</span> <span class="tok-punctuation">{</span>',
      '  <span class="tok-property">title</span><span class="tok-punctuation">:</span> <span class="tok-type">string</span><span class="tok-punctuation">;</span>',
      '  <span class="tok-property">settled</span><span class="tok-punctuation">:</span> <span class="tok-type">boolean</span><span class="tok-punctuation">;</span>',
      '<span class="tok-punctuation">}</span><span class="tok-punctuation">;</span>',
      '',
      '<span class="tok-keyword">export</span> <span class="tok-keyword">function</span> <span class="tok-function tok-search">parseEntry</span><span class="tok-punctuation">(</span><span class="tok-property">raw</span><span class="tok-punctuation">:</span> <span class="tok-type">string</span><span class="tok-punctuation">)</span> <span class="tok-punctuation">{</span>',
      '  <span class="tok-keyword">const</span> <span class="tok-property">value</span> <span class="tok-operator">=</span> <span class="tok-type">JSON</span><span class="tok-punctuation">.</span><span class="tok-function">parse</span><span class="tok-punctuation">(</span><span class="tok-property">raw</span><span class="tok-punctuation">)</span><span class="tok-punctuation">;</span>',
      '  <span class="tok-control">if</span> <span class="tok-punctuation">(</span><span class="tok-operator">!</span><span class="tok-property">value</span><span class="tok-punctuation">.</span><span class="tok-property">title</span><span class="tok-punctuation">)</span> <span class="tok-control">throw</span> <span class="tok-keyword">new</span> <span class="tok-type">Error</span><span class="tok-punctuation">(</span><span class="tok-string">"missing title"</span><span class="tok-punctuation">)</span><span class="tok-punctuation">;</span>',
      '  <span class="tok-control">return</span> <span class="tok-selection"><span class="tok-property">value</span> <span class="tok-keyword">as</span> <span class="tok-type">Entry</span></span><span class="tok-punctuation">;</span>',
      '<span class="tok-punctuation">}</span>',
      '<span class="tok-function tok-error">parseEntry</span><span class="tok-punctuation">(</span><span class="tok-number">42</span><span class="tok-punctuation">)</span><span class="tok-punctuation">;</span>',
    ],
    rust: [
      '<span class="tok-comment">// values stay quiet; ownership remains visible</span>',
      '<span class="tok-keyword">struct</span> <span class="tok-type">Entry</span> <span class="tok-punctuation">{</span>',
      '    <span class="tok-property">title</span><span class="tok-punctuation">:</span> <span class="tok-type">String</span><span class="tok-punctuation">,</span>',
      '    <span class="tok-property">settled</span><span class="tok-punctuation">:</span> <span class="tok-type">bool</span><span class="tok-punctuation">,</span>',
      '<span class="tok-punctuation">}</span>',
      '',
      '<span class="tok-keyword">fn</span> <span class="tok-function tok-search">write_entry</span><span class="tok-punctuation">(</span><span class="tok-property">entry</span><span class="tok-punctuation">:</span> <span class="tok-operator">&amp;</span><span class="tok-type">Entry</span><span class="tok-punctuation">)</span> <span class="tok-operator">-&gt;</span> <span class="tok-type">Result</span><span class="tok-operator">&lt;</span><span class="tok-punctuation">()</span><span class="tok-punctuation">,</span> <span class="tok-type">Error</span><span class="tok-operator">&gt;</span> <span class="tok-punctuation">{</span>',
      '    <span class="tok-function">println!</span><span class="tok-punctuation">(</span><span class="tok-string">"{}"</span><span class="tok-punctuation">,</span> <span class="tok-property">entry</span><span class="tok-punctuation">.</span><span class="tok-property">title</span><span class="tok-punctuation">)</span><span class="tok-punctuation">;</span>',
      '    <span class="tok-control">if</span> <span class="tok-property">entry</span><span class="tok-punctuation">.</span><span class="tok-property">settled</span> <span class="tok-punctuation">{</span> <span class="tok-control">return</span> <span class="tok-type">Ok</span><span class="tok-punctuation">(())</span><span class="tok-punctuation">;</span> <span class="tok-punctuation">}</span>',
      '    <span class="tok-type">Err</span><span class="tok-punctuation">(</span><span class="tok-function">todo!</span><span class="tok-punctuation">())</span>',
      '<span class="tok-punctuation">}</span>',
      '<span class="tok-function tok-error">write_entry</span><span class="tok-punctuation">(</span><span class="tok-number">42</span><span class="tok-punctuation">)</span><span class="tok-punctuation">;</span>',
    ],
    python: [
      '<span class="tok-comment"># parse one notebook line without hiding failure</span>',
      '<span class="tok-keyword">from</span> <span class="tok-type">dataclasses</span> <span class="tok-keyword">import</span> <span class="tok-function">dataclass</span>',
      '',
      '<span class="tok-special">@dataclass</span><span class="tok-punctuation">(</span><span class="tok-property">frozen</span><span class="tok-operator">=</span><span class="tok-keyword">True</span><span class="tok-punctuation">)</span>',
      '<span class="tok-keyword">class</span> <span class="tok-type">Entry</span><span class="tok-punctuation">:</span>',
      '    <span class="tok-property">title</span><span class="tok-punctuation">:</span> <span class="tok-type">str</span>',
      '    <span class="tok-property">settled</span><span class="tok-punctuation">:</span> <span class="tok-type">bool</span> <span class="tok-operator">=</span> <span class="tok-keyword">False</span>',
      '',
      '<span class="tok-keyword">def</span> <span class="tok-function tok-search">parse_entry</span><span class="tok-punctuation">(</span><span class="tok-property">raw</span><span class="tok-punctuation">:</span> <span class="tok-type">str</span><span class="tok-punctuation">)</span> <span class="tok-operator">-&gt;</span> <span class="tok-type">Entry</span><span class="tok-punctuation">:</span>',
      '    <span class="tok-control">if</span> <span class="tok-operator">not</span> <span class="tok-property">raw</span><span class="tok-punctuation">.</span><span class="tok-function">strip</span><span class="tok-punctuation">():</span> <span class="tok-control">raise</span> <span class="tok-type">ValueError</span><span class="tok-punctuation">(</span><span class="tok-string">"empty"</span><span class="tok-punctuation">)</span>',
      '    <span class="tok-control">return</span> <span class="tok-selection"><span class="tok-type">Entry</span><span class="tok-punctuation">(</span><span class="tok-property">raw</span><span class="tok-punctuation">.</span><span class="tok-function">strip</span><span class="tok-punctuation">())</span></span>',
      '<span class="tok-function tok-error">parse_entry</span><span class="tok-punctuation">(</span><span class="tok-number">42</span><span class="tok-punctuation">)</span>',
    ],
    lua: [
      '<span class="tok-comment">-- one compact module, no runtime dependency</span>',
      '<span class="tok-keyword">local</span> <span class="tok-type">Ledger</span> <span class="tok-operator">=</span> <span class="tok-punctuation">{}</span>',
      '<span class="tok-type">Ledger</span><span class="tok-punctuation">.</span><span class="tok-property">__index</span> <span class="tok-operator">=</span> <span class="tok-type">Ledger</span>',
      '',
      '<span class="tok-keyword">function</span> <span class="tok-type">Ledger</span><span class="tok-punctuation">:</span><span class="tok-function tok-search">new</span><span class="tok-punctuation">(</span><span class="tok-property">title</span><span class="tok-punctuation">)</span>',
      '  <span class="tok-control">if</span> <span class="tok-property">title</span> <span class="tok-operator">==</span> <span class="tok-string">""</span> <span class="tok-control">then</span>',
      '    <span class="tok-function">error</span><span class="tok-punctuation">(</span><span class="tok-string">"title required"</span><span class="tok-punctuation">)</span>',
      '  <span class="tok-control">end</span>',
      '  <span class="tok-control">return</span> <span class="tok-function">setmetatable</span><span class="tok-punctuation">(</span><span class="tok-selection"><span class="tok-punctuation">{</span> <span class="tok-property">title</span> <span class="tok-operator">=</span> <span class="tok-property">title</span> <span class="tok-punctuation">}</span></span><span class="tok-punctuation">,</span> <span class="tok-type">Ledger</span><span class="tok-punctuation">)</span>',
      '<span class="tok-control">end</span>',
      '',
      '<span class="tok-type">Ledger</span><span class="tok-punctuation">:</span><span class="tok-function tok-error">new</span><span class="tok-punctuation">(</span><span class="tok-number">42</span><span class="tok-punctuation">)</span>',
    ],
    diff: [
      '<span class="tok-comment">diff --git a/ledger.ts b/ledger.ts</span>',
      '<span class="tok-special">index 6cc2..83ad 100644</span>',
      '<span class="tok-keyword">--- a/ledger.ts</span>',
      '<span class="tok-keyword">+++ b/ledger.ts</span>',
      '<span class="tok-special">@@ -4,7 +4,8 @@</span>',
      '<span class="tok-control">- const tone = "electric";</span>',
      '<span class="tok-type">+ const tone = "restrained";</span>',
      '<span class="tok-type">+ const paper = "ivory";</span>',
      '  <span class="tok-keyword">return</span> <span class="tok-punctuation">{</span> <span class="tok-property">tone</span><span class="tok-punctuation">,</span> <span class="tok-property">paper</span> <span class="tok-punctuation">}</span><span class="tok-punctuation">;</span>',
      '',
      '<span class="tok-comment"># state is also carried by + / − signs</span>',
      '<span class="tok-search">semantic identities remain intact</span>',
    ],
    markdown: [
      '<span class="tok-function"># Field notes</span>',
      '',
      '<span class="tok-comment">&gt; Paper is part of the interface.</span>',
      '',
      'Keep <span class="tok-type">**meaning**</span> ahead of <span class="tok-special">_ornament_</span>.',
      '',
      '<span class="tok-keyword">## Checks</span>',
      '',
      '<span class="tok-property">- [x]</span> opaque core palette',
      '<span class="tok-property">- [x]</span> conventional ANSI colors',
      '<span class="tok-property">- [ ]</span> <span class="tok-search">optional shader</span>',
      '<span class="tok-string">`make verify`</span>',
    ],
  };

  const swatchRoles = [
    ["blue ink", "blue_ink.primary"],
    ["moss", "moss.primary"],
    ["teal", "teal.primary"],
    ["burgundy", "burgundy"],
    ["rust", "rust"],
    ["violet", "violet"],
    ["amber", "amber"],
    ["graphite", "graphite"],
  ];

  const get = (object, path) => path.split(".").reduce((value, key) => value[key], object);
  const escape = (text) => text.replaceAll("&", "&amp;").replaceAll("<", "&lt;").replaceAll(">", "&gt;");

  function codeMarkup() {
    return samples[document.querySelector("#sample-select").value].join("\n");
  }

  function renderCode() {
    const markup = codeMarkup();
    document.querySelectorAll("[data-code]").forEach((node) => { node.innerHTML = markup; });
    document.querySelectorAll(".mini-editor").forEach((node) => { node.innerHTML = `<pre>${markup.split("\n").slice(0, 7).join("\n")}</pre>`; });
  }

  function renderSwatches() {
    document.querySelectorAll("[data-swatches]").forEach((container) => {
      const style = container.closest("[data-theme]").dataset.theme;
      const mode = palette.modes[style];
      container.innerHTML = swatchRoles.map(([label, path]) => {
        const color = get(mode, path);
        return `<div class="swatch"><div class="swatch-color" style="background:${color}"></div><span>${escape(label)} · ${color}</span></div>`;
      }).join("");
    });
  }

  function renderAnsi() {
    document.querySelectorAll("[data-ansi]").forEach((container) => {
      const style = container.closest("[data-theme]").dataset.theme;
      container.innerHTML = palette.modes[style].ansi.map((entry) =>
        `<div class="ansi-cell"><div class="ansi-chip" style="background:${entry.hex}"></div><span>${entry.index} · ${escape(entry.index < 8 ? entry.name : `+${entry.name.replace("bright ", "")}`)}</span><span>${entry.hex}</span></div>`
      ).join("");
    });
  }

  const transcript = `<span class="ansi-6">quireveil</span> <span class="ansi-8">main</span> <span class="ansi-4">~/notes</span> $ git status --short
<span class="ansi-3"> M</span> palette/palette.json
<span class="ansi-2">??</span> fixtures/ledger.rs

$ make verify
<span class="ansi-2">PASS</span> palette schema and generated drift
<span class="ansi-2">PASS</span> Neovim light/dark loading
<span class="ansi-3">WARN</span> remote host has no truecolor terminfo
<span class="ansi-1">FAIL</span> fixture: expected 4, received 3
<span class="ansi-5">hint</span>: inspect <span class="ansi-6">https://example.test/report</span>`;

  function renderTranscripts() {
    document.querySelectorAll("[data-transcript]").forEach((node) => { node.innerHTML = transcript; });
    document.querySelectorAll(".mini-terminal").forEach((node) => { node.innerHTML = `<pre>${transcript.split("\n").slice(0, 6).join("\n")}</pre>`; });
  }

  const linear = (value) => {
    const channel = value / 255;
    return channel <= 0.04045 ? channel / 12.92 : ((channel + 0.055) / 1.055) ** 2.4;
  };
  const luminance = (hex) => 0.2126 * linear(parseInt(hex.slice(1, 3), 16)) + 0.7152 * linear(parseInt(hex.slice(3, 5), 16)) + 0.0722 * linear(parseInt(hex.slice(5, 7), 16));
  const contrast = (first, second) => {
    const [high, low] = [luminance(first), luminance(second)].sort((a, b) => b - a);
    return (high + 0.05) / (low + 0.05);
  };

  function renderContrast() {
    const rows = [
      ["Principal text", "foreground.primary", "background", 7],
      ["Comments", "foreground.muted", "background", 4.5],
      ["Moss syntax", "moss.primary", "background", 4.5],
      ["Teal strings", "teal.primary", "background", 4.5],
      ["Error", "diagnostic.error", "background", 4.5],
      ["Boundary", "border.default", "background", 3],
      ["Selection", "selection.foreground", "selection.background", 4.5],
      ["Current search", "search.current_foreground", "search.current_background", 4.5],
    ];
    document.querySelector("#contrast-body").innerHTML = ["light", "dark"].flatMap((style) => rows.map(([label, foregroundPath, backgroundPath, target]) => {
      const mode = palette.modes[style];
      const foreground = get(mode, foregroundPath);
      const background = get(mode, backgroundPath);
      const ratio = contrast(foreground, background);
      return `<tr><td>${style}</td><td>${label}</td><td><span class="color-pair"><i style="background:${foreground}"></i><i style="background:${background}"></i></span>${foreground} / ${background}</td><td class="pass">${ratio.toFixed(2)}:1</td><td>${target.toFixed(1)}:1</td></tr>`;
    })).join("");
  }

  function setMode(mode) {
    const grid = document.querySelector("#mode-grid");
    grid.classList.toggle("show-light", mode === "light");
    grid.classList.toggle("show-dark", mode === "dark");
    document.querySelectorAll(".mode-button").forEach((button) => {
      const active = button.dataset.mode === mode;
      button.classList.toggle("is-active", active);
      button.setAttribute("aria-pressed", String(active));
    });
    document.querySelector("#control-status").textContent = `${mode === "both" ? "Both modes" : `${mode[0].toUpperCase()}${mode.slice(1)} mode`} visible. ${document.querySelector("#sample-select").selectedOptions[0].text} sample selected.`;
  }

  document.querySelectorAll(".mode-button").forEach((button) => button.addEventListener("click", () => setMode(button.dataset.mode)));
  document.querySelector("#sample-select").addEventListener("change", () => {
    renderCode();
    const active = document.querySelector(".mode-button.is-active").dataset.mode;
    setMode(active);
  });

  renderCode();
  renderSwatches();
  renderAnsi();
  renderTranscripts();
  renderContrast();
})();
