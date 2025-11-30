/*
 * Copyright © 2012 Iain Churcher
 *
 * Based on GLtron by Andreas Umbach (www.gltron.org)
 *
 * This file is part of GL TRON.
 *
 * GL TRON is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * GL TRON is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with GL TRON.  If not, see <http://www.gnu.org/licenses/>.
 *
 */

using System;
using Microsoft.Xna.Framework;
using GltronMobileEngine.Video;

namespace GltronMobileEngine
{
    /// <summary>
    /// The Recognizer - a floating, animated 3D object that moves around the arena
    /// Ported from the original Java GLTron implementation
    /// </summary>
    public class Recognizer
    {
        // Private fields
        private float _alpha;
        private readonly float _gridSize;
        
        // Constants from Java version
        private readonly float[] _xv = { 0.5f, 0.3245f, 0.6f, 0.5f, 0.68f, -0.3f };
        private readonly float[] _yv = { 0.8f, 1.0f, 0.0f, 0.2f, 0.2f, 0.0f };
        private readonly Vector4 _colour = new Vector4(0.2f, 0.8f, 1.0f, 0.85f); // Bright cyan/blue - very alien
        
        // Shadow matrix for shadow rendering
        private readonly Matrix _shadowMatrix = new Matrix(
            4.0f, 0.0f, 0.0f, 0.0f,
            0.0f, 4.0f, 0.0f, 0.0f,
            -2.0f, -2.0f, 0.0f, 0.0f,
            0.0f, 0.0f, 0.0f, 4.0f
        );
        
        private const float SCALE_FACTOR = 1.0f;  // Increased from 0.4f to make recognizer more visible
        private const float HEIGHT = 25.0f;      // Lower but still floating
        
        public Recognizer(float gridSize)
        {
            _alpha = 0.0f;
            _gridSize = gridSize;
        }
        
        /// <summary>
        /// Update the recognizer's animation based on elapsed time
        /// </summary>
        /// <param name="deltaTime">Time elapsed in milliseconds</param>
        public void DoMovement(long deltaTime)
        {
            _alpha += deltaTime / 2000.0f;
        }
        
        /// <summary>
        /// Reset the recognizer animation
        /// </summary>
        public void Reset()
        {
            _alpha = 0.0f;
        }
        
        /// <summary>
        /// Get the current position of the recognizer
        /// </summary>
        /// <param name="modelBoundingBoxSize">Size of the recognizer model's bounding box</param>
        /// <returns>Current position as Vector3</returns>
        public Vector3 GetPosition(Vector3 modelBoundingBoxSize)
        {
            float max = modelBoundingBoxSize.X * SCALE_FACTOR;
            float boundary = _gridSize - max;
            
            float x = (max + (GetX() + 1.0f) * boundary) / 2.0f;
            float y = (max + (GetY() + 1.0f) * boundary) / 2.0f;
            
            return new Vector3(x, HEIGHT, y); // Note: Y is height in MonoGame coordinate system
        }
        
        /// <summary>
        /// Get the current velocity of the recognizer
        /// </summary>
        /// <returns>Current velocity as Vector3</returns>
        public Vector3 GetVelocity()
        {
            return new Vector3(GetDX() * _gridSize / 100.0f, 0.0f, GetDY() * _gridSize / 100.0f);
        }
        
        /// <summary>
        /// Get the current rotation angle of the recognizer
        /// </summary>
        /// <returns>Rotation angle in degrees</returns>
        public float GetAngle()
        {
            Vector3 velocity = GetVelocity();
            float dxval = velocity.X;
            float dyval = velocity.Z; // Note: Z is the Y equivalent in MonoGame
            
            float phi = (float)Math.Acos(dxval / Math.Sqrt(dxval * dxval + dyval * dyval));
            
            if (dyval < 0.0f)
                phi = (float)(2.0f * Math.PI - phi);
            
            return (float)((phi + Math.PI / 2.0f) * 180.0f / Math.PI);
        }
        
        /// <summary>
        /// Get the world transformation matrix for the recognizer
        /// </summary>
        /// <param name="modelBoundingBoxSize">Size of the recognizer model's bounding box</param>
        /// <returns>World transformation matrix</returns>
        public Matrix GetWorldMatrix(Vector3 modelBoundingBoxSize)
        {
            Vector3 position = GetPosition(modelBoundingBoxSize);
            float angle = GetAngle();
            
            return Matrix.CreateScale(SCALE_FACTOR) *
                   Matrix.CreateRotationY(MathHelper.ToRadians(angle)) *
                   Matrix.CreateTranslation(position);
        }
        
        /// <summary>
        /// Get the shadow transformation matrix
        /// </summary>
        /// <param name="modelBoundingBoxSize">Size of the recognizer model's bounding box</param>
        /// <returns>Shadow transformation matrix</returns>
        public Matrix GetShadowMatrix(Vector3 modelBoundingBoxSize)
        {
            Vector3 position = GetPosition(modelBoundingBoxSize);
            float angle = GetAngle();
            
            return _shadowMatrix *
                   Matrix.CreateScale(SCALE_FACTOR) *
                   Matrix.CreateRotationY(MathHelper.ToRadians(angle)) *
                   Matrix.CreateTranslation(position);
        }
        
        /// <summary>
        /// Get the recognizer's color
        /// </summary>
        /// <returns>Color as Vector4</returns>
        public Vector4 GetColor()
        {
            return _colour;
        }
        
        // Private animation calculation methods (ported from Java)
        private float GetX()
        {
            return (_xv[0] * (float)Math.Sin(_xv[1] * _alpha + _xv[2]) - 
                    _xv[3] * (float)Math.Sin(_xv[4] * _alpha + _xv[5]));
        }
        
        private float GetY()
        {
            return (_yv[0] * (float)Math.Cos(_yv[1] * _alpha + _yv[2] - 
                    _yv[3] * (float)Math.Sin(_yv[4] * _alpha + _yv[5])));
        }
        
        private float GetDX()
        {
            return (_xv[1] * _xv[0] * (float)Math.Cos(_xv[1] * _alpha + _xv[2]) - 
                    _xv[4] * _xv[3] * (float)Math.Cos(_xv[4] * _alpha + _xv[5]));
        }
        
        private float GetDY()
        {
            return -(_yv[1] * _yv[0] * (float)Math.Sin(_yv[1] * _alpha + _yv[2]) - 
                     _yv[4] * _yv[3] * (float)Math.Sin(_yv[4] * _alpha + _yv[5]));
        }
    }
}
