;;; emacs_spec.el --- Runtime checks for Nib themes -*- lexical-binding: t; -*-

(require 'ansi-color)

(let* ((tests-directory (file-name-directory (or load-file-name buffer-file-name)))
       (root-directory (expand-file-name ".." tests-directory))
       (theme-directory (expand-file-name "emacs" root-directory))
       backgrounds)
  (add-to-list 'custom-theme-load-path theme-directory)
  (dolist (theme '(nib-light nib-dark))
    (mapc #'disable-theme custom-enabled-themes)
    (load-theme theme t)
    (unless (memq theme custom-enabled-themes)
      (error "Theme did not enable: %S" theme))
    (dolist (face '(default cursor region mode-line font-lock-comment-face
                            font-lock-function-name-face font-lock-keyword-face
                            font-lock-string-face font-lock-type-face error warning))
      (unless (facep face)
        (error "Required face is unavailable: %S" face)))
    (let ((foreground (face-attribute 'default :foreground nil t))
          (background (face-attribute 'default :background nil t)))
      (unless (and (stringp foreground) (stringp background))
        (error "Default face colors were not applied for %S" theme))
      (push background backgrounds))
    (unless (= (length ansi-color-names-vector) 8)
      (error "ANSI color vector is incomplete for %S" theme)))
  (unless (not (equal (car backgrounds) (cadr backgrounds)))
    (error "Light and dark themes resolved to the same background"))
  (mapc #'disable-theme custom-enabled-themes))

(princ "Emacs runtime: pass\n")

;;; emacs_spec.el ends here
