local H = require("tests.harness")
local Player = require("game.player")
local World = require("game.world")
local Tool = require("game.tool")
local config = require("game.config")

H.test("tool damages and cuts material by hardness", function()
  local world = World.new({ solids = { { x = 20, y = 80, w = 6, h = 8, material = "brick" } } })
  local p = Player.new(10, 84)
  for _ = 1, 3 do Tool.use(p, world, 1, 0, "cut") end
  H.ok(world.solids[1].destroyed)
end)

H.test("tool manipulates objects and produces recoil", function()
  local world = World.new({ objects = { { x = 28, y = 80, w = 6, h = 6, vx = 0, vy = 0 } } })
  local p = Player.new(10, 84)
  local result = Tool.use(p, world, 1, 0, "manipulate")
  H.eq(result.kind, "manipulate"); H.ok(world.objects[1].vx > 0); H.ok(p.vx < 0)
end)

H.test("elastic surface refreshes and launches", function()
  local world = World.new({ solids = { { x = 0, y = 80, w = 40, h = 10, material = "elastic" } } })
  local p = Player.new(10, 78); p.vy = 100; p.dashes = 0
  for _ = 1, 8 do p:update(world, { moveX = 0, aimDirX = 1, aimDirY = 0 }, config.step) end
  H.ok(p.vy < 0, "elastic should launch"); H.eq(p.dashes, config.player.dashResources)
end)

H.test("movement attack defeats enemy and grants speed", function()
  local world = World.new({ enemies = { { x = 10, y = 80, w = 6, h = 6, alive = true } } })
  local p = Player.new(10, 80); p.state = "air_dash"; p.vx = 40
  p:update(world, { moveX = 0, aimDirX = 1, aimDirY = 0 }, config.step)
  H.ok(not world.enemies[1].alive); H.eq(p.state, "enemy_impact"); H.ok(p.vx > 40)
end)

H.test("slope data redirects grounded speed", function()
  local world = World.new({ solids = { { x = 0, y = 80, w = 40, h = 10, material = "brick", slope = -8 } } })
  local p = Player.new(10, 80); p.grounded = true; local before = p.vx
  p:update(world, { moveX = 0, aimDirX = 1, aimDirY = 0 }, config.step)
  H.ok(p.vx > before)
end)
