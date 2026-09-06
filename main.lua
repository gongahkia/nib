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
local Replay = require("game.replay")
local Serialization = require("game.serialization")
local HotReload = require("game.hot_reload")
local Scoring = require("game.scoring")

local game = { debug = false, generationDebug = false, cameraX = 0, cameraY = 0, gamepadPressed = {}, mousePressed = {}, mode = "menu", seedText = "20260906", runSerial = 0 }
local function read(name) if love.filesystem.getInfo(name) then return love.filesystem.read(name) end end
local function write(name, text) return love.filesystem.write(name, text) end

local function beginRun(seed)
  game.session = Session.new(tonumber(seed) or 1); game.currentSeed = game.session.seed; game.mode = "run"; game.fixed.accumulator, game.fixed.ticks = 0, 0; game.runSerial = game.runSerial + 1
  local id = string.format("run-%d-%d-%d", game.currentSeed, os.time(), game.runSerial); game.recorder = Replay.new(game.currentSeed, { bindings = game.profile.bindings }, id); game.recorded = false
  love.filesystem.write("hot-level.json", Serialization.encode(game.session.level))
end
local function finishRun()
  if game.recorded then return end
  local session = game.session
  for _, incident in ipairs(session.incidents) do if incident.loreId then Persistence.unlockLore(game.profile, incident.loreId) end end
  Persistence.recordSummary(game.profile, { id = game.recorder.data.id, seed = session.seed, status = session.status, ticks = session.tick, result = session.result }); Persistence.save(game.profile, write)
  love.filesystem.createDirectory("runs"); love.filesystem.write("runs/" .. game.recorder.data.id .. ".json", Serialization.encode(game.recorder.data)); game.recorded = true
end

function love.load()
  render.load(); game.profile = Persistence.load(read); game.input = Input.new(game.profile.bindings)
  game.hot = HotReload.new("hot-level.json", love.filesystem.getInfo, love.filesystem.read, function(text) return Serialization.decode(text, "level") end)
  game.fixed = Fixed.new(config.step, function()
    if game.mode ~= "run" then return end
    local snapshot; local joysticks = love.joystick.getJoysticks()
    if joysticks[1] and joysticks[1]:isGamepad() then snapshot = Input.fromGamepad(joysticks[1], game.gamepadPressed)
    else
      local mx, my = love.mouse.getPosition(); local w, h = love.graphics.getDimensions(); local scale = math.max(1, math.floor(math.min(w / 128, h / 128))); local cx, cy = (w - 128 * scale) / 2, (h - 128 * scale) / 2
      local mouseWorld; if love.mouse.isDown(1) or love.mouse.isDown(2) then mouseWorld = { x = (mx - cx) / scale + game.cameraX, y = (my - cy) / scale + game.cameraY } end; snapshot = game.input:snapshot(mouseWorld)
      if love.mouse.isDown(1) then snapshot.grapple, snapshot.grapplePressed = true, game.mousePressed[1] or false end
      if love.mouse.isDown(2) then snapshot.tool, snapshot.toolPressed = true, game.mousePressed[2] or false end
    end
    local before, oldState = game.session.status, game.session.player.state; game.session:update(snapshot); game.recorder:capture(game.session, snapshot)
    if oldState ~= game.session.player.state then local sounds = { jump_rise = "jump", air_dash = "dash", slide = game.session.player.rail and "grind" or "slide", grapple_attach = "grapple_attach", grapple_release = "grapple_release", tool_cutting = "tool_cut", recoil_impulse = "tool_impulse", stumble_recovery = "stumble" }; if sounds[game.session.player.state] then Audio.play(sounds[game.session.player.state]) end end
    local player = game.session.player
    if player.transitionReason == "landed" and player.stateTicks == 0 then Audio.play("land") end
    if player.grounded and math.abs(player.vx) > 12 and game.session.tick % math.max(5, math.floor(26 - math.abs(player.vx) / 4)) == 0 then local _, material = game.session.world:surfaceAt(player.x, player.y); Audio.play("footstep", material and material.footstepPitch or 1) end
    if player.toolEvent and player.toolEvent.destroyed then Audio.play("breakage") end
    if game.session.tick % 45 == 0 and game.session.hand.activeContacts > 0 then Audio.play("hand_step") end
    if before == "running" and game.session.status ~= "running" then Audio.play(game.session.status == "finished" and "exit" or "capture"); finishRun() end
    game.cameraX = math.max(game.session.hand.x - 8, game.session.player.x - 43); game.cameraY = game.session.player.y - 88; game.gamepadPressed, game.mousePressed = {}, {}
  end)
  local ok = pcall(Audio.load); if not ok then Audio.enabled = false end
end
function love.update(dt) game.fixed:advance(dt) end

