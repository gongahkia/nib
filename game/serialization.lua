local Json = require("game.json")
local Schema = require("game.schema")
local S = {}
function S.encode(value) return Json.encode(value) .. "\n" end
function S.decode(text, kind)
  local ok, value = pcall(Json.decode, text); if not ok then return nil, { path = "$", message = value } end
  local migrated, migrationError = Schema.migrate(kind, value); if not migrated then return nil, migrationError end
  local valid, err = kind == "level" and Schema.validateLevel(migrated) or kind == "replay" and Schema.validateReplay(migrated) or true
  if not valid then return nil, err end; return migrated
end
return S
