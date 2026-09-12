const root = document.documentElement;
const data = window.NIB_SHOWCASE;
const ramps = document.querySelector("#ramps");
const ansi = document.querySelector("#ansi");
const status = document.querySelector("#copy-status");

function setMode(mode) {
  root.dataset.nibTheme = mode;
  document.querySelectorAll("[data-mode]").forEach((button) => {
    button.setAttribute("aria-pressed", String(button.dataset.mode === mode));
  });
  renderAnsi(mode);
}

function announceCopy(color) {
  status.textContent = `Copied ${color}`;
  status.classList.add("visible");
  window.setTimeout(() => status.classList.remove("visible"), 1200);
}

function copyColor(color) {
  navigator.clipboard.writeText(color).then(() => announceCopy(color));
}

function renderRamps() {
  for (const [family, colors] of Object.entries(data.foundation)) {
    const row = document.createElement("div");
    row.className = "ramp";
    const heading = document.createElement("h3");
    heading.textContent = family.replace("_", " ");
    const swatches = document.createElement("div");
    swatches.className = "swatches";
    for (const [step, color] of Object.entries(colors)) {
      const button = document.createElement("button");
      button.type = "button";
      button.className = "swatch";
      button.style.setProperty("--swatch", color);
      button.title = `${family} ${step}: ${color}`;
      button.setAttribute("aria-label", `Copy ${family} ${step}, ${color}`);
      button.innerHTML = `<span>${step}</span>`;
      button.addEventListener("click", () => copyColor(color));
      swatches.append(button);
    }
    row.append(heading, swatches);
    ramps.append(row);
  }
}

function renderAnsi(mode) {
  ansi.replaceChildren();
  const modePalette = data.palette.modes[mode];
  for (const color of data.palette.modes[mode].ansi) {
    const swatch = document.createElement("div");
    swatch.className = "ansi";
    swatch.style.setProperty("--ansi", color.hex);
    swatch.style.setProperty(
      "--ansi-label",
      contrast(color.hex, modePalette.background) >
        contrast(color.hex, modePalette.foreground.primary)
        ? modePalette.background
        : modePalette.foreground.primary,
    );
    swatch.innerHTML = `<b>${color.index} · ${color.name}</b><span>${color.hex}</span>`;
    ansi.append(swatch);
  }
}

function luminance(color) {
  const channels = color.match(/[\da-f]{2}/gi).map((value) => parseInt(value, 16) / 255);
  const linear = channels.map((value) =>
    value <= 0.04045 ? value / 12.92 : ((value + 0.055) / 1.055) ** 2.4,
  );
  return 0.2126 * linear[0] + 0.7152 * linear[1] + 0.0722 * linear[2];
}

function contrast(first, second) {
  const values = [luminance(first), luminance(second)].sort((a, b) => b - a);
  return (values[0] + 0.05) / (values[1] + 0.05);
}

document.querySelectorAll("[data-mode]").forEach((button) => {
  button.addEventListener("click", () => setMode(button.dataset.mode));
});

renderRamps();
setMode("light");
