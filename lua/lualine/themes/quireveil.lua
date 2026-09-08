local palette = require("quireveil").get_palette("auto")

local normal = {
  a = { fg = palette.cursor.foreground, bg = palette.blue_ink.primary, gui = "bold" },
  b = { fg = palette.foreground.primary, bg = palette.surface.subtle },
  c = { fg = palette.foreground.secondary, bg = palette.surface.elevated },
}

return {
  normal = normal,
  insert = {
    a = { fg = palette.background, bg = palette.moss.primary, gui = "bold" },
    b = normal.b,
    c = normal.c,
  },
  visual = {
    a = { fg = palette.background, bg = palette.violet, gui = "bold" },
    b = normal.b,
    c = normal.c,
  },
  replace = {
    a = { fg = palette.background, bg = palette.burgundy, gui = "bold" },
    b = normal.b,
    c = normal.c,
  },
  command = {
    a = { fg = palette.background, bg = palette.amber, gui = "bold" },
    b = normal.b,
    c = normal.c,
  },
  terminal = {
    a = { fg = palette.background, bg = palette.teal.primary, gui = "bold" },
    b = normal.b,
    c = normal.c,
  },
  inactive = {
    a = { fg = palette.foreground.muted, bg = palette.surface.elevated },
    b = { fg = palette.foreground.muted, bg = palette.surface.elevated },
    c = { fg = palette.foreground.disabled, bg = palette.surface.elevated },
  },
}
