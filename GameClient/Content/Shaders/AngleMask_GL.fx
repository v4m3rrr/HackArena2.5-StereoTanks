#pragma target glsl
#define PI 3.14159265359
#define TWO_PI 6.28318530718
#define MAX_SEGMENTS 8

// Tekstura jako parametr (widoczna w C# jako "SpriteTexture")
texture2D SpriteTexture;

sampler2D SpriteSampler = sampler_state
{
    Texture = <SpriteTexture>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

cbuffer MaskParams : register(b0)
{
    int NumSegments;
    float GlobalOpacity;
    float Pct[MAX_SEGMENTS];
    float4 ColorArr[MAX_SEGMENTS];
};

struct VS_IN
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

struct VS_OUT
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
};

VS_OUT MainVS(VS_IN input)
{
    VS_OUT o;
    o.Position = input.Position;
    o.TexCoord = input.TexCoord;
    return o;
}

float4 MainPS(VS_OUT input) : COLOR
{
    float2 uv = input.TexCoord;
    float2 dir = uv - float2(0.5, 0.5);
    float ang = atan2(dir.x, -dir.y);
    if (ang < 0)
        ang += TWO_PI;

    float cumAngle = 0;
    float4 maskCol = float4(0, 0, 0, 0);

    [unroll]
    for (int i = 0; i < MAX_SEGMENTS; ++i)
    {
        if (i >= NumSegments) break;

        cumAngle += Pct[i] * TWO_PI;
        if (ang < cumAngle)
        {
            maskCol = ColorArr[i];
            break;
        }
    }

    float4 baseCol = tex2D(SpriteSampler, uv);
    float outAlpha = baseCol.a * maskCol.a * GlobalOpacity;
    float3 outRGB = baseCol.rgb * maskCol.rgb;

    return float4(outRGB, outAlpha);
}

technique AngleMaskEffect
{
    pass P0
    {
        VertexShader = compile vs_3_0 MainVS();
        PixelShader = compile ps_3_0 MainPS();
    }
}
