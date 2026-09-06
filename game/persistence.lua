local Json = require("game.json")
local P = {}
local function defaults() return { schemaVersion = 1, lore = {}, cosmetics = { scarf = "magenta" }, runs = {}, bindings = require("game.input").defaults } end
function P.load(read) local text = read("profile.json"); if not text then return defaults() end; local ok, data = pcall(Json.decode, text); if not ok or data.schemaVersion ~= 1 then return defaults(), "profile invalid; defaults loaded" end; data.lore, data.cosmetics, data.runs, data.bindings = data.lore or {}, data.cosmetics or {}, data.runs or {}, data.bindings or require("game.input").defaults; return data end
function P.save(profile, write) return write("profile.json", Json.encode(profile)) end
function P.unlockLore(profile, id) local first = not profile.lore[id]; profile.lore[id] = true; return first end
function P.recordSummary(profile, summary) profile.runs[#profile.runs + 1] = summary end
return P
