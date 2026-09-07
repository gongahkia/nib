-- Generates Summing's original one-tile Ashen Pilgrim comparison pack.
-- The MCP client replaces the output token with an absolute .aseprite path.

local output = "__SUMMING_OUTPUT__"
local width = 24
local height = 24
local spr = Sprite(width, height, ColorMode.RGB)
local layer = spr.layers[1]
layer.name = "ashen pilgrim"

local rgba = app.pixelColor.rgba
local colors = {
  ink = rgba(21, 24, 35, 255),
  void = rgba(10, 13, 20, 255),
  stone = rgba(64, 58, 72, 255),
  light = rgba(205, 185, 138, 255),
  oxide = rgba(159, 87, 74, 255),
  blood = rgba(184, 73, 67, 255),
  gold = rgba(216, 184, 102, 255),
  cyan = rgba(143, 202, 196, 255),
}

local function pixel(image, x, y, color)
  if x >= 0 and x < width and y >= 0 and y < height then image:putPixel(x, y, color) end
end

local function rect(image, x, y, w, h, color)
  for py = y, y + h - 1 do
    for px = x, x + w - 1 do pixel(image, px, py, color) end
  end
end

local function line(image, x0, y0, x1, y1, color, thickness)
  local dx = math.abs(x1 - x0)
  local sx = x0 < x1 and 1 or -1
  local dy = -math.abs(y1 - y0)
  local sy = y0 < y1 and 1 or -1
  local err = dx + dy
  while true do
    local radius = math.floor((thickness or 1) / 2)
    for oy = -radius, radius do
      for ox = -radius, radius do pixel(image, x0 + ox, y0 + oy, color) end
    end
    if x0 == x1 and y0 == y1 then break end
    local e2 = 2 * err
    if e2 >= dy then err = err + dy; x0 = x0 + sx end
    if e2 <= dx then err = err + dx; y0 = y0 + sy end
  end
end

local function outlined_line(image, x0, y0, x1, y1, color)
  line(image, x0, y0, x1, y1, colors.ink, 3)
  line(image, x0, y0, x1, y1, color, 1)
end

local function scarf(image, x, y, direction, lift)
  local tail = x - direction * 6
  outlined_line(image, x, y, tail, y + (lift or 2), colors.oxide)
  pixel(image, tail - direction, y + (lift or 2), colors.gold)
end

local function pilgrim(image, pose)
  local x = pose.x or 0
  local y = pose.y or 0
  local body_y = 10 + y
  local head_y = 3 + y
  local scarf_direction = pose.scarf_direction or 1
  scarf(image, 9 + x, 8 + y, scarf_direction, pose.scarf_lift or 2)

  local left_foot = pose.left_foot or { 9 + x, 22 }
  local right_foot = pose.right_foot or { 15 + x, 22 }
  outlined_line(image, 10 + x, body_y + 6, left_foot[1], left_foot[2], colors.stone)
  outlined_line(image, 14 + x, body_y + 6, right_foot[1], right_foot[2], colors.light)

  rect(image, 7 + x, body_y - 1, 10, 9, colors.ink)
  rect(image, 8 + x, body_y, 8, 6, colors.stone)
  rect(image, 9 + x, body_y, 6, 2, colors.light)
  rect(image, 8 + x, body_y + 6, 3, 2, colors.oxide)
  rect(image, 13 + x, body_y + 6, 3, 2, colors.oxide)

  local left_hand = pose.left_hand or { 5 + x, 16 + y }
  local right_hand = pose.right_hand or { 19 + x, 15 + y }
  outlined_line(image, 8 + x, body_y + 1, left_hand[1], left_hand[2], colors.light)
  outlined_line(image, 15 + x, body_y + 1, right_hand[1], right_hand[2], colors.stone)

  rect(image, 8 + x, head_y, 8, 8, colors.ink)
  rect(image, 9 + x, head_y + 1, 6, 5, colors.light)
  rect(image, 10 + x, head_y + 4, 5, 3, colors.void)
  pixel(image, 13 + x, head_y + 4, colors.cyan)
  pixel(image, 14 + x, head_y + 5, colors.gold)
  rect(image, 7 + x, head_y + 2, 2, 4, colors.stone)
  pixel(image, 11 + x, head_y, colors.gold)
end

