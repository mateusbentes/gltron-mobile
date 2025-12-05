using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using GltronMobileEngine.Interfaces;

namespace GltronMobileEngine
{
    /// <summary>
    /// Ultra-defensive AI with learning capabilities for GLTron Mobile.
    /// Focuses on wall avoidance and survival with multi-layer heuristics.
    /// </summary>
    public static class ComputerAI
    {
        // Game state
        private static ISegment[]? _walls;
        private static IPlayer[]? _players;
        private static float _gridSize;
        private static long _currentTime;
        private static int _aiLevel = 3; // 0=easy, 1=medium, 2=hard, 3=expert

        // Persistent per-player memory
        private static Dictionary<int, AIMemory> _aiMemory = new Dictionary<int, AIMemory>();

        // AI tuning parameters
        private static readonly int[] MIN_TURN_TIME = { 600, 400, 200, 100 };
        private static readonly float[] CRITICAL_DISTANCE = { 0.3f, 0.2f, 0.15f, 0.1f };
        private static readonly float[] SAFETY_MARGIN = { 15f, 12f, 10f, 8f };

        private class AIMemory
        {
            public float[] DangerMemory = new float[3]; // forward, left, right
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
        /// Initialize the AI system with references to walls and players.
        /// </summary>
        public static void InitAI(ISegment[] walls, IPlayer[] players, float gridSize)
        {
            _walls = walls;
            _players = players;
            _gridSize = gridSize;

            // Create memory per AI player
            for (int i = 0; i < players.Length; i++)
            {
                if (!_aiMemory.ContainsKey(i))
                    _aiMemory[i] = new AIMemory();
            }

            System.Diagnostics.Debug.WriteLine($"ComputerAI: Initialized for {players.Length} players, grid size {gridSize}");
        }

        /// <summary>
        /// Update global time.
        /// </summary>
        public static void UpdateTime(long deltaTime, long currentTime)
        {
            _currentTime = currentTime;
        }

        /// <summary>
        /// Main AI decision entry point.
        /// </summary>
        public static void DoComputer(int playerIndex, int ownPlayerIndex)
        {
            if (_players == null || _walls == null) return;
            if (playerIndex >= _players.Length || playerIndex == ownPlayerIndex) return;

            var player = _players[playerIndex];
            if (player == null || player.getSpeed() <= 0f) return;

            // Ensure memory exists
            if (!_aiMemory.ContainsKey(playerIndex))
                _aiMemory[playerIndex] = new AIMemory();

            var memory = _aiMemory[playerIndex];

            // Prevent rapid turning
            if (_currentTime - memory.LastTurnTime < MIN_TURN_TIME[_aiLevel])
                return;

            memory.DecayMemory();

            // Distance probes - NOW PASSING PLAYER INDEX TO AVOID SELF-COLLISION
            float[] distances = CalculateDistances(player, playerIndex);

            // Debug only when forward is dangerous
            if (distances[1] < 10f)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"AI {playerIndex}: L={distances[0]:F1} F={distances[1]:F1} R={distances[2]:F1} Spiral={memory.SpiralCounter}");
            }

            int decision = MakeUltraDefensiveDecision(playerIndex, distances, memory);

