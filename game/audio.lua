local config = require("game.config").audio
local Audio = { sources = {}, enabled = false }

local function synth(spec)
  local rate, count = 22050, math.floor(22050 * spec.duration)
  local data = love.sound.newSoundData(count, rate, 16, 1)
  for i = 0, count - 1 do
    local t, envelope = i / rate, 1 - i / count
    local square = math.sin(t * spec.frequency * math.pi * 2) >= 0 and 1 or -1
    local noise = ((i * 1103515245 + 12345) % 65536) / 32768 - 1
    data:setSample(i, (square * 0.65 + noise * 0.2) * envelope * 0.22)
  end
  return love.audio.newSource(data, "static")
end

function Audio.load()
  for name, spec in pairs(config) do Audio.sources[name] = synth(spec) end
  Audio.enabled = true
end

function Audio.play(name, pitch)
  if not Audio.enabled or not Audio.sources[name] then return end
  local source = Audio.sources[name]:clone(); source:setPitch(pitch or 1); source:play()
end

return Audio
