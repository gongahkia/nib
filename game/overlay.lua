local Json = require("game.json")
local Overlay = {}
local colours = { terrain = "#775652", player = "#9cc7b0", fast = "#fade94", stumble = "#dd4968", collision = "#53a08f", hand = "#893b5a", selected = "#e0a368" }
local function esc(text) return tostring(text):gsub("&", "&amp;"):gsub("<", "&lt;"):gsub('"', "&quot;") end
function Overlay.build(level, replay, selection)
  local minX, maxX, minY, maxY = level.spawn.x - 50, level.exit.x + 50, math.huge, -math.huge
  for _, n in ipairs(level.nodes) do minY, maxY = math.min(minY, n.y - 50), math.max(maxY, n.y + 30) end
  local sx, sy = 1200 / (maxX - minX), 360 / math.max(1, maxY - minY)
  local function point(x, y) return 10 + (x - minX) * sx, 10 + (y - minY) * sy end
  local data = { schemaVersion = 1, kind = "trajectory-overlay", runId = replay.id, selection = selection, bounds = { minX, minY, maxX, maxY }, trajectory = {}, contacts = {}, stateChanges = {}, rejectedInputs = {}, toolEvents = {}, terrainChanges = {}, collectibles = {}, pressure = {} }
  local svg = { '<svg xmlns="http://www.w3.org/2000/svg" width="1220" height="400" viewBox="0 0 1220 400">', '<rect width="1220" height="400" fill="#0c0a12"/>' }
  for _, solid in ipairs(level.solids) do local x, y = point(solid.x, solid.y); local selected = selection and selection.solidId == solid.id; svg[#svg + 1] = string.format('<rect x="%.2f" y="%.2f" width="%.2f" height="%.2f" fill="%s" opacity=".65" data-solid="%s"/>', x, y, solid.w * sx, math.max(1, solid.h * sy), selected and colours.selected or colours.terrain, esc(solid.id or "")) end
  local previousState
  for i, frame in ipairs(replay.frames) do
    local x, y = point(frame.position[1], frame.position[2]); local speed = math.sqrt(frame.velocity[1]^2 + frame.velocity[2]^2); local item = { tick = frame.tick, x = frame.position[1], y = frame.position[2], speed = speed, state = frame.state }
    data.trajectory[#data.trajectory + 1] = item; data.pressure[#data.pressure + 1] = { tick = frame.tick, routeDistance = frame.hand.routeDistance, closingSpeed = frame.hand.closingSpeed }
    if i > 1 then local prior = data.trajectory[i - 1]; local px, py = point(prior.x, prior.y); svg[#svg + 1] = string.format('<line x1="%.2f" y1="%.2f" x2="%.2f" y2="%.2f" stroke="%s" stroke-width="1"/>', px, py, x, y, speed > 52 and colours.fast or colours.player) end
    if frame.state ~= previousState then data.stateChanges[#data.stateChanges + 1] = { tick = frame.tick, state = frame.state, x = frame.position[1], y = frame.position[2] }; previousState = frame.state end
    for _, contact in ipairs(frame.collisions or {}) do data.contacts[#data.contacts + 1] = { tick = frame.tick, x = frame.position[1], y = frame.position[2], axis = contact.axis }; svg[#svg + 1] = string.format('<circle cx="%.2f" cy="%.2f" r="1.5" fill="%s"/>', x, y, colours.collision) end
    for _, rejected in ipairs(frame.rejectedInputs or {}) do data.rejectedInputs[#data.rejectedInputs + 1] = { tick = frame.tick, input = rejected.input, reason = rejected.reason, x = frame.position[1], y = frame.position[2] }; svg[#svg + 1] = string.format('<path d="M %.2f %.2f l 4 4 m -4 0 l 4 -4" stroke="%s"/>', x - 2, y - 2, colours.stumble) end
    if frame.toolEvent then data.toolEvents[#data.toolEvents + 1] = { tick = frame.tick, kind = frame.toolEvent.kind, x = frame.position[1], y = frame.position[2] } end
    if frame.terrainChanges and #frame.terrainChanges > 0 then data.terrainChanges[#data.terrainChanges + 1] = { tick = frame.tick, solids = frame.terrainChanges } end
    if frame.collectibleEvent then data.collectibles[#data.collectibles + 1] = frame.collectibleEvent end
    if frame.stumble then svg[#svg + 1] = string.format('<circle cx="%.2f" cy="%.2f" r="4" fill="none" stroke="%s"/>', x, y, colours.stumble) end
    if selection and frame.tick >= selection.tick - 30 and frame.tick <= selection.tick + 30 then svg[#svg + 1] = string.format('<circle cx="%.2f" cy="%.2f" r="3" fill="none" stroke="%s"/>', x, y, colours.selected) end
  end
  svg[#svg + 1] = string.format('<text x="10" y="392" fill="#f9f0d7" font-family="monospace" font-size="11">run %s · %d ticks · speed-coloured trajectory · contacts and rejected inputs</text>', esc(replay.id), #replay.frames); svg[#svg + 1] = "</svg>"
  return data, table.concat(svg, "\n")
end
function Overlay.json(data) return Json.encode(data) .. "\n" end
return Overlay
