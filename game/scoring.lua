local config = require("game.config").scoring
local Scoring = {}
function Scoring.new() return { style = 0, collectibles = 0, chain = {}, chainTick = -999, events = {} } end
function Scoring.event(score, tick, kind)
  local weights = { state_chain = 35, high_speed = 2, near_miss = 90, creative_destruction = 120, impact_break = 100, enemy_speed = 140, tool_use = 10, stumble = -80 }
  if tick - score.chainTick < 90 and score.chain[#score.chain] ~= kind then score.style = score.style + weights.state_chain end
  score.style = math.max(0, score.style + (weights[kind] or 0)); score.chain[#score.chain + 1] = kind; score.chainTick = tick; if #score.chain > 6 then table.remove(score.chain, 1) end
  score.events[#score.events + 1] = { tick = tick, kind = kind, value = weights[kind] or 0 }
end
function Scoring.collect(score) score.collectibles = score.collectibles + 1 end
function Scoring.result(score, ticks) local seconds = ticks / 60; local time = math.max(0, math.floor((config.parTimeSeconds * 1.5 - seconds) * config.timeWeight)); local collectible = score.collectibles * config.collectibleWeight; return { seconds = seconds, time = time, style = math.floor(score.style), collectibles = collectible, total = time + math.floor(score.style) + collectible } end
return Scoring
