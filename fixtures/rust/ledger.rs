#[derive(Debug, Clone, PartialEq, Eq)]
pub struct Entry {
    pub title: String,
    pub tags: Vec<String>,
    pub settled: bool,
}

impl Entry {
    pub fn parse(raw: &str) -> Result<Self, &'static str> {
        let mut parts = raw.split('#').map(str::trim);
        let title = parts.next().unwrap_or_default();
        if title.is_empty() {
            return Err("title is required");
        }
        Ok(Self {
            title: title.to_owned(),
            tags: parts.filter(|tag| !tag.is_empty()).map(str::to_owned).collect(),
            settled: false,
        })
    }
}
