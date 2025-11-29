using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using GltronMobileEngine.Sound;
using Player = GltronMobileEngine.Player;

namespace GltronMobileGame
{
    public class GLTronGame
    {
        // Define Time data
        public long TimeLastFrame;
        public long TimeCurrent;
        public long TimeDt;

        // Define arena setting 
        private float mCurrentGridSize = 100.0f; // Valor padrão

        public const int MAX_PLAYERS = 6;
        public const int OWN_PLAYER = 0;
        public int mCurrentPlayers = 2; // Valor padrão

        // Define game textures (serão carregadas no MonoGame Game1)
        // private GLTexture ExplodeTex;
        // private GLTexture SplashScreen;

        // private Model LightBike; // Será carregado no MonoGame Game1
        // private Model RecognizerModel; // Será carregado no MonoGame Game1
        // private Video Visual; // Substituir por lógica de viewport/câmera MonoGame
        // private WorldGraphics World; // Substituir por lógica de renderização de mundo MonoGame
        // private Lighting Lights = new Lighting(); // Substituir por lógica de iluminação MonoGame

        private Player[] Players = new Player[MAX_PLAYERS];

        // private Recognizer mRecognizer; // Implementar depois

        // Camera data (Substituir por lógica de câmera MonoGame)
        // private Camera Cam;

        // Trails_Renderer TrailRenderer; // Substituir por lógica de trilhas MonoGame

        // input processing
        bool boProcessInput = false;
        bool boProcessReset = false;
        int inputDirection;

        bool boInitialState = true;
        private bool boLoading = true;
        private bool boShowMenu = true;

        // sound index (Serão usados como identificadores no MonoGame)
        public static int CRASH_SOUND = 1;
        public static int ENGINE_SOUND = 2;
        public static int MUSIC_SOUND = 3;
        public static int RECOGNIZER_SOUND = 4;

        float mEngineSoundModifier = 1.0f;
        long mEngineStartTime = 0;

        // Font (Substituir por lógica de HUD MonoGame)
        public GltronMobileEngine.Video.HUD tronHUD; // set from Game1

        // Ads (Não aplicável no MonoGame)
        // Handler _handler;

        // Context (Não aplicável no MonoGame)
        // Context mContext;
        // GL10 gl; // Não aplicável no MonoGame

        public Segment[] Walls = new Segment[4];

        private int aiCount = 1;

        // Preferences (Substituir por um sistema de preferências MonoGame)
        // public static UserPrefs mPrefs;

        public GLTronGame()
        {
            try
            {
                Android.Util.Log.Info("GLTRON", "GLTronGame constructor: Starting initialization");
                
                // Initialize walls array first
                for (int i = 0; i < 4; i++)
                {
                    Walls[i] = new Segment();
                }
                Android.Util.Log.Info("GLTRON", "GLTronGame constructor: Walls array initialized");
                
                initWalls();
                Android.Util.Log.Info("GLTRON", "GLTronGame constructor: Walls initialized successfully");
                
                // Initialize Players array
                for (int i = 0; i < MAX_PLAYERS; i++)
                {
                    Players[i] = null; // Explicitly set to null initially
                }
                Android.Util.Log.Info("GLTRON", "GLTronGame constructor: Players array initialized");
                
                Android.Util.Log.Info("GLTRON", "GLTronGame constructor: Completed successfully");
            }
            catch (System.Exception ex)
            {
                Android.Util.Log.Error("GLTRON", $"GLTronGame constructor failed: {ex}");
                throw;
            }
        }

        public int GetOwnPlayerScore()
        {
            try
            {
                if (Players == null)
                {
                    Android.Util.Log.Warn("GLTRON", "GetOwnPlayerScore: Players array is null");
                    return 0;
                }
                
                if (Players[OWN_PLAYER] == null)
                {
                    Android.Util.Log.Warn("GLTRON", "GetOwnPlayerScore: OWN_PLAYER is null");
                    return 0;
                }
                
                return Players[OWN_PLAYER].getScore();
            }
            catch (System.Exception ex)
            {
                Android.Util.Log.Error("GLTRON", $"GetOwnPlayerScore: Exception: {ex}");
                return 0;
            }
        }

