local H = { count = 0, failed = 0 }

function H.test(name, fn)
  H.count = H.count + 1
  local ok, err = xpcall(fn, debug.traceback)
  if ok then io.write("ok ", H.count, " - ", name, "\n")
  else H.failed = H.failed + 1; io.write("not ok ", H.count, " - ", name, "\n", err, "\n") end
end

function H.eq(actual, expected, message)
  if actual ~= expected then error((message or "values differ") .. ": expected " .. tostring(expected) .. ", got " .. tostring(actual), 2) end
end

function H.near(actual, expected, epsilon, message)
  if math.abs(actual - expected) > epsilon then error((message or "values not near") .. ": expected " .. expected .. ", got " .. actual, 2) end
end

function H.ok(value, message) if not value then error(message or "expected truthy value", 2) end end

function H.finish()
  io.write(string.format("1..%d\n%d passed, %d failed\n", H.count, H.count - H.failed, H.failed))
  if H.failed > 0 then os.exit(1) end
end

return H
