local config = require("game.config").player
local Player = {}
Player.__index = Player

local function approach(value, target, amount)
  if value < target then return math.min(value + amount, target) end
  return math.max(value - amount, target)
end

function Player.new(x, y)
  return setmetatable({
    x = x or 14, y = y or 80, vx = 0, vy = 0, w = config.width, h = config.height,
    state = "idle", previousState = "idle", transitionReason = "spawn", stateTicks = 0, tick = 0,
    grounded = false, wall = 0, coyote = 0, buffers = {}, dashes = config.dashResources,
    grapples = config.grappleResources, grapple = nil, facing = 1, contacts = {}, rejected = {},
    footstepClock = 0, finished = false, captured = false,
  }, Player)
end

function Player:setState(state, reason)
  if self.state ~= state then
    self.previousState, self.state = self.state, state
    self.transitionReason, self.stateTicks = reason or "", 0
  end
end

function Player:buffer(input)
  local names = { "jump", "dash", "slide", "dive", "grapple", "tool", "pull" }
  for _, name in ipairs(names) do
    if input[name .. "Pressed"] then self.buffers[name] = config.bufferTicks end
    if self.buffers[name] then
      self.buffers[name] = self.buffers[name] - 1
      if self.buffers[name] <= 0 then self.buffers[name] = nil; self.rejected[#self.rejected + 1] = { tick = self.tick, input = name } end
    end
  end
end

function Player:consume(name)
  if self.buffers[name] then self.buffers[name] = nil; return true end
  return false
end

function Player:aim(input)
  if input.mouseAim then
    local dx, dy = input.aimX - self.x, input.aimY - self.y
    local length = math.max(0.001, math.sqrt(dx * dx + dy * dy))
    return dx / length, dy / length
  end
  return input.aimDirX or self.facing, input.aimDirY or 0
end

local function moveAxis(self, world, amount, axis)
  if amount == 0 then return false end
  local sign = amount > 0 and 1 or -1
  local remaining, hit = math.abs(amount), false
  while remaining > 0 do
    local delta = math.min(1, remaining) * sign
    local nx, ny = self.x, self.y
    if axis == "x" then nx = nx + delta else ny = ny + delta end
    local contacts = world:queryRect(nx - self.w / 2, ny - self.h, self.w, self.h)
    if #contacts > 0 then hit = true; self.contacts[#self.contacts + 1] = { axis = axis, sign = sign, terrain = contacts[1] }; break end
    self.x, self.y = nx, ny
    remaining = remaining - math.abs(delta)
  end
  return hit
end

function Player:update(world, input, dt)
  self.tick, self.stateTicks, self.contacts, self.rejected = self.tick + 1, self.stateTicks + 1, {}, {}
  self:buffer(input)
  local wasGrounded = self.grounded
  self.grounded = #world:queryRect(self.x - self.w / 2, self.y + 0.1, self.w, 1) > 0
  self.wall = 0
  if #world:queryRect(self.x - self.w / 2 - 0.2, self.y - self.h + 1, 0.3, self.h - 2) > 0 then self.wall = -1 end
  if #world:queryRect(self.x + self.w / 2 - 0.1, self.y - self.h + 1, 0.3, self.h - 2) > 0 then self.wall = 1 end
  if self.grounded then
    self.coyote, self.dashes, self.grapples = config.coyoteTicks, config.dashResources, config.grappleResources
  elseif self.wall ~= 0 then
    self.coyote, self.dashes, self.grapples = config.coyoteTicks, config.dashResources, config.grappleResources
  else self.coyote = math.max(0, self.coyote - 1) end

  local mx = input.moveX or 0
  if math.abs(mx) > 0.1 then self.facing = mx > 0 and 1 or -1 end

  if self.state == "stumble_recovery" then
    self.vx = approach(self.vx, 0, config.friction * 0.35 * dt)
    if self.stateTicks >= config.stumbleTicks then self:setState("idle", "recovered") end
  elseif self.state == "air_dash" then
    if self.stateTicks >= config.dashTicks then self:setState("fall", "dash expired") end
  elseif self.state == "slide" then
    self.vx = approach(self.vx, 0, config.slideFriction * dt)
    if self.stateTicks >= config.slideTicks or not input.slide then self:setState("acceleration_run", "slide released") end
  else
    local target = mx * config.maxRunSpeed
    local accel = self.grounded and (mx ~= 0 and (self.vx * mx < 0 and config.turnAcceleration or config.runAcceleration) or config.friction) or config.airAcceleration
    self.vx = approach(self.vx, target, accel * dt)
    if not self.grounded then self.vy = math.min(config.maxFallSpeed, self.vy + config.gravity * dt) end

    if self:consume("jump") and self.coyote > 0 then
      if self.wall ~= 0 and not self.grounded then self.vx = -self.wall * config.wallJumpX; self:setState("jump_rise", "wall jump")
      else self:setState("jump_rise", "buffered jump") end
      self.vy, self.coyote = -config.jumpSpeed, 0
    elseif self:consume("dash") and self.dashes > 0 then
      local ax, ay = self:aim(input); self.vx, self.vy = ax * config.dashSpeed, ay * config.dashSpeed
      self.dashes = self.dashes - 1; self:setState("air_dash", "dash input")
    elseif self:consume("slide") and self.grounded and math.abs(self.vx) > 15 then
      self.vx = self.vx + self.facing * 8; self:setState("slide", "slide input")
    elseif self:consume("dive") and not self.grounded then
      self.vx, self.vy = self.facing * math.max(config.diveX, math.abs(self.vx)), config.diveY
      self:setState("dive", "dive input")
    elseif self:consume("grapple") and self.grapples > 0 then
      local ax, ay = self:aim(input)
      local anchor = world:nearestAnchor(self.x, self.y - self.h / 2, ax, ay, config.grappleRange)
      if anchor then self.grapple, self.grapples = anchor, self.grapples - 1; self:setState("grapple_attach", "anchor acquired")
      else self.rejected[#self.rejected + 1] = { tick = self.tick, input = "grapple", reason = "no anchor" } end
    end

    if self.grapple then
      if input.grapple then
        local dx, dy = self.grapple.x - self.x, self.grapple.y - (self.y - self.h / 2)
        local length = math.max(1, math.sqrt(dx * dx + dy * dy))
        self.vx = self.vx + dx / length * config.grapplePull * dt
        self.vy = self.vy + dy / length * config.grapplePull * dt
        local speed = math.sqrt(self.vx * self.vx + self.vy * self.vy)
        if speed > config.grappleMaxSpeed then self.vx, self.vy = self.vx / speed * config.grappleMaxSpeed, self.vy / speed * config.grappleMaxSpeed end
        self:setState(self.stateTicks < 3 and "grapple_attach" or "grapple_travel", "grapple loaded")
      else
        self.vx = self.vx + self.facing * config.grappleReleaseImpulse
        self.grapple = nil; self:setState("grapple_release", "grapple released")
      end
    end
  end

  local hitX = moveAxis(self, world, self.vx * dt, "x")
  if hitX then
    if self.grounded and self.vy >= 0 and math.abs(self.vx) > 22 then
      -- Low-obstacle corner correction doubles as a momentum vault.
      local oldY = self.y
      for lift = 1, config.cornerPixels + 3 do
        if #world:queryRect(self.x + self.facing, oldY - self.h - lift, self.w, self.h) == 0 then
          self.y, self.vx, self.vy = oldY - lift, self.facing * config.vaultSpeedX, -config.vaultSpeedY
          self:setState("vault_mantle", "corner vault"); hitX = false; break
        end
      end
    end
    if hitX then
      if math.abs(self.vx) > 75 and self.state ~= "air_dash" then self:setState("stumble_recovery", "hard wall impact") end
      self.vx = 0
    end
  end
  local hitY = moveAxis(self, world, self.vy * dt, "y")
  if hitY then
    if self.vy > 0 then
      if self.state == "dive" then self.vx = self.vx * 1.15 end
      self.grounded = true; self.dashes, self.grapples = config.dashResources, config.grappleResources
    end
    self.vy = 0
  end

  if not wasGrounded and self.grounded and self.state ~= "slide" then self:setState(math.abs(self.vx) > config.highSpeed and "high_speed_run" or "idle", "landed") end
  if self.wall ~= 0 and not self.grounded and self.vy > 0 and self.state ~= "air_dash" then self.vy = math.min(self.vy, config.wallSlideSpeed); self:setState("wall_traversal", "wall contact")
  elseif not self.grounded and self.state ~= "air_dash" and self.state ~= "dive" and not self.grapple then self:setState(self.vy < 0 and "jump_rise" or "fall", "airborne")
  elseif self.grounded and self.state ~= "slide" and self.state ~= "stumble_recovery" then
    if mx ~= 0 and self.vx * mx < -2 then self:setState("turn_skid", "reversed input")
    elseif math.abs(self.vx) >= config.highSpeed then self:setState("high_speed_run", "speed threshold")
    elseif math.abs(self.vx) > 2 then self:setState("acceleration_run", "moving")
    else self:setState("idle", "stopped") end
  end
end

function Player:debugData()
  return {
    state = self.state, previous = self.previousState, reason = self.transitionReason,
    velocity = { self.vx, self.vy }, grounded = self.grounded, wall = self.wall,
    buffered = self.buffers, resources = { dash = self.dashes, grapple = self.grapples },
  }
end

return Player
