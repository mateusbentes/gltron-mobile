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
            
            // Always render continuously for a game
            RenderMode = Rendermode.Continuously;
            
            // IMPORTANT: Register view so MonoGame can create GraphicsDevice
            _game.Services.AddService(typeof(View), this);
            _game.Services.AddService(typeof(AndroidGameView), this);
        }

        public void OnSurfaceCreated(IGL10 gl, Javax.Microedition.Khronos.Egl.EGLConfig config)
        {
            Android.Util.Log.Debug("GLTRON", "AndroidGameView.OnSurfaceCreated");
            
            if (!_isInitialized)
            {
                try
                {
                    Android.Util.Log.Debug("GLTRON", "Initializing MonoGame with first RunOneFrame call");
                    _isInitialized = true;
                    
                    // First RunOneFrame initializes Game + GraphicsDevice
                    _game.RunOneFrame();
                    
                    Android.Util.Log.Debug("GLTRON", "MonoGame initialized successfully");
                }
                catch (Exception ex)
                {
                    Android.Util.Log.Error("GLTRON", $"MonoGame initialization failed: {ex.Message}");
                    Android.Util.Log.Error("GLTRON", $"Stack trace: {ex.StackTrace}");
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
            try
            {
                if (_isInitialized && _game != null)
                {
                    // Run one frame of the game
                    _game.RunOneFrame();
                }
            }
            catch (Exception ex)
            {
                Android.Util.Log.Error("GLTRON", $"Game frame failed: {ex.Message}");
                Android.Util.Log.Error("GLTRON", $"Stack trace: {ex.StackTrace}");
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
