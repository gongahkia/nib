// Nib showcase paper stage: static, one texture sample, no time input.

float nib_hash(vec2 point) {
    vec3 value = fract(vec3(point.xyx) * 0.1031);
    value += dot(value, value.yzx + 33.33);
    return fract((value.x + value.y) * value.z);
}

float nib_luma(vec3 color) {
    return dot(color, vec3(0.2126, 0.7152, 0.0722));
}

void mainImage(out vec4 fragColor, in vec2 fragCoord) {
    vec2 uv = fragCoord / iResolution.xy;
    vec4 source = texture(iChannel0, uv);
    float paperMask = 1.0 - smoothstep(0.025, 0.18, length(source.rgb - iBackgroundColor));
    float darkMode = 1.0 - step(0.45, nib_luma(iBackgroundColor));
    float strength = mix(0.0100, 0.0070, darkMode);
    float coarse = nib_hash(floor(fragCoord * 0.5)) - 0.5;
    float fine = nib_hash(floor(fragCoord)) - 0.5;
    float grain = coarse * 0.65 + fine * 0.35;
    vec3 tint = mix(vec3(0.92, 0.82, 0.62), vec3(0.55, 0.72, 0.82), darkMode);
    vec3 textured = source.rgb + grain * strength * tint * paperMask;
    fragColor = vec4(clamp(textured, 0.0, 1.0), source.a);
}
