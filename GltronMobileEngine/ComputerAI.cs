using System;
using Microsoft.Xna.Framework;
using GltronMobileEngine.Interfaces;

namespace GltronMobileEngine
{
    /// <summary>
    /// Computer AI system for GLTron Mobile - Enhanced competitive version
    /// Based on original GLTron AI with improved strategic capabilities
    /// </summary>
    public static class ComputerAI
    {
        // Direction indices for segment calculations
        private const int E_FRONT = 0;
        private const int E_LEFT = 1;
        private const int E_RIGHT = 2;
        private const int E_BACKLEFT = 3;
        private const int E_MAX = 4;
        
        // AI difficulty parameters (0=easy, 1=medium, 2=hard, 3=expert)
        private static readonly int[] MIN_TURN_TIME = { 600, 400, 200, 100 };
        private static readonly float[] MAX_SEG_LENGTH = { 0.6f, 0.3f, 0.3f, 0.3f };
        private static readonly float[] CRITICAL = { 0.2f, 0.08037f, 0.08037f, 0.08037f };
        private static readonly int[] SPIRAL = { 10, 10, 10, 10 };
        private static readonly float[] RL_DELTA = { 0f, 10f, 20f, 30f };
        
        // Strategic parameters
        private const float SAVE_T_DIFF = 0.500f;
        private const float HOPELESS_T = 0.80f;
        private const float FLT_MAX = 10000.0f;
        private const float OPPONENT_CLOSE_DISTANCE = 48.0f;

        // Aggressive action table [location][direction_difference]
        private static readonly int[,] aggressive_action = {
            { 2, 0, 2, 2 },
            { 0, 1, 1, 2 },
            { 0, 1, 1, 2 },
            { 0, 1, 1, 2 },
            { 0, 2, 2, 1 },
            { 0, 2, 2, 1 },
            { 0, 2, 2, 1 },
            { 1, 1, 1, 0 }
        };

        // Evasive action table [location][direction_difference]
        private static readonly int[,] evasive_action = {
            { 1, 1, 2, 2 },
            { 1, 1, 2, 0 },
            { 1, 1, 2, 0 },
            { 1, 1, 2, 0 },
            { 2, 0, 1, 1 },
            { 2, 0, 1, 1 },
            { 2, 0, 1, 1 },
            { 2, 2, 1, 1 }
        };
        
        // Game state
        private static ISegment[]? _walls;
        private static IPlayer[]? _players;
        private static float _gridSize;
        private static long _currentTime;
        
        // Per-player AI state
        private static long[]? _aiTime;
        private static int[]? _tdiff;
        private static float[]? _distances; // front, left, right, backleft
        
        // AI difficulty level
        private static int _aiLevel = 3; // Default to expert
        
        // Random for tie-breaking
        private static readonly Random _random = new Random();
        
        public static void InitAI(ISegment[] walls, IPlayer[] players, float gridSize)
        {
            try
            {
                _walls = walls;
                _players = players;
                _gridSize = gridSize;
                _aiTime = new long[players.Length];
                _tdiff = new int[players.Length];
                _distances = new float[E_MAX];
                
                System.Diagnostics.Debug.WriteLine($"GLTRON: Competitive AI initialized for {players.Length} players on {gridSize}x{gridSize} grid");
                
#if ANDROID
                Android.Util.Log.Info("GLTRON", $"Competitive AI initialized for {players.Length} players on {gridSize}x{gridSize} grid");
#endif
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GLTRON: AI initialization failed: {ex}");
            }
        }
        
        public static void UpdateTime(long deltaTime, long currentTime)
        {
            _currentTime = currentTime;
        }
        
