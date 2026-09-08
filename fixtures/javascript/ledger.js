export function parseEntry(raw) {
  const [title = "", ...tags] = raw.split("#").map((part) => part.trim());
  if (!title) {
    throw new TypeError("title is required");
  }
  return Object.freeze({ title, tags: tags.filter(Boolean), settled: false });
}

export function summarize({ title, tags, settled }) {
  const mark = settled ? "✓" : "·";
  return `${mark} ${title} [${tags.join(", ")}]`;
}
