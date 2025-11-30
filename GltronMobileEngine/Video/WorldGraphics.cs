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

        // CRITICAL FIX: Use CullNone to match Java OpenGL behavior
        _gd.RasterizerState = RasterizerState.CullNone;
        _gd.BlendState = BlendState.AlphaBlend;
        _gd.DepthStencilState = DepthStencilState.Default;

        float h = 48.0f;
        float s = _gridSize;

        var quads = new (Vector3 a, Vector3 b, Vector3 c, Vector3 d)[]
        {
            (new Vector3(0, h, 0), new Vector3(s, h, 0), new Vector3(0, 0, 0), new Vector3(s, 0, 0)),
            (new Vector3(s, h, 0), new Vector3(s, h, s), new Vector3(s, 0, 0), new Vector3(s, 0, s)),
            (new Vector3(s, h, s), new Vector3(0, h, s), new Vector3(s, 0, s), new Vector3(0, 0, s)),
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

        // CRITICAL FIX: Center skybox around grid center, not origin
        float d = _gridSize * 3f;
        float centerX = _gridSize / 2f;
        float centerZ = _gridSize / 2f;

        var faces = new Vector3[][]
        {
            // Right face
            new [] { new Vector3(centerX+d,d,-d+centerZ), new Vector3(centerX+d,-d,-d+centerZ), new Vector3(centerX+d,-d,d+centerZ), new Vector3(centerX+d,d,d+centerZ) },
            // Top face  
            new [] { new Vector3(centerX+d,d,d+centerZ), new Vector3(centerX-d,d,d+centerZ), new Vector3(centerX-d,-d,d+centerZ), new Vector3(centerX+d,-d,d+centerZ) },
            // Left face
            new [] { new Vector3(centerX-d,d,-d+centerZ), new Vector3(centerX+d,d,-d+centerZ), new Vector3(centerX+d,d,d+centerZ), new Vector3(centerX-d,d,d+centerZ) },
            // Bottom face
            new [] { new Vector3(centerX+d,-d,-d+centerZ), new Vector3(centerX-d,-d,-d+centerZ), new Vector3(centerX-d,-d,d+centerZ), new Vector3(centerX+d,-d,d+centerZ) },
            // Back face
            new [] { new Vector3(centerX-d,d,-d+centerZ), new Vector3(centerX-d,-d,-d+centerZ), new Vector3(centerX+d,-d,-d+centerZ), new Vector3(centerX+d,d,-d+centerZ) },
            // Front face
            new [] { new Vector3(centerX-d,-d,-d+centerZ), new Vector3(centerX-d,d,-d+centerZ), new Vector3(centerX-d,d,d+centerZ), new Vector3(centerX-d,-d,d+centerZ) },
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
        
        // TODO: Load and draw light cycle model
        // For now, draw a simple colored cube to represent the bike
        try
        {
            float x = p.getXpos();
            float y = p.getYpos();
            int direction = p.getDirection();
            
            // Create a simple bike representation using a colored cube
            var world = Matrix.CreateTranslation(x, 2f, y); // Slightly above ground
            fx.World = world;
            fx.DiffuseColor = GetPlayerColor(p.getPlayerNum());
            
            // Apply effect and draw a simple cube (placeholder until model loading is implemented)
            foreach (var pass in fx.CurrentTechnique.Passes)
            {
                pass.Apply();
                // TODO: Draw actual light cycle model here
                // For now, this is a placeholder that shows bike position
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GLTRON: DrawBike error: {ex.Message}");
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
        // Match Java player colors
        Vector3[] colors = {
            new Vector3(0.0f, 0.1f, 0.9f),    // Blue
            new Vector3(1.0f, 0.55f, 0.14f),  // Orange/Yellow  
            new Vector3(0.75f, 0.02f, 0.02f), // Red
            new Vector3(0.8f, 0.8f, 0.8f),    // Grey
            new Vector3(0.12f, 0.75f, 0.0f),  // Green
            new Vector3(0.75f, 0.0f, 0.35f)   // Purple
        };
        
        return colors[playerNum % colors.Length];
    }

    public void SetGridSize(float gridSize)
    {
        _gridSize = gridSize;
    }

    public void EndDraw() { }
}
