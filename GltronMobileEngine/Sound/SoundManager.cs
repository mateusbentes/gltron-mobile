using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;

namespace GltronMobileEngine.Sound;

public class SoundManager
{
    private static SoundManager? _instance;
    public static SoundManager Instance => _instance ??= new SoundManager();

    private ContentManager? _content;
    private Song? _music;
    private SoundEffect? _engine;
    private SoundEffect? _crash;
    private SoundEffectInstance? _engineInstance;

    private SoundManager() { }

    public void Initialize(ContentManager content)
    {
        _content = content;
        try
        {
            _music = _content.Load<Song>("Assets/song_revenge_of_cats");
            _engine = _content.Load<SoundEffect>("Assets/game_engine");
            _crash = _content.Load<SoundEffect>("Assets/game_crash");
            // Sound content loaded successfully
        }
        catch (System.Exception ex)
        {
            // Sound content load failed - continue without sound
            System.Diagnostics.Debug.WriteLine($"Sound initialization failed: {ex.Message}");
            // Don't throw - allow game to continue without sound
        }
    }

    public void PlayMusic(bool loop = true, float volume = 0.5f)
    {
        if (_music == null) return;
        MediaPlayer.IsRepeating = loop;
        MediaPlayer.Volume = volume;
        MediaPlayer.Play(_music);
    }

    public void StopMusic()
    {
        if (MediaPlayer.State == MediaState.Playing)
            MediaPlayer.Stop();
    }

    public void PlayEngine(float volume = 0.3f, bool loop = true)
    {
        if (_engine == null) return;
        
        try
        {
            _engineInstance ??= _engine.CreateInstance();
            if (_engineInstance == null) return;
            
            _engineInstance.Volume = volume;
            _engineInstance.IsLooped = loop;
            if (_engineInstance.State != SoundState.Playing)
                _engineInstance.Play();
        }
        catch (System.Exception)
        {
            // Ignore sound errors to prevent game crashes
        }
    }

    public void StopEngine()
    {
        try
        {
            if (_engineInstance != null && _engineInstance.State == SoundState.Playing)
                _engineInstance.Stop();
        }
        catch (System.Exception)
        {
            // Ignore sound errors to prevent game crashes
        }
    }

    public void PlayCrash(float volume = 0.8f)
    {
        try
        {
            _crash?.Play(volume, 0f, 0f);
        }
        catch (System.Exception)
        {
            // Ignore sound errors to prevent game crashes
        }
    }

    public void Dispose()
    {
        try
        {
            _engineInstance?.Dispose();
            _engineInstance = null;
        }
        catch (System.Exception)
        {
            // Ignore disposal errors
        }
    }
}
