local Schema = {}
Schema.current = { level = 1, replay = 1, profile = 1, bindings = 1, cli = 1 }
local function fail(path, message) return nil, { path = path, message = message } end
local function array(value, path) if type(value) ~= "table" then return fail(path, "expected array") end; return true end
function Schema.validateLevel(level)
  if type(level) ~= "table" then return fail("$", "expected object") end
  if level.schemaVersion ~= 1 then return fail("$.schemaVersion", "expected supported version 1") end
  if level.kind ~= "palimpsest-level" then return fail("$.kind", "expected palimpsest-level") end
  if type(level.seed) ~= "number" then return fail("$.seed", "expected number") end
  for _, name in ipairs({ "nodes", "links", "solids", "anchors", "rails", "collectibles" }) do local ok, err = array(level[name], "$." .. name); if not ok then return nil, err end end
  if type(level.spawn) ~= "table" or type(level.spawn.node) ~= "number" then return fail("$.spawn.node", "expected node id") end
  if type(level.exit) ~= "table" or type(level.exit.node) ~= "number" then return fail("$.exit.node", "expected node id") end
  for i, solid in ipairs(level.solids) do
    for _, field in ipairs({ "x", "y", "w", "h" }) do if type(solid[field]) ~= "number" then return fail(string.format("$.solids[%d].%s", i, field), "expected number") end end
    if type(solid.material) ~= "string" then return fail(string.format("$.solids[%d].material", i), "expected material string") end
  end
  return true
end
function Schema.validateReplay(replay)
  if type(replay) ~= "table" then return fail("$", "expected object") end
  if replay.schemaVersion ~= 1 then return fail("$.schemaVersion", "expected supported version 1") end
  if replay.kind ~= "palimpsest-replay" then return fail("$.kind", "expected palimpsest-replay") end
  if type(replay.seed) ~= "number" then return fail("$.seed", "expected number") end
  if type(replay.frames) ~= "table" then return fail("$.frames", "expected array") end
  if type(replay.integrity) ~= "table" or replay.integrity.algorithm ~= "fnv1a32" then return fail("$.integrity.algorithm", "expected fnv1a32") end
  return true
end
function Schema.migrate(kind, data)
  local current = Schema.current[kind]; if not current then return nil, { path = "$.kind", message = "unknown schema kind" } end
  if data.schemaVersion == current then return data end
  return nil, { path = "$.schemaVersion", message = "no registered migration from version " .. tostring(data.schemaVersion) .. " to " .. current }
end
return Schema
