using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GltronMobileEngine.Video;

public enum CameraType
{
    Circling,
    Follow,
    FollowFar,
    FollowClose,
    Bird,
    Cockpit,
    Mouse
}

public class Camera
{
    public void SetProjection(float gridSize, float aspect)
    {
        // CRITICAL FIX: Match Java Video.doPerspective settings
        float fov = MathHelper.ToRadians(105f); // Same as Java
        float znear = 1.0f;
        float zfar = gridSize * 6.5f;
        if (zfar < 200f) zfar = 200f;
        
        Projection = Matrix.CreatePerspectiveFieldOfView(fov, aspect, znear, zfar);
        
        // Debug logging
        try
        {
#if ANDROID
            Android.Util.Log.Info("GLTRON", $"Camera projection updated: FOV={MathHelper.ToDegrees(fov):F1}°, aspect={aspect:F2}, near={znear}, far={zfar}");
#endif
        }
        catch { }
    }

    public Matrix View { get; private set; }
    public Matrix Projection { get; private set; }
    public int ViewportWidth { get; }
    public int ViewportHeight { get; }

    // GLTron camera constants (optimized for following player)
    private const float CAM_CIRCLE_DIST = 25.0f;
    private const float CAM_FOLLOW_DIST = 20.0f;  // Good distance behind player
    private const float CAM_FOLLOW_FAR_DIST = 35.0f;
    private const float CAM_FOLLOW_CLOSE_DIST = 12.0f;
    private const float CAM_FOLLOW_BIRD_DIST = 60.0f;
    private const float CAM_CIRCLE_Z = 15.0f;
    private const float CAM_COCKPIT_Z = 4.0f;
    private const float CAM_SPEED = 0.001f;
    private const float B_HEIGHT = 0.0f;

    private CameraType _cameraType;
    private Vector3 _target;
    private Vector3 _camPos;
    private Vector3 _lastPlayerPos = Vector3.Zero; // For smooth following
    private float[] _movement = new float[4]; // R, CHI, PHI, PHI_OFFSET
    private float _phi;

    // Camera defaults from original (R, CHI, PHI)
    private static readonly float[,] CamDefaults = {
        { CAM_CIRCLE_DIST, MathHelper.Pi / 3.0f, 0.0f }, // circle
        { CAM_FOLLOW_DIST, MathHelper.Pi / 4.0f, MathHelper.Pi / 72.0f }, // follow
        { CAM_FOLLOW_FAR_DIST, MathHelper.Pi / 4.0f, MathHelper.Pi / 72.0f }, // follow far
        { CAM_FOLLOW_CLOSE_DIST, MathHelper.Pi / 4.0f, MathHelper.Pi / 72.0f }, // follow close
        { CAM_FOLLOW_BIRD_DIST, MathHelper.Pi / 4.0f, MathHelper.Pi / 72.0f }, // birds-eye view
        { CAM_COCKPIT_Z, MathHelper.Pi / 8.0f, 0.0f }, // cockpit
        { CAM_CIRCLE_DIST, MathHelper.Pi / 3.0f, 0.0f } // free
    };

    public Camera(Viewport viewport)
    {
        ViewportWidth = viewport.Width;
        ViewportHeight = viewport.Height;
        
        // Projection setup similar to Java Video.doPerspective (FOV ~105°, znear 1.0, zfar depends on grid)
        Projection = Matrix.CreatePerspectiveFieldOfView(
            MathHelper.ToRadians(105f),
            viewport.AspectRatio,
            1.0f,
            650f);
        
        // CRITICAL FIX: Start with follow camera that tracks the player (like Java version)
        _cameraType = CameraType.Follow;
        InitializeFollowCamera();
        
        // CRITICAL FIX: Set initial camera position to see arena center properly
        float arenaCenter = 50f; // Should be gridSize / 2
        _target = new Vector3(arenaCenter, 0f, arenaCenter); // Arena center at ground level
        _camPos = new Vector3(arenaCenter, 25f, arenaCenter + 30f); // Higher and further back for better view
        
        UpdateViewMatrix();
    }