        public static void DoComputer(int playerIndex, int targetIndex)
        {
            if (_players == null || _walls == null || _aiTime == null) return;
            if (playerIndex >= _players.Length || playerIndex == targetIndex) return;
            
            var player = _players[playerIndex];
            if (player == null || player.getSpeed() <= 0.0f) return;
            
            // Avoid short turns based on difficulty level
            if ((_currentTime - _aiTime[playerIndex]) < MIN_TURN_TIME[_aiLevel])
                return;
            
            try
            {
                // Calculate distances using segment intersection (like original)
                CalculateDistances(playerIndex);
                
                // Find closest opponent
                int closestOpp = GetClosestOpponent(playerIndex);
                float distance = GetOpponentDistance(playerIndex, closestOpp);
                
                // Decide between simple and active AI based on opponent proximity
                if (closestOpp == -1 || 
                    distance > OPPONENT_CLOSE_DISTANCE || 
                    _distances![E_FRONT] < distance)
                {
                    DoComputerSimple(playerIndex, targetIndex);
                }
                else
                {
                    DoComputerActive(playerIndex, closestOpp);
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GLTRON: AI error for player {playerIndex}: {ex}");
            }
        }
        
        private static void CalculateDistances(int playerIndex)
        {
            if (_players == null || _distances == null) return;
            
            var player = _players[playerIndex];
            int currDir = player.getDirection();
            int dirLeft = (currDir + 3) % 4;
            int dirRight = (currDir + 1) % 4;
            
            float x = player.getXpos();
            float y = player.getYpos();
            
            // Direction vectors
            float[] dirX = { 0.0f, 1.0f, 0.0f, -1.0f };
            float[] dirY = { -1.0f, 0.0f, 1.0f, 0.0f };
            
            // Initialize segments for each direction
            var segments = new Segment[E_MAX];
            for (int i = 0; i < E_MAX; i++)
            {
                segments[i] = new Segment();
                segments[i].vStart = new Vec(x, y, 0);
            }
            
            // Set direction vectors - scale them to be long rays for intersection
            float rayLength = _gridSize * 2.0f; // Make rays long enough to hit any wall
            
            segments[E_FRONT].vDirection = new Vec(dirX[currDir] * rayLength, dirY[currDir] * rayLength, 0);
            segments[E_LEFT].vDirection = new Vec(dirX[dirLeft] * rayLength, dirY[dirLeft] * rayLength, 0);
            segments[E_RIGHT].vDirection = new Vec(dirX[dirRight] * rayLength, dirY[dirRight] * rayLength, 0);
            
            // Backleft is diagonal (for spiral detection)
            segments[E_BACKLEFT].vDirection = new Vec(
                (dirX[dirLeft] - dirX[currDir]) * rayLength,
                (dirY[dirLeft] - dirY[currDir]) * rayLength,
                0
            );
            // Don't normalize backleft - keep it scaled
            
            // Initialize distances to maximum
            _distances[E_FRONT] = FLT_MAX;
            _distances[E_LEFT] = FLT_MAX;
            _distances[E_RIGHT] = FLT_MAX;
            _distances[E_BACKLEFT] = FLT_MAX;
            
            // Check intersections with walls FIRST (most important)
            if (_walls != null && _walls.Length >= 4)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (_walls[i] == null) continue;
                    CheckSegmentIntersection(segments[E_FRONT], _walls[i], ref _distances[E_FRONT]);
                    CheckSegmentIntersection(segments[E_LEFT], _walls[i], ref _distances[E_LEFT]);
                    CheckSegmentIntersection(segments[E_RIGHT], _walls[i], ref _distances[E_RIGHT]);
                    CheckSegmentIntersection(segments[E_BACKLEFT], _walls[i], ref _distances[E_BACKLEFT]);
                }
            }
            
