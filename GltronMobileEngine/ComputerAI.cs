using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using GltronMobileEngine.Interfaces;

namespace GltronMobileEngine
{
    /// <summary>
    /// Ultra-defensive AI with learning capabilities for GLTron Mobile
    /// Focuses on wall avoidance and survival
    /// </summary>
    public static class ComputerAI
    {
        // Game state
        private static ISegment[]? _walls;
        private static IPlayer[]? _players;
        private static float _gridSize;
        private static long _currentTime;
        private static int _aiLevel = 3; // 0=easy, 1=medium, 2=hard, 3=expert
        
        // Per-AI learning memory (persists during session)
        private static Dictionary<int, AIMemory> _aiMemory = new Dictionary<int, AIMemory>();
        
        // AI parameters by difficulty level
        private static readonly int[] MIN_TURN_TIME = { 600, 400, 200, 100 };
        private static readonly float[] CRITICAL_DISTANCE = { 0.3f, 0.2f, 0.15f, 0.1f };
        private static readonly float[] SAFETY_MARGIN = { 15f, 12f, 10f, 8f };
        
        private class AIMemory
        {
            public float[] DangerMemory = new float[3]; // Forward, Left, Right
            public int LastTurnDirection = 0;
            public long LastTurnTime = 0;
            public int SpiralCounter = 0;
            
            public void DecayMemory()
            {
                for (int i = 0; i < 3; i++)
                    DangerMemory[i] *= 0.95f;
            }
        }
        
        /// <summary>
        /// Initialize AI system
        /// </summary>
        public static void InitAI(ISegment[] walls, IPlayer[] players, float gridSize)
        {
            _walls = walls;
            _players = players;
            _gridSize = gridSize;
            
            // Initialize memory for each AI player
            for (int i = 0; i < players.Length; i++)
            {
                if (!_aiMemory.ContainsKey(i))
                    _aiMemory[i] = new AIMemory();
            }
            
            System.Diagnostics.Debug.WriteLine($"ComputerAI: Initialized for {players.Length} players, grid size {gridSize}");
        }
        
        /// <summary>
        /// Update current time
        /// </summary>
        public static void UpdateTime(long deltaTime, long currentTime)
        {
            _currentTime = currentTime;
        }
        
        /// <summary>
        /// Main AI decision function
        /// </summary>
        public static void DoComputer(int playerIndex, int ownPlayerIndex)
        {
            if (_players == null || _walls == null) return;
            if (playerIndex >= _players.Length || playerIndex == ownPlayerIndex) return;
            
            var player = _players[playerIndex];
            if (player == null || player.getSpeed() <= 0.0f) return;
            
            // Get or create AI memory
            if (!_aiMemory.ContainsKey(playerIndex))
                _aiMemory[playerIndex] = new AIMemory();
            
            var memory = _aiMemory[playerIndex];
            
            // Check minimum turn time - CRITICAL: prevent rapid turning
            if (_currentTime - memory.LastTurnTime < MIN_TURN_TIME[_aiLevel])
                return;
            
            // Decay danger memory over time
            memory.DecayMemory();
            
            // Calculate distances in each direction
            float[] distances = CalculateDistances(player);
            
            // Debug output
            if (distances[1] < 10f) // Forward distance is critical
            {
                System.Diagnostics.Debug.WriteLine($"AI {playerIndex}: L={distances[0]:F1} F={distances[1]:F1} R={distances[2]:F1} Spiral={memory.SpiralCounter}");
            }
            
            // Make ultra-defensive decision
            int decision = MakeUltraDefensiveDecision(playerIndex, distances, memory);
            
            // Execute the decision
            if (decision != 0)
            {
                player.doTurn(decision, _currentTime);
                memory.LastTurnTime = _currentTime;
                memory.LastTurnDirection = decision;
                
                // Update spiral counter with decay
                if (decision == Player.TURN_LEFT)
                {
                    memory.SpiralCounter++;
                    if (memory.SpiralCounter > 10) memory.SpiralCounter = 10; // Cap it
                }
                else if (decision == Player.TURN_RIGHT)
                {
                    memory.SpiralCounter--;
                    if (memory.SpiralCounter < -10) memory.SpiralCounter = -10; // Cap it
                }
            }
            else
            {
                // Decay spiral counter when going straight
                if (memory.SpiralCounter > 0) memory.SpiralCounter--;
                else if (memory.SpiralCounter < 0) memory.SpiralCounter++;
            }
        }
        
