// Quireveil daily ink stage: static, five texture samples, no time input.

float qv_hash(vec2 point) {
    vec3 value = fract(vec3(point.xyx) * 0.1031);
    value += dot(value, value.yzx + 33.33);
    return fract((value.x + value.y) * value.z);
}

float qv_luma(vec3 color) {
    return dot(color, vec3(0.2126, 0.7152, 0.0722));
}

void mainImage(out vec4 fragColor, in vec2 fragCoord) {
    vec2 uv = fragCoord / iResolution.xy;
    vec2 pixel = 1.0 / iResolution.xy;
    vec4 center = texture(iChannel0, uv);
    vec3 neighbors = (
        texture(iChannel0, uv + vec2(pixel.x, 0.0)).rgb +
        texture(iChannel0, uv - vec2(pixel.x, 0.0)).rgb +
        texture(iChannel0, uv + vec2(0.0, pixel.y)).rgb +
        texture(iChannel0, uv - vec2(0.0, pixel.y)).rgb
    ) * 0.25;

    float localEdge = smoothstep(0.018, 0.13, length(center.rgb - neighbors));
    float inkMask = smoothstep(0.055, 0.24, length(center.rgb - iBackgroundColor));
    float edgeMask = localEdge * inkMask;
    vec3 feathered = mix(center.rgb, neighbors, 0.032 * edgeMask);

    float density = qv_hash(floor(fragCoord * 0.5)) * 0.007;
    vec3 darker = feathered * (1.0 - density);
    vec3 lighter = feathered + (1.0 - feathered) * density;
    float darkMode = 1.0 - step(0.45, qv_luma(iBackgroundColor));
    vec3 pooled = mix(darker, lighter, darkMode);
    pooled = mix(feathered, pooled, edgeMask);
    fragColor = vec4(clamp(pooled, 0.0, 1.0), center.a);
}
