using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GltronMobileEngine.Video;
using GltronMobileEngine.Sound;

namespace GltronMobileEngine
{
    /// <summary>
    /// Multiplatform Player class for GLTron Mobile
    /// Compatible with Android, iOS, and other MonoGame platforms
    /// </summary>
    public class Player : Interfaces.IPlayer
    {
        // Constants and variables from Java Player.java - multiplatform compatible
#pragma warning disable CS0169 // Field is never used - planned for future 3D model implementation
        private SimpleModel? Cycle; // Will be replaced by MonoGame model system
#pragma warning restore CS0169
        private int Player_num;
        private int Direction;
        private int LastDirection;
        
        // Explosion system - platform agnostic
        private bool _exploding = false;
#pragma warning disable CS0414 // Field is assigned but never used - planned for explosion animation
        private float _explodeTimer = 0f;
#pragma warning restore CS0414

        private int Score;

        // Trail system - multiplatform compatible
        private Segment[] Trails = new Segment[1000];

        // private HUD tronHUD; // Substituir por um sistema de HUD MonoGame

        private int trailOffset;
        private float trailHeight;

        private float Speed;
        public long TurnTime;
        public readonly float[] DIRS_X = { 0.0f, -1.0f, 0.0f, 1.0f };
        public readonly float[] DIRS_Y = { -1.0f, 0.0f, 1.0f, 0.0f };
        private const float SPEED_OZ_FREQ = 1200.0f;
        private const float SPEED_OZ_FACTOR = 0.09f;

        private readonly float[] dirangles = { 0.0f, -90.0f, -180.0f, 90.0f, 180.0f, -270.0f };
        public const int TURN_LEFT = 3;
        public const int TURN_RIGHT = 1;
        public const int TURN_LENGTH = 200;
        public const float TRAIL_HEIGHT = 1.5f; // Reduced from 3.5f to be proportional to larger motorcycles
        private const float EXP_RADIUS_MAX = 30.0f;
        private const float EXP_RADIUS_DELTA = 0.01f;

        // private TrailMesh Trailmesh; // Substituir por um sistema de trilhas MonoGame

        private float exp_radius;

        // CRITICAL FIX: Match Java START_POS exactly
        private readonly float[,] START_POS = {
            { 0.5f, 0.25f },   // Player 0 - Java: { 0.5f, 0.25f}
            { 0.75f, 0.5f },   // Player 1 - Java: {0.75f, 0.5f}
            { 0.5f, 0.4f },    // Player 2 - Java: {0.5f, 0.4f}
            { 0.25f, 0.5f },   // Player 3 - Java: {0.25f, 0.5f}
            { 0.25f, 0.25f },  // Player 4 - Java: {0.25f, 0.25f}
            { 0.65f, 0.35f }   // Player 5 - Java: {0.65f, 0.35f}
        };

        // Cores serão tratadas de forma diferente no MonoGame
        // private readonly float[,] ColourDiffuse = { ... };
        // private readonly float[,] ColourSpecular = { ... };
        // private readonly float[,] ColourAlpha = { ... };

        private static bool[] ColourTaken = { false, false, false, false, false, false };
        private int mPlayerColourIndex;

        // private readonly int[,] LOD_DIST = { ... };

        public Player(int player_number, float gridSize) // Simplificado, HUD e Model serão injetados depois
        {
            int colour = 0;
            bool done = false;

            // CRITICAL FIX: Use random direction like Java version
            // Java: Direction = rand.nextInt(3); // accepts values 0..3
            Random rand = new Random(player_number + (int)DateTime.Now.Ticks); // Seed with player number for variety
            Direction = rand.Next(4); // 0, 1, 2, or 3 like Java
            LastDirection = Direction;

            Trails[0] = new Segment();
            trailOffset = 0;
            
            // CRITICAL FIX: Use safe spawn position (will be set by SetSafeSpawnPosition)
            Trails[trailOffset].vStart.v[0] = START_POS[player_number, 0] * gridSize;
            Trails[trailOffset].vStart.v[1] = START_POS[player_number, 1] * gridSize;
            // CRITICAL FIX: Start with tiny visible trail segment for immediate visibility
            Trails[trailOffset].vDirection.v[0] = 0.01f * DIRS_X[Direction];
            Trails[trailOffset].vDirection.v[1] = 0.01f * DIRS_Y[Direction];

            trailHeight = TRAIL_HEIGHT; // Start with full trail height immediately

            // tronHUD = hud; // Removido por enquanto

            // Slower default speed for better control
            Speed = 6.0f;
            exp_radius = 0.0f;

            // Cycle = mesh; // Removido por enquanto
            Player_num = player_number;
            Score = 0;

            // CRITICAL FIX: Color selection logic like Java version
            if (player_number == 0) // OWN_PLAYER
            {
                // Reset color array for first player (like Java version)
                for (colour = 0; colour < 6; colour++)
                {
                    ColourTaken[colour] = false;
                }

                // Player 0 gets blue (index 0) by default
                mPlayerColourIndex = 0; // Blue for human player
                ColourTaken[mPlayerColourIndex] = true;
            }
            else
            {
                // AI players get different colors (like Java version)
                while (!done && colour < 6)
                {
                    if (!ColourTaken[colour])
                    {
                        ColourTaken[colour] = true;
                        mPlayerColourIndex = colour;
                        done = true;
                    }
                    colour++;
                }
                
                // Fallback if all colors taken
                if (!done)
                {
                    mPlayerColourIndex = player_number % 6;
                }
            }
        }

