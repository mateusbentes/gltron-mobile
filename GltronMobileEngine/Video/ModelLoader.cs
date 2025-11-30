using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GltronMobileEngine.Video
{
    public static class ModelLoader
    {
        /// <summary>
        /// Load FBX models from content pipeline
        /// </summary>
        public static Model? LoadFbxModel(ContentManager content, string assetPath)
        {
            try
            {
                return content.Load<Model>(assetPath);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GLTRON: Failed to load FBX model '{assetPath}': {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Legacy method - now returns null since we use FBX models
        /// </summary>
        [System.Obsolete("Use LoadFbxModel instead")]
        public static SimpleModel LoadLightcycle()
        {
            return new SimpleModel();
        }
    }
}
