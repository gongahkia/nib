local A = {}

A.states = {
  idle = { lean = 0, arm = -1, leg = 1 }, acceleration_run = { lean = 1, arm = 2, leg = -2 },
  high_speed_run = { lean = 2, arm = 3, leg = -3 }, turn_skid = { lean = -2, arm = -3, leg = 3 },
  jump_rise = { lean = 0, arm = -3, leg = 2 }, fall = { lean = 0, arm = 3, leg = -1 },
  wall_traversal = { lean = -2, arm = -3, leg = 1 }, air_dash = { lean = 3, arm = -4, leg = -3 },
  slide = { lean = 4, arm = 1, leg = -4, crouch = 3 }, dive = { lean = 4, arm = 4, leg = 3 },
  vault_mantle = { lean = 2, arm = -4, leg = 4 }, grapple_aim = { lean = 0, arm = -4, leg = 0 },
  grapple_attach = { lean = -1, arm = -5, leg = 2 }, grapple_travel = { lean = 2, arm = -5, leg = 3 },
  grapple_release = { lean = 2, arm = 3, leg = -2 }, tool_cutting = { lean = 2, arm = -4, leg = 1 },
  tool_push_pull = { lean = 3, arm = -3, leg = 3 }, recoil_impulse = { lean = -4, arm = 4, leg = -2 },
  enemy_impact = { lean = 1, arm = 4, leg = 4 }, stumble_recovery = { lean = -4, arm = 4, leg = -4 },
  hand_capture = { lean = 0, arm = -5, leg = -5 }, exit_finish = { lean = 0, arm = -5, leg = 2 },
}

function A.pose(state, tick, facing)
  local base = A.states[state] or A.states.idle
  local phase = math.floor(tick / 3) % 2 == 0 and 1 or -1
  return { lean = base.lean * facing, arm = base.arm * phase, leg = base.leg * phase, crouch = base.crouch or 0 }
end

return A
