local M = {}

local function enabled(options, name)
  return options.integrations[name] ~= false
end

local function add(target, definitions)
  for name, definition in pairs(definitions) do
    target[name] = definition
  end
end

local function telescope(palette)
  return {
    TelescopeNormal = { fg = palette.foreground.primary, bg = palette.surface.floating },
    TelescopePromptNormal = { fg = palette.foreground.primary, bg = palette.surface.elevated },
    TelescopeResultsNormal = { link = "TelescopeNormal" },
    TelescopePreviewNormal = { link = "TelescopeNormal" },
    TelescopeBorder = { fg = palette.border.default, bg = palette.surface.floating },
    TelescopePromptBorder = { fg = palette.border.focus, bg = palette.surface.elevated },
    TelescopeResultsBorder = { link = "TelescopeBorder" },
    TelescopePreviewBorder = { link = "TelescopeBorder" },
    TelescopeSelection = { fg = palette.selection.foreground, bg = palette.selection.background },
    TelescopeSelectionCaret = { fg = palette.rust, bg = palette.selection.background, bold = true },
    TelescopeMultiSelection = { fg = palette.violet, bold = true },
    TelescopeMultiIcon = { fg = palette.violet },
    TelescopeMatching = { fg = palette.blue_ink.bright, bold = true },
    TelescopePromptPrefix = { fg = palette.moss.primary, bold = true },
    TelescopePromptTitle = { fg = palette.background, bg = palette.blue_ink.primary, bold = true },
    TelescopeResultsTitle = { fg = palette.background, bg = palette.moss.primary, bold = true },
    TelescopePreviewTitle = { fg = palette.background, bg = palette.teal.primary, bold = true },
  }
end

local function cmp(palette)
  return {
    CmpItemAbbr = { fg = palette.foreground.secondary },
    CmpItemAbbrDeprecated = { fg = palette.foreground.disabled, strikethrough = true },
    CmpItemAbbrMatch = { fg = palette.blue_ink.bright, bold = true },
    CmpItemAbbrMatchFuzzy = { fg = palette.teal.primary, bold = true },
    CmpItemMenu = { fg = palette.foreground.muted },
    CmpItemKind = { fg = palette.graphite },
    CmpItemKindText = { fg = palette.foreground.secondary },
    CmpItemKindMethod = { fg = palette.blue_ink.bright },
    CmpItemKindFunction = { fg = palette.blue_ink.bright },
    CmpItemKindConstructor = { fg = palette.violet },
    CmpItemKindField = { fg = palette.foreground.secondary },
    CmpItemKindVariable = { fg = palette.foreground.primary },
    CmpItemKindClass = { fg = palette.moss.primary },
    CmpItemKindInterface = { fg = palette.moss.primary },
    CmpItemKindModule = { fg = palette.moss.muted },
    CmpItemKindProperty = { fg = palette.sepia },
    CmpItemKindUnit = { fg = palette.amber },
    CmpItemKindValue = { fg = palette.amber },
    CmpItemKindEnum = { fg = palette.moss.primary },
    CmpItemKindKeyword = { fg = palette.blue_ink.deep },
    CmpItemKindSnippet = { fg = palette.rust },
    CmpItemKindColor = { fg = palette.violet },
    CmpItemKindFile = { fg = palette.blue_ink.primary },
    CmpItemKindReference = { fg = palette.teal.primary },
    CmpItemKindFolder = { fg = palette.blue_ink.primary },
    CmpItemKindEnumMember = { fg = palette.amber },
    CmpItemKindConstant = { fg = palette.amber },
    CmpItemKindStruct = { fg = palette.moss.primary },
    CmpItemKindEvent = { fg = palette.burgundy },
    CmpItemKindOperator = { fg = palette.graphite },
    CmpItemKindTypeParameter = { fg = palette.moss.muted },
  }
end

local function gitsigns(palette)
  return {
    GitSignsAdd = { fg = palette.diagnostic.success },
    GitSignsChange = { fg = palette.diagnostic.information },
    GitSignsDelete = { fg = palette.diagnostic.error },
    GitSignsChangedelete = { fg = palette.violet },
    GitSignsTopdelete = { fg = palette.diagnostic.error },
    GitSignsUntracked = { fg = palette.moss.muted },
    GitSignsAddNr = { link = "GitSignsAdd" },
    GitSignsChangeNr = { link = "GitSignsChange" },
    GitSignsDeleteNr = { link = "GitSignsDelete" },
    GitSignsAddLn = { bg = palette.diff.add },
    GitSignsChangeLn = { bg = palette.diff.change },
    GitSignsAddPreview = { fg = palette.diff.foreground, bg = palette.diff.add },
    GitSignsDeletePreview = { fg = palette.diff.foreground, bg = palette.diff.delete },
    GitSignsCurrentLineBlame = { fg = palette.foreground.muted },
    GitSignsAddInline = { bg = palette.diff.add_text },
    GitSignsChangeInline = { bg = palette.diff.change_text },
    GitSignsDeleteInline = { bg = palette.diff.delete_text },
  }
end

