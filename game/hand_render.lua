local palette = require("game.palette")
local R = {}
function R.draw(hand, cameraX, cameraY, debug)
  palette.set(14); love.graphics.ellipse("fill", math.floor(hand.x - cameraX), math.floor(hand.y - cameraY), 17, 11)
  for _, finger in ipairs(hand.fingers) do local r, j, t = finger.root, finger.joint, finger.target; palette.set(14); love.graphics.setLineWidth(3); love.graphics.line(r.x - cameraX, r.y - cameraY, j.x - cameraX, j.y - cameraY, t.x - cameraX, t.y - cameraY); palette.set(finger.loaded and 16 or 5); love.graphics.circle("fill", t.x - cameraX, t.y - cameraY, 2); if debug then palette.set(finger.reachable and 10 or 16); love.graphics.setLineWidth(1); love.graphics.circle("line", t.x - cameraX, t.y - cameraY, 3); love.graphics.points(j.x - cameraX, j.y - cameraY) end end
  love.graphics.setLineWidth(1)
end
return R
