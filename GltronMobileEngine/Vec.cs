using System;

namespace GltronMobileEngine;

/// <summary>
/// Multiplatform 3D Vector class for GLTron Mobile
/// Compatible with Android, iOS, and other MonoGame platforms
/// </summary>
public class Vec
{
    public float[] v = new float[3];

    public Vec()
    {
        v[0] = 0.0f;
        v[1] = 0.0f;
        v[2] = 0.0f;
    }

    public Vec(float x, float y, float z = 0.0f)
    {
        v[0] = x;
        v[1] = y;
        v[2] = z;
    }

    public Vec(Vec other)
    {
        v[0] = other.v[0];
        v[1] = other.v[1];
        v[2] = other.v[2];
    }

    public float Length()
    {
        return (float)Math.Sqrt(v[0] * v[0] + v[1] * v[1] + v[2] * v[2]);
    }

    public void Normalise()
    {
        float len = Length();
        if (len > 0.0f)
        {
            v[0] /= len;
            v[1] /= len;
            v[2] /= len;
        }
    }

    public float Dot(Vec other)
    {
        return v[0] * other.v[0] + v[1] * other.v[1] + v[2] * other.v[2];
    }

    public Vec Sub(Vec other)
    {
        return new Vec(v[0] - other.v[0], v[1] - other.v[1], v[2] - other.v[2]);
    }

    public Vec Add(Vec other)
    {
        return new Vec(v[0] + other.v[0], v[1] + other.v[1], v[2] + other.v[2]);
    }

    public Vec Cross(Vec other)
    {
        return new Vec(
            v[1] * other.v[2] - v[2] * other.v[1],
            v[2] * other.v[0] - v[0] * other.v[2],
            v[0] * other.v[1] - v[1] * other.v[0]
        );
    }
}