    private void InitializeCirclingCamera()
    {
        _movement[0] = CamDefaults[0, 0]; // R
        _movement[1] = CamDefaults[0, 1]; // CHI
        _movement[2] = CamDefaults[0, 2]; // PHI
        _movement[3] = 0.0f; // PHI_OFFSET
        _phi = 0.0f;
    }

    private void InitializeFollowCamera()
    {
        // CRITICAL FIX: Match Java camera defaults exactly
        _movement[0] = CAM_FOLLOW_DIST; // 20.0f - same as Java
        _movement[1] = MathHelper.Pi / 4.0f; // 45 degree elevation - same as Java  
        _movement[2] = MathHelper.Pi / 72.0f; // Small rotation offset - same as Java
        _movement[3] = 0.0f; // No additional offset
        _phi = 0.0f;
    }

    private void InitializeBirdCamera()
    {
        _movement[0] = CamDefaults[4, 0]; // R - bird's eye distance
        _movement[1] = CamDefaults[4, 1]; // CHI - bird's eye angle
        _movement[2] = CamDefaults[4, 2]; // PHI
        _movement[3] = 0.0f; // PHI_OFFSET
        _phi = 0.0f;
    }

    public void Update(Vector3 playerPos, GameTime gameTime)
    {
        // Choose camera behavior based on type
        switch (_cameraType)
        {
            case CameraType.Follow:
                UpdateFollowingCamera(playerPos, gameTime);
                break;
            case CameraType.Bird:
                UpdateBirdEyeCamera();
                break;
            default:
                UpdateFollowingCamera(playerPos, gameTime);
                break;
        }
        
        UpdateViewMatrix();
    }

    // Add method to update camera with player direction
    public void UpdateWithPlayerDirection(Vector3 playerPos, int playerDirection, GameTime gameTime)
    {
        switch (_cameraType)
        {
            case CameraType.Follow:
                UpdateFollowingCameraWithDirection(playerPos, playerDirection, gameTime);
                break;
            case CameraType.Bird:
                UpdateBirdEyeCamera();
                break;
            default:
                UpdateFollowingCameraWithDirection(playerPos, playerDirection, gameTime);
                break;
        }
        UpdateViewMatrix();
    }
    // CRITICAL FIX: Java-like turn-coupled phi interpolation (matches original exactly)
    public void UpdateWithTurn(Vector3 playerPos, int dir, int lastDir, long currentTimeMs, long turnStartTimeMs, long turnLengthMs)
    {
        if (playerPos == Vector3.Zero)
        {
            _target = new Vector3(50f, 0f, 50f); // Y=0 for ground level
            _camPos = new Vector3(50f, 15f, 65f); // Higher camera for better view
            UpdateViewMatrix();
            return;
        }
        
        // Handle direction wrapping like Java version
        int d = dir; 
        int ld = lastDir;
        if (d == 1 && ld == 2) d = 4;
        if (d == 2 && ld == 1) ld = 4;
        
        float phi;
        float basePhi = _movement[2] + _movement[3];
        
        // Smooth interpolation during turn like Java version
        if (currentTimeMs - turnStartTimeMs < turnLengthMs && turnLengthMs > 0)
        {
            long time = currentTimeMs - turnStartTimeMs;
            float[] camAngles = { MathHelper.PiOver2, 0f, 3f * MathHelper.PiOver2, MathHelper.Pi, 2f * MathHelper.Pi };
            phi = ((turnLengthMs - time) * camAngles[ld] + time * camAngles[d]) / (float)turnLengthMs + basePhi;
        }
        else
        {
            float[] camAngles = { MathHelper.PiOver2, 0f, 3f * MathHelper.PiOver2, MathHelper.Pi, 2f * MathHelper.Pi };
            phi = camAngles[dir % 4] + basePhi;
        }
        
        // Use proper camera distance and elevation like Java version
        float r = _movement[0] > 0 ? _movement[0] : 20f; // Slightly closer than Java default
        float chi = _movement[1] > 0 ? _movement[1] : MathHelper.PiOver4; // 45 degree elevation
        
        // Target at player position on ground
        _target = new Vector3(playerPos.X, 0f, playerPos.Z);
        
        // Position camera using spherical coordinates (like Java version)
        _camPos.X = _target.X + r * (float)System.Math.Cos(phi) * (float)System.Math.Sin(chi);
        _camPos.Y = _target.Y + r * (float)System.Math.Cos(chi); // Fixed: Y is height in MonoGame
        _camPos.Z = _target.Z + r * (float)System.Math.Sin(phi) * (float)System.Math.Sin(chi);
        
        UpdateViewMatrix();
    }