local function which_key(palette)
  return {
    WhichKey = { fg = palette.blue_ink.bright, bold = true },
    WhichKeySeparator = { fg = palette.border.default },
    WhichKeyGroup = { fg = palette.moss.primary, bold = true },
    WhichKeyDesc = { fg = palette.foreground.secondary },
    WhichKeyNormal = { link = "NormalFloat" },
    WhichKeyTitle = { link = "FloatTitle" },
    WhichKeyBorder = { link = "FloatBorder" },
    WhichKeyValue = { fg = palette.foreground.muted },
    WhichKeyIcon = { fg = palette.teal.primary },
    WhichKeyIconAzure = { fg = palette.blue_ink.bright },
    WhichKeyIconBlue = { fg = palette.blue_ink.primary },
    WhichKeyIconCyan = { fg = palette.teal.primary },
    WhichKeyIconGreen = { fg = palette.moss.primary },
    WhichKeyIconGrey = { fg = palette.graphite },
    WhichKeyIconOrange = { fg = palette.rust },
    WhichKeyIconPurple = { fg = palette.violet },
    WhichKeyIconRed = { fg = palette.burgundy },
    WhichKeyIconYellow = { fg = palette.amber },
  }
end

local function trouble()
  return {
    TroubleNormal = { link = "NormalFloat" },
    TroubleNormalNC = { link = "NormalFloat" },
    TroubleText = { link = "Normal" },
    TroublePreview = { link = "Visual" },
    TroubleFilename = { link = "Directory" },
    TroubleBasename = { link = "TroubleFilename" },
    TroubleDirectory = { link = "Directory" },
    TroubleIconDirectory = { link = "Special" },
    TroubleSource = { link = "Comment" },
    TroubleCode = { link = "Special" },
    TroublePos = { link = "LineNr" },
    TroubleCount = { link = "TabLineSel" },
    TroubleIndent = { link = "LineNr" },
    TroubleIndentFoldClosed = { link = "CursorLineNr" },
    TroubleIndentFoldOpen = { link = "TroubleIndent" },
  }
end

local function noice()
  return {
    NoiceCmdline = { link = "MsgArea" },
    NoiceCmdlineIcon = { link = "DiagnosticInfo" },
    NoiceCmdlineIconSearch = { link = "DiagnosticWarn" },
    NoiceCmdlinePrompt = { link = "Title" },
    NoiceCmdlinePopup = { link = "NormalFloat" },
    NoiceCmdlinePopupBorder = { link = "FloatBorder" },
    NoiceCmdlinePopupBorderSearch = { link = "DiagnosticWarn" },
    NoiceCmdlinePopupTitle = { link = "FloatTitle" },
    NoiceConfirm = { link = "NormalFloat" },
    NoiceConfirmBorder = { link = "FloatBorder" },
    NoiceCursor = { link = "Cursor" },
    NoiceMini = { link = "MsgArea" },
    NoicePopup = { link = "NormalFloat" },
    NoicePopupBorder = { link = "FloatBorder" },
    NoicePopupmenu = { link = "Pmenu" },
    NoicePopupmenuBorder = { link = "FloatBorder" },
    NoicePopupmenuMatch = { link = "PmenuMatch" },
    NoicePopupmenuSelected = { link = "PmenuSel" },
    NoiceScrollbar = { link = "PmenuSbar" },
    NoiceScrollbarThumb = { link = "PmenuThumb" },
    NoiceFormatProgressDone = { link = "Search" },
    NoiceFormatProgressTodo = { link = "CursorLine" },
    NoiceLspProgressSpinner = { link = "Constant" },
    NoiceLspProgressTitle = { link = "NonText" },
    NoiceLspProgressClient = { link = "Title" },
  }
end

local function snacks()
  return {
    SnacksPickerMatch = { link = "PmenuMatch" },
    SnacksPickerSearch = { link = "Search" },
    SnacksPickerPrompt = { fg = "fg", bold = true },
    SnacksPickerInputSearch = { link = "@keyword" },
    SnacksPickerSpecial = { link = "Special" },
    SnacksPickerLabel = { link = "SnacksPickerSpecial" },
    SnacksPickerTotals = { link = "NonText" },
    SnacksPickerDirectory = { link = "Directory" },
    SnacksPickerPathIgnored = { link = "NonText" },
    SnacksPickerPathHidden = { link = "NonText" },
    SnacksPickerDir = { link = "NonText" },
    SnacksPickerToggle = { link = "DiagnosticVirtualTextInfo" },
    SnacksPickerRow = { link = "String" },
    SnacksPickerCol = { link = "LineNr" },
    SnacksPickerGitStatusAdded = { link = "Added" },
    SnacksPickerGitStatusModified = { link = "Changed" },
    SnacksPickerGitStatusDeleted = { link = "Removed" },
    SnacksPickerGitStatusUnmerged = { link = "DiagnosticError" },
    SnacksPickerGitStatusStaged = { link = "DiagnosticHint" },
    SnacksPickerPickWin = { link = "Search" },
    SnacksPickerPickWinCurrent = { link = "CurSearch" },
  }
end

function M.groups(palette, options)
  local groups = {}
  local integrations = {
    telescope = telescope,
    cmp = cmp,
    gitsigns = gitsigns,
    which_key = which_key,
    trouble = trouble,
    noice = noice,
    snacks = snacks,
  }
  for name, factory in pairs(integrations) do
    if enabled(options, name) then
      add(groups, factory(palette))
    end
  end
  return groups
end

return M