            // Then check intersections with all player trails
            for (int i = 0; i < _players.Length; i++)
            {
                if (_players[i] == null || _players[i].getTrailHeight() < Player.TRAIL_HEIGHT)
                    continue;
                
                int trailCount = _players[i].getTrailOffset() + 1;
                
                for (int j = 0; j < trailCount; j++)
                {
                    // Skip own current segment
                    if (i == playerIndex && j == _players[i].getTrailOffset())
                        break;
                    
                    var trail = _players[i].getTrail(j);
                    if (trail == null) continue;
                    
                    // Check intersection for each direction
                    CheckSegmentIntersection(segments[E_FRONT], trail, ref _distances[E_FRONT]);
                    CheckSegmentIntersection(segments[E_LEFT], trail, ref _distances[E_LEFT]);
                    CheckSegmentIntersection(segments[E_RIGHT], trail, ref _distances[E_RIGHT]);
                    CheckSegmentIntersection(segments[E_BACKLEFT], trail, ref _distances[E_BACKLEFT]);
                }
            }
            
            // Debug: Log distances if any are critically low
            if (_distances[E_FRONT] < 5.0f || _distances[E_LEFT] < 5.0f || _distances[E_RIGHT] < 5.0f)
            {
                System.Diagnostics.Debug.WriteLine($"AI {playerIndex} distances: F={_distances[E_FRONT]:F1} L={_distances[E_LEFT]:F1} R={_distances[E_RIGHT]:F1} at ({x:F1},{y:F1}) dir={currDir}");
            }
        }
        
        private static void CheckSegmentIntersection(Segment ray, ISegment wall, ref float distance)
        {
            var result = ray.Intersect(wall);
            if (result != null)
            {
                float t1 = ray.t1;
                float t2 = ray.t2;
                
                // t1 is the parameter along the ray (0 to 1 for full ray length)
                // t2 is the parameter along the wall (0 to 1 for wall segment)
                if (t1 > 0.0f && t2 >= 0.0f && t2 <= 1.0f)
                {
                    // Calculate actual distance from ray parameter
                    float rayLength = ray.vDirection.Length();
                    float actualDistance = t1 * rayLength;
                    
                    if (actualDistance < distance)
                    {
                        distance = actualDistance;
                    }
                }
            }
        }
        
        private static int GetClosestOpponent(int playerIndex)
        {
            if (_players == null) return -1;
            
            var player = _players[playerIndex];
            float minDistance = FLT_MAX;
            int closest = -1;
            
            for (int i = 0; i < _players.Length; i++)
            {
                if (i == playerIndex || _players[i] == null || _players[i].getSpeed() <= 0.0f)
                    continue;
                
                float dx = _players[i].getXpos() - player.getXpos();
                float dy = _players[i].getYpos() - player.getYpos();
                float d = Math.Abs(dx) + Math.Abs(dy); // Manhattan distance for speed
                
                if (d < minDistance)
                {
                    minDistance = d;
                    closest = i;
                }
            }
            
            return closest;
        }
        
        private static float GetOpponentDistance(int playerIndex, int opponentIndex)
        {
            if (_players == null || opponentIndex < 0 || opponentIndex >= _players.Length)
                return FLT_MAX;
            
            var player = _players[playerIndex];
            var opponent = _players[opponentIndex];
            
            if (opponent == null || opponent.getSpeed() <= 0.0f)
                return FLT_MAX;
            
            float dx = opponent.getXpos() - player.getXpos();
            float dy = opponent.getYpos() - player.getYpos();
            return Math.Abs(dx) + Math.Abs(dy);
        }
        
        public static void SetAiLevel(int level)
        {
            _aiLevel = Math.Clamp(level, 0, 3);
            System.Diagnostics.Debug.WriteLine($"GLTRON: AI level set to {_aiLevel}");
        }

        // Helper class for segment calculations
        private class Segment
        {
            public Vec vStart;
            public Vec vDirection;
            public float t1, t2;
            
            public Segment()
            {
                vStart = new Vec(0, 0, 0);
                vDirection = new Vec(0, 0, 0);
            }
            
