local World = {}
World.__index = World
local materials = require("game.materials")

function World.new(data)
  return setmetatable({
    solids = data and data.solids or {}, anchors = data and data.anchors or {}, rails = data and data.rails or {},
    objects = data and data.objects or {}, enemies = data and data.enemies or {}, events = {}, materials = materials,
  }, World)
end

function World.playground()
  return World.new({
    solids = {
      { x = -40, y = 104, w = 245, h = 24, material = "stone" },
      { x = 45, y = 85, w = 12, h = 19, material = "brick" },
      { x = 78, y = 95, w = 32, h = 9, material = "metal" },
      { x = 124, y = 74, w = 10, h = 30, material = "sticky" },
      { x = 142, y = 99, w = 18, h = 5, material = "elastic" },
      { x = 164, y = 96, w = 26, h = 8, material = "ice" },
      { x = 202, y = 88, w = 10, h = 16, material = "glass" },
      { x = 218, y = 96, w = 28, h = 8, material = "brick", slope = -8 },
    },
    anchors = { { x = 68, y = 59 }, { x = 113, y = 55 }, { x = 151, y = 43 } },
    rails = { { x1 = 72, y1 = 72, x2 = 112, y2 = 68 } },
    objects = { { x = 116, y = 67, w = 6, h = 6, vx = 0, vy = 0 } },
    enemies = { { x = 181, y = 90, w = 6, h = 6, alive = true, kind = "sweeper" } },
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

function World:surfaceAt(x, y)
  local hits = self:queryRect(x - 1, y, 2, 2)
  if hits[1] then return hits[1], self.materials[hits[1].material] end
end

function World:updateObjects(dt)
  for _, object in ipairs(self.objects) do
    object.vy = (object.vy or 0) + 200 * dt
    object.x, object.y = object.x + (object.vx or 0) * dt, object.y + object.vy * dt
    object.vx = (object.vx or 0) * 0.97
    local floor = self:queryRect(object.x - object.w / 2, object.y, object.w, 1)
    if #floor > 0 and object.vy > 0 then object.y, object.vy = floor[1].y, 0 end
  end
end

function World:nearRail(x, y)
  for _, rail in ipairs(self.rails) do
    local dx, dy = rail.x2 - rail.x1, rail.y2 - rail.y1
    local t = math.max(0, math.min(1, ((x - rail.x1) * dx + (y - rail.y1) * dy) / (dx * dx + dy * dy)))
    local rx, ry = rail.x1 + t * dx, rail.y1 + t * dy
    if (x - rx)^2 + (y - ry)^2 < 9 then return rail, dx, dy end
  end
end

return World
