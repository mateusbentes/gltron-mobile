#nullable disable
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GltronMobileEngine;

namespace GltronMobileGame.Video;

public class HUD
{
    private readonly SpriteBatch _sb;
    private readonly SpriteFont _font;
    private readonly string[] _console;
    private GltronMobileEngine.Player _player;
    private int _pos;
    private int _offset;
    private bool _showWin;
    private bool _showLose;
    private bool _showInstr = true;

    public HUD(SpriteBatch sb, SpriteFont font, int consoleDepth = 100)
    {
        _sb = sb;
        _font = font;
        _console = new string[consoleDepth];
    }

    public void AddLineToConsole(string line)
    {
        _console[_pos] = line;
        _pos = (_pos + 1) % _console.Length;
    }

    public void DisplayWin() => _showWin = true;
    public void DisplayLose() => _showLose = true;
    public void DisplayInstr(bool show) => _showInstr = show;
    public void SetPlayer(GltronMobileEngine.Player player) => _player = player;

    public void ResetConsole()
    {
        for (int i = 0; i < _console.Length; i++) _console[i] = null;
        _pos = 0;
        _offset = 0;
        _showWin = false;
        _showLose = false;
        _showInstr = true;
    }

    public void Draw(GameTime gameTime, int score)
    {
        float fps = 1f / (float)gameTime.ElapsedGameTime.TotalSeconds;
        float speed = 0f;
        try { if (_player != null) speed = _player.getSpeed(); } catch { }

        _sb.DrawString(_font, $"FPS: {fps:0}  Score: {score}  Speed: {speed:0.0}", new Vector2(10, 10), Color.White);

        if (_showInstr)
            _sb.DrawString(_font, "Tap left/right to turn", new Vector2(10, 30), Color.Yellow);
        if (_showWin)
            _sb.DrawString(_font, "YOU WIN", new Vector2(10, 50), Color.Lime);
        if (_showLose)
            _sb.DrawString(_font, "YOU LOSE", new Vector2(10, 50), Color.Red);

        int lines = 0; int y = 80;
        for (int i = 0; i < _console.Length && lines < 10; i++)
        {
            int idx = (_pos - 1 - i - _offset + _console.Length) % _console.Length;
            var txt = _console[idx];
            if (string.IsNullOrEmpty(txt)) break;
            _sb.DrawString(_font, txt, new Vector2(10, y), Color.LightGray);
            y += 16; lines++;
        }
    }
}
