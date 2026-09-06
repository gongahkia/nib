local render = require("game.render")
local palette = require("game.palette")
local config = require("game.config")
local Fixed = require("game.fixed_step")
local Input = require("game.input")
local Player = require("game.player")
local World = require("game.world")
local RunnerRender = require("game.runner_render")
local Audio = require("game.audio")

local game = { debug = true, cameraX = 0, cameraY = 0, gamepadPressed = {} }

function love.load()
  render.load()
  game.world, game.player, game.input = World.playground(), Player.new(14, 90), Input.new()
  game.fixed = Fixed.new(config.step, function(dt)
    local snapshot
    local joysticks = love.joystick.getJoysticks()
    if joysticks[1] and joysticks[1]:isGamepad() then snapshot = Input.fromGamepad(joysticks[1], game.gamepadPressed)
    else
      local mx, my = love.mouse.getPosition()
      local w, h = love.graphics.getDimensions()
      local scale = math.max(1, math.floor(math.min(w / 128, h / 128)))
      local cx, cy = (w - 128 * scale) / 2, (h - 128 * scale) / 2
      local mouseWorld
      if love.mouse.isDown(1) or love.mouse.isDown(2) then mouseWorld = { x = (mx - cx) / scale + game.cameraX, y = (my - cy) / scale + game.cameraY } end
      snapshot = game.input:snapshot(mouseWorld)
    end
    local previous = game.player.state
    game.player:update(game.world, snapshot, dt)
    game.world:updateObjects(dt)
    if previous ~= game.player.state then
      local sounds = { jump_rise = "jump", air_dash = "dash", slide = "slide", grapple_attach = "grapple_attach", grapple_release = "grapple_release", stumble_recovery = "stumble" }
      if sounds[game.player.state] then Audio.play(sounds[game.player.state]) end
    end
    game.cameraX = math.max(0, game.player.x - 43)
    game.gamepadPressed = {}
  end)
  local ok = pcall(Audio.load); if not ok then Audio.enabled = false end
end

function love.update(dt) game.fixed:advance(dt) end

function love.draw()
  render.beginFrame()
  render.drawWorld(game.world, game.cameraX, game.cameraY)
  RunnerRender.draw(game.player, game.cameraX, game.cameraY)
  if game.debug then
    local d = game.player:debugData(); palette.set(9)
    love.graphics.print(d.state, 1, 1)
    love.graphics.print(string.format("<%s %s", d.previous, d.reason), 1, 8)
    love.graphics.print(string.format("v %.1f %.1f", d.velocity[1], d.velocity[2]), 1, 15)
    love.graphics.print(string.format("g:%s w:%d a:%d/%d", tostring(d.grounded), d.wall, d.resources.dash, d.resources.grapple), 1, 22)
    local buffered = {}; for k, v in pairs(d.buffered) do buffered[#buffered + 1] = k .. ":" .. v end
    love.graphics.print("buf " .. table.concat(buffered, ","), 1, 29)
  end
  render.endFrame()
end

function love.keypressed(key)
  if key == "escape" then love.event.quit() end
  if key == "f3" then game.debug = not game.debug end
  game.input:keypressed(key)
end

function love.keyreleased(key) game.input:keyreleased(key) end
function love.gamepadpressed(_, button) game.gamepadPressed[button] = true end
