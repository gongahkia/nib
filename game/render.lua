local palette = require("game.palette")

local Render = { width = 128, height = 128 }

function Render.load()
  Render.canvas = love.graphics.newCanvas(Render.width, Render.height)
  Render.canvas:setFilter("nearest", "nearest")
  love.graphics.setDefaultFilter("nearest", "nearest")
end

function Render.beginFrame()
  love.graphics.setCanvas(Render.canvas)
  palette.set(1)
  love.graphics.clear(love.graphics.getColor())
end

function Render.endFrame()
  love.graphics.setCanvas()
  local w, h = love.graphics.getDimensions()
  local scale = math.max(1, math.floor(math.min(w / Render.width, h / Render.height)))
  local x, y = math.floor((w - Render.width * scale) / 2), math.floor((h - Render.height * scale) / 2)
  palette.set(9)
  love.graphics.draw(Render.canvas, x, y, 0, scale, scale)
end

function Render.drawContract()
  for layer = 1, 4 do
    palette.set(1 + layer)
    local y = 18 + layer * 12
    for x = -16, 144, 12 do
      love.graphics.rectangle("fill", x - layer * 3, y, 8, 18 + layer * 5)
    end
  end
  palette.set(12)
  love.graphics.rectangle("fill", 0, 97, 128, 31)
  palette.set(7)
  love.graphics.rectangle("fill", 12, 92, 104, 5)
  palette.set(16)
  love.graphics.circle("fill", 35, 87, 3)
  palette.set(9)
  love.graphics.print("PALIMPSEST RUN", 25, 38)
  palette.set(7)
  love.graphics.print("PROVISIONAL", 37, 47)
end

return Render
