shader_type canvas_item;
uniform sampler2D transitionTex;
uniform float threshold : hint_range(0,1);
void fragment() {
    float falloff = 1.0 - texture(transitionTex, UV).r;
    float remap  = mix(-0.1, 1.1, threshold);
    falloff = step(falloff, remap);
    COLOR.a = falloff;
}