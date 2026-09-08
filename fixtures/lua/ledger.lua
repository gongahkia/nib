local Ledger = {}
Ledger.__index = Ledger

function Ledger.new(title, tags)
  assert(title ~= "", "title is required")
  return setmetatable({ title = title, tags = tags or {}, settled = false }, Ledger)
end

function Ledger:summary()
  local mark = self.settled and "✓" or "·"
  return string.format("%s %s [%s]", mark, self.title, table.concat(self.tags, ", "))
end

return Ledger
