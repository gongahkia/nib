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
  const auditKey = "nib-blind-preference-audit-v1";
  const escape = (text) => String(text)
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;")
    .replaceAll('"', "&quot;");

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

  const themes = [
    {
      slug: "nib",
      name: "Nib",
      source: "palette/palette.json",
      modes: { light: nibMode("light"), dark: nibMode("dark") },
    },
    ...comparisons.themes,
  ];
  const themeBySlug = new Map(themes.map((theme) => [theme.slug, theme]));

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

  function modeColors(mode) {
    return [mode.background, mode.surface, mode.foreground, mode.muted, ...accentOrder.map((name) => mode.accents[name])];
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

  function shuffle(items) {
    const shuffled = [...items];
    for (let index = shuffled.length - 1; index > 0; index -= 1) {
      const swap = Math.floor(Math.random() * (index + 1));
      [shuffled[index], shuffled[swap]] = [shuffled[swap], shuffled[index]];
    }
    return shuffled;
  }

  function newDraft() {
    return {
      phase: "light",
      lightChoice: null,
      darkChoice: null,
      lightFeedback: "",
      darkFeedback: "",
      overallFeedback: "",
      tags: [],
    };
  }

  function createAudit() {
    const opponents = shuffle(themes.slice(1).map((theme) => theme.slug));
    return {
      version: 1,
      auditId: `nib-blind-${Date.now()}`,
      startedAt: new Date().toISOString(),
      position: 0,
      queue: opponents.map((opponent) => ({
        options: Math.random() < 0.5 ? ["nib", opponent] : [opponent, "nib"],
      })),
      responses: [],
      draft: newDraft(),
    };
  }

  function validAudit(candidate) {
    if (!candidate || candidate.version !== 1 || !Array.isArray(candidate.queue)) return false;
    if (!Number.isInteger(candidate.position) || candidate.position < 0 || candidate.position > candidate.queue.length) return false;
    if (candidate.queue.length !== themes.length - 1 || !Array.isArray(candidate.responses)) return false;
    const opponents = [];
    for (const matchup of candidate.queue) {
      if (!Array.isArray(matchup.options) || matchup.options.length !== 2 || !matchup.options.includes("nib")) return false;
      if (!matchup.options.every((slug) => themeBySlug.has(slug)) || matchup.options[0] === matchup.options[1]) return false;
      opponents.push(matchup.options.find((slug) => slug !== "nib"));
    }
    return new Set(opponents).size === themes.length - 1;
  }

  function loadAudit() {
    try {
      const candidate = JSON.parse(localStorage.getItem(auditKey) || "null");
      return validAudit(candidate) ? candidate : createAudit();
    } catch (_error) {
      return createAudit();
    }
  }

  let audit = loadAudit();
  if (audit.position < audit.queue.length && audit.draft.phase === "feedback") {
    audit.draft.phase = "dark";
  }

  function saveAudit() {
    localStorage.setItem(auditKey, JSON.stringify(audit));
  }

  function currentMatchup() {
    return audit.queue[audit.position];
  }

  function selectedTheme(choice) {
    const index = choice === "A" ? 0 : 1;
    return themeBySlug.get(currentMatchup().options[index]);
  }

  function optionMarkup(choice, style) {
    const theme = selectedTheme(choice);
    const mode = theme.modes[style];
    const selected = audit.draft[`${style}Choice`] === choice;
    return `<article class="blind-option${selected ? " is-selected" : ""}" style="${styleVariables(mode)}">
      <header><strong>Option ${choice}</strong><span>${style} mode</span></header>
      <pre class="reference-code">${typescriptSample.join("\n")}</pre>
      <div class="reference-swatches" aria-label="Option ${choice} accent colours">${swatches(mode)}</div>
      <button type="button" class="choose-option" data-choice="${choice}" aria-pressed="${selected}">${selected ? `Option ${choice} selected` : `Choose option ${choice}`}</button>
    </article>`;
  }

  function modeFeedbackMarkup(style) {
    const choice = audit.draft[`${style}Choice`];
    if (!choice) {
      return '<p class="comment-placeholder">Choose an option to add your comment.</p>';
    }
    const field = `${style}Feedback`;
    const prompt = style === "light"
      ? `Why did Option ${choice} work better in light mode?`
      : `Why did Option ${choice} work better in dark mode?`;
    const placeholder = style === "light"
      ? "Tone, readability, syntax colours…"
      : "Contrast, eye comfort, accents…";
    const otherComment = audit.draft[style === "light" ? "darkFeedback" : "lightFeedback"].trim();
    const requirement = otherComment
      ? "Optional — your other mode already has a comment."
      : "Comment on this mode or the other mode before saving.";
    return `<label class="mode-feedback">${prompt}
      <textarea data-feedback-field="${field}" rows="3" placeholder="${placeholder}">${escape(audit.draft[field])}</textarea>
      <small>${requirement}</small>
    </label>`;
  }

  function updateContinueState() {
    const button = document.querySelector("#continue-audit");
    const draft = audit.draft;
    if (draft.phase === "light") button.disabled = !draft.lightChoice;
    if (draft.phase === "dark") {
      button.disabled = !draft.darkChoice || (!draft.lightFeedback.trim() && !draft.darkFeedback.trim());
    }
  }

  function renderComplete() {
    document.querySelector("#audit-title").textContent = "Blind run complete";
    document.querySelector("#audit-instruction").textContent = "Export the hidden mappings and feedback when you are ready to synthesize the results.";
    document.querySelector("#step-number").textContent = "Complete";
    document.querySelector("#blind-stage").innerHTML = `<section class="completion-panel">
      <strong>${audit.responses.filter((response) => !response.skipped).length} answered</strong>
      <span>${audit.responses.filter((response) => response.skipped).length} skipped</span>
      <p>Theme names stayed hidden during every decision. The export contains the key needed for later analysis.</p>
      <div>
        <button type="button" class="quiet-button" data-complete-action="copy">Copy results</button>
        <button type="button" class="primary-button" data-complete-action="export">Export JSON</button>
      </div>
    </section>`;
    document.querySelector(".audit-actions").hidden = true;
  }

  function render() {
    const complete = audit.position >= audit.queue.length;
    document.body.dataset.phase = complete ? "complete" : audit.draft.phase;
    const answered = audit.responses.some((response) => !response.skipped);
    document.querySelector("#copy-audit").hidden = !answered;
    document.querySelector("#export-audit").hidden = !answered;
    document.querySelector("#audit-progress").textContent = complete
      ? `${audit.queue.length} / ${audit.queue.length} complete`
      : `Blind test ${audit.position + 1} / ${audit.queue.length}`;

    const phaseProgress = { light: 0, dark: 1 / 2, feedback: 1 / 2 };
    const progress = complete ? 100 : ((audit.position + phaseProgress[audit.draft.phase]) / audit.queue.length) * 100;
    document.querySelector("#progress-fill").style.width = `${progress}%`;
    if (complete) {
      renderComplete();
      return;
    }

    document.querySelector(".audit-actions").hidden = false;
    const phase = audit.draft.phase;
    const copy = {
      light: ["1 of 2 · light", "Choose the light mode", "Comment here or save your one comment for the dark mode."],
      dark: ["2 of 2 · dark", "Choose the dark mode", "One comment across light or dark is enough to save this matchup."],
      feedback: ["2 of 2 · dark", "Choose the dark mode", "One comment across light or dark is enough to save this matchup."],
    }[phase];
    document.querySelector("#step-number").textContent = copy[0];
    document.querySelector("#audit-title").textContent = copy[1];
    document.querySelector("#audit-instruction").textContent = copy[2];
    document.querySelector("#audit-status").textContent = "";
    document.querySelector("#skip-matchup").hidden = false;
    const continueButton = document.querySelector("#continue-audit");
    continueButton.textContent = phase === "light" ? "Continue to dark" : "Save & next matchup";
    const renderedPhase = phase === "feedback" ? "dark" : phase;
    document.querySelector("#blind-stage").innerHTML = `<div class="blind-options">${optionMarkup("A", renderedPhase)}${optionMarkup("B", renderedPhase)}</div>${modeFeedbackMarkup(renderedPhase)}`;
    updateContinueState();
  }

  function responseForCurrentMatchup(skipped = false) {
    const matchup = currentMatchup();
    const first = themeBySlug.get(matchup.options[0]);
    const second = themeBySlug.get(matchup.options[1]);
    if (skipped) {
      return {
        testNumber: audit.position + 1,
        options: { A: matchup.options[0], B: matchup.options[1] },
        skipped: true,
        recordedAt: new Date().toISOString(),
      };
    }
    return {
      testNumber: audit.position + 1,
      options: { A: matchup.options[0], B: matchup.options[1] },
      lightChoice: audit.draft.lightChoice,
      lightWinner: selectedTheme(audit.draft.lightChoice).slug,
      darkChoice: audit.draft.darkChoice,
      darkWinner: selectedTheme(audit.draft.darkChoice).slug,
      feedback: {
        light: audit.draft.lightFeedback.trim(),
        dark: audit.draft.darkFeedback.trim(),
        overall: audit.draft.overallFeedback.trim(),
        tags: [...audit.draft.tags],
      },
      normalizedPaletteSimilarity: similarity(first, second),
      skipped: false,
      recordedAt: new Date().toISOString(),
    };
  }

  function finishMatchup(skipped = false) {
    audit.responses.push(responseForCurrentMatchup(skipped));
    audit.position += 1;
    audit.draft = newDraft();
    saveAudit();
    render();
    window.scrollTo({ top: 0, behavior: "smooth" });
  }

  function chooseOption(choice) {
    audit.draft[`${audit.draft.phase}Choice`] = choice;
    saveAudit();
    render();
    document.querySelector("#audit-status").textContent = `Option ${choice} selected.`;
    document.querySelector("[data-feedback-field]")?.focus();
  }

  function reportData() {
    const identify = (slug) => {
      const theme = themeBySlug.get(slug);
      return { slug: theme.slug, name: theme.name, source: theme.source };
    };
    return {
      format: "nib-blind-preference-audit",
      version: 1,
      auditId: audit.auditId,
      startedAt: audit.startedAt,
      exportedAt: new Date().toISOString(),
      design: {
        anchor: "Nib",
        opponentOrder: "shuffled",
        sideOrder: "randomized per matchup",
        sequence: ["light choice", "dark choice", "at least one mode-specific comment"],
        sample: "TypeScript",
        identitiesVisibleDuringTest: false,
      },
      progress: { completed: audit.responses.length, total: audit.queue.length },
      responses: audit.responses.map((response) => ({
        ...response,
        options: { A: identify(response.options.A), B: identify(response.options.B) },
        lightWinner: response.lightWinner ? identify(response.lightWinner) : undefined,
        darkWinner: response.darkWinner ? identify(response.darkWinner) : undefined,
      })),
    };
  }

  function exportAudit() {
    const blob = new Blob([`${JSON.stringify(reportData(), null, 2)}\n`], { type: "application/json" });
    const link = document.createElement("a");
    link.href = URL.createObjectURL(blob);
    link.download = `${audit.auditId}.json`;
    document.body.append(link);
    link.click();
    link.remove();
    URL.revokeObjectURL(link.href);
    document.querySelector("#audit-status").textContent = "Results exported.";
  }

  async function copyAudit() {
    try {
      await navigator.clipboard.writeText(JSON.stringify(reportData(), null, 2));
      document.querySelector("#audit-status").textContent = "Results copied.";
    } catch (_error) {
      document.querySelector("#audit-status").textContent = "Clipboard unavailable. Use Export results.";
    }
  }

  function restartAudit() {
    if (audit.responses.length > 0 && !window.confirm("Discard this blind run and reshuffle every matchup?")) return;
    audit = createAudit();
    saveAudit();
    render();
  }

  document.querySelector("#blind-stage").addEventListener("click", (event) => {
    const choice = event.target.closest("[data-choice]");
    if (choice) chooseOption(choice.dataset.choice);
    const completeAction = event.target.closest("[data-complete-action]");
    if (completeAction?.dataset.completeAction === "copy") copyAudit();
    if (completeAction?.dataset.completeAction === "export") exportAudit();
  });
  document.querySelector("#blind-stage").addEventListener("input", (event) => {
    const field = event.target.dataset.feedbackField;
    if (!field) return;
    audit.draft[field] = event.target.value;
    saveAudit();
    updateContinueState();
  });
  document.querySelector("#continue-audit").addEventListener("click", () => {
    if (audit.draft.phase === "light" && audit.draft.lightChoice) audit.draft.phase = "dark";
    else if (
      ["dark", "feedback"].includes(audit.draft.phase)
      && audit.draft.darkChoice
      && (audit.draft.lightFeedback.trim() || audit.draft.darkFeedback.trim())
    ) {
      finishMatchup();
      return;
    }
    saveAudit();
    render();
  });
  document.querySelector("#skip-matchup").addEventListener("click", () => finishMatchup(true));
  document.querySelector("#restart-audit").addEventListener("click", restartAudit);
  document.querySelector("#export-audit").addEventListener("click", exportAudit);
  document.querySelector("#copy-audit").addEventListener("click", copyAudit);

  saveAudit();
  render();
})();
