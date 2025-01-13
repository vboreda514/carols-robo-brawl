shader_type canvas_item;

uniform vec4 hover_color = vec4(1.5, 1.5, 1.5, 1.0);  // Increased brightness
uniform vec4 selected_color = vec4(1.2, 1, 0.0, 0.3);  // More transparent gold
uniform bool is_hovered = false;
uniform bool is_selected = false;

void fragment() {
    vec4 base_color = texture(TEXTURE, UV);
    
    if (is_selected) {
        // Create very translucent gold overlay by using alpha blending
        vec4 overlay = selected_color;
        base_color.rgb = mix(base_color.rgb, overlay.rgb, overlay.a);
    }
    // Only apply hover effect if not selected
    else if (is_hovered) {
        // Increased brightness multiplier
        base_color.rgb *= hover_color.rgb;
        // Clamp values to prevent over-brightening
        base_color.rgb = clamp(base_color.rgb, 0.0, 1.0);
    }
    
    COLOR = base_color;
}