        public void doTurn(int direction, long current_time)
        {
            float x = getXpos();
            float y = getYpos();

            // Prevent array bounds exception
            if (trailOffset >= Trails.Length - 1)
            {
                // Multiplatform logging
                System.Diagnostics.Debug.WriteLine("GLTRON: Trail array full, resetting player");
                
                // Platform-specific logging if available
                try
                {
#if ANDROID
                    Android.Util.Log.Warn("GLTRON", "Trail array full, resetting player");
#endif
                }
                catch { /* Ignore platform-specific logging errors */ }
                
                // Reset trail when array is full
                trailOffset = 0;
                Speed = 0.0f; // Stop player to prevent further issues
                return;
            }

            // CRITICAL FIX: Finalize current trail segment before creating new one
            if (trailOffset >= 0 && Trails[trailOffset] != null)
            {
                // Current segment ends at current position
                float currentSegmentLength = Math.Abs(Trails[trailOffset].vDirection.v[0]) + Math.Abs(Trails[trailOffset].vDirection.v[1]);
                System.Diagnostics.Debug.WriteLine($"GLTRON: Player {Player_num} finalizing segment {trailOffset} with length {currentSegmentLength:F2}");
            }

            trailOffset++;
            Trails[trailOffset] = new Segment();
            Trails[trailOffset].vStart.v[0] = x;
            Trails[trailOffset].vStart.v[1] = y;
            Trails[trailOffset].vDirection.v[0] = 0.0f;
            Trails[trailOffset].vDirection.v[1] = 0.0f;

            LastDirection = Direction;
            Direction = (Direction + direction) % 4;
            TurnTime = current_time;
            
            System.Diagnostics.Debug.WriteLine($"GLTRON: Player {Player_num} turned from direction {LastDirection} to {Direction} at ({x:F1},{y:F1})");
            
            // Sound feedback for turns (like Java version)
            try
            {
                SoundManager.Instance.PlayEngine(1.3f, true); // Boost engine sound on turn
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GLTRON: Turn sound error: {ex.Message}");
            }
        }

        public void doMovement(long dt, long current_time, Interfaces.ISegment[] walls, Interfaces.IPlayer[] players)
        {
            // Work entirely against the IPlayer interface to avoid invalid casts
            DoMovementInternal(dt, current_time, walls, players);
        }