local function low_pilgrim(image, kind)
  if kind == "slide" then
    scarf(image, 8, 17, 1, -2)
    outlined_line(image, 13, 19, 21, 21, colors.stone)
    outlined_line(image, 11, 20, 17, 22, colors.light)
    rect(image, 6, 15, 10, 7, colors.ink)
    rect(image, 7, 16, 8, 4, colors.stone)
    rect(image, 2, 15, 6, 6, colors.ink)
    rect(image, 3, 16, 5, 3, colors.light)
    rect(image, 4, 18, 4, 2, colors.void)
    pixel(image, 6, 18, colors.cyan)
    line(image, 18, 23, 22, 23, colors.oxide, 1)
    return
  end

  scarf(image, 9, 15, 1, 1)
  rect(image, 6, 13, 11, 9, colors.ink)
  rect(image, 7, 14, 9, 6, colors.stone)
  rect(image, 8, 14, 6, 2, colors.light)
  rect(image, 8, 9, 8, 7, colors.ink)
  rect(image, 9, 10, 6, 4, colors.light)
  rect(image, 10, 12, 5, 3, colors.void)
  pixel(image, 13, 12, colors.cyan)
  if kind == "crawl" then
    outlined_line(image, 7, 20, 3, 22, colors.light)
    outlined_line(image, 15, 20, 20, 22, colors.stone)
  else
    outlined_line(image, 9, 20, 8, 22, colors.light)
    outlined_line(image, 14, 20, 16, 22, colors.stone)
  end
end

local function ledge(image)
  pilgrim(image, {
    y = 1,
    left_hand = { 8, 2 }, right_hand = { 15, 2 },
    left_foot = { 8, 22 }, right_foot = { 16, 20 },
    scarf_lift = 4,
  })
  rect(image, 7, 1, 10, 2, colors.gold)
end

local function wall_cling(image)
  pilgrim(image, {
    x = 2,
    left_hand = { 19, 9 }, right_hand = { 20, 13 },
    left_foot = { 18, 20 }, right_foot = { 19, 22 },
    scarf_direction = -1, scarf_lift = 3,
  })
  rect(image, 22, 4, 2, 19, colors.stone)
  pixel(image, 21, 8, colors.gold)
  pixel(image, 21, 18, colors.gold)
end

local function dash(image)
  scarf(image, 9, 9, 1, 0)
  scarf(image, 8, 11, 1, 1)
  outlined_line(image, 14, 16, 21, 17, colors.light)
  outlined_line(image, 12, 18, 19, 21, colors.stone)
  rect(image, 6, 8, 11, 10, colors.ink)
  rect(image, 7, 9, 9, 7, colors.stone)
  rect(image, 12, 5, 8, 8, colors.ink)
  rect(image, 13, 6, 6, 5, colors.light)
  rect(image, 15, 8, 5, 3, colors.void)
  pixel(image, 18, 8, colors.cyan)
  line(image, 1, 13, 5, 13, colors.cyan, 1)
  line(image, 2, 17, 6, 17, colors.oxide, 1)
end

local function rope(image)
  line(image, 12, 0, 12, 23, colors.gold, 1)
  pilgrim(image, {
    left_hand = { 11, 8 }, right_hand = { 13, 11 },
    left_foot = { 10, 22 }, right_foot = { 14, 20 },
    scarf_direction = -1, scarf_lift = 3,
  })
end

local function tool(image, raised)
  pilgrim(image, {
    left_hand = raised and { 16, 7 } or { 17, 12 },
    right_hand = raised and { 18, 8 } or { 19, 14 },
    scarf_lift = raised and 1 or 3,
  })
  if raised then
    outlined_line(image, 17, 8, 20, 3, colors.gold)
    line(image, 17, 3, 22, 6, colors.light, 2)
  else
    outlined_line(image, 18, 13, 21, 18, colors.gold)
    line(image, 18, 19, 23, 17, colors.light, 2)
    pixel(image, 22, 21, colors.oxide)
    pixel(image, 20, 22, colors.gold)
  end
end

local function bomb(image)
  pilgrim(image, { right_hand = { 19, 11 }, scarf_lift = 1 })
  rect(image, 18, 8, 5, 5, colors.ink)
  rect(image, 19, 9, 3, 3, colors.blood)
  pixel(image, 21, 7, colors.gold)
end