    private void UpdateFollowingCamera(Vector3 playerPos, GameTime gameTime)
    {
        // GLTron-style third-person camera following behind the motorcycle
        if (playerPos != Vector3.Zero)
        {
            // Smooth camera interpolation for less jarring movement
            float lerpFactor = 0.2f; // Slightly faster for responsive following
            
            if (_lastPlayerPos == Vector3.Zero)
            {
                _lastPlayerPos = playerPos; // First frame
            }
            
            Vector3 smoothPlayerPos = Vector3.Lerp(_lastPlayerPos, playerPos, lerpFactor);
            _lastPlayerPos = smoothPlayerPos;
            
            // Target slightly ahead of the player for better view
            _target = new Vector3(smoothPlayerPos.X, 2f, smoothPlayerPos.Z);
            
            // GLTron-style camera: close behind and slightly above the motorcycle
            // Distance: 12 units behind, Height: 8 units above ground
            float cameraDistance = 12f;
            float cameraHeight = 8f;
            
            _camPos = new Vector3(smoothPlayerPos.X, cameraHeight, smoothPlayerPos.Z + cameraDistance);
        }
        else
        {
            // Fallback to arena center view when no player position
            _target = new Vector3(50f, 2f, 50f);
            _camPos = new Vector3(50f, 8f, 62f);
        }
    }

    private void UpdateFollowingCameraWithDirection(Vector3 playerPos, int playerDirection, GameTime gameTime)
    {
        // GLTron-style third-person camera following behind the motorcycle based on direction
        if (playerPos != Vector3.Zero)
        {
            // Smooth camera interpolation
            float lerpFactor = 0.25f; // Responsive following
            
            if (_lastPlayerPos == Vector3.Zero)
            {
                _lastPlayerPos = playerPos;
            }
            
            Vector3 smoothPlayerPos = Vector3.Lerp(_lastPlayerPos, playerPos, lerpFactor);
            _lastPlayerPos = smoothPlayerPos;
            
            // GLTron direction vectors: 0=up(-Y), 1=right(+X), 2=down(+Y), 3=left(-X)
            Vector3[] directionVectors = {
                new Vector3(0, 0, -1), // Up (negative Z)
                new Vector3(1, 0, 0),  // Right (positive X)
                new Vector3(0, 0, 1),  // Down (positive Z)
                new Vector3(-1, 0, 0)  // Left (negative X)
            };
            
            Vector3 forwardDir = directionVectors[playerDirection % 4];
            Vector3 backwardDir = -forwardDir;
            
            // Target slightly ahead of the player in movement direction
            _target = smoothPlayerPos + forwardDir * 3f + new Vector3(0, 1f, 0);
            
            // Position camera behind the motorcycle
            float cameraDistance = 10f;
            float cameraHeight = 6f;
            
            _camPos = smoothPlayerPos + backwardDir * cameraDistance + new Vector3(0, cameraHeight, 0);
        }
        else
        {
            // Fallback
            _target = new Vector3(50f, 1f, 50f);
            _camPos = new Vector3(50f, 6f, 60f);
        }
    }