        private void DoMovementInternal(long dt, long current_time, Interfaces.ISegment[] walls, Interfaces.IPlayer[] players)
        {
            float fs;
            float t;

            if (Speed > 0.0f) // Player is still alive
            {
                // Lógica de oscilação de velocidade (SPEED_OZ)
                fs = (float)(1.0f - SPEED_OZ_FACTOR + SPEED_OZ_FACTOR *
                    Math.Cos(0.0f * (float)Math.PI / 4.0f +
                               (current_time % SPEED_OZ_FREQ) *
                               2.0f * Math.PI / SPEED_OZ_FREQ));

                t = dt / 100.0f * Speed * fs;

                // CRITICAL FIX: Ensure trail direction vectors are properly accumulated
                float deltaX = t * DIRS_X[Direction];
                float deltaY = t * DIRS_Y[Direction];
                
                Trails[trailOffset].vDirection.v[0] += deltaX;
                Trails[trailOffset].vDirection.v[1] += deltaY;
                
                // CRITICAL FIX: Ensure trail height is always at maximum when player is moving
                if (trailHeight < TRAIL_HEIGHT)
                {
                    trailHeight = TRAIL_HEIGHT; // Instantly restore full trail height
                }
                
                // Debug trail growth
                if (trailOffset % 10 == 0) // Log every 10th update to avoid spam
                {
                    float segLength = Math.Abs(Trails[trailOffset].vDirection.v[0]) + Math.Abs(Trails[trailOffset].vDirection.v[1]);
                    System.Diagnostics.Debug.WriteLine($"GLTRON: Player {Player_num} segment {trailOffset} length: {segLength:F2}, delta: ({deltaX:F3},{deltaY:F3})");
                }

                // Debug: Check if player is going outside expected bounds (use actual grid size)
                float currentX = getXpos();
                float currentY = getYpos();
                
                // Note: Grid size should come from game, not hardcoded to 100
                // This is just for debugging - actual collision is handled by wall collision detection

                // First, test explicit wall segment intersections
                doCrashTestWalls(walls);
                // Safety net: bounds check in case segment math misses exact boundary contact
                try
                {
                    float gx = getXpos();
                    float gy = getYpos();
                    float gridSize = 100f; // default
                    // Try to infer grid from walls
                    if (walls != null && walls.Length >= 2 && walls[1] is Segment rw)
                    {
                        // right wall starts at (size,0)
                        gridSize = Math.Max(gridSize, Math.Max(rw.vStart.v[0], rw.vStart.v[1]));
                        gridSize = Math.Max(gridSize, Math.Abs(rw.vDirection.v[0]) + Math.Abs(rw.vDirection.v[1]));
                    }
                    const float eps = 0.01f;
                    if (gx < -eps || gy < -eps || gx > gridSize + eps || gy > gridSize + eps)
                    {
                        // Clamp endpoint to boundary for visual consistency
                        gx = Math.Clamp(gx, 0f, gridSize);
                        gy = Math.Clamp(gy, 0f, gridSize);
                        var Current = Trails[trailOffset];
                        Current.vDirection.v[0] = gx - Current.vStart.v[0];
                        Current.vDirection.v[1] = gy - Current.vStart.v[1];
                        Speed = 0.0f;
                        _exploding = true;
                        _explodeTimer = 0f;
                        try { SoundManager.Instance.PlayCrash(); SoundManager.Instance.StopEngine(); } catch (System.Exception ex) { System.Diagnostics.Debug.WriteLine($"GLTRON: Sound system error: {ex.Message}"); }
                        LogCrash($"Player {Player_num} CRASH wall (bounds)!");
                    }
                }
                catch { }

                doCrashTestPlayer(players);

            }
            else
            {
                if (trailHeight > 0.0f)
                {
                    trailHeight -= (dt * TRAIL_HEIGHT) / 1000.0f;
                }
                if (exp_radius < EXP_RADIUS_MAX)
                {
                    exp_radius += (dt * EXP_RADIUS_DELTA);
                }
            }
        }

        // Getters
        public float getXpos()
        {
            return Trails[trailOffset].vStart.v[0] + Trails[trailOffset].vDirection.v[0];
        }

        public float getYpos()
        {
            return Trails[trailOffset].vStart.v[1] + Trails[trailOffset].vDirection.v[1];
        }

        public float getSpeed()
        {
            return Speed;
        }

        public void setSpeed(float speed)
        {
            Speed = speed;
            
            // CRITICAL FIX: When speed is set (game start), ensure trail is immediately visible
            if (speed > 0.0f && trailHeight < TRAIL_HEIGHT)
            {
                trailHeight = TRAIL_HEIGHT; // Instant full trail height
                
                // Ensure current trail segment has some length for immediate visibility
                if (trailOffset >= 0 && Trails[trailOffset] != null)
                {
                    float currentLength = Math.Abs(Trails[trailOffset].vDirection.v[0]) + Math.Abs(Trails[trailOffset].vDirection.v[1]);
                    if (currentLength < 0.01f)
                    {
                        // Add tiny visible segment in current direction
                        Trails[trailOffset].vDirection.v[0] = 0.01f * DIRS_X[Direction];
                        Trails[trailOffset].vDirection.v[1] = 0.01f * DIRS_Y[Direction];
                    }
                }
            }
        }

        public float getTrailHeight()
        {
            return trailHeight;
        }

        public int getTrailOffset()
        {
            return trailOffset;
        }

        public Segment getTrail(int offset)
        {
            return Trails[offset];
        }

