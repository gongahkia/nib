local Fixed = {}
Fixed.__index = Fixed

function Fixed.new(step, update)
  return setmetatable({ step = step, updateFn = update, accumulator = 0, ticks = 0 }, Fixed)
end

function Fixed:advance(dt)
  self.accumulator = math.min(self.accumulator + dt, self.step * 8)
  local count = 0
  while self.accumulator >= self.step do
    self.updateFn(self.step); self.accumulator, self.ticks, count = self.accumulator - self.step, self.ticks + 1, count + 1
  end
  return count
end

return Fixed
