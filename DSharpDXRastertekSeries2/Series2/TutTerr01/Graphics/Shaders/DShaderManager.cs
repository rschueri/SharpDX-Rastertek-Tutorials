﻿using SharpDX;
using SharpDX.Direct3D11;
using System;

namespace DSharpDXRastertek.Series2.TutTerr01.Graphics.Shaders
{
    public class DShaderManager
    {
        // Properties
        public DColorShader ColorShader { get; set; }
        public DFontShader FontShader { get; set; }

        // Methods
        public bool Initilize(DDX11 D3DDevice, IntPtr windowsHandle)
        {
            // Create the texture shader object.
            ColorShader = new DColorShader();

            // Initialize the texture shader object.
            if (!ColorShader.Initialize(D3DDevice.Device, windowsHandle))
                return false;

            // Create the font shader object.
            FontShader = new DFontShader();

            // Initialize the font shader object.
            if (!FontShader.Initialize(D3DDevice.Device, windowsHandle))
                return false;

            return true;
        }
        public void ShutDown()
        {
            // Release the font shader object.
            FontShader?.Shuddown();
            FontShader = null;
            // Release the texture shader object.
            ColorShader?.ShutDown();
            ColorShader = null;
        }
        public bool RenderColorShader(DeviceContext deviceContext, int indexCount, Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix)
        {
            // Render the TextureShader.
            if (!ColorShader.Render(deviceContext, indexCount, worldMatrix, viewMatrix, projectionMatrix))
                return false;

            return true;
        }
        public bool RenderFontShader(DeviceContext deviceContext, int indexCount, Matrix worldMatrix, Matrix viewMatrix, Matrix orthoMatrix, ShaderResourceView texture, Vector4 fontColour)
        {
            // Render the FontShader.
            if (!FontShader.Render(deviceContext, indexCount, worldMatrix, viewMatrix, orthoMatrix, texture, fontColour))
                return false;

            return true;
        }
    }
}