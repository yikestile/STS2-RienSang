shader_type canvas_item;
render_mode blend_mix;

uniform sampler2D smoke_tex : source_color;
uniform sampler2D dissolve_noise_tex;
uniform float phase : hint_range(-2.0, 2.0) = 0.0;
uniform float dissolve_value : hint_range(0.0, 1.0) = 0.0;
uniform float noise_rotation : hint_range(0.0, 6.28) = 0.0; 
uniform vec4 smoke_color : source_color = vec4(0.1, 0.1, 0.1, 1.0);

// CHANGED: Range now allows negative values for upward motion
uniform float gravity_strength : hint_range(-500.0, 500.0) = 0.0;

void vertex() {
    // Moves the vertices based on how far they are along the line (UV.x)
    // Positive = Down, Negative = Up
    VERTEX.y += UV.x * gravity_strength;
}

void fragment() {
    // 1. LINEAR MOTION & RANGE MASK
    float shifted_x = UV.x + phase;
    float range_mask = step(0.0, shifted_x) * step(shifted_x, 1.0);
    
    // 2. SAMPLE TEXTURE
    vec2 moving_uv = vec2(shifted_x, UV.y);
    vec4 smoke_sample = texture(smoke_tex, moving_uv);
    
    // 3. ROTATING DISSOLVE LOGIC
    vec2 centered_uv = UV - vec2(0.5);
    float active_rotation = noise_rotation - (TIME * 0.5); 
    
    float cos_r = cos(active_rotation);
    float sin_r = sin(active_rotation);
    
    vec2 rotated_uv = vec2(
        centered_uv.x * cos_r - centered_uv.y * sin_r,
        centered_uv.x * sin_r + centered_uv.y * cos_r
    ) + vec2(0.5);
    
    float dissolve_noise = texture(dissolve_noise_tex, rotated_uv).r;
    float dissolve_mask = step(dissolve_value, dissolve_noise);
    
    // 4. APPLY MASK & COLORS
    float mask = smoke_sample.r * range_mask; 
    vec3 final_rgb = smoke_color.rgb * mask; 
    float final_alpha = mask * COLOR.a * dissolve_mask;
    
    COLOR = vec4(final_rgb * final_alpha, final_alpha);
}