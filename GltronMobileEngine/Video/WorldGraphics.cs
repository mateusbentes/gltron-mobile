using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using GltronMobileEngine.Interfaces;

namespace GltronMobileEngine.Video;

public class WorldGraphics
{
    public BasicEffect Effect { get; private set; }
    private readonly GraphicsDevice _gd;
    private float _gridSize = 100f;

    private Texture2D? _texFloor;
    private Texture2D? _texWall;
    private readonly Texture2D?[] _skyFaces = new Texture2D?[6];
    
    // CRITICAL FIX: Add motorcycle model support
    private SimpleObjLoader.SimpleObjModel? _lightCycleObjModel;
    
    // CRITICAL FIX: Add recognizer model support
    private SimpleObjLoader.SimpleObjModel? _recognizerObjModel;
    private Vector3 _recognizerBoundingBoxSize = Vector3.One;

    public WorldGraphics(GraphicsDevice gd, ContentManager cm)
    {
        _gd = gd;
        Effect = new BasicEffect(gd)
        {
            TextureEnabled = true,
            LightingEnabled = false,
            VertexColorEnabled = false
        };
    }

    public void LoadContent(ContentManager content)
    {
        _texFloor = content.Load<Texture2D>("Assets/gltron_floor");
        _texWall = content.Load<Texture2D>("Assets/gltron_wall_1");
        _skyFaces[0] = content.Load<Texture2D>("Assets/skybox0");
        _skyFaces[1] = content.Load<Texture2D>("Assets/skybox1");
        _skyFaces[2] = content.Load<Texture2D>("Assets/skybox2");
        _skyFaces[3] = content.Load<Texture2D>("Assets/skybox3");
        _skyFaces[4] = content.Load<Texture2D>("Assets/skybox4");
        _skyFaces[5] = content.Load<Texture2D>("Assets/skybox5");
        
        // CRITICAL FIX: Load motorcycle and recognizer models with robust multi-path loading
        _lightCycleObjModel = TryLoadObjModel("lightcyclehigh.obj", content);
        _recognizerObjModel = TryLoadObjModel("recognizerhigh.obj", content);
        
        if (_recognizerObjModel != null)
        {
            _recognizerBoundingBoxSize = CalculateBoundingBoxSize(_recognizerObjModel);
            System.Diagnostics.Debug.WriteLine($"GLTRON: Recognizer bounding box size: {_recognizerBoundingBoxSize}");
        }
    }
    
    /// <summary>
    /// CRITICAL FIX: Robust OBJ model loading with multiple path fallbacks
    /// Uses TitleContainer.OpenStream for cross-platform file access
    /// </summary>
    private SimpleObjLoader.SimpleObjModel? TryLoadObjModel(string filename, ContentManager content)
    {
        System.Diagnostics.Debug.WriteLine($"GLTRON: Attempting to load {filename}");
        
        // Try multiple path formats for cross-platform compatibility
        // Note: Files should be set to "Copy to Output Directory = Copy if newer" in project
        string[] possiblePaths = {
            $"Content/Assets/{filename}",                           // Standard content path
            $"Assets/{filename}",                                   // Direct assets path
            filename,                                               // Direct filename
            $"./Content/Assets/{filename}",                         // Relative content path
            $"./Assets/{filename}",                                 // Relative assets path
            $"./{filename}",                                        // Current directory
            Path.Combine("Content", "Assets", filename),            // Platform-specific path
            Path.Combine("Assets", filename)                        // Platform-specific assets path
        };
        
        foreach (string relativePath in possiblePaths)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"GLTRON: Trying TitleContainer path: {relativePath}");
                
                using var stream = Microsoft.Xna.Framework.TitleContainer.OpenStream(relativePath);
                if (stream == null || stream.Length == 0)
                {
                    System.Diagnostics.Debug.WriteLine($"GLTRON: Stream is null or empty for {relativePath}");
                    continue;
                }
                
                System.Diagnostics.Debug.WriteLine($"GLTRON: Stream opened successfully, length: {stream.Length}");
                
                var model = SimpleObjLoader.LoadFromStream(stream);
                
