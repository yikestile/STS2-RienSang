shader_type canvas_item;
// This file is intentionally left minimal as its content has been moved inline to riensang.tscn
void fragment() {
    COLOR = texture(TEXTURE, UV);
}
