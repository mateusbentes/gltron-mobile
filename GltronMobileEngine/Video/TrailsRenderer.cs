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
        if (p == null) return;
        
        int trailOffset = p.getTrailOffset();
        float trailHeight = p.getTrailHeight();
        
        // CRITICAL FIX: Show trails even when height is low (fading trails)
        if (trailOffset <= 0 || trailHeight <= 0.01f) return;
        
        // CRITICAL FIX: Draw trail walls correctly - each segment is a wall from start to start+direction
        var verts = new List<VertexPositionColor>();
        
        // Get player color (like Java version)
        Color trailColor = GetPlayerTrailColor(p);
        
        System.Diagnostics.Debug.WriteLine($"GLTRON: Drawing trail for player {p.getPlayerNum()}, offset: {trailOffset}, height: {trailHeight:F2}");
        
        // CRITICAL FIX: Build trail segments properly - each segment represents a wall
        int validSegments = 0;
        for (int i = 0; i <= trailOffset; i++)
        {
            var segment = p.getTrail(i);
            if (segment == null) continue;
            
            // Each segment goes from vStart to vStart + vDirection
            var segStart = new Vector3(segment.vStart.v[0], 0f, segment.vStart.v[1]);
            var segEnd = new Vector3(segment.vStart.v[0] + segment.vDirection.v[0], 0f, 
                                   segment.vStart.v[1] + segment.vDirection.v[1]);
            
            // CRITICAL FIX: Accept smaller segments but ensure they're not zero
            float segLength = Vector3.Distance(segStart, segEnd);
            if (segLength < 0.001f) continue;
            
            validSegments++;
            System.Diagnostics.Debug.WriteLine($"GLTRON: Trail segment {i}: ({segStart.X:F1},{segStart.Z:F1}) to ({segEnd.X:F1},{segEnd.Z:F1}), length: {segLength:F2}");
            
            // CRITICAL FIX: Create simple trail wall (no duplication)
            var bottomStart = segStart;
            var bottomEnd = segEnd;
            var topStart = segStart + new Vector3(0, trailHeight, 0);
            var topEnd = segEnd + new Vector3(0, trailHeight, 0);
            
            // Create single trail wall quad (visible from both sides due to CullNone)
            // Triangle 1: bottom-start, top-start, bottom-end
            verts.Add(new VertexPositionColor(bottomStart, trailColor));
            verts.Add(new VertexPositionColor(topStart, trailColor));
            verts.Add(new VertexPositionColor(bottomEnd, trailColor));
            
            // Triangle 2: top-start, top-end, bottom-end  
            verts.Add(new VertexPositionColor(topStart, trailColor));
            verts.Add(new VertexPositionColor(topEnd, trailColor));
            verts.Add(new VertexPositionColor(bottomEnd, trailColor));
        }
        
        System.Diagnostics.Debug.WriteLine($"GLTRON: Generated {validSegments} valid trail segments, {verts.Count} vertices for player {p.getPlayerNum()}");
        
        if (verts.Count == 0) 
        {
            System.Diagnostics.Debug.WriteLine($"GLTRON: No trail vertices generated for player {p.getPlayerNum()}");
            return;
        }
        
        System.Diagnostics.Debug.WriteLine($"GLTRON: Generated {verts.Count} trail vertices for player {p.getPlayerNum()}");
        
        // Ensure buffer is large enough
        if (_vb.VertexCount < verts.Count)
        {
            _vb.Dispose();
            _vb = new DynamicVertexBuffer(_gd, typeof(VertexPositionColor), Math.Max(verts.Count * 2, 8192), BufferUsage.WriteOnly);
        }
        
        // CRITICAL FIX: Set up proper rendering state for trail walls
        var prevBlend = _gd.BlendState;
        var prevDepth = _gd.DepthStencilState;
        var prevRaster = _gd.RasterizerState;
        
        // CRITICAL FIX: Use proper blend state for semi-transparent trails
        _gd.BlendState = BlendState.AlphaBlend;
        _gd.DepthStencilState = DepthStencilState.Default;
        _gd.RasterizerState = RasterizerState.CullNone; // Draw both sides
        
        // CRITICAL FIX: Configure effect properly for vertex colors
        world.Effect.VertexColorEnabled = true;
        world.Effect.TextureEnabled = false;
        world.Effect.LightingEnabled = false;
        world.Effect.World = Matrix.Identity;
        world.Effect.DiffuseColor = Vector3.One; // Ensure white diffuse for proper vertex colors
        world.Effect.Alpha = 1.0f;
        
        try
        {
            _vb.SetData(verts.ToArray());
            _gd.SetVertexBuffer(_vb);
            
            foreach (var pass in world.Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                _gd.DrawPrimitives(PrimitiveType.TriangleList, 0, verts.Count / 3);
            }
            
            System.Diagnostics.Debug.WriteLine($"GLTRON: Successfully rendered {verts.Count / 3} trail triangles for player {p.getPlayerNum()}");
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GLTRON: Trail rendering error for player {p.getPlayerNum()}: {ex.Message}");
        }
        
        // Reset effect state
        world.Effect.VertexColorEnabled = false;
        world.Effect.TextureEnabled = true;
        _gd.BlendState = prevBlend;
        _gd.DepthStencilState = prevDepth;
        _gd.RasterizerState = prevRaster;
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
