using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GltronMobileGame.Video
{
    public class MenuRenderer
    {
        private readonly SpriteBatch _spriteBatch;
        private readonly SpriteFont _font;
        private readonly GraphicsDevice _graphicsDevice;
        
        // Menu colors
        private readonly Color _backgroundColor = new Color(0, 0, 20); // Dark blue
        private readonly Color _titleColor = Color.Cyan;
        private readonly Color _textColor = Color.White;
        private readonly Color _highlightColor = Color.Yellow;
        private readonly Color _gridColor = new Color(0, 100, 200, 100); // Semi-transparent blue

        // Animation
        private float _animationTimer;
        private float _pulseTimer;

        public MenuRenderer(SpriteBatch spriteBatch, SpriteFont font, GraphicsDevice graphicsDevice)
        {
            _spriteBatch = spriteBatch;
            _font = font;
            _graphicsDevice = graphicsDevice;
            _animationTimer = 0f;
            _pulseTimer = 0f;
        }

        public void Update(float deltaTime)
        {
            _animationTimer += deltaTime;
            _pulseTimer += deltaTime * 3f; // Faster pulse
        }

        public void DrawSplashScreen(GameTime gameTime)
        {
            var viewport = _graphicsDevice.Viewport;
            
            // Clear with dark background
            _graphicsDevice.Clear(_backgroundColor);

            _spriteBatch.Begin();

            // Draw title with glow effect
            string title = "GL TRON";
            var titleSize = _font.MeasureString(title);
            var titlePos = new Vector2(
                (viewport.Width - titleSize.X) / 2,
                viewport.Height / 3
            );

            // Glow effect
            float pulse = (float)(Math.Sin(_pulseTimer) * 0.3f + 0.7f);
            var glowColor = Color.Cyan * (pulse * 0.5f);
            
            // Draw multiple offset copies for glow
            for (int i = -2; i <= 2; i++)
            {
                for (int j = -2; j <= 2; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        _spriteBatch.DrawString(_font, title, 
                            titlePos + new Vector2(i, j), glowColor);
                    }
                }
            }
            
            // Draw main title
            _spriteBatch.DrawString(_font, title, titlePos, _titleColor * pulse);

            // Draw loading text
            string loadingText = "Loading...";
            var loadingSize = _font.MeasureString(loadingText);
            var loadingPos = new Vector2(
                (viewport.Width - loadingSize.X) / 2,
                viewport.Height * 0.7f
            );
            
            _spriteBatch.DrawString(_font, loadingText, loadingPos, _textColor * 0.8f);

            _spriteBatch.End();
        }

        public void DrawMainMenu(GameTime gameTime)
        {
            var viewport = _graphicsDevice.Viewport;
            
            // Clear with dark background
            _graphicsDevice.Clear(_backgroundColor);

            _spriteBatch.Begin();

            // Draw grid background
            DrawGridBackground(viewport);

            // Draw title
            string title = "GL TRON";
            var titleSize = _font.MeasureString(title);
            var titlePos = new Vector2(
                (viewport.Width - titleSize.X) / 2,
                viewport.Height / 6
            );

            // Animated title with pulse
            float pulse = (float)(Math.Sin(_pulseTimer) * 0.2f + 0.8f);
            var glowColor = Color.Cyan * (pulse * 0.3f);
            
            // Glow effect
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        _spriteBatch.DrawString(_font, title, 
                            titlePos + new Vector2(i * 2, j * 2), glowColor);
                    }
                }
            }
            
            _spriteBatch.DrawString(_font, title, titlePos, _titleColor);

            // Draw subtitle
            string subtitle = "Light Cycle Combat";
            var subtitleSize = _font.MeasureString(subtitle);
            var subtitlePos = new Vector2(
                (viewport.Width - subtitleSize.X) / 2,
                titlePos.Y + titleSize.Y + 10
            );
            _spriteBatch.DrawString(_font, subtitle, subtitlePos, _textColor * 0.7f);

            // Draw instructions
            float instructionY = viewport.Height / 2;
            
            string[] instructions = {
                "TAP TO START",
                "",
                "Controls:",
                "• Tap LEFT side to turn left",
                "• Tap RIGHT side to turn right",
                "• Avoid walls and trails",
                "",
                "Press BACK for settings"
            };

            for (int i = 0; i < instructions.Length; i++)
            {
                string instruction = instructions[i];
                if (string.IsNullOrEmpty(instruction)) continue;

                var textSize = _font.MeasureString(instruction);
                var textPos = new Vector2(
                    (viewport.Width - textSize.X) / 2,
                    instructionY + i * 30
                );

                Color textColor = _textColor;
                
                // Highlight "TAP TO START"
                if (i == 0)
                {
                    float tapPulse = (float)(Math.Sin(_pulseTimer * 2) * 0.3f + 0.7f);
                    textColor = Color.Lerp(_highlightColor, Color.White, tapPulse);
                    
                    // Add glow to tap instruction
                    _spriteBatch.DrawString(_font, instruction, 
                        textPos + new Vector2(-1, -1), _highlightColor * 0.3f);
                    _spriteBatch.DrawString(_font, instruction, 
                        textPos + new Vector2(1, 1), _highlightColor * 0.3f);
                }
                else if (instruction.StartsWith("•"))
                {
                    textColor = Color.LightGray;
                }

                _spriteBatch.DrawString(_font, instruction, textPos, textColor);
            }

            // Draw version info
            string version = "MonoGame Port v1.0";
            var versionSize = _font.MeasureString(version);
            var versionPos = new Vector2(
                viewport.Width - versionSize.X - 10,
                viewport.Height - versionSize.Y - 10
            );
            _spriteBatch.DrawString(_font, version, versionPos, _textColor * 0.5f);

            _spriteBatch.End();
        }

        public void DrawGameOverMenu(GameTime gameTime, bool playerWon, int score)
        {
            var viewport = _graphicsDevice.Viewport;
            
            _spriteBatch.Begin();

            // Semi-transparent overlay
            DrawOverlay(viewport, Color.Black * 0.7f);

            // Game Over title
            string title = playerWon ? "YOU WIN!" : "GAME OVER";
            var titleSize = _font.MeasureString(title);
            var titlePos = new Vector2(
                (viewport.Width - titleSize.X) / 2,
                viewport.Height / 3
            );

            Color titleColor = playerWon ? Color.Lime : Color.Red;
            float pulse = (float)(Math.Sin(_pulseTimer * 2) * 0.3f + 0.7f);
            
            // Glow effect
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        _spriteBatch.DrawString(_font, title, 
                            titlePos + new Vector2(i * 2, j * 2), titleColor * 0.3f);
                    }
                }
            }
            
            _spriteBatch.DrawString(_font, title, titlePos, titleColor * pulse);

            // Score
            string scoreText = $"Score: {score}";
            var scoreSize = _font.MeasureString(scoreText);
            var scorePos = new Vector2(
                (viewport.Width - scoreSize.X) / 2,
                titlePos.Y + titleSize.Y + 30
            );
            _spriteBatch.DrawString(_font, scoreText, scorePos, _textColor);

            // Instructions
            string restartText = "TAP TO RESTART";
            var restartSize = _font.MeasureString(restartText);
            var restartPos = new Vector2(
                (viewport.Width - restartSize.X) / 2,
                scorePos.Y + 60
            );
            
            float restartPulse = (float)(Math.Sin(_pulseTimer * 3) * 0.4f + 0.6f);
            _spriteBatch.DrawString(_font, restartText, restartPos, _highlightColor * restartPulse);

            _spriteBatch.End();
        }

        private void DrawGridBackground(Viewport viewport)
        {
            // Draw a simple grid pattern to simulate the game arena
            int gridSize = 50;
            
            // Vertical lines
            for (int x = 0; x < viewport.Width; x += gridSize)
            {
                DrawLine(new Vector2(x, 0), new Vector2(x, viewport.Height), _gridColor);
            }
            
            // Horizontal lines
            for (int y = 0; y < viewport.Height; y += gridSize)
            {
                DrawLine(new Vector2(0, y), new Vector2(viewport.Width, y), _gridColor);
            }
        }

        private void DrawLine(Vector2 start, Vector2 end, Color color)
        {
            // Simple line drawing using a 1x1 white texture (we'll create this if needed)
            // For now, just draw small rectangles to simulate lines
            Vector2 direction = end - start;
            float length = direction.Length();
            direction.Normalize();
            
            int segments = (int)(length / 2);
            for (int i = 0; i < segments; i += 4) // Dashed line effect
            {
                Vector2 pos = start + direction * i;
                // We would draw a small rectangle here, but without a pixel texture,
                // we'll skip this for now and just draw the grid with the font
            }
        }

        private void DrawOverlay(Viewport viewport, Color color)
        {
            // Draw a colored rectangle overlay
            // Since we don't have a pixel texture, we'll simulate with multiple small rectangles
            // This is a simplified approach - in a real implementation you'd use a 1x1 white texture
        }
    }
}
