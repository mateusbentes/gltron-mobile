using System;
using Microsoft.Xna.Framework;
using GltronMobileEngine.Interfaces;

namespace GltronMobileEngine
{
    /// <summary>
    /// Computer AI system for GLTron Mobile - Enhanced competitive version.
    /// This static class implements a grid-aware AI that:
    ///  - avoids walls and trails,
    ///  - chooses between simple and active strategies,
    ///  - uses timing, safety buffers and difficulty levels to decide,
    ///  - provides aggressive/evasive actions based on encounter geometry.
    /// 
    /// Notes:
    ///  - This AI expects external code to call InitAI(...) once,
    ///    UpdateTime(...) each tick (to update current time),
    ///    and DoComputer(playerIndex, targetIndex) for each CPU player.
    ///  - The code uses interfaces IPlayer and ISegment already present in the engine.
    /// </summary>
    public static class ComputerAI
    {
        // Direction indices (for ray tests)
        private const int E_FRONT = 0;
        private const int E_LEFT = 1;
        private const int E_RIGHT = 2;
        private const int E_BACKLEFT = 3;
        private const int E_MAX = 4;

        // Difficulty tables (tunable)
        private static readonly int[] MIN_TURN_TIME = { 600, 400, 200, 100 };
        private static readonly float[] MAX_SEG_LENGTH = { 0.6f, 0.3f, 0.3f, 0.3f };
        private static readonly float[] CRITICAL = { 0.2f, 0.08037f, 0.08037f, 0.08037f };
        private static readonly int[] SPIRAL = { 10, 10, 10, 10 };
        private static readonly float[] RL_DELTA = { 0f, 10f, 20f, 30f };

        // Strategic constants
        private const float SAVE_T_DIFF = 0.500f;
        private const float HOPELESS_T = 0.80f;
        private const float FLT_MAX = 10000.0f;
        private const float OPPONENT_CLOSE_DISTANCE = 48.0f;

        // Aggressive / evasive action tables
        // Actions: 0 = no turn, 1 = turn left, 2 = turn right
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

        // Game state (set in InitAI)
        private static ISegment[]? _walls;
        private static IPlayer[]? _players;
        private static float _gridSize;
        private static long _currentTime;

        // Per-player AI state (initialized in InitAI)
        private static long[]? _aiTime;
        private static int[]? _tdiff;
        private static float[]? _distances; // shared scratch for direction distances (size E_MAX)

        // AI difficulty level (0..3)
        private static int _aiLevel = 3;

        // Random for tie-breaking and gentle randomness
        private static readonly Random _random = new Random();

        /// <summary>
        /// Initialize AI with world segments (walls), players and grid size.
        /// Must be called before using other methods.
        /// </summary>
        public static void InitAI(ISegment[] walls, IPlayer[] players, float gridSize)
        {
            try
            {
                _walls = walls ?? throw new ArgumentNullException(nameof(walls));
                _players = players ?? throw new ArgumentNullException(nameof(players));
                _gridSize = gridSize;

                _aiTime = new long[_players.Length];
                _tdiff = new int[_players.Length];
                _distances = new float[E_MAX];

                System.Diagnostics.Debug.WriteLine($"GLTRON: Competitive AI initialized for {_players.Length} players on grid {gridSize}");
#if ANDROID
                try { Android.Util.Log.Info("GLTRON", $"Competitive AI initialized for {_players.Length} players on grid {gridSize}"); } catch { }
#endif
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GLTRON: AI initialization failed: {ex}");
                throw;
            }
        }

        /// <summary>
        /// Update current time - should be called each engine tick (in ms).
        /// </summary>
        public static void UpdateTime(long deltaTime, long currentTime)
        {
            _currentTime = currentTime;
        }

