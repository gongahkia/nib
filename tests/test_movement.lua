local H = require("tests.harness")
local PRNG = require("game.prng")
local Fixed = require("game.fixed_step")
local Player = require("game.player")
local World = require("game.world")
local rootConfig = require("game.config")
local config = rootConfig.player

local function blank() return { moveX = 0, moveY = 0, aimDirX = 1, aimDirY = 0 } end
local function settle(player, world)
  for _ = 1, 60 do player:update(world, blank(), rootConfig.step) end
end

H.test("project PRNG is repeatable", function()
  local a, b = PRNG.new(42), PRNG.new(42)
  for _ = 1, 100 do H.eq(a:nextU32(), b:nextU32()) end
end)

H.test("fixed step ignores frame partition", function()
  local a, b = 0, 0
  local fa = Fixed.new(1 / 60, function(dt) a = a + dt end)
  local fb = Fixed.new(1 / 60, function(dt) b = b + dt end)
  fa:advance(1 / 30); fb:advance(1 / 60); fb:advance(1 / 60)
  H.near(a, b, 1e-9); H.eq(fa.ticks, 2)
end)

H.test("buffered jump fires on landing", function()
  local world, p = World.playground(), Player.new(20, 102)
  p.vy = 24
  p:update(world, { moveX = 0, jumpPressed = true, aimDirX = 1, aimDirY = 0 }, rootConfig.step)
  for _ = 1, 8 do p:update(world, blank(), rootConfig.step) end
  H.ok(p.vy < 0, "buffered jump should launch after contact")
end)

H.test("coyote jump is accepted", function()
  local world, p = World.playground(), Player.new(20, 104)
  settle(p, world); p.x, p.coyote, p.grounded = 300, config.coyoteTicks, false
  p:update(world, { moveX = 1, jumpPressed = true, aimDirX = 1, aimDirY = 0 }, rootConfig.step)
  H.eq(p.state, "jump_rise"); H.ok(p.vy < 0)
end)

H.test("dash resource refreshes on wall contact", function()
  local world, p = World.playground(), Player.new(121.7, 80)
  p.dashes = 0; p:update(world, blank(), rootConfig.step)
  H.eq(p.dashes, config.dashResources)
end)

H.test("eight direction grapple selects marked anchor", function()
  local world, p = World.playground(), Player.new(60, 83)
  p:update(world, { moveX = 0, grapplePressed = true, grapple = true, aimDirX = 0, aimDirY = -1 }, rootConfig.step)
  H.ok(p.grapple ~= nil); H.eq(p.state, "grapple_attach")
end)

H.test("all required animation states are data", function()
  local states = require("game.animation").states
  local required = { "idle", "acceleration_run", "high_speed_run", "turn_skid", "jump_rise", "fall", "wall_traversal", "air_dash", "slide", "dive", "vault_mantle", "grapple_aim", "grapple_attach", "grapple_travel", "grapple_release", "tool_cutting", "tool_push_pull", "recoil_impulse", "enemy_impact", "stumble_recovery", "hand_capture", "exit_finish" }
  for _, state in ipairs(required) do H.ok(states[state], "missing " .. state) end
end)
