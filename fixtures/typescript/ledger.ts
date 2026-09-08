export interface Entry {
  readonly title: string;
  readonly tags: readonly string[];
  settled: boolean;
}

export function parseEntry(raw: string): Entry {
  const [title = "", ...tags] = raw.split("#").map((part) => part.trim());
  if (!title) throw new Error("title is required");
  return { title, tags: tags.filter(Boolean), settled: false };
}

export const summarize = ({ title, tags, settled }: Entry): string =>
  `${settled ? "✓" : "·"} ${title} [${tags.join(", ")}]`;
