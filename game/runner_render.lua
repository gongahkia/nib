local palette = require("game.palette")
local animation = require("game.animation")
local R = {}

local function line(x1, y1, x2, y2, colour)
  palette.set(colour); love.graphics.line(math.floor(x1 + 0.5), math.floor(y1 + 0.5), math.floor(x2 + 0.5), math.floor(y2 + 0.5))
end

function R.draw(player, cameraX, cameraY)
  local x, y = math.floor(player.x - cameraX), math.floor(player.y - cameraY)
  local pose, f = animation.pose(player.state, player.stateTicks, player.facing), player.facing
  love.graphics.push(); love.graphics.translate(x, y - pose.crouch); love.graphics.rotate(pose.lean * 0.045)
  palette.set(9); love.graphics.rectangle("fill", -2, -8, 4, 5); love.graphics.rectangle("fill", -2, -11, 4, 3)
  line(0, -7, f * pose.arm, -4, 10); line(0, -7, -f * pose.arm, -5, 10)
  line(-1, -3, f * pose.leg, 0, 9); line(1, -3, -f * pose.leg, 0, 9)
  palette.set(16); love.graphics.rectangle("fill", -f * 3, -8, 2, 1)
  love.graphics.pop()
  if player.grapple then palette.set(7); love.graphics.line(x, y - 6, player.grapple.x - cameraX, player.grapple.y - cameraY) end
end

return R