        /// <summary>
        /// Calculate distances to obstacles in all directions
        /// </summary>
        private static float[] CalculateDistances(IPlayer player)
        {
            float x = player.getXpos();
            float y = player.getYpos();
            int direction = player.getDirection();
            
            // Direction vectors: 0=up, 1=right, 2=down, 3=left
            float[] dirX = { 0.0f, 1.0f, 0.0f, -1.0f };
            float[] dirY = { -1.0f, 0.0f, 1.0f, 0.0f };
            
            float[] distances = new float[3]; // left, forward, right
            
            for (int i = 0; i < 3; i++)
            {
                int checkDir = (direction + i - 1 + 4) % 4; // left, forward, right
                distances[i] = CalculateDistanceInDirection(x, y, checkDir, dirX, dirY);
            }
            
            return distances;
        }
        
        /// <summary>
        /// Calculate distance until collision in a specific direction
        /// </summary>
        private static float CalculateDistanceInDirection(float startX, float startY, int direction, float[] dirX, float[] dirY)
        {
            float dx = dirX[direction];
            float dy = dirY[direction];
            float maxDistance = 100f;
            float step = 0.5f; // Smaller steps for better accuracy
            
            for (float d = step; d <= maxDistance; d += step)
            {
                float checkX = startX + dx * d;
                float checkY = startY + dy * d;
                
                // Check wall collision - walls are at 0 and gridSize
                float wallMargin = 0.5f;
                if (checkX <= wallMargin || checkX >= (_gridSize - wallMargin) ||
                    checkY <= wallMargin || checkY >= (_gridSize - wallMargin))
                {
                    return Math.Max(0.1f, d - step); // Return distance just before collision
                }
                
                // Check trail collision
                if (CheckTrailCollision(checkX, checkY))
                {
                    return Math.Max(0.1f, d - step); // Return distance just before collision
                }
            }
            
            return maxDistance;
        }
        
