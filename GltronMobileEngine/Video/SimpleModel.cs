using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GltronMobileEngine.Video
{
    /// <summary>
    /// Wrapper for MonoGame Model objects with additional functionality
    /// </summary>
    public class SimpleModel
    {
        public Model? FbxModel { get; set; }
        public Matrix[]? BoneTransforms { get; set; }
        
        public SimpleModel()
        {
        }
        
        public SimpleModel(Model fbxModel)
        {
            FbxModel = fbxModel;
            if (fbxModel != null)
            {
                BoneTransforms = new Matrix[fbxModel.Bones.Count];
                fbxModel.CopyAbsoluteBoneTransformsTo(BoneTransforms);
            }
        }
        
        /// <summary>
        /// Draw the FBX model if available
        /// </summary>
        public void Draw(GraphicsDevice device, Matrix world, Matrix view, Matrix proj, Color tint)
        {
            if (FbxModel == null || BoneTransforms == null) return;
            
            foreach (var mesh in FbxModel.Meshes)
            {
                Matrix meshWorld = BoneTransforms[mesh.ParentBone.Index] * world;
                
                foreach (var effect in mesh.Effects)
                {
                    if (effect is BasicEffect basicEffect)
                    {
                        basicEffect.World = meshWorld;
                        basicEffect.View = view;
                        basicEffect.Projection = proj;
                        basicEffect.EnableDefaultLighting();
                        basicEffect.DiffuseColor = tint.ToVector3();
                        basicEffect.Alpha = tint.A / 255.0f;
                    }
                }
                mesh.Draw();
            }
        }
        
        /// <summary>
        /// Get approximate bounding box size from the model
        /// </summary>
        public Vector3 GetBoundingBoxSize()
        {
            if (FbxModel == null) return Vector3.One;
            
            var min = new Vector3(float.MaxValue);
            var max = new Vector3(float.MinValue);
            
            foreach (var mesh in FbxModel.Meshes)
            {
                var sphere = mesh.BoundingSphere;
                var center = sphere.Center;
                var radius = sphere.Radius;
                min = Vector3.Min(min, center - new Vector3(radius));
                max = Vector3.Max(max, center + new Vector3(radius));
            }
            
            return max - min;
        }
    }
}
