﻿///////////////////////
////   GLOBALS
///////////////////////
cbuffer PerFrameBuffer
{
	matrix worldMatrix;
	matrix viewMatrix;
	matrix projectionMatrix;
};

cbuffer FogBuffer
{
	float fogStart;
	float fogEnd;
}

//////////////////////
////   TYPES
//////////////////////
struct VertexInputType
{
	float4 position : POSITION;
	float2 tex : TEXCOORD0;
};

struct PixelInputType
{
	float4 position : SV_POSITION;
	float2 tex : TEXCOORD0;
	float fogFactor : FOG;
};

/////////////////////////////////////
/////   Vertex Shader
/////////////////////////////////////
PixelInputType FogVertexShader(VertexInputType input)
{
	PixelInputType output;
	float4 cameraPosition;

	// Change the position vector to be 4 units for proper matrix calculations.
	input.position.w = 1.0f;

	// Calculate the position of the vertex against the world, view, and projection matrices.
	output.position = mul(input.position, worldMatrix);
	output.position = mul(output.position, viewMatrix);
	output.position = mul(output.position, projectionMatrix);

	// Store the texture coordinates for the pixel shader to use.
	output.tex = input.tex;

	// Calculate the camera position
	cameraPosition = mul(input.position, worldMatrix);
	cameraPosition = mul(cameraPosition, viewMatrix);

	// Calculate linear fog.
	output.fogFactor = saturate((fogEnd - cameraPosition.z) / (fogEnd - fogStart));
	
	// Exponential Fog = 1.0 / 2.71828 power (ViewpointDistance * FogDensity)

	return output;
}