        /// <summary>
        /// Primary CPU decision entry for a single player.
        /// playerIndex = the index of the AI player to process.
        /// targetIndex = currently targeted player (may be -1).
        /// </summary>
        public static void DoComputer(int playerIndex, int targetIndex)
        {
            if (_players == null || _walls == null || _aiTime == null || _distances == null) return;
            if (playerIndex < 0 || playerIndex >= _players.Length) return;
            if (playerIndex == targetIndex) return;

            var player = _players[playerIndex];
            if (player == null) return;
            if (player.getSpeed() <= 0.0f) return;

            // Throttle turns by difficulty-specific minimum turn time
            if ((_currentTime - _aiTime[playerIndex]) < MIN_TURN_TIME[_aiLevel]) return;

            try
            {
                // Compute distances (front/left/right/backleft) using raycasts/intersections
                CalculateDistances(playerIndex);

                // Find closest opponent and its approximate Manhattan distance
                int closestOpp = GetClosestOpponent(playerIndex);
                float distanceToOpp = GetOpponentDistance(playerIndex, closestOpp);

                // If no close opponent or front distance is shorter than opponent distance -> simple behavior
                if (closestOpp == -1 || distanceToOpp > OPPONENT_CLOSE_DISTANCE || _distances[E_FRONT] < distanceToOpp)
                {
                    DoComputerSimple(playerIndex, targetIndex);
                }
                else
                {
                    DoComputerActive(playerIndex, closestOpp);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GLTRON: AI error for player {playerIndex}: {ex}");
            }
        }

        /// <summary>
        /// Build rays and compute intersection distances with walls and trails.
        /// Results are stored in _distances[E_*].
        /// </summary>
        private static void CalculateDistances(int playerIndex)
        {
            if (_players == null || _distances == null) return;
            
            var player = _players[playerIndex];
            if (player == null) return;

            int currDir = player.getDirection();
            int dirLeft = (currDir + 3) % 4;
            int dirRight = (currDir + 1) % 4;

            float x = player.getXpos();
            float y = player.getYpos();

            // Unit direction vectors indexed by direction 0..3
            float[] dirX = { 0.0f, 1.0f, 0.0f, -1.0f };
            float[] dirY = { -1.0f, 0.0f, 1.0f, 0.0f };

            // Prepare rays (as Segment helper)
            var segments = new Segment[E_MAX];
            for (int i = 0; i < E_MAX; i++)
            {
                segments[i] = new Segment();
                segments[i].vStart = new Vec(x, y, 0);
            }

            // Ray length large enough to hit any relevant obstacle in the grid
            float rayLength = _gridSize * 4.0f; // slightly larger than before for better look-ahead
            segments[E_FRONT].vDirection = new Vec(dirX[currDir] * rayLength, dirY[currDir] * rayLength, 0);
            segments[E_LEFT].vDirection = new Vec(dirX[dirLeft] * rayLength, dirY[dirLeft] * rayLength, 0);
            segments[E_RIGHT].vDirection = new Vec(dirX[dirRight] * rayLength, dirY[dirRight] * rayLength, 0);
            // back-left diagonal for spiral/backtracking detection
            segments[E_BACKLEFT].vDirection = new Vec(
                (dirX[dirLeft] - dirX[currDir]) * rayLength,
                (dirY[dirLeft] - dirY[currDir]) * rayLength,
                0
            );

            // Initialize distances to large value
            for (int i = 0; i < E_MAX; i++) _distances[i] = FLT_MAX;

            // Check intersections against world walls first
            if (_walls != null)
            {
                foreach (var wall in _walls)
                {
                    if (wall == null) continue;
                    CheckSegmentIntersection(segments[E_FRONT], wall, ref _distances[E_FRONT]);
                    CheckSegmentIntersection(segments[E_LEFT], wall, ref _distances[E_LEFT]);
                    CheckSegmentIntersection(segments[E_RIGHT], wall, ref _distances[E_RIGHT]);
                    CheckSegmentIntersection(segments[E_BACKLEFT], wall, ref _distances[E_BACKLEFT]);
                }
            }

            // Then check all trails from players (skip inactive ones)
            if (_players != null)
            {
                for (int i = 0; i < _players.Length; i++)
                {
                    var p = _players[i];
                    if (p == null) continue;
                    if (p.getTrailHeight() < Player.TRAIL_HEIGHT) continue;
                    int trailCount = p.getTrailOffset() + 1;
                    for (int t = 0; t < trailCount; t++)
                    {
                        // skip the player's current last segment when comparing against itself
                        if (i == playerIndex && t == p.getTrailOffset()) break;
                        var trail = p.getTrail(t);
                        if (trail == null) continue;
                        CheckSegmentIntersection(segments[E_FRONT], trail, ref _distances[E_FRONT]);
                        CheckSegmentIntersection(segments[E_LEFT], trail, ref _distances[E_LEFT]);
                        CheckSegmentIntersection(segments[E_RIGHT], trail, ref _distances[E_RIGHT]);
                        CheckSegmentIntersection(segments[E_BACKLEFT], trail, ref _distances[E_BACKLEFT]);
                    }
                }
            }

            // Debug alert for very small distances
            if (_distances[E_FRONT] < 8.0f || _distances[E_LEFT] < 8.0f || _distances[E_RIGHT] < 8.0f)
            {
                System.Diagnostics.Debug.WriteLine($"AI {playerIndex} ALERT: F={_distances[E_FRONT]:F1} L={_distances[E_LEFT]:F1} R={_distances[E_RIGHT]:F1}");
            }
        }

        /// <summary>
        /// Check intersection between a ray and an ISegment (wall or trail).
        /// If hit, update the provided distance (minimum).
        /// </summary>
        private static void CheckSegmentIntersection(Segment ray, ISegment wall, ref float distance)
        {
            if (ray == null || wall == null) return;
            var hit = ray.Intersect(wall);
            if (hit != null)
            {
                float t1 = ray.t1;
                float t2 = ray.t2;
                
                // t1 is the parameter along the ray (0 to 1 for full ray length)
                // t2 is the parameter along the wall (0 to 1 for wall segment)
                if (t1 > 0.0f && t2 >= 0.0f && t2 <= 1.0f)
                {
                    float rayLen = ray.vDirection.Length();
                    float actualDistance = t1 * rayLen;
                    if (actualDistance < distance) distance = actualDistance;
                }
            }
        }

        /// <summary>
        /// Return index of the closest alive opponent (by Manhattan metric), or -1 if none found.
        /// </summary>
        private static int GetClosestOpponent(int playerIndex)
        {
            if (_players == null) return -1;
            var me = _players[playerIndex];
            if (me == null) return -1;

            float best = FLT_MAX;
            int bestIdx = -1;
            for (int i = 0; i < _players.Length; i++)
            {
                if (i == playerIndex) continue;
                var p = _players[i];
                if (p == null || p.getSpeed() <= 0.0f) continue;
                float dx = Math.Abs(p.getXpos() - me.getXpos());
                float dy = Math.Abs(p.getYpos() - me.getYpos());
                float d = dx + dy; // Manhattan - cheap and effective
                if (d < best)
                {
                    best = d;
                    bestIdx = i;
                }
            }
            return bestIdx;
        }

        /// <summary>
        /// Return Manhattan-like distance to opponent or FLT_MAX if invalid.
        /// </summary>
        private static float GetOpponentDistance(int playerIndex, int opponentIndex)
        {
            if (_players == null) return FLT_MAX;
            if (opponentIndex < 0 || opponentIndex >= _players.Length) return FLT_MAX;
            var me = _players[playerIndex];
            var opp = _players[opponentIndex];
            if (me == null || opp == null || opp.getSpeed() <= 0.0f) return FLT_MAX;
            float dx = Math.Abs(opp.getXpos() - me.getXpos());
            float dy = Math.Abs(opp.getYpos() - me.getYpos());
            return dx + dy;
        }

        /// <summary>
        /// Set AI difficulty (0..3).
        /// </summary>
        public static void SetAiLevel(int level)
        {
            _aiLevel = Math.Clamp(level, 0, 3);
            System.Diagnostics.Debug.WriteLine($"GLTRON: AI level set to {_aiLevel}");
        }

        // -------------------------
        // Segment helper class: simple parametric segment-ray intersection
        // -------------------------
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

            // Intersect this ray (parametric t1) with another segment (parametric t2)
            public Vec? Intersect(ISegment other)
            {
                if (other == null) return null;

                float sx = other.vStart.v[0];
                float sy = other.vStart.v[1];
                float dx = other.vDirection.v[0];
                float dy = other.vDirection.v[1];

                float ax = vDirection.v[0];
                float ay = vDirection.v[1];

                // Solve: vStart + t1 * vDirection = otherStart + t2 * otherDir
                float det = ax * (-dy) - ay * (-dx);
                if (Math.Abs(det) < 1e-6f) return null; // parallel or degenerate

                float diffX = sx - vStart.v[0];
                float diffY = sy - vStart.v[1];

                t1 = (diffX * (-dy) - diffY * (-dx)) / det;
                t2 = (diffX * ay - diffY * ax) / det;

                if (t1 >= 0f && t2 >= 0f && t2 <= 1.0f)
                {
                    return new Vec(
                        vStart.v[0] + t1 * ax,
                        vStart.v[1] + t1 * ay,
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

        // -------------------------
        // Simple mode: local, safety-first decisions
        // -------------------------
        private static void DoComputerSimple(int playerIndex, int targetIndex)
        {
            if (_players == null || _distances == null || _tdiff == null || _aiTime == null) return;
            
            var player = _players[playerIndex];
            if (player == null) return;

            float front = _distances[E_FRONT];
            float left = _distances[E_LEFT];
            float right = _distances[E_RIGHT];
            float backleft = _distances[E_BACKLEFT];

            bool shouldBeAggressive = ShouldBeAggressive(playerIndex, front, left, right);

            float criticalDist = CRITICAL[_aiLevel] * _gridSize;
            float maxSegLength = MAX_SEG_LENGTH[_aiLevel] * _gridSize;
            ISegment? trailSegment = null;
            try { trailSegment = player.getTrail(player.getTrailOffset()); } catch { }
            float segmentLength = GetSegmentLength(trailSegment);

            float speed = player.getSpeed();
            float reactionTime = MIN_TURN_TIME[_aiLevel] / 1000.0f;
            float stoppingDistance = speed * reactionTime;
            float safetyBuffer = 3.0f;
            float dynamicSafetyMargin = stoppingDistance + safetyBuffer;

            // Emergency evasion if front is too close
            if (front < dynamicSafetyMargin)
            {
                // Emergency evasion - turn immediately to the side with more space
                if (left > right && left > dynamicSafetyMargin)
                {
                    player.doTurn(Player.TURN_LEFT, _currentTime);
                    _tdiff[playerIndex]++;
                    _aiTime[playerIndex] = _currentTime;
                    return;
                }
                else if (right > dynamicSafetyMargin)
                {
                    player.doTurn(Player.TURN_RIGHT, _currentTime);
                    _tdiff[playerIndex]--;
                    _aiTime[playerIndex] = _currentTime;
                    return;
                }
                // If both sides are bad, pick the better one
            }

            // If safe to continue straight, do so
            if (front > criticalDist && segmentLength < maxSegLength && front > dynamicSafetyMargin * 2.0f)
            {
                return;
            }

            // Evaluate turn options
            float leftScore = EvaluateTurnOption(playerIndex, Player.TURN_LEFT);
            float rightScore = EvaluateTurnOption(playerIndex, Player.TURN_RIGHT);

            // Anti-spiral heuristics
            bool preferLeft = false;
            bool preferRight = false;
            int tdiff = _tdiff[playerIndex];
            if (tdiff > SPIRAL[_aiLevel] / 2) preferLeft = true;
            else if (tdiff < -SPIRAL[_aiLevel] / 2) preferRight = true;

            float minTurnDistance = Math.Max(dynamicSafetyMargin, 5.0f);

            if (left > minTurnDistance && right > minTurnDistance)
            {
                // Both safe: choose based on aggression, scores and tie-breaking
                if (shouldBeAggressive && Math.Abs(leftScore - rightScore) < 5.0f)
                {
                    // Aggressive mode: alternate turns for unpredictability
                    if (tdiff >= 0)
                    {
                        player.doTurn(Player.TURN_LEFT, _currentTime);
                        _tdiff[playerIndex]++;
                    }
                    else
                    {
                        player.doTurn(Player.TURN_RIGHT, _currentTime);
                        _tdiff[playerIndex]--;
                    }
                }
                else if (leftScore > rightScore || (Math.Abs(leftScore - rightScore) < 1e-3f && preferLeft))
                {
                    player.doTurn(Player.TURN_LEFT, _currentTime);
                    _tdiff[playerIndex]++;
                }
                else
                {
                    player.doTurn(Player.TURN_RIGHT, _currentTime);
                    _tdiff[playerIndex]--;
                }
                _aiTime[playerIndex] = _currentTime;
            }
            else if (left > minTurnDistance)
            {
                // Only left is safe
                player.doTurn(Player.TURN_LEFT, _currentTime);
                _tdiff[playerIndex]++;
                _aiTime[playerIndex] = _currentTime;
            }
            else if (right > minTurnDistance)
            {
                // Only right is safe
                player.doTurn(Player.TURN_RIGHT, _currentTime);
                _tdiff[playerIndex]--;
                _aiTime[playerIndex] = _currentTime;
            }
            else
            {
                // No safe options: choose least bad or do nothing
                if (left > right && left > 0)
                {
                    player.doTurn(Player.TURN_LEFT, _currentTime);
                    _tdiff[playerIndex]++;
                    _aiTime[playerIndex] = _currentTime;
                }
                else if (right > 0)
                {
                    player.doTurn(Player.TURN_RIGHT, _currentTime);
                    _tdiff[playerIndex]--;
                    _aiTime[playerIndex] = _currentTime;
                }
                // otherwise no choice
            }
        }

        // -------------------------
        // Active mode: strategic decisions when an opponent is near
        // -------------------------
        private static void DoComputerActive(int playerIndex, int targetIndex)
        {
            if (_players == null || _aiTime == null) return;
            
            var player = _players[playerIndex];
            if (player == null) return;

            var target = (targetIndex >= 0 && targetIndex < _players.Length) ? _players[targetIndex] : null;
            if (target == null || target.getSpeed() <= 0.0f)
            {
                DoComputerSimple(playerIndex, targetIndex);
                return;
            }

            int location = CalculateSector(player, target);
            var intersection = CalculateIntersection(player, target);
            
            if (intersection == null)
            {
                DoComputerSimple(playerIndex, targetIndex);
                return;
            }

            float t_player = intersection.Item1;
            float t_opponent = intersection.Item2;

            // Choose behavior based on sector and timing
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
                default:
                    DoComputerSimple(playerIndex, targetIndex);
                    break;
            }
        }

        /// <summary>
        /// Calculate which octant (0..7) the player is relative to the opponent's facing direction.
        /// Useful for deciding head-on vs flank scenarios.
        /// </summary>
        private static int CalculateSector(IPlayer player, IPlayer opponent)
        {
            // Calculate relative position sector (0-7, like compass directions)
            float px = player.getXpos();
            float py = player.getYpos();
            float ox = opponent.getXpos();
            float oy = opponent.getYpos();

            float[] dirX = { 0.0f, 1.0f, 0.0f, -1.0f };
            float[] dirY = { -1.0f, 0.0f, 1.0f, 0.0f };

            float opDirX = dirX[opponent.getDirection()];
            float opDirY = dirY[opponent.getDirection()];

            float diffX = px - ox;
            float diffY = py - oy;
            
            // Normalize vectors
            float diffLen = (float)Math.Sqrt(diffX * diffX + diffY * diffY);
            if (diffLen > 1e-6f) { diffX /= diffLen; diffY /= diffLen; }

            float opLen = (float)Math.Sqrt(opDirX * opDirX + opDirY * opDirY);
            if (opLen > 1e-6f) { opDirX /= opLen; opDirY /= opLen; }

            // Dot and cross for angle between vectors
            float dot = diffX * opDirX + diffY * opDirY;
            float cross = diffX * opDirY - diffY * opDirX;
            
            float angle = (float)Math.Atan2(cross, dot);
            if (angle < 0) angle += 2.0f * (float)Math.PI;

            int sector = (int)((angle + Math.PI / 8.0f) / (Math.PI / 4.0f)) % 8;
            return sector;
        }

        /// <summary>
        /// Return estimated intersection times (t_player, t_opponent) if paths cross.
        /// Values are positive when intersection is ahead; may return null for parallel.
        /// </summary>
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

            // Build perpendicular to opponent direction scaled by player speed
            float perpX = -oDirY;
            float perpY = oDirX;
            
            // Normalize perpendicular
            float perpLen = (float)Math.Sqrt(perpX * perpX + perpY * perpY);
            if (perpLen > 1e-6f) { perpX /= perpLen; perpY /= perpLen; }
            perpX *= player.getSpeed();
            perpY *= player.getSpeed();

            float det = oDirX * perpY - oDirY * perpX;
            if (Math.Abs(det) < 1e-6f) return null; // nearly parallel

            float diffX = px - ox;
            float diffY = py - oy;

            float t_opponent = (diffX * perpY - diffY * perpX) / det;
            float t_player = (diffX * oDirY - diffY * oDirX) / det;
            
            if (t_player < 0) t_player = -t_player;

            return Tuple.Create(t_player, t_opponent);
        }

        /// <summary>
        /// Safely perform an AI action (0=no turn,1=left,2=right) with safety checks.
        /// </summary>
        private static void AiAction(int action, int playerIndex)
        {
            if (_players == null || _distances == null || _tdiff == null || _aiTime == null) return;
            
            var player = _players[playerIndex];
            if (player == null) return;

            float reactionDistance = MIN_TURN_TIME[_aiLevel] * player.getSpeed() / 1000.0f;
            float safetyBuffer = 5.0f;
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

        /// <summary>
        /// Safe getter for segment length (handles null).
        /// </summary>
        private static float GetSegmentLength(ISegment? segment)
        {
            if (segment == null) return 0.0f;
            float dx = segment.vDirection.v[0];
            float dy = segment.vDirection.v[1];
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// Decide whether the AI should be aggressive, based on local space and nearby opponents.
        /// </summary>
        private static bool ShouldBeAggressive(int playerIndex, float front, float left, float right)
        {
            if (_players == null) return false;
            
            // Be aggressive if we have enough space
            float minSafeDistance = 15.0f;
            if (front < minSafeDistance && left < minSafeDistance && right < minSafeDistance) return false;

            var me = _players[playerIndex];
            if (me == null) return false;

            for (int i = 0; i < _players.Length; i++)
            {
                if (i == playerIndex) continue;
                var p = _players[i];
                if (p == null || p.getSpeed() <= 0.0f) continue;
                float dx = p.getXpos() - me.getXpos();
                float dy = p.getYpos() - me.getYpos();
                float dist = (float)Math.Sqrt(dx * dx + dy * dy);
                if (dist < 30.0f && Math.Max(front, Math.Max(left, right)) > 20.0f) return true;
            }
            
            return false;
        }

        /// <summary>
        /// Evaluate how desirable turning in turnDirection (Player.TURN_LEFT/RIGHT) is.
        /// Higher is better.
        /// </summary>
        private static float EvaluateTurnOption(int playerIndex, int turnDirection)
        {
            if (_players == null || _distances == null) return 0.0f;
            
            // Evaluate how good a turn option is
            float distance = (turnDirection == Player.TURN_LEFT) ? _distances[E_LEFT] : _distances[E_RIGHT];
            
            // Base score is the distance available
            float score = distance;
            
            // Bonus for avoiding walls
            if (distance > 30.0f) score += 10.0f;
            if (distance > 50.0f) score += 20.0f;
            
            // Penalty for getting too close to walls
            if (distance < 10.0f) score -= 50.0f;
            if (distance < 5.0f) score -= 100.0f;
            
            return score;
        }

        /// <summary>
        /// Utility: find closest opponent to given player instance (alternative API).
        /// </summary>
        public static IPlayer? GetClosestOpponent(IPlayer player, int ownPlayerIndex)
        {
            if (_players == null) return null;
            
            IPlayer? closest = null;
            float minDistance = float.MaxValue;
            
            float myX = player.getXpos();
            float myY = player.getYpos();

            for (int i = 0; i < _players.Length; i++)
            {
                if (i == ownPlayerIndex) continue;
                var p = _players[i];
                if (p == null || p.getSpeed() <= 0.0f) continue;
                float otherX = p.getXpos();
                float otherY = p.getYpos();
                float distance = (float)Math.Sqrt((otherX - myX) * (otherX - myX) + (otherY - myY) * (otherY - myY));
                
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = p;
                }
            }
            
            return closest;
        }
    }
}
