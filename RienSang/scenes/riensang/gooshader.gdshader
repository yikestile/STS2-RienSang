shader_type canvas_item;
render_mode blend_mix;

uniform sampler2D smoke_tex;
uniform sampler2D dissolve_noise_tex;
uniform float phase : hint_range(-2.0, 2.0) = 0.0;
uniform float dissolve_value : hint_range(0.0, 1.0) = 0.0;
uniform vec4 smoke_color : source_color = vec4(0.1, 0.1, 0.1, 1.0);
uniform float scroll_speed = 1.0;

// SPREADSHEET CONTROLS
uniform int h_frames = 8;
uniform int v_frames = 8;
uniform int frame = 0; // Animate this in the AnimationPlayer

void fragment() {
    // 1. CALCULATE FRAME UVs
    // Scale the UV down to the size of a single frame
    vec2 frame_size = vec2(1.0 / float(h_frames), 1.0 / float(v_frames));
    
    // Determine column and row from the current frame index
    float frame_x = float(frame % h_frames);
    float frame_y = float(frame / h_frames);
    
    // Shift UV to the correct frame position
    vec2 frame_uv = (UV * frame_size) + vec2(frame_x * frame_size.x, frame_y * frame_size.y);

    // 2. PHASING MOTION (Applied to the frame-shifted UV)
    vec2 moving_uv = vec2(fract(frame_uv.x + phase * scroll_speed), frame_uv.y);
    
    // 3. TEXTURE MASK
    vec4 smoke_sample = texture(smoke_tex, moving_uv);
    float mask = smoke_sample.r; 
    
    // 4. DISSOLVE (Using standard UV so noise stays pinned to the particle)
    float dissolve_noise = texture(dissolve_noise_tex, UV).r;
    float dissolve_mask = step(dissolve_value, dissolve_noise);
    
    // 5. FINAL OUTPUT
    vec3 final_rgb = smoke_sample.rgb * smoke_color.rgb * mask; 
    float final_alpha = mask * COLOR.a * dissolve_mask;
    
    COLOR = vec4(final_rgb, final_alpha);
}