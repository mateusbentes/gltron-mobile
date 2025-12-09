using System;
using Android.Content;
using Android.Views;
using Android.Opengl;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Javax.Microedition.Khronos.Opengles;
using Javax.Microedition.Khronos.Egl;

namespace GltronMobileGame
{
    public class AndroidGameView : GLSurfaceView, GLSurfaceView.IRenderer
    {
        private Game _game;
        private bool _isInitialized = false;

        public AndroidGameView(Context context, Game game) : base(context)
        {
            _game = game;
            
            // Set up OpenGL ES 2.0 context
            SetEGLContextClientVersion(2);
            SetRenderer(this);
            
            // Only render when explicitly requested
            RenderMode = Rendermode.Continuously;
        }

        public void OnSurfaceCreated(IGL10 gl, Javax.Microedition.Khronos.Egl.EGLConfig config)
        {
            Android.Util.Log.Debug("GLTRON", "AndroidGameView.OnSurfaceCreated");
            
            if (!_isInitialized)
            {
                try
                {
                    // The game will initialize itself when we call RunOneFrame
                    // We just need to set up the OpenGL context here
                    Android.Util.Log.Debug("GLTRON", "OpenGL surface created, game will initialize on first frame");
                    _isInitialized = true;
                }
                catch (Exception ex)
                {
                    Android.Util.Log.Error("GLTRON", $"Surface creation failed: {ex.Message}");
                }
            }
        }

        public void OnSurfaceChanged(IGL10 gl, int width, int height)
        {
            Android.Util.Log.Debug("GLTRON", $"AndroidGameView.OnSurfaceChanged: {width}x{height}");
            
            // Set the OpenGL viewport
            gl.GlViewport(0, 0, width, height);
            
            // Update game's graphics device if needed
            if (_game?.GraphicsDevice != null)
            {
                var graphicsDeviceManager = _game.Services.GetService(typeof(IGraphicsDeviceManager)) as GraphicsDeviceManager;
                if (graphicsDeviceManager != null)
                {
                    // Update the preferred back buffer size
                    graphicsDeviceManager.PreferredBackBufferWidth = width;
                    graphicsDeviceManager.PreferredBackBufferHeight = height;
                    graphicsDeviceManager.ApplyChanges();
                }
            }
        }

        public void OnDrawFrame(IGL10 gl)
        {
            if (_isInitialized && _game != null)
            {
                try
                {
                    // Run one frame of the game - this will handle initialization automatically
                    _game.RunOneFrame();
                }
                catch (Exception ex)
                {
                    Android.Util.Log.Error("GLTRON", $"Game frame failed: {ex.Message}");
                }
            }
        }

        protected override void OnDetachedFromWindow()
        {
            Android.Util.Log.Debug("GLTRON", "AndroidGameView.OnDetachedFromWindow");
            base.OnDetachedFromWindow();
        }

        public void Pause()
        {
            OnPause();
        }

        public void Resume()
        {
            OnResume();
        }
    }
}
