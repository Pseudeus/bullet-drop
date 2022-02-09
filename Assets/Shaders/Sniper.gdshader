shader_type spatial;
render_mode unshaded;

uniform sampler2D AlbedoTexture;
uniform sampler2D AlphaTexture;

void fragment()
{
	vec3 n_out3p0;
	vec4 n_tex_read = texture(AlbedoTexture, UV.xy);
	n_out3p0 = n_tex_read.rgb;
	
	vec4 tex_frg_2_read = texture(AlphaTexture, UV.xy);
	float n_out2p0 = tex_frg_2_read.a;
	
	ALBEDO = n_out3p0;
	ALPHA = n_out2p0;
}