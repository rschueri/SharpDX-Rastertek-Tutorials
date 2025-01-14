﻿using DSharpDXRastertek.TutTerr16.System;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DSharpDXRastertek.TutTerr16.Graphics.Shaders
{
    public class DTerrainShader                 // 302 lines
    {
        // Structures
        [StructLayout(LayoutKind.Sequential)]
        internal struct DMatrixBuffer
        {
            public Matrix world;
            public Matrix view;
            public Matrix projection;
        }
        [StructLayout(LayoutKind.Sequential)]
        internal struct DLightBuffer
        {
            public Vector4 lightDiffuseColor;
            public Vector3 lightDirection;
            public float colorTextureBrightness; 
        }

        // Properties
        public VertexShader VertexShader { get; set; }
        public PixelShader PixelShader { get; set; }
        public InputLayout Layout { get; set; }
        public SharpDX.Direct3D11.Buffer ConstantMatrixBuffer { get; set; }
        public SharpDX.Direct3D11.Buffer ConstantLightBuffer { get; set; }
        public SamplerState SampleState { get; set; }

        // Methods
        public bool Initialize(Device device, IntPtr windowHandler)
        {
            // Initialize the vertex and pixel shaders.
            return InitializeShader(device, windowHandler, "terrain.vs", "terrain.ps");
        }
        private bool InitializeShader(Device device, IntPtr windowHandler, string vsFileName, string psFileName)
        {
            try
            {
                // Setup full pathes
                vsFileName = DSystemConfiguration.ShaderFilePath + vsFileName;
                psFileName = DSystemConfiguration.ShaderFilePath + psFileName;

                // Compile the Vertex & Pixel Shader code.
                ShaderBytecode vertexShaderByteCode = ShaderBytecode.CompileFromFile(vsFileName, "TerrainVertexShader", DSystemConfiguration.VertexShaderProfile, ShaderFlags.None, EffectFlags.None);
                ShaderBytecode pixelShaderByteCode = ShaderBytecode.CompileFromFile(psFileName, "TerrainPixelShader", DSystemConfiguration.PixelShaderProfile, ShaderFlags.None, EffectFlags.None);

                // Create the Vertex & Pixel Shader from the buffer.
                VertexShader = new VertexShader(device, vertexShaderByteCode);
                PixelShader = new PixelShader(device, pixelShaderByteCode);

                // Now setup the layout of the data that goes into the shader.
                // This setup needs to match the VertexType structure in the Model and in the shader.
                InputElement[] inputElements = new InputElement[]
                {
                    new InputElement()
                    {
                        SemanticName = "POSITION",
                        SemanticIndex = 0,
                        Format = SharpDX.DXGI.Format.R32G32B32_Float,
                        Slot = 0,
                        AlignedByteOffset = 0,
                        Classification = InputClassification.PerVertexData,
                        InstanceDataStepRate = 0
                    },
                    new InputElement()
                    {
                        SemanticName = "TEXCOORD",
                        SemanticIndex = 0,
                        Format = SharpDX.DXGI.Format.R32G32_Float,
                        Slot = 0,
                        AlignedByteOffset = InputElement.AppendAligned,
                        Classification = InputClassification.PerVertexData,
                        InstanceDataStepRate = 0
                    },
                    new InputElement()
                    {
                        SemanticName = "NORMAL",
                        SemanticIndex = 0,
                        Format = SharpDX.DXGI.Format.R32G32B32_Float,
                        Slot = 0,
                        AlignedByteOffset = InputElement.AppendAligned,
                        Classification = InputClassification.PerVertexData,
                        InstanceDataStepRate = 0
                    },
                    new InputElement()
                    {
                        SemanticName = "TANGENT",
                        SemanticIndex = 0,
                        Format = SharpDX.DXGI.Format.R32G32B32_Float,
                        Slot = 0,
                        AlignedByteOffset = InputElement.AppendAligned,
                        Classification = InputClassification.PerVertexData,
                        InstanceDataStepRate = 0
                    },
                     new InputElement()
                    {
                        SemanticName = "BINORMAL",
                        SemanticIndex = 0,
                        Format = SharpDX.DXGI.Format.R32G32B32_Float,
                        Slot = 0,
                        AlignedByteOffset = InputElement.AppendAligned,
                        Classification = InputClassification.PerVertexData,
                        InstanceDataStepRate = 0
                    },
                    new InputElement()
                    {
                        SemanticName = "COLOR",
                        SemanticIndex = 0,
                        Format = SharpDX.DXGI.Format.R32G32B32A32_Float,
                        Slot = 0,
                        AlignedByteOffset = InputElement.AppendAligned,
                        Classification = InputClassification.PerVertexData,
                        InstanceDataStepRate = 0
                    }
                };

                // Create the vertex input the layout.
                Layout = new InputLayout(device, ShaderSignature.GetInputSignature(vertexShaderByteCode), inputElements);

                // Release the vertex and pixel shader buffers, since they are no longer needed.
                vertexShaderByteCode.Dispose();
                pixelShaderByteCode.Dispose();

                // Create a texture sampler state description.
                SamplerStateDescription samplerDesc = new SamplerStateDescription()
                {
                    Filter = Filter.MinMagMipLinear,
                    AddressU = TextureAddressMode.Clamp,
                    AddressV = TextureAddressMode.Clamp,
                    AddressW = TextureAddressMode.Clamp,
                    MipLodBias = 0.0f,
                    MaximumAnisotropy = 1,
                    ComparisonFunction = Comparison.Always,
                    BorderColor = new Color4(0, 0, 0, 0),
                    MinimumLod = 0,
                    MaximumLod = float.MaxValue
                };

                // Create the texture sampler state.
                SampleState = new SamplerState(device, samplerDesc);

                // Setup the description of the dynamic matrix constant buffer that is in the vertex shader.
                BufferDescription matrixBufferDesc = new BufferDescription()
                {
                    Usage = ResourceUsage.Dynamic,
                    SizeInBytes = Utilities.SizeOf<DMatrixBuffer>(),
                    BindFlags = BindFlags.ConstantBuffer,
                    CpuAccessFlags = CpuAccessFlags.Write,
                    OptionFlags = ResourceOptionFlags.None,
                    StructureByteStride = 0
                };

                // Create the constant buffer pointer so we can access the vertex shader constant buffer from within this class.
                ConstantMatrixBuffer = new SharpDX.Direct3D11.Buffer(device, matrixBufferDesc);

                // Setup the description of the light dynamic constant bufffer that is in the pixel shader.
                // Note that ByteWidth alwalys needs to be a multiple of the 16 if using D3D11_BIND_CONSTANT_BUFFER or CreateBuffer will fail.
                BufferDescription lightBufferDesc = new BufferDescription() 
                {
                    Usage = ResourceUsage.Dynamic,
                    SizeInBytes = Utilities.SizeOf<DLightBuffer>(), // Must be divisable by 16 bytes, so this is equated to 32.
                    BindFlags = BindFlags.ConstantBuffer,
                    CpuAccessFlags = CpuAccessFlags.Write,
                    OptionFlags = ResourceOptionFlags.None, 
                    StructureByteStride = 0
                };

                // Create the constant buffer pointer so we can access the vertex shader constant buffer from within this class.
                ConstantLightBuffer = new SharpDX.Direct3D11.Buffer(device, lightBufferDesc);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error initializing shader. Error is " + ex.Message);
                return false;
            }
        }
        public void ShutDown()
        {
            // Shutdown the vertex and pixel shaders as well as the related objects.
            ShuddownShader();
        }
        private void ShuddownShader()
        {
            // Release the light constant buffer.
            ConstantLightBuffer?.Dispose();
            ConstantLightBuffer = null;
            // Release the matrix constant buffer.
            ConstantMatrixBuffer?.Dispose();
            ConstantMatrixBuffer = null;
            // Release the sampler state.
            SampleState?.Dispose();
            SampleState = null;
            // Release the layout.
            Layout?.Dispose();
            Layout = null;
            // Release the pixel shader.
            PixelShader?.Dispose();
            PixelShader = null;
            // Release the vertex shader.
            VertexShader?.Dispose();
            VertexShader = null;
        }
        public bool Render(DeviceContext deviceContext, int indexCount, Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix, ShaderResourceView colorTexture, ShaderResourceView normalTexture, Vector4 lightDiffuseColour, Vector3 lightDirection, float colorTextureBrightness)
        {
            // Set the shader parameters that it will use for rendering.
            if (!SetShaderParameters(deviceContext, worldMatrix, viewMatrix, projectionMatrix, colorTexture, normalTexture, lightDiffuseColour, lightDirection, colorTextureBrightness))
                return false;

            // Now render the prepared buffers with the shader.
            RenderShader(deviceContext, indexCount);

            return true;
        }
        private bool SetShaderParameters(DeviceContext deviceContext, Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix, ShaderResourceView colorTexture, ShaderResourceView normalTexture, Vector4 lightDiffuseColour, Vector3 lightDirection, float colorTextureBrightness)
        {
            try
            {
                // Transpose the matrices to prepare them for shader.
                worldMatrix.Transpose();
                viewMatrix.Transpose();
                projectionMatrix.Transpose();

                // Lock the matrix constant buffer so it can be written to.
                DataStream mappedResource;
                deviceContext.MapSubresource(ConstantMatrixBuffer, MapMode.WriteDiscard, MapFlags.None, out mappedResource);

                // Copy the matrices into the constant buffer.
                DMatrixBuffer matrixBuffer = new DMatrixBuffer() 
                {
                    world = worldMatrix,
                    view = viewMatrix,
                    projection = projectionMatrix
                };
                mappedResource.Write(matrixBuffer);

                // Unlock the constant buffer.
                deviceContext.UnmapSubresource(ConstantMatrixBuffer, 0);

                // Set the position of the constant buffer in the vertex shader.
                int bufferPositionNumber = 0;

                // Finally set the constant buffer in the vertex shader with the updated values.
                deviceContext.VertexShader.SetConstantBuffer(bufferPositionNumber, ConstantMatrixBuffer); 
               
                // Lock the light constant buffer so it can be written to.
                deviceContext.MapSubresource(ConstantLightBuffer, MapMode.WriteDiscard, MapFlags.None, out mappedResource);

                // Copy the lighting variables into the constant buffer.
                DLightBuffer lightBuffer = new DLightBuffer() 
                {
                    lightDiffuseColor = lightDiffuseColour,
                    lightDirection = lightDirection,
                    colorTextureBrightness = colorTextureBrightness
                };
                mappedResource.Write(lightBuffer);

                // Unlock the constant buffer.
                deviceContext.UnmapSubresource(ConstantLightBuffer, 0);

                // Set the position of the light constant buffer in the pixel shader.
                bufferPositionNumber = 0;

                // Finally set the light constant buffer in the pixel shader with the updated values.
                deviceContext.PixelShader.SetConstantBuffer(bufferPositionNumber, ConstantLightBuffer);

                // Set shader texture resources in the pixel shader.
                deviceContext.PixelShader.SetShaderResources(0, colorTexture, normalTexture);

                return true;
            }
            catch
            {
                return false;
            }
        }
        private void RenderShader(DeviceContext deviceContext, int indexCount)
        {
            // Set the vertex input layout.
            deviceContext.InputAssembler.InputLayout = Layout;

            // Set the vertex and pixel shaders that will be used to render this triangle.
            deviceContext.VertexShader.Set(VertexShader);
            deviceContext.PixelShader.Set(PixelShader);

            // Set the sampler state in the pixel shader.
            deviceContext.PixelShader.SetSampler(0, SampleState);

            // Render the triangle.
            deviceContext.DrawIndexed(indexCount, 0, 0);
        }
    }
}