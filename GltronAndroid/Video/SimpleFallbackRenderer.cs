using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GltronMonoGame.Video
{
    /// <summary>
    /// Fallback renderer that works without SpriteFont - uses simple graphics
    /// </summary>
    public class SimpleFallbackRenderer
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly SpriteBatch _spriteBatch;
        private Texture2D _pixel;
        
        // Colors
        private readonly Color _backgroundColor = new Color(0, 0, 20);
        private readonly Color _titleColor = Color.Cyan;
        private readonly Color _textColor = Color.White;
        private readonly Color _highlightColor = Color.Yellow;
        private readonly Color _gridColor = new Color(0, 100, 200, 100);

        // Animation
        private float _animationTimer;

        public SimpleFallbackRenderer(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = spriteBatch;
            
            // Create a 1x1 white pixel texture for drawing shapes
            _pixel = new Texture2D(_graphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }

        public void Update(float deltaTime)
        {
            _animationTimer += deltaTime;
        }

        public void DrawSplashScreen(GameTime gameTime)
        {
            var viewport = _graphicsDevice.Viewport;
            
            // Clear with dark background
            _graphicsDevice.Clear(_backgroundColor);

            _spriteBatch.Begin();

            // Draw simple title using rectangles (pixel art style)
            DrawPixelText("GLTRON", viewport.Width / 2 - 60, (int)(viewport.Height / 3), _titleColor);
            
            // Draw loading indicator
            DrawLoadingBar(viewport.Width / 2 - 100, (int)(viewport.Height * 0.7f), 200, 10);

            _spriteBatch.End();
        }

        public void DrawMainMenu(GameTime gameTime)
        {
            var viewport = _graphicsDevice.Viewport;
            
            // Clear with dark background
            _graphicsDevice.Clear(_backgroundColor);

            _spriteBatch.Begin();

            // Draw grid background
            DrawGrid(viewport);

            // Draw title
            DrawPixelText("GLTRON", viewport.Width / 2 - 60, (int)(viewport.Height / 6), _titleColor);
            
            // Draw subtitle
            DrawPixelText("LIGHT CYCLE", viewport.Width / 2 - 80, (int)(viewport.Height / 6) + 40, _textColor);

            // Draw pulsing "TAP TO START"
            float pulse = (float)(System.Math.Sin(_animationTimer * 3) * 0.3f + 0.7f);
            Color tapColor = Color.Lerp(_highlightColor, Color.White, pulse);
            DrawPixelText("TAP TO START", viewport.Width / 2 - 90, (int)(viewport.Height / 2), tapColor);

            // Draw instructions
            int instructionY = (int)(viewport.Height * 0.6f);
            DrawPixelText("TAP LEFT TO TURN LEFT", viewport.Width / 2 - 140, instructionY, _textColor);
            DrawPixelText("TAP RIGHT TO TURN RIGHT", viewport.Width / 2 - 150, instructionY + 30, _textColor);
            DrawPixelText("AVOID WALLS AND TRAILS", viewport.Width / 2 - 140, instructionY + 60, _textColor);

            _spriteBatch.End();
        }

        public void DrawGameOverMenu(GameTime gameTime, bool playerWon, int score)
        {
            var viewport = _graphicsDevice.Viewport;
            
            _spriteBatch.Begin();

            // Semi-transparent overlay
            DrawRectangle(0, 0, viewport.Width, viewport.Height, Color.Black * 0.7f);

            // Game Over title
            string title = playerWon ? "YOU WIN" : "GAME OVER";
            Color titleColor = playerWon ? Color.Lime : Color.Red;
            
            DrawPixelText(title, viewport.Width / 2 - 60, (int)(viewport.Height / 3), titleColor);

            // Score (simple number display)
            DrawPixelText($"SCORE {score}", viewport.Width / 2 - 70, (int)(viewport.Height / 3) + 50, _textColor);

            // Restart instruction
            float pulse = (float)(System.Math.Sin(_animationTimer * 2) * 0.4f + 0.6f);
            DrawPixelText("TAP TO RESTART", viewport.Width / 2 - 100, (int)(viewport.Height * 0.7f), _highlightColor * pulse);

            _spriteBatch.End();
        }

        private void DrawPixelText(string text, int x, int y, Color color)
        {
            // Simple pixel-art style text using rectangles
            // Each character is 8x12 pixels
            int charWidth = 8;
            int charHeight = 12;
            
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                int charX = x + i * (charWidth + 2);
                
                // Draw character based on simple patterns
                DrawCharacter(c, charX, y, charWidth, charHeight, color);
            }
        }

        private void DrawCharacter(char c, int x, int y, int width, int height, Color color)
        {
            // Simple character patterns using rectangles
            switch (char.ToUpper(c))
            {
                case 'A':
                    DrawRectangle(x + 2, y, 4, 2, color); // Top
                    DrawRectangle(x, y + 2, 2, 10, color); // Left
                    DrawRectangle(x + 6, y + 2, 2, 10, color); // Right
                    DrawRectangle(x + 2, y + 6, 4, 2, color); // Middle
                    break;
                case 'C':
                    DrawRectangle(x + 2, y, 4, 2, color); // Top
                    DrawRectangle(x, y + 2, 2, 8, color); // Left
                    DrawRectangle(x + 2, y + 10, 4, 2, color); // Bottom
                    break;
                case 'E':
                    DrawRectangle(x, y, 8, 2, color); // Top
                    DrawRectangle(x, y + 2, 2, 8, color); // Left
                    DrawRectangle(x, y + 5, 6, 2, color); // Middle
                    DrawRectangle(x, y + 10, 8, 2, color); // Bottom
                    break;
                case 'G':
                    DrawRectangle(x + 2, y, 4, 2, color); // Top
                    DrawRectangle(x, y + 2, 2, 8, color); // Left
                    DrawRectangle(x + 2, y + 10, 4, 2, color); // Bottom
                    DrawRectangle(x + 6, y + 6, 2, 4, color); // Right bottom
                    DrawRectangle(x + 4, y + 6, 4, 2, color); // Middle right
                    break;
                case 'H':
                    DrawRectangle(x, y, 2, 12, color); // Left
                    DrawRectangle(x + 6, y, 2, 12, color); // Right
                    DrawRectangle(x + 2, y + 5, 4, 2, color); // Middle
                    break;
                case 'I':
                    DrawRectangle(x + 2, y, 4, 2, color); // Top
                    DrawRectangle(x + 3, y + 2, 2, 8, color); // Middle
                    DrawRectangle(x + 2, y + 10, 4, 2, color); // Bottom
                    break;
                case 'L':
                    DrawRectangle(x, y, 2, 12, color); // Left
                    DrawRectangle(x, y + 10, 8, 2, color); // Bottom
                    break;
                case 'N':
                    DrawRectangle(x, y, 2, 12, color); // Left
                    DrawRectangle(x + 6, y, 2, 12, color); // Right
                    DrawRectangle(x + 2, y + 3, 2, 2, color); // Diagonal 1
                    DrawRectangle(x + 4, y + 6, 2, 2, color); // Diagonal 2
                    break;
                case 'O':
                    DrawRectangle(x + 2, y, 4, 2, color); // Top
                    DrawRectangle(x, y + 2, 2, 8, color); // Left
                    DrawRectangle(x + 6, y + 2, 2, 8, color); // Right
                    DrawRectangle(x + 2, y + 10, 4, 2, color); // Bottom
                    break;
                case 'R':
                    DrawRectangle(x, y, 2, 12, color); // Left
                    DrawRectangle(x + 2, y, 4, 2, color); // Top
                    DrawRectangle(x + 6, y + 2, 2, 3, color); // Right top
                    DrawRectangle(x + 2, y + 5, 4, 2, color); // Middle
                    DrawRectangle(x + 4, y + 7, 2, 2, color); // Diagonal
                    DrawRectangle(x + 6, y + 9, 2, 3, color); // Right bottom
                    break;
                case 'S':
                    DrawRectangle(x + 2, y, 6, 2, color); // Top
                    DrawRectangle(x, y + 2, 2, 3, color); // Left top
                    DrawRectangle(x + 2, y + 5, 4, 2, color); // Middle
                    DrawRectangle(x + 6, y + 7, 2, 3, color); // Right bottom
                    DrawRectangle(x, y + 10, 6, 2, color); // Bottom
                    break;
                case 'T':
                    DrawRectangle(x, y, 8, 2, color); // Top
                    DrawRectangle(x + 3, y + 2, 2, 10, color); // Middle
                    break;
                case 'U':
                    DrawRectangle(x, y, 2, 10, color); // Left
                    DrawRectangle(x + 6, y, 2, 10, color); // Right
                    DrawRectangle(x + 2, y + 10, 4, 2, color); // Bottom
                    break;
                case 'V':
                    DrawRectangle(x, y, 2, 8, color); // Left
                    DrawRectangle(x + 6, y, 2, 8, color); // Right
                    DrawRectangle(x + 2, y + 8, 2, 2, color); // Bottom left
                    DrawRectangle(x + 4, y + 8, 2, 2, color); // Bottom right
                    DrawRectangle(x + 3, y + 10, 2, 2, color); // Bottom center
                    break;
                case 'W':
                    DrawRectangle(x, y, 2, 12, color); // Left
                    DrawRectangle(x + 6, y, 2, 12, color); // Right
                    DrawRectangle(x + 3, y + 6, 2, 6, color); // Middle
                    break;
                case 'Y':
                    DrawRectangle(x, y, 2, 5, color); // Left top
                    DrawRectangle(x + 6, y, 2, 5, color); // Right top
                    DrawRectangle(x + 2, y + 5, 4, 2, color); // Middle
                    DrawRectangle(x + 3, y + 7, 2, 5, color); // Bottom
                    break;
                case ' ':
                    // Space - do nothing
                    break;
                default:
                    // Unknown character - draw a small rectangle
                    DrawRectangle(x + 2, y + 5, 4, 2, color);
                    break;
            }
        }

        private void DrawRectangle(int x, int y, int width, int height, Color color)
        {
            _spriteBatch.Draw(_pixel, new Rectangle(x, y, width, height), color);
        }

        private void DrawGrid(Viewport viewport)
        {
            int gridSize = 50;
            
            // Vertical lines
            for (int x = 0; x < viewport.Width; x += gridSize)
            {
                DrawRectangle(x, 0, 1, viewport.Height, _gridColor);
            }
            
            // Horizontal lines
            for (int y = 0; y < viewport.Height; y += gridSize)
            {
                DrawRectangle(0, y, viewport.Width, 1, _gridColor);
            }
        }

        private void DrawLoadingBar(float x, float y, float width, float height)
        {
            // Background
            DrawRectangle((int)x, (int)y, (int)width, (int)height, Color.Gray);
            
            // Progress (animated)
            float progress = (float)(System.Math.Sin(_animationTimer * 2) * 0.5f + 0.5f);
            DrawRectangle((int)x, (int)y, (int)(width * progress), (int)height, _titleColor);
        }

        public void Dispose()
        {
            _pixel?.Dispose();
        }
    }
}
