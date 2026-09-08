local M = {}

local defaults = {
  style = "auto",
  transparent = false,
  italics = true,
  terminal_colors = true,
  integrations = {},
}

M.config = vim.deepcopy(defaults)

local function validate(options)
  if options.style ~= nil and not vim.tbl_contains({ "auto", "light", "dark" }, options.style) then
    error("quireveil: style must be 'auto', 'light', or 'dark'")
  end
  for _, key in ipairs({ "transparent", "italics", "terminal_colors" }) do
    if options[key] ~= nil and type(options[key]) ~= "boolean" then
      error("quireveil: " .. key .. " must be a boolean")
    end
  end
  if options.integrations ~= nil and type(options.integrations) ~= "table" then
    error("quireveil: integrations must be a table")
  end
end

local function resolve_style(requested)
  local style = requested or M.config.style
  if style == "auto" then
    style = vim.o.background
  end
  if style ~= "light" and style ~= "dark" then
    error("quireveil: resolved style must be 'light' or 'dark'")
  end
  return style
end

local function set_terminal_colors(colors)
  for index = 0, 15 do
    vim.g["terminal_color_" .. index] = colors and colors[index + 1] or nil
  end
end

function M.get_palette(style)
  local resolved = resolve_style(style)
  return require("quireveil.palette." .. resolved)
end

function M.load(style, colors_name)
  if vim.fn.has("nvim-0.10") ~= 1 then
    error("quireveil requires Neovim 0.10 or newer")
  end

  local resolved = resolve_style(style)
  if style ~= nil and style ~= "auto" then
    vim.o.background = resolved
  end
  vim.o.termguicolors = true

  vim.cmd("highlight clear")
  if vim.fn.exists("syntax_on") == 1 then
    vim.cmd("syntax reset")
  end

  local palette = require("quireveil.palette." .. resolved)
  local groups = require("quireveil.highlights").groups(palette, M.config)
  local integrations = require("quireveil.integrations").groups(palette, M.config)
  for name, definition in pairs(vim.tbl_extend("force", groups, integrations)) do
    vim.api.nvim_set_hl(0, name, definition)
  end

  if M.config.terminal_colors then
    set_terminal_colors(palette.ansi)
  else
    set_terminal_colors(nil)
  end

  vim.g.colors_name = colors_name or "quireveil"
end

function M.setup(options)
  options = options or {}
  validate(options)
  M.config = vim.tbl_deep_extend("force", vim.deepcopy(defaults), options)
  M.load(M.config.style, "quireveil")
end

return M
