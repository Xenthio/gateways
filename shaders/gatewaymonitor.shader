//=========================================================================================================================
// Optional
//=========================================================================================================================
HEADER
{
	CompileTargets = ( IS_SM_50 && ( PC || VULKAN ) );
	Description = "portalz";
}

//=========================================================================================================================
// Optional
//=========================================================================================================================
FEATURES
{
    #include "common/features.hlsl"

	Feature(F_ENABLE_DEPTH, 0..1, "Render Stuff");
	Feature(F_PORTAL_SHAPE, 0..1, "Render Stuff");
}

//=========================================================================================================================
COMMON
{
	#include "common/shared.hlsl"
}

//=========================================================================================================================

struct VertexInput
{
	#include "common/vertexinput.hlsl"
};

//=========================================================================================================================

struct PixelInput
{
	#include "common/pixelinput.hlsl"
};

//=========================================================================================================================

VS
{
	#include "common/vertex.hlsl"
	//
	// Main
	//
	PixelInput MainVs( INSTANCED_SHADER_PARAMS( VS_INPUT i ) )
	{
		PixelInput o = ProcessVertex( i );
		// Add your vertex manipulation functions here

		return FinalizeVertex( o );
	}
}

//=========================================================================================================================

PS
{
	//#include "system.fxc"
	#include "common.fxc"
	//#include "math_general.fxc"
	//#include "encoded_normals.fxc"


	RenderState( DepthEnable, F_ENABLE_DEPTH ? true : false );
    RenderState( DepthWriteEnable, F_ENABLE_DEPTH ? true : false );

	CreateInputTexture2D( Texture, Srgb, 8, "", "", "Color", Default3( 1.0, 1.0, 1.0 ) );
	// CreateTexture2DInRegister( g_tColor, 0 ) < Channel( RGBA, None( Texture ), Srgb ); OutputFormat( RGBA8888 ); SrgbRead( true ); >;
	CreateTexture2DInRegister( g_tColor, 0 ) < Channel( RGBA, None( Texture ), Srgb ); OutputFormat( RGBA16161616F ); SrgbRead( true ); >;
	//TextureAttribute( RepresentativeTexture, g_tColor );

	//CreateTexture2D( g_tDepthBuffer ) < Attribute( "DepthBufferCopyTexture" ); 	SrgbRead( false ); Filter( MIN_MAG_MIP_POINT ); AddressU( CLAMP ); AddressV( CLAMP ); >;

    // float GetDepth(float2 uv)
    // {
    //     return Tex2DLevel( g_tDepthBuffer, uv, 0 ).r + g_flViewportMinZ;
    // }

	float4 MainPs( PixelInput i ) : SV_Target0
	{
		float4 o;

#if F_PORTAL_SHAPE
		if(length(i.vTextureCoords.xy - float2(0.5, 0.5)) > 0.5)
			discard;
#endif //F_PORTAL_SHAPE

		float2 vScreenUv = CalculateViewportUvFromInvSize( i.vPositionSs.xy, g_vInvGBufferSize.xy );

        o = Tex2D( g_tColor, vScreenUv );

		return o;
	}
}