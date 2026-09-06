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

function Render.drawWorld(world, cameraX, cameraY)
  -- Distant architecture: palette-only ordered dither and slow parallax.
  for layer = 1, 3 do
    palette.set(1 + layer)
    local offset = math.floor(cameraX * layer * 0.08) % 17
    for x = -20, 148, 17 do
      local h = 22 + ((x + layer * 13) % 25)
      love.graphics.rectangle("fill", x - offset, 91 - h - layer * 7, 9 + layer, h)
    end
  end
  for _, solid in ipairs(world.solids) do
    if not solid.destroyed then
      palette.set(world.materials[solid.material] and world.materials[solid.material].colour or 5)
      love.graphics.rectangle("fill", math.floor(solid.x - cameraX), math.floor(solid.y - cameraY), solid.w, solid.h)
      palette.set(7)
      love.graphics.line(math.floor(solid.x - cameraX), math.floor(solid.y - cameraY), math.floor(solid.x + solid.w - cameraX), math.floor(solid.y - cameraY))
    end
  end
  for _, anchor in ipairs(world.anchors) do
    palette.set(7); love.graphics.circle("line", math.floor(anchor.x - cameraX), math.floor(anchor.y - cameraY), 2)
  end
  for _, rail in ipairs(world.rails) do
    palette.set(10); love.graphics.line(rail.x1 - cameraX, rail.y1 - cameraY, rail.x2 - cameraX, rail.y2 - cameraY)
  end
  for _, object in ipairs(world.objects) do palette.set(7); love.graphics.rectangle("fill", object.x - object.w / 2 - cameraX, object.y - object.h - cameraY, object.w, object.h) end
  for _, enemy in ipairs(world.enemies) do if enemy.alive then palette.set(16); love.graphics.rectangle("fill", enemy.x - 3 - cameraX, enemy.y - 6 - cameraY, 6, 6); palette.set(1); love.graphics.points(enemy.x - 1 - cameraX, enemy.y - 4 - cameraY) end end
end

return Render
