local H = require("tests.harness")
local Generator = require("game.generator")
local World = require("game.world")
local Player = require("game.player")
local Hand = require("game.hand")
local Scoring = require("game.scoring")
local Persistence = require("game.persistence")
local Json = require("game.json")
local config = require("game.config")

H.test("hand IK is finite and deterministic", function()
  local level = assert(Generator.generate(19)); local worldA, worldB = World.new(level), World.new(assert(Generator.generate(19)))
  local playerA, playerB = Player.new(40, 96), Player.new(40, 96); local a, b = Hand.new(level.hand, 19), Hand.new(level.hand, 19)
  for _ = 1, 180 do a:update(worldA, level, playerA, config.step); b:update(worldB, level, playerB, config.step) end
  H.near(a.x, b.x, 1e-9); H.near(a.fingers[2].joint.x, b.fingers[2].joint.x, 1e-9); H.ok(a.x == a.x and a.y == a.y)
end)

H.test("hand captures by geometric contact", function()
  local level = assert(Generator.generate(2)); local world = World.new(level); local p = Player.new(level.hand.x + 1, level.hand.y)
  local hand = Hand.new(level.hand, 2); H.ok(hand:update(world, level, p, config.step)); H.ok(hand.captureReason ~= nil)
end)

H.test("score exposes independently tunable components", function()
  local score = Scoring.new(); Scoring.event(score, 1, "near_miss"); Scoring.collect(score); local result = Scoring.result(score, 600)
  H.ok(result.time > 0); H.ok(result.style > 0); H.eq(result.collectibles, 500); H.eq(result.total, result.time + result.style + result.collectibles)
end)

H.test("lore and cosmetics persistence round trips without power", function()
  local stored = {}; local read = function(name) return stored[name] end; local write = function(name, text) stored[name] = text; return true end
  local profile = Persistence.load(read); H.ok(Persistence.unlockLore(profile, "fold-evidence-2")); H.ok(not Persistence.unlockLore(profile, "fold-evidence-2")); Persistence.recordSummary(profile, { seed = 2, status = "finished" }); Persistence.save(profile, write)
  local loaded = Persistence.load(read); H.ok(loaded.lore["fold-evidence-2"]); H.eq(loaded.cosmetics.scarf, "magenta"); H.eq(loaded.runs[1].seed, 2); H.eq(loaded.power, nil)
  H.eq(Json.encode(Json.decode(stored["profile.json"])), stored["profile.json"])
end)
