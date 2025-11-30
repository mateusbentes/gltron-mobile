using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;

namespace GltronMobileGame.Sound;

public class SoundManager
{
    private static SoundManager _instance;
    public static SoundManager Instance => _instance ??= new SoundManager();

    private ContentManager _content;
    private Song _music;
    private SoundEffect _engine;
    private SoundEffect _crash;
    private SoundEffect _recognizer;
    private SoundEffectInstance _engineInstance;
    private SoundEffectInstance _recognizerInstance;

    private SoundManager() { }

    public void Initialize(ContentManager content)
    {
        _content = content;
        try
        {
            _music = _content.Load<Song>("Assets/song_revenge_of_cats");
            _engine = _content.Load<SoundEffect>("Assets/game_engine");
            _crash = _content.Load<SoundEffect>("Assets/game_crash");
            _recognizer = _content.Load<SoundEffect>("Assets/game_recognizer");
            try { Android.Util.Log.Info("GLTRON", "Sound content loaded (including recognizer)"); } catch { }
        }
        catch (System.Exception ex)
        {
            try { Android.Util.Log.Error("GLTRON", $"Sound content load failed: {ex}"); } catch { }
            throw;
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
        _engineInstance ??= _engine.CreateInstance();
        _engineInstance.Volume = volume;
        _engineInstance.IsLooped = loop;
        if (_engineInstance.State != SoundState.Playing)
            _engineInstance.Play();
    }

    public void StopEngine()
    {
        if (_engineInstance != null && _engineInstance.State == SoundState.Playing)
            _engineInstance.Stop();
    }

    public void PlayCrash(float volume = 0.8f)
    {
        _crash?.Play(volume, 0f, 0f);
    }

    public void PlayRecognizer(float volume = 0.3f, bool loop = true)
    {
        if (_recognizer == null) return;
        _recognizerInstance ??= _recognizer.CreateInstance();
        _recognizerInstance.Volume = volume;
        _recognizerInstance.IsLooped = loop;
        if (_recognizerInstance.State != SoundState.Playing)
            _recognizerInstance.Play();
    }

    public void StopRecognizer()
    {
        if (_recognizerInstance != null && _recognizerInstance.State == SoundState.Playing)
            _recognizerInstance.Stop();
    }

    public void StopSound(int soundId)
    {
        // Handle legacy sound ID system from Java version
        switch (soundId)
        {
            case 1: // CRASH_SOUND
                // Crash sounds are one-shot, no need to stop
                break;
            case 2: // ENGINE_SOUND
                StopEngine();
                break;
            case 3: // MUSIC_SOUND
                StopMusic();
                break;
            case 4: // RECOGNIZER_SOUND
                StopRecognizer();
                break;
        }
    }
}
