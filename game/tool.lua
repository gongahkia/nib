local Tool = {}

local function pointSegmentDistance(px, py, x1, y1, x2, y2)
  local dx, dy = x2 - x1, y2 - y1
  local length2 = dx * dx + dy * dy
  if length2 == 0 then return math.sqrt((px - x1)^2 + (py - y1)^2) end
  local t = math.max(0, math.min(1, ((px - x1) * dx + (py - y1) * dy) / length2))
  local x, y = x1 + t * dx, y1 + t * dy
  return math.sqrt((px - x)^2 + (py - y)^2)
end

function Tool.use(player, world, ax, ay, mode)
  local range, x1, y1 = 20, player.x, player.y - player.h / 2
  local x2, y2 = x1 + ax * range, y1 + ay * range
  if mode == "manipulate" then
    local best
    for _, object in ipairs(world.objects) do
      local distance = math.sqrt((object.x - x2)^2 + (object.y - y2)^2)
      if distance < 12 and (not best or distance < best.distance) then best = { object = object, distance = distance } end
    end
    if best then
      local direction = player.buffers.pull and -1 or 1
      best.object.vx = (best.object.vx or 0) + ax * 42 * direction
      best.object.vy = (best.object.vy or 0) + ay * 42 * direction
      player.vx, player.vy = player.vx - ax * 19 * direction, player.vy - ay * 19 * direction
      return { kind = "manipulate", object = best.object }
    end
  else
    local hit
    for _, solid in ipairs(world.solids) do
      local material = world.materials[solid.material]
      local cx, cy = solid.x + solid.w / 2, solid.y + solid.h / 2
      if not solid.destroyed and material and material.destructible and pointSegmentDistance(cx, cy, x1, y1, x2, y2) < math.max(solid.w, solid.h) / 2 + 2 then
        solid.damage = (solid.damage or 0) + 3
        if solid.damage >= material.hardness then solid.destroyed = true end
        hit = solid; break
      end
    end
    player.vx, player.vy = player.vx - ax * 24, player.vy - ay * 24
    return { kind = hit and "cut" or "recoil", terrain = hit, destroyed = hit and hit.destroyed or false }
  end
  return { kind = "recoil" }
end

return Tool
