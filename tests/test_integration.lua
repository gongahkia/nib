local H = require("tests.harness")
local Input = require("game.input")
local Player = require("game.player")
local World = require("game.world")
local Session = require("game.session")
local Generator = require("game.generator")
local config = require("game.config")

H.test("keyboard bindings are fully replaceable", function()
  local input = Input.new({ jump = { "space" }, left = { "a" } }); H.ok(input:rebind("jump", { "k" })); input:keypressed("k"); H.ok(input:snapshot().jumpPressed); local ok = input:rebind("missing", { "q" }); H.ok(not ok)
end)

H.test("free mouse aim and eight-direction aim remain distinct", function()
  local p = Player.new(10, 20); local mx, my = p:aim({ mouseAim = true, aimX = 10, aimY = 0 }); H.near(mx, 0, 1e-9); H.near(my, -1, 1e-9)
  local kx, ky = p:aim({ aimDirX = 0.70710678, aimDirY = -0.70710678 }); H.near(kx, 0.70710678, 1e-8); H.near(ky, -0.70710678, 1e-8)
end)

H.test("gamepad mapping works without hardware", function()
  local fake = { getGamepadAxis = function(_, axis) return ({ leftx = 0.8, lefty = 0, rightx = -0.8, righty = -0.8 })[axis] end, isGamepadDown = function(_, button) return button == "a" end }
  local snapshot = Input.fromGamepad(fake, { a = true }); H.ok(snapshot.moveX > 0); H.ok(snapshot.aimDirX < 0 and snapshot.aimDirY < 0); H.ok(snapshot.jump and snapshot.jumpPressed)
end)

H.test("corner correction vaults a short obstruction", function()
  local world = World.new({ solids = { { x = -20, y = 80, w = 50, h = 20, material = "stone" }, { x = 30, y = 77, w = 10, h = 23, material = "brick" } } }); local p = Player.new(27, 80); p.grounded, p.vx = true, 50
  p:update(world, { moveX = 1, aimDirX = 1, aimDirY = 0 }, config.step); H.ok(p.y < 80 or p.state == "vault_mantle")
end)

H.test("seed retry regenerates identical simulation setup", function()
  local a, b = Session.new(9981), Session.new(9981); H.eq(a.level.exit.x, b.level.exit.x); H.eq(a.hand.phase, b.hand.phase); H.eq(#a.level.collectibles, #b.level.collectibles)
end)

H.test("real exit requires geometric arrival and completes", function()
  local s = Session.new(44); s.hand.x = -10000; s.player.x, s.player.y = s.level.exit.x - 2, s.level.exit.y
  s:update({ moveX = 0, aimDirX = 1, aimDirY = 0 }); H.eq(s.status, "finished"); H.ok(s.result.total >= 0)
end)

H.test("void is a nonfatal stumble with momentum loss", function()
  local s = Session.new(45); s.hand.x = -10000; s.player.y, s.player.vx = s.maxRouteY + 100, 60
  s:update({ moveX = 0, aimDirX = 1, aimDirY = 0 }); H.eq(s.status, "running"); H.eq(s.player.state, "stumble_recovery"); H.ok(math.abs(s.player.vx) < 60)
end)
