shader_type spatial;
render_mode unshaded;

uniform sampler2D Reticle;

void fragment()
{
	vec4 reticle_data;
	vec4 reticle_data_read = texture(Reticle, UV.xy);
	reticle_data = reticle_data_read.rgba;
	
	ALBEDO = vec3(reticle_data.rgb);
	ALPHA = reticle_data.a;
}