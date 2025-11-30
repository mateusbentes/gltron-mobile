namespace GltronMobileEngine.Interfaces;

/// <summary>
/// Multiplatform interface for Player functionality
/// Compatible with Android, iOS, and other MonoGame platforms
/// </summary>
public interface IPlayer
{
    // Player identification
    int getPlayerNum();
    
    // Position and movement
    float getXpos();
    float getYpos();
    float getSpeed();
    void setSpeed(float speed);
    
    // Direction and turning
    int getDirection();
    void doTurn(int direction, long currentTime);
    
    // Trail management
    int getTrailOffset();
    ISegment getTrail(int index);
    float getTrailHeight();
    
    // Game state
    int getScore();
    void addScore(int points);
    bool getExplode(); // Explosion state
    bool isVisible(); // Visibility state
    int getLastDirection(); // Last direction for camera interpolation
    
    // Movement and collision - multiplatform compatible
    void doMovement(long timeDt, long timeCurrent, ISegment[] walls, IPlayer[] players);
}
