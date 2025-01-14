﻿using DSharpDXRastertek.Tut21.Graphics.Data;
using DSharpDXRastertek.Tut21.System;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace DSharpDXRastertek.Tut21.Graphics.Models
{
    public class DBumpMapModel                  // 353 lines
    {
        // Structures
        [StructLayout(LayoutKind.Sequential)]
        public struct DVertex
        {
            public Vector3 position;
            public Vector2 texture;
            public Vector3 normal;
            public Vector3 tangent;
            public Vector3 binormal;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct DModelFormat
        {
            public float x, y, z;       // Position
            public float tu, tv;        // Texture Co-ordinates
            public float nx, ny, nz;    // Normal
            public float tx, ty, tz;    // Tangrnt Normal
            public float bx, by, bz;    // Bi-Tangent Normal
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct DTempVertex
        {
            public float x, y, z;
            public float tu, tv;
            public float nx, ny, nz;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct DVector
        {
            public float x, y, z;
        }

        // Properties
        private SharpDX.Direct3D11.Buffer VertexBuffer { get; set; }
        private SharpDX.Direct3D11.Buffer IndexBuffer { get; set; }
        private int VertexCount { get; set; }
        public int IndexCount { get; private set; }
        public DTextureArray TextureCollection { get; private set; }
        public DModelFormat[] ModelObject { get; private set; }

        // Constructor
        public DBumpMapModel() { }

        // Methods
        public bool Initialize(SharpDX.Direct3D11.Device device, string modelFormatFilename, string[] textureFileNames)
        {
            // Load in the model data.
            if (!LoadModel(modelFormatFilename))
                return false;

            // Calculate the normal, tangent, and binormal vectors for the model.
            CalculateModelVectors();

            // Initialize the vertex and index buffer.
            if (!InitializeBuffers(device))
                return false;

            // Load the texture for this model.
            if (!LoadTextures(device, textureFileNames))
                return false;

            return true;
        }
        private bool LoadModel(string modelFormatFilename)
        {
            modelFormatFilename = DSystemConfiguration.ModelFilePath + modelFormatFilename;
            List<string> lines = null;

            try
            {
                lines = File.ReadLines(modelFormatFilename).ToList();

                var vertexCountString = lines[0].Split(new char[] { ':' })[1].Trim();
                VertexCount = int.Parse(vertexCountString);
                IndexCount = VertexCount;
                ModelObject = new DModelFormat[VertexCount];

                for (var i = 4; i < lines.Count && i < 4 + VertexCount; i++)
                {
                    var modelArray = lines[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                    ModelObject[i - 4] = new DModelFormat()
                    {
                        x = float.Parse(modelArray[0]),
                        y = float.Parse(modelArray[1]),
                        z = float.Parse(modelArray[2]),
                        tu = float.Parse(modelArray[3]),
                        tv = float.Parse(modelArray[4]),
                        nx = float.Parse(modelArray[5]),
                        ny = float.Parse(modelArray[6]),
                        nz = float.Parse(modelArray[7])
                    };
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private bool LoadTextures(SharpDX.Direct3D11.Device device, string[] textureFileNames)
        {
           for (var i = 0; i < textureFileNames.Length; i++)
				textureFileNames[i] = DSystemConfiguration.DataFilePath + textureFileNames[i];

            // Create the texture object.
            TextureCollection = new DTextureArray();

            // Initialize the texture object.
            TextureCollection.Initialize(device, textureFileNames);

            return true;
        }
        public void Shutdown()
        {
            // Release the model texture.
            ReleaseTextures();

            // Release the vertex and index buffers.
            ShutdownBuffers();

            // Release the model data.
            ReleaseModel();
        }
        private void ReleaseModel()
        {
            ModelObject = null;
        }
        private void ReleaseTextures()
        {
            // Release the textures object.
            TextureCollection?.Shutdown();
            TextureCollection = null;
        }
        public void Render(SharpDX.Direct3D11.DeviceContext deviceContext)
        {
            // Put the vertex and index buffers on the graphics pipeline to prepare for drawings.
            RenderBuffers(deviceContext);
        }
        private bool InitializeBuffers(SharpDX.Direct3D11.Device device)
        {
            try
            {
                // Create the vertex array.
                
                DVertex[] vertices = new DVertex[VertexCount];
                // Create the index array.
                var indices = new int[IndexCount];

                for (var i = 0; i < VertexCount; i++)
                {
                    vertices[i] = new DVertex()
                    {
                        position = new Vector3(ModelObject[i].x, ModelObject[i].y, ModelObject[i].z),
                        texture = new Vector2(ModelObject[i].tu, ModelObject[i].tv),
                        normal = new Vector3(ModelObject[i].nx, ModelObject[i].ny, ModelObject[i].nz),
                        tangent = new Vector3(ModelObject[i].tx, ModelObject[i].ty, ModelObject[i].tz),
                        binormal = new Vector3(ModelObject[i].bx, ModelObject[i].by, ModelObject[i].bz)
                    };

                    indices[i] = i;
                }

                // Create the vertex buffer.
                VertexBuffer = SharpDX.Direct3D11.Buffer.Create(device, BindFlags.VertexBuffer, vertices);

                // Create the index buffer.
                IndexBuffer = SharpDX.Direct3D11.Buffer.Create(device, BindFlags.IndexBuffer, indices);

                return true;
            }
            catch
            {
                return false;
            }
        }
        private void ShutdownBuffers()
        {
            // Return the index buffer.
            IndexBuffer?.Dispose();
            IndexBuffer = null;
            // Release the vertex buffer.
            VertexBuffer?.Dispose();
            VertexBuffer = null;
        }
        private void RenderBuffers(SharpDX.Direct3D11.DeviceContext deviceContext)
        {
            // Set the vertex buffer to active in the input assembler so it can be rendered.
            deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(VertexBuffer, Utilities.SizeOf<DVertex>(), 0));
            // Set the index buffer to active in the input assembler so it can be rendered.
            deviceContext.InputAssembler.SetIndexBuffer(IndexBuffer, Format.R32_UInt, 0);
            // Set the type of the primitive that should be rendered from this vertex buffer, in this case triangles.
            deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
        }
        public void CalculateModelVectors()
        {
            // Calculate the number of faces in the model.
            int faceCount = VertexCount / 3;

            // Initialize the index to the model data.
            int index = 0;

            DTempVertex vertex1, vertex2, vertex3;
            DVector tangent, binormal, normal;

            // Go through all the faces and calculate the tangent, binormal, and normal vectors.
            for (int i = 0; i < faceCount; i++)
            {
                // Get the three vertices for the face from the model.
                vertex1.x = ModelObject[index].x;
                vertex1.y = ModelObject[index].y;
                vertex1.z = ModelObject[index].z;
                vertex1.tu = ModelObject[index].tu;
                vertex1.tv = ModelObject[index].tv;
                vertex1.nx = ModelObject[index].nx;
                vertex1.ny = ModelObject[index].ny;
                vertex1.nz = ModelObject[index].nz;
                index++;

                // Second Vertrx
                vertex2.x = ModelObject[index].x;
                vertex2.y = ModelObject[index].y;
                vertex2.z = ModelObject[index].z;
                vertex2.tu = ModelObject[index].tu;
                vertex2.tv = ModelObject[index].tv;
                vertex2.nx = ModelObject[index].nx;
                vertex2.ny = ModelObject[index].ny;
                vertex2.nz = ModelObject[index].nz;
                index++;

                // Third Vertex
                vertex3.x = ModelObject[index].x;
                vertex3.y = ModelObject[index].y;
                vertex3.z = ModelObject[index].z;
                vertex3.tu = ModelObject[index].tu;
                vertex3.tv = ModelObject[index].tv;
                vertex3.nx = ModelObject[index].nx;
                vertex3.ny = ModelObject[index].ny;
                vertex3.nz = ModelObject[index].nz;
                index++;

                // Calculate the tangent and binormal of that face.
                CalculateTangentBinormal(vertex1, vertex2, vertex3, out tangent, out binormal);

                // Calculate the new normal using the tangent and binormal.
                CalculateNormal(tangent, binormal, out normal);

                // Store the normal, tangent, and binormal for this face back in the model structure.
                ModelObject[index - 1].nx = normal.x;
                ModelObject[index - 1].ny = normal.y;
                ModelObject[index - 1].nz = normal.z;
                ModelObject[index - 1].tx = tangent.x;
                ModelObject[index - 1].ty = tangent.y;
                ModelObject[index - 1].tz = tangent.z;
                ModelObject[index - 1].bx = binormal.x;
                ModelObject[index - 1].by = binormal.y;
                ModelObject[index - 1].bz = binormal.z;

                // Second Vertex
                ModelObject[index - 2].nx = normal.x;
                ModelObject[index - 2].ny = normal.y;
                ModelObject[index - 2].nz = normal.z;
                ModelObject[index - 2].tx = tangent.x;
                ModelObject[index - 2].ty = tangent.y;
                ModelObject[index - 2].tz = tangent.z;
                ModelObject[index - 2].bx = binormal.x;
                ModelObject[index - 2].by = binormal.y;
                ModelObject[index - 2].bz = binormal.z;

                // Third Vertex
                ModelObject[index - 3].nx = normal.x;
                ModelObject[index - 3].ny = normal.y;
                ModelObject[index - 3].nz = normal.z;
                ModelObject[index - 3].tx = tangent.x;
                ModelObject[index - 3].ty = tangent.y;
                ModelObject[index - 3].tz = tangent.z;
                ModelObject[index - 3].bx = binormal.x;
                ModelObject[index - 3].by = binormal.y;
                ModelObject[index - 3].bz = binormal.z;
            }
        }
        private void CalculateTangentBinormal(DTempVertex vertex1, DTempVertex vertex2, DTempVertex vertex3, out DVector tangent, out DVector binormal)
        {
            // Calculate the two vectors for the this face.
            float[] vector1 = new[] { vertex2.x - vertex1.x, vertex2.y - vertex1.y, vertex2.z - vertex1.z };
            float[] vector2 = new[] { vertex3.x - vertex1.x, vertex3.y - vertex1.y, vertex3.z - vertex1.z };

            // Calculate the tu and tv texture space vectors.
            float[] tuVector = new[] { vertex2.tu - vertex1.tu, vertex3.tu - vertex1.tu };
            float[] tvVector = new[] { vertex2.tv - vertex1.tv, vertex3.tv - vertex1.tv };

            // Calculate the denominator of the tangent / binormal equation.
            float den = 1.0f / (tuVector[0] * tvVector[1] - tuVector[1] * tvVector[0]);

            // Calculate the cross products and multiply by the coefficient to get the tangent and binormal.
            tangent.x = (tvVector[1] * vector1[0] - tvVector[0] * vector2[0]) * den;
            tangent.y = (tvVector[1] * vector1[1] - tvVector[0] * vector2[1]) * den;
            tangent.z = (tvVector[1] * vector1[2] - tvVector[0] * vector2[2]) * den;

            binormal.x = (tuVector[0] * vector2[0] - tuVector[1] * vector1[0]) * den;
            binormal.y = (tuVector[0] * vector2[1] - tuVector[1] * vector1[1]) * den;
            binormal.z = (tuVector[0] * vector2[2] - tuVector[1] * vector1[2]) * den;

            // Calculate the length of this Tengent normal.
            float length = (float)Math.Sqrt(tangent.x * tangent.x + tangent.y * tangent.y + tangent.z * tangent.z);

            // Normalize the normal and the store it.
            tangent.x = tangent.x / length;
            tangent.y = tangent.y / length;
            tangent.z = tangent.z / length;

            // Calculate the length of this Bi-Tangent normal.
            length = (float)Math.Sqrt(binormal.x * binormal.x + binormal.y * binormal.y + binormal.z * binormal.z);

            // Normalize the Bi-Tangent normal and the store it.
            binormal.x = binormal.x / length;
            binormal.y = binormal.y / length;
            binormal.z = binormal.z / length;
        }
        private void CalculateNormal(DVector tangent, DVector binormal, out DVector normal)
        {
            // Calculate the cross product of the tangent and binormal which will give the normal vector.
            normal.x = tangent.y * binormal.z - tangent.z * binormal.y;
            normal.y = tangent.z * binormal.x - tangent.x * binormal.z;
            normal.z = tangent.x * binormal.y - tangent.y * binormal.x;

            // Calculate the length of the normal.
            var length = (float)Math.Sqrt(normal.x * normal.x + normal.y * normal.y + normal.z * normal.z);

            // Normalize the normal.
            normal.x = normal.x / length;
            normal.y = normal.y / length;
            normal.z = normal.z / length;
        }
    }
}