    private void UpdateBirdEyeCamera()
    {
        // Bird's eye view - good for seeing entire arena
        _target = new Vector3(50f, 0f, 50f); // Arena center
        _camPos = new Vector3(50f, 60f, 50f); // High above center
    }

    private void UpdateCirclingCamera(float dt)
    {
        // Circling camera rotates around the player (like original GLTron)
        _phi += CAM_SPEED * dt;
        
        float r = _movement[0];     // Distance from player
        float chi = _movement[1];   // Elevation angle
        float phi = _phi + _movement[2] + _movement[3]; // Rotation angle
        
        // Position camera in spherical coordinates around target (GLTron style)
        _camPos.X = _target.X + r * (float)System.Math.Cos(phi) * (float)System.Math.Sin(chi);
        _camPos.Y = _target.Y + r * (float)System.Math.Sin(phi) * (float)System.Math.Sin(chi);
        _camPos.Z = _target.Z + r * (float)System.Math.Cos(chi);
        
        // Debug logging
        try
        {
#if ANDROID
            if (dt > 0) // Only log occasionally
            {
                Android.Util.Log.Debug("GLTRON", $"Camera: target=({_target.X:F1},{_target.Y:F1},{_target.Z:F1}) cam=({_camPos.X:F1},{_camPos.Y:F1},{_camPos.Z:F1}) phi={phi:F2}");
            }
#endif
        }
        catch { }
    }

    private void UpdateFollowCamera()
    {
        float r = _movement[0];
        float chi = _movement[1];
        float phi = _movement[2] + _movement[3];
        
        // Follow camera stays behind the player
        _camPos.X = _target.X + r * (float)System.Math.Cos(phi) * (float)System.Math.Sin(chi);
        _camPos.Y = _target.Y + r * (float)System.Math.Sin(phi) * (float)System.Math.Sin(chi);
        _camPos.Z = r * (float)System.Math.Cos(chi);
    }

    private void UpdateBirdCamera()
    {
        // Bird's eye view - high above the arena center, looking down
        Vector3 arenaCenter = new Vector3(50f, 50f, 0f);
        _target = arenaCenter; // Always look at arena center
        
        float r = _movement[0]; // Distance from center
        float height = CAM_FOLLOW_BIRD_DIST; // High above
        
        // Position camera high above arena center
        _camPos = new Vector3(arenaCenter.X, arenaCenter.Y, height);
    }

    private void UpdateViewMatrix()
    {
        // Use standard Y-up for MonoGame
        Vector3 up = Vector3.UnitY;
        
        // Ensure camera is not at the same position as target
        if (Vector3.Distance(_camPos, _target) < 0.1f)
        {
            _camPos = _target + new Vector3(0, -20f, 30f);
        }
        
        try
        {
            View = Matrix.CreateLookAt(_camPos, _target, up);
            
            // Debug logging
#if ANDROID
            float distance = Vector3.Distance(_camPos, _target);
            Android.Util.Log.Debug("GLTRON", $"Camera distance: {distance:F2}, View matrix created successfully");
#endif
        }
        catch (System.Exception ex)
        {
            try
            {
#if ANDROID
                Android.Util.Log.Error("GLTRON", $"Failed to create view matrix: {ex.Message}");
#endif
            }
            catch { /* Ignore logging errors */ }
            
            // Fallback view matrix
            View = Matrix.Identity;
        }
    }

    public void SetCameraType(CameraType type)
    {
        _cameraType = type;
        int index = (int)type;
        if (index < CamDefaults.GetLength(0))
        {
            _movement[0] = CamDefaults[index, 0];
            _movement[1] = CamDefaults[index, 1];
            _movement[2] = CamDefaults[index, 2];
        }
        
        try
        {
#if ANDROID
            Android.Util.Log.Info("GLTRON", $"Camera type changed to: {type}");
#endif
        }
        catch { }
    }

    public CameraType GetCameraType()
    {
        return _cameraType;
    }
}