            // Execute turn
            if (decision != 0)
            {
                player.doTurn(decision, _currentTime);
                memory.LastTurnTime = _currentTime;
                memory.LastTurnDirection = decision;

                // Spiral counter update
                if (decision == Player.TURN_LEFT)
                {
                    memory.SpiralCounter++;
                    if (memory.SpiralCounter > 10) memory.SpiralCounter = 10;
                }
                else if (decision == Player.TURN_RIGHT)
                {
                    memory.SpiralCounter--;
                    if (memory.SpiralCounter < -10) memory.SpiralCounter = -10;
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
        /// Returns left, forward, and right distances.
        /// </summary>
        private static float[] CalculateDistances(IPlayer player, int playerIndex)
        {
            float x = player.getXpos();
            float y = player.getYpos();
            int direction = player.getDirection();

            float[] dirX = { 0f, 1f, 0f, -1f };
            float[] dirY = { -1f, 0f, 1f, 0f };

            float[] distances = new float[3];

            for (int i = 0; i < 3; i++)
            {
                int checkDir = (direction + i - 1 + 4) % 4;
                distances[i] = CalculateDistanceInDirection(x, y, checkDir, dirX, dirY, playerIndex);
            }

            return distances;
        }

        /// <summary>
        /// Raycast distance in a given direction.
        /// </summary>
        private static float CalculateDistanceInDirection(
            float startX, float startY, int direction, float[] dirX, float[] dirY, int playerIndex)
        {
            float dx = dirX[direction];
            float dy = dirY[direction];

            float maxDistance = 100f;
            float step = 0.5f;

            for (float d = step; d <= maxDistance; d += step)
            {
                float checkX = startX + dx * d;
                float checkY = startY + dy * d;

                float wallMargin = 0.5f;

                // Boundary check
                if (checkX <= wallMargin || checkX >= (_gridSize - wallMargin) ||
                    checkY <= wallMargin || checkY >= (_gridSize - wallMargin))
                {
                    return Math.Max(0.1f, d - step);
                }

                // Trail collision - NOW WITH PLAYER INDEX TO AVOID CHECKING OWN RECENT TRAIL
                if (CheckTrailCollision(checkX, checkY, playerIndex, startX, startY, d))
                    return Math.Max(0.1f, d - step);
            }

            return maxDistance;
        }

        /// <summary>
        /// True if the point is touching any player's trail.
        /// FIXED: Now ignores the AI's own recent trail segments to prevent self-collision detection.
        /// </summary>
        private static bool CheckTrailCollision(float x, float y, int aiPlayerIndex, float aiX, float aiY, float distanceFromAI)
        {
            if (_players == null) return false;

            foreach (var player in _players)
            {
                if (player == null || player.getTrailHeight() <= 0) continue;

                int playerIdx = Array.IndexOf(_players, player);
                bool isOwnPlayer = (playerIdx == aiPlayerIndex);

                int trailCount = player.getTrailOffset() + 1;
                
                // For the AI's own trail, skip the most recent segments to avoid immediate self-collision
                int startIndex = 0;
                if (isOwnPlayer)
                {
                    // Skip the last 3-5 trail segments (adjust based on speed and turn frequency)
                    // This prevents the AI from thinking it's blocked by its own current path
                    startIndex = Math.Max(0, trailCount - 5);
                }

                for (int i = startIndex; i < trailCount; i++)
                {
                    var trail = player.getTrail(i);
                    if (trail == null) continue;

                    float sx = trail.vStart.v[0];
                    float sy = trail.vStart.v[1];
                    float ex = sx + trail.vDirection.v[0];
                    float ey = sy + trail.vDirection.v[1];

                    // Skip zero-length segments
                    if (Math.Abs(trail.vDirection.v[0]) < 0.1f &&
                        Math.Abs(trail.vDirection.v[1]) < 0.1f)
                        continue;

                    // Additional check: for own player, ignore segments very close to current position
                    if (isOwnPlayer)
                    {
                        float distToSegmentStart = (float)Math.Sqrt(
                            (sx - aiX) * (sx - aiX) + (sy - aiY) * (sy - aiY));
                        
                        // If this segment starts very close to AI (within 3 units), and we're checking
                        // a point close to the AI, skip it
                        if (distToSegmentStart < 3.0f && distanceFromAI < 3.0f)
                            continue;
                    }

                    // Use a slightly larger threshold for collision detection
                    if (PointNearLineSegment(x, y, sx, sy, ex, ey, 1.2f))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Distance from a point to a finite line segment.
        /// </summary>
        private static bool PointNearLineSegment(
            float px, float py, float x1, float y1, float x2, float y2, float threshold)
        {
            float dx = x2 - x1;
            float dy = y2 - y1;

            float lengthSq = dx * dx + dy * dy;

            if (lengthSq < 0.001f)
            {
                float dist = (float)Math.Sqrt(
                    (px - x1) * (px - x1) +
                    (py - y1) * (py - y1));
                return dist < threshold;
            }

            float t = Math.Max(0, Math.Min(1,
                ((px - x1) * dx + (py - y1) * dy) / lengthSq));

            float projX = x1 + t * dx;
            float projY = y1 + t * dy;

            float sd = (float)Math.Sqrt(
                (px - projX) * (px - projX) +
                (py - projY) * (py - projY));

            return sd < threshold;
        }

        /// <summary>
        /// Core ultra-defensive decision logic.
        /// </summary>
        private static int MakeUltraDefensiveDecision(int playerIndex, float[] distances, AIMemory memory)
        {
            float left = distances[0];
            float forward = distances[1];
            float right = distances[2];

            // Adjust safety margins based on difficulty
            float safetyMargin = SAFETY_MARGIN[_aiLevel];
            float criticalDist = CRITICAL_DISTANCE[_aiLevel] * _gridSize;
            float emergency = 7.0f;

            // Emergency: must turn
            if (forward < emergency)
            {
                if (left > right && left > emergency)
                    return Player.TURN_LEFT;
                if (right > emergency)
                    return Player.TURN_RIGHT;

                return (left > right) ? Player.TURN_LEFT : Player.TURN_RIGHT;
            }

            // Learning memory update
            if (forward < safetyMargin)
            {
                float d = (safetyMargin - forward) / safetyMargin;
                memory.DangerMemory[0] = Math.Min(5f, memory.DangerMemory[0] + d * 0.5f);
            }
            if (left < safetyMargin)
            {
                float d = (safetyMargin - left) / safetyMargin;
                memory.DangerMemory[1] = Math.Min(5f, memory.DangerMemory[1] + d * 0.5f);
            }
            if (right < safetyMargin)
            {
                float d = (safetyMargin - right) / safetyMargin;
                memory.DangerMemory[2] = Math.Min(5f, memory.DangerMemory[2] + d * 0.5f);
            }

            // Compute lane scores
            float forwardScore = forward - memory.DangerMemory[0] * 5f;
            float leftScore = left - memory.DangerMemory[1] * 5f;
            float rightScore = right - memory.DangerMemory[2] * 5f;

            // Anti-spiral system
            if (Math.Abs(memory.SpiralCounter) > 5)
            {
                if (memory.SpiralCounter > 5)
                {
                    // Been turning left too much - strongly prefer right
                    rightScore += 30f;
                    leftScore -= 10f;
                }
                else
                {
                    // Been turning right too much - strongly prefer left
                    leftScore += 30f;
                    rightScore -= 10f;
                }
            }

            // If safe ahead, prefer going straight
            if (forward > criticalDist && forward > safetyMargin)
            {
                // Plenty of space ahead - continue straight unless sides are much better
                if (leftScore > forwardScore + 15f && left > safetyMargin)
                    return Player.TURN_LEFT;

                if (rightScore > forwardScore + 15f && right > safetyMargin)
                    return Player.TURN_RIGHT;

                return 0;
            }

            // Otherwise, pick the best side
            if (leftScore > rightScore)
                return Player.TURN_LEFT;
            else
                return Player.TURN_RIGHT;
        }

        /// <summary>
        /// Adjust AI difficulty level.
        /// </summary>
        public static void SetAiLevel(int level)
        {
            _aiLevel = Math.Clamp(level, 0, 3);
            System.Diagnostics.Debug.WriteLine($"ComputerAI: AI level set to {_aiLevel}");
        }
    }
}
