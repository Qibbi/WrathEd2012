float4 RecolorColor : register(c0);
sampler2D Implicit : register(s0);

float4 main(float2 texCoord : TEXCOORD) : COLOR
{
	float4 color = tex2D(Implicit, texCoord);
	color.xyz *= RecolorColor;
	return color;
}