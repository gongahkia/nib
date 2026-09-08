local source = debug.getinfo(1, "S").source:sub(2)
local root = vim.fs.dirname(vim.fs.dirname(vim.fs.normalize(source)))
vim.opt.runtimepath:prepend(root)

local function fail(message)
  error("nvim_spec: " .. message, 0)
end

local function expect(condition, message)
  if not condition then
    fail(message)
  end
end

local function highlight(name)
  return vim.api.nvim_get_hl(0, { name = name, link = false })
end

local function hex_value(value)
  return tonumber(value:sub(2), 16)
end

expect(vim.fn.has("nvim-0.10") == 1, "Neovim 0.10 or newer is required")

vim.o.background = "light"
vim.cmd.colorscheme("nib")
local theme = require("nib")
local light = theme.get_palette("light")
expect(vim.g.colors_name == "nib", "default entry point set the wrong colors_name")
expect(vim.o.background == "light", "auto mode did not preserve the light background")
expect(highlight("Normal").fg == hex_value(light.foreground.primary), "light Normal foreground mismatch")
expect(highlight("Normal").bg == hex_value(light.background), "light Normal background mismatch")

local required_groups = {
  "Normal", "NormalNC", "NormalFloat", "FloatBorder", "WinSeparator", "Pmenu", "PmenuSel",
  "LineNr", "CursorLine", "StatusLine", "TabLine", "Search", "IncSearch", "Visual",
  "MatchParen", "Folded", "SpellBad", "DiagnosticError", "DiagnosticVirtualTextWarn",
  "DiagnosticFloatingInfo", "DiagnosticSignHint", "DiagnosticUnderlineError", "DiffAdd",
  "DiffChange", "DiffDelete", "DiffText", "QuickFixLine", "markdownH1", "helpHyperTextJump",
  "@variable", "@function.method.call", "@keyword.conditional", "@string", "@type",
  "@markup.heading.2", "@diff.plus", "@lsp.type.function", "@lsp.type.class",
  "@lsp.mod.deprecated", "@lsp.mod.readonly", "@lsp.typemod.variable.readonly",
  "@lsp.typemod.function.defaultLibrary", "TelescopeNormal", "CmpItemKindFunction",
  "GitSignsAdd", "WhichKey", "TroubleNormal", "NoicePopup", "SnacksPickerMatch",
}
for _, name in ipairs(required_groups) do
  expect(next(highlight(name)) ~= nil, "required highlight is empty: " .. name)
end

for index = 0, 15 do
  expect(vim.g["terminal_color_" .. index] == light.ansi[index + 1], "terminal color mismatch: " .. index)
end

local lualine = require("lualine.themes.nib")
expect(lualine.normal.a.bg == light.blue_ink.primary, "lualine theme is not palette-derived")

vim.api.nvim_set_hl(0, "NibStale", { fg = 0xFF00FF })
vim.cmd.colorscheme("nib-dark")
local dark = theme.get_palette("dark")
expect(vim.g.colors_name == "nib-dark", "dark entry point set the wrong colors_name")
expect(vim.o.background == "dark", "dark entry point did not select dark background")
expect(highlight("Normal").bg == hex_value(dark.background), "dark Normal background mismatch")
expect(next(highlight("NibStale")) == nil, "switching left a stale highlight")

vim.cmd.colorscheme("nib-light")
expect(vim.g.colors_name == "nib-light", "light entry point set the wrong colors_name")
expect(vim.o.background == "light", "light entry point did not select light background")

theme.setup({
  style = "dark",
  transparent = true,
  italics = false,
  terminal_colors = false,
  integrations = { telescope = false },
})
expect(vim.g.colors_name == "nib", "setup set the wrong colors_name")
expect(highlight("Normal").bg == nil, "transparent setup retained Normal background")
expect(not highlight("Comment").italic, "italics=false retained comment italics")
expect(vim.g.terminal_color_0 == nil and vim.g.terminal_color_15 == nil, "terminal colors were not disabled")
expect(next(highlight("TelescopeNormal")) == nil, "disabled integration remained defined")

local valid, message = pcall(theme.setup, { style = "sepia" })
expect(not valid and tostring(message):match("style must be"), "invalid style did not fail clearly")

theme.setup({ style = "auto" })
print("Neovim runtime: pass (" .. tostring(vim.version()) .. ")")
