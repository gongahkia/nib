local bit = require("bit")
local M = {}

function M.fnv1a(text)
  local h = 2166136261
  for i = 1, #text do
    h = bit.tobit(bit.bxor(h, text:byte(i)))
    h = bit.tobit(h * 16777619)
  end
  return string.format("%08x", h < 0 and h + 4294967296 or h)
end

function M.state(p)
  return M.fnv1a(table.concat({
    p.tick, math.floor(p.x * 1000 + 0.5), math.floor(p.y * 1000 + 0.5),
    math.floor(p.vx * 1000 + 0.5), math.floor(p.vy * 1000 + 0.5),
    p.state, p.dashes, p.grapples, p.grounded and 1 or 0,
  }, ":"))
end

return M
