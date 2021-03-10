int bIsMouseOver : register(c0);
float Time : register(c1);
sampler2D Implicit : register(s0);
sampler2D AlphaTex : register(s1);
sampler2D ScrollTex : register(s2);

float4 main(float2 texCoord : TEXCOORD) : COLOR
{
	float4 color = tex2D(Implicit, texCoord);
	if (bIsMouseOver)
	{
		float4 alphaColor = tex2D(AlphaTex, texCoord);
		texCoord.y += Time;
		float4 scrollColor = tex2D(ScrollTex, texCoord);
		color.xyz += scrollColor.xyz * color.w * alphaColor.w;
	}
	return color;
}