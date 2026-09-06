local Input = {}
Input.__index = Input

Input.defaults = {
  left = { "a", "left" }, right = { "d", "right" }, up = { "w", "up" }, down = { "s", "down" },
  jump = { "space", "z" }, dash = { "lshift", "x" }, slide = { "c", "lctrl" },
  dive = { "v" }, grapple = { "e" }, tool = { "q" }, pull = { "r" }, debug = { "f3" }, reload = { "f5" },
}

function Input.new(bindings)
  return setmetatable({ bindings = bindings or Input.defaults, pressed = {}, held = {}, mouseAim = nil }, Input)
end

function Input:keypressed(key)
  for action, keys in pairs(self.bindings) do
    for _, bound in ipairs(keys) do if bound == key then self.pressed[action], self.held[action] = true, true end end
  end
end

function Input:keyreleased(key)
  for action, keys in pairs(self.bindings) do
    for _, bound in ipairs(keys) do if bound == key then self.held[action] = false end end
  end
end

function Input:snapshot(mouseWorld)
  local s = {}
  for action in pairs(self.bindings) do s[action] = self.held[action] or false; s[action .. "Pressed"] = self.pressed[action] or false end
  s.moveX = (s.right and 1 or 0) - (s.left and 1 or 0)
  s.moveY = (s.down and 1 or 0) - (s.up and 1 or 0)
  if mouseWorld then
    s.aimX, s.aimY, s.mouseAim = mouseWorld.x, mouseWorld.y, true
  else
    local ax, ay = s.moveX, s.moveY
    if ax == 0 and ay == 0 then ax = 1 end
    local length = math.sqrt(ax * ax + ay * ay)
    s.aimDirX, s.aimDirY = ax / length, ay / length
  end
  self.pressed = {}
  return s
end

function Input.fromGamepad(joystick, pressed)
  local x, y = joystick:getGamepadAxis("leftx"), joystick:getGamepadAxis("lefty")
  local ax, ay = joystick:getGamepadAxis("rightx"), joystick:getGamepadAxis("righty")
  local dead = 0.28
  x, y = math.abs(x) > dead and x or 0, math.abs(y) > dead and y or 0
  ax, ay = math.abs(ax) > dead and ax or 0, math.abs(ay) > dead and ay or 0
  if ax == 0 and ay == 0 then ax, ay = x, y end
  if ax == 0 and ay == 0 then ax = 1 end
  local length = math.sqrt(ax * ax + ay * ay)
  return {
    moveX = x, moveY = y, aimDirX = ax / length, aimDirY = ay / length,
    jump = joystick:isGamepadDown("a"), dash = joystick:isGamepadDown("x"),
    slide = joystick:isGamepadDown("b"), dive = joystick:isGamepadDown("dpdown"),
    grapple = joystick:isGamepadDown("rightshoulder"), tool = joystick:isGamepadDown("leftshoulder"),
    jumpPressed = pressed and pressed.a, dashPressed = pressed and pressed.x,
    slidePressed = pressed and pressed.b, divePressed = pressed and pressed.dpdown,
    grapplePressed = pressed and pressed.rightshoulder, toolPressed = pressed and pressed.leftshoulder,
  }
end

return Input
