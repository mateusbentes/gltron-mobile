using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GltronMobileEngine.Interfaces;

namespace GltronMobileEngine.Video;

public class TrailsRenderer
{
    private readonly GraphicsDevice _gd;
    private DynamicVertexBuffer _vb;

    public TrailsRenderer(GraphicsDevice gd)
    {
        _gd = gd;
        _vb = new DynamicVertexBuffer(_gd, typeof(VertexPositionColor), 8192, BufferUsage.WriteOnly);
    }

    public void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
    {
        // No content needed yet
    }

    public void DrawTrail(WorldGraphics world, IPlayer p)
    {
        if (p == null || p.getTrailHeight() <= 0.0f) return;
        
        // CRITICAL FIX: Draw trail walls like Java version, not just lines
        var verts = new List<VertexPositionColor>();
        
        // Get player color (like Java version)
        Color trailColor = GetPlayerTrailColor(p);
        float trailHeight = p.getTrailHeight();
        
        for (int i = 0; i <= p.getTrailOffset(); i++)
        {
            var s = p.getTrail(i);
            if (s == null) continue;
            
            // Create trail segment as a wall (like Java TrailMesh)
            var startPos = new Vector3(s.vStart.v[0], 0f, s.vStart.v[1]);
            var endPos = startPos + new Vector3(s.vDirection.v[0], 0f, s.vDirection.v[1]);
            
            // Skip zero-length segments
            if (Vector3.Distance(startPos, endPos) < 0.1f) continue;
            
            // Create wall quad for this trail segment
            var topStart = startPos + new Vector3(0, trailHeight, 0);
            var topEnd = endPos + new Vector3(0, trailHeight, 0);
            
            // Add quad vertices (two triangles)
            // Triangle 1
            verts.Add(new VertexPositionColor(startPos, trailColor));    // bottom start
            verts.Add(new VertexPositionColor(topStart, trailColor));    // top start  
            verts.Add(new VertexPositionColor(endPos, trailColor));      // bottom end
            
            // Triangle 2
            verts.Add(new VertexPositionColor(topStart, trailColor));    // top start
            verts.Add(new VertexPositionColor(topEnd, trailColor));      // top end
            verts.Add(new VertexPositionColor(endPos, trailColor));      // bottom end
        }
        
        if (verts.Count == 0) return;
        
        // Ensure buffer is large enough
        if (_vb.VertexCount < verts.Count)
        {
            _vb.Dispose();
            _vb = new DynamicVertexBuffer(_gd, typeof(VertexPositionColor), verts.Count * 2, BufferUsage.WriteOnly);
        }
        
        // Set up rendering state for trail walls
        world.Effect.VertexColorEnabled = true;
        world.Effect.TextureEnabled = false;
        world.Effect.LightingEnabled = false;
        world.Effect.World = Matrix.Identity;
        
        _vb.SetData(verts.ToArray());
        _gd.SetVertexBuffer(_vb);
        
        foreach (var pass in world.Effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            _gd.DrawPrimitives(PrimitiveType.TriangleList, 0, verts.Count / 3);
        }
        
        // Reset effect state
        world.Effect.VertexColorEnabled = false;
        world.Effect.TextureEnabled = true;
    }
    
    private Color GetPlayerTrailColor(IPlayer p)
    {
        // CRITICAL FIX: Match Java player trail colors exactly
        Color[] trailColors = {
            new Color(0.0f, 0.1f, 0.900f, 0.8f),      // Blue - Player 0 (human)
            new Color(1.00f, 0.550f, 0.140f, 0.8f),   // Orange - Player 1 (AI)
            new Color(0.750f, 0.020f, 0.020f, 0.8f),  // Red - Player 2 (AI)
            new Color(0.800f, 0.800f, 0.800f, 0.8f),  // Grey - Player 3 (AI)
            new Color(0.120f, 0.750f, 0.0f, 0.8f),    // Green - Player 4 (AI)
            new Color(0.750f, 0.0f, 0.35f, 0.8f)      // Purple - Player 5 (AI)
        };
        
        int colorIndex = p.getPlayerNum();
        if (p is GltronMobileEngine.Player concretePlayer)
        {
            colorIndex = concretePlayer.getPlayerColorIndex();
        }
        
        return trailColors[colorIndex % trailColors.Length];
    }
}