        public Player GetOwnPlayer()
        {
            try
            {
                if (Players == null)
                {
                    Android.Util.Log.Warn("GLTRON", "GetOwnPlayer: Players array is null");
                    return null;
                }
                
                if (OWN_PLAYER >= Players.Length)
                {
                    Android.Util.Log.Error("GLTRON", $"GetOwnPlayer: OWN_PLAYER index {OWN_PLAYER} out of bounds");
                    return null;
                }
                
                var player = Players[OWN_PLAYER];
                if (player == null)
                {
                    Android.Util.Log.Warn("GLTRON", "GetOwnPlayer: OWN_PLAYER is null");
                }
                
                return player;
            }
            catch (System.Exception ex)
            {
                Android.Util.Log.Error("GLTRON", $"GetOwnPlayer: Exception: {ex}");
                return null;
            }
        }

        public Player[] GetPlayers()
        {
            try
            {
                if (Players == null)
                {
                    Android.Util.Log.Warn("GLTRON", "GetPlayers: Players array is null");
                    return new Player[MAX_PLAYERS]; // Return empty array instead of null
                }
                
                return Players;
            }
            catch (System.Exception ex)
            {
                Android.Util.Log.Error("GLTRON", $"GetPlayers: Exception: {ex}");
                return new Player[MAX_PLAYERS]; // Return empty array on error
            }
        }

        public GltronMobileEngine.Interfaces.ISegment[] GetWalls()
        {
            try
            {
                if (Walls == null)
                {
                    Android.Util.Log.Warn("GLTRON", "GetWalls: Walls array is null, reinitializing");
                    Walls = new Segment[4];
                    for (int i = 0; i < 4; i++)
                    {
                        Walls[i] = new Segment();
                    }
                    initWalls();
                }
                
                return Walls;
            }
            catch (System.Exception ex)
            {
                Android.Util.Log.Error("GLTRON", $"GetWalls: Exception: {ex}");
                return new Segment[4]; // Return empty array on error
            }
        }

        public bool IsShowingMenu()
        {
            return boShowMenu;
        }

