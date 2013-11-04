float4x4 xWorldViewProjection;
float4x4 lightViewProjection;

float4x4 xWorld;
float3 xLightPos;
float xLightPower;
float xAmbient;
float3 DiffuseColor;

struct VertexToPixel
{
    float4 Position      : POSITION;    
	float3 Normal        : TEXCOORD0;
	float3 Position3d    : TEXCOORD1;

};

struct PixelToFrame
{
	float4 Color         : COLOR0;
};

float DotProduct(float3 lightPos, float3 pos3D, float3 normal)
{
	float3 lightDir = normalize(pos3D - lightPos);
		return dot(-lightDir, normal);
}

VertexToPixel SimplestVertexShader(float4 inPosition : POSITION, float3 inNormal : TEXCOORD0)
{
	VertexToPixel Output = (VertexToPixel)0;

	Output.Position = mul(inPosition, xWorldViewProjection);
	Output.Normal = normalize(mul(inNormal, (float3x3)xWorld));    
	Output.Position3d = mul(inPosition, xWorld);

	return Output;
}

PixelToFrame OurFirstPixelShader(VertexToPixel PSIn)
{
	PixelToFrame Output = (PixelToFrame)0;    

	float diffuseLightingFactor = DotProduct(xLightPos, PSIn.Position3d, PSIn.Normal);
	diffuseLightingFactor = saturate(diffuseLightingFactor);
	diffuseLightingFactor *= xLightPower;

	Output.Color = float4(DiffuseColor, 1) * (diffuseLightingFactor + xAmbient);

	return Output;
}

technique Colored
{
	pass Pass0
	{
		VertexShader = compile vs_3_0 SimplestVertexShader();
		PixelShader = compile ps_3_0 OurFirstPixelShader();
	}
}