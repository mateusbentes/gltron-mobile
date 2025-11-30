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
        
        // CRITICAL FIX: Load motorcycle model using custom OBJ loader
        try
        {
            // Try to load the OBJ file directly
            string objPath = Path.Combine(content.RootDirectory, "Assets", "lightcyclehigh.obj");
            _lightCycleObjModel = SimpleObjLoader.LoadFromFile(objPath);
            
            if (_lightCycleObjModel != null)
            {
                System.Diagnostics.Debug.WriteLine($"GLTRON: Motorcycle OBJ model loaded successfully - {_lightCycleObjModel.Vertices.Length} vertices, {_lightCycleObjModel.TriangleCount} triangles");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("GLTRON: Could not load motorcycle OBJ model - using cube representation");
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GLTRON: Could not load motorcycle model: {ex.Message}");
        }
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
        _gd.RasterizerState = RasterizerState.CullNone; // Match Java OpenGL behavior
        Effect.Texture = _texFloor;
        Effect.World = Matrix.Identity;
        
        // CRITICAL FIX: Floor should be neutral colored, not blue
        Effect.DiffuseColor = Vector3.One; // White/neutral color for floor
        Effect.Alpha = 1.0f;

        // CRITICAL FIX: Match Java floor rendering exactly
        int l = (int)(_gridSize / 4f);
        if (l <= 0) l = 25;
        float t = l / 12f;

        foreach (var pass in Effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            for (int i = 0; i < (int)_gridSize; i += l)
            {
                for (int j = 0; j < (int)_gridSize; j += l)
                {
                    // Match Java floor quad generation
                    var verts = new VertexPositionTexture[4];
                    verts[0] = new VertexPositionTexture(new Vector3(i, 0, j), new Vector2(0f, 0f));
                    verts[1] = new VertexPositionTexture(new Vector3(i + l, 0, j), new Vector2(t, 0f));
                    verts[2] = new VertexPositionTexture(new Vector3(i, 0, j + l), new Vector2(0f, t));
                    verts[3] = new VertexPositionTexture(new Vector3(i + l, 0, j + l), new Vector2(t, t));

                    using var vb = new VertexBuffer(_gd, typeof(VertexPositionTexture), 4, BufferUsage.WriteOnly);
                    vb.SetData(verts);
                    _gd.SetVertexBuffer(vb);
                    _gd.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
                }
            }
        }
    }

    public void DrawWalls(ISegment[] walls)
    {
        if (_texWall == null) return;

        Effect.World = Matrix.Identity;
        Effect.Texture = _texWall;
        
        // CRITICAL FIX: Walls should be white/neutral colored, not player colored
        Effect.DiffuseColor = Vector3.One; // White color for walls
        Effect.Alpha = 1.0f;

        // CRITICAL FIX: Use CullNone to match Java OpenGL behavior
        _gd.RasterizerState = RasterizerState.CullNone;
        _gd.BlendState = BlendState.AlphaBlend;
        _gd.DepthStencilState = DepthStencilState.Default;

        // CRITICAL FIX: Match Java wall geometry exactly
        float h = 48.0f; // Wall height from Java
        float s = _gridSize;

        // Java wall vertices (converted to MonoGame coordinate system)
        var quads = new (Vector3 a, Vector3 b, Vector3 c, Vector3 d)[]
        {
            // Wall 1: Bottom wall (Y=0)
            (new Vector3(0, h, 0), new Vector3(s, h, 0), new Vector3(0, 0, 0), new Vector3(s, 0, 0)),
            // Wall 2: Right wall (X=gridSize)  
            (new Vector3(s, h, 0), new Vector3(s, h, s), new Vector3(s, 0, 0), new Vector3(s, 0, s)),
            // Wall 3: Top wall (Y=gridSize)
            (new Vector3(s, h, s), new Vector3(0, h, s), new Vector3(s, 0, s), new Vector3(0, 0, s)),
            // Wall 4: Left wall (X=0)
            (new Vector3(0, h, s), new Vector3(0, h, 0), new Vector3(0, 0, s), new Vector3(0, 0, 0)),
        };

        float t = _gridSize / 240f;
        var uvs = new Vector2[]
        {
            new Vector2(t, 1f), new Vector2(0f, 1f), new Vector2(t, 0f), new Vector2(0f, 0f)
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

                using var vb = new VertexBuffer(_gd, typeof(VertexPositionTexture), 4, BufferUsage.WriteOnly);
                vb.SetData(verts);
                _gd.SetVertexBuffer(vb);
                _gd.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
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
                // Fallback: Draw a better-looking bike representation
                var world = Matrix.CreateScale(4f, 3f, 8f) * // Even bigger for visibility
                           Matrix.CreateRotationY(direction * MathHelper.PiOver2) *
                           Matrix.CreateTranslation(x, 2f, y); // Position on ground
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
                
                // Draw a simple but visible cube
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

    public void SetGridSize(float gridSize)
    {
        _gridSize = gridSize;
    }

    public void EndDraw() { }
}
