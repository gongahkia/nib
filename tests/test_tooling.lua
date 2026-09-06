local H = require("tests.harness")
local Generator = require("game.generator")
local Serialization = require("game.serialization")
local Schema = require("game.schema")
local HotReload = require("game.hot_reload")
local Session = require("game.session")
local Replay = require("game.replay")
local Overlay = require("game.overlay")
local Obstacle = require("game.obstacle")
local Json = require("game.json")

H.test("level serialization load-save-load preserves meaning", function()
  local level = assert(Generator.generate(8008)); local text = Serialization.encode(level); local loaded = assert(Serialization.decode(text, "level")); local second = Serialization.encode(loaded)
  H.eq(second, text)
end)

H.test("schema validation reports useful JSON path", function()
  local level = assert(Generator.generate(4)); level.solids[2].x = "wrong"; local ok, err = Schema.validateLevel(level)
  H.ok(not ok); H.eq(err.path, "$.solids[2].x")
end)

H.test("hot reload applies only a newly valid version", function()
  local modified, content = 1, Serialization.encode(assert(Generator.generate(5)))
  local hot = HotReload.new("level.json", function() return { modtime = modified } end, function() return content end, function(text) return Serialization.decode(text, "level") end)
  H.ok(hot:poll()); H.eq(hot.value.seed, 5); H.ok(not hot:poll())
  modified, content = 2, "not-json"; H.ok(not hot:poll()); H.ok(hot.status:match("error")); H.eq(hot.value.seed, 5)
end)

local function recordedRun(seed, ticks)
  local session, replay = Session.new(seed), Replay.new(seed, {}, "fixture-" .. seed)
  for _ = 1, ticks do local input = { moveX = 1, aimDirX = 1, aimDirY = 0 }; session:update(input); replay:capture(session, input); if session.status ~= "running" then break end end
  return replay.data
end

H.test("replay verification detects equality and divergence", function()
  local replay = recordedRun(65, 70); local verified = Replay.verify(replay); H.ok(verified.valid)
  replay.frames[math.min(10, #replay.frames)].hash = "00000000"; local divergent = Replay.verify(replay); H.ok(not divergent.valid); H.eq(divergent.reason, "state divergence")
end)

H.test("abandoned runs replay as an explicit deterministic input", function()
  local session, replay = Session.new(66), Replay.new(66, {}, "abandon-fixture")
  local input = { abort = true }; session:update(input); replay:capture(session, input)
  local verified = Replay.verify(replay.data); H.ok(verified.valid); H.eq(verified.status, "abandoned")
end)

H.test("trajectory overlay exposes SVG and structured events", function()
  local replay = recordedRun(16, 35); local data, svg = Overlay.build(assert(Generator.generate(16)), replay)
  H.eq(data.kind, "trajectory-overlay"); H.eq(#data.trajectory, #replay.frames); H.ok(svg:match("<svg")); H.ok(svg:match("data%-solid"))
end)

H.test("bounded obstacle revision improves measured tolerance", function()
  local level = assert(Generator.generate(33)); local target = level.solids[3]
  local replay = { id = "synthetic-obstacle", frames = { { tick = 1, position = { target.x + target.w - 8, target.y }, velocity = { 60, 0 }, state = "fall", collisions = {}, rejectedInputs = {}, hand = { routeDistance = 30, closingSpeed = 2 } } } }
  local report, winner = Obstacle.ab(level, replay, 1); H.ok(report, winner); H.ok(report.after.landingMargin > report.before.landingMargin); H.ok(report.after.completionRate > report.before.completionRate); H.ok(report.constraints.reachability); H.eq(winner.solids[3].id, target.id)
end)

H.test("CLI emits versioned JSON contracts", function()
  local path = "/tmp/palimpsest-cli-test-level.json"; local pipe = assert(io.popen("./bin/palimpsest generate 91 " .. path)); local output = pipe:read("*a"); local ok = pipe:close(); H.ok(ok)
  local payload = Json.decode(output); H.eq(payload.outputSchemaVersion, 1); H.eq(payload.command, "generate")
  local validate = assert(io.popen("./bin/palimpsest validate " .. path)); local checked = Json.decode(validate:read("*a")); H.ok(validate:close()); H.ok(checked.valid)
  os.remove(path)
end)
