// Upgrade NOTE: commented out 'float4x4 _CameraToWorld', a built-in variable
// Upgrade NOTE: replaced '_CameraToWorld' with 'unity_CameraToWorld'

// Upgrade NOTE: commented out 'float4x4 _CameraToWorld', a built-in variable
// Upgrade NOTE: replaced '_CameraToWorld' with 'unity_CameraToWorld'

Shader "Hidden/SGSR2_Velocity"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }

    CGINCLUDE
    #include "UnityCG.cginc"

    struct appdata
    {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
    };

    struct v2f
    {
        float4 vertex : SV_POSITION;
        float2 uv : TEXCOORD0;
    };

    sampler2D _MainTex;
    sampler2D _CameraDepthTexture;
    float4x4 _CurrVP;
    float4x4 _PrevVP;
    float4x4 _CurrInvProj;

    v2f vert(appdata v)
    {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = v.uv;
        return o;
    }

    float2 EncodeVelocity(float2 velocity)
    {
        // Encode velocity to [0,1] range
        return velocity * 0.499f + 32767.0f / 65535.0f;
    }

    float4 frag(v2f i) : SV_Target
    {
        float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
        float4 clipPos = float4(i.uv * 2.0 - 1.0, depth, 1.0);

        
        // Transform to world space
        float4 worldPos = mul(_CurrInvProj, clipPos);
        worldPos /= worldPos.w;
        worldPos = mul(unity_CameraToWorld, worldPos);

        // Calculate previous position
        float4 prevClipPos = mul(_PrevVP, worldPos);
        float4 currClipPos = mul(_CurrVP, worldPos);

        float2 prevPos = prevClipPos.xy / prevClipPos.w;
        float2 currPos = currClipPos.xy / currClipPos.w;

        // Calculate velocity
        float2 velocity = currPos - prevPos;

        // return float4(velocity,0,1);
        // Encode velocity
        return float4(EncodeVelocity(velocity), 0, 1);
    }
    ENDCG

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
    }
} 