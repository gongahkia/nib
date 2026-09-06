package.path = "./?.lua;./?/init.lua;" .. package.path
local Json = require("game.json")
local Serialization = require("game.serialization")
local Schema = require("game.schema")
local Generator = require("game.generator")
local Reach = require("game.reachability")
local Storage = require("game.storage")
local Replay = require("game.replay")
local Overlay = require("game.overlay")
local Obstacle = require("game.obstacle")

local function emit(payload) payload.outputSchemaVersion = 1; io.write(Json.encode(payload), "\n") end
local function die(message, code, details) io.stderr:write(Json.encode({ outputSchemaVersion = 1, error = message, details = details }), "\n"); os.exit(code or 2) end
local function read(path) local text = Storage.read(path); if not text then die("cannot read file", 3, { path = path }) end; return text end
local function write(path, text) local ok, err = Storage.write(path, text); if not ok then die("cannot write file", 4, { path = path, reason = err }) end end
local function replayPath(id) if id:find("/") or id:match("%.json$") then return id end; return Storage.runPath(id) end
local function loadLevel(path) local level, err = Serialization.decode(read(path), "level"); if not level then die("invalid level", 5, err) end; return level end
local function loadReplay(id) local replay, err = Serialization.decode(read(replayPath(id)), "replay"); if not replay then die("invalid replay", 5, err) end; return replay end

local command = arg[1] or "help"
if command == "help" then
  emit({ command = "help", commands = {
    "generate <seed> <level.json>", "validate <level.json>", "inspect <level.json>", "reachability <level.json>",
    "audit <count> [first-seed]", "runs", "summarize <run-id|replay.json>", "replay <run-id|replay.json>",
    "overlay <run-id|replay.json> <overlay.svg>", "problems <run-id|replay.json> <tick>",
    "ab <run-id|replay.json> <tick> <output-prefix>", "delete-run <run-id>",
  } })
elseif command == "generate" then
  local seed, path = tonumber(arg[2]), arg[3]; if not seed or not path then die("usage: generate <seed> <level.json>") end
  local level, err = Generator.generate(seed); if not level then die(err, 6) end; write(path, Serialization.encode(level)); emit({ command = command, seed = seed, path = path, metrics = level.metrics, validation = { valid = level.validation.valid, unreachable = level.validation.unreachable } })
