local PRNG = require("game.prng")
local Reach = require("game.reachability")
local materials = require("game.materials")
local Generator = {}

Generator.version = 1
Generator.params = {
  sectionCount = 22, horizontalLength = { 650, 850 }, verticalLength = { 350, 450 },
  targetDuration = { 180, 300 }, collectibleEvery = 2,
}

local strata = { "buried_homes", "transit_gullet", "civic_machine", "market_scaffold", "reservoir_archive" }
local rhythms = { "sprint", "dash_gap", "grapple_arc", "rail_drop", "wall_rise", "tool_breach", "elastic_chain" }
local materialNames = { "stone", "brick", "metal", "ice", "sticky", "elastic", "glass" }

local function node(level, x, y, section, provenance)
  local n = { id = #level.nodes + 1, x = x, y = y, section = section, provenance = provenance }
  level.nodes[#level.nodes + 1] = n
  return n
end

local function addSolid(level, x, y, w, h, material, provenance, slope)
  level.solids[#level.solids + 1] = { id = #level.solids + 1, x = x, y = y, w = w, h = h, material = material, provenance = provenance, slope = slope }
end

local function addLink(level, from, to, mode, distance, dy, provenance)
  level.links[#level.links + 1] = { id = #level.links + 1, from = from.id, to = to.id, mode = mode, distance = distance, dy = dy, refresh = true, provenance = provenance }
end

local function buildAttempt(seed, attempt)
  local rng = PRNG.new(seed + attempt * 7919)
  local level = {
    schemaVersion = 1, kind = "palimpsest-level", seed = seed, generationAttempt = attempt,
    nodes = {}, links = {}, solids = {}, anchors = {}, rails = {}, hazards = {}, enemies = {}, inhabitants = {},
    machines = {}, objects = {}, collectibles = {}, decisions = {}, warnings = {},
  }
  local x, y, previous = 12, 96, node(level, 12, 96, 0, "spawn")
  level.spawn = { x = x, y = y - 1, node = previous.id }
  addSolid(level, -32, y, 120, 32, "bedrock", "spawn apron")
  local sectionKinds = {}

  for section = 1, Generator.params.sectionCount do
    local vertical = section % 3 == 0 or section % 5 == 0
    local kind = vertical and "vertical" or "horizontal"
    sectionKinds[#sectionKinds + 1] = kind
    local stratum = rng:choice(strata)
    local rhythm = rng:choice(rhythms)
    local sectionLength = vertical and rng:int(Generator.params.verticalLength[1], Generator.params.verticalLength[2]) or rng:int(Generator.params.horizontalLength[1], Generator.params.horizontalLength[2])
    local steps = vertical and rng:int(6, 9) or rng:int(9, 13)
    local dx = sectionLength / steps
    local direction = vertical and (rng:chance(0.5) and -1 or 1) or 0
    if vertical and y + direction * 68 < 44 then direction = 1 end
    if vertical and y + direction * 68 > 260 then direction = -1 end
    level.decisions[#level.decisions + 1] = { section = section, kind = kind, stratum = stratum, rhythm = rhythm, steps = steps, source = "grammar.section" }

    for step = 1, steps do
      local oldX, oldY = x, y
      x = x + dx
      local wave = math.floor(math.sin((section * 7 + step) * 0.71) * 3)
      if vertical then y = y + direction * rng:int(7, 10) else y = y + wave end
      y = math.max(44, math.min(270, y))
      local mode = "run"
      if rhythm == "dash_gap" and step % 4 == 2 then mode = "dash"
      elseif rhythm == "grapple_arc" and step % 4 == 1 then mode = "grapple"
      elseif rhythm == "rail_drop" and step % 3 == 0 then mode = "grind"
      elseif vertical and direction < 0 then mode = step % 2 == 0 and "wall" or "grapple"
      elseif rhythm == "tool_breach" and step % 4 == 0 then mode = "tool"
      elseif rhythm == "elastic_chain" and step % 3 == 0 then mode = "elastic"
      elseif math.abs(y - oldY) > 5 then mode = "slope" end
      local current = node(level, x, y, section, stratum .. "/" .. rhythm)
      local distance = math.sqrt((x - oldX)^2 + (y - oldY)^2)
      if mode == "dash" then distance = math.min(distance, Reach.capabilities().jumpDistance + Reach.capabilities().dashDistance - rng:int(4, 10)) end
      if mode == "grapple" then distance = math.min(distance, Reach.capabilities().grappleRange - rng:int(4, 12)); level.anchors[#level.anchors + 1] = { x = (oldX + x) / 2, y = math.min(oldY, y) - rng:int(14, 25), provenance = current.provenance } end
      addLink(level, previous, current, mode, distance, y - oldY, current.provenance)
      if step % 6 == 0 then
        local branch = node(level, x - dx * 0.45, y - 22, section, current.provenance .. "/optional_branch")
        addLink(level, previous, branch, "grapple", math.min(Reach.capabilities().grappleRange - 8, dx), -22, branch.provenance)
        addLink(level, branch, current, "dash", math.min(Reach.capabilities().jumpDistance + Reach.capabilities().dashDistance - 8, dx), 22, branch.provenance)
        level.anchors[#level.anchors + 1] = { x = branch.x, y = branch.y - 12, provenance = branch.provenance }
      end

      local material = rng:choice(materialNames)
      local platformWidth = math.max(18, dx - (mode == "dash" and rng:int(8, 15) or 0))
      local platformHeight = rng:int(7, 15)
      addSolid(level, x - platformWidth / 2, y, platformWidth, platformHeight, material, current.provenance, mode == "slope" and (y - oldY) or nil)
      addSolid(level, x - platformWidth / 2, y + platformHeight, platformWidth, 12, "stone", current.provenance .. "/understructure")
      if mode == "grind" then level.rails[#level.rails + 1] = { x1 = oldX, y1 = oldY - 10, x2 = x, y2 = y - 10, provenance = current.provenance } end
      if mode == "tool" then
        addSolid(level, x - 6, y - 28, 8, 28, rng:choice({ "brick", "glass" }), current.provenance .. "/breach")
      end
      if mode == "elastic" then addSolid(level, x - 8, y - 3, 16, 3, "elastic", current.provenance .. "/spring") end
      if rng:chance(0.13) then level.hazards[#level.hazards + 1] = { x = x + 5, y = y - 3, kind = "spikes", node = current.id } end
      if rng:chance(0.11) then level.enemies[#level.enemies + 1] = { x = x - 5, y = y, w = 6, h = 6, alive = true, kind = rng:choice({ "sweeper", "lamp_drone" }), node = current.id } end
      if rng:chance(0.12) then level.inhabitants[#level.inhabitants + 1] = { x = x, y = y, reaction = rng:choice({ "flee", "hide", "shutter", "warn" }), node = current.id } end
      if rng:chance(0.09) then level.machines[#level.machines + 1] = { x = x + 3, y = y, kind = rng:choice({ "pump", "caretaker", "heat_exchanger" }), node = current.id } end
      if rng:chance(0.08) then level.objects[#level.objects + 1] = { x = x, y = y - 9, w = 6, h = 6, vx = 0, vy = 0, node = current.id } end
      if step == math.ceil(steps / 2) and section % Generator.params.collectibleEvery == 0 then
        level.collectibles[#level.collectibles + 1] = { id = string.format("fragment-%02d-%02d", section, rng:int(1, 9)), loreId = "fold-evidence-" .. section, x = x, y = y - 15, node = current.id }
      end
      previous = current
    end
  end
  level.exit = { x = x + 18, y = y, node = previous.id }
  addSolid(level, x - 16, y, 70, 32, "bedrock", "exit apron")
  level.hand = { x = level.spawn.x - require("game.config").hand.startGap, y = level.spawn.y, phase = rng:float() * math.pi * 2 }
  level.sectionKinds = sectionKinds
  return level
end

local function metrics(level, validation, elapsed)
  local counts, destructible, repeated, prior = {}, 0, 0, nil
  for _, solid in ipairs(level.solids) do
    counts[solid.material] = (counts[solid.material] or 0) + 1
    if materials[solid.material].destructible then destructible = destructible + 1 end
  end
  for _, decision in ipairs(level.decisions) do if prior == decision.rhythm then repeated = repeated + 1 end; prior = decision.rhythm end
  local vertical = 0; for _, kind in ipairs(level.sectionKinds) do if kind == "vertical" then vertical = vertical + 1 end end
  local elevation = level.nodes[#level.nodes].y - level.nodes[1].y
  return {
    routeLength = level.exit.x - level.spawn.x, expectedDuration = (level.exit.x - level.spawn.x) / 55,
    horizontalRatio = (#level.sectionKinds - vertical) / #level.sectionKinds, verticalRatio = vertical / #level.sectionKinds,
    branchCount = math.max(0, #level.links - (#level.nodes - 1)), reconnectionCount = math.max(0, #level.links - (#level.nodes - 1)), elevationChange = elevation, materialDistribution = counts,
    destructibleRatio = destructible / #level.solids, requiredMovementStates = validation.used,
    optionalMovementStates = { slide = true, dive = true, vault = true, tool_recoil = true },
    anchorCount = #level.anchors, railCount = #level.rails, collectibleReachable = #validation.unreachable == 0,
    minimumExecutionMargin = validation.minimumMargin, repeatedPatternFrequency = repeated / #level.decisions,
    generationMilliseconds = elapsed, validationMilliseconds = 0,
  }
end

function Generator.generate(seed)
  seed = tonumber(seed) or os.time()
  local started, repairs, rejections = os.clock(), 0, 0
  for attempt = 0, 5 do
    local level = buildAttempt(seed, attempt)
    local validation = Reach.validate(level)
    local repairPass = 0
    while not validation.valid and repairPass < 12 do
      -- Repair the current frontier, then validate again so later blocked frontiers become inspectable.
      local changed = false
      for _, evidence in ipairs(validation.evidence) do
        if not evidence.reachable then
          for _, link in ipairs(level.links) do if link.from == evidence.from and link.to == evidence.to then link.mode, link.distance, link.hardness = "tool", 1, 3; repairs, changed = repairs + 1, true end end
        end
      end
      if not changed then break end
      repairPass, validation = repairPass + 1, Reach.validate(level)
    end
    if validation.valid then
      level.validation = validation
      level.metrics = metrics(level, validation, (os.clock() - started) * 1000)
      level.metrics.repairCount, level.metrics.rejectionCount = repairs, rejections
      return level
    end
    rejections = rejections + 1
  end
  return nil, "generation rejected six deterministic attempts"
end

return Generator