local function drawMenu()
  render.drawContract(); palette.set(9); love.graphics.print("N  FRESH SEED", 27, 66); love.graphics.print("S  ENTER SEED", 27, 75); love.graphics.print("F2 REMAP KEYS", 27, 84); palette.set(7); love.graphics.print("SEED " .. game.seedText, 27, 95); love.graphics.print("FULL KIT ALWAYS READY", 16, 116)
  if game.remapAction then palette.set(1); love.graphics.rectangle("fill", 10, 49, 108, 25); palette.set(9); love.graphics.printf("PRESS KEY FOR\n" .. game.remapAction .. " (TAB SKIPS)", 10, 53, 108, "center") end
end
local function drawDebug(session)
  local d, h = session.player:debugData(), session.hand:debugData(); palette.set(9); love.graphics.print(d.state, 1, 1); love.graphics.print(string.format("<%s %s", d.previous, d.reason), 1, 8); love.graphics.print(string.format("v %.1f %.1f g:%s w:%d", d.velocity[1], d.velocity[2], tostring(d.grounded), d.wall), 1, 15)
  local buffers = 0; for _ in pairs(d.buffered) do buffers = buffers + 1 end; love.graphics.print(string.format("buf:%d air:%d/%d", buffers, d.resources.dash, d.resources.grapple), 1, 22); love.graphics.print(string.format("hand route %.1f close %.1f", h.routeDistance, h.closingSpeed), 1, 29); love.graphics.print(string.format("fingers %d ikfail %d contact %d", h.activeFingerContacts, h.failedIK, h.terrainContacts), 1, 36); love.graphics.print("hot " .. game.hot.status, 1, 43)
end
function love.draw()
  render.beginFrame()
  if game.mode == "menu" then drawMenu()
  else
    local s = game.session; render.drawWorld(s.world, game.cameraX, game.cameraY); render.drawLife(s.level, game.cameraX, game.cameraY, s.collected); if game.generationDebug then render.drawGenerationDebug(s.level, s.world, game.cameraX, game.cameraY) end; HandRender.draw(s.hand, game.cameraX, game.cameraY, game.debug); RunnerRender.draw(s.player, game.cameraX, game.cameraY)
    palette.set(9); love.graphics.print(string.format("%05.1f", s.tick / 60), 97, 1); love.graphics.print(string.format("S%04d C%d", s.score.style, s.score.collectibles), 78, 9); if game.debug then drawDebug(s) end
    if s.status ~= "running" then palette.set(1, 0.9); love.graphics.rectangle("fill", 13, 37, 102, 54); palette.set(9); love.graphics.printf(s.status == "finished" and "EXIT REACHED" or "TAKEN", 13, 42, 102, "center"); love.graphics.printf(string.format("TIME %d  STYLE %d\nFRAG %d  TOTAL %d\nR RETRY  N NEW", s.result.time, s.result.style, s.result.collectibles, s.result.total), 13, 55, 102, "center") end
  end
  render.endFrame()
end
function love.keypressed(key)
  if key == "escape" then if game.mode == "run" then if game.session.status == "running" then game.session.status, game.session.result = "abandoned", Scoring.result(game.session.score, game.session.tick); finishRun() end; game.mode = "menu" else love.event.quit() end; return end
  if key == "f3" then game.debug = not game.debug; return end
  if key == "f4" then game.generationDebug = not game.generationDebug; return end
  if game.remapAction then if key ~= "tab" then game.input:rebind(game.remapAction, { key }); game.profile.bindings = game.input.bindings; Persistence.save(game.profile, write) end; game.remapIndex = game.remapIndex + 1; game.remapAction = game.remapActions[game.remapIndex]; return end
  if key == "f2" then game.remapActions = {}; for action in pairs(game.input.bindings) do game.remapActions[#game.remapActions + 1] = action end; table.sort(game.remapActions); game.remapIndex, game.remapAction = 1, game.remapActions[1]; return end
  if key == "f5" and game.mode == "run" then local loaded, level = game.hot:poll(true); if loaded then local old = game.session; game.session = Session.fromLevel(level); game.session.player.x, game.session.player.y, game.session.player.vx, game.session.player.vy = old.player.x, old.player.y, old.player.vx, old.player.vy end; return end
  if game.mode == "menu" then if key == "n" then beginRun((os.time() * 1103515245 + love.timer.getTime() * 1000) % 2147483647) elseif key == "s" or key == "return" then beginRun(game.seedText) elseif key == "backspace" then game.seedText = game.seedText:sub(1, -2) elseif key:match("^%d$") then game.seedText = (game.seedText .. key):sub(-10) end
  elseif game.session.status ~= "running" then if key == "r" then beginRun(game.currentSeed) elseif key == "n" then beginRun(os.time() % 2147483647) end
  else game.input:keypressed(key) end
end
function love.keyreleased(key) if game.input then game.input:keyreleased(key) end end
function love.gamepadpressed(_, button) game.gamepadPressed[button] = true end
function love.mousepressed(_, _, button) game.mousePressed[button] = true end
