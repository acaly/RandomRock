struct VS_IN
{
	float4 pos : POSITION;
	float4 nor : NORMAL;
};

struct PS_IN
{
	float4 pos : SV_POSITION;
	float4 nor : NORMAL;
};

cbuffer VS_CONSTANT_BUFFER : register(b0)
{
	float4x4 worldViewProj;
}

PS_IN VS(VS_IN input)
{
	PS_IN output = (PS_IN)0;

	output.pos = mul(input.pos, worldViewProj);
	output.nor = input.nor;

	return output;
}

float4 PS(PS_IN input) : SV_Target
{
	return float4(float3(0.8, 0.5, 0.5) * (dot(input.nor.xyz, float3(0.3, 0.6, 1.2)) + 0.1), 1);
}