            public Vec? Intersect(ISegment other)
            {
                // Get segment data from interface
                float sx = other.vStart.v[0];
                float sy = other.vStart.v[1];
                float dx = other.vDirection.v[0];
                float dy = other.vDirection.v[1];
                
                // Calculate intersection using parametric line equations
                float det = vDirection.v[0] * (-dy) - vDirection.v[1] * (-dx);
                
                if (Math.Abs(det) < 0.0001f) // Parallel segments
                    return null;
                
                float diffX = sx - vStart.v[0];
                float diffY = sy - vStart.v[1];
                
                t1 = (diffX * (-dy) - diffY * (-dx)) / det;
                t2 = (diffX * vDirection.v[1] - diffY * vDirection.v[0]) / det;
                
                if (t1 >= 0 && t2 >= 0 && t2 <= 1.0f)
                {
                    return new Vec(
                        vStart.v[0] + t1 * vDirection.v[0],
                        vStart.v[1] + t1 * vDirection.v[1],
                        0
                    );
                }
                
                return null;
            }
            
            public float Length()
            {
                return vDirection.Length();
            }
        }
        
        private static void DoComputerSimple(int playerIndex, int targetIndex)
        {
            if (_players == null || _distances == null || _tdiff == null || _aiTime == null) return;
            
            var player = _players[playerIndex];
            var trail = player.getTrail(player.getTrailOffset());
            
            float front = _distances[E_FRONT];
            float left = _distances[E_LEFT];
            float right = _distances[E_RIGHT];
            float backleft = _distances[E_BACKLEFT];
            
            // Check if we need to turn based on critical distance and segment length
            float criticalDist = CRITICAL[_aiLevel] * _gridSize;
            float maxSegLength = MAX_SEG_LENGTH[_aiLevel] * _gridSize;
            float segmentLength = GetSegmentLength(trail);
            
            // Add safety margin to prevent wall collisions
            float safetyMargin = 2.0f + player.getSpeed() * 0.1f; // Dynamic margin based on speed
            
            // Turn if getting too close to wall or segment too long
            if (front > criticalDist && segmentLength < maxSegLength && front > safetyMargin)
                return; // Safe to continue straight
            
            // Decide where to turn
            if (front > right && front > left)
                return; // No way out, continue straight (will crash)
            
            float rlDelta = RL_DELTA[_aiLevel];
            int tdiff = _tdiff[playerIndex];
            
            // Safety threshold - minimum distance required to make a turn
            float minTurnDistance = 3.0f + player.getSpeed() * 0.15f;
            
            // Anti-spiral logic with backleft check
            if (left > minTurnDistance && left > rlDelta && 
                Math.Abs(right - left) < rlDelta && 
                backleft > left && 
                tdiff < SPIRAL[_aiLevel])
            {
                player.doTurn(Player.TURN_LEFT, _currentTime);
                _tdiff[playerIndex]++;
                _aiTime[playerIndex] = _currentTime;
            }
            else if (right > minTurnDistance && right > left && tdiff > -SPIRAL[_aiLevel])
            {
                player.doTurn(Player.TURN_RIGHT, _currentTime);
                _tdiff[playerIndex]--;
                _aiTime[playerIndex] = _currentTime;
            }
            else if (left > minTurnDistance && right < left && tdiff < SPIRAL[_aiLevel])
            {
                player.doTurn(Player.TURN_LEFT, _currentTime);
                _tdiff[playerIndex]++;
                _aiTime[playerIndex] = _currentTime;
            }
            else
            {
                // Balance turns based on tdiff, but only if safe
                if (tdiff > 0 && right > minTurnDistance)
                {
                    player.doTurn(Player.TURN_RIGHT, _currentTime);
                    _tdiff[playerIndex]--;
                    _aiTime[playerIndex] = _currentTime;
                }
                else if (left > minTurnDistance)
                {
                    player.doTurn(Player.TURN_LEFT, _currentTime);
                    _tdiff[playerIndex]++;
                    _aiTime[playerIndex] = _currentTime;
                }
                // If neither direction is safe, don't turn (will crash, but no choice)
            }
        }

        
        private static void DoComputerActive(int playerIndex, int targetIndex)
        {
            if (_players == null || _aiTime == null) return;
            
            var player = _players[playerIndex];
            var target = _players[targetIndex];
            
            if (target == null || target.getSpeed() <= 0.0f)
            {
                DoComputerSimple(playerIndex, targetIndex);
                return;
            }
            
            // Calculate intersection point and timing
            int location = CalculateSector(player, target);
            var intersection = CalculateIntersection(player, target);
            
            if (intersection == null)
            {
                DoComputerSimple(playerIndex, targetIndex);
                return;
            }
            
            float t_player = intersection.Item1;
            float t_opponent = intersection.Item2;
            
            // Make strategic decision based on sector and timing
            switch (location)
            {
                case 0:
                case 1:
                case 6:
                case 7:
                    // Head-on or near head-on collision scenarios
                    if (t_player < t_opponent)
                    {
                        // We reach intersection first - be aggressive
                        AiAggressive(playerIndex, targetIndex, location);
                    }
                    else
                    {
                        if (t_opponent < HOPELESS_T)
                        {
                            // Opponent very close to intersection - evade
                            AiEvasive(playerIndex, targetIndex, location);
                        }
                        else if (t_opponent - t_player < SAVE_T_DIFF)
                        {
                            // Close timing - try to win the race
                            AiAggressive(playerIndex, targetIndex, location);
                        }
                        else
                        {
                            // Opponent has advantage - evade
                            AiEvasive(playerIndex, targetIndex, location);
                        }
                    }
                    break;
                    
                case 2:
                case 3:
                case 4:
                case 5:
                    // Side or rear approaches - use simple AI
                    DoComputerSimple(playerIndex, targetIndex);
                    break;
                    
                default:
                    DoComputerSimple(playerIndex, targetIndex);
                    break;
            }
        }
        
