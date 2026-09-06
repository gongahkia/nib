local config = require("game.config").player
local Reach = {}

function Reach.capabilities()
  local flight = config.jumpSpeed * 2 / config.gravity
  local jumpDistance = config.maxRunSpeed * flight
  return {
    jumpDistance = jumpDistance,
    jumpRise = config.jumpSpeed * config.jumpSpeed / (2 * config.gravity),
    dashDistance = config.dashSpeed * config.dashTicks / 60,
    grappleRange = config.grappleRange,
    wallRise = config.wallJumpY * config.wallJumpY / (2 * config.gravity),
    toolCutHardness = 7,
  }
end

local function canTraverse(link, resources, caps)
  local margin
  if link.mode == "run" or link.mode == "slope" or link.mode == "grind" then margin = 1
  elseif link.mode == "jump" then margin = caps.jumpDistance - (link.distance or 0)
  elseif link.mode == "dash" then
    if resources.dash < 1 then return false, -1 end
    margin = caps.jumpDistance + caps.dashDistance - (link.distance or 0)
  elseif link.mode == "wall" then margin = caps.wallRise - math.abs(link.dy or 0)
  elseif link.mode == "grapple" then
    if resources.grapple < 1 then return false, -1 end
    margin = caps.grappleRange - (link.distance or 0)
  elseif link.mode == "tool" then margin = caps.toolCutHardness - (link.hardness or 0)
  elseif link.mode == "elastic" then margin = 132 * 132 / (2 * config.gravity) - math.abs(link.dy or 0)
  else return false, -999 end
  return margin >= 0, margin
end

function Reach.validate(level)
  local caps, queue, seen = Reach.capabilities(), { { id = level.spawn.node, dash = 1, grapple = 1 } }, {}
  local evidence, minMargin, used = {}, math.huge, {}
  local adjacency = {}
  for _, link in ipairs(level.links) do
    adjacency[link.from] = adjacency[link.from] or {}; adjacency[link.from][#adjacency[link.from] + 1] = link
    if not link.oneWay then adjacency[link.to] = adjacency[link.to] or {}; adjacency[link.to][#adjacency[link.to] + 1] = { from = link.to, to = link.from, mode = link.mode, distance = link.distance, dy = -(link.dy or 0), hardness = link.hardness } end
  end
  local head = 1
  while head <= #queue do
    local current = queue[head]; head = head + 1
    local key = current.id .. ":" .. current.dash .. ":" .. current.grapple
    if not seen[key] then
      seen[key] = true
      for _, link in ipairs(adjacency[current.id] or {}) do
        local ok, margin = canTraverse(link, current, caps)
        evidence[#evidence + 1] = { from = link.from, to = link.to, mode = link.mode, reachable = ok, margin = margin }
        if ok then
          minMargin, used[link.mode] = math.min(minMargin, margin), true
          local dash = link.mode == "dash" and current.dash - 1 or current.dash
          local grapple = link.mode == "grapple" and current.grapple - 1 or current.grapple
          if link.refresh then dash, grapple = 1, 1 end
          queue[#queue + 1] = { id = link.to, dash = dash, grapple = grapple }
        end
      end
    end
  end
  local nodeReachable = {}
  for key in pairs(seen) do nodeReachable[tonumber(key:match("^(%d+):"))] = true end
  local unreachable = {}
  if not nodeReachable[level.exit.node] then unreachable[#unreachable + 1] = "exit" end
  for _, collectible in ipairs(level.collectibles) do if not nodeReachable[collectible.node] then unreachable[#unreachable + 1] = collectible.id end end
  return {
    valid = #unreachable == 0, unreachable = unreachable, nodeReachable = nodeReachable,
    evidence = evidence, capabilities = caps, minimumMargin = minMargin == math.huge and 0 or minMargin, used = used,
  }
end

return Reach
