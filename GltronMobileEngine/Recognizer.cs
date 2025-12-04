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
        
        // Constants from original GLTron Java version
        // These create a complex Lissajous curve movement pattern
        private readonly float[] _xv = { 0.5f, 0.3245f, 0.6f, 0.5f, 0.68f, -0.3f };
        private readonly float[] _yv = { 0.8f, 1.0f, 0.0f, 0.2f, 0.2f, 0.0f };
        private readonly Vector4 _colour = new Vector4(0.6f, 0.16f, 0.2f, 0.50f); // Original red/pink color
        
        // Shadow matrix for shadow rendering
        private readonly Matrix _shadowMatrix = new Matrix(
            4.0f, 0.0f, 0.0f, 0.0f,
            0.0f, 4.0f, 0.0f, 0.0f,
            -2.0f, -2.0f, 0.0f, 0.0f,
            0.0f, 0.0f, 0.0f, 4.0f
        );
        
        private const float SCALE_FACTOR = 0.25f;  // Original scale factor
        private const float HEIGHT = 40.0f;        // Original height above ground
        
        // Placement mode: original GLTron behavior by default
        private bool _aboveMode = false;
        public void SetAboveMode(bool above) => _aboveMode = above;
        
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
            // Original GLTron timing
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

            if (_aboveMode)
            {
                // Alternative mode: Center recognizer above arena with gentle movement
                float center = _gridSize * 0.5f;
                float baseHeight = Math.Max(20.0f, _gridSize * 0.15f);
                
                // Add some lateral movement even in above mode for interest
                float xOffset = (float)Math.Sin(_alpha * 0.3f) * _gridSize * 0.15f;
                float zOffset = (float)Math.Cos(_alpha * 0.25f) * _gridSize * 0.15f;
                float bob = (float)Math.Sin(_alpha * 0.8f) * 3.0f;
                
                return new Vector3(center + xOffset, baseHeight + bob, center + zOffset);
            }

            // Original GLTron movement pattern inside arena
            // The recognizer moves in a complex Lissajous curve pattern
            float boundary = _gridSize - max;
            
            // GetX() and GetY() return values between -1 and 1
            // We map this to the available space in the arena
            float normalizedX = GetX(); // -1 to 1
            float normalizedY = GetY(); // -1 to 1
            
            // Map from [-1, 1] to [max, gridSize - max]
            // This ensures the recognizer stays within the arena with proper margins
            float x = max + (normalizedX + 1.0f) * boundary / 2.0f;
            float z = max + (normalizedY + 1.0f) * boundary / 2.0f;

            // Additional safety clamping with wall margins
            // The recognizer should never get too close to walls
            float wallMargin = Math.Max(5.0f, max * 3.0f); // Larger margin for safety
            x = MathHelper.Clamp(x, wallMargin, _gridSize - wallMargin);
            z = MathHelper.Clamp(z, wallMargin, _gridSize - wallMargin);
            
            // Add slight vertical bobbing for more dynamic movement
            float verticalBob = (float)Math.Sin(_alpha * 2.0f) * 2.0f;
            
            return new Vector3(x, HEIGHT + verticalBob, z);
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
            float dyval = velocity.Z; // Z is the Y in 3D space (ground plane)
            
            // Handle zero velocity case
            float magnitude = (float)Math.Sqrt(dxval * dxval + dyval * dyval);
            if (magnitude < 0.0001f)
                return 0.0f;
            
            float phi = (float)Math.Acos(MathHelper.Clamp(dxval / magnitude, -1.0f, 1.0f));
            
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
            
            // Add some rotation animation for visual interest
            float spinAngle = _alpha * 30.0f; // Slow rotation
            
            return Matrix.CreateScale(SCALE_FACTOR) *
                   Matrix.CreateRotationY(MathHelper.ToRadians(angle)) *
                   Matrix.CreateRotationZ(MathHelper.ToRadians(spinAngle)) *
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
            // Add slight color pulsing for more dynamic appearance
            float pulse = (float)(Math.Sin(_alpha * 3.0f) * 0.1f + 0.9f);
            return _colour * pulse;
        }
        
        /// <summary>
        /// Get recognizer info for debugging
        /// </summary>
        /// <returns>Debug string with position and velocity info</returns>
        public string GetDebugInfo(Vector3 modelBoundingBoxSize)
        {
            Vector3 pos = GetPosition(modelBoundingBoxSize);
            Vector3 vel = GetVelocity();
            return $"Recognizer: Pos({pos.X:F1}, {pos.Y:F1}, {pos.Z:F1}) Vel({vel.X:F2}, {vel.Z:F2}) Alpha={_alpha:F2}";
        }
        
        /// <summary>
        /// Check if recognizer is in safe zone (not too close to walls)
        /// </summary>
        public bool IsInSafeZone(Vector3 modelBoundingBoxSize)
        {
            Vector3 pos = GetPosition(modelBoundingBoxSize);
            float margin = 10.0f;
            return pos.X > margin && pos.X < (_gridSize - margin) &&
                   pos.Z > margin && pos.Z < (_gridSize - margin);
        }
        
        // Private animation calculation methods (exact port from original Java GLTron)
        private float GetX()
        {
            // Original formula for X position (-1 to 1 range)
            return (_xv[0] * (float)Math.Sin(_xv[1] * _alpha + _xv[2]) - 
                    _xv[3] * (float)Math.Sin(_xv[4] * _alpha + _xv[5]));
        }
        
        private float GetY()
        {
            // Original formula for Y position (-1 to 1 range)
            // Note: There was a typo in the original - should be + not -
            return (_yv[0] * (float)Math.Cos(_yv[1] * _alpha + _yv[2]) - 
                    _yv[3] * (float)Math.Sin(_yv[4] * _alpha + _yv[5]));
        }
        
        private float GetDX()
        {
            // Derivative of X for velocity calculation
            return (_xv[1] * _xv[0] * (float)Math.Cos(_xv[1] * _alpha + _xv[2]) - 
                    _xv[4] * _xv[3] * (float)Math.Cos(_xv[4] * _alpha + _xv[5]));
        }
        
        private float GetDY()
        {
            // Derivative of Y for velocity calculation
            return -(_yv[1] * _yv[0] * (float)Math.Sin(_yv[1] * _alpha + _yv[2]) - 
                     _yv[4] * _yv[3] * (float)Math.Sin(_yv[4] * _alpha + _yv[5]));
        }
    }
}
