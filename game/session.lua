local Generator = require("game.generator")
local World = require("game.world")
local Player = require("game.player")
local Hand = require("game.hand")
local Scoring = require("game.scoring")
local config = require("game.config")
local Session = {}
Session.__index = Session

function Session.new(seed)
  local level = assert(Generator.generate(seed))
  return Session.fromLevel(level)
end

function Session.fromLevel(level)
  return setmetatable({ seed = level.seed, level = level, world = World.new(level), player = Player.new(level.spawn.x, level.spawn.y), hand = Hand.new(level.hand, level.seed), score = Scoring.new(), tick = 0, status = "running", result = nil, collected = {}, incidents = {} }, Session)
end

function Session:update(input)
  if self.status ~= "running" then return end
  self.tick = self.tick + 1
  local p, oldState = self.player, self.player.state
  p:update(self.world, input, config.step); self.world:updateObjects(config.step)
  if p.state ~= oldState then Scoring.event(self.score, self.tick, p.state == "high_speed_run" and "high_speed" or "state_chain") end
  for _, event in ipairs(p.styleEvents) do if not event.scored then Scoring.event(self.score, self.tick, event.kind); event.scored = true end end
  for _, hazard in ipairs(self.level.hazards) do
    local distance = math.sqrt((p.x - hazard.x)^2 + (p.y - hazard.y)^2)
    if distance < 5 then p.vx = -p.facing * 15; p:setState("stumble_recovery", "hazard impact"); Scoring.event(self.score, self.tick, "stumble")
    elseif distance < 11 and not hazard.nearMiss then hazard.nearMiss = true; Scoring.event(self.score, self.tick, "near_miss") end
  end
  for _, collectible in ipairs(self.level.collectibles) do
    if not self.collected[collectible.id] and math.abs(p.x - collectible.x) < 6 and math.abs(p.y - collectible.y) < 14 then
      self.collected[collectible.id] = true; Scoring.collect(self.score)
      self.incidents[#self.incidents + 1] = { tick = self.tick, kind = "collectible", id = collectible.id, loreId = collectible.loreId }
    end
  end
  for _, human in ipairs(self.level.inhabitants) do
    if human.x - self.hand.x < 90 then human.active = true; if human.reaction == "flee" then human.x = human.x + 13 * config.step elseif human.reaction == "hide" then human.hidden = true end end
  end
  if self.hand:update(self.world, self.level, p, config.step) then
    self.status, p.captured = "captured", true; p:setState("hand_capture", self.hand.captureReason); self.result = Scoring.result(self.score, self.tick)
  elseif p.x >= self.level.exit.x then
    self.status, p.finished = "finished", true; p:setState("exit_finish", "real exit reached"); self.result = Scoring.result(self.score, self.tick)
  end
end

return Session
