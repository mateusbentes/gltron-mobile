using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GltronMobileEngine.Video
{
    /// <summary>
    /// Comprehensive diagnostics for model loading issues
    /// </summary>
    public static class ModelDiagnostics
    {
        public static void DiagnoseModelLoading(ContentManager content)
        {
            System.Diagnostics.Debug.WriteLine("=== GLTRON MODEL DIAGNOSTICS ===");
            
            // Test 1: Check if content manager is working
            try
            {
                System.Diagnostics.Debug.WriteLine("✓ ContentManager is available");
                System.Diagnostics.Debug.WriteLine($"✓ Content root directory: {content.RootDirectory}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ContentManager error: {ex.Message}");
                return;
            }
            
            // Test 2: Try loading a known working asset (texture)
            try
            {
                var testTexture = content.Load<Texture2D>("Assets/gltron_floor");
                System.Diagnostics.Debug.WriteLine("✓ Basic asset loading works (texture loaded successfully)");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Basic asset loading failed: {ex.Message}");
                System.Diagnostics.Debug.WriteLine("❌ This suggests content pipeline issues");
            }
            
            // Test 3: Try loading FBX models with detailed error reporting
            TestModelLoading(content, "Assets/lightcyclehigh", "Lightcycle");
            TestModelLoading(content, "Assets/recognizerhigh", "Recognizer");
            
            System.Diagnostics.Debug.WriteLine("=== END DIAGNOSTICS ===");
        }
        
        private static void TestModelLoading(ContentManager content, string assetPath, string modelName)
        {
            System.Diagnostics.Debug.WriteLine($"\n--- Testing {modelName} Model ---");
            
            try
            {
                System.Diagnostics.Debug.WriteLine($"Attempting to load: {assetPath}");
                
                var model = content.Load<Model>(assetPath);
                
                if (model == null)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ {modelName}: Model loaded but is null");
                    return;
                }
                
                System.Diagnostics.Debug.WriteLine($"✓ {modelName}: Model loaded successfully!");
                System.Diagnostics.Debug.WriteLine($"  - Meshes: {model.Meshes.Count}");
                System.Diagnostics.Debug.WriteLine($"  - Bones: {model.Bones.Count}");
                
                // Check each mesh
                for (int i = 0; i < model.Meshes.Count; i++)
                {
                    var mesh = model.Meshes[i];
                    System.Diagnostics.Debug.WriteLine($"  - Mesh {i}: '{mesh.Name}'");
                    System.Diagnostics.Debug.WriteLine($"    * Effects: {mesh.Effects.Count}");
                    System.Diagnostics.Debug.WriteLine($"    * Bounding Sphere: Center({mesh.BoundingSphere.Center.X:F2}, {mesh.BoundingSphere.Center.Y:F2}, {mesh.BoundingSphere.Center.Z:F2}) Radius({mesh.BoundingSphere.Radius:F2})");
                    
                    // Check effects
                    foreach (var effect in mesh.Effects)
                    {
                        System.Diagnostics.Debug.WriteLine($"    * Effect Type: {effect.GetType().Name}");
                        if (effect is BasicEffect be)
                        {
                            System.Diagnostics.Debug.WriteLine($"      - Has Texture: {be.Texture != null}");
                            System.Diagnostics.Debug.WriteLine($"      - Diffuse Color: ({be.DiffuseColor.X:F2}, {be.DiffuseColor.Y:F2}, {be.DiffuseColor.Z:F2})");
                        }
                    }
                }
                
                // Test bone transforms
                try
                {
                    var boneTransforms = new Microsoft.Xna.Framework.Matrix[model.Bones.Count];
                    model.CopyAbsoluteBoneTransformsTo(boneTransforms);
                    System.Diagnostics.Debug.WriteLine($"✓ {modelName}: Bone transforms copied successfully");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ {modelName}: Bone transform error: {ex.Message}");
                }
            }
            catch (ContentLoadException ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ {modelName}: ContentLoadException - {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ This usually means the FBX file wasn't processed by the content pipeline");
                System.Diagnostics.Debug.WriteLine($"❌ Check that {assetPath}.fbx exists and is included in Content.mgcb");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ {modelName}: Unexpected error - {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ Stack trace: {ex.StackTrace}");
            }
        }
    }
}