        private static int CalculateSector(IPlayer player, IPlayer opponent)
        {
            // Calculate relative position sector (0-7, like compass directions)
            float px = player.getXpos();
            float py = player.getYpos();
            float ox = opponent.getXpos();
            float oy = opponent.getYpos();
            
            // Direction vectors
            float[] dirX = { 0.0f, 1.0f, 0.0f, -1.0f };
            float[] dirY = { -1.0f, 0.0f, 1.0f, 0.0f };
            
            float opDirX = dirX[opponent.getDirection()];
            float opDirY = dirY[opponent.getDirection()];
            
            // Vector from opponent to player
            float diffX = px - ox;
            float diffY = py - oy;
            
            // Normalize vectors
            float diffLen = (float)Math.Sqrt(diffX * diffX + diffY * diffY);
            if (diffLen > 0.0001f)
            {
                diffX /= diffLen;
                diffY /= diffLen;
            }
            
            float opDirLen = (float)Math.Sqrt(opDirX * opDirX + opDirY * opDirY);
            if (opDirLen > 0.0001f)
            {
                opDirX /= opDirLen;
                opDirY /= opDirLen;
            }
            
            // Calculate angle
            float dot = diffX * opDirX + diffY * opDirY;
            float cross = diffX * opDirY - diffY * opDirX;
            
            float angle = (float)Math.Atan2(cross, dot);
            if (angle < 0) angle += 2.0f * (float)Math.PI;
            
            // Convert to sector (0-7)
            int sector = (int)((angle + Math.PI / 8.0f) / (Math.PI / 4.0f)) % 8;
            return sector;
        }
        
