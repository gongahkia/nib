local bit = require("bit")
local PRNG = {}
PRNG.__index = PRNG

local function u32(x)
  if x < 0 then return x + 4294967296 end
  return x
end

function PRNG.new(seed)
  seed = tonumber(seed) or 1
  seed = bit.tobit(seed)
  if seed == 0 then seed = 0x6d2b79f5 end
  return setmetatable({ state = seed }, PRNG)
end

function PRNG:nextU32()
  local x = self.state
  x = bit.bxor(x, bit.lshift(x, 13))
  x = bit.bxor(x, bit.rshift(x, 17))
  x = bit.bxor(x, bit.lshift(x, 5))
  self.state = bit.tobit(x)
  return u32(self.state)
end

function PRNG:float() return self:nextU32() / 4294967296 end
function PRNG:int(a, b) return a + math.floor(self:float() * (b - a + 1)) end
function PRNG:choice(items) return items[self:int(1, #items)] end
function PRNG:chance(p) return self:float() < p end
function PRNG:clone() return PRNG.new(self.state) end

return PRNG
