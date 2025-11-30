using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using GltronMobileEngine.Interfaces;

namespace GltronMobileEngine.Video;

public class WorldGraphics
{
    public BasicEffect Effect { get; private set; }
    private GraphicsDevice _gd;

    private VertexBuffer? _floorVB;
    private Texture2D? _texFloor;
    private Texture2D? _texWall;
    private Texture2D? _texSky;

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
        _texSky = content.Load<Texture2D>("Assets/skybox0");

        var verts = new VertexPositionTexture[4];
        float s = 1f;
        verts[0] = new VertexPositionTexture(new Vector3(-s, 0, -s), new Vector2(0, 0));
        verts[1] = new VertexPositionTexture(new Vector3(s, 0, -s), new Vector2(10, 0));
        verts[2] = new VertexPositionTexture(new Vector3(-s, 0, s), new Vector2(0, 10));
        verts[3] = new VertexPositionTexture(new Vector3(s, 0, s), new Vector2(10, 10));
        _floorVB = new VertexBuffer(_gd, typeof(VertexPositionTexture), 4, BufferUsage.WriteOnly);
        _floorVB.SetData(verts);
    }

    public void BeginDraw(Matrix view, Matrix proj)
    {
        Effect.View = view;
        Effect.Projection = proj;
        Effect.World = Matrix.Identity;
    }

    public void DrawFloor()
    {
        if (_texFloor == null || _floorVB == null) return;
        
        _gd.RasterizerState = RasterizerState.CullNone;
        _gd.DepthStencilState = DepthStencilState.Default;
        Effect.Texture = _texFloor;
        
        // Scale floor to match GLTron arena size (100x100 units)
        // But position it correctly at origin
        Effect.World = Matrix.CreateScale(50f, 1f, 50f) * Matrix.CreateTranslation(50f, 0f, 50f);
        
        foreach (var pass in Effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            _gd.SetVertexBuffer(_floorVB);
            _gd.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
        }
    }

    public void DrawWalls(ISegment[] walls)
    {
        if (walls == null || _texWall == null) return;
        
        // CRITICAL FIX: Reset world matrix to identity before drawing walls
        // The floor drawing sets a scale transform that must be cleared
        Effect.World = Matrix.Identity;
        Effect.Texture = _texWall;
        
        try
        {
#if ANDROID
            Android.Util.Log.Debug("GLTRON", $"Drawing {walls.Length} wall segments with world matrix reset");
#endif
        }
        catch { }
        
        foreach (var seg in walls)
        {
            var start = new Vector3(seg.vStart.v[0], 0, seg.vStart.v[1]);
            var end = new Vector3(seg.vStart.v[0] + seg.vDirection.v[0], 0, seg.vStart.v[1] + seg.vDirection.v[1]);
            
            try
            {
#if ANDROID
                Android.Util.Log.Debug("GLTRON", $"Wall segment: start=({start.X:F1},{start.Y:F1},{start.Z:F1}) end=({end.X:F1},{end.Y:F1},{end.Z:F1})");
#endif
            }
            catch { }
            
            DrawWallSegment(start, end, 8f);
        }
    }

    private void DrawWallSegment(Vector3 a, Vector3 b, float height)
    {
        // Calculate wall segment geometry
        var dir = b - a;
        var len = dir.Length();
        if (len <= 0.0001f) return; // Skip degenerate segments
        
        dir.Normalize();
        var up = Vector3.Up * height;

        var v0 = a;
        var v1 = a + up;
        var v2 = b;
        var v3 = b + up;

        var verts = new VertexPositionTexture[4];
        verts[0] = new VertexPositionTexture(v0, new Vector2(0, 1));
        verts[1] = new VertexPositionTexture(v1, new Vector2(0, 0));
        verts[2] = new VertexPositionTexture(v2, new Vector2(len * 0.05f, 1));
        verts[3] = new VertexPositionTexture(v3, new Vector2(len * 0.05f, 0));

        // PERFORMANCE FIX: Use DrawUserPrimitives instead of creating VB per frame
        // Also ensure world matrix is identity for walls
        foreach (var pass in Effect.CurrentTechnique.Passes)
        {
            Effect.World = Matrix.Identity; // Ensure no scale transform from floor
            pass.Apply();
            _gd.DrawUserPrimitives(PrimitiveType.TriangleStrip, verts, 0, 2);
        }
    }

    public void DrawSkybox()
    {
        // Placeholder: could draw a cube with sky textures later.
    }

    public void DrawBike(BasicEffect fx, IPlayer p)
    {
        // Placeholder: until model loader exists, skip
    }

    public void DrawExplosion(BasicEffect fx, IPlayer p)
    {
        // Placeholder: draw translucent ring later
    }

    public void EndDraw() { }
}
