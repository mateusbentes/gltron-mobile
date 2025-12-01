using System;

namespace GltronMobileEngine;

/// <summary>
/// Multiplatform Segment class for GLTron Mobile
/// Compatible with Android, iOS, and other MonoGame platforms
/// </summary>
public class Segment : Interfaces.ISegment
{
    public Vec vStart { get; set; } = new Vec();
    public Vec vDirection { get; set; } = new Vec();
    public float t1 = 0.0f;
    public float t2 = 0.0f;

    public Segment() { }

    public Segment(Vec start, Vec direction)
    {
        vStart = start;
        vDirection = direction;
    }

    public Vec? Intersect(Segment other)
    {
        if (other == null || other.vDirection == null || other.vStart == null ||
            vDirection == null || vStart == null)
        {
            return null;
        }

        Vec v1 = vDirection;
        Vec v2 = other.vDirection;
        Vec v3 = other.vStart.Sub(vStart);

        if (v3 == null) return null;

        float cross = v1.v[0] * v2.v[1] - v1.v[1] * v2.v[0];

        // Use a more robust epsilon for floating point comparison
        if (Math.Abs(cross) < 1e-6f)
        {
            // Parallel or collinear
            return null;
        }

        try
        {
            t1 = (v3.v[0] * v2.v[1] - v3.v[1] * v2.v[0]) / cross;
            t2 = (v3.v[0] * v1.v[1] - v3.v[1] * v1.v[0]) / cross;

            // Check for valid intersection parameters
            if (!float.IsNaN(t1) && !float.IsNaN(t2) && 
                !float.IsInfinity(t1) && !float.IsInfinity(t2))
            {
                // CRITICAL FIX: Let Player.cs decide the exact criteria (< vs <=)
                // Return intersection if parameters are in valid range [0.0f, 1.0f]
                if (t1 >= 0.0f && t1 <= 1.0f && t2 >= 0.0f && t2 <= 1.0f)
                {
                    // Intersection point
                    float intersectX = vStart.v[0] + t1 * v1.v[0];
                    float intersectY = vStart.v[1] + t1 * v1.v[1];
                    
                    if (!float.IsNaN(intersectX) && !float.IsNaN(intersectY) &&
                        !float.IsInfinity(intersectX) && !float.IsInfinity(intersectY))
                    {
                        return new Vec(intersectX, intersectY);
                    }
                }
            }
        }
        catch (System.Exception)
        {
            // Return null on any arithmetic exception
            return null;
        }

        return null;
    }
}
