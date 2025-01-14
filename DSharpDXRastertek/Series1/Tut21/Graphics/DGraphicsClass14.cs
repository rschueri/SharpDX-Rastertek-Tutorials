﻿using DSharpDXRastertek.Tut21.Graphics.Camera;
using DSharpDXRastertek.Tut21.Graphics.Data;
using DSharpDXRastertek.Tut21.Graphics.Models;
using DSharpDXRastertek.Tut21.Graphics.Shaders;
using DSharpDXRastertek.Tut21.System;
using SharpDX;
using System;
using System.Linq;
using System.Windows.Forms;

namespace DSharpDXRastertek.Tut21.Graphics
{
    public class DGraphics                 // 149 lines
    {
        // Properties
        private DDX11 D3D { get; set; }
        public DCamera Camera { get; set; }
        private DLight Light { get; set; }
        private DBumpMapModel BumpMapModel { get; set; }
        private DSpecMapShader SpecMapShader { get; set; }

        // Static properties
        public static float Rotation { get; set; }

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
                Camera.SetPosition(0, 0, -5);
                Camera.Render();
               
                // Create the model class.
                BumpMapModel = new DBumpMapModel();

                // Initialize the model object.
                if (!BumpMapModel.Initialize(D3D.Device, "Cube.txt", new[] { "stone02.bmp", "bump02.bmp", "spec02.bmp" }))
                {
                    MessageBox.Show("Could not initialize the model object", "Error", MessageBoxButtons.OK);
                    return false;
                }

                // Create the bump map shader object.
				SpecMapShader = new DSpecMapShader();

				// Initialize the bump map shader object.
				if (!SpecMapShader.Initialize(D3D.Device, windowHandle))
				{
					MessageBox.Show("Could not initialize the light shader", "Error", MessageBoxButtons.OK);
					return false;
				}

                // Create the light object.
                Light = new DLight();

                // Initialize the light object.
				Light.SetDiffuseColor(1, 1, 1, 1f);
				Light.SetDirection(0, 0, 1);
				Light.SetSpecularColor(0, 1, 1, 1);
				Light.SetSpecularPower(16);

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
            // Release the light object.
            Light = null;
            // Release the camera object.
            Camera = null;

            // Release the shader 
            SpecMapShader?.ShutDown();
            SpecMapShader = null;
            // Release the model object.
            BumpMapModel?.Shutdown();
            BumpMapModel = null;
            // Release the Direct3D object.
            D3D?.ShutDown();
            D3D = null;
        }
        public bool Frame()
        {
            // Set the position of the camera.
            Camera.SetPosition(0.0f, 0.0f, -5.0f);

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
            
            // Rotate the world matrix by the rotation value so that the triangle will spin.
            Rotate();
            
            // Construct the frustum.
            // Rotate the world matrix by the rotation value so that the triangle will spin.
            Matrix.RotationY(Rotation, out worldMatrix);

            // Put the model vertex and index buffers on the graphics pipeline to prepare them for drawing.
            BumpMapModel.Render(D3D.DeviceContext);

            // Render the model using the color shader.
			if (!SpecMapShader.Render(D3D.DeviceContext, BumpMapModel.IndexCount, worldMatrix, viewMatrix, projectionMatrix, BumpMapModel.TextureCollection.Select(item => item.TextureResource).ToArray(), Light.Direction, Light.DiffuseColour, Camera.GetPosition(), Light.SpecularColor, Light.SpecularPower))
				return false;

            // Present the rendered scene to the screen.
            D3D.EndScene();

            return true;
        }

        // Static Methods.
        static void Rotate()
        {
            Rotation += (float)Math.PI * 0.0025f;
            if (Rotation > 360)
                Rotation -= 360;
        }
    }
}