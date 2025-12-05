using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GltronMobileEngine.Interfaces;

namespace GltronMobileEngine.Video;

public class TrailsRenderer
{
    private readonly GraphicsDevice _gd;
    // We will use WorldGraphics.Effect directly to guarantee identical matrices/states
    public bool TrailsIgnoreDepth { get; set; } = false; // use DepthRead for correct depth interaction

    public TrailsRenderer(GraphicsDevice gd)
    {
        _gd = gd;
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
        
        // CRITICAL FIX: Show trails even when height is very low (fading trails) - improved threshold
        if (trailOffset <= 0 || trailHeight <= 0.001f) return;
        
        // CRITICAL FIX: Draw trail walls correctly - each segment is a wall from start to start+direction
        var verts = new List<VertexPositionColor>();
        // minimum wall half-thickness to avoid degenerate long-thin triangles
        const float halfWidth = 0.02f;
        
        // Get player color (like Java version)
        Color trailColor = GetPlayerTrailColor(p);
        
        System.Diagnostics.Debug.WriteLine($"GLTRON: Drawing trail for player {p.getPlayerNum()}, offset: {trailOffset}, height: {trailHeight:F2}");
        
        // CRITICAL FIX: Build trail segments properly - each segment represents a wall
        int validSegments = 0;
        // Stitch small gaps by ensuring consecutive segments connect exactly
        Vector3? lastEnd = null;
        const float epsilon = 0.001f;
        const float snapTolerance = 0.10f;
        for (int i = 0; i <= trailOffset; i++)
        {
            var segment = p.getTrail(i);
            if (segment == null) continue;
            
            // Each segment goes from vStart to vStart + vDirection
            var segStart = new Vector3(segment.vStart.v[0], 0f, segment.vStart.v[1]);
            var segEnd = new Vector3(segment.vStart.v[0] + segment.vDirection.v[0], 0f, 
                                   segment.vStart.v[1] + segment.vDirection.v[1]);
            
            // If previous end exists and is extremely close but not exactly equal, snap start to previous end
            if (lastEnd.HasValue && Vector3.Distance(lastEnd.Value, segStart) < snapTolerance)
            {
                segStart = lastEnd.Value;
            }
            
            // If the segment is nearly zero-length, still keep continuity by extending a tiny epsilon towards its nominal end
            float segLength = Vector3.Distance(segStart, segEnd);
            if (segLength < epsilon)
            {
                // Try to get direction from vDirection; normalise and extend minimally
                var dir = new Vector3(segment.vDirection.v[0], 0f, segment.vDirection.v[1]);
                if (dir.LengthSquared() > 0f)
                {
                    dir.Normalize();
                    segEnd = segStart + dir * 0.02f; // tiny visible stub to keep continuity
                    segLength = 0.02f;
                }
                else
                {
                    // No direction yet; maintain stitching reference to current start
                    lastEnd = segStart;
                    continue;
                }
            }
            
            validSegments++;
            System.Diagnostics.Debug.WriteLine($"GLTRON: Trail segment {i}: ({segStart.X:F1},{segStart.Z:F1}) to ({segEnd.X:F1},{segEnd.Z:F1}), length: {segLength:F2}");
            
            // Build wall quad with a tiny width: offset to both sides using a perpendicular in XZ plane
            var axis = segEnd - segStart; // in XZ plane
            float axisLenSq = axis.LengthSquared();
            Vector3 perp;
            if (axisLenSq > 0.0004f) // ~0.02^2
            {
                perp = new Vector3(-axis.Z, 0f, axis.X);
                perp.Normalize();
            }
            else
            {
                // Very short segment: use a stable perpendicular
                perp = Vector3.UnitX;
            }
            perp *= halfWidth;

            float h = Math.Max(trailHeight, 0.1f); // ensure visible height even when fading

            // Lift slightly above floor to avoid z-fighting - improved for camera proximity
            const float yLift = 0.002f;
            segStart.Y += yLift;
            segEnd.Y += yLift;

            var bottomStartL = segStart - perp;
            var bottomStartR = segStart + perp;
            var bottomEndL = segEnd - perp;
            var bottomEndR = segEnd + perp;

            var topStartL = bottomStartL + new Vector3(0, h, 0);
            var topStartR = bottomStartR + new Vector3(0, h, 0);
            var topEndL = bottomEndL + new Vector3(0, h, 0);
            var topEndR = bottomEndR + new Vector3(0, h, 0);

            // Two quads (left and right faces) to form a thin wall slab
            // Face 1
            verts.Add(new VertexPositionColor(bottomStartL, trailColor));
            verts.Add(new VertexPositionColor(topStartL, trailColor));
            verts.Add(new VertexPositionColor(bottomEndL, trailColor));

            verts.Add(new VertexPositionColor(topStartL, trailColor));
            verts.Add(new VertexPositionColor(topEndL, trailColor));
            verts.Add(new VertexPositionColor(bottomEndL, trailColor));

            // Face 2
            verts.Add(new VertexPositionColor(bottomStartR, trailColor));
            verts.Add(new VertexPositionColor(topStartR, trailColor));
            verts.Add(new VertexPositionColor(bottomEndR, trailColor));

            verts.Add(new VertexPositionColor(topStartR, trailColor));
            verts.Add(new VertexPositionColor(topEndR, trailColor));
            verts.Add(new VertexPositionColor(bottomEndR, trailColor));

            lastEnd = segEnd;
        }
        
        System.Diagnostics.Debug.WriteLine($"GLTRON: Generated {validSegments} valid trail segments, {verts.Count} vertices for player {p.getPlayerNum()}");
        
        if (verts.Count == 0) 
        {
            System.Diagnostics.Debug.WriteLine($"GLTRON: No trail vertices generated for player {p.getPlayerNum()}");
            return;
        }
        
        System.Diagnostics.Debug.WriteLine($"GLTRON: Generated {verts.Count} trail vertices for player {p.getPlayerNum()}");
        
        // Save device and effect state
        var prevBlend = _gd.BlendState;
        var prevDepth = _gd.DepthStencilState;
        var prevRaster = _gd.RasterizerState;

        bool prevVC = world.Effect.VertexColorEnabled;
        bool prevTex = world.Effect.TextureEnabled;
        bool prevLight = world.Effect.LightingEnabled;
        Vector3 prevDiffuse = world.Effect.DiffuseColor;
        float prevAlpha = world.Effect.Alpha;

        // Configure states
        _gd.BlendState = BlendState.AlphaBlend;
        _gd.DepthStencilState = TrailsIgnoreDepth ? DepthStencilState.None : DepthStencilState.DepthRead;
        _gd.RasterizerState = RasterizerState.CullNone; // draw both sides

        // Use world.Effect directly to guarantee matching matrices
        world.Effect.VertexColorEnabled = true;
        world.Effect.TextureEnabled = false;
        world.Effect.LightingEnabled = false;
        world.Effect.World = Matrix.Identity;
        world.Effect.DiffuseColor = Vector3.One;
        world.Effect.Alpha = 1.0f;

        try
        {
            var data = verts.ToArray();
            foreach (var pass in world.Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                _gd.DrawUserPrimitives(PrimitiveType.TriangleList, data, 0, data.Length / 3);
            }
            
            System.Diagnostics.Debug.WriteLine($"GLTRON: Successfully rendered {verts.Count / 3} trail triangles for player {p.getPlayerNum()}");
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GLTRON: Trail rendering error for player {p.getPlayerNum()}: {ex.Message}");
        }
        
        // Restore effect and device state
        world.Effect.VertexColorEnabled = prevVC;
        world.Effect.TextureEnabled = prevTex;
        world.Effect.LightingEnabled = prevLight;
        world.Effect.DiffuseColor = prevDiffuse;
        world.Effect.Alpha = prevAlpha;

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