        // Interface implementation - returns ISegment
        Interfaces.ISegment Interfaces.IPlayer.getTrail(int index)
        {
            return getTrail(index);
        }

        public int getDirection()
        {
            return Direction;
        }

        public int getLastDirection()
        {
            return LastDirection;
        }

        public bool getExplode()
        {
            return _exploding;
        }

        public void setExplodeTex(object texture)
        {
            // Platform-agnostic texture setting
            // Implementation will depend on the rendering system used
            // For now, just log that texture was set
            System.Diagnostics.Debug.WriteLine($"GLTRON: Player {Player_num} explosion texture set");
        }

        public bool isVisible()
        {
            // Basic visibility check - player is visible if speed > 0 or still exploding
            return Speed > 0.0f || _exploding || trailHeight > 0.0f;
        }

        public int getPlayerNum()
        {
            return Player_num;
        }
        
        public int getPlayerColorIndex()
        {
            return mPlayerColourIndex;
        }

        public int getScore()
        {
            return Score;
        }

        public void addScore(int score)
        {
            Score += score;
        }

        public void doCrashTestWalls(Interfaces.ISegment[] walls)
        {
            if (walls == null || Speed == 0.0f) return;

            Segment Current = Trails[trailOffset];
            Vec? V;

            for (int j = 0; j < walls.Length; j++)
            {
                if (walls[j] == null) continue;
                Segment Wall = walls[j] as Segment;
                if (Wall == null) continue;

                V = (Wall is Segment seg) ? Current.Intersect(seg) : null;
                if (V != null)
                {
                    // Accept collision with wall when our moving segment crosses it anywhere except exactly at our own start point.
                    // For walls we also accept hits on the wall endpoints (t2 == 1 allowed).
                    bool hitCurrentSegment = Current.t1 > 0.0f && Current.t1 < 1.0f; // strictly inside our segment progression
                    bool onWallSegment = Wall.t1 >= 0.0f && Wall.t1 <= 1.0f; // use wall's parameter (t over Wall) if set by Intersect
                    // Some implementations store other segment's param in Current.t2; preserve existing behavior as fallback
                    bool onWallViaCurrent = Current.t2 >= 0.0f && Current.t2 <= 1.0f;

                    if ((hitCurrentSegment && onWallViaCurrent) || (hitCurrentSegment && onWallSegment))
                    {
                        Current.vDirection.v[0] = V.v[0] - Current.vStart.v[0];
                        Current.vDirection.v[1] = V.v[1] - Current.vStart.v[1];
                        Speed = 0.0f;
                        _exploding = true;
                        _explodeTimer = 0f;
                        try { SoundManager.Instance.PlayCrash(); SoundManager.Instance.StopEngine(); } catch (System.Exception ex) { System.Diagnostics.Debug.WriteLine($"GLTRON: Sound system error: {ex.Message}"); }
                        LogCrash($"Player {Player_num} CRASH wall {j}!");
                        break;
                    }
                }
            }
        }

        public void doCrashTestPlayer(Interfaces.IPlayer[] players)
        {
            int j, k;
            Segment Current = Trails[trailOffset];
            Interfaces.ISegment Wall;
            Vec? V;

            for (j = 0; j < players.Length; j++)
            {
                if (players[j] == null || players[j].getTrailHeight() < TRAIL_HEIGHT)
                    continue;

                for (k = 0; k < players[j].getTrailOffset() + 1; k++)
                {
                    // Avoid collision with our own newest trail segment
                    if (ReferenceEquals(players[j], this) && k >= trailOffset - 1)
                        break;

                    Wall = players[j].getTrail(k);

                    V = (Wall is Segment seg) ? Current.Intersect(seg) : null;

                    if (V != null)
                    {
                        // Trails: collision must not happen at the trail endpoint (t2 < 1.0f)
                        // t1 < 1.0f avoids colliding with our own current endpoint
                        if (Current.t1 >= 0.0f && Current.t1 < 1.0f && Current.t2 >= 0.0f && Current.t2 < 1.0f)
                        {
                            Current.vDirection.v[0] = V.v[0] - Current.vStart.v[0];
                            Current.vDirection.v[1] = V.v[1] - Current.vStart.v[1];
                            Speed = 0.0f;
                            _exploding = true;
                            _explodeTimer = 0f;
                            players[j].addScore(10);
                            try { SoundManager.Instance.PlayCrash(); SoundManager.Instance.StopEngine(); } catch (System.Exception ex) { System.Diagnostics.Debug.WriteLine($"GLTRON: Sound system error: {ex.Message}"); }
                            LogCrash($"Player {Player_num} CRASH trail!");
                            break;
                        }
                    }
                }
            }
        }
        
