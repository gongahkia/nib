PYTHON ?= python3

.PHONY: generate check-generated shaders verify

generate:
	$(PYTHON) scripts/generate.py

check-generated:
	$(PYTHON) scripts/generate.py --check

shaders:
	$(PYTHON) scripts/validate_shaders.py

verify:
	$(PYTHON) tests/verify.py
