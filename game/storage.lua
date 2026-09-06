local Storage = {}
local function base()
  local xdg = os.getenv("XDG_DATA_HOME")
  if xdg and xdg ~= "" then return xdg .. "/love/palimpsest-run" end
  return (os.getenv("HOME") or ".") .. "/.local/share/love/palimpsest-run"
end
function Storage.base() return base() end
function Storage.ensure(path) return os.execute(string.format("mkdir -p %q", path)) == 0 end
function Storage.read(path) local f = io.open(path, "rb"); if not f then return nil end; local text = f:read("*a"); f:close(); return text end
function Storage.write(path, text) local directory = path:match("^(.*)/[^/]+$"); if directory then Storage.ensure(directory) end; local f, err = io.open(path, "wb"); if not f then return nil, err end; f:write(text); f:close(); return true end
function Storage.runPath(id) return base() .. "/runs/" .. id .. ".json" end
function Storage.listRuns()
  local result = {}; local pipe = io.popen(string.format("find %q -maxdepth 1 -type f -name '*.json' -printf '%%f\\n' 2>/dev/null | sort", base() .. "/runs"))
  if not pipe then return result end; for line in pipe:lines() do result[#result + 1] = line:gsub("%.json$", "") end; pipe:close(); return result
end
function Storage.deleteRun(id)
  if not id:match("^[%w_.-]+$") then return nil, "invalid run id" end
  local path = Storage.runPath(id); local f = io.open(path, "rb"); if not f then return nil, "run not found" end; f:close(); return os.remove(path)
end
return Storage