elseif command == "validate" then
  local level = loadLevel(arg[2] or ""); local valid, err = Schema.validateLevel(level); if not valid then die("schema validation failed", 5, err) end; local reach = Reach.validate(level); if not reach.valid then die("reachability validation failed", 7, { unreachable = reach.unreachable }) end; emit({ command = command, valid = true, collectibleCount = #level.collectibles, minimumMargin = reach.minimumMargin })
elseif command == "inspect" then
  local level = loadLevel(arg[2] or ""); emit({ command = command, seed = level.seed, decisions = level.decisions, metrics = level.metrics, warnings = level.warnings, generationAttempt = level.generationAttempt })
elseif command == "reachability" then
  local level = loadLevel(arg[2] or ""); local report = Reach.validate(level); emit({ command = command, valid = report.valid, unreachable = report.unreachable, capabilities = report.capabilities, minimumMargin = report.minimumMargin, evidence = report.evidence }); if not report.valid then os.exit(7) end
elseif command == "audit" then
  local count, first = tonumber(arg[2]), tonumber(arg[3]) or 1; if not count or count < 1 or count > 1000 then die("count must be between 1 and 1000") end
  local report = { command = command, count = count, firstSeed = first, valid = 0, failed = {}, unique = {}, routeLength = { min = math.huge, max = -math.huge }, generationMilliseconds = 0 }
  for seed = first, first + count - 1 do local level, err = Generator.generate(seed); if level and level.validation.valid then report.valid = report.valid + 1; report.unique[Json.encode(level.decisions)] = true; report.routeLength.min = math.min(report.routeLength.min, level.metrics.routeLength); report.routeLength.max = math.max(report.routeLength.max, level.metrics.routeLength); report.generationMilliseconds = report.generationMilliseconds + level.metrics.generationMilliseconds else report.failed[#report.failed + 1] = { seed = seed, reason = err } end end
  local unique = 0; for _ in pairs(report.unique) do unique = unique + 1 end; report.unique = unique; emit(report); if #report.failed > 0 then os.exit(8) end
elseif command == "runs" then
  local ids, summaries = Storage.listRuns(), {}; for _, id in ipairs(ids) do local ok, replay = pcall(Json.decode, Storage.read(Storage.runPath(id))); summaries[#summaries + 1] = ok and { id = id, seed = replay.seed, status = replay.status, ticks = #replay.frames } or { id = id, invalid = true } end; emit({ command = command, runs = summaries })
elseif command == "summarize" then
  local replay = loadReplay(arg[2] or ""); local states, stumbles, rejected = {}, 0, 0; for _, frame in ipairs(replay.frames) do states[frame.state] = (states[frame.state] or 0) + 1; if frame.stumble then stumbles = stumbles + 1 end; rejected = rejected + #(frame.rejectedInputs or {}) end; emit({ command = command, id = replay.id, seed = replay.seed, status = replay.status, ticks = #replay.frames, seconds = #replay.frames * replay.fixedStep, result = replay.result, states = states, stumbles = stumbles, rejectedInputs = rejected, integrity = replay.integrity })
elseif command == "replay" then
  local replay = loadReplay(arg[2] or ""); local result = Replay.verify(replay); result.command = command; result.id = replay.id; emit(result); if not result.valid then os.exit(9) end
elseif command == "overlay" then
  local replay, output = loadReplay(arg[2] or ""), arg[3]; if not output then die("usage: overlay <run-id|replay.json> <overlay.svg>") end; local level = assert(Generator.generate(replay.seed)); local data, svg = Overlay.build(level, replay); write(output, svg); write(output .. ".json", Overlay.json(data)); emit({ command = command, svg = output, data = output .. ".json", ticks = #replay.frames })
elseif command == "problems" then
  local replay, tick = loadReplay(arg[2] or ""), tonumber(arg[3]); if not tick then die("usage: problems <run-id|replay.json> <tick>") end; local level = assert(Generator.generate(replay.seed)); local found, err = Obstacle.locate(level, replay, tick); if not found then die(err, 10) end; emit({ command = command, tick = tick, obstacle = found.solid, distance = found.distance, approach = { position = found.frame.position, velocity = found.frame.velocity, state = found.frame.state, collisions = found.frame.collisions, rejectedInputs = found.frame.rejectedInputs } })
elseif command == "ab" then
  local replay, tick, prefix = loadReplay(arg[2] or ""), tonumber(arg[3]), arg[4]; if not tick or not prefix then die("usage: ab <run-id|replay.json> <tick> <output-prefix>") end; local level = assert(Generator.generate(replay.seed)); local report, winner = Obstacle.ab(level, replay, tick); if not report then die(winner, 11) end; write(prefix .. ".level.json", Serialization.encode(winner)); write(prefix .. ".report.json", Json.encode(report) .. "\n"); local data, svg = Overlay.build(winner, replay, { tick = tick, solidId = report.obstacle.solidId }); write(prefix .. ".svg", svg); write(prefix .. ".overlay.json", Overlay.json(data)); emit({ command = command, report = prefix .. ".report.json", level = prefix .. ".level.json", overlay = prefix .. ".svg", before = report.before, after = report.after, constraints = report.constraints })
elseif command == "delete-run" then
  local id = arg[2]; if not id then die("usage: delete-run <run-id>") end; local ok, err = Storage.deleteRun(id); if not ok then die(err, 12) end; emit({ command = command, deleted = id })
else die("unknown command", 2, { command = command }) end
