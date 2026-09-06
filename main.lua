local render = require("game.render")

function love.load()
  render.load()
end

function love.draw()
  render.beginFrame()
  render.drawContract()
  render.endFrame()
end

function love.keypressed(key)
  if key == "escape" then love.event.quit() end
end