                if (model != null && model.Vertices.Length > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"GLTRON: {filename} loaded successfully from {relativePath}");
                    System.Diagnostics.Debug.WriteLine($"GLTRON: Model stats - {model.Vertices.Length} vertices, {model.TriangleCount} triangles");
                    return model;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"GLTRON: Model parsing failed or empty for {relativePath}");
                }
            }
            catch (System.Exception pathEx)
            {
                System.Diagnostics.Debug.WriteLine($"GLTRON: Failed to load from {relativePath}: {pathEx.Message}");
            }
        }
        
        System.Diagnostics.Debug.WriteLine($"GLTRON: ALL ATTEMPTS FAILED for {filename} - will use fallback cube rendering");
        System.Diagnostics.Debug.WriteLine($"GLTRON: Make sure {filename} is set to 'Copy to Output Directory = Copy if newer' in the project");
        return null;
    }

    public void BeginDraw(Matrix view, Matrix proj)
    {
        Effect.View = view;
        Effect.Projection = proj;
        Effect.World = Matrix.Identity;
    }

    public void DrawFloor()
    {
        if (_texFloor == null) return;
        
        // CRITICAL FIX: Ensure proper render states for floor
        _gd.BlendState = BlendState.AlphaBlend;
        _gd.DepthStencilState = DepthStencilState.Default;
        _gd.RasterizerState = RasterizerState.CullNone;
        Effect.Texture = _texFloor;
        Effect.World = Matrix.Identity;
        
        // CRITICAL FIX: Floor should be neutral colored
        Effect.DiffuseColor = Vector3.One;
        Effect.Alpha = 1.0f;

        // CRITICAL FIX: Base arena size on actual player coordinate system
        // Players use normalized coordinates (0.0-1.0) * gridSize
        // So the arena should span from 0 to gridSize in both X and Z
        float arenaSize = _gridSize;
        
        // Use reasonable tile size based on arena size for good texture detail
        int tileSize = Math.Max(1, (int)(arenaSize / 8f)); // 8 tiles across the arena
        float uvScale = 1.0f; // Simple 1:1 UV mapping per tile

        System.Diagnostics.Debug.WriteLine($"GLTRON: Floor rendering - ArenaSize: {arenaSize}, TileSize: {tileSize}, UVScale: {uvScale}");

        foreach (var pass in Effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            for (int x = 0; x < (int)arenaSize; x += tileSize)
            {
                for (int z = 0; z < (int)arenaSize; z += tileSize)
                {
                    // Ensure we don't go beyond arena bounds
                    int actualTileX = Math.Min(tileSize, (int)arenaSize - x);
                    int actualTileZ = Math.Min(tileSize, (int)arenaSize - z);
                    
                    var verts = new VertexPositionTexture[4];
                    verts[0] = new VertexPositionTexture(new Vector3(x, 0, z), new Vector2(0f, 0f));
                    verts[1] = new VertexPositionTexture(new Vector3(x + actualTileX, 0, z), new Vector2(uvScale, 0f));
                    verts[2] = new VertexPositionTexture(new Vector3(x, 0, z + actualTileZ), new Vector2(0f, uvScale));
                    verts[3] = new VertexPositionTexture(new Vector3(x + actualTileX, 0, z + actualTileZ), new Vector2(uvScale, uvScale));

                    DrawQuad(verts);
                }
            }
        }
    }

    public void DrawWalls(ISegment[] walls)
    {
        if (_texWall == null) return;

        Effect.World = Matrix.Identity;
        Effect.Texture = _texWall;
        
        // CRITICAL FIX: Walls should be white/neutral colored
        Effect.DiffuseColor = Vector3.One;
        Effect.Alpha = 1.0f;

        // CRITICAL FIX: Use CullNone for proper wall visibility
        _gd.RasterizerState = RasterizerState.CullNone;
        _gd.BlendState = BlendState.AlphaBlend;
        _gd.DepthStencilState = DepthStencilState.Default;

        // CRITICAL FIX: Base wall dimensions on actual player coordinate system
        // Players move in 0 to gridSize range, so walls should enclose this area
        float arenaSize = _gridSize;
        float wallHeight = arenaSize * 0.4f; // Wall height proportional to arena size
        float uvScale = 1.0f; // Simple UV mapping

        System.Diagnostics.Debug.WriteLine($"GLTRON: Wall rendering - ArenaSize: {arenaSize}, WallHeight: {wallHeight}, UVScale: {uvScale}");

        // Create walls that properly enclose the player movement area (0 to gridSize)
        var quads = new (Vector3 a, Vector3 b, Vector3 c, Vector3 d)[]
        {
            // Bottom wall (Z=0) - from X=0 to X=arenaSize
            (new Vector3(0, wallHeight, 0), new Vector3(arenaSize, wallHeight, 0), 
             new Vector3(0, 0, 0), new Vector3(arenaSize, 0, 0)),
            
            // Right wall (X=arenaSize) - from Z=0 to Z=arenaSize
            (new Vector3(arenaSize, wallHeight, 0), new Vector3(arenaSize, wallHeight, arenaSize), 
             new Vector3(arenaSize, 0, 0), new Vector3(arenaSize, 0, arenaSize)),
            
            // Top wall (Z=arenaSize) - from X=arenaSize to X=0
            (new Vector3(arenaSize, wallHeight, arenaSize), new Vector3(0, wallHeight, arenaSize), 
             new Vector3(arenaSize, 0, arenaSize), new Vector3(0, 0, arenaSize)),
            
            // Left wall (X=0) - from Z=arenaSize to Z=0
            (new Vector3(0, wallHeight, arenaSize), new Vector3(0, wallHeight, 0), 
             new Vector3(0, 0, arenaSize), new Vector3(0, 0, 0)),
        };

        // Simple UV coordinates for clean wall texturing
        var uvs = new Vector2[]
        {
            new Vector2(uvScale, 1f), new Vector2(0f, 1f), new Vector2(uvScale, 0f), new Vector2(0f, 0f)
        };

        foreach (var pass in Effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            foreach (var q in quads)
            {
                var verts = new VertexPositionTexture[4];
                verts[0] = new VertexPositionTexture(q.a, uvs[0]);
                verts[1] = new VertexPositionTexture(q.b, uvs[1]);
                verts[2] = new VertexPositionTexture(q.c, uvs[2]);
                verts[3] = new VertexPositionTexture(q.d, uvs[3]);

                DrawQuad(verts);
            }
        }
    }

    public void DrawSkybox()
    {
        if (_skyFaces[0] == null) return;

        // CRITICAL FIX: Proper skybox rendering like Java version
        var prevDepth = _gd.DepthStencilState;
        _gd.DepthStencilState = DepthStencilState.None; // Disable depth testing
        _gd.RasterizerState = RasterizerState.CullNone; // Draw both sides
        _gd.BlendState = BlendState.Opaque; // No blending for skybox
        Effect.World = Matrix.Identity;
        
        // CRITICAL FIX: Skybox should be neutral colored
        Effect.DiffuseColor = Vector3.One; // White/neutral color
        Effect.Alpha = 1.0f;

        // CRITICAL FIX: Skybox positioning like Java version
        float d = _gridSize * 3f;

        var faces = new Vector3[][]
        {
            // Match Java skybox face order and positioning
            new [] { new Vector3(d,d,-d), new Vector3(d,-d,-d), new Vector3(d,-d,d), new Vector3(d,d,d) },      // Right
            new [] { new Vector3(d,d,d), new Vector3(-d,d,d), new Vector3(-d,-d,d), new Vector3(d,-d,d) },      // Front
            new [] { new Vector3(-d,d,-d), new Vector3(d,d,-d), new Vector3(d,d,d), new Vector3(-d,d,d) },      // Top
            new [] { new Vector3(d,-d,-d), new Vector3(-d,-d,-d), new Vector3(-d,-d,d), new Vector3(d,-d,d) },  // Bottom
            new [] { new Vector3(-d,d,-d), new Vector3(-d,-d,-d), new Vector3(d,-d,-d), new Vector3(d,d,-d) },  // Back
            new [] { new Vector3(-d,-d,-d), new Vector3(-d,d,-d), new Vector3(-d,d,d), new Vector3(-d,-d,d) },  // Left
        };

        // CRITICAL FIX: Proper UV coordinates for skybox (match Java version)
        var uvs = new [] { new Vector2(1,1), new Vector2(0,1), new Vector2(0,0), new Vector2(1,0) };

        for (int i = 0; i < 6; i++)
        {
            var tex = _skyFaces[i];
            if (tex == null) continue;
            Effect.Texture = tex;
            foreach (var pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                var f = faces[i];
                var verts = new VertexPositionTexture[4];
                verts[0] = new VertexPositionTexture(f[0], uvs[0]);
                verts[1] = new VertexPositionTexture(f[1], uvs[1]);
                verts[2] = new VertexPositionTexture(f[2], uvs[2]);
                verts[3] = new VertexPositionTexture(f[3], uvs[3]);
                using var vb = new VertexBuffer(_gd, typeof(VertexPositionTexture), 4, BufferUsage.WriteOnly);
                vb.SetData(verts);
                _gd.SetVertexBuffer(vb);
                _gd.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            }
        }

        _gd.DepthStencilState = prevDepth;
    }

    public void DrawBike(BasicEffect fx, IPlayer? p)
    {
        if (p == null) return;
        
        try
        {
            float x = p.getXpos();
            float y = p.getYpos();
            int direction = p.getDirection();
            
            // CRITICAL FIX: Draw actual motorcycle model or fallback
            if (_lightCycleObjModel != null)
            {
                // Use loaded OBJ model
                var world = Matrix.CreateScale(0.5f) * // Scale down the model
                           Matrix.CreateRotationY(direction * MathHelper.PiOver2) *
                           Matrix.CreateTranslation(x, 0f, y);
                fx.World = world;
                
                // Get player color
                int colorIndex = p.getPlayerNum();
                if (p is GltronMobileEngine.Player concretePlayer)
                {
                    colorIndex = concretePlayer.getPlayerColorIndex();
                }
                fx.DiffuseColor = GetPlayerColor(colorIndex);
                fx.Alpha = 1.0f;
                
                // Set up effect for model rendering
                fx.TextureEnabled = false;
                fx.VertexColorEnabled = false;
                fx.LightingEnabled = true;
                fx.EnableDefaultLighting();
                
                // Draw the OBJ model
                DrawObjModel(fx, _lightCycleObjModel);
            }
            else
            {
                // Fallback: Draw a motorcycle-shaped representation
                var world = Matrix.CreateScale(2f, 1.5f, 4f) * // Motorcycle proportions: wider than tall, longer than wide
                           Matrix.CreateRotationY(direction * MathHelper.PiOver2) *
                           Matrix.CreateTranslation(x, 1f, y); // Position on ground
                fx.World = world;
                
                // Get player color
                int colorIndex = p.getPlayerNum();
                if (p is GltronMobileEngine.Player concretePlayer)
                {
                    colorIndex = concretePlayer.getPlayerColorIndex();
                }
                fx.DiffuseColor = GetPlayerColor(colorIndex);
                fx.Alpha = 1.0f;
                
                // Disable texture for solid color rendering
                fx.TextureEnabled = false;
                fx.VertexColorEnabled = false;
                fx.LightingEnabled = false;
                
                // Draw a motorcycle-shaped cube
                DrawSimpleCube(fx);
                
                // Reset effect state
                fx.TextureEnabled = true;
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GLTRON: DrawBike error: {ex.Message}");
        }
    }
    
    private void DrawSimpleCube(BasicEffect fx)
    {
        // CRITICAL FIX: Draw a visible cube using BasicEffect's DiffuseColor
        // Use simple vertex positions without colors (let BasicEffect handle coloring)
        var vertices = new VertexPositionTexture[]
        {
            // Multiple faces for a more visible cube
            // Top face
            new VertexPositionTexture(new Vector3(-0.5f, 0.5f, -0.5f), Vector2.Zero),
            new VertexPositionTexture(new Vector3(0.5f, 0.5f, -0.5f), Vector2.UnitX),
            new VertexPositionTexture(new Vector3(-0.5f, 0.5f, 0.5f), Vector2.UnitY),
            new VertexPositionTexture(new Vector3(0.5f, 0.5f, 0.5f), Vector2.One),
            
            // Front face
            new VertexPositionTexture(new Vector3(-0.5f, -0.5f, 0.5f), Vector2.Zero),
            new VertexPositionTexture(new Vector3(0.5f, -0.5f, 0.5f), Vector2.UnitX),
            new VertexPositionTexture(new Vector3(-0.5f, 0.5f, 0.5f), Vector2.UnitY),
            new VertexPositionTexture(new Vector3(0.5f, 0.5f, 0.5f), Vector2.One),
        };
        
        foreach (var pass in fx.CurrentTechnique.Passes)
        {
            pass.Apply();
            
            using var vb = new VertexBuffer(_gd, typeof(VertexPositionTexture), vertices.Length, BufferUsage.WriteOnly);
            vb.SetData(vertices);
            _gd.SetVertexBuffer(vb);
            
            // Draw top face
            _gd.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            // Draw front face  
            _gd.DrawPrimitives(PrimitiveType.TriangleStrip, 4, 2);
        }
    }

    public void DrawExplosion(BasicEffect fx, IPlayer? p)
    {
        if (p == null || !p.getExplode()) return;
        
        try
        {
            float x = p.getXpos();
            float y = p.getYpos();
            
            // TODO: Implement explosion effect
            // For now, draw a bright colored effect at crash location
            var world = Matrix.CreateScale(3f) * Matrix.CreateTranslation(x, 3f, y);
            fx.World = world;
            fx.DiffuseColor = Vector3.One; // White explosion
            fx.EmissiveColor = Vector3.One * 0.5f; // Glowing effect
            
            foreach (var pass in fx.CurrentTechnique.Passes)
            {
                pass.Apply();
                // TODO: Draw actual explosion particles/effects here
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GLTRON: DrawExplosion error: {ex.Message}");
        }
    }
    
    private Vector3 GetPlayerColor(int playerNum)
    {
        // CRITICAL FIX: Match Java player colors exactly (from ColourDiffuse array)
        Vector3[] colors = {
            new Vector3(0.0f, 0.1f, 0.900f),      // Blue - Player 0 (human)
            new Vector3(1.00f, 0.550f, 0.140f),   // Orange/Yellow - Player 1 (AI)
            new Vector3(0.750f, 0.020f, 0.020f),  // Red - Player 2 (AI)
            new Vector3(0.800f, 0.800f, 0.800f),  // Grey - Player 3 (AI)
            new Vector3(0.120f, 0.750f, 0.0f),    // Green - Player 4 (AI)
            new Vector3(0.750f, 0.0f, 0.35f)      // Purple - Player 5 (AI)
        };
        
        var color = colors[playerNum % colors.Length];
        
        try
        {
#if ANDROID
            Android.Util.Log.Debug("GLTRON", $"Player {playerNum} color: R={color.X:F2} G={color.Y:F2} B={color.Z:F2}");
#endif
        }
        catch { }
        
        return color;
    }
    
    private void DrawObjModel(BasicEffect fx, SimpleObjLoader.SimpleObjModel model)
    {
        try
        {
            if (model.Vertices.Length == 0) return;
            
            foreach (var pass in fx.CurrentTechnique.Passes)
            {
                pass.Apply();
                
                using var vb = new VertexBuffer(_gd, typeof(VertexPositionNormalTexture), model.Vertices.Length, BufferUsage.WriteOnly);
                using var ib = new IndexBuffer(_gd, IndexElementSize.ThirtyTwoBits, model.Indices.Length, BufferUsage.WriteOnly);
                
                vb.SetData(model.Vertices);
                ib.SetData(model.Indices);
                
                _gd.SetVertexBuffer(vb);
                _gd.Indices = ib;
                _gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, model.TriangleCount);
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GLTRON: DrawObjModel error: {ex.Message}");
        }
    }

    public void DrawRecognizer(BasicEffect fx, Recognizer recognizer)
    {
        if (recognizer == null || _recognizerObjModel == null) return;
        
        try
        {
            // Set up recognizer rendering state
            _gd.BlendState = BlendState.AlphaBlend;
            _gd.DepthStencilState = DepthStencilState.Default;
            _gd.RasterizerState = RasterizerState.CullCounterClockwise;
            
            // Get recognizer transformation
            Matrix worldMatrix = recognizer.GetWorldMatrix(_recognizerBoundingBoxSize);
            fx.World = worldMatrix;
            
            // Set recognizer color
            Vector4 recognizerColor = recognizer.GetColor();
            fx.DiffuseColor = new Vector3(recognizerColor.X, recognizerColor.Y, recognizerColor.Z);
            fx.Alpha = recognizerColor.W;
            
            // Set up effect for model rendering
            fx.TextureEnabled = false;
            fx.VertexColorEnabled = false;
            fx.LightingEnabled = true;
            fx.EnableDefaultLighting();
            
            // Set special lighting for recognizer (like Java version)
            fx.EmissiveColor = new Vector3(0.1f, 0.05f, 0.05f); // Slight red glow
            fx.SpecularColor = new Vector3(recognizerColor.X, recognizerColor.Y, recognizerColor.Z);
            fx.SpecularPower = 16.0f;
            
            // Draw the recognizer model
            if (_recognizerObjModel != null)
            {
                DrawObjModel(fx, _recognizerObjModel);
            }
            
            // Draw shadow (simplified version - full stencil shadows would be complex in MonoGame)
            DrawRecognizerShadow(fx, recognizer);
            
            // Reset effect state
            fx.EmissiveColor = Vector3.Zero;
            fx.SpecularColor = Vector3.Zero;
            fx.TextureEnabled = true;
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GLTRON: DrawRecognizer error: {ex.Message}");
        }
    }
    
    private void DrawRecognizerShadow(BasicEffect fx, Recognizer recognizer)
    {
        try
        {
            // Simplified shadow - draw a darker version on the ground
            var prevBlend = _gd.BlendState;
            _gd.BlendState = BlendState.AlphaBlend;
            
            // Get shadow transformation (projected to ground)
            Vector3 position = recognizer.GetPosition(_recognizerBoundingBoxSize);
            float angle = recognizer.GetAngle();
            
            // Project shadow onto the ground (Y = 0)
            Matrix shadowWorld = Matrix.CreateScale(0.25f) *
                                Matrix.CreateRotationY(MathHelper.ToRadians(angle)) *
                                Matrix.CreateTranslation(position.X, 0.1f, position.Z); // Slightly above ground
            
            fx.World = shadowWorld;
            fx.DiffuseColor = Vector3.Zero; // Black shadow
            fx.Alpha = 0.3f; // Semi-transparent
            fx.LightingEnabled = false; // No lighting for shadow
            
            // Draw shadow
            if (_recognizerObjModel != null)
            {
                DrawObjModel(fx, _recognizerObjModel);
            }
            
            _gd.BlendState = prevBlend;
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GLTRON: DrawRecognizerShadow error: {ex.Message}");
        }
    }
    
    private Vector3 CalculateBoundingBoxSize(SimpleObjLoader.SimpleObjModel model)
    {
        if (model.Vertices.Length == 0)
            return Vector3.One;
        
        Vector3 min = new Vector3(float.MaxValue);
        Vector3 max = new Vector3(float.MinValue);
        
        foreach (var vertex in model.Vertices)
        {
            Vector3 pos = vertex.Position;
            min = Vector3.Min(min, pos);
            max = Vector3.Max(max, pos);
        }
        
        return max - min;
    }

    public void SetGridSize(float gridSize)
    {
        _gridSize = gridSize;
    }

    /// <summary>
    /// Helper method to draw a quad from 4 vertices
    /// </summary>
    private void DrawQuad(VertexPositionTexture[] verts)
    {
        if (verts.Length != 4) return;
        
        using var vb = new VertexBuffer(_gd, typeof(VertexPositionTexture), 4, BufferUsage.WriteOnly);
        vb.SetData(verts);
        _gd.SetVertexBuffer(vb);
        _gd.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
    }

    public void EndDraw() { }
}
