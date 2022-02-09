shader_type spatial;

uniform vec4 background : hint_color;
uniform vec4 foreground : hint_color;
uniform float level_depth;

void vertex()
{
	mat4 world = WORLD_MATRIX;
	vec3 local = VERTEX;
	
	vec3 local2world = (world * vec4(local, 1.0)).xyz - local;
	COLOR.rgb = local2world;
}

void fragment()
{
	vec3 fore = foreground.rgb;
	vec3 back = background.rgb;
	
	float depth = COLOR.z;
	
	vec3 color = mix(fore, back, depth / level_depth + 0.5);
	ALBEDO = color;
}