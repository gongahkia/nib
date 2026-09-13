PYTHON ?= python3

.PHONY: generate check-generated install shaders verify

generate:
	$(PYTHON) scripts/generate.py

check-generated:
	$(PYTHON) scripts/generate.py --check

install:
	$(PYTHON) scripts/install.py --apply

shaders:
	$(PYTHON) scripts/validate_shaders.py

verify:
	$(PYTHON) tests/verify.py
