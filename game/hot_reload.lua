local Hot = {}; Hot.__index = Hot
function Hot.new(path, stat, read, decode) return setmetatable({ path = path, stat = stat, read = read, decode = decode, modified = nil, status = "waiting", value = nil }, Hot) end
function Hot:poll(force)
  local info = self.stat(self.path); if not info then self.status = "missing"; return false end
  local modified = info.modtime or info.modified or 0; if not force and self.modified == modified then return false end
  local text, readError = self.read(self.path); if not text then self.status = "error: " .. tostring(readError); return false end
  local value, err = self.decode(text); if not value then self.status = "error " .. err.path .. ": " .. err.message; return false end
  self.modified, self.value, self.status = modified, value, "loaded"; return true, value
end
return Hot
