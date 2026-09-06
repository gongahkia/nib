local config = require("game.config").hand
local Hand = {}; Hand.__index = Hand
local function solveIK(rootX, rootY, targetX, targetY, upper, lower, bend)
  local dx, dy = targetX - rootX, targetY - rootY; local distance = math.sqrt(dx * dx + dy * dy)
  local reachable = distance <= upper + lower and distance >= math.abs(upper - lower)
  local clamped = math.max(math.abs(upper - lower) + 0.001, math.min(upper + lower - 0.001, distance)); local base = math.atan2(dy, dx)
  local angle = math.acos(math.max(-1, math.min(1, (upper * upper + clamped * clamped - lower * lower) / (2 * upper * clamped))))
  local jointAngle = base + angle * bend
  return { x = rootX + math.cos(jointAngle) * upper, y = rootY + math.sin(jointAngle) * upper }, reachable
end
function Hand.new(setup, seed) return setmetatable({ x = setup.x, y = setup.y - 18, vx = 0, phase = setup.phase or 0, tick = 0, seed = seed, fingers = {}, activeContacts = 0, failedIK = 0, terrainContacts = 0, routeDistance = 0, directDistance = 0, closingSpeed = 0, captureReason = nil }, Hand) end
function Hand:routeHeight(level, x) local nearest, distance; for _, n in ipairs(level.nodes) do local d = math.abs(n.x - x); if not distance or d < distance then nearest, distance = n, d end end; return nearest and nearest.y or 104 end
function Hand:update(world, level, player, dt)
  self.tick = self.tick + 1; local oldDistance = player.x - self.x; local targetSpeed = oldDistance > 64 and config.pressureSpeed or config.baseSpeed
  self.vx = self.vx + math.max(-35 * dt, math.min(35 * dt, targetSpeed - self.vx)); self.x = self.x + self.vx * dt
  local routeY = self:routeHeight(level, self.x); self.y = self.y + (routeY - 20 - self.y) * math.min(1, dt * 4); self.phase = self.phase + self.vx * dt * 0.06
  self.fingers, self.activeContacts = {}, 0
  for i = 1, 4 do
    local rootX, rootY = self.x - 12 + i * 7, self.y + 4; local gait = self.phase + i * 1.45; local targetX = rootX + math.cos(gait) * 12 + 3; local targetY = self:routeHeight(level, targetX); local loaded = math.sin(gait) > -0.25
    if loaded then self.activeContacts, self.terrainContacts = self.activeContacts + 1, self.terrainContacts + 1 else targetY = targetY - 8 end
    local joint, reachable = solveIK(rootX, rootY, targetX, targetY, 11, 13, i % 2 == 0 and 1 or -1); if not reachable then self.failedIK = self.failedIK + 1 end
    self.fingers[i] = { root = { x = rootX, y = rootY }, joint = joint, target = { x = targetX, y = targetY }, loaded = loaded, reachable = reachable }
    if loaded and self.tick % 45 == i * 7 then local hits = world:queryRect(targetX - 2, targetY - 2, 4, 4); if hits[1] and world.materials[hits[1].material].destructible then hits[1].damage = (hits[1].damage or 0) + 1; if hits[1].damage > world.materials[hits[1].material].hardness then hits[1].destroyed = true end end end
  end
  self.routeDistance = player.x - self.x; self.directDistance = math.sqrt((player.x - self.x)^2 + ((player.y - 5) - self.y)^2); self.closingSpeed = (oldDistance - self.routeDistance) / dt
  if self.directDistance <= config.captureRadius + 12 or self.routeDistance <= 3 then self.captureReason = "palm_or_finger_proximity"; return true end
  for _, finger in ipairs(self.fingers) do if finger.loaded and math.sqrt((player.x - finger.target.x)^2 + (player.y - finger.target.y)^2) <= config.captureRadius then self.captureReason = "loaded_fingertip_contact"; return true end end
  return false
end
function Hand:debugData() return { routeDistance = self.routeDistance, directDistance = self.directDistance, closingSpeed = self.closingSpeed, activeFingerContacts = self.activeContacts, failedIK = self.failedIK, terrainContacts = self.terrainContacts, captureReason = self.captureReason } end
Hand.solveIK = solveIK
return Hand
