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

            // Rolling average of last 3 frames distances: [left, forward, right]
            public float[][] DistHistory = new float[3][] { new float[3], new float[3], new float[3] };
            public int DistHistIndex = 0;
            public int DistHistCount = 0;

            // Decision stickiness
            public int LastDecision = 0; // -1(left), 0(straight), +1(right)
            public int StableFrames = 0; // how many frames the alternative has been better

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
            float[] distancesRaw = CalculateDistances(player, playerIndex);

            // Update rolling average in memory
            memory.DistHistory[memory.DistHistIndex][0] = distancesRaw[0];
            memory.DistHistory[memory.DistHistIndex][1] = distancesRaw[1];
            memory.DistHistory[memory.DistHistIndex][2] = distancesRaw[2];
            memory.DistHistIndex = (memory.DistHistIndex + 1) % 3;
            if (memory.DistHistCount < 3) memory.DistHistCount++;

            float[] distances = new float[3];
            for (int i = 0; i < memory.DistHistCount; i++)
            {
                distances[0] += memory.DistHistory[i][0];
                distances[1] += memory.DistHistory[i][1];
                distances[2] += memory.DistHistory[i][2];
            }
            distances[0] /= Math.Max(1, memory.DistHistCount);
            distances[1] /= Math.Max(1, memory.DistHistCount);
            distances[2] /= Math.Max(1, memory.DistHistCount);

            // Debug only when forward is dangerous
            if (distances[1] < 10f)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"AI {playerIndex}: L={distances[0]:F1} F={distances[1]:F1} R={distances[2]:F1} Spiral={memory.SpiralCounter}");
            }

            int decision = MakeUltraDefensiveDecision(playerIndex, distances, memory);

            // Decision stickiness: require stability across frames to switch decisions when not emergency
            if (decision == Player.TURN_LEFT)
            {
                // desired = -1
                if (memory.LastDecision == -1)
                    memory.StableFrames++;
                else
                    memory.StableFrames = 1;
                memory.LastDecision = -1;
            }
            else if (decision == Player.TURN_RIGHT)
            {
                if (memory.LastDecision == 1)
                    memory.StableFrames++;
                else
                    memory.StableFrames = 1;
                memory.LastDecision = 1;
            }
            else
            {
                if (memory.LastDecision == 0)
                    memory.StableFrames++;
                else
                    memory.StableFrames = 1;
                memory.LastDecision = 0;
            }

            // If not emergency (forward reasonably safe), require at least 2 stable frames before taking a side turn
            bool forwardSafe = distances[1] > 10.0f;
            if (forwardSafe && decision != 0 && memory.StableFrames < 2)
            {
                decision = 0; // hold straight until the side has been better consistently
            }

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

            // Match Player.DIRS_X/Y mapping exactly (see Player.cs)
            float[] dirX = { 0.0f, -1.0f, 0.0f, 1.0f };
            float[] dirY = { -1.0f, 0.0f, 1.0f, 0.0f };

            float[] distances = new float[3];

            for (int i = 0; i < 3; i++)
            {
                // i=0: Left probe (turn left from current direction)
                // i=1: Forward probe (current direction)
                // i=2: Right probe (turn right from current direction)
                int checkDir;
                if (i == 0) // Left probe
                {
                    // Left turn is (current direction + Player.TURN_LEFT) % 4 where TURN_LEFT == 3
                    checkDir = (direction + Player.TURN_LEFT) % 4;
                }
                else if (i == 2) // Right probe
                {
                    // Right turn is (current direction + Player.TURN_RIGHT) % 4 where TURN_RIGHT == 1
                    checkDir = (direction + Player.TURN_RIGHT) % 4;
                }
                else // Forward probe (i == 1)
                {
                    checkDir = direction;
                }
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

            float d = 0f;
            while (d <= maxDistance)
            {
                // Adaptive step: finer near origin for first 5 units
                float step = (d < 5.0f) ? 0.1f : 0.25f;
                d += step;

                float checkX = startX + dx * d;
                float checkY = startY + dy * d;

                // Base wall margin
                float wallMargin = 0.5f;
                // Near-wall anticipation: if we're heading toward a wall and are already close, anticipate earlier
                float nearWallThreshold = 10.0f;
                if ((dx < 0 && startX < nearWallThreshold) || (dx > 0 && (_gridSize - startX) < nearWallThreshold) ||
                    (dy < 0 && startY < nearWallThreshold) || (dy > 0 && (_gridSize - startY) < nearWallThreshold))
                {
                    wallMargin = 1.5f; // inflate margin when near the arena boundary for earlier detection
                }

                // Hard early clamp when extremely close in heading direction to avoid late discovery
                if (dx < 0 && startX <= 6.0f) { if (checkX <= wallMargin) return Math.Max(0.1f, d - step); }
                if (dx > 0 && (_gridSize - startX) <= 6.0f) { if (checkX >= (_gridSize - wallMargin)) return Math.Max(0.1f, d - step); }
                if (dy < 0 && startY <= 6.0f) { if (checkY <= wallMargin) return Math.Max(0.1f, d - step); }
                if (dy > 0 && (_gridSize - startY) <= 6.0f) { if (checkY >= (_gridSize - wallMargin)) return Math.Max(0.1f, d - step); }

                // Boundary check (hard stop for walls)
                if (checkX <= wallMargin || checkX >= (_gridSize - wallMargin) ||
                    checkY <= wallMargin || checkY >= (_gridSize - wallMargin))
                {
                    return Math.Max(0.1f, d - step);
                }

                // Trail collision - ignore own most recent trail segments better
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

            for (int p = 0; p < _players.Length; p++)
            {
                var player = _players[p];
                if (player == null || player.getTrailHeight() <= 0) continue;

                bool isOwnPlayer = (p == aiPlayerIndex);

                int trailCount = player.getTrailOffset() + 1;

                // For the AI's own trail, skip the most recent segments to avoid immediate self-collision
                int startIndex = 0;
                if (isOwnPlayer)
                {
                    // Skip the last few trail segments (current drawing + previous turns)
                    startIndex = Math.Max(0, trailCount - 8);
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
                    if (Math.Abs(trail.vDirection.v[0]) < 0.1f && Math.Abs(trail.vDirection.v[1]) < 0.1f)
                        continue;

                    if (isOwnPlayer)
                    {
                        float distToSegmentStart = (float)Math.Sqrt((sx - aiX) * (sx - aiX) + (sy - aiY) * (sy - aiY));
                        float distToSegmentEnd = (float)Math.Sqrt((ex - aiX) * (ex - aiX) + (ey - aiY) * (ey - aiY));
                        // Heuristic: if point is very close to AI and segment starts/ends near AI, ignore it (current drawing or fresh corner)
                        if ((distToSegmentStart < 3.0f || distToSegmentEnd < 3.0f) && distanceFromAI < 3.0f)
                            continue;
                    }

                    // Use slightly larger threshold for others; slightly smaller for our own very-near checks
                    float threshold = 1.4f; // others' trails are slightly fatter for safety
                    if (isOwnPlayer && distanceFromAI < 3.0f)
                        threshold = 0.6f;

                    if (PointNearLineSegment(x, y, sx, sy, ex, ey, threshold))
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
            // Scale emergency with speed and wall proximity
            float speed = (_players != null && playerIndex < _players.Length && _players[playerIndex] != null) ? _players[playerIndex].getSpeed() : 6.0f;
            float wallProximity = Math.Min(Math.Min(distances[1], Math.Min(distances[0], distances[2])), 10.0f);
            float emergency = 7.0f + 0.25f * speed + ((wallProximity < 10.0f) ? (10.0f - wallProximity) * 0.2f : 0.0f);

            // Dynamic friction: discourage rapid consecutive turns
            float recentTurnBias = 0f;
            long sinceTurn = _currentTime - memory.LastTurnTime;
            if (sinceTurn > 0 && sinceTurn < (long)(MIN_TURN_TIME[_aiLevel] * 1.5f))
            {
                recentTurnBias = 12f; // stronger requirement to justify a new turn
            }
            bool inCooldown = sinceTurn > 0 && sinceTurn < (long)(MIN_TURN_TIME[_aiLevel] * 2.0f);

            // Emergency: must turn
            // If the path ahead is critically short, we must turn.
            if (forward < emergency)
            {
                // Prefer the side with more space, but only if it's also safe (greater than emergency distance)
                if (left > right && left > emergency)
                    return Player.TURN_LEFT;
                
                if (right > emergency)
                    return Player.TURN_RIGHT;

                // If both sides are also dangerous, choose with side-stickiness to avoid zig-zag
                // Use SpiralCounter as a rough last-turn bias
                if (Math.Abs(memory.SpiralCounter) > 0)
                {
                    if (memory.SpiralCounter > 0) // leaning left previously
                        return (right >= left - 2f) ? Player.TURN_RIGHT : Player.TURN_LEFT;
                    else // leaning right previously
                        return (left >= right - 2f) ? Player.TURN_LEFT : Player.TURN_RIGHT;
                }
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
            float forwardScore = forward - memory.DangerMemory[0] * 4f; // reduce memory weight slightly
            float leftScore = left - memory.DangerMemory[1] * 4f;
            float rightScore = right - memory.DangerMemory[2] * 4f;

            // Mild preference to keep going straight when safe, to avoid boxing inward
            if (forward > safetyMargin)
            {
                forwardScore += 8f; // stronger straight bias when safe
            }

            // Anti-spiral system (avoid turning inward repeatedly when not in emergency)
            if (Math.Abs(memory.SpiralCounter) > 5 && forward > emergency)
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
            // If the path ahead is safe (greater than critical distance and safety margin)
            if (forward > criticalDist && forward > safetyMargin)
            {
                // Strong straight bias: require a much larger advantage to justify a turn
                // Add extra penalty if we're still in cooldown (only huge advantage can force turn)
                float neededAdvantage = 30f + recentTurnBias + (inCooldown ? 10f : 0f);

                // Additionally, if forward is well above emergency, avoid turning unless sides are dramatically better
                if (forward > emergency + 3.0f)
                {
                    neededAdvantage += 5f;
                }

                if (leftScore > forwardScore + neededAdvantage && left > safetyMargin)
                    return Player.TURN_LEFT;

                if (rightScore > forwardScore + neededAdvantage && right > safetyMargin)
                    return Player.TURN_RIGHT;

                // Otherwise, continue straight (0 = no turn)
                return 0;
            }

            // Otherwise, pick the best side
            // If the path ahead is not safe, choose the direction with the highest score.
            // Apply recent-turn friction lightly here too to avoid rapid alternating turns
            if (leftScore > rightScore + recentTurnBias * 0.5f)
                return Player.TURN_LEFT;
            else if (rightScore > leftScore + recentTurnBias * 0.5f)
                return Player.TURN_RIGHT;
            else
            {
                // Tie or too close: hold straight if not yet dangerous
                if (forward > emergency)
                    return 0;
                return (left >= right) ? Player.TURN_LEFT : Player.TURN_RIGHT;
            }
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
