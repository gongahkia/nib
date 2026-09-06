local config = require("game.config")
local Hash = require("game.hash")
local Json = require("game.json")
local Session = require("game.session")
local Replay = {}; Replay.__index = Replay
local inputKeys = { "abort", "moveX", "moveY", "aimDirX", "aimDirY", "aimX", "aimY", "mouseAim", "jump", "jumpPressed", "dash", "dashPressed", "slide", "slidePressed", "dive", "divePressed", "grapple", "grapplePressed", "tool", "toolPressed", "pull", "pullPressed" }
local function inputCopy(input) local out = {}; for _, key in ipairs(inputKeys) do if input[key] ~= nil and input[key] ~= false then out[key] = input[key] end end; return out end
local function statePayload(session)
  local p, h, terrain, enemies, objects, fingers = session.player, session.hand, {}, {}, {}, {}
  for _, solid in ipairs(session.world.solids) do if solid.destroyed or solid.damage then terrain[#terrain + 1] = { solid.id or 0, solid.destroyed or false, solid.damage or 0 } end end
  for i, enemy in ipairs(session.world.enemies) do enemies[i] = enemy.alive end
  for i, object in ipairs(session.world.objects) do objects[i] = { object.x, object.y, object.vx, object.vy } end
  for i, finger in ipairs(h.fingers) do fingers[i] = { finger.joint.x, finger.joint.y, finger.target.x, finger.target.y, finger.loaded } end
  return { tick = session.tick, status = session.status, player = { x = p.x, y = p.y, vx = p.vx, vy = p.vy, state = p.state, dash = p.dashes, grapple = p.grapples }, hand = { x = h.x, y = h.y, phase = h.phase, fingers = fingers }, terrain = terrain, enemies = enemies, objects = objects, collected = session.collected }
end
function Replay.stateHash(session) return Hash.fnv1a(Json.encode(statePayload(session))) end
function Replay.new(seed, settings, id)
  return setmetatable({ data = { schemaVersion = 1, kind = "palimpsest-replay", id = id or string.format("run-%d-%d", seed, os.time()), seed = seed, build = config.version, fixedStep = config.step, settings = settings or {}, frames = {}, events = {}, integrity = { algorithm = "fnv1a32", finalHash = "" } } }, Replay)
end
function Replay:capture(session, input)
  local p, h = session.player, session.hand; local latest = session.incidents[#session.incidents]; if latest and latest.tick ~= session.tick then latest = nil end
  local terrainChanges = {}; for _, solid in ipairs(session.world.solids) do if solid.destroyed then terrainChanges[#terrainChanges + 1] = solid.id end end
  local collisions = {}; for _, contact in ipairs(p.contacts) do collisions[#collisions + 1] = { axis = contact.axis, sign = contact.sign, solidId = contact.terrain and contact.terrain.id } end
  local rejected = {}; for _, item in ipairs(p.rejected) do rejected[#rejected + 1] = { tick = item.tick, input = item.input, reason = item.reason } end
  local toolEvent = p.toolEvent and { kind = p.toolEvent.kind, destroyed = p.toolEvent.destroyed, solidId = p.toolEvent.terrain and p.toolEvent.terrain.id } or nil
  local frame = { tick = session.tick, input = inputCopy(input), position = { p.x, p.y }, velocity = { p.vx, p.vy }, state = p.state, resources = { dash = p.dashes, grapple = p.grapples }, collisions = collisions, rejectedInputs = rejected, stumble = p.state == "stumble_recovery", collectibleEvent = latest, toolEvent = toolEvent, terrainChanges = terrainChanges, hand = { routeDistance = h.routeDistance, directDistance = h.directDistance, closingSpeed = h.closingSpeed }, hash = Replay.stateHash(session) }
  self.data.frames[#self.data.frames + 1] = frame; self.data.integrity.finalHash = frame.hash; self.data.status = session.status; self.data.result = session.result
end
function Replay.verify(data, level)
  if data.build ~= config.version or math.abs(data.fixedStep - config.step) > 1e-12 then return { valid = false, reason = "incompatible build or fixed step", expectedBuild = config.version } end
  local session = level and Session.fromLevel(level) or Session.new(data.seed)
  for index, frame in ipairs(data.frames) do
    session:update(frame.input or {}); local hash = Replay.stateHash(session)
    if hash ~= frame.hash then return { valid = false, reason = "state divergence", tick = frame.tick, frame = index, expected = frame.hash, actual = hash } end
  end
  return { valid = data.integrity.finalHash == (data.frames[#data.frames] and data.frames[#data.frames].hash or ""), ticks = #data.frames, finalHash = data.integrity.finalHash, status = session.status }
end
return Replay
