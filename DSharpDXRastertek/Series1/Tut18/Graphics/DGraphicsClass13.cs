﻿using DSharpDXRastertek.Tut18.Graphics.Camera;
using DSharpDXRastertek.Tut18.Graphics.Models;
using DSharpDXRastertek.Tut18.Graphics.Shaders;
using DSharpDXRastertek.Tut18.System;
using System;
using System.Linq;
using System.Windows.Forms;

namespace DSharpDXRastertek.Tut18.Graphics
{
    public class DGraphics                  // 116 lines
    {
        // Properties
        private DDX11 D3D { get; set; }
        private DCamera Camera { get; set; }
        private DModel Model { get; set; }
        private DLightMapShader LightMapShader { get; set; }

        // Construtor
        public DGraphics() { }

        // Methods.
        public bool Initialize(DSystemConfiguration configuration, IntPtr windowHandle)
        {
            try
            {
                // Create the Direct3D object.
                D3D = new DDX11();
                
                // Initialize the Direct3D object.
                if (!D3D.Initialize(configuration, windowHandle))
                    return false;

                // Create the camera object
                Camera = new DCamera();

                // Initialize a base view matrix the camera for 2D user interface rendering.
                Camera.SetPosition(0, 0, -1);
                Camera.Render();
               
                // Create the model class.
                Model = new DModel();

                // Initialize the model object.
                if (!Model.Initialize(D3D.Device, "square.txt", new[] { "stone01.dds", "light01.dds" }))
                {
                    MessageBox.Show("Could not initialize the model object", "Error", MessageBoxButtons.OK);
                    return false;
                }

                // Create the light shader object.
                LightMapShader = new DLightMapShader();

                // Initialize the light shader object.
                if (!LightMapShader.Initialize(D3D.Device, windowHandle))
                {
                    MessageBox.Show("Could not initialize the light shader", "Error", MessageBoxButtons.OK);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not initialize Direct3D\nError is '" + ex.Message + "'");
                return false;
            }
        }
        public void Shutdown()
        {
            Camera = null;

            // Release the light shader object.
            LightMapShader?.ShutDown();
            LightMapShader = null;
            // Release the model object.
            Model?.Shutdown();
            Model = null;
            // Release the Direct3D object.
            D3D?.ShutDown();
            D3D = null;
        }
        internal bool Frame()
        {
            // Set the position of the camera.
            Camera.SetPosition(0, 0, -5.0f);

            return true;
        }
        public bool Render()
        {
            // Clear the buffer to begin the scene.
            D3D.BeginScene(0f, 0f, 0f, 1f);

            // Generate the view matrix based on the camera position.
            Camera.Render();

            // Get the world, view, and projection matrices from camera and d3d objects.
            var viewMatrix = Camera.ViewMatrix;
            var worldMatrix = D3D.WorldMatrix;
            var projectionMatrix = D3D.ProjectionMatrix;
           
            // Put the model vertex and index buffer on the graphics pipeline to prepare them for drawing.
            Model.Render(D3D.DeviceContext);

            // Render the model using the color shader.
            if (!LightMapShader.Render(D3D.DeviceContext, Model.IndexCount, worldMatrix, viewMatrix, projectionMatrix, Model.TextureCollection.Select(item => item.TextureResource).ToArray()))
                return false;

            // Present the rendered scene to the screen.
            D3D.EndScene();

            return true;
        }
    }
}