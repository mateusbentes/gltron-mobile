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
    public Matrix View { get; private set; }
    public Matrix Projection { get; private set; }
    public int ViewportWidth { get; }
    public int ViewportHeight { get; }

    // GLTron camera constants (adjusted for better arena view)
    private const float CAM_CIRCLE_DIST = 35.0f;  // Increased to see full arena
    private const float CAM_FOLLOW_DIST = 25.0f;
    private const float CAM_FOLLOW_FAR_DIST = 45.0f;
    private const float CAM_FOLLOW_CLOSE_DIST = 15.0f;
    private const float CAM_FOLLOW_BIRD_DIST = 80.0f;  // Better bird's eye view
    private const float CAM_CIRCLE_Z = 20.0f;  // Higher up to see arena
    private const float CAM_COCKPIT_Z = 4.0f;
    private const float CAM_SPEED = 0.001f;  // Slightly faster rotation
    private const float B_HEIGHT = 0.0f;

    private CameraType _cameraType;
    private Vector3 _target;
    private Vector3 _camPos;
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
        
        // More conservative perspective settings
        Projection = Matrix.CreatePerspectiveFieldOfView(
            MathHelper.PiOver4, // 45 degrees - more standard
            viewport.AspectRatio,
            0.1f,  // Very close near plane
            1000f); // Far plane to see everything
        
        // Start with a simple follow camera that works
        _cameraType = CameraType.Follow;
        InitializeFollowCamera();
        
        // Set initial camera position to see arena
        _target = new Vector3(50f, 50f, 0f); // Arena center
        _camPos = new Vector3(50f, 20f, 30f); // Behind and above arena center
        
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
        _movement[0] = 30.0f; // Distance from target
        _movement[1] = MathHelper.PiOver4; // 45 degree elevation
        _movement[2] = 0.0f; // No rotation
        _movement[3] = 0.0f; // No offset
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
        // Simple camera setup - look at arena center from a good angle
        _target = new Vector3(50f, 0f, 50f); // Center of 100x100 arena
        _camPos = new Vector3(50f, 80f, 120f); // Behind and above center
        
        UpdateViewMatrix();
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
#if ANDROID
            Android.Util.Log.Error("GLTRON", $"Failed to create view matrix: {ex.Message}");
#endif
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
    }
}
