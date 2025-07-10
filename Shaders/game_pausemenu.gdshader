shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, filter_linear_mipmap;
uniform float blur_size = 2.0; // Overall scale of the blur
uniform vec2 step = vec2(1.0, 0.0); // Direction of blur: (1,0) = horizontal, (0,1) = vertical

const float DEFAULT_RADIUS = 5.0;

void fragment() {
    vec2 pixel_step = blur_size / DEFAULT_RADIUS * step * SCREEN_PIXEL_SIZE;

    COLOR.rgb =
        0.011194727 * texture(screen_texture, SCREEN_UV - 10.0 * pixel_step).rgb +
        0.039368696 * texture(screen_texture, SCREEN_UV - 8.415809477 * pixel_step).rgb +
        0.071308558 * texture(screen_texture, SCREEN_UV - 6.435363708 * pixel_step).rgb +
        0.110237219 * texture(screen_texture, SCREEN_UV - 4.455121108 * pixel_step).rgb +
        0.145451038 * texture(screen_texture, SCREEN_UV - 2.475020813 * pixel_step).rgb +
        0.163798995 * texture(screen_texture, SCREEN_UV - 0.495000167 * pixel_step).rgb +
        0.157439298 * texture(screen_texture, SCREEN_UV + 1.485004498 * pixel_step).rgb +
        0.129158204 * texture(screen_texture, SCREEN_UV + 3.465057055 * pixel_step).rgb +
        0.090434685 * texture(screen_texture, SCREEN_UV + 5.445220765 * pixel_step).rgb +
        0.054043977 * texture(screen_texture, SCREEN_UV + 7.425557483 * pixel_step).rgb +
        0.027564604 * texture(screen_texture, SCREEN_UV + 9.406126897 * pixel_step).rgb;

    COLOR.a = 1.0;
}


void vertex() {
	// Called for every vertex the material is visible on.
}


//void light() {
	// Called for every pixel for every light affecting the CanvasItem.
	// Uncomment to replace the default light processing function with this one.
//}
