shader_type canvas_item;

uniform sampler2D impact_tex : source_color;
uniform sampler2D detail_tex : source_color; 
uniform sampler2D dissolve_noise_tex;

uniform float phase : hint_range(-2.0, 2.0) = 0.0;
uniform float rotation_speed : hint_range(-5.0, 5.0) = 1.5;

// --- WINDOW & GEOMETRY ---
uniform float view_width : hint_range(0.1, 2.0) = 1.0; 
uniform float x_offset : hint_range(-1.0, 1.0) = 0.0;  
uniform float radius_left : hint_range(0.0, 0.5) = 0.45;  
uniform float radius_right : hint_range(0.0, 0.5) = 0.15; 

// --- GLOW & DISSOLVE CONTROLS ---
uniform float dissolve_value : hint_range(0.0, 1.0) = 0.0;
uniform float noise_rotation : hint_range(0.0, 6.28) = 0.0;
uniform int glow_direction : hint_range(0, 1) = 0; 
uniform bool flip_glow = false; 

uniform float global_alpha : hint_range(0.0, 1.0) = 1.0; 
uniform float glow_alpha : hint_range(0.0, 1.0) = 1.0; 
uniform float glow_cutoff : hint_range(0.0, 1.0) = 0.5; 

uniform vec4 slash_color : source_color = vec4(0.1, 0.1, 0.1, 1.0);
uniform vec4 noise_color : source_color = vec4(0.0, 0.4, 0.8, 1.0);
uniform vec4 glow_color : source_color = vec4(0.4, 0.7, 1.0, 1.0);

uniform float noise_intensity : hint_range(0.0, 5.0) = 1.0;
uniform float glow_intensity : hint_range(0.0, 10.0) = 5.0;
uniform float glow_power : hint_range(0.0, 20.0) = 5.0;

void fragment() {
    // 1. WINDOW COORDINATES
    vec2 local_uv = UV;
    local_uv.x = (local_uv.x - 0.5 + x_offset) / view_width + 0.5;

    if (local_uv.x < 0.0 || local_uv.x > 1.0) { discard; }

    // 2. CONE CALCULATIONS
    float current_radius = mix(radius_left, radius_right, local_uv.x);
    float centered_y = local_uv.y - 0.5;
    if (abs(centered_y) > current_radius) { discard; }

    // 3. 3D WRAP UVs
    float curve_pos = clamp(centered_y / current_radius, -1.0, 1.0);
    float wrap_angle = asin(curve_pos);
    float u_base = (wrap_angle / 3.14159) + 0.5;
    
    float u_front = fract(u_base + (TIME * rotation_speed));
    float v_tex = local_uv.x - phase; 
    vec2 warped_uv = vec2(u_front, v_tex);

    // 4. ROTATING DISSOLVE
    float active_rotation = noise_rotation + (TIME * 0.5); 
    vec2 centered_uv = UV - vec2(0.5);
    float cos_r = cos(active_rotation);
    float sin_r = sin(active_rotation);
    vec2 rot_uv = vec2(centered_uv.x * cos_r - centered_uv.y * sin_r, centered_uv.x * sin_r + centered_uv.y * cos_r) + vec2(0.5);
    float dissolve_mask = step(dissolve_value, texture(dissolve_noise_tex, rot_uv).r);

    // 5. TEXTURE SAMPLING
    vec4 base_tex = texture(TEXTURE, warped_uv);
    vec4 noise_layer = texture(detail_tex, warped_uv);
    float base_mask = base_tex.r;

    // 6. SMART GLOW CUTOFF
    // glow_direction 0 wraps around the cylinder (Y), 1 goes along the length (X)
    float select_uv = (glow_direction == 0) ? u_base : local_uv.x;
    float edge_crop;
    if (flip_glow) {
        edge_crop = smoothstep(glow_cutoff, glow_cutoff + 0.1, select_uv);
    } else {
        edge_crop = smoothstep(glow_cutoff, glow_cutoff - 0.1, select_uv);
    }

    // 7. COMPOSITION
    vec3 base_pass = base_tex.rgb * slash_color.rgb;
    vec3 noise_pass = noise_layer.rgb * noise_color.rgb * noise_intensity;
    vec3 glow_pass = base_tex.rgb * glow_color.rgb * (glow_intensity * glow_alpha) * edge_crop;
    
    vec3 final_rgb = (base_pass + noise_pass + glow_pass) * base_mask;
    final_rgb *= glow_power;
    
    float shading = cos(wrap_angle);
    float edge_fade = smoothstep(0.0, 0.05, local_uv.x) * smoothstep(1.0, 0.95, local_uv.x);
    float final_alpha = base_mask * global_alpha * COLOR.a * dissolve_mask * shading * edge_fade;

    COLOR = vec4(final_rgb * final_alpha, final_alpha);
}