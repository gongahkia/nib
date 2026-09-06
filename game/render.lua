local palette = require("game.palette")

local Render = { width = 128, height = 128 }

function Render.load()
  Render.canvas = love.graphics.newCanvas(Render.width, Render.height)
  Render.paletteCanvas = love.graphics.newCanvas(Render.width, Render.height)
  Render.canvas:setFilter("nearest", "nearest")
  Render.paletteCanvas:setFilter("nearest", "nearest")
  love.graphics.setDefaultFilter("nearest", "nearest")
  love.graphics.setLineStyle("rough")
  Render.shader = love.graphics.newShader([[
    extern vec3 colours[16];
    vec4 effect(vec4 colour, Image texture, vec2 uv, vec2 screen) {
      vec3 source = Texel(texture, uv).rgb;
      vec3 best = colours[0];
      float distance = dot(source - best, source - best);
      for (int i = 1; i < 16; i++) {
        float candidate = dot(source - colours[i], source - colours[i]);
        if (candidate < distance) { distance = candidate; best = colours[i]; }
      }
      return vec4(best, 1.0);
    }
  ]])
  local colours = {}; for i = 1, 16 do colours[i] = { palette[i][1] / 255, palette[i][2] / 255, palette[i][3] / 255 } end
  Render.shader:send("colours", unpack(colours))
end

function Render.beginFrame()
  love.graphics.setCanvas(Render.canvas)
  palette.set(1)
  love.graphics.clear(love.graphics.getColor())
end

function Render.endFrame()
  love.graphics.setCanvas(Render.paletteCanvas)
  love.graphics.setShader(Render.shader)
  palette.set(9)
  love.graphics.draw(Render.canvas, 0, 0)
  love.graphics.setShader()
  love.graphics.setCanvas()
  local w, h = love.graphics.getDimensions()
  local scale = math.max(1, math.floor(math.min(w / Render.width, h / Render.height)))
  local x, y = math.floor((w - Render.width * scale) / 2), math.floor((h - Render.height * scale) / 2)
  palette.set(9)
  love.graphics.draw(Render.paletteCanvas, x, y, 0, scale, scale)
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
      if solid.slope then
        love.graphics.polygon("fill", solid.x - cameraX, solid.y - cameraY, solid.x + solid.w - cameraX, solid.y + solid.slope - cameraY, solid.x + solid.w - cameraX, solid.y + solid.h - cameraY, solid.x - cameraX, solid.y + solid.h - cameraY)
      else love.graphics.rectangle("fill", math.floor(solid.x - cameraX), math.floor(solid.y - cameraY), solid.w, solid.h) end
      palette.set(7)
      love.graphics.line(math.floor(solid.x - cameraX), math.floor(solid.y - cameraY), math.floor(solid.x + solid.w - cameraX), math.floor(solid.y + (solid.slope or 0) - cameraY))
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

function Render.drawLife(level, cameraX, cameraY, collected)
  for _, human in ipairs(level.inhabitants) do
    if not human.hidden then palette.set(9); love.graphics.rectangle("fill", human.x - cameraX, human.y - 6 - cameraY, 2, 6); if human.active then palette.set(16); love.graphics.points(human.x - cameraX - 1, human.y - 8 - cameraY) end end
  end
  for _, machine in ipairs(level.machines) do palette.set(10); love.graphics.rectangle("line", machine.x - 3 - cameraX, machine.y - 7 - cameraY, 6, 7); love.graphics.points(machine.x - cameraX, machine.y - 4 - cameraY) end
  for _, hazard in ipairs(level.hazards) do palette.set(16); love.graphics.polygon("fill", hazard.x - 3 - cameraX, hazard.y - cameraY, hazard.x - cameraX, hazard.y - 5 - cameraY, hazard.x + 3 - cameraX, hazard.y - cameraY) end
  for _, collectible in ipairs(level.collectibles) do if not collected[collectible.id] then palette.set(7); love.graphics.polygon("line", collectible.x - cameraX, collectible.y - 3 - cameraY, collectible.x + 3 - cameraX, collectible.y - cameraY, collectible.x - cameraX, collectible.y + 3 - cameraY, collectible.x - 3 - cameraX, collectible.y - cameraY) end end
  palette.set(7); love.graphics.rectangle("line", level.exit.x - cameraX, level.exit.y - 18 - cameraY, 8, 18)
end

function Render.drawGenerationDebug(level, world, cameraX, cameraY)
  for _, solid in ipairs(world.solids) do if not solid.destroyed then palette.set(world.materials[solid.material].destructible and 16 or 10); love.graphics.rectangle("line", solid.x - cameraX, solid.y - cameraY, solid.w, solid.h) end end
  for _, link in ipairs(level.links) do local a, b = level.nodes[link.from], level.nodes[link.to]; if a and b then palette.set(10, 0.7); love.graphics.line(a.x - cameraX, a.y - cameraY - 2, b.x - cameraX, b.y - cameraY - 2) end end
  for _, n in ipairs(level.nodes) do palette.set(level.validation.nodeReachable[n.id] and 7 or 16); love.graphics.points(n.x - cameraX, n.y - cameraY - 2) end
end

return Render