        /// <summary>
        /// Check if a point collides with any trail
        /// </summary>
        private static bool CheckTrailCollision(float x, float y)
        {
            if (_players == null) return false;
            
            foreach (var player in _players)
            {
                if (player == null || player.getTrailHeight() <= 0) continue;
                
                // Check all trail segments
                int trailCount = player.getTrailOffset() + 1;
                for (int i = 0; i < trailCount; i++)
                {
                    var trail = player.getTrail(i);
                    if (trail == null) continue;
                    
                    // Get segment endpoints
                    float sx = trail.vStart.v[0];
                    float sy = trail.vStart.v[1];
                    float ex = sx + trail.vDirection.v[0];
                    float ey = sy + trail.vDirection.v[1];
                    
                    // Skip very short segments
                    if (Math.Abs(trail.vDirection.v[0]) < 0.1f && Math.Abs(trail.vDirection.v[1]) < 0.1f)
                        continue;
                    
                    // Check collision with trail segment (trail width ~1.0)
                    if (PointNearLineSegment(x, y, sx, sy, ex, ey, 1.0f))
                        return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Check if a point is near a line segment
        /// </summary>
        private static bool PointNearLineSegment(float px, float py, float x1, float y1, float x2, float y2, float threshold)
        {
            float dx = x2 - x1;
            float dy = y2 - y1;
            float lengthSq = dx * dx + dy * dy;
            
            if (lengthSq < 0.001f)
            {
                // Point to point distance
                float pointDist = (float)Math.Sqrt((px - x1) * (px - x1) + (py - y1) * (py - y1));
                return pointDist < threshold;
            }
            
            float t = Math.Max(0, Math.Min(1, ((px - x1) * dx + (py - y1) * dy) / lengthSq));
            float projX = x1 + t * dx;
            float projY = y1 + t * dy;
            
            float segmentDist = (float)Math.Sqrt((px - projX) * (px - projX) + (py - projY) * (py - projY));
            return segmentDist < threshold;
        }
        
        /// <summary>
        /// Make ultra-defensive decision based on distances and memory
        /// </summary>
        private static int MakeUltraDefensiveDecision(int playerIndex, float[] distances, AIMemory memory)
        {
            float left = distances[0];
            float forward = distances[1];
            float right = distances[2];
            
            // Adjust safety margins based on difficulty
            float safetyMargin = SAFETY_MARGIN[_aiLevel];
            float criticalDist = CRITICAL_DISTANCE[_aiLevel] * _gridSize;
            float emergencyDist = 5.0f; // Absolute minimum distance
            
            // CRITICAL: Prevent immediate collision
            if (forward < emergencyDist)
            {
                // Emergency evasion - turn to side with most space
                if (left > right && left > emergencyDist)
                {
                    memory.DangerMemory[0] = 10f; // Remember forward was dangerous
                    return Player.TURN_LEFT;
                }
                else if (right > emergencyDist)
                {
                    memory.DangerMemory[0] = 10f; // Remember forward was dangerous
                    return Player.TURN_RIGHT;
                }
                else if (left > right)
                {
                    return Player.TURN_LEFT; // Turn to better side even if both are bad
                }
                else
                {
                    return Player.TURN_RIGHT;
                }
            }
            
            // Update danger memory based on close calls (learning)
            if (forward < safetyMargin) 
            {
                float danger = (safetyMargin - forward) / safetyMargin;
                memory.DangerMemory[0] = Math.Min(memory.DangerMemory[0] + danger * 0.5f, 5f);
            }
            if (left < safetyMargin) 
            {
                float danger = (safetyMargin - left) / safetyMargin;
                memory.DangerMemory[1] = Math.Min(memory.DangerMemory[1] + danger * 0.5f, 5f);
            }
            if (right < safetyMargin) 
            {
                float danger = (safetyMargin - right) / safetyMargin;
                memory.DangerMemory[2] = Math.Min(memory.DangerMemory[2] + danger * 0.5f, 5f);
            }
            
            // Calculate scores (higher is better) with memory influence
            float forwardScore = forward - memory.DangerMemory[0] * 5f;
            float leftScore = left - memory.DangerMemory[1] * 5f;
            float rightScore = right - memory.DangerMemory[2] * 5f;
            
            // Strong anti-spiral logic to prevent self-destruction
            if (Math.Abs(memory.SpiralCounter) > 3)
            {
                if (memory.SpiralCounter > 3)
                {
                    // Been turning left too much - strongly prefer right
                    rightScore += 30f;
                    leftScore -= 10f;
                }
                else if (memory.SpiralCounter < -3)
                {
                    // Been turning right too much - strongly prefer left
                    leftScore += 30f;
                    rightScore -= 10f;
                }
            }
            
            // Decision logic based on situation
            if (forward > criticalDist && forward > safetyMargin)
            {
                // Plenty of space ahead - continue straight unless sides are much better
                if (leftScore > forwardScore + 15f && left > safetyMargin)
                    return Player.TURN_LEFT;
                if (rightScore > forwardScore + 15f && right > safetyMargin)
                    return Player.TURN_RIGHT;
                    
                // Reset spiral counter when going straight
                if (memory.SpiralCounter > 0) memory.SpiralCounter--;
                else if (memory.SpiralCounter < 0) memory.SpiralCounter++;
                
                return 0; // Continue straight
            }
            
            // Need to turn - choose best direction
            if (forward < safetyMargin || forward < criticalDist)
            {
                // Must turn - pick the best side
                if (leftScore > rightScore && left > emergencyDist)
                {
                    return Player.TURN_LEFT;
                }
                else if (right > emergencyDist)
                {
                    return Player.TURN_RIGHT;
                }
                else if (left > right)
                {
                    return Player.TURN_LEFT;
                }
                else
                {
                    return Player.TURN_RIGHT;
                }
            }
            
            return 0; // Default: continue straight
        }
        
        /// <summary>
        /// Set AI difficulty level
        /// </summary>
        public static void SetAiLevel(int level)
        {
            _aiLevel = Math.Clamp(level, 0, 3);
            System.Diagnostics.Debug.WriteLine($"ComputerAI: Level set to {_aiLevel}");
        }
    }
}