        private static Tuple<float, float>? CalculateIntersection(IPlayer player, IPlayer opponent)
        {
            // Calculate intersection timing for both players
            float px = player.getXpos();
            float py = player.getYpos();
            float ox = opponent.getXpos();
            float oy = opponent.getYpos();
            
            float[] dirX = { 0.0f, 1.0f, 0.0f, -1.0f };
            float[] dirY = { -1.0f, 0.0f, 1.0f, 0.0f };
            
            float pDirX = dirX[player.getDirection()] * player.getSpeed();
            float pDirY = dirY[player.getDirection()] * player.getSpeed();
            float oDirX = dirX[opponent.getDirection()] * opponent.getSpeed();
            float oDirY = dirY[opponent.getDirection()] * opponent.getSpeed();
            
            // Create perpendicular from player position
            float perpX = -oDirY;
            float perpY = oDirX;
            
            // Normalize perpendicular
            float perpLen = (float)Math.Sqrt(perpX * perpX + perpY * perpY);
            if (perpLen > 0.0001f)
            {
                perpX /= perpLen;
                perpY /= perpLen;
            }
            
            // Scale by player speed
            perpX *= player.getSpeed();
            perpY *= player.getSpeed();
            
            // Calculate intersection parameters
            float det = oDirX * perpY - oDirY * perpX;
            if (Math.Abs(det) < 0.0001f)
                return null; // Parallel paths
            
            float diffX = px - ox;
            float diffY = py - oy;
            
            float t_opponent = (diffX * perpY - diffY * perpX) / det;
            float t_player = (diffX * oDirY - diffY * oDirX) / det;
            
            if (t_player < 0) t_player = -t_player;
            
            return Tuple.Create(t_player, t_opponent);
        }
        
        private static void AiAction(int action, int playerIndex)
        {
            if (_players == null || _distances == null || _tdiff == null || _aiTime == null) return;
            
            var player = _players[playerIndex];
            
            // Calculate minimum safe distance based on speed and reaction time
            float reactionDistance = MIN_TURN_TIME[_aiLevel] * player.getSpeed() / 1000.0f;
            float safetyBuffer = 5.0f; // Minimum buffer from walls
            float saveDistance = Math.Max(reactionDistance + safetyBuffer, 10.0f);
            
            switch (action)
            {
                case 0: // No turn
                    break;
                case 1: // Turn left
                    if (_distances[E_LEFT] > saveDistance)
                    {
                        player.doTurn(Player.TURN_LEFT, _currentTime);
                        _tdiff[playerIndex]++;
                        _aiTime[playerIndex] = _currentTime;
                    }
                    break;
                case 2: // Turn right
                    if (_distances[E_RIGHT] > saveDistance)
                    {
                        player.doTurn(Player.TURN_RIGHT, _currentTime);
                        _tdiff[playerIndex]--;
                        _aiTime[playerIndex] = _currentTime;
                    }
                    break;
            }
        }
        
        private static void AiAggressive(int playerIndex, int targetIndex, int location)
        {
            if (_players == null) return;
            
            int playerDir = _players[playerIndex].getDirection();
            int targetDir = _players[targetIndex].getDirection();
            int dirDiff = (4 + playerDir - targetDir) % 4;
            
            AiAction(aggressive_action[location, dirDiff], playerIndex);
        }
        
        private static void AiEvasive(int playerIndex, int targetIndex, int location)
        {
            if (_players == null) return;
            
            int playerDir = _players[playerIndex].getDirection();
            int targetDir = _players[targetIndex].getDirection();
            int dirDiff = (4 + playerDir - targetDir) % 4;
            
            AiAction(evasive_action[location, dirDiff], playerIndex);
        }
        
        private static float GetSegmentLength(ISegment segment)
        {
            if (segment == null) return 0.0f;
            float dx = segment.vDirection.v[0];
            float dy = segment.vDirection.v[1];
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }
        
        public static IPlayer? GetClosestOpponent(IPlayer player, int ownPlayerIndex)
        {
            if (_players == null) return null;
            
            IPlayer? closest = null;
            float minDistance = float.MaxValue;
            
            float myX = player.getXpos();
            float myY = player.getYpos();
            
            for (int i = 0; i < _players.Length; i++)
            {
                if (i == ownPlayerIndex || _players[i] == null || _players[i].getSpeed() <= 0.0f) continue;
                
                float otherX = _players[i].getXpos();
                float otherY = _players[i].getYpos();
                float distance = (float)Math.Sqrt((otherX - myX) * (otherX - myX) + (otherY - myY) * (otherY - myY));
                
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = _players[i];
                }
            }
            
            return closest;
        }
    }
}
