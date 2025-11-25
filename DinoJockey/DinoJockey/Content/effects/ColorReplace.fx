sampler TextureSampler : register(s0);

float4 TargetColor;   // Color que queremos reemplazar
float4 NewColor;      // Color nuevo
float Tolerance;      // Qué tan parecido debe ser el color para reemplazarlo

float4 PixelShaderFunction(float2 texCoord : TEXCOORD0) : COLOR0
{
    float4 texColor = tex2D(TextureSampler, texCoord);

    // Calculamos la diferencia entre el color actual y el objetivo
    float diff = distance(texColor.rgb, TargetColor.rgb);

    // Si está dentro de la tolerancia, lo reemplazamos
    if (diff < Tolerance)
    {
        // Conservamos la alpha original
        return float4(NewColor.rgb, texColor.a);
    }

    // Si no coincide, dejamos el color original
    return texColor;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
