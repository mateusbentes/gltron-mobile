using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace GltronMobileEngine.Video
{
    /// <summary>
    /// Simple OBJ loader for GLTron motorcycle models
    /// Loads basic vertex and face data from .obj files
    /// </summary>
    public class SimpleObjLoader
    {
        public class SimpleObjModel
        {
            public VertexPositionNormalTexture[] Vertices { get; set; } = Array.Empty<VertexPositionNormalTexture>();
            public int[] Indices { get; set; } = Array.Empty<int>();
            public int TriangleCount => Indices.Length / 3;
        }

        public static SimpleObjModel? LoadFromContent(ContentManager content, string assetPath)
        {
            try
            {
                // Try to load as a text asset
                string objContent = content.Load<string>(assetPath);
                return ParseObjContent(objContent);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GLTRON: Failed to load OBJ from content: {ex.Message}");
                return null;
            }
        }

        public static SimpleObjModel? LoadFromFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    System.Diagnostics.Debug.WriteLine($"GLTRON: OBJ file not found: {filePath}");
                    return null;
                }

                string objContent = File.ReadAllText(filePath);
                return ParseObjContent(objContent);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GLTRON: Failed to load OBJ file: {ex.Message}");
                return null;
            }
        }

        private static SimpleObjModel ParseObjContent(string objContent)
        {
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var texCoords = new List<Vector2>();
            var faces = new List<(int v, int vt, int vn)>();

            string[] lines = objContent.Split('\n');

            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#"))
                    continue;

                string[] parts = trimmedLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) continue;

                switch (parts[0])
                {
                    case "v": // Vertex
                        if (parts.Length >= 4)
                        {
                            if (float.TryParse(parts[1], out float x) &&
                                float.TryParse(parts[2], out float y) &&
                                float.TryParse(parts[3], out float z))
                            {
                                vertices.Add(new Vector3(x, y, z));
                            }
                        }
                        break;

                    case "vn": // Normal
                        if (parts.Length >= 4)
                        {
                            if (float.TryParse(parts[1], out float nx) &&
                                float.TryParse(parts[2], out float ny) &&
                                float.TryParse(parts[3], out float nz))
                            {
                                normals.Add(new Vector3(nx, ny, nz));
                            }
                        }
                        break;

                    case "vt": // Texture coordinate
                        if (parts.Length >= 3)
                        {
                            if (float.TryParse(parts[1], out float u) &&
                                float.TryParse(parts[2], out float v))
                            {
                                texCoords.Add(new Vector2(u, v));
                            }
                        }
                        break;

                    case "f": // Face
                        if (parts.Length >= 4) // Triangle or quad
                        {
                            // Parse face indices (format: v/vt/vn or v//vn or v)
                            var faceVertices = new List<(int v, int vt, int vn)>();
                            
                            for (int i = 1; i < parts.Length; i++)
                            {
                                var indices = ParseFaceVertex(parts[i]);
                                if (indices.HasValue)
                                {
                                    faceVertices.Add(indices.Value);
                                }
                            }

                            // Convert quad to triangles if needed
                            if (faceVertices.Count == 3)
                            {
                                // Triangle
                                faces.AddRange(faceVertices);
                            }
                            else if (faceVertices.Count == 4)
                            {
                                // Quad -> two triangles
                                faces.Add(faceVertices[0]);
                                faces.Add(faceVertices[1]);
                                faces.Add(faceVertices[2]);
                                
                                faces.Add(faceVertices[0]);
                                faces.Add(faceVertices[2]);
                                faces.Add(faceVertices[3]);
                            }
                        }
                        break;
                }
            }

            // Build final vertex array
            var finalVertices = new List<VertexPositionNormalTexture>();
            var finalIndices = new List<int>();

            for (int i = 0; i < faces.Count; i++)
            {
                var face = faces[i];
                
                Vector3 position = face.v > 0 && face.v <= vertices.Count ? vertices[face.v - 1] : Vector3.Zero;
                Vector3 normal = face.vn > 0 && face.vn <= normals.Count ? normals[face.vn - 1] : Vector3.Up;
                Vector2 texCoord = face.vt > 0 && face.vt <= texCoords.Count ? texCoords[face.vt - 1] : Vector2.Zero;

                finalVertices.Add(new VertexPositionNormalTexture(position, normal, texCoord));
                finalIndices.Add(i);
            }

            System.Diagnostics.Debug.WriteLine($"GLTRON: Loaded OBJ model - {vertices.Count} vertices, {faces.Count} face vertices, {finalVertices.Count} final vertices");

            return new SimpleObjModel
            {
                Vertices = finalVertices.ToArray(),
                Indices = finalIndices.ToArray()
            };
        }

        private static (int v, int vt, int vn)? ParseFaceVertex(string faceVertex)
        {
            try
            {
                string[] parts = faceVertex.Split('/');
                
                int v = 0, vt = 0, vn = 0;
                
                if (parts.Length >= 1 && !string.IsNullOrEmpty(parts[0]))
                    int.TryParse(parts[0], out v);
                
                if (parts.Length >= 2 && !string.IsNullOrEmpty(parts[1]))
                    int.TryParse(parts[1], out vt);
                
                if (parts.Length >= 3 && !string.IsNullOrEmpty(parts[2]))
                    int.TryParse(parts[2], out vn);

                return (v, vt, vn);
            }
            catch
            {
                return null;
            }
        }
    }
}
