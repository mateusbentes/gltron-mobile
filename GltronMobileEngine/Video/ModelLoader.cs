using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GltronMobileEngine.Video
{
    public static class ModelLoader
    {
        /// <summary>
        /// Load a model trying multiple asset names in order; returns first that succeeds.
        /// </summary>
        private static Model? TryLoadModelMany(ContentManager content, string[] names, string label)
        {
            foreach (var name in names)
            {
                try
                {
                    var m = content.Load<Model>(name);
                    System.Diagnostics.Debug.WriteLine($"GLTRON: ✅ Loaded {label} as '{name}'");
                    return m;
                }
                catch (ContentLoadException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"GLTRON: ❌ Failed to load {label} as '{name}': {ex.Message}");
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"GLTRON: ❌ Unexpected error loading {label} as '{name}': {ex.Message}");
                }
            }
            return null;
        }

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
        /// Load the lightcycle model for a player. Currently uses a common FBX.
        /// </summary>
        public static Model? LoadLightcycle(ContentManager content, int playerNum)
        {
            // Try FBX under common asset names, then OBJ as fallback
            var model = TryLoadModelMany(content,
                new [] { "Assets/lightcyclehigh", "lightcyclehigh", "Assets/lightcyclehigh.fbx", "lightcyclehigh.fbx" },
                $"Lightcycle P{playerNum}");
            if (model != null) return model;

            // Fallback to OBJ
            return TryLoadModelMany(content,
                new [] { "Assets/lightcyclehigh", "lightcyclehigh", "Assets/lightcyclehigh.obj", "lightcyclehigh.obj" },
                $"Lightcycle OBJ P{playerNum}");
        }

        /// <summary>
        /// Load the recognizer model (FBX first, OBJ fallback)
        /// </summary>
        public static Model? LoadRecognizer(ContentManager content)
        {
            var model = TryLoadModelMany(content,
                new [] { "Assets/recognizerhigh", "recognizerhigh", "Assets/recognizerhigh.fbx", "recognizerhigh.fbx" },
                "Recognizer");
            if (model != null) return model;

            return TryLoadModelMany(content,
                new [] { "Assets/recognizerhigh", "recognizerhigh", "Assets/recognizerhigh.obj", "recognizerhigh.obj" },
                "Recognizer OBJ");
        }

        /// <summary>
        /// Legacy placeholder removed from gameplay use. Use FBX loader above.
        /// </summary>
        [System.Obsolete("Use LoadLightcycle(content, playerNum) instead")] 
        public static SimpleModel LoadLightcycle()
        {
            return new SimpleModel();
        }
    }
}
