using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GltronMobileEngine.Video;

public class HUD
{
    private readonly SpriteBatch _sb;
    private readonly SpriteFont _font;
    private readonly string?[] _console;
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
    public void ResetConsole()
    {
        for (int i = 0; i < _console.Length; i++) _console[i] = null;
        _pos = 0; _offset = 0; _showWin = _showLose = false; _showInstr = true;
    }

    public void Draw(GameTime gameTime, int score)
    {
        _sb.Begin();
        // FPS placeholder
        double fps = gameTime.ElapsedGameTime.TotalMilliseconds > 0
            ? 1000.0 / gameTime.ElapsedGameTime.TotalMilliseconds
            : 0;
        _sb.DrawString(_font, $"FPS: {fps:0}  Score: {score}", new Vector2(10, 10), Color.White);

        // Instructions / win/lose
        if (_showInstr)
            _sb.DrawString(_font, "Swipe left/right to turn", new Vector2(10, 30), Color.Yellow);
        if (_showWin)
            _sb.DrawString(_font, "YOU WIN", new Vector2(10, 50), Color.Lime);
        if (_showLose)
            _sb.DrawString(_font, "YOU LOSE", new Vector2(10, 50), Color.Red);

        // Console scaffold (last ~10 lines)
        int lines = 0; int y = 80;
        for (int i = 0; i < _console.Length && lines < 10; i++)
        {
            int idx = (_pos - 1 - i - _offset + _console.Length) % _console.Length;
            var txt = _console[idx];
            if (string.IsNullOrEmpty(txt)) break;
            _sb.DrawString(_font, txt, new Vector2(10, y), Color.LightGray);
            y += 16; lines++;
        }
        _sb.End();
    }
}
