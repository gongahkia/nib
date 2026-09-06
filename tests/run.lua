package.path = "./?.lua;./?/init.lua;" .. package.path
require("tests.test_movement")
require("tests.harness").finish()
