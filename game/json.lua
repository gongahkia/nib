local Json = {}
local escapes = { ['"'] = '\\"', ['\\'] = '\\\\', ['\b'] = '\\b', ['\f'] = '\\f', ['\n'] = '\\n', ['\r'] = '\\r', ['\t'] = '\\t' }
local function isArray(t) local count, max = 0, 0; for k in pairs(t) do if type(k) ~= "number" or k < 1 or k % 1 ~= 0 then return false end; count, max = count + 1, math.max(max, k) end; return count == max end
local function encode(value, stack)
  local kind = type(value)
  if kind == "nil" then return "null" elseif kind == "boolean" then return value and "true" or "false"
  elseif kind == "number" then if value ~= value or value == math.huge or value == -math.huge then error("cannot encode non-finite number") end; return string.format("%.14g", value)
  elseif kind == "string" then return '"' .. value:gsub('[%z\1-\31\\"]', function(c) return escapes[c] or string.format("\\u%04x", c:byte()) end) .. '"'
  elseif kind ~= "table" then error("cannot encode " .. kind) end
  if stack[value] then error("cannot encode cyclic table") end; stack[value] = true
  local out = {}; if isArray(value) then for i = 1, #value do out[i] = encode(value[i], stack) end; stack[value] = nil; return "[" .. table.concat(out, ",") .. "]" end
  local keys = {}; for key in pairs(value) do if type(key) ~= "string" then error("object keys must be strings") end; keys[#keys + 1] = key end; table.sort(keys)
  for _, key in ipairs(keys) do out[#out + 1] = encode(key, stack) .. ":" .. encode(value[key], stack) end; stack[value] = nil; return "{" .. table.concat(out, ",") .. "}"
end
function Json.encode(value) return encode(value, {}) end
function Json.decode(text)
  local at = 1; local parse
  local function skip() at = text:find("[^ \n\r\t]", at) or (#text + 1) end
  local function stringValue()
    at = at + 1; local out = {}
    while at <= #text do local c = text:sub(at, at); at = at + 1; if c == '"' then return table.concat(out) end
      if c == "\\" then local e = text:sub(at, at); at = at + 1; local map = { ['"'] = '"', ['\\'] = '\\', ['/'] = '/', b = '\b', f = '\f', n = '\n', r = '\r', t = '\t' }; if e == "u" then local hex = text:sub(at, at + 3); at = at + 4; out[#out + 1] = string.char(tonumber(hex, 16) % 256) elseif map[e] then out[#out + 1] = map[e] else error("invalid escape at " .. at) end else out[#out + 1] = c end
    end; error("unterminated string")
  end
  local function arrayValue() at = at + 1; local out = {}; skip(); if text:sub(at, at) == "]" then at = at + 1; return out end; while true do out[#out + 1] = parse(); skip(); local c = text:sub(at, at); at = at + 1; if c == "]" then return out elseif c ~= "," then error("expected ',' or ']' at " .. (at - 1)) end end end
  local function objectValue() at = at + 1; local out = {}; skip(); if text:sub(at, at) == "}" then at = at + 1; return out end; while true do skip(); if text:sub(at, at) ~= '"' then error("expected string key at " .. at) end; local key = stringValue(); skip(); if text:sub(at, at) ~= ":" then error("expected ':' at " .. at) end; at = at + 1; out[key] = parse(); skip(); local c = text:sub(at, at); at = at + 1; if c == "}" then return out elseif c ~= "," then error("expected ',' or '}' at " .. (at - 1)) end end end
  parse = function() skip(); local c = text:sub(at, at); if c == '"' then return stringValue() elseif c == "[" then return arrayValue() elseif c == "{" then return objectValue() elseif text:sub(at, at + 3) == "true" then at = at + 4; return true elseif text:sub(at, at + 4) == "false" then at = at + 5; return false elseif text:sub(at, at + 3) == "null" then at = at + 4; return nil end; local start = at; while text:sub(at, at):match("[-+0-9.eE]") do at = at + 1 end; local number = tonumber(text:sub(start, at - 1)); if not number then error("invalid JSON value at " .. start) end; return number end
  local value = parse(); skip(); if at <= #text then error("trailing JSON at " .. at) end; return value
end
return Json
