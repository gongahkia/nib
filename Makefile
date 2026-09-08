PYTHON ?= python3

.PHONY: generate check-generated shaders preview verify

generate:
	$(PYTHON) scripts/generate.py

check-generated:
	$(PYTHON) scripts/generate.py --check

shaders:
	$(PYTHON) scripts/validate_shaders.py

preview: check-generated
	$(PYTHON) -m http.server 8765 --bind 127.0.0.1

verify:
	$(PYTHON) tests/verify.py
