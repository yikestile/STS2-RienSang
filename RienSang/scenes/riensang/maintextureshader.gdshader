shader_type canvas_item;

uniform sampler2D detail_tex : source_color; 
uniform sampler2D dissolve_noise_tex;
uniform float phase : hint_range(-2.0, 2.0) = 0.0;
uniform float dissolve_value : hint_range(0.0, 1.0) = 0.0;

// --- ROTATION CONTROL ---
uniform float noise_rotation : hint_range(0.0, 6.28) = 0.0; 

// --- NEW CUTOFF CONTROLS ---
uniform int glow_direction : hint_range(0, 1) = 0; 
uniform bool flip_glow = false; 

// Layer Alpha Controls
uniform float global_alpha : hint_range(0.0, 1.0) = 1.0; 
uniform float glow_alpha : hint_range(0.0, 1.0) = 1.0;    

uniform vec4 slash_color : source_color = vec4(0.1, 0.1, 0.1, 1.0);
uniform vec4 noise_color : source_color = vec4(0.0, 0.4, 0.8, 1.0);
uniform vec4 glow_color : source_color = vec4(0.4, 0.7, 1.0, 1.0);

uniform float noise_intensity : hint_range(0.0, 5.0) = 1.0;
uniform float glow_intensity : hint_range(0.0, 10.0) = 5.0;
uniform float glow_cutoff : hint_range(0.0, 1.0) = 0.5; 
uniform float glow_power : hint_range(0.0, 20.0) = 5.0;

void fragment() {
    // 1. ROTATING DISSOLVE LOGIC (REVERSED)
    vec2 centered_uv = UV - vec2(0.5);
    // CHANGED: Using '-' instead of '+' to reverse the rotation direction
    float active_rotation = noise_rotation + (TIME * 0.5); 
    
    float cos_r = cos(active_rotation);
    float sin_r = sin(active_rotation);
    
    vec2 rotated_uv = vec2(
        centered_uv.x * cos_r - centered_uv.y * sin_r,
        centered_uv.x * sin_r + centered_uv.y * cos_r
    ) + vec2(0.5);

    float dissolve_noise = texture(dissolve_noise_tex, rotated_uv).r;
    float dissolve_mask = step(dissolve_value, dissolve_noise);

    // 2. LINEAR MOTION
    float shifted_x = UV.x + phase;
    float range_mask = step(0.0, shifted_x) * step(shifted_x, 1.0);
    vec2 moving_uv = vec2(shifted_x, UV.y);
    
    // 3. SAMPLE TEXTURES
    vec4 base_tex = texture(TEXTURE, moving_uv);
    vec4 noise_layer = texture(detail_tex, moving_uv);
    float mask = base_tex.r * range_mask;
    
    // 4. SMART GLOW CUTOFF LOGIC
    float select_uv = (glow_direction == 0) ? UV.y : UV.x;
    
    float edge_crop;
    if (flip_glow) {
        edge_crop = smoothstep(glow_cutoff, glow_cutoff + 0.1, select_uv);
    } else {
        edge_crop = smoothstep(glow_cutoff, glow_cutoff - 0.1, select_uv);
    }
    
    // 5. LAYER CALCULATIONS
    vec3 base_pass = base_tex.rgb * slash_color.rgb;
    vec3 noise_pass = noise_layer.rgb * noise_color.rgb * noise_intensity;
    vec3 glow_pass = base_tex.rgb * glow_color.rgb * (glow_intensity * glow_alpha) * edge_crop;
    
    // 6. FINAL COMBINATION
    vec3 final_rgb = (base_pass + noise_pass + glow_pass) * mask;
    final_rgb *= glow_power;
    
    float final_alpha = mask * global_alpha * COLOR.a * dissolve_mask;

    COLOR = vec4(final_rgb * final_alpha, final_alpha);
}