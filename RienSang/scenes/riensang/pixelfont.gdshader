shader_type canvas_item;
render_mode blend_add;

// We pass the color and intensity separately to prevent white-out
uniform vec3 glow_color : source_color = vec3(0.30, 0.51, 0.87); // 4d82de
uniform float intensity : hint_range(0.0, 10.0) = 4.5;

void fragment() {
    vec4 tex_color = texture(TEXTURE, UV);

    // 1. Surgical Black Removal
    if (tex_color.r < 0.5) {
        discard;
    }

    // 2. Force the Alpha to be solid
    // This ensures the character doesn't look 'see-through'
    float alpha = 1.0;

	vec3 base_ink = glow_color * 0.8; // The actual letter color (LDR)
	vec3 glow_bleed = glow_color * intensity; // The HDR energy for the bloom
	COLOR = vec4(base_ink + glow_bleed, 1.0);
}