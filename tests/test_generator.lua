local H = require("tests.harness")
local Generator = require("game.generator")
local Reach = require("game.reachability")

local function signature(level)
  local parts = { level.seed, #level.nodes, #level.links, #level.solids, #level.collectibles }
  for _, n in ipairs(level.nodes) do parts[#parts + 1] = string.format("%d:%.3f:%.3f:%s", n.id, n.x, n.y, n.provenance) end
  for _, s in ipairs(level.solids) do parts[#parts + 1] = string.format("%.3f:%.3f:%s", s.x, s.y, s.material) end
  return table.concat(parts, "|")
end

H.test("same seed generation is structurally identical", function()
  local a, b = assert(Generator.generate(8675309)), assert(Generator.generate(8675309))
  H.eq(signature(a), signature(b)); H.eq(a.hand.phase, b.hand.phase)
end)

H.test("generated exit and every collectible are movement reachable", function()
  local level = assert(Generator.generate(42)); local report = Reach.validate(level)
  H.ok(report.valid); H.eq(#report.unreachable, 0); H.ok(report.nodeReachable[level.exit.node])
  for _, collectible in ipairs(level.collectibles) do H.ok(report.nodeReachable[collectible.node]) end
end)

H.test("generator repairs an impossible movement link", function()
  local level = assert(Generator.generate(77))
  level.links[1].mode, level.links[1].distance = "jump", 999
  H.ok(not Reach.validate(level).valid)
  level.links[1].mode, level.links[1].distance, level.links[1].hardness = "tool", 1, 3
  H.ok(Reach.validate(level).valid)
end)

H.test("bounded seed audit is stable and diverse", function()
  local signatures, lengths, materials = {}, {}, {}
  for seed = 1, 20 do
    local level = assert(Generator.generate(seed)); H.ok(level.validation.valid)
    signatures[signature(level)], lengths[#lengths + 1] = true, level.metrics.routeLength
    for material in pairs(level.metrics.materialDistribution) do materials[material] = true end
    H.ok(level.metrics.expectedDuration >= 180 and level.metrics.expectedDuration <= 300)
    H.ok(level.metrics.verticalRatio > 0 and level.metrics.horizontalRatio > 0)
  end
  local unique, materialCount = 0, 0; for _ in pairs(signatures) do unique = unique + 1 end; for _ in pairs(materials) do materialCount = materialCount + 1 end
  H.eq(unique, 20); H.ok(materialCount >= 6)
end)
