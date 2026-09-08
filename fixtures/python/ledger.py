from dataclasses import dataclass, field


@dataclass(frozen=True, slots=True)
class Entry:
    title: str
    tags: tuple[str, ...] = field(default_factory=tuple)
    settled: bool = False

    def summary(self) -> str:
        mark = "✓" if self.settled else "·"
        return f"{mark} {self.title} [{', '.join(self.tags)}]"


def parse_entry(raw: str) -> Entry:
    title, *tags = (part.strip() for part in raw.split("#"))
    if not title:
        raise ValueError("title is required")
    return Entry(title=title, tags=tuple(filter(None, tags)))
