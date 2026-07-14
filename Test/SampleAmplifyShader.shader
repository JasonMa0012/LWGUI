// Made with Amplify Shader Editor v1.9.9.10
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "New Amplify Shader"
{
	Properties
	{
		[Main(Numeric, _, on, off)] _NumericGroupToenableASEintegrationyouneedtoInstallASEv19910EnableProjectSettingsLWGUIASEIntegration( "====== Numeric Group ======%To enable ASE integration, you need to:%- Install ASE v1.9.9.10+%- Enable: Project Settings > LWGUI > ASE Integration", Float ) = 0
		[Sub(Numeric)] _Float( "Float", Float ) = 0
		[Sub(Numeric)] _Range( "Range", Range( 0, 1 ) ) = 0
		[SubPowerSlider(Numeric, 5)] _SubPowerSlider( "Sub Power Slider", Range( 0, 100 ) ) = 50
		[SubIntRange(Numeric)] _SubIntRange( "Sub Int Range", Range( -10, 10 ) ) = 0
		[MinMaxSlider(Numeric, _Min, _Max)] _MinMaxSlider( "Min Max Slider", Range( -1, 1 ) ) = 0
		[Hidden] _Min( "_Min", Range( -1, 1 ) ) = -1
		[Hidden] _Max( "_Max", Range( -1, 1 ) ) = 1
		[BitMask(Numeric)] _BitMask( "Bit Mask", Float ) = 0
		[Main(Enum, _, on, off)] _EnumGroup( "====== Enum Group - ASE Function ======", Float ) = 0
		[SubEnum(Enum, R, 0, G, 1, B, 2)] _SubEnum( "Sub Enum", Float ) = 0
		[KWEnum(Enum, Red, _ENABLE_RED, Green, _ENABLE_GREEN, Blue, _ENABLE_BLUE)] _KWEnum( "KW Enum", Float ) = 0
		[SubKeywordEnum(Enum, _KEY_1, _KEY_2, _KEY_3)] _SubKeywordEnum( "Sub Keyword Enum", Float ) = 0
		[Preset(Enum, LWGUI_ShaderPropertyPreset)] _Preset( "Preset", Float ) = 0
		[Sub(Enum)] _float1( "Float in Preset", Range( -1, 1 ) ) = 0
		[Main(Ramp, _, on, on)] _RampGroup( "====== Ramp Group ======", Float ) = 0
		[Ramp(Ramp)] _Ramp( "Ramp", 2D ) = "white" {}
		[RampAtlas(Ramp)] _RampAtlas( "Ramp Atlas", 2D ) = "white" {}
		[RampAtlasIndexer(Ramp, _RampAtlas)] _RampAtlasIndexer( "Ramp Atlas Indexer", Float ) = 0
		[Main(Vector, _, on, on)] _VectorTextureGroup( "====== Vector / Texture Group ======", Float ) = 0
		[Color(Vector, _ColorG, _ColorB, _ColorA)] _Color( "Color", Color ) = ( 1, 0, 0, 1 )
		[HideInInspector] _ColorG( "Color G", Color ) = ( 0, 1, 0, 1 )
		[HideInInspector] _ColorB( "Color B", Color ) = ( 0, 0, 1, 1 )
		[HideInInspector] _ColorA( "Color A", Color ) = ( 1, 1, 1, 1 )
		[Channel(Vector)] _Channel( "Channel", Vector ) = ( 0, 1, 0, 0 )
		[Tex(Vector, _Channel)] _TexwithChannel( "Tex with Channel", 2D ) = "white" {}
		[Tex(Vector, _Range)] _TexwithRange( "Tex with Range", 2D ) = "white" {}
		[Tex(Vector, _Float)] _TexwithFloat( "Tex with Float", 2D ) = "white" {}
		[Main(Appearance, _, on)][HelpURL(github.com, JasonMa0012, LWGUI)] _AppearanceDecoratorsGroup( "====== Appearance Decorators Group ======", Float ) = 0
		[Sub(Appearance)][Title(Appearance, Title)] _Title( "Title", Float ) = 0
		[Sub(Appearance)][Helpbox(Helpbox 1, Helpbox 2, Helpbox 3)] _HelpboxSample( "Multilingual Helpbox%Hello, world!%你好, 世界!%안녕, 세계!%%", Float ) = 0
		[Sub(Appearance)][Tooltip(Tooltip 1, Tooltip 2, Tooltip 3)] _TooltipSample( "Multilingual Tooltip#Hello, world#你好, 世界!#안녕, 세계!#", Float ) = 0
		[Sub(Appearance)][ReadOnly] _ReadOnly( "Read Only", Float ) = 0
		[Sub(Appearance)][Hidden] _Hidden( "Hidden", Float ) = 0
		[Main(Condition, _, on)] _ConditionDecoratorsGroup( "====== Condition Decorators Group ======", Float ) = 0
		[SubToggle(Condition)] _Toggle( "Toggle", Float ) = 0
		[Sub(Condition)][ShowIf(_Toggle, Equal, 0)] _ShowIfFalse0( "Show If False (0)", Float ) = 0
		[Sub(Condition)][ShowIf(_Toggle, Equal, 1)] _ShowIfTrue1( "Show If True (1)", Float ) = 0
		[Sub(Condition)][ActiveIf(And, _Toggle, Equal, 1)] _ActiveIfTrue1( "Active If True (1)", Float ) = 0
		[SubToggle(Condition)][PassSwitch(UniversalForwardOnly)] _PassSwitch( "Pass Switch", Float ) = 1
		[Title(Out of Group)] _Float0( "Float 0", Float ) = 0
		_Range1( "Range 1", Range( 0, 1 ) ) = 0.5
		_Vector0( "Vector 0", Vector ) = ( 0, 0, 0, 0 )
		_Color0( "Color 0", Color ) = ( 0, 0, 0, 0 )
		_TextureSample0( "Texture Sample 0", 2D ) = "white" {}


		//_TessPhongStrength( "Tess Phong Strength", Range( 0, 1 ) ) = 0.5
		//_TessValue( "Tess Max Tessellation", Range( 1, 32 ) ) = 16
		//_TessMin( "Tess Min Distance", Float ) = 10
		//_TessMax( "Tess Max Distance", Float ) = 25
		//_TessEdgeLength ( "Tess Edge length", Range( 2, 50 ) ) = 16
		//_TessMaxDisp( "Tess Max Displacement", Float ) = 25

		[HideInInspector] _QueueOffset("_QueueOffset", Float) = 0
        [HideInInspector] _QueueControl("_QueueControl", Float) = -1

        [HideInInspector][NoScaleOffset] unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset] unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset] unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}

		[HideInInspector][ToggleUI] _ReceiveShadows("Receive Shadows", Float) = 1.0

		//[HideInInspector] _AlphaClip("__clip", Float) = 0.0
	}

	SubShader
	{
		PackageRequirements
		{
			"com.unity.render-pipelines.universal": "[14.0,15.0]"
		}

		

		

		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" "UniversalMaterialType"="Unlit" }

	LOD 0

		ZWrite On
		Cull Back
		AlphaToMask Off
		ColorMask RGBA
		Blend One Zero, One Zero
		BlendOp Add, Add

		

		HLSLINCLUDE
		#pragma target 4.5
		#pragma prefer_hlslcc gles
		// ensure rendering platforms toggle list is visible

		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"

		#define ASE_ADJUST_CLIP_POSITION( x ) x

		#ifndef ASE_TESS_FUNCS
		#define ASE_TESS_FUNCS
		float4 FixedTess( float tessValue )
		{
			return tessValue;
		}

		float CalcDistanceTessFactor (float4 vertex, float minDist, float maxDist, float tess, float4x4 o2w, float3 cameraPos )
		{
			float3 wpos = mul(o2w,vertex).xyz;
			float dist = distance (wpos, cameraPos);
			float f = clamp(1.0 - (dist - minDist) / (maxDist - minDist), 0.01, 1.0) * tess;
			return f;
		}

		float4 CalcTriEdgeTessFactors (float3 triVertexFactors)
		{
			float4 tess;
			tess.x = 0.5 * (triVertexFactors.y + triVertexFactors.z);
			tess.y = 0.5 * (triVertexFactors.x + triVertexFactors.z);
			tess.z = 0.5 * (triVertexFactors.x + triVertexFactors.y);
			tess.w = (triVertexFactors.x + triVertexFactors.y + triVertexFactors.z) / 3.0f;
			return tess;
		}

		float CalcEdgeTessFactor (float3 wpos0, float3 wpos1, float edgeLen, float3 cameraPos, float4 scParams )
		{
			float dist = distance (0.5 * (wpos0+wpos1), cameraPos);
			float len = distance(wpos0, wpos1);
			float f = max(len * scParams.y / (edgeLen * dist), 1.0);
			return f;
		}

		float DistanceFromPlane (float3 pos, float4 plane)
		{
			float d = dot (float4(pos,1.0f), plane);
			return d;
		}

		bool WorldViewFrustumCull (float3 wpos0, float3 wpos1, float3 wpos2, float cullEps, float4 planes[6] )
		{
			float4 planeTest;
			planeTest.x = (( DistanceFromPlane(wpos0, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[0]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.y = (( DistanceFromPlane(wpos0, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[1]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.z = (( DistanceFromPlane(wpos0, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[2]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.w = (( DistanceFromPlane(wpos0, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[3]) > -cullEps) ? 1.0f : 0.0f );
			return !all (planeTest);
		}

		float4 DistanceBasedTess( float4 v0, float4 v1, float4 v2, float tess, float minDist, float maxDist, float4x4 o2w, float3 cameraPos )
		{
			float3 f;
			f.x = CalcDistanceTessFactor (v0,minDist,maxDist,tess,o2w,cameraPos);
			f.y = CalcDistanceTessFactor (v1,minDist,maxDist,tess,o2w,cameraPos);
			f.z = CalcDistanceTessFactor (v2,minDist,maxDist,tess,o2w,cameraPos);

			return CalcTriEdgeTessFactors (f);
		}

		float4 EdgeLengthBasedTess( float4 v0, float4 v1, float4 v2, float edgeLength, float4x4 o2w, float3 cameraPos, float4 scParams )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;
			tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
			tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
			tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
			tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			return tess;
		}

		float4 EdgeLengthBasedTessCull( float4 v0, float4 v1, float4 v2, float edgeLength, float maxDisplacement, float4x4 o2w, float3 cameraPos, float4 scParams, float4 planes[6] )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;

			if (WorldViewFrustumCull(pos0, pos1, pos2, maxDisplacement, planes))
			{
				tess = 0.0f;
			}
			else
			{
				tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
				tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
				tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
				tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			}
			return tess;
		}
		#endif //ASE_TESS_FUNCS
		ENDHLSL

		
		Pass
		{
			
			Name "Forward"
			Tags { "LightMode"="UniversalForwardOnly" }

			Cull Back
			ZWrite On
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA
			Blend One Zero, One Zero
			BlendOp Add, Add

			

			HLSLPROGRAM

			

			#define _NORMAL_DROPOFF_TS 1
			#pragma shader_feature_local_fragment _RECEIVE_SHADOWS_OFF
			#pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
			#pragma multi_compile_instancing
			#pragma instancing_options renderinglayer
			#pragma multi_compile_fragment _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define ASE_VERSION 19910
			#define ASE_SRP_VERSION 140012


			

			#pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3

			

			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON
			#pragma multi_compile_fragment _ DEBUG_DISPLAY

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS SHADERPASS_UNLIT

			
            #if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#endif
		

			
			#if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
			#endif
		

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"

			
			#if ASE_SRP_VERSION >=140009
			#include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
			#endif
		

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Debug/Debugging3D.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceData.hlsl"

			#if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

			#pragma shader_feature_local _ENABLE_RED _ENABLE_GREEN _ENABLE_BLUE


			#if defined(ASE_WRITE_DEPTH_CONSERVATIVE) && (SHADER_TARGET >= 45)
				#define ASE_SV_DEPTH SV_DepthLessEqual
				#define ASE_SV_POSITION_QUALIFIERS linear noperspective centroid
			#else
				#define ASE_SV_DEPTH SV_Depth
				#define ASE_SV_POSITION_QUALIFIERS
			#endif

			struct Attributes
			{
				float4 positionOS : POSITION;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryings
			{
				ASE_SV_POSITION_QUALIFIERS float4 positionCS : SV_POSITION;
				float4 positionWSAndFogFactor : TEXCOORD0;
				half3 normalWS : TEXCOORD1;
				half4 tangentWS : TEXCOORD2;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _Channel;
			float4 _Vector0;
			float4 _ColorA;
			float4 _ColorB;
			float4 _Color0;
			float4 _Color;
			float4 _ColorG;
			float _Max;
			float _ShowIfFalse0;
			float _ConditionDecoratorsGroup;
			float _Toggle;
			float _ReadOnly;
			float _TooltipSample;
			float _AppearanceDecoratorsGroup;
			float _Title;
			float _Range;
			float _Range1;
			float _Float0;
			float _Float;
			float _ShowIfTrue1;
			float _ActiveIfTrue1;
			float _Hidden;
			float _HelpboxSample;
			float _Min;
			float _SubPowerSlider;
			float _SubIntRange;
			float _MinMaxSlider;
			float _SubEnum;
			float _KWEnum;
			float _PassSwitch;
			float _SubKeywordEnum;
			float _float1;
			float _EnumGroup;
			float _RampGroup;
			float _RampAtlasIndexer;
			float _VectorTextureGroup;
			float _NumericGroupToenableASEintegrationyouneedtoInstallASEv19910EnableProjectSettingsLWGUIASEIntegration;
			float _Preset;
			float _BitMask;
			float _AlphaClip;
			float _Cutoff;
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			sampler2D _Ramp;
			sampler2D _RampAtlas;
			sampler2D _TextureSample0;
			sampler2D _TexwithChannel;
			sampler2D _TexwithRange;
			sampler2D _TexwithFloat;


			
			PackedVaryings VertexFunction( Attributes input  )
			{
				PackedVaryings output = (PackedVaryings)0;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					input.positionOS.xyz = vertexValue;
				#else
					input.positionOS.xyz += vertexValue;
				#endif

				input.normalOS = input.normalOS;
				input.tangentOS = input.tangentOS;

				VertexPositionInputs vertexInput = GetVertexPositionInputs( input.positionOS.xyz );
				VertexNormalInputs normalInput = GetVertexNormalInputs( input.normalOS, input.tangentOS );

				float fogFactor = 0;
				#if defined(ASE_FOG) && !defined(_FOG_FRAGMENT)
					fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
				#endif

				output.positionCS = ASE_ADJUST_CLIP_POSITION( vertexInput.positionCS );
				output.positionWSAndFogFactor = float4( vertexInput.positionWS, fogFactor );
				output.normalWS = normalInput.normalWS;
				output.tangentWS = half4( normalInput.tangentWS, ( input.tangentOS.w > 0.0 ? 1.0 : -1.0 ) * GetOddNegativeScale() );;
				return output;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 positionOS : INTERNALTESSPOS;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( Attributes input )
			{
				VertexControl output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				output.positionOS = input.positionOS;
				output.normalOS = input.normalOS;
				output.tangentOS = input.tangentOS;
				
				return output;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> input)
			{
				TessellationFactors output;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				output.edge[0] = tf.x; output.edge[1] = tf.y; output.edge[2] = tf.z; output.inside = tf.w;
				return output;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			PackedVaryings DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				Attributes output = (Attributes) 0;
				output.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				output.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				output.tangentOS = patch[0].tangentOS * bary.x + patch[1].tangentOS * bary.y + patch[2].tangentOS * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = output.positionOS.xyz - patch[i].normalOS * (dot(output.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				output.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * output.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], output);
				return VertexFunction(output);
			}
			#else
			PackedVaryings vert ( Attributes input )
			{
				return VertexFunction( input );
			}
			#endif

			half4 frag ( PackedVaryings input
						#if defined( ASE_WRITE_DEPTH )
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						#ifdef _WRITE_RENDERING_LAYERS
						, out float4 outRenderingLayers : SV_Target1
						#endif
						 ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

				#if defined( _SURFACE_TYPE_TRANSPARENT )
					const bool isTransparent = true;
				#else
					const bool isTransparent = false;
				#endif

				#if defined(LOD_FADE_CROSSFADE)
					LODFadeCrossFade( input.positionCS );
				#endif

				#if defined(MAIN_LIGHT_CALCULATE_SHADOWS)
					float4 shadowCoord = TransformWorldToShadowCoord( input.positionWSAndFogFactor.xyz );
				#else
					float4 shadowCoord = float4(0, 0, 0, 0);
				#endif

				// @diogo: mikktspace compliant
				float renormFactor = 1.0 / max( FLT_MIN, length( input.normalWS ) );

				float3 PositionWS = input.positionWSAndFogFactor.xyz;
				float3 PositionRWS = GetCameraRelativePositionWS( PositionWS );
				half3 ViewDirWS = GetWorldSpaceNormalizeViewDir( PositionWS );
				float4 ShadowCoord = shadowCoord;
				float4 ScreenPosNorm = float4( GetNormalizedScreenSpaceUV( input.positionCS ), input.positionCS.zw );
				float4 ClipPos = ComputeClipSpacePosition( ScreenPosNorm.xy, input.positionCS.z ) * input.positionCS.w;
				float4 ScreenPos = ComputeScreenPos( ClipPos );
				float3 TangentWS = input.tangentWS.xyz * renormFactor;
				float3 BitangentWS = cross( input.normalWS, input.tangentWS.xyz ) * input.tangentWS.w * renormFactor;
				float3 NormalWS = input.normalWS * renormFactor;

				float4 color12_g4 = IsGammaSpace() ? float4( 1, 0, 0, 0 ) : float4( 1, 0, 0, 0 );
				float4 color13_g4 = IsGammaSpace() ? float4( 0, 1, 0, 0 ) : float4( 0, 1, 0, 0 );
				float4 color18_g4 = IsGammaSpace() ? float4( 0, 0, 1, 0 ) : float4( 0, 0, 1, 0 );
				#if defined( _ENABLE_RED )
				float3 staticSwitch8_g4 = color12_g4.rgb;
				#elif defined( _ENABLE_GREEN )
				float3 staticSwitch8_g4 = color13_g4.rgb;
				#elif defined( _ENABLE_BLUE )
				float3 staticSwitch8_g4 = color18_g4.rgb;
				#else
				float3 staticSwitch8_g4 = color12_g4.rgb;
				#endif
				
				float3 BakedAlbedo = 0;
				float3 BakedEmission = 0;
				float3 Color = staticSwitch8_g4;
				float3 Normal = float3(0, 0, 1);
				float Alpha = 1;
				#if defined( _ALPHATEST_ON )
					float AlphaClipThreshold = _Cutoff;
					float AlphaClipThresholdShadow = 0.5;
				#endif


				#if defined( ASE_WRITE_DEPTH )
					input.positionCS.z = input.positionCS.z;
				#endif

				#if defined( _ALPHATEST_ON )
					AlphaDiscard( Alpha, AlphaClipThreshold );
				#endif

				#if defined(MAIN_LIGHT_CALCULATE_SHADOWS) && defined(ASE_CHANGES_WORLD_POS)
					ShadowCoord = TransformWorldToShadowCoord( PositionWS );
				#endif

				InputData inputData = (InputData)0;
				inputData.positionWS = PositionWS;
				inputData.positionCS = input.positionCS;
				inputData.normalizedScreenSpaceUV = ScreenPosNorm.xy;
				inputData.normalWS = NormalWS;
				inputData.viewDirectionWS = ViewDirWS;

				#ifdef ASE_FOG
					inputData.fogCoord = InitializeInputDataFog(float4(inputData.positionWS, 1.0), input.positionWSAndFogFactor.w);
				#endif

				#if defined(_DBUFFER)
					ApplyDecalToBaseColor(input.positionCS, Color);
				#endif

				#ifdef ASE_FOG
					#ifdef TERRAIN_SPLAT_ADDPASS
						Color.rgb = MixFogColor(Color.rgb, half3(0,0,0), inputData.fogCoord);
					#else
						Color.rgb = MixFog(Color.rgb, inputData.fogCoord);
					#endif
				#endif

				#if defined( ASE_WRITE_DEPTH )
					outputDepth = input.positionCS.z;
				#endif

				#ifdef _WRITE_RENDERING_LAYERS
					uint renderingLayers = GetMeshRenderingLayer();
					outRenderingLayers = float4( EncodeMeshRenderingLayer( renderingLayers ), 0, 0, 0 );
				#endif

				#if defined( ASE_OPAQUE_KEEP_ALPHA )
					return half4( Color, Alpha );
				#else
					return half4( Color, OutputAlpha( Alpha, isTransparent ) );
				#endif
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "ShadowCaster"
			Tags { "LightMode"="ShadowCaster" }

			ZWrite On
			ZTest LEqual
			AlphaToMask Off
			ColorMask 0

			HLSLPROGRAM

			

			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_instancing
			#pragma multi_compile_fragment _ LOD_FADE_CROSSFADE
			#define ASE_FOG 1
			#define ASE_VERSION 19910
			#define ASE_SRP_VERSION 140012


			

			#pragma multi_compile _ _CASTING_PUNCTUAL_LIGHT_SHADOW // @diogo: removed _vertex for POM node

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS SHADERPASS_SHADOWCASTER

			
            #if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#endif
		

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

			#if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

			

			#if defined(ASE_WRITE_DEPTH_CONSERVATIVE) && (SHADER_TARGET >= 45)
				#define ASE_SV_DEPTH SV_DepthLessEqual
				#define ASE_SV_POSITION_QUALIFIERS linear noperspective centroid
			#else
				#define ASE_SV_DEPTH SV_Depth
				#define ASE_SV_POSITION_QUALIFIERS
			#endif

			struct Attributes
			{
				float4 positionOS : POSITION;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryings
			{
				ASE_SV_POSITION_QUALIFIERS float4 positionCS : SV_POSITION;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _Channel;
			float4 _Vector0;
			float4 _ColorA;
			float4 _ColorB;
			float4 _Color0;
			float4 _Color;
			float4 _ColorG;
			float _Max;
			float _ShowIfFalse0;
			float _ConditionDecoratorsGroup;
			float _Toggle;
			float _ReadOnly;
			float _TooltipSample;
			float _AppearanceDecoratorsGroup;
			float _Title;
			float _Range;
			float _Range1;
			float _Float0;
			float _Float;
			float _ShowIfTrue1;
			float _ActiveIfTrue1;
			float _Hidden;
			float _HelpboxSample;
			float _Min;
			float _SubPowerSlider;
			float _SubIntRange;
			float _MinMaxSlider;
			float _SubEnum;
			float _KWEnum;
			float _PassSwitch;
			float _SubKeywordEnum;
			float _float1;
			float _EnumGroup;
			float _RampGroup;
			float _RampAtlasIndexer;
			float _VectorTextureGroup;
			float _NumericGroupToenableASEintegrationyouneedtoInstallASEv19910EnableProjectSettingsLWGUIASEIntegration;
			float _Preset;
			float _BitMask;
			float _AlphaClip;
			float _Cutoff;
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			sampler2D _Ramp;
			sampler2D _RampAtlas;
			sampler2D _TextureSample0;
			sampler2D _TexwithChannel;
			sampler2D _TexwithRange;
			sampler2D _TexwithFloat;


			float3 _LightDirection;
			float3 _LightPosition;

			
			PackedVaryings VertexFunction( Attributes input )
			{
				PackedVaryings output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( output );

				

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					input.positionOS.xyz = vertexValue;
				#else
					input.positionOS.xyz += vertexValue;
				#endif

				input.normalOS = input.normalOS;
				input.tangentOS = input.tangentOS;

				float3 positionWS = TransformObjectToWorld( input.positionOS.xyz );
				half3 normalWS = TransformObjectToWorldDir(input.normalOS);

				#if _CASTING_PUNCTUAL_LIGHT_SHADOW
					float3 lightDirectionWS = normalize(_LightPosition - positionWS);
				#else
					float3 lightDirectionWS = _LightDirection;
				#endif

				float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));

				#if UNITY_REVERSED_Z
					positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
				#else
					positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
				#endif

				output.positionCS = ASE_ADJUST_CLIP_POSITION( positionCS );
				return output;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 positionOS : INTERNALTESSPOS;
				half3 normalOS : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( Attributes input )
			{
				VertexControl output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				output.positionOS = input.positionOS;
				output.normalOS = input.normalOS;
				
				return output;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> input)
			{
				TessellationFactors output;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				output.edge[0] = tf.x; output.edge[1] = tf.y; output.edge[2] = tf.z; output.inside = tf.w;
				return output;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			PackedVaryings DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				Attributes output = (Attributes) 0;
				output.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				output.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = output.positionOS.xyz - patch[i].normalOS * (dot(output.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				output.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * output.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], output);
				return VertexFunction(output);
			}
			#else
			PackedVaryings vert ( Attributes input )
			{
				return VertexFunction( input );
			}
			#endif

			half4 frag(PackedVaryings input
						#if defined( ASE_WRITE_DEPTH )
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						 ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( input );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( input );

				float4 ScreenPosNorm = float4( GetNormalizedScreenSpaceUV( input.positionCS ), input.positionCS.zw );
				float4 ClipPos = ComputeClipSpacePosition( ScreenPosNorm.xy, input.positionCS.z ) * input.positionCS.w;
				float4 ScreenPos = ComputeScreenPos( ClipPos );

				

				float Alpha = 1;
				#if defined( _ALPHATEST_ON )
					float AlphaClipThreshold = _Cutoff;
					float AlphaClipThresholdShadow = 0.5;
				#endif

				#if defined( ASE_WRITE_DEPTH )
					input.positionCS.z = input.positionCS.z;
				#endif

				#if defined( _ALPHATEST_ON )
					#if defined( _ALPHATEST_SHADOW_ON )
						AlphaDiscard( Alpha, AlphaClipThresholdShadow );
					#else
						AlphaDiscard( Alpha, AlphaClipThreshold );
					#endif
				#endif

				#if defined(LOD_FADE_CROSSFADE)
					LODFadeCrossFade( input.positionCS );
				#endif

				#if defined( ASE_WRITE_DEPTH )
					outputDepth = input.positionCS.z;
				#endif

				return 0;
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "DepthOnly"
			Tags { "LightMode"="DepthOnly" }

			ZWrite On
			ColorMask R
			AlphaToMask Off

			HLSLPROGRAM

			

			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_instancing
			#pragma multi_compile_fragment _ LOD_FADE_CROSSFADE
			#define ASE_FOG 1
			#define ASE_VERSION 19910
			#define ASE_SRP_VERSION 140012


			

			#pragma vertex vert
			#pragma fragment frag

			
            #if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#endif
		

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

			#if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

			

			#if defined(ASE_WRITE_DEPTH_CONSERVATIVE) && (SHADER_TARGET >= 45)
				#define ASE_SV_DEPTH SV_DepthLessEqual
				#define ASE_SV_POSITION_QUALIFIERS linear noperspective centroid
			#else
				#define ASE_SV_DEPTH SV_Depth
				#define ASE_SV_POSITION_QUALIFIERS
			#endif

			struct Attributes
			{
				float4 positionOS : POSITION;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryings
			{
				ASE_SV_POSITION_QUALIFIERS float4 positionCS : SV_POSITION;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _Channel;
			float4 _Vector0;
			float4 _ColorA;
			float4 _ColorB;
			float4 _Color0;
			float4 _Color;
			float4 _ColorG;
			float _Max;
			float _ShowIfFalse0;
			float _ConditionDecoratorsGroup;
			float _Toggle;
			float _ReadOnly;
			float _TooltipSample;
			float _AppearanceDecoratorsGroup;
			float _Title;
			float _Range;
			float _Range1;
			float _Float0;
			float _Float;
			float _ShowIfTrue1;
			float _ActiveIfTrue1;
			float _Hidden;
			float _HelpboxSample;
			float _Min;
			float _SubPowerSlider;
			float _SubIntRange;
			float _MinMaxSlider;
			float _SubEnum;
			float _KWEnum;
			float _PassSwitch;
			float _SubKeywordEnum;
			float _float1;
			float _EnumGroup;
			float _RampGroup;
			float _RampAtlasIndexer;
			float _VectorTextureGroup;
			float _NumericGroupToenableASEintegrationyouneedtoInstallASEv19910EnableProjectSettingsLWGUIASEIntegration;
			float _Preset;
			float _BitMask;
			float _AlphaClip;
			float _Cutoff;
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			sampler2D _Ramp;
			sampler2D _RampAtlas;
			sampler2D _TextureSample0;
			sampler2D _TexwithChannel;
			sampler2D _TexwithRange;
			sampler2D _TexwithFloat;


			float3 _LightDirection;
			float3 _LightPosition;

			
			PackedVaryings VertexFunction( Attributes input  )
			{
				PackedVaryings output = (PackedVaryings)0;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					input.positionOS.xyz = vertexValue;
				#else
					input.positionOS.xyz += vertexValue;
				#endif

				VertexPositionInputs vertexInput = GetVertexPositionInputs( input.positionOS.xyz );

				output.positionCS = ASE_ADJUST_CLIP_POSITION( vertexInput.positionCS );
				return output;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 positionOS : INTERNALTESSPOS;
				half3 normalOS : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( Attributes input )
			{
				VertexControl output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				output.positionOS = input.positionOS;
				output.normalOS = input.normalOS;
				
				return output;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> input)
			{
				TessellationFactors output;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				output.edge[0] = tf.x; output.edge[1] = tf.y; output.edge[2] = tf.z; output.inside = tf.w;
				return output;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			PackedVaryings DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				Attributes output = (Attributes) 0;
				output.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				output.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = output.positionOS.xyz - patch[i].normalOS * (dot(output.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				output.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * output.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], output);
				return VertexFunction(output);
			}
			#else
			PackedVaryings vert ( Attributes input )
			{
				return VertexFunction( input );
			}
			#endif

			half4 frag(PackedVaryings input
						#if defined( ASE_WRITE_DEPTH )
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						 ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( input );

				float4 ScreenPosNorm = float4( GetNormalizedScreenSpaceUV( input.positionCS ), input.positionCS.zw );
				float4 ClipPos = ComputeClipSpacePosition( ScreenPosNorm.xy, input.positionCS.z ) * input.positionCS.w;
				float4 ScreenPos = ComputeScreenPos( ClipPos );

				

				float Alpha = 1;
				#if defined( _ALPHATEST_ON )
					float AlphaClipThreshold = _Cutoff;
				#endif

				#if defined( ASE_WRITE_DEPTH )
					input.positionCS.z = input.positionCS.z;
				#endif

				#if defined( _ALPHATEST_ON )
					AlphaDiscard( Alpha, AlphaClipThreshold );
				#endif

				#if defined(LOD_FADE_CROSSFADE)
					LODFadeCrossFade( input.positionCS );
				#endif

				#if defined( ASE_WRITE_DEPTH )
					outputDepth = input.positionCS.z;
				#endif

				return 0;
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "SceneSelectionPass"
			Tags { "LightMode"="SceneSelectionPass" }

			Cull Off
			AlphaToMask Off

			HLSLPROGRAM

			

			#define _NORMAL_DROPOFF_TS 1
			#define ASE_FOG 1
			#define ASE_VERSION 19910
			#define ASE_SRP_VERSION 140012


			

			#pragma vertex vert
			#pragma fragment frag

			#define ATTRIBUTES_NEED_NORMAL
			#define ATTRIBUTES_NEED_TANGENT
			#define SHADERPASS SHADERPASS_DEPTHONLY

			
            #if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#endif
		

			
			#if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
			#endif
		

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"

			
			#if ASE_SRP_VERSION >=140009
			#include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
			#endif
		

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			

			struct Attributes
			{
				float4 positionOS : POSITION;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryings
			{
				float4 positionCS : SV_POSITION;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _Channel;
			float4 _Vector0;
			float4 _ColorA;
			float4 _ColorB;
			float4 _Color0;
			float4 _Color;
			float4 _ColorG;
			float _Max;
			float _ShowIfFalse0;
			float _ConditionDecoratorsGroup;
			float _Toggle;
			float _ReadOnly;
			float _TooltipSample;
			float _AppearanceDecoratorsGroup;
			float _Title;
			float _Range;
			float _Range1;
			float _Float0;
			float _Float;
			float _ShowIfTrue1;
			float _ActiveIfTrue1;
			float _Hidden;
			float _HelpboxSample;
			float _Min;
			float _SubPowerSlider;
			float _SubIntRange;
			float _MinMaxSlider;
			float _SubEnum;
			float _KWEnum;
			float _PassSwitch;
			float _SubKeywordEnum;
			float _float1;
			float _EnumGroup;
			float _RampGroup;
			float _RampAtlasIndexer;
			float _VectorTextureGroup;
			float _NumericGroupToenableASEintegrationyouneedtoInstallASEv19910EnableProjectSettingsLWGUIASEIntegration;
			float _Preset;
			float _BitMask;
			float _AlphaClip;
			float _Cutoff;
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			sampler2D _Ramp;
			sampler2D _RampAtlas;
			sampler2D _TextureSample0;
			sampler2D _TexwithChannel;
			sampler2D _TexwithRange;
			sampler2D _TexwithFloat;


			
			int _ObjectId;
			int _PassValue;

			struct SurfaceDescription
			{
				float Alpha;
				float AlphaClipThreshold;
			};

			PackedVaryings VertexFunction(Attributes input  )
			{
				PackedVaryings output;
				ZERO_INITIALIZE(PackedVaryings, output);

				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					input.positionOS.xyz = vertexValue;
				#else
					input.positionOS.xyz += vertexValue;
				#endif

				VertexPositionInputs vertexInput = GetVertexPositionInputs( input.positionOS.xyz );

				output.positionCS = ASE_ADJUST_CLIP_POSITION( vertexInput.positionCS );
				return output;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 positionOS : INTERNALTESSPOS;
				half3 normalOS : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( Attributes input )
			{
				VertexControl output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				output.positionOS = input.positionOS;
				output.normalOS = input.normalOS;
				
				return output;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> input)
			{
				TessellationFactors output;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				output.edge[0] = tf.x; output.edge[1] = tf.y; output.edge[2] = tf.z; output.inside = tf.w;
				return output;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			PackedVaryings DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				Attributes output = (Attributes) 0;
				output.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				output.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = output.positionOS.xyz - patch[i].normalOS * (dot(output.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				output.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * output.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], output);
				return VertexFunction(output);
			}
			#else
			PackedVaryings vert ( Attributes input )
			{
				return VertexFunction( input );
			}
			#endif

			half4 frag(PackedVaryings input ) : SV_Target
			{
				SurfaceDescription surfaceDescription = (SurfaceDescription)0;

				

				surfaceDescription.Alpha = 1;
				#if defined( _ALPHATEST_ON )
					surfaceDescription.AlphaClipThreshold = _Cutoff;
				#endif

				#ifdef _ALPHATEST_ON
					clip(surfaceDescription.Alpha - surfaceDescription.AlphaClipThreshold);
				#endif

				half4 outColor = half4(_ObjectId, _PassValue, 1.0, 1.0);
				return outColor;
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "ScenePickingPass"
			Tags { "LightMode"="Picking" }

			AlphaToMask Off

			HLSLPROGRAM

			

			#define _NORMAL_DROPOFF_TS 1
			#define ASE_FOG 1
			#define ASE_VERSION 19910
			#define ASE_SRP_VERSION 140012


			

			#pragma vertex vert
			#pragma fragment frag

			#define ATTRIBUTES_NEED_NORMAL
			#define ATTRIBUTES_NEED_TANGENT

			#define SHADERPASS SHADERPASS_DEPTHONLY

			
            #if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#endif
		

			
			#if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
			#endif
		

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"

			
			#if ASE_SRP_VERSION >=140009
			#include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
			#endif
		

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

			

			struct Attributes
			{
				float4 positionOS : POSITION;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryings
			{
				float4 positionCS : SV_POSITION;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _Channel;
			float4 _Vector0;
			float4 _ColorA;
			float4 _ColorB;
			float4 _Color0;
			float4 _Color;
			float4 _ColorG;
			float _Max;
			float _ShowIfFalse0;
			float _ConditionDecoratorsGroup;
			float _Toggle;
			float _ReadOnly;
			float _TooltipSample;
			float _AppearanceDecoratorsGroup;
			float _Title;
			float _Range;
			float _Range1;
			float _Float0;
			float _Float;
			float _ShowIfTrue1;
			float _ActiveIfTrue1;
			float _Hidden;
			float _HelpboxSample;
			float _Min;
			float _SubPowerSlider;
			float _SubIntRange;
			float _MinMaxSlider;
			float _SubEnum;
			float _KWEnum;
			float _PassSwitch;
			float _SubKeywordEnum;
			float _float1;
			float _EnumGroup;
			float _RampGroup;
			float _RampAtlasIndexer;
			float _VectorTextureGroup;
			float _NumericGroupToenableASEintegrationyouneedtoInstallASEv19910EnableProjectSettingsLWGUIASEIntegration;
			float _Preset;
			float _BitMask;
			float _AlphaClip;
			float _Cutoff;
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			sampler2D _Ramp;
			sampler2D _RampAtlas;
			sampler2D _TextureSample0;
			sampler2D _TexwithChannel;
			sampler2D _TexwithRange;
			sampler2D _TexwithFloat;


			
			float4 _SelectionID;

			struct SurfaceDescription
			{
				float Alpha;
				float AlphaClipThreshold;
			};

			PackedVaryings VertexFunction(Attributes input  )
			{
				PackedVaryings output;
				ZERO_INITIALIZE(PackedVaryings, output);

				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					input.positionOS.xyz = vertexValue;
				#else
					input.positionOS.xyz += vertexValue;
				#endif

				VertexPositionInputs vertexInput = GetVertexPositionInputs( input.positionOS.xyz );

				output.positionCS = ASE_ADJUST_CLIP_POSITION( vertexInput.positionCS );
				return output;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 positionOS : INTERNALTESSPOS;
				half3 normalOS : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( Attributes input )
			{
				VertexControl output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				output.positionOS = input.positionOS;
				output.normalOS = input.normalOS;
				
				return output;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> input)
			{
				TessellationFactors output;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				output.edge[0] = tf.x; output.edge[1] = tf.y; output.edge[2] = tf.z; output.inside = tf.w;
				return output;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			PackedVaryings DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				Attributes output = (Attributes) 0;
				output.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				output.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = output.positionOS.xyz - patch[i].normalOS * (dot(output.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				output.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * output.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], output);
				return VertexFunction(output);
			}
			#else
			PackedVaryings vert ( Attributes input )
			{
				return VertexFunction( input );
			}
			#endif

			half4 frag(PackedVaryings input ) : SV_Target
			{
				SurfaceDescription surfaceDescription = (SurfaceDescription)0;

				

				surfaceDescription.Alpha = 1;
				#if defined( _ALPHATEST_ON )
					surfaceDescription.AlphaClipThreshold = _Cutoff;
				#endif

				#ifdef _ALPHATEST_ON
					clip(surfaceDescription.Alpha - surfaceDescription.AlphaClipThreshold);
				#endif

				half4 outColor = 0;
				outColor = unity_SelectionID;

				return outColor;
			}

			ENDHLSL
		}

		
		Pass
		{
			
			Name "DepthNormals"
			Tags { "LightMode"="DepthNormalsOnly" }

			ZTest LEqual
			ZWrite On

			HLSLPROGRAM

			

        	#define _NORMAL_DROPOFF_TS 1
        	#pragma multi_compile_instancing
        	#pragma multi_compile_fragment _ LOD_FADE_CROSSFADE
        	#define ASE_FOG 1
        	#define ASE_VERSION 19910
        	#define ASE_SRP_VERSION 140012


			

        	#pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT

			

			#pragma vertex vert
			#pragma fragment frag

			#define ATTRIBUTES_NEED_NORMAL
			#define ATTRIBUTES_NEED_TANGENT
			#define VARYINGS_NEED_NORMAL_WS
			#define VARYINGS_NEED_TANGENT_WS

			#define SHADERPASS SHADERPASS_DEPTHNORMALSONLY

			
            #if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#endif
		

			
			#if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
			#endif
		

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"

			
			#if ASE_SRP_VERSION >=140009
			#include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
			#endif
		

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

            #if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

			

			#if defined(ASE_WRITE_DEPTH_CONSERVATIVE) && (SHADER_TARGET >= 45)
				#define ASE_SV_DEPTH SV_DepthLessEqual
				#define ASE_SV_POSITION_QUALIFIERS linear noperspective centroid
			#else
				#define ASE_SV_DEPTH SV_Depth
				#define ASE_SV_POSITION_QUALIFIERS
			#endif

			struct Attributes
			{
				float4 positionOS : POSITION;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryings
			{
				ASE_SV_POSITION_QUALIFIERS float4 positionCS : SV_POSITION;
				half3 normalWS : TEXCOORD0;
				float4 tangentWS : TEXCOORD1;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _Channel;
			float4 _Vector0;
			float4 _ColorA;
			float4 _ColorB;
			float4 _Color0;
			float4 _Color;
			float4 _ColorG;
			float _Max;
			float _ShowIfFalse0;
			float _ConditionDecoratorsGroup;
			float _Toggle;
			float _ReadOnly;
			float _TooltipSample;
			float _AppearanceDecoratorsGroup;
			float _Title;
			float _Range;
			float _Range1;
			float _Float0;
			float _Float;
			float _ShowIfTrue1;
			float _ActiveIfTrue1;
			float _Hidden;
			float _HelpboxSample;
			float _Min;
			float _SubPowerSlider;
			float _SubIntRange;
			float _MinMaxSlider;
			float _SubEnum;
			float _KWEnum;
			float _PassSwitch;
			float _SubKeywordEnum;
			float _float1;
			float _EnumGroup;
			float _RampGroup;
			float _RampAtlasIndexer;
			float _VectorTextureGroup;
			float _NumericGroupToenableASEintegrationyouneedtoInstallASEv19910EnableProjectSettingsLWGUIASEIntegration;
			float _Preset;
			float _BitMask;
			float _AlphaClip;
			float _Cutoff;
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			sampler2D _Ramp;
			sampler2D _RampAtlas;
			sampler2D _TextureSample0;
			sampler2D _TexwithChannel;
			sampler2D _TexwithRange;
			sampler2D _TexwithFloat;


			
			struct SurfaceDescription
			{
				float Alpha;
				float AlphaClipThreshold;
			};

			PackedVaryings VertexFunction( Attributes input  )
			{
				PackedVaryings output;
				ZERO_INITIALIZE(PackedVaryings, output);

				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					input.positionOS.xyz = vertexValue;
				#else
					input.positionOS.xyz += vertexValue;
				#endif

				input.normalOS = input.normalOS;
				input.tangentOS = input.tangentOS;

				VertexPositionInputs vertexInput = GetVertexPositionInputs( input.positionOS.xyz );
				VertexNormalInputs normalInput = GetVertexNormalInputs( input.normalOS, input.tangentOS );

				output.positionCS = ASE_ADJUST_CLIP_POSITION( vertexInput.positionCS );
				output.normalWS = normalInput.normalWS;
				output.tangentWS = float4( normalInput.tangentWS, ( input.tangentOS.w > 0.0 ? 1.0 : -1.0 ) * GetOddNegativeScale() );
				return output;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 positionOS : INTERNALTESSPOS;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( Attributes input )
			{
				VertexControl output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				output.positionOS = input.positionOS;
				output.normalOS = input.normalOS;
				output.tangentOS = input.tangentOS;
				
				return output;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> input)
			{
				TessellationFactors output;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				output.edge[0] = tf.x; output.edge[1] = tf.y; output.edge[2] = tf.z; output.inside = tf.w;
				return output;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			PackedVaryings DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				Attributes output = (Attributes) 0;
				output.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				output.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				output.tangentOS = patch[0].tangentOS * bary.x + patch[1].tangentOS * bary.y + patch[2].tangentOS * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = output.positionOS.xyz - patch[i].normalOS * (dot(output.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				output.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * output.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], output);
				return VertexFunction(output);
			}
			#else
			PackedVaryings vert ( Attributes input )
			{
				return VertexFunction( input );
			}
			#endif

			void frag(PackedVaryings input
						, out half4 outNormalWS : SV_Target0
						#if defined( ASE_WRITE_DEPTH )
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						#ifdef _WRITE_RENDERING_LAYERS
						, out float4 outRenderingLayers : SV_Target1
						#endif
						 )
			{
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( input );

				// @diogo: mikktspace compliant
				float renormFactor = 1.0 / max( FLT_MIN, length( input.normalWS ) );

				float3 NormalWS = input.normalWS * renormFactor;
				float3 TangentWS = input.tangentWS.xyz * renormFactor;
				float3 BitangentWS = cross( input.normalWS, input.tangentWS.xyz ) * input.tangentWS.w * renormFactor;
				float4 ScreenPosNorm = float4( GetNormalizedScreenSpaceUV( input.positionCS ), input.positionCS.zw );
				float4 ClipPos = ComputeClipSpacePosition( ScreenPosNorm.xy, input.positionCS.z ) * input.positionCS.w;
				float4 ScreenPos = ComputeScreenPos( ClipPos );

				

				float3 Normal = float3(0, 0, 1);
				float Alpha = 1;
				#if defined( _ALPHATEST_ON )
					float AlphaClipThreshold = _Cutoff;
				#endif

				#if defined( ASE_WRITE_DEPTH )
					input.positionCS.z = input.positionCS.z;
				#endif

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				#if defined(LOD_FADE_CROSSFADE)
					LODFadeCrossFade( input.positionCS );
				#endif

				#if defined( ASE_WRITE_DEPTH )
					outputDepth = input.positionCS.z;
				#endif

				#if defined(_GBUFFER_NORMALS_OCT)
					float2 octNormalWS = PackNormalOctQuadEncode(NormalWS);
					float2 remappedOctNormalWS = saturate(octNormalWS * 0.5 + 0.5);
					half3 packedNormalWS = PackFloat2To888(remappedOctNormalWS);
					outNormalWS = half4(packedNormalWS, 0.0);
				#else
					#if defined(_NORMALMAP)
						#if _NORMAL_DROPOFF_TS
							float3 normalWS = TransformTangentToWorld(Normal, half3x3(TangentWS, BitangentWS, NormalWS));
						#elif _NORMAL_DROPOFF_OS
							float3 normalWS = TransformObjectToWorldNormal(Normal);
						#elif _NORMAL_DROPOFF_WS
							float3 normalWS = Normal;
						#endif
					#else
						float3 normalWS = NormalWS;
					#endif
					outNormalWS = half4(NormalizeNormalPerPixel(normalWS), 0.0);
				#endif

				#ifdef _WRITE_RENDERING_LAYERS
					uint renderingLayers = GetMeshRenderingLayer();
					outRenderingLayers = float4(EncodeMeshRenderingLayer(renderingLayers), 0, 0, 0);
				#endif
			}
			ENDHLSL
		}

	
	}
	

	

	CustomEditor "LWGUI.LWGUI"
	FallBack "Hidden/Shader Graph/FallbackError"
	
	Fallback Off
}
/*ASEBEGIN
Version=19910
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":53,"pos":[-864,3872],"params":["Inherit","False","628.9534","920.3547","Out of Group","5","52","49","51","50","48","","1,1,1,1","0","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":11,"pos":[-848,896],"params":["Inherit","False","1356.853","1478.783","Vector Group","5","32","31","54","55","56","","1,1,1,1","0","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":41,"pos":[-848,3088],"params":["Inherit","False","589.4493","699.1997","Condition Decorators Group","6","47","45","44","42","43","40","","1,1,1,1","0","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":39,"pos":[-800,1040],"params":["Inherit","False","1311.946","286.7615","Color and other Drawers will reference the names of other Properties. Currently, you must manually lock these Property names to prevent the auto-generated names from changing.","4","27","30","29","28","","1,1,1,1","0","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":10,"pos":[-848,2416],"params":["Inherit","False","668.1052","615.8208","Appearance Decorators Group","6","37","33","36","35","34","46","","1,1,1,1","0","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":12,"pos":[-848,144],"params":["Inherit","False","557.5201","697.4403","Ramp Group","4","26","25","22","21","","1,1,1,1","0","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":13,"pos":[-848,-720],"params":["Inherit","False","675.7715","709.7142","Numeric Group","9","16","17","23","20","19","18","15","14","57","","1,1,1,1","0","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":14,"pos":[-512,-192],"params":["Inherit","False","Property","_Max","_Max","7","0","Create","True","0","0","0","True","0","False","Object","-1","LWGUI:348:eyJkcmF3ZXIiOnsiX2RyYXdlclR5cGVOYW1lIjoiIiwiX3BhcmFtZXRlcnMiOltdLCJfaXNEZWNvcmF0b3IiOmZhbHNlLCJfY29uc3RydWN0b3JTaWduYXR1cmUiOiIiLCJfbmFtZWRQYXJhbWV0ZXJzIjpbXX0sImRlY29yYXRvcnMiOlt7Il9kcmF3ZXJUeXBlTmFtZSI6IkhpZGRlbkRlY29yYXRvciIsIl9wYXJhbWV0ZXJzIjpbXSwiX2lzRGVjb3JhdG9yIjp0cnVlLCJfY29uc3RydWN0b3JTaWduYXR1cmUiOiIiLCJfbmFtZWRQYXJhbWV0ZXJzIjpbXX1dfQ==","1","0","-1","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":15,"pos":[-512,-256],"params":["Inherit","False","Property","_Min","_Min","6","0","Create","True","0","0","0","True","0","False","Object","-1","LWGUI:348:eyJkcmF3ZXIiOnsiX2RyYXdlclR5cGVOYW1lIjoiIiwiX3BhcmFtZXRlcnMiOltdLCJfaXNEZWNvcmF0b3IiOmZhbHNlLCJfY29uc3RydWN0b3JTaWduYXR1cmUiOiIiLCJfbmFtZWRQYXJhbWV0ZXJzIjpbXX0sImRlY29yYXRvcnMiOlt7Il9kcmF3ZXJUeXBlTmFtZSI6IkhpZGRlbkRlY29yYXRvciIsIl9wYXJhbWV0ZXJzIjpbXSwiX2lzRGVjb3JhdG9yIjp0cnVlLCJfY29uc3RydWN0b3JTaWduYXR1cmUiOiIiLCJfbmFtZWRQYXJhbWV0ZXJzIjpbXX1dfQ==","-1","0","-1","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":19,"pos":[-784,-352],"params":["Inherit","False","Property","_SubIntRange","Sub Int Range","4","0","Create","True","0","0","0","True","0","False","Object","-1","LWGUI:280:eyJkcmF3ZXIiOnsiX2RyYXdlclR5cGVOYW1lIjoiU3ViSW50UmFuZ2VEcmF3ZXIiLCJfcGFyYW1ldGVycyI6WyJOdW1lcmljIl0sIl9pc0RlY29yYXRvciI6ZmFsc2UsIl9jb25zdHJ1Y3RvclNpZ25hdHVyZSI6InN0cmluZyBncm91cCIsIl9uYW1lZFBhcmFtZXRlcnMiOlt7Im5hbWUiOiJncm91cCIsInZhbHVlIjoiTnVtZXJpYyJ9XX0sImRlY29yYXRvcnMiOltdfQ==","0","0","-10","10","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":20,"pos":[-784,-240],"params":["Inherit","False","Property","_MinMaxSlider","Min Max Slider","5","0","Create","True","0","0","0","True","0","False","Object","-1","LWGUI:452:eyJkcmF3ZXIiOnsiX2RyYXdlclR5cGVOYW1lIjoiTWluTWF4U2xpZGVyRHJhd2VyIiwiX3BhcmFtZXRlcnMiOlsiTnVtZXJpYyIsIl9NaW4iLCJfTWF4Il0sIl9pc0RlY29yYXRvciI6ZmFsc2UsIl9jb25zdHJ1Y3RvclNpZ25hdHVyZSI6InN0cmluZyBncm91cCxzdHJpbmcgbWluUHJvcE5hbWUsc3RyaW5nIG1heFByb3BOYW1lIiwiX25hbWVkUGFyYW1ldGVycyI6W3sibmFtZSI6Imdyb3VwIiwidmFsdWUiOiJOdW1lcmljIn0seyJuYW1lIjoibWluUHJvcE5hbWUiLCJ2YWx1ZSI6Il9NaW4ifSx7Im5hbWUiOiJtYXhQcm9wTmFtZSIsInZhbHVlIjoiX01heCJ9XX0sImRlY29yYXRvcnMiOltdfQ==","0","0","-1","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.TexturePropertyNode, AmplifyShaderEditor","id":21,"pos":[-784,288],"params":["Inherit","True","Property","_Ramp","Ramp","17","0","Create","True","0","0","0","True","0","False","LWGUI:260:eyJkcmF3ZXIiOnsiX2RyYXdlclR5cGVOYW1lIjoiUmFtcERyYXdlciIsIl9wYXJhbWV0ZXJzIjpbIlJhbXAiXSwiX2lzRGVjb3JhdG9yIjpmYWxzZSwiX2NvbnN0cnVjdG9yU2lnbmF0dXJlIjoic3RyaW5nIGdyb3VwIiwiX25hbWVkUGFyYW1ldGVycyI6W3sibmFtZSI6Imdyb3VwIiwidmFsdWUiOiJSYW1wIn1dfSwiZGVjb3JhdG9ycyI6W119","1522a7c4f7317044ab912724c1594427","1522a7c4f7317044ab912724c1594427","False","white","Auto","Texture2D","False","-1","0","2","SAMPLER2D","0","SAMPLERSTATE","1"]}
{"type":"AmplifyShaderEditor.TexturePropertyNode, AmplifyShaderEditor","id":22,"pos":[-784,496],"params":["Inherit","True","Property","_RampAtlas","Ramp Atlas","18","0","Create","True","0","0","0","True","0","False","LWGUI:268:eyJkcmF3ZXIiOnsiX2RyYXdlclR5cGVOYW1lIjoiUmFtcEF0bGFzRHJhd2VyIiwiX3BhcmFtZXRlcnMiOlsiUmFtcCJdLCJfaXNEZWNvcmF0b3IiOmZhbHNlLCJfY29uc3RydWN0b3JTaWduYXR1cmUiOiJzdHJpbmcgZ3JvdXAiLCJfbmFtZWRQYXJhbWV0ZXJzIjpbeyJuYW1lIjoiZ3JvdXAiLCJ2YWx1ZSI6IlJhbXAifV19LCJkZWNvcmF0b3JzIjpbXX0=","cfbe5a628e96c0d4bae3645ad922a368","cfbe5a628e96c0d4bae3645ad922a368","False","white","Auto","Texture2D","False","-1","0","2","SAMPLER2D","0","SAMPLERSTATE","1"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":24,"pos":[-784,16],"params":["Inherit","False","SampleAmplifyFunction_Enum","9","","4","47c66267a97c66a4f950ad83bd0e4064","0","0","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":25,"pos":[-784,192],"params":["Inherit","False","Property","_RampGroup","====== Ramp Group ======","16","0","Create","True","0","0","0","True","0","False","Object","-1","LWGUI:532:eyJkcmF3ZXIiOnsiX2RyYXdlclR5cGVOYW1lIjoiTWFpbkRyYXdlciIsIl9wYXJhbWV0ZXJzIjpbIlJhbXAiLCIiLCJvbiIsIm9uIl0sIl9pc0RlY29yYXRvciI6ZmFsc2UsIl9jb25zdHJ1Y3RvclNpZ25hdHVyZSI6InN0cmluZyBncm91cCxrZXl3b3JkIGtleXdvcmQsZW51bSBkZWZhdWx0Rm9sZGluZ1N0YXRlLGVudW0gZGVmYXVsdFRvZ2dsZURpc3BsYXllZCIsIl9uYW1lZFBhcmFtZXRlcnMiOlt7Im5hbWUiOiJncm91cCIsInZhbHVlIjoiUmFtcCJ9LHsibmFtZSI6ImtleXdvcmQiLCJ2YWx1ZSI6IiJ9LHsibmFtZSI6ImRlZmF1bHRGb2xkaW5nU3RhdGUiLCJ2YWx1ZSI6Im9uIn0seyJuYW1lIjoiZGVmYXVsdFRvZ2dsZURpc3BsYXllZCIsInZhbHVlIjoib24ifV19LCJkZWNvcmF0b3JzIjpbXX0=","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":26,"pos":[-784,720],"params":["Inherit","False","Property","_RampAtlasIndexer","Ramp Atlas Indexer","19","0","Create","True","0","0","0","True","0","False","Object","-1","LWGUI:396:eyJkcmF3ZXIiOnsiX2RyYXdlclR5cGVOYW1lIjoiUmFtcEF0bGFzSW5kZXhlckRyYXdlciIsIl9wYXJhbWV0ZXJzIjpbIlJhbXAiLCJfUmFtcEF0bGFzIl0sIl9pc0RlY29yYXRvciI6ZmFsc2UsIl9jb25zdHJ1Y3RvclNpZ25hdHVyZSI6InN0cmluZyBncm91cCxzdHJpbmcgcmFtcEF0bGFzUHJvcE5hbWUiLCJfbmFtZWRQYXJhbWV0ZXJzIjpbeyJuYW1lIjoiZ3JvdXAiLCJ2YWx1ZSI6IlJhbXAifSx7Im5hbWUiOiJyYW1wQXRsYXNQcm9wTmFtZSIsInZhbHVlIjoiX1JhbXBBdGxhcyJ9XX0sImRlY29yYXRvcnMiOltdfQ==","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.ColorNode, AmplifyShaderEditor","id":27,"pos":[-528,1104],"params":["Inherit","False","Property","_ColorG","Color G","22","1","[HideInInspector]","Create","False","0","0","0","True","0","False","Object","-1","","0,1,0,1","0,0,0,0","True","True","0","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.ColorNode, AmplifyShaderEditor","id":30,"pos":[-736,1104],"params":["Inherit","False","Property","_Color","Color","21","0","Create","True","0","0","0","True","0","False","Object","-1","LWGUI:508:eyJkcmF3ZXIiOnsiX2RyYXdlclR5cGVOYW1lIjoiQ29sb3JEcmF3ZXIiLCJfcGFyYW1ldGVycyI6WyJWZWN0b3IiLCJfQ29sb3JHIiwiX0NvbG9yQiIsIl9Db2xvckEiXSwiX2lzRGVjb3JhdG9yIjpmYWxzZSwiX2NvbnN0cnVjdG9yU2lnbmF0dXJlIjoic3RyaW5nIGdyb3VwLHN0cmluZyBjb2xvcjIsc3RyaW5nIGNvbG9yMyxzdHJpbmcgY29sb3I0IiwiX25hbWVkUGFyYW1ldGVycyI6W3sibmFtZSI6Imdyb3VwIiwidmFsdWUiOiJWZWN0b3IifSx7Im5hbWUiOiJjb2xvcjIiLCJ2YWx1ZSI6Il9Db2xvckcifSx7Im5hbWUiOiJjb2xvcjMiLCJ2YWx1ZSI6Il9Db2xvckIifSx7Im5hbWUiOiJjb2xvcjQiLCJ2YWx1ZSI6Il9Db2xvckEifV19LCJkZWNvcmF0b3JzIjpbXX0=","1,0,0,1","0,0,0,0","True","True","0","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.ColorNode, AmplifyShaderEditor","id":28,"pos":[-320,1104],"params":["Inherit","False","Property","_ColorB","Color B","23","1","[HideInInspector]","Create","False","0","0","0","True","0","False","Object","-1","","0,0,1,1","0,0,0,0","True","True","0","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.ColorNode, AmplifyShaderEditor","id":29,"pos":[-112,1104],"params":["Inherit","False","Property","_ColorA","Color A","24","1","[HideInInspector]","Create","False","0","0","0","True","0","False","Object","-1","","1,1,1,1","0,0,0,0","True","True","0","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.Vector4Node, AmplifyShaderEditor","id":31,"pos":[-800,1360],"params":["Inherit","False","Property","_Channel","Channel","25","0","Create","False","0","0","0","True","0","False","Object","-1","LWGUI:272:eyJkcmF3ZXIiOnsiX2RyYXdlclR5cGVOYW1lIjoiQ2hhbm5lbERyYXdlciIsIl9wYXJhbWV0ZXJzIjpbIlZlY3RvciJdLCJfaXNEZWNvcmF0b3IiOmZhbHNlLCJfY29uc3RydWN0b3JTaWduYXR1cmUiOiJzdHJpbmcgZ3JvdXAiLCJfbmFtZWRQYXJhbWV0ZXJzIjpbeyJuYW1lIjoiZ3JvdXAiLCJ2YWx1ZSI6IlZlY3RvciJ9XX0sImRlY29yYXRvcnMiOltdfQ==","0,1,0,0","0,0,0,0","0","5","FLOAT4","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":32,"pos":[-800,944],"params":["Inherit","False","Property","_VectorTextureGroup","====== Vector / Texture Group ======","20","0","Create","True","0","0","0","True","0","False","Object","-1","LWGUI:536:eyJkcmF3ZXIiOnsiX2RyYXdlclR5cGVOYW1lIjoiTWFpbkRyYXdlciIsIl9wYXJhbWV0ZXJzIjpbIlZlY3RvciIsIiIsIm9uIiwib24iXSwiX2lzRGVjb3JhdG9yIjpmYWxzZSwiX2NvbnN0cnVjdG9yU2lnbmF0dXJlIjoic3RyaW5nIGdyb3VwLGtleXdvcmQga2V5d29yZCxlbnVtIGRlZmF1bHRGb2xkaW5nU3RhdGUsZW51bSBkZWZhdWx0VG9nZ2xlRGlzcGxheWVkIiwiX25hbWVkUGFyYW1ldGVycyI6W3sibmFtZSI6Imdyb3VwIiwidmFsdWUiOiJWZWN0b3IifSx7Im5hbWUiOiJrZXl3b3JkIiwidmFsdWUiOiIifSx7Im5hbWUiOiJkZWZhdWx0Rm9sZGluZ1N0YXRlIiwidmFsdWUiOiJvbiJ9LHsibmFtZSI6ImRlZmF1bHRUb2dnbGVEaXNwbGF5ZWQiLCJ2YWx1ZSI6Im9uIn1dfSwiZGVjb3JhdG9ycyI6W119","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":46,"pos":[-800,2944],"params":["Inherit","False","Property","_Hidden","Hidden","34","0","Create","True","0","0","0","True","0","False","Object","-1","LWGUI:440:eyJkcmF3ZXIiOnsiX2RyYXdlclR5cGVOYW1lIjoiU3ViRHJhd2VyIiwiX3BhcmFtZXRlcnMiOlsiQXBwZWFyYW5jZSJdLCJfaXNEZWNvcmF0b3IiOmZhbHNlLCJfY29uc3RydWN0b3JTaWduYXR1cmUiOiJzdHJpbmcgZ3JvdXAiLCJfbmFtZWRQYXJhbWV0ZXJzIjpbeyJuYW1lIjoiZ3JvdXAiLCJ2YWx1ZSI6IkFwcGVhcmFuY2UifV19LCJkZWNvcmF0b3JzIjpbeyJfZHJhd2VyVHlwZU5hbWUiOiJIaWRkZW5EZWNvcmF0b3IiLCJfcGFyYW1ldGVycyI6W10sIl9pc0RlY29yYXRvciI6dHJ1ZSwiX2NvbnN0cnVjdG9yU2lnbmF0dXJlIjoiIiwiX25hbWVkUGFyYW1ldGVycyI6W119XX0=","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":47,"pos":[-816,3648],"params":["Inherit","False","Property","_PassSwitch","Pass Switch","40","0","Create","True","0","0","0","True","0","False","Object","-1","LWGUI:584:eyJkcmF3ZXIiOnsiX2RyYXdlclR5cGVOYW1lIjoiU3ViVG9nZ2xlRHJhd2VyIiwiX3BhcmFtZXRlcnMiOlsiQ29uZGl0aW9uIl0sIl9pc0RlY29yYXRvciI6ZmFsc2UsIl9jb25zdHJ1Y3RvclNpZ25hdHVyZSI6InN0cmluZyBncm91cCIsIl9uYW1lZFBhcmFtZXRlcnMiOlt7Im5hbWUiOiJncm91cCIsInZhbHVlIjoiQ29uZGl0aW9uIn1dfSwiZGVjb3JhdG9ycyI6W3siX2RyYXdlclR5cGVOYW1lIjoiUGFzc1N3aXRjaERlY29yYXRvciIsIl9wYXJhbWV0ZXJzIjpbIlVuaXZlcnNhbEZvcndhcmRPbmx5Il0sIl9pc0RlY29yYXRvciI6dHJ1ZSwiX2NvbnN0cnVjdG9yU2lnbmF0dXJlIjoicGFzc25hbWUgbGlnaHRNb2RlTmFtZTEiLCJfbmFtZWRQYXJhbWV0ZXJzIjpbeyJuYW1lIjoibGlnaHRNb2RlTmFtZTEiLCJ2YWx1ZSI6IlVuaXZlcnNhbEZvcndhcmRPbmx5In1dfV19","1","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":44,"pos":[-816,3440],"params":["Inherit","False","Property","_ShowIfTrue1","Show If True (1)","38","0","Create","True","0","0","0","True","0","False","Object","-1","LWGUI:684:eyJkcmF3ZXIiOnsiX2RyYXdlclR5cGVOYW1lIjoiU3ViRHJhd2VyIiwiX3BhcmFtZXRlcnMiOlsiQ29uZGl0aW9uIl0sIl9pc0RlY29yYXRvciI6ZmFsc2UsIl9jb25zdHJ1Y3RvclNpZ25hdHVyZSI6InN0cmluZyBncm91cCIsIl9uYW1lZFBhcmFtZXRlcnMiOlt7Im5hbWUiOiJncm91cCIsInZhbHVlIjoiQ29uZGl0aW9uIn1dfSwiZGVjb3JhdG9ycyI6W3siX2RyYXdlclR5cGVOYW1lIjoiU2hvd0lmRGVjb3JhdG9yIiwiX3BhcmFtZXRlcnMiOlsiX1RvZ2dsZSIsIiIsIjEiXSwiX2lzRGVjb3JhdG9yIjp0cnVlLCJfY29uc3RydWN0b3JTaWduYXR1cmUiOiJzdHJpbmcgcHJvcE5hbWVPcktleXdvcmQsZW51bSBjb21wYXJlRnVuY3Rpb24sZmxvYXQgdmFsdWUiLCJfbmFtZWRQYXJhbWV0ZXJzIjpbeyJuYW1lIjoicHJvcE5hbWVPcktleXdvcmQiLCJ2YWx1ZSI6Il9Ub2dnbGUifSx7Im5hbWUiOiJjb21wYXJlRnVuY3Rpb24iLCJ2YWx1ZSI6IiJ9LHsibmFtZSI6InZhbHVlIiwidmFsdWUiOiIxIn1dfV19","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":40,"pos":[-816,3152],"params":["Inherit","False","Property","_ConditionDecoratorsGroup","====== Condition Decorators Group ======","35","0","Create","True","0","0","0","True","0","False","Object","-1","LWGUI:440:eyJkcmF3ZXIiOnsiX2RyYXdlclR5cGVOYW1lIjoiTWFpbkRyYXdlciIsIl9wYXJhbWV0ZXJzIjpbIkNvbmRpdGlvbiIsIiIsIm9uIl0sIl9pc0RlY29yYXRvciI6ZmFsc2UsIl9jb25zdHJ1Y3RvclNpZ25hdHVyZSI6InN0cmluZyBncm91cCxrZXl3b3JkIGtleXdvcmQsZW51bSBkZWZhdWx0Rm9sZGluZ1N0YXRlIiwiX25hbWVkUGFyYW1ldGVycyI6W3sibmFtZSI6Imdyb3VwIiwidmFsdWUiOiJDb25kaXRpb24ifSx7Im5hbWUiOiJrZXl3b3JkIiwidmFsdWUiOiIifSx7Im5hbWUiOiJkZWZhdWx0Rm9sZGluZ1N0YXRlIiwidmFsdWUiOiJvbiJ9XX0sImRlY29yYXRvcnMiOltdfQ==","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":37,"pos":[-800,2848],"params":["Inherit","False","Property","_ReadOnly","Read Only","33","0","Create","True","0","0","0","True","0","False","Object","-1","LWGUI:444:eyJkcmF3ZXIiOnsiX2RyYXdlclR5cGVOYW1lIjoiU3ViRHJhd2VyIiwiX3BhcmFtZXRlcnMiOlsiQXBwZWFyYW5jZSJdLCJfaXNEZWNvcmF0b3IiOmZhbHNlLCJfY29uc3RydWN0b3JTaWduYXR1cmUiOiJzdHJpbmcgZ3JvdXAiLCJfbmFtZWRQYXJhbWV0ZXJzIjpbeyJuYW1lIjoiZ3JvdXAiLCJ2YWx1ZSI6IkFwcGVhcmFuY2UifV19LCJkZWNvcmF0b3JzIjpbeyJfZHJhd2VyVHlwZU5hbWUiOiJSZWFkT25seURlY29yYXRvciIsIl9wYXJhbWV0ZXJzIjpbXSwiX2lzRGVjb3JhdG9yIjp0cnVlLCJfY29uc3RydWN0b3JTaWduYXR1cmUiOiIiLCJfbmFtZWRQYXJhbWV0ZXJzIjpbXX1dfQ==","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":34,"pos":[-800,2736],"params":["Inherit","False","Property","_TooltipSample","Multilingual Tooltip#Hello, world#你好, 世界!#안녕, 세계!#","32","0","Create","False","0","0","0","True","0","False","Object","-1","LWGUI:660:eyJkcmF3ZXIiOnsiX2RyYXdlclR5cGVOYW1lIjoiU3ViRHJhd2VyIiwiX3BhcmFtZXRlcnMiOlsiQXBwZWFyYW5jZSJdLCJfaXNEZWNvcmF0b3IiOmZhbHNlLCJfY29uc3RydWN0b3JTaWduYXR1cmUiOiJzdHJpbmcgZ3JvdXAiLCJfbmFtZWRQYXJhbWV0ZXJzIjpbeyJuYW1lIjoiZ3JvdXAiLCJ2YWx1ZSI6IkFwcGVhcmFuY2UifV19LCJkZWNvcmF0b3JzIjpbeyJfZHJhd2VyVHlwZU5hbWUiOiJUb29sdGlwRGVjb3JhdG9yIiwiX3BhcmFtZXRlcnMiOlsiVG9vbHRpcCAxIiwiVG9vbHRpcCAyIiwiVG9vbHRpcCAzIl0sIl9pc0RlY29yYXRvciI6dHJ1ZSwiX2NvbnN0cnVjdG9yU2lnbmF0dXJlIjoic3RyaW5nIHMxLHN0cmluZyBzMixzdHJpbmcgczMiLCJfbmFtZWRQYXJhbWV0ZXJzIjpbeyJuYW1lIjoiczEiLCJ2YWx1ZSI6IlRvb2x0aXAgMSJ9LHsibmFtZSI6InMyIiwidmFsdWUiOiJUb29sdGlwIDIifSx7Im5hbWUiOiJzMyIsInZhbHVlIjoiVG9vbHRpcCAzIn1dfV19","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":33,"pos":[-800,2464],"params":["Inherit","False","Property","_AppearanceDecoratorsGroup","====== Appearance Decorators Group ======","29","0","Create","True","0","0","0","True","0","False","Object","-1","LWGUI:824:eyJkcmF3ZXIiOnsiX2RyYXdlclR5cGVOYW1lIjoiTWFpbkRyYXdlciIsIl9wYXJhbWV0ZXJzIjpbIkFwcGVhcmFuY2UiLCIiLCJvbiJdLCJfaXNEZWNvcmF0b3IiOmZhbHNlLCJfY29uc3RydWN0b3JTaWduYXR1cmUiOiJzdHJpbmcgZ3JvdXAsa2V5d29yZCBrZXl3b3JkLGVudW0gZGVmYXVsdEZvbGRpbmdTdGF0ZSIsIl9uYW1lZFBhcmFtZXRlcnMiOlt7Im5hbWUiOiJncm91cCIsInZhbHVlIjoiQXBwZWFyYW5jZSJ9LHsibmFtZSI6ImtleXdvcmQiLCJ2YWx1ZSI6IiJ9LHsibmFtZSI6ImRlZmF1bHRGb2xkaW5nU3RhdGUiLCJ2YWx1ZSI6Im9uIn1dfSwiZGVjb3JhdG9ycyI6W3siX2RyYXdlclR5cGVOYW1lIjoiSGVscFVSTERlY29yYXRvciIsIl9wYXJhbWV0ZXJzIjpbImdpdGh1Yi5jb20iLCJKYXNvbk1hMDAxMiIsIkxXR1VJIl0sIl9pc0RlY29yYXRvciI6dHJ1ZSwiX2NvbnN0cnVjdG9yU2lnbmF0dXJlIjoic3RyaW5nIHMxLHN0cmluZyBzMixzdHJpbmcgczMiLCJfbmFtZWRQYXJhbWV0ZXJzIjpbeyJuYW1lIjoiczEiLCJ2YWx1ZSI6ImdpdGh1Yi5jb20ifSx7Im5hbWUiOiJzMiIsInZhbHVlIjoiSmFzb25NYTAwMTIifSx7Im5hbWUiOiJzMyIsInZhbHVlIjoiTFdHVUkifV19XX0=","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":36,"pos":[-800,2576],"params":["Inherit","False","Property","_Title","Title","30","0","Create","True","0","0","0","True","0","False","Object","-1","LWGUI:596:eyJkcmF3ZXIiOnsiX2RyYXdlclR5cGVOYW1lIjoiU3ViRHJhd2VyIiwiX3BhcmFtZXRlcnMiOlsiQXBwZWFyYW5jZSJdLCJfaXNEZWNvcmF0b3IiOmZhbHNlLCJfY29uc3RydWN0b3JTaWduYXR1cmUiOiJzdHJpbmcgZ3JvdXAiLCJfbmFtZWRQYXJhbWV0ZXJzIjpbeyJuYW1lIjoiZ3JvdXAiLCJ2YWx1ZSI6IkFwcGVhcmFuY2UifV19LCJkZWNvcmF0b3JzIjpbeyJfZHJhd2VyVHlwZU5hbWUiOiJUaXRsZURlY29yYXRvciIsIl9wYXJhbWV0ZXJzIjpbIkFwcGVhcmFuY2UiLCJUaXRsZSJdLCJfaXNEZWNvcmF0b3IiOnRydWUsIl9jb25zdHJ1Y3RvclNpZ25hdHVyZSI6InN0cmluZyBncm91cCxzdHJpbmcgaGVhZGVyIiwiX25hbWVkUGFyYW1ldGVycyI6W3sibmFtZSI6Imdyb3VwIiwidmFsdWUiOiJBcHBlYXJhbmNlIn0seyJuYW1lIjoiaGVhZGVyIiwidmFsdWUiOiJUaXRsZSJ9XX1dfQ==","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor","id":49,"pos":[-816,4480],"params":["Inherit","True","Property","_TextureSample0","Texture Sample 0","45","0","Create","True","0","0","0","True","0","False","","-1","None","None","True","0","False","white","Auto","False","Object","-1","Auto","Texture2D","False","8","0","SAMPLER2D","","False","1","FLOAT2","0,0","False","2","FLOAT","0","False","3","FLOAT2","0,0","False","4","FLOAT2","0,0","False","5","FLOAT","1","False","6","FLOAT","0","False","7","SAMPLERSTATE","","False","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.ColorNode, AmplifyShaderEditor","id":52,"pos":[-816,4288],"params":["Inherit","False","Property","_Color0","Color 0","44","0","Create","True","0","0","0","True","0","False","Object","-1","","0,0,0,0","0,0,0,0","True","True","0","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor","id":54,"pos":[-800,1552],"params":["Inherit","True","Property","_TexwithChannel","Tex with Channel","26","0","Create","True","0","0","0","True","0","False","LWGUI:368:eyJkcmF3ZXIiOnsiX2RyYXdlclR5cGVOYW1lIjoiVGV4RHJhd2VyIiwiX3BhcmFtZXRlcnMiOlsiVmVjdG9yIiwiX0NoYW5uZWwiXSwiX2lzRGVjb3JhdG9yIjpmYWxzZSwiX2NvbnN0cnVjdG9yU2lnbmF0dXJlIjoic3RyaW5nIGdyb3VwLHN0cmluZyBleHRyYVByb3BOYW1lIiwiX25hbWVkUGFyYW1ldGVycyI6W3sibmFtZSI6Imdyb3VwIiwidmFsdWUiOiJWZWN0b3IifSx7Im5hbWUiOiJleHRyYVByb3BOYW1lIiwidmFsdWUiOiJfQ2hhbm5lbCJ9XX0sImRlY29yYXRvcnMiOltdfQ==","-1","None","None","True","0","False","white","Auto","False","Object","-1","Auto","Texture2D","False","8","0","SAMPLER2D","","False","1","FLOAT2","0,0","False","2","FLOAT","0","False","3","FLOAT2","0,0","False","4","FLOAT2","0,0","False","5","FLOAT","1","False","6","FLOAT","0","False","7","SAMPLERSTATE","","False","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor","id":55,"pos":[-800,1776],"params":["Inherit","True","Property","_TexwithRange","Tex with Range","27","0","Create","True","0","0","0","True","0","False","LWGUI:360:eyJkcmF3ZXIiOnsiX2RyYXdlclR5cGVOYW1lIjoiVGV4RHJhd2VyIiwiX3BhcmFtZXRlcnMiOlsiVmVjdG9yIiwiX1JhbmdlIl0sIl9pc0RlY29yYXRvciI6ZmFsc2UsIl9jb25zdHJ1Y3RvclNpZ25hdHVyZSI6InN0cmluZyBncm91cCxzdHJpbmcgZXh0cmFQcm9wTmFtZSIsIl9uYW1lZFBhcmFtZXRlcnMiOlt7Im5hbWUiOiJncm91cCIsInZhbHVlIjoiVmVjdG9yIn0seyJuYW1lIjoiZXh0cmFQcm9wTmFtZSIsInZhbHVlIjoiX1JhbmdlIn1dfSwiZGVjb3JhdG9ycyI6W119","-1","None","None","True","0","False","white","Auto","False","Object","-1","Auto","Texture2D","False","8","0","SAMPLER2D","","False","1","FLOAT2","0,0","False","2","FLOAT","0","False","3","FLOAT2","0,0","False","4","FLOAT2","0,0","False","5","FLOAT","1","False","6","FLOAT","0","False","7","SAMPLERSTATE","","False","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor","id":56,"pos":[-800,2000],"params":["Inherit","True","Property","_TexwithFloat","Tex with Float","28","0","Create","True","0","0","0","True","0","False","LWGUI:360:eyJkcmF3ZXIiOnsiX2RyYXdlclR5cGVOYW1lIjoiVGV4RHJhd2VyIiwiX3BhcmFtZXRlcnMiOlsiVmVjdG9yIiwiX0Zsb2F0Il0sIl9pc0RlY29yYXRvciI6ZmFsc2UsIl9jb25zdHJ1Y3RvclNpZ25hdHVyZSI6InN0cmluZyBncm91cCxzdHJpbmcgZXh0cmFQcm9wTmFtZSIsIl9uYW1lZFBhcmFtZXRlcnMiOlt7Im5hbWUiOiJncm91cCIsInZhbHVlIjoiVmVjdG9yIn0seyJuYW1lIjoiZXh0cmFQcm9wTmFtZSIsInZhbHVlIjoiX0Zsb2F0In1dfSwiZGVjb3JhdG9ycyI6W119","-1","None","None","True","0","False","white","Auto","False","Object","-1","Auto","Texture2D","False","8","0","SAMPLER2D","","False","1","FLOAT2","0,0","False","2","FLOAT","0","False","3","FLOAT2","0,0","False","4","FLOAT2","0,0","False","5","FLOAT","1","False","6","FLOAT","0","False","7","SAMPLERSTATE","","False","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":51,"pos":[-816,4032],"params":["Inherit","False","Property","_Range1","Range 1","42","0","Create","True","0","0","0","True","0","False","Object","-1","","0.5","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.Vector4Node, AmplifyShaderEditor","id":50,"pos":[-816,4112],"params":["Inherit","False","Property","_Vector0","Vector 0","43","0","Create","True","0","0","0","True","0","False","Object","-1","","0,0,0,0","0,0,0,0","0","5","FLOAT4","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":48,"pos":[-816,3952],"params":["Inherit","False","Property","_Float0","Float 0","41","0","Create","True","0","0","0","True","0","False","Object","-1","LWGUI:436:eyJkcmF3ZXIiOnsiX2RyYXdlclR5cGVOYW1lIjoiIiwiX3BhcmFtZXRlcnMiOltdLCJfaXNEZWNvcmF0b3IiOmZhbHNlLCJfY29uc3RydWN0b3JTaWduYXR1cmUiOiIiLCJfbmFtZWRQYXJhbWV0ZXJzIjpbXX0sImRlY29yYXRvcnMiOlt7Il9kcmF3ZXJUeXBlTmFtZSI6IlRpdGxlRGVjb3JhdG9yIiwiX3BhcmFtZXRlcnMiOlsiT3V0IG9mIEdyb3VwIl0sIl9pc0RlY29yYXRvciI6dHJ1ZSwiX2NvbnN0cnVjdG9yU2lnbmF0dXJlIjoic3RyaW5nIGhlYWRlciIsIl9uYW1lZFBhcmFtZXRlcnMiOlt7Im5hbWUiOiJoZWFkZXIiLCJ2YWx1ZSI6Ik91dCBvZiBHcm91cCJ9XX1dfQ==","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":16,"pos":[-784,-592],"params":["Inherit","False","Property","_Float","Float","1","0","Create","False","0","0","0","True","0","False","Object","-1","LWGUI:268:eyJkcmF3ZXIiOnsiX2RyYXdlclR5cGVOYW1lIjoiU3ViRHJhd2VyIiwiX3BhcmFtZXRlcnMiOlsiTnVtZXJpYyJdLCJfaXNEZWNvcmF0b3IiOmZhbHNlLCJfY29uc3RydWN0b3JTaWduYXR1cmUiOiJzdHJpbmcgZ3JvdXAiLCJfbmFtZWRQYXJhbWV0ZXJzIjpbeyJuYW1lIjoiZ3JvdXAiLCJ2YWx1ZSI6Ik51bWVyaWMifV19LCJkZWNvcmF0b3JzIjpbXX0=","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":57,"pos":[-784,-112],"params":["Inherit","False","Property","_BitMask","Bit Mask","8","0","Create","True","0","0","0","True","0","False","Object","-1","LWGUI:272:eyJkcmF3ZXIiOnsiX2RyYXdlclR5cGVOYW1lIjoiQml0TWFza0RyYXdlciIsIl9wYXJhbWV0ZXJzIjpbIk51bWVyaWMiXSwiX2lzRGVjb3JhdG9yIjpmYWxzZSwiX2NvbnN0cnVjdG9yU2lnbmF0dXJlIjoic3RyaW5nIGdyb3VwIiwiX25hbWVkUGFyYW1ldGVycyI6W3sibmFtZSI6Imdyb3VwIiwidmFsdWUiOiJOdW1lcmljIn1dfSwiZGVjb3JhdG9ycyI6W119","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":17,"pos":[-784,-512],"params":["Inherit","False","Property","_Range","Range","2","0","Create","False","0","0","0","True","0","False","Object","-1","LWGUI:268:eyJkcmF3ZXIiOnsiX2RyYXdlclR5cGVOYW1lIjoiU3ViRHJhd2VyIiwiX3BhcmFtZXRlcnMiOlsiTnVtZXJpYyJdLCJfaXNEZWNvcmF0b3IiOmZhbHNlLCJfY29uc3RydWN0b3JTaWduYXR1cmUiOiJzdHJpbmcgZ3JvdXAiLCJfbmFtZWRQYXJhbWV0ZXJzIjpbeyJuYW1lIjoiZ3JvdXAiLCJ2YWx1ZSI6Ik51bWVyaWMifV19LCJkZWNvcmF0b3JzIjpbXX0=","0","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":18,"pos":[-784,-432],"params":["Inherit","False","Property","_SubPowerSlider","Sub Power Slider","3","0","Create","True","0","0","0","True","0","False","Object","-1","LWGUI:344:eyJkcmF3ZXIiOnsiX2RyYXdlclR5cGVOYW1lIjoiU3ViUG93ZXJTbGlkZXJEcmF3ZXIiLCJfcGFyYW1ldGVycyI6WyJOdW1lcmljIiwiNSJdLCJfaXNEZWNvcmF0b3IiOmZhbHNlLCJfY29uc3RydWN0b3JTaWduYXR1cmUiOiJzdHJpbmcgZ3JvdXAsZmxvYXQgcG93ZXIiLCJfbmFtZWRQYXJhbWV0ZXJzIjpbeyJuYW1lIjoiZ3JvdXAiLCJ2YWx1ZSI6Ik51bWVyaWMifSx7Im5hbWUiOiJwb3dlciIsInZhbHVlIjoiNSJ9XX0sImRlY29yYXRvcnMiOltdfQ==","50","0","0","100","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":42,"pos":[-816,3248],"params":["Inherit","False","Property","_Toggle","Toggle","36","0","Create","False","0","0","0","True","0","False","Object","-1","LWGUI:280:eyJkcmF3ZXIiOnsiX2RyYXdlclR5cGVOYW1lIjoiU3ViVG9nZ2xlRHJhd2VyIiwiX3BhcmFtZXRlcnMiOlsiQ29uZGl0aW9uIl0sIl9pc0RlY29yYXRvciI6ZmFsc2UsIl9jb25zdHJ1Y3RvclNpZ25hdHVyZSI6InN0cmluZyBncm91cCIsIl9uYW1lZFBhcmFtZXRlcnMiOlt7Im5hbWUiOiJncm91cCIsInZhbHVlIjoiQ29uZGl0aW9uIn1dfSwiZGVjb3JhdG9ycyI6W119","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":43,"pos":[-816,3344],"params":["Inherit","False","Property","_ShowIfFalse0","Show If False (0)","37","0","Create","True","0","0","0","True","0","False","Object","-1","LWGUI:684:eyJkcmF3ZXIiOnsiX2RyYXdlclR5cGVOYW1lIjoiU3ViRHJhd2VyIiwiX3BhcmFtZXRlcnMiOlsiQ29uZGl0aW9uIl0sIl9pc0RlY29yYXRvciI6ZmFsc2UsIl9jb25zdHJ1Y3RvclNpZ25hdHVyZSI6InN0cmluZyBncm91cCIsIl9uYW1lZFBhcmFtZXRlcnMiOlt7Im5hbWUiOiJncm91cCIsInZhbHVlIjoiQ29uZGl0aW9uIn1dfSwiZGVjb3JhdG9ycyI6W3siX2RyYXdlclR5cGVOYW1lIjoiU2hvd0lmRGVjb3JhdG9yIiwiX3BhcmFtZXRlcnMiOlsiX1RvZ2dsZSIsIiIsIiJdLCJfaXNEZWNvcmF0b3IiOnRydWUsIl9jb25zdHJ1Y3RvclNpZ25hdHVyZSI6InN0cmluZyBwcm9wTmFtZU9yS2V5d29yZCxlbnVtIGNvbXBhcmVGdW5jdGlvbixmbG9hdCB2YWx1ZSIsIl9uYW1lZFBhcmFtZXRlcnMiOlt7Im5hbWUiOiJwcm9wTmFtZU9yS2V5d29yZCIsInZhbHVlIjoiX1RvZ2dsZSJ9LHsibmFtZSI6ImNvbXBhcmVGdW5jdGlvbiIsInZhbHVlIjoiIn0seyJuYW1lIjoidmFsdWUiLCJ2YWx1ZSI6IiJ9XX1dfQ==","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":45,"pos":[-816,3552],"params":["Inherit","False","Property","_ActiveIfTrue1","Active If True (1)","39","0","Create","True","0","0","0","True","0","False","Object","-1","LWGUI:720:eyJkcmF3ZXIiOnsiX2RyYXdlclR5cGVOYW1lIjoiU3ViRHJhd2VyIiwiX3BhcmFtZXRlcnMiOlsiQ29uZGl0aW9uIl0sIl9pc0RlY29yYXRvciI6ZmFsc2UsIl9jb25zdHJ1Y3RvclNpZ25hdHVyZSI6InN0cmluZyBncm91cCIsIl9uYW1lZFBhcmFtZXRlcnMiOlt7Im5hbWUiOiJncm91cCIsInZhbHVlIjoiQ29uZGl0aW9uIn1dfSwiZGVjb3JhdG9ycyI6W3siX2RyYXdlclR5cGVOYW1lIjoiQWN0aXZlSWZEZWNvcmF0b3IiLCJfcGFyYW1ldGVycyI6WyIiLCJfVG9nZ2xlIiwiIiwiMSJdLCJfaXNEZWNvcmF0b3IiOnRydWUsIl9jb25zdHJ1Y3RvclNpZ25hdHVyZSI6ImVudW0gbG9naWNhbE9wZXJhdG9yLHN0cmluZyBwcm9wTmFtZU9yS2V5d29yZCxlbnVtIGNvbXBhcmVGdW5jdGlvbixmbG9hdCB2YWx1ZSIsIl9uYW1lZFBhcmFtZXRlcnMiOlt7Im5hbWUiOiJwcm9wTmFtZU9yS2V5d29yZCIsInZhbHVlIjoiX1RvZ2dsZSJ9LHsibmFtZSI6ImNvbXBhcmlzb25NZXRob2QiLCJ2YWx1ZSI6IiJ9LHsibmFtZSI6InZhbHVlIiwidmFsdWUiOiIxIn1dfV19","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":35,"pos":[-800,2656],"params":["Inherit","False","Property","_HelpboxSample","Multilingual Helpbox%Hello, world!%你好, 世界!%안녕, 세계!%%","31","0","Create","False","0","0","0","True","0","False","Object","-1","LWGUI:660:eyJkcmF3ZXIiOnsiX2RyYXdlclR5cGVOYW1lIjoiU3ViRHJhd2VyIiwiX3BhcmFtZXRlcnMiOlsiQXBwZWFyYW5jZSJdLCJfaXNEZWNvcmF0b3IiOmZhbHNlLCJfY29uc3RydWN0b3JTaWduYXR1cmUiOiJzdHJpbmcgZ3JvdXAiLCJfbmFtZWRQYXJhbWV0ZXJzIjpbeyJuYW1lIjoiZ3JvdXAiLCJ2YWx1ZSI6IkFwcGVhcmFuY2UifV19LCJkZWNvcmF0b3JzIjpbeyJfZHJhd2VyVHlwZU5hbWUiOiJIZWxwYm94RGVjb3JhdG9yIiwiX3BhcmFtZXRlcnMiOlsiSGVscGJveCAxIiwiSGVscGJveCAyIiwiSGVscGJveCAzIl0sIl9pc0RlY29yYXRvciI6dHJ1ZSwiX2NvbnN0cnVjdG9yU2lnbmF0dXJlIjoic3RyaW5nIHMxLHN0cmluZyBzMixzdHJpbmcgczMiLCJfbmFtZWRQYXJhbWV0ZXJzIjpbeyJuYW1lIjoiczEiLCJ2YWx1ZSI6IkhlbHBib3ggMSJ9LHsibmFtZSI6InMyIiwidmFsdWUiOiJIZWxwYm94IDIifSx7Im5hbWUiOiJzMyIsInZhbHVlIjoiSGVscGJveCAzIn1dfV19","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":23,"pos":[-816,-672],"params":["Inherit","False","Property","_NumericGroupToenableASEintegrationyouneedtoInstallASEv19910EnableProjectSettingsLWGUIASEIntegration","====== Numeric Group ======%To enable ASE integration, you need to:%- Install ASE v1.9.9.10+%- Enable: Project Settings > LWGUI > ASE Integration","0","0","Create","True","0","0","0","True","0","False","Object","-1","LWGUI:544:eyJkcmF3ZXIiOnsiX2RyYXdlclR5cGVOYW1lIjoiTWFpbkRyYXdlciIsIl9wYXJhbWV0ZXJzIjpbIk51bWVyaWMiLCIiLCJvbiIsIm9mZiJdLCJfaXNEZWNvcmF0b3IiOmZhbHNlLCJfY29uc3RydWN0b3JTaWduYXR1cmUiOiJzdHJpbmcgZ3JvdXAsa2V5d29yZCBrZXl3b3JkLGVudW0gZGVmYXVsdEZvbGRpbmdTdGF0ZSxlbnVtIGRlZmF1bHRUb2dnbGVEaXNwbGF5ZWQiLCJfbmFtZWRQYXJhbWV0ZXJzIjpbeyJuYW1lIjoiZ3JvdXAiLCJ2YWx1ZSI6Ik51bWVyaWMifSx7Im5hbWUiOiJrZXl3b3JkIiwidmFsdWUiOiIifSx7Im5hbWUiOiJkZWZhdWx0Rm9sZGluZ1N0YXRlIiwidmFsdWUiOiJvbiJ9LHsibmFtZSI6ImRlZmF1bHRUb2dnbGVEaXNwbGF5ZWQiLCJ2YWx1ZSI6Im9mZiJ9XX0sImRlY29yYXRvcnMiOltdfQ==","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":0,"pos":[0,16],"params":["Float","False","False","-1","3","LWGUI.LWGUI","0","19","New Amplify Shader","2992e84f91cbeb14eab234972e07ea9d","True","ExtraPrePass","0","0","ExtraPrePass","6","False","True","1","1","False","","0","False","","1","1","False","","0","False","","True","1","False","","1","False","","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","False","False","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Unlit","True","5","True","12","all","0","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","True","3","False","","True","True","0","False","","0","False","","False","True","0","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":2,"pos":[0,0],"params":["Float","False","False","-1","3","LWGUI.LWGUI","0","19","New Amplify Shader","2992e84f91cbeb14eab234972e07ea9d","True","ShadowCaster","0","2","ShadowCaster","0","False","True","1","1","False","","0","False","","1","1","False","","0","False","","True","1","False","","1","False","","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","False","False","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Unlit","True","5","True","12","all","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","False","False","True","False","False","False","False","0","False","","False","False","False","False","False","False","False","False","False","True","1","False","","True","3","False","","False","False","True","1","LightMode=ShadowCaster","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":3,"pos":[0,0],"params":["Float","False","False","-1","3","LWGUI.LWGUI","0","19","New Amplify Shader","2992e84f91cbeb14eab234972e07ea9d","True","DepthOnly","0","3","DepthOnly","0","False","True","1","1","False","","0","False","","1","1","False","","0","False","","True","1","False","","1","False","","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","False","False","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Unlit","True","5","True","12","all","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","False","False","True","True","False","False","False","0","False","","False","False","False","False","False","False","False","False","False","True","1","False","","False","False","False","True","1","LightMode=DepthOnly","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":4,"pos":[0,0],"params":["Float","False","False","-1","3","LWGUI.LWGUI","0","19","New Amplify Shader","2992e84f91cbeb14eab234972e07ea9d","True","Meta","0","4","Meta","0","False","True","1","1","False","","0","False","","1","1","False","","0","False","","True","1","False","","1","False","","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","False","False","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Unlit","True","5","True","12","all","0","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","2","False","","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","1","LightMode=Meta","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":5,"pos":[0,0],"params":["Float","False","False","-1","3","LWGUI.LWGUI","0","19","New Amplify Shader","2992e84f91cbeb14eab234972e07ea9d","True","Universal2D","0","5","Universal2D","0","False","True","1","1","False","","0","False","","1","1","False","","0","False","","True","1","False","","1","False","","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","False","False","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Unlit","True","5","True","12","all","0","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","True","3","False","","True","True","0","False","","0","False","","False","True","1","LightMode=Universal2D","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":6,"pos":[0,0],"params":["Float","False","False","-1","3","LWGUI.LWGUI","0","19","New Amplify Shader","2992e84f91cbeb14eab234972e07ea9d","True","SceneSelectionPass","0","6","SceneSelectionPass","0","False","True","1","1","False","","0","False","","1","1","False","","0","False","","True","1","False","","1","False","","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","False","False","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Unlit","True","5","True","12","all","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","2","False","","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","1","LightMode=SceneSelectionPass","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":7,"pos":[0,0],"params":["Float","False","False","-1","3","LWGUI.LWGUI","0","19","New Amplify Shader","2992e84f91cbeb14eab234972e07ea9d","True","ScenePickingPass","0","7","ScenePickingPass","0","False","True","1","1","False","","0","False","","1","1","False","","0","False","","True","1","False","","1","False","","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","False","False","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Unlit","True","5","True","12","all","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","1","LightMode=Picking","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":8,"pos":[0,0],"params":["Float","False","False","-1","3","LWGUI.LWGUI","0","19","New Amplify Shader","2992e84f91cbeb14eab234972e07ea9d","True","DepthNormals","0","8","DepthNormals","0","False","True","1","1","False","","0","False","","1","1","False","","0","False","","True","1","False","","1","False","","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","False","False","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Unlit","True","5","True","12","all","0","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","1","False","","True","3","False","","False","False","True","1","LightMode=DepthNormalsOnly","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":9,"pos":[0,0],"params":["Float","False","False","-1","3","LWGUI.LWGUI","0","19","New Amplify Shader","2992e84f91cbeb14eab234972e07ea9d","True","DepthNormalsOnly","0","9","DepthNormalsOnly","0","False","True","1","1","False","","0","False","","1","1","False","","0","False","","True","1","False","","1","False","","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","False","False","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Unlit","True","5","True","12","all","0","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","1","False","","True","3","False","","False","False","True","1","LightMode=DepthNormalsOnly","False","True","9","d3d11","metal","vulkan","xboxone","xboxseries","playstation","ps4","ps5","switch","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":1,"pos":[16,16],"params":["Float","False","True","-1","3","LWGUI.LWGUI","0","19","New Amplify Shader","2992e84f91cbeb14eab234972e07ea9d","True","Forward","0","1","Forward","11","False","True","1","1","False","","0","False","","1","1","False","","0","False","","True","1","False","","1","False","","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","False","False","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Unlit","True","5","True","12","all","0","False","True","1","1","False","","0","False","","1","1","False","","0","False","","True","1","False","","1","False","","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","True","3","False","","True","True","0","False","","0","False","","False","True","1","LightMode=UniversalForwardOnly","False","False","0","","0","0","Standard","28","Surface","0","0","  Keep Alpha","0","0","  Blend","0","0","Two Sided","1","0","Alpha Clipping","0","0","  Use Shadow Threshold","0","0","Fragment Normal Space","0","0","Forward Only","0","0","Cast Shadows","1","0","Receive Shadows","2","0","Receive SSAO","1","0","GPU Instancing","1","0","LOD CrossFade","1","0","Built-in Fog","1","0","Meta Pass","0","0","Extra Pre Pass","0","0","Tessellation","0","0","  Phong","0","0","  Strength","0.5,False,","0","  Type","0","0","  Tess","16,False,","0","  Min","10,False,","0","  Max","25,False,","0","  Edge Length","16,False,","0","  Max Displacement","25,False,","0","Write Depth","0","0","  Conservative","0","0","Vertex Position","1","0","0","10","False","True","True","True","False","False","True","True","True","False","False","","False","0"]}
{"wire":[1,2,24,0]}
ASEEND*/
//CHKSM=9D96F055B4D07596512CFBB9CB7E5447D2E1CF59