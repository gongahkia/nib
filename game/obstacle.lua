local Json = require("game.json")
local Reach = require("game.reachability")
local Obstacle = {}
function Obstacle.locate(level, replay, tick)
  local frame = replay.frames[math.max(1, math.min(#replay.frames, tick))]; if not frame then return nil, "tick outside replay" end
  local px, py, best, distance = frame.position[1], frame.position[2]
  for _, solid in ipairs(level.solids) do local cx, cy = solid.x + solid.w / 2, solid.y; local d = math.sqrt((px - cx)^2 + (py - cy)^2); if not distance or d < distance then best, distance = solid, d end end
  return { solid = best, frame = frame, distance = distance }
end
local function copy(value) return Json.decode(Json.encode(value)) end
local function evaluate(solid, frame)
  local speed = math.max(1, math.abs(frame.velocity[1])); local center = solid.x + solid.w / 2; local landing = frame.position[1] + frame.velocity[1] * 0.18
  local landingMargin = solid.w / 2 - math.abs(landing - center); local timingWindow = solid.w / speed
  local successes = 0; local samples = {}
  for offset = -4, 4 do local perturbed = landing + frame.velocity[1] * offset / 60; local margin = solid.w / 2 - math.abs(perturbed - center); local ok = margin >= 0; if ok then successes = successes + 1 end; samples[#samples + 1] = { tickOffset = offset, landingX = perturbed, margin = margin, complete = ok } end
  return { approach = { velocity = frame.velocity, state = frame.state }, timingWindow = timingWindow, landingMargin = landingMargin, completionRate = successes / #samples, samples = samples, failureReason = landingMargin < 0 and "landing outside surface" or "insufficient measured margin" }
end
function Obstacle.ab(level, replay, tick)
  local located, err = Obstacle.locate(level, replay, tick); if not located then return nil, err end
  local original, variants = evaluate(located.solid, located.frame), {}
  local changes = { { dx = -2, dw = 2 }, { dx = -2, dw = 4 }, { dx = -3, dw = 6 }, { dx = 0, dw = 3 } }
  for index, change in ipairs(changes) do
    local candidate = copy(level); local solid
    for _, item in ipairs(candidate.solids) do if item.id == located.solid.id then solid = item; break end end
    solid.x, solid.w = solid.x + change.dx, solid.w + change.dw
    local validation, measured = Reach.validate(candidate), evaluate(solid, located.frame)
    variants[#variants + 1] = { index = index, change = change, metrics = measured, constraintsValid = validation.valid, level = candidate }
  end
  table.sort(variants, function(a, b) if a.constraintsValid ~= b.constraintsValid then return a.constraintsValid end; if a.metrics.completionRate ~= b.metrics.completionRate then return a.metrics.completionRate > b.metrics.completionRate end; return a.metrics.landingMargin > b.metrics.landingMargin end)
  local winner = variants[1]; if not winner.constraintsValid or winner.metrics.completionRate <= original.completionRate or winner.metrics.landingMargin <= original.landingMargin then return nil, "no variant demonstrated a verified margin improvement" end
  return { schemaVersion = 1, kind = "obstacle-ab-report", runId = replay.id, tick = tick, obstacle = { solidId = located.solid.id, distance = located.distance }, before = original, after = winner.metrics, change = winner.change, constraints = { reachability = true, collectibles = true, generator = true }, variantsEvaluated = #variants }, winner.level
end
return Obstacle
