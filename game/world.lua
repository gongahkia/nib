local World = {}
World.__index = World

function World.new(data)
  return setmetatable({
    solids = data and data.solids or {}, anchors = data and data.anchors or {}, rails = data and data.rails or {},
    objects = data and data.objects or {}, enemies = data and data.enemies or {}, events = {},
  }, World)
end

function World.playground()
  return World.new({
    solids = {
      { x = -40, y = 104, w = 245, h = 24, material = "stone" },
      { x = 45, y = 85, w = 12, h = 19, material = "brick" },
      { x = 78, y = 95, w = 32, h = 9, material = "metal" },
      { x = 124, y = 74, w = 10, h = 30, material = "sticky" },
    },
    anchors = { { x = 68, y = 59 }, { x = 113, y = 55 }, { x = 151, y = 43 } },
    rails = { { x1 = 72, y1 = 72, x2 = 112, y2 = 68 } },
  })
end

local function overlap(a, b)
  return a.x < b.x + b.w and a.x + a.w > b.x and a.y < b.y + b.h and a.y + a.h > b.y
end

function World:queryRect(x, y, w, h)
  local result, box = {}, { x = x, y = y, w = w, h = h }
  for _, solid in ipairs(self.solids) do if not solid.destroyed and overlap(box, solid) then result[#result + 1] = solid end end
  return result
end

function World:nearestAnchor(x, y, dx, dy, range)
  local best, bestDistance
  for _, a in ipairs(self.anchors) do
    local ox, oy = a.x - x, a.y - y
    local distance = math.sqrt(ox * ox + oy * oy)
    local dot = distance > 0 and (ox * dx + oy * dy) / distance or 0
    if distance <= range and dot > 0.55 and (not bestDistance or distance < bestDistance) then best, bestDistance = a, distance end
  end
  return best
end

return World
