shader_type canvas_item;

uniform sampler2D icon_texture : source_color;

void fragment() {
    // This shader now only draws the icon provided by icon_texture.
    // The centering and text are handled by C# Nodes.
    vec4 tex = texture(icon_texture, UV);
    COLOR = tex;
}