local function hurt(image)
  pilgrim(image, {
    x = 1, y = 1,
    left_hand = { 3, 10 }, right_hand = { 18, 18 },
    left_foot = { 8, 22 }, right_foot = { 18, 22 },
    scarf_direction = -1, scarf_lift = 0,
  })
  line(image, 2, 4, 5, 7, colors.blood, 1)
  line(image, 3, 8, 0, 10, colors.blood, 1)
end

local function death(image)
  scarf(image, 9, 20, -1, 1)
  outlined_line(image, 8, 21, 3, 22, colors.light)
  outlined_line(image, 14, 21, 21, 22, colors.stone)
  rect(image, 7, 16, 10, 6, colors.ink)
  rect(image, 8, 17, 8, 3, colors.stone)
  rect(image, 14, 14, 7, 7, colors.ink)
  rect(image, 15, 15, 5, 4, colors.light)
  rect(image, 16, 17, 4, 3, colors.void)
  pixel(image, 18, 17, colors.blood)
  line(image, 2, 23, 22, 23, colors.oxide, 1)
end

local drawers = {
  function(i) pilgrim(i, {}) end,
  function(i) pilgrim(i, { y = 1, scarf_lift = 1 }) end,
  function(i) pilgrim(i, { left_hand = { 5, 12 }, right_hand = { 18, 17 }, left_foot = { 6, 22 }, right_foot = { 18, 21 }, scarf_lift = 3 }) end,
  function(i) pilgrim(i, { left_hand = { 7, 17 }, right_hand = { 19, 11 }, left_foot = { 11, 22 }, right_foot = { 19, 22 }, scarf_lift = 2 }) end,
  function(i) pilgrim(i, { left_hand = { 5, 16 }, right_hand = { 18, 10 }, left_foot = { 17, 22 }, right_foot = { 8, 21 }, scarf_lift = 1 }) end,
  function(i) pilgrim(i, { left_hand = { 7, 10 }, right_hand = { 20, 16 }, left_foot = { 19, 22 }, right_foot = { 10, 22 }, scarf_lift = 3 }) end,
  function(i) pilgrim(i, { y = 2, left_hand = { 5, 17 }, right_hand = { 19, 17 }, left_foot = { 8, 22 }, right_foot = { 16, 22 }, scarf_lift = 0 }) end,
  function(i) pilgrim(i, { y = -1, left_hand = { 6, 9 }, right_hand = { 18, 8 }, left_foot = { 8, 21 }, right_foot = { 17, 20 }, scarf_lift = 4 }) end,
  function(i) pilgrim(i, { y = -2, left_hand = { 5, 9 }, right_hand = { 19, 9 }, left_foot = { 9, 21 }, right_foot = { 15, 21 }, scarf_lift = 2 }) end,
  function(i) pilgrim(i, { left_hand = { 5, 8 }, right_hand = { 19, 8 }, left_foot = { 7, 21 }, right_foot = { 17, 21 }, scarf_lift = 5 }) end,
  function(i) low_pilgrim(i, "crouch") end,
  function(i) low_pilgrim(i, "crouch") end,
  function(i) low_pilgrim(i, "crawl") end,
  function(i) low_pilgrim(i, "slide") end,
  ledge,
  function(i) pilgrim(i, { y = 2, left_hand = { 7, 7 }, right_hand = { 17, 5 }, left_foot = { 7, 22 }, right_foot = { 18, 21 }, scarf_lift = 1 }) end,
  wall_cling,
  function(i) pilgrim(i, { y = -1, left_hand = { 5, 8 }, right_hand = { 17, 13 }, left_foot = { 5, 20 }, right_foot = { 15, 22 }, scarf_direction = -1, scarf_lift = 4 }) end,
  dash,
  rope,
  function(i) tool(i, true) end,
  function(i) tool(i, false) end,
  bomb,
  hurt,
  death,
}

app.transaction(function()
  for index, draw in ipairs(drawers) do
    local frame
    if index == 1 then
      frame = spr.frames[1]
    else
      frame = spr:newEmptyFrame()
    end
    local image = Image(spr.spec)
    image:clear()
    draw(image)
    local cel = layer:cel(frame)
    if cel then cel.image = image else spr:newCel(layer, frame, image, Point(0, 0)) end
    frame.duration = index >= 3 and index <= 6 and 0.08 or 0.12
  end
end)

spr:saveAs(output)
print("OK:" .. output .. ":frames=" .. #spr.frames)