        public void initialiseGame()
        {
            try
            {
                Android.Util.Log.Info("GLTRON", "GLTronGame.initialiseGame: Starting");
                
                // Verify walls are initialized
                if (Walls == null)
                {
                    Android.Util.Log.Error("GLTRON", "GLTronGame.initialiseGame: Walls array is null!");
                    Walls = new Segment[4];
                }
                
                for (int i = 0; i < 4; i++)
                {
                    if (Walls[i] == null)
                    {
                        Android.Util.Log.Warn("GLTRON", $"GLTronGame.initialiseGame: Wall {i} is null, creating new Segment");
                        Walls[i] = new Segment();
                    }
                }
                
                initWalls();
                Android.Util.Log.Info("GLTRON", "GLTronGame.initialiseGame: Walls reinitialized");

                // Verify Players array
                if (Players == null)
                {
                    Android.Util.Log.Error("GLTRON", "GLTronGame.initialiseGame: Players array is null!");
                    Players = new Player[MAX_PLAYERS];
                }

                // Initialize players with detailed logging
                Android.Util.Log.Info("GLTRON", $"GLTronGame.initialiseGame: Creating {mCurrentPlayers} players");
                for (int player = 0; player < mCurrentPlayers; player++)
                {
                    try
                    {
                        Android.Util.Log.Info("GLTRON", $"GLTronGame.initialiseGame: Creating player {player}");
                        Players[player] = new Player(player, mCurrentGridSize);
                        
                        if (Players[player] == null)
                        {
                            Android.Util.Log.Error("GLTRON", $"GLTronGame.initialiseGame: Player {player} creation returned null!");
                            continue;
                        }
                        
                        Players[player].setSpeed(10.0f); // Set initial speed so players are active
                        Android.Util.Log.Info("GLTRON", $"GLTronGame.initialiseGame: Player {player} created and speed set");
                    }
                    catch (System.Exception ex)
                    {
                        Android.Util.Log.Error("GLTRON", $"GLTronGame.initialiseGame: Failed to create player {player}: {ex}");
                        Players[player] = null;
                    }
                }

                // Verify OWN_PLAYER exists
                if (Players[OWN_PLAYER] == null)
                {
                    Android.Util.Log.Error("GLTRON", "GLTronGame.initialiseGame: OWN_PLAYER is null after initialization!");
                }
                else
                {
                    Android.Util.Log.Info("GLTRON", "GLTronGame.initialiseGame: OWN_PLAYER created successfully");
                }

                ResetTime();
                Android.Util.Log.Info("GLTRON", "GLTronGame.initialiseGame: Time reset");

                boLoading = false;
                boShowMenu = true;
                
                Android.Util.Log.Info("GLTRON", "GLTronGame.initialiseGame: Completed successfully"); 
                Android.Util.Log.Info("GLTRON", $"Players created: {mCurrentPlayers}");
                Android.Util.Log.Info("GLTRON", $"Grid size: {mCurrentGridSize}");
                Android.Util.Log.Info("GLTRON", $"OWN_PLAYER null check: {Players[OWN_PLAYER] == null}");
            }
            catch (System.Exception ex)
            {
                Android.Util.Log.Error("GLTRON", $"GLTronGame.initialiseGame: CRITICAL FAILURE: {ex}");
                Android.Util.Log.Error("GLTRON", $"GLTronGame.initialiseGame: Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        // Métodos de ciclo de vida (pauseGame, resumeGame) serão adaptados para o MonoGame

        // drawSplash será adaptado para o MonoGame

        public void updateScreenSize(int width, int height)
        {
            try 
            { 
                Android.Util.Log.Info("GLTRON", $"Screen size updated: {width}x{height}");
                
                // Ensure we're in landscape mode
                if (width < height)
                {
                    Android.Util.Log.Warn("GLTRON", "Screen appears to be in portrait mode!");
                }
                
                // Update any screen-dependent calculations here
                // For now, just log the info
            } 
            catch { }
        }

        public void addTouchEvent(float x, float y, int screenWidth, int screenHeight)
        {
            try
            {
                Android.Util.Log.Info("GLTRON", $"addTouchEvent: x={x}, y={y}, screen={screenWidth}x{screenHeight}");
                
                if (boLoading)
                {
                    Android.Util.Log.Info("GLTRON", "addTouchEvent: Game is loading, ignoring touch");
                    return;
                }

                // Handle menu state
                if (boShowMenu)
                {
                    Android.Util.Log.Info("GLTRON", "addTouchEvent: Menu tap detected, starting game");
                    boShowMenu = false;
                    tronHUD?.AddLineToConsole("Game Started!");
                    tronHUD?.DisplayInstr(true);
                    return;
                }

                // Null check for Players array and OWN_PLAYER
                if (Players == null)
                {
                    Android.Util.Log.Error("GLTRON", "addTouchEvent: Players array is null");
                    return;
                }
                
                if (Players[OWN_PLAYER] == null)
                {
                    Android.Util.Log.Error("GLTRON", "addTouchEvent: OWN_PLAYER is null");
                    return;
                }

                if (Players[OWN_PLAYER].getSpeed() > 0.0f)
            {
                if (boInitialState)
                {
                    // Hide instructions and start engine after first tap
                    tronHUD?.DisplayInstr(false);
                    tronHUD?.AddLineToConsole($"Player {OWN_PLAYER} started");
                    SoundManager.Instance.PlayEngine(0.3f, true);
                    boInitialState = false;
                }
                else
                {
                    if (x <= (screenWidth / 2))
                    {
                        inputDirection = Player.TURN_LEFT;
                    }
                    else
                    {
                        inputDirection = Player.TURN_RIGHT;
                    }
                    boProcessInput = true;
                }
            }
            else
            {
                // Reiniciar o jogador
                if (Players[OWN_PLAYER] != null && Players[OWN_PLAYER].getTrailHeight() <= 0.0f)
                {
                    Android.Util.Log.Info("GLTRON", "addTouchEvent: Player reset requested");
                    boProcessReset = true;
                }
            }
            }
            catch (System.Exception ex)
            {
                Android.Util.Log.Error("GLTRON", $"addTouchEvent: Exception: {ex}");
            }
        }

        public void RunGame(GameTime gameTime)
        {
            try
            {
                if (gameTime == null)
                {
                    Android.Util.Log.Error("GLTRON", "RunGame: gameTime is null");
                    return;
                }
                
                UpdateTime(gameTime);
                // ComputerAI.updateTime(TimeDt, TimeCurrent); // Implementar depois

                if (boProcessInput)
                {
                    if (Players == null)
                    {
                        Android.Util.Log.Error("GLTRON", "RunGame: Players array is null during input processing");
                        boProcessInput = false;
                        return;
                    }
                    
                    if (Players[OWN_PLAYER] == null)
                    {
                        Android.Util.Log.Error("GLTRON", "RunGame: OWN_PLAYER is null during input processing");
                        boProcessInput = false;
                        return;
                    }
                    
                    Players[OWN_PLAYER].doTurn(inputDirection, TimeCurrent);
                    mEngineSoundModifier = 1.3f;
                    boProcessInput = false;
                }

            if (boProcessReset)
            {
                // Lógica de reset (refresh preferences, re-init world, etc.)
                // Por enquanto, apenas recria os jogadores
                SoundManager.Instance.StopEngine();
                for (int plyr = 0; plyr < mCurrentPlayers; plyr++)
                {
                    Players[plyr] = new Player(plyr, mCurrentGridSize);
                    Players[plyr].setSpeed(10.0f); // Velocidade padrão
                }

                // mRecognizer = new Recognizer(mCurrentGridSize); // Implementar depois

                // tronHUD.resetConsole(); // Implementar depois
                // Cam = new Camera(Players[OWN_PLAYER], CamType.E_CAM_TYPE_CIRCLING); // Implementar depois

                // Lógica de som (parar/iniciar)

                // tronHUD.displayInstr(true); // Implementar depois

                boInitialState = true;
                boProcessReset = false;
            }

            // round robin AI (Implementar depois)
            // if (Players[aiCount].getTrailHeight() == Players[aiCount].TRAIL_HEIGHT)
            // {
            //     ComputerAI.doComputer(aiCount, OWN_PLAYER);
            // }

            // Manage sounds (Implementar depois)

            // aiCount++;
            // if (aiCount > (mCurrentPlayers - 1))
            //     aiCount = 1;

            // RenderGame(); // Será chamado pelo Game1.Draw()

            // Movimento dos jogadores
            if (Players != null)
            {
                for (int player = 0; player < mCurrentPlayers && player < Players.Length; player++)
                {
                    if (Players[player] != null)
                    {
                        try
                        {
                            Players[player].doMovement(TimeDt, TimeCurrent, Walls, Players);
                        }
                        catch (System.Exception ex)
                        {
                            Android.Util.Log.Error("GLTRON", $"RunGame: Player {player} movement error: {ex}");
                        }
                    }
                }
            }
            }
            catch (System.Exception ex)
            {
                Android.Util.Log.Error("GLTRON", $"RunGame: Exception: {ex}");
            }
        }

        // DT smoothing experiment (Adaptado para MonoGame)
        private const int MAX_SAMPLES = 20;
        private long[] DtHist = new long[MAX_SAMPLES];
        private int DtHead = 0;
        private int DtElements = 0;

        private void ResetTime()
        {
            TimeLastFrame = Environment.TickCount;
            TimeCurrent = TimeLastFrame;
            TimeDt = 0;
            DtHead = 0;
            DtElements = 0;
        }

        private void UpdateTime(GameTime gameTime)
        {
            // Usar GameTime para DT
            TimeDt = (long)gameTime.ElapsedGameTime.TotalMilliseconds;
            TimeCurrent = Environment.TickCount;

            // Lógica de suavização de DT (mantida para fidelidade, mas pode ser simplificada)
            long RealDt = TimeDt;

            DtHist[DtHead] = RealDt;

            DtHead++;

            if (DtHead >= MAX_SAMPLES)
            {
                DtHead = 0;
            }

            if (DtElements == MAX_SAMPLES)
            {
                // Average the last MAX_SAMPLE DT's
                TimeDt = 0;
                for (int i = 0; i < MAX_SAMPLES; i++)
                {
                    TimeDt += DtHist[i];
                }
                TimeDt /= MAX_SAMPLES;
            }
            else
            {
                TimeDt = RealDt;
                DtElements++;
            }
        }

        private void initWalls()
        {
            float[,] raw = {
                { 0.0f, 0.0f, 1.0f, 0.0f },
                { 1.0f, 0.0f, 0.0f, 1.0f },
                { 1.0f, 1.0f, -1.0f, 0.0f },
                { 0.0f, 1.0f, 0.0f, -1.0f }
            };

            float width = mCurrentGridSize;
            float height = mCurrentGridSize;

            for (int j = 0; j < 4; j++)
            {
                Walls[j] = new Segment();
                Walls[j].vStart.v[0] = raw[j, 0] * width;
                Walls[j].vStart.v[1] = raw[j, 1] * height;
                Walls[j].vDirection.v[0] = raw[j, 2] * width;
                Walls[j].vDirection.v[1] = raw[j, 3] * height;
            }
        }

        // RenderGame será adaptado para o MonoGame Game1.Draw()
        public void RenderGame(GraphicsDevice graphicsDevice)
        {
            // Show menu state
            if (boShowMenu)
            {
                // Menu is handled by HUD
                return;
            }

            // Lógica de renderização 3D será implementada no Game1.Draw()
            // Por enquanto, apenas a lógica de checagem de vencedor

            bool boOwnPlayerActive = true;
            bool boOtherPlayersActive = false;
            bool boCheckWinner = false;

            if (!boInitialState)
            {
                for (int player = 0; player < mCurrentPlayers; player++)
                {
                    // Players[player].doMovement(TimeDt, TimeCurrent, Walls, Players); // Já feito no RunGame

                    // check win lose should be in game logic not render - FIXME
                    if (player == OWN_PLAYER)
                    {
                        if (Players[player].getSpeed() == 0.0f)
                            boOwnPlayerActive = false;
                    }
                    else
                    {
                        if (Players[player].getSpeed() > 0.0f)
                            boOtherPlayersActive = true;
                    }

                    boCheckWinner = true;

                }

                // mRecognizer.doMovement(TimeDt); // Implementar depois
            }

            // Cam.doCameraMovement(Players[OWN_PLAYER], TimeCurrent, TimeDt); // Implementar depois

            // Configurações de OpenGL (MonoGame)
            graphicsDevice.Clear(Color.Black);

            // Renderização de mundo, ciclos, trilhas, HUD (HUD drawn in Game1)

            if (boCheckWinner)
            {
                if (!boOwnPlayerActive && boOtherPlayersActive)
                {
                    tronHUD?.DisplayLose();
                    tronHUD?.AddLineToConsole("You Lose!");
                }
                else if (boOwnPlayerActive && !boOtherPlayersActive)
                {
                    tronHUD?.DisplayWin();
                    tronHUD?.AddLineToConsole("You Win!");
                    Players[OWN_PLAYER].setSpeed(0.0f);
                }
            }

            // tronHUD.draw(Visual, TimeDt, Players[OWN_PLAYER].getScore()); // Implementar depois
        }
    }
}
