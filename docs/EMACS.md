# Emacs guide

Nib ships two dependency-free custom themes generated from the canonical
palette:

- `emacs/nib-light-theme.el`
- `emacs/nib-dark-theme.el`

They target GNU Emacs 27.1 or newer. The themes cover core faces, font-lock,
search, selection, matching delimiters, line numbers, mode lines, completion,
diffs, Flymake, compilation, Dired, Org, tab bars, and useful optional faces
for Markdown Mode, Flycheck, Company, Corfu, Vertico, Consult, and Magit.
Optional-package faces do not add runtime dependencies.

## Install

Copy or symlink both files into a directory on `custom-theme-load-path`. A
typical Linux or macOS user-local installation is:

```sh
mkdir -p ~/.emacs.d/themes
cp emacs/nib-{light,dark}-theme.el ~/.emacs.d/themes/
```

Then add the directory and choose one explicit variant in your own init file:

```elisp
(add-to-list 'custom-theme-load-path
             (expand-file-name "themes" user-emacs-directory))
(load-theme 'nib-dark t) ; or nib-light
```

The `t` argument suppresses the interactive trust prompt only for that call;
review locally installed theme code before using it. Without an init-file
change, run `M-x load-theme` and select `nib-light` or `nib-dark`.

Nib deliberately exposes two independent themes instead of guessing the
desktop appearance from inside Emacs. This keeps selection explicit and makes
theme switching predictable:

```elisp
(mapc #'disable-theme custom-enabled-themes)
(load-theme 'nib-light t)
```

## Terminal colors

Both files set `ansi-color-names-vector` and `ansi-term-color-vector` from the
same ANSI 0–15 palette used by Ghostty, Neovim, VS Code/Cursor, and Zed. The
eight standard and eight bright `ansi-color-*` faces are also defined. Emacs
terminal emulation and external terminal processes can still differ according
to terminal capabilities and package behavior.

## Remove

Disable the active theme with `M-x disable-theme`, remove any `load-theme`
line you added, and delete only `nib-light-theme.el` and `nib-dark-theme.el`
from your theme directory. The repository never edits Emacs configuration.

## Verification

When Emacs is installed, the release suite runs:

```sh
emacs --batch -Q -l tests/emacs_spec.el
```

The test loads both variants in a clean session, checks representative core
faces and terminal vectors, and confirms distinct resolved backgrounds.
Without Emacs, `make verify` performs generated-file, delimiter, face-presence,
canonical-color, and ANSI structural checks and reports runtime validation as
skipped.
