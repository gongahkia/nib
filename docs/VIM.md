# Vim guide

Nib provides generated `nib`, `nib-light`, and `nib-dark` Vim colorschemes in
`vim/colors/`. They cover the built-in interface and syntax groups, spelling,
search, selections, diff surfaces, and Vim terminal ANSI colors.

## Install

Copy the generated files into Vim's user colors directory:

```sh
mkdir -p ~/.vim/colors
cp vim/colors/*.vim ~/.vim/colors/
```

On Windows, use `%USERPROFILE%\vimfiles\colors` instead. To develop directly
from a checkout, add only the repository's `vim/` subdirectory to
`runtimepath`; keeping it separate avoids colliding with Nib's Neovim runtime.

## Select a variant

For the automatic entry point, set Vim's background before loading Nib:

```vim
set termguicolors
set background=dark
colorscheme nib
```

Use `background=light` for the light variant. The explicit entry points ignore
the prior mode and can be loaded directly:

```vim
colorscheme nib-light
colorscheme nib-dark
```

If `background` changes later, run `colorscheme nib` again. Exact Nib colors
require truecolor; `g:terminal_ansi_colors` still supplies the canonical
16-color terminal palette.

## Remove

Delete only `nib.vim`, `nib-light.vim`, and `nib-dark.vim` from the user colors
directory, then select another colorscheme.
