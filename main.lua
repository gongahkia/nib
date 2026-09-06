local render = require("game.render")
local palette = require("game.palette")
local config = require("game.config")
local Fixed = require("game.fixed_step")
local Input = require("game.input")
local Session = require("game.session")
local RunnerRender = require("game.runner_render")
local HandRender = require("game.hand_render")
local Audio = require("game.audio")
local Persistence = require("game.persistence")

local game = { debug = false, cameraX = 0, cameraY = 0, gamepadPressed = {}, mode = "menu", seedText = "20260906" }
local function read(name) if love.filesystem.getInfo(name) then return love.filesystem.read(name) end end
local function write(name, text) return love.filesystem.write(name, text) end

local function beginRun(seed)
  game.session = Session.new(tonumber(seed) or 1); game.currentSeed = game.session.seed; game.mode = "run"; game.fixed.accumulator, game.fixed.ticks = 0, 0
end
local function finishRun()
  local session = game.session
  for _, incident in ipairs(session.incidents) do if incident.loreId then Persistence.unlockLore(game.profile, incident.loreId) end end
  Persistence.recordSummary(game.profile, { seed = session.seed, status = session.status, ticks = session.tick, result = session.result }); Persistence.save(game.profile, write)
end

function love.load()
  render.load(); game.input = Input.new(); game.profile = Persistence.load(read)
  game.fixed = Fixed.new(config.step, function()
    if game.mode ~= "run" then return end
    local snapshot; local joysticks = love.joystick.getJoysticks()
    if joysticks[1] and joysticks[1]:isGamepad() then snapshot = Input.fromGamepad(joysticks[1], game.gamepadPressed)
    else
      local mx, my = love.mouse.getPosition(); local w, h = love.graphics.getDimensions(); local scale = math.max(1, math.floor(math.min(w / 128, h / 128))); local cx, cy = (w - 128 * scale) / 2, (h - 128 * scale) / 2
      local mouseWorld; if love.mouse.isDown(1) or love.mouse.isDown(2) then mouseWorld = { x = (mx - cx) / scale + game.cameraX, y = (my - cy) / scale + game.cameraY } end; snapshot = game.input:snapshot(mouseWorld)
    end
    local before = game.session.status; game.session:update(snapshot)
    if before == "running" and game.session.status ~= "running" then Audio.play(game.session.status == "finished" and "exit" or "capture"); finishRun() end
    game.cameraX = math.max(game.session.hand.x - 8, game.session.player.x - 43); game.cameraY = game.session.player.y - 88; game.gamepadPressed = {}
  end)
  local ok = pcall(Audio.load); if not ok then Audio.enabled = false end
end
function love.update(dt) game.fixed:advance(dt) end

local function drawMenu()
  render.drawContract(); palette.set(9); love.graphics.print("N  FRESH SEED", 27, 68); love.graphics.print("S  ENTER SEED", 27, 77); palette.set(7); love.graphics.print("SEED " .. game.seedText, 27, 88); love.graphics.print("FULL KIT ALWAYS READY", 16, 113)
end
local function drawDebug(session)
  local d, h = session.player:debugData(), session.hand:debugData(); palette.set(9); love.graphics.print(d.state, 1, 1); love.graphics.print(string.format("<%s %s", d.previous, d.reason), 1, 8); love.graphics.print(string.format("v %.1f %.1f g:%s w:%d", d.velocity[1], d.velocity[2], tostring(d.grounded), d.wall), 1, 15)
  local buffers = 0; for _ in pairs(d.buffered) do buffers = buffers + 1 end; love.graphics.print(string.format("buf:%d air:%d/%d", buffers, d.resources.dash, d.resources.grapple), 1, 22); love.graphics.print(string.format("hand route %.1f close %.1f", h.routeDistance, h.closingSpeed), 1, 29); love.graphics.print(string.format("fingers %d ikfail %d contact %d", h.activeFingerContacts, h.failedIK, h.terrainContacts), 1, 36)
end
function love.draw()
  render.beginFrame()
  if game.mode == "menu" then drawMenu()
  else
    local s = game.session; render.drawWorld(s.world, game.cameraX, game.cameraY); render.drawLife(s.level, game.cameraX, game.cameraY, s.collected); HandRender.draw(s.hand, game.cameraX, game.cameraY, game.debug); RunnerRender.draw(s.player, game.cameraX, game.cameraY)
    palette.set(9); love.graphics.print(string.format("%05.1f", s.tick / 60), 97, 1); love.graphics.print(string.format("S%04d C%d", s.score.style, s.score.collectibles), 78, 9); if game.debug then drawDebug(s) end
    if s.status ~= "running" then palette.set(1, 0.9); love.graphics.rectangle("fill", 13, 37, 102, 54); palette.set(9); love.graphics.printf(s.status == "finished" and "EXIT REACHED" or "TAKEN", 13, 42, 102, "center"); love.graphics.printf(string.format("TIME %d  STYLE %d\nFRAG %d  TOTAL %d\nR RETRY  N NEW", s.result.time, s.result.style, s.result.collectibles, s.result.total), 13, 55, 102, "center") end
  end
  render.endFrame()
end
function love.keypressed(key)
  if key == "escape" then if game.mode == "run" then game.mode = "menu" else love.event.quit() end; return end
  if key == "f3" then game.debug = not game.debug; return end
  if game.mode == "menu" then if key == "n" then beginRun((os.time() * 1103515245 + love.timer.getTime() * 1000) % 2147483647) elseif key == "s" or key == "return" then beginRun(game.seedText) elseif key == "backspace" then game.seedText = game.seedText:sub(1, -2) elseif key:match("^%d$") then game.seedText = (game.seedText .. key):sub(-10) end
  elseif game.session.status ~= "running" then if key == "r" then beginRun(game.currentSeed) elseif key == "n" then beginRun(os.time() % 2147483647) end
  else game.input:keypressed(key) end
end
function love.keyreleased(key) if game.input then game.input:keyreleased(key) end end
function love.gamepadpressed(_, button) game.gamepadPressed[button] = true end
