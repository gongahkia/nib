package.path = "./?.lua;./?/init.lua;" .. package.path
local Generator = require("game.generator")
local World = require("game.world")
local Player = require("game.player")
local Hand = require("game.hand")
local Json = require("game.json")
local config = require("game.config")

local count, first = tonumber(arg[1]) or 200, tonumber(arg[2]) or 1
if count < 1 or count > 5000 then io.stderr:write("count must be between 1 and 5000\n"); os.exit(2) end
local report = { schemaVersion = 1, kind = "seed-soak", count = count, firstSeed = first, valid = 0, failures = {}, warnings = {}, routeLength = { min = math.huge, max = -math.huge }, duration = { min = math.huge, max = -math.huge }, repairs = 0, rejections = 0, ik = { solutions = 0, failures = 0 } }
local function finite(n) return type(n) == "number" and n == n and n ~= math.huge and n ~= -math.huge end
for seed = first, first + count - 1 do
  local level, err = Generator.generate(seed)
  if not level then report.failures[#report.failures + 1] = { seed = seed, reason = err }
  else
    local valid = level.validation.valid
    for i, solid in ipairs(level.solids) do if not finite(solid.x) or not finite(solid.y) or solid.w <= 0 or solid.h <= 0 then valid = false; report.failures[#report.failures + 1] = { seed = seed, reason = "invalid geometry", solid = i }; break end end
    if valid then
      local world, player, hand = World.new(level), Player.new(level.spawn.x, level.spawn.y), Hand.new(level.hand, seed)
      for _ = 1, 180 do hand:update(world, level, player, config.step); for _, finger in ipairs(hand.fingers) do report.ik.solutions = report.ik.solutions + 1; if not finger.reachable then report.ik.failures = report.ik.failures + 1 end; if not finite(finger.joint.x) or not finite(finger.joint.y) then valid = false end end end
    end
    if valid then report.valid = report.valid + 1 else report.failures[#report.failures + 1] = { seed = seed, reason = "non-finite hand pose" } end
    report.routeLength.min, report.routeLength.max = math.min(report.routeLength.min, level.metrics.routeLength), math.max(report.routeLength.max, level.metrics.routeLength)
    report.duration.min, report.duration.max = math.min(report.duration.min, level.metrics.expectedDuration), math.max(report.duration.max, level.metrics.expectedDuration)
    report.repairs, report.rejections = report.repairs + level.metrics.repairCount, report.rejections + level.metrics.rejectionCount
  end
end
report.ok = #report.failures == 0
if report.ik.failures / math.max(1, report.ik.solutions) > 0.25 then report.warnings[#report.warnings + 1] = "more than 25% of sampled IK targets required clamping" end
io.write(Json.encode(report), "\n")
if not report.ok then os.exit(1) end
