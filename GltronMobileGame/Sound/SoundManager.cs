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
            // Load only background music for menu
            _music = _content.Load<Song>("Assets/song_revenge_of_cats");
            System.Diagnostics.Debug.WriteLine("GLTRON: Initialize: Loaded music for menu");
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GLTRON: Sound content load failed: {ex}");
            throw;
        }
    }

    public void PlayMusic(bool loop = true, float volume = 0.6f)
    {
        if (_music == null) { System.Diagnostics.Debug.WriteLine("GLTRON: PlayMusic: _music is null"); return; }
        MediaPlayer.IsRepeating = loop;
        MediaPlayer.Volume = volume;
        if (MediaPlayer.State != MediaState.Playing)
        {
            MediaPlayer.Play(_music);
        }
        System.Diagnostics.Debug.WriteLine($"GLTRON: PlayMusic called: state={MediaPlayer.State}, vol={MediaPlayer.Volume}, loop={MediaPlayer.IsRepeating}");
    }

    public void StopMusic()
    {
        if (MediaPlayer.State == MediaState.Playing)
        {
            MediaPlayer.Stop();
            System.Diagnostics.Debug.WriteLine("GLTRON: StopMusic called: music stopped");
        }
    }

    private bool _gameplaySfxLoaded = false;
    public void EnsureGameplaySfxLoaded()
    {
        if (_gameplaySfxLoaded) return;
        _engine = _content.Load<SoundEffect>("Assets/game_engine");
        _crash = _content.Load<SoundEffect>("Assets/game_crash");
        _recognizer = _content.Load<SoundEffect>("Assets/game_recognizer");
        _gameplaySfxLoaded = true;
        System.Diagnostics.Debug.WriteLine("GLTRON: Gameplay SFX loaded (engine, crash, recognizer)");
    }

    public void PlayEngine(float volume = 0.3f, bool loop = true)
    {
        EnsureGameplaySfxLoaded();
        if (_engine == null) return;
        _engineInstance ??= _engine.CreateInstance();
        _engineInstance.Volume = volume;
        _engineInstance.IsLooped = loop;
        if (_engineInstance.State != SoundState.Playing)
            _engineInstance.Play();
        System.Diagnostics.Debug.WriteLine($"GLTRON: PlayEngine: state={_engineInstance.State}, vol={_engineInstance.Volume}, loop={_engineInstance.IsLooped}");
    }

    public void StopEngine()
    {
        if (_engineInstance != null && _engineInstance.State == SoundState.Playing)
            _engineInstance.Stop();
    }

    public void PlayCrash(float volume = 0.3f)
    {
        EnsureGameplaySfxLoaded();
        _crash?.Play(volume, 0f, 0f);
    }

    public void PlayRecognizer(float volume = 0.3f, bool loop = true)
    {
        EnsureGameplaySfxLoaded();
        if (_recognizer == null) return;
        _recognizerInstance ??= _recognizer.CreateInstance();
        _recognizerInstance.Volume = volume;
        _recognizerInstance.IsLooped = loop;
        if (_recognizerInstance.State != SoundState.Playing)
            _recognizerInstance.Play();
        System.Diagnostics.Debug.WriteLine($"GLTRON: PlayRecognizer: state={_recognizerInstance.State}, vol={_recognizerInstance.Volume}, loop={_recognizerInstance.IsLooped}");
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
                // No-op: do not stop music via legacy StopSound to keep BGM across menu/gameplay
                break;
            case 4: // RECOGNIZER_SOUND
                StopRecognizer();
                break;
        }
    }
}
