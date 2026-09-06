package.path = "./?.lua;./?/init.lua;" .. package.path
require("tests.test_movement")
require("tests.test_materials")
require("tests.test_generator")
require("tests.test_run")
require("tests.test_tooling")
require("tests.test_integration")
require("tests.harness").finish()