        private void LogCrash(string message)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"GLTRON: {message}");
#if ANDROID
                Android.Util.Log.Error("GLTRON", message);
#endif
                // TODO: Add to HUD console when available
            }
            catch { /* Ignore logging errors */ }
        }
        
        /// <summary>
        /// CRITICAL FIX: Set a safe spawn position that doesn't collide with existing trails
        /// </summary>
        public void SetSafeSpawnPosition(float x, float y, float gridSize)
        {
            try
            {
                // Update the starting position
                Trails[trailOffset].vStart.v[0] = x;
                Trails[trailOffset].vStart.v[1] = y;
                
                // Reset trail direction based on current direction
                Trails[trailOffset].vDirection.v[0] = 0.01f * DIRS_X[Direction];
                Trails[trailOffset].vDirection.v[1] = 0.01f * DIRS_Y[Direction];
                
                System.Diagnostics.Debug.WriteLine($"GLTRON: Player {Player_num} spawn position set to ({x:F1}, {y:F1})");
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GLTRON: SetSafeSpawnPosition error: {ex}");
            }
        }
        
        /// <summary>
        /// CRITICAL FIX: Check if a position collides with this player's trails
        /// </summary>
        public bool CheckPositionCollision(float x, float y, float safeRadius = 3.0f)
        {
            try
            {
                if (trailHeight <= 0.0f) return false; // No collision if trail is gone
                
                // Check collision with all trail segments
                for (int i = 0; i <= trailOffset; i++)
                {
                    if (Trails[i] == null) continue;
                    
                    // Get trail segment endpoints
                    float segStartX = Trails[i].vStart.v[0];
                    float segStartY = Trails[i].vStart.v[1];
                    float segEndX = segStartX + Trails[i].vDirection.v[0];
                    float segEndY = segStartY + Trails[i].vDirection.v[1];
                    
                    // Calculate distance from point to line segment
                    float distance = DistancePointToLineSegment(x, y, segStartX, segStartY, segEndX, segEndY);
                    
                    if (distance < safeRadius)
                    {
                        return true; // Collision detected
                    }
                }
                
                return false; // No collision
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GLTRON: CheckPositionCollision error: {ex}");
                return true; // Assume collision on error for safety
            }
        }
        
        /// <summary>
        /// Calculate distance from point to line segment
        /// </summary>
        private float DistancePointToLineSegment(float px, float py, float x1, float y1, float x2, float y2)
        {
            try
            {
                float dx = x2 - x1;
                float dy = y2 - y1;
                float lengthSquared = dx * dx + dy * dy;
                
                if (lengthSquared < 0.001f) // Very short segment, treat as point
                {
                    dx = px - x1;
                    dy = py - y1;
                    return (float)Math.Sqrt(dx * dx + dy * dy);
                }
                
                // Calculate projection parameter
                float t = ((px - x1) * dx + (py - y1) * dy) / lengthSquared;
                t = Math.Max(0, Math.Min(1, t)); // Clamp to segment
                
                // Find closest point on segment
                float closestX = x1 + t * dx;
                float closestY = y1 + t * dy;
                
                // Calculate distance
                dx = px - closestX;
                dy = py - closestY;
                return (float)Math.Sqrt(dx * dx + dy * dy);
            }
            catch
            {
                return 0.0f; // Return 0 on error (no collision)
            }
        }
        
        /// <summary>
        /// CRITICAL FIX: Clear all trails for clean restart
        /// </summary>
        public void ClearAllTrails()
        {
            try
            {
                trailOffset = 0;
                trailHeight = TRAIL_HEIGHT;
                
                // Clear all trail segments
                for (int i = 0; i < Trails.Length; i++)
                {
                    if (Trails[i] != null)
                    {
                        Trails[i].vDirection.v[0] = 0.0f;
                        Trails[i].vDirection.v[1] = 0.0f;
                    }
                }
                
                // Reset first trail segment
                if (Trails[0] != null)
                {
                    Trails[0].vDirection.v[0] = 0.01f * DIRS_X[Direction];
                    Trails[0].vDirection.v[1] = 0.01f * DIRS_Y[Direction];
                }
                
                System.Diagnostics.Debug.WriteLine($"GLTRON: Player {Player_num} trails cleared");
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GLTRON: ClearAllTrails error: {ex}");
            }
        }

    }
}
