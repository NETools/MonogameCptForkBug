matrix WorldViewProjection;

RWStructuredBuffer<int> buffer1;
RWStructuredBuffer<int> buffer2;

struct VertexShaderInput
{
	float4 Position : POSITION0;
    int Id : BLENDINDICES0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
    int Id : Depth0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	output.Position = mul(input.Position, WorldViewProjection);
    output.Id = input.Id;
	
	return output;
}

float4 MainPS(VertexShaderOutput input) : SV_TARGET
{
    InterlockedMax(buffer1[input.Id], 1000);
    //buffer2[0] = 0xabc; // when writing to this buffer, the bug occurs!.
	
    return float4(1, 0, 0, 1);
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile vs_5_0 MainVS();
		PixelShader = compile ps_5_0 MainPS();
	}
};