using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using GltronMobileEngine.Video;
using GltronMobileEngine.Sound;
using Player = GltronMobileEngine.Player;

namespace GltronMobileGame
{
    public class GLTronGame
    {
        // Note: Camera and WorldGraphics are handled by Game1.cs, not here
        // private Camera _camera;  // Removed - unused
        // private WorldGraphics _world;  // Removed - unused


        // Multiplatform logging helper
        private static void LogInfo(string message)
        {
            System.Diagnostics.Debug.WriteLine($"GLTRON: {message}");
#if ANDROID
            try { Android.Util.Log.Info("GLTRON", message); } catch { }
#endif
        }
        
        private static void LogError(string message)
        {
            System.Diagnostics.Debug.WriteLine($"GLTRON: ERROR - {message}");
#if ANDROID
            try { Android.Util.Log.Error("GLTRON", message); } catch { }
#endif
        }
        
        private static void LogWarn(string message)
        {
            System.Diagnostics.Debug.WriteLine($"GLTRON: WARN - {message}");
#if ANDROID
            try { Android.Util.Log.Warn("GLTRON", message); } catch { }
#endif
        }

        // Define Time data
        public long TimeLastFrame;
        public long TimeCurrent;
        public long TimeDt;
        private const int DtHistSize = 8;
        private long[] DtHist = new long[DtHistSize];
        private int DtHead = 0;
        private int DtElements = 0;

        // Define arena setting 
        private float mCurrentGridSize = 100.0f; // Valor padrão

        public const int MAX_PLAYERS = 6;
        public const int OWN_PLAYER = 0;
        public int mCurrentPlayers = 4; // Original GLTron has 4 players

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
            // ContentManager must be provided by host Game; leave null until Initialize is called

            try
            {
                LogInfo("GLTronGame constructor: Starting initialization");
                
                // Initialize walls array first
                if (Walls == null)
                {
                    Walls = new Segment[4];
                }
                
                for (int i = 0; i < 4; i++)
                {
                    Walls[i] = new Segment();
                }
                LogInfo("GLTronGame constructor: Walls array initialized");
                
                initWalls();
                LogInfo("GLTronGame constructor: Walls initialized successfully");
                
                // Initialize Players array
                if (Players == null)
                {
                    Players = new Player[MAX_PLAYERS];
                }
                
                for (int i = 0; i < MAX_PLAYERS; i++)
                {
                    Players[i] = null; // Explicitly set to null initially
                }
                LogInfo("GLTronGame constructor: Players array initialized");
                
                LogInfo("GLTronGame constructor: Completed successfully");
            }
            catch (System.Exception ex)
            {
                LogError($"GLTronGame constructor failed: {ex}");
                throw;
            }
        }

        public int GetOwnPlayerScore()
        {
            try
            {
                if (Players == null || Players[OWN_PLAYER] == null) return 0;
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
                if (Players == null) return null;
                if (OWN_PLAYER >= Players.Length) return null;
                return Players[OWN_PLAYER];
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
                return Players ?? new Player[MAX_PLAYERS];
            }
            catch (System.Exception ex)
            {
                Android.Util.Log.Error("GLTRON", $"GetPlayers: Exception: {ex}");
                return new Player[MAX_PLAYERS];
            }
        }

        public GltronMobileEngine.Interfaces.ISegment[] GetWalls()
        {
            try
            {
                if (Walls == null)
                {
                    Walls = new Segment[4];
                    for (int i = 0; i < 4; i++) Walls[i] = new Segment();
                    initWalls();
                }
                return Walls;
            }
            catch (System.Exception ex)
            {
                Android.Util.Log.Error("GLTRON", $"GetWalls: Exception: {ex}");
                return new Segment[4];
            }
        }

        public bool IsShowingMenu()
        {
            return boShowMenu;
        }

        public float GetGridSize()
        {
            return mCurrentGridSize;
        }

        private void initWalls()
        {
            if (Walls == null || Walls.Length < 4) Walls = new Segment[4];
            float[,] raw = {
                { 0.0f, 0.0f, 1.0f, 0.0f },
                { 1.0f, 0.0f, 0.0f, 1.0f },
                { 1.0f, 1.0f, -1.0f, 0.0f },
                { 0.0f, 1.0f, 0.0f, -1.0f }
            };
            float w = mCurrentGridSize;
            float h = mCurrentGridSize;
            for (int j = 0; j < 4; j++)
            {
                if (Walls[j] == null) Walls[j] = new Segment();
                Walls[j].vStart.v[0] = raw[j, 0] * w;
                Walls[j].vStart.v[1] = raw[j, 1] * h;
                Walls[j].vDirection.v[0] = raw[j, 2] * w;
                Walls[j].vDirection.v[1] = raw[j, 3] * h;
            }
        }

        public void initialiseGame()
        {
            try
            {
                // Ensure walls are allocated and valid
                if (Walls == null) Walls = new Segment[4];
                for (int i = 0; i < 4; i++)
                {
                    if (Walls[i] == null)
                    {
                        LogWarn($"GLTronGame.initialiseGame: Wall {i} is null, creating new Segment");
                        Walls[i] = new Segment();
                    }
                }
                
                // Initialize walls geometry
                initWalls();
                LogInfo("GLTronGame.initialiseGame: Walls initialized");

                // Ensure players array
                if (Players == null)
                {
                    LogError("GLTronGame.initialiseGame: Players array is null!");
                    Players = new Player[MAX_PLAYERS];
                }

                // Initialize players with detailed logging
                LogInfo($"GLTronGame.initialiseGame: Creating {mCurrentPlayers} players (like original GLTron)");
                for (int player = 0; player < mCurrentPlayers; player++)
                {
                    try
                    {
                        LogInfo($"GLTronGame.initialiseGame: Creating player {player}");
                        Players[player] = new Player(player, mCurrentGridSize);

                        if (Players[player] == null)
                        {
                            LogError($"GLTronGame.initialiseGame: Player {player} creation returned null!");
                            continue;
                        }

                        Players[player].setSpeed(10.0f); // Set initial speed so players are active

                        // Log player starting position and direction
                        float x = Players[player].getXpos();
                        float y = Players[player].getYpos();
                        int dir = Players[player].getDirection();
                        LogInfo($"GLTronGame.initialiseGame: Player {player} created at ({x:F1},{y:F1}) facing direction {dir}");
                    }
                    catch (System.Exception ex)
                    {
                        LogError($"GLTronGame.initialiseGame: Failed to create player {player}: {ex}");
                        Players[player] = null;
                    }
                }

                // Verify OWN_PLAYER exists
                if (Players[OWN_PLAYER] == null)
                {
                    LogError("GLTronGame.initialiseGame: OWN_PLAYER is null after initialization!");
                }
                else
                {
                    LogInfo("GLTronGame.initialiseGame: OWN_PLAYER created successfully");
                }

                // CRITICAL: Initialize AI system (like Java version)
                try
                {
                    GltronMobileEngine.ComputerAI.InitAI(Walls, Players, mCurrentGridSize);
                    LogInfo("GLTronGame.initialiseGame: AI system initialized");
                }
                catch (System.Exception ex)
                {
                    LogError($"GLTronGame.initialiseGame: AI initialization failed: {ex}");
                }

                // Reset timing and flags
                ResetTime();
                LogInfo("GLTronGame.initialiseGame: Time reset");

                boLoading = false;
                boShowMenu = true;
                
                LogInfo("GLTronGame.initialiseGame: Completed successfully");
                LogInfo($"Players created: {mCurrentPlayers}");
                LogInfo($"Grid size: {mCurrentGridSize}");
                LogInfo($"OWN_PLAYER null check: {Players[OWN_PLAYER] == null}");
            }
            catch (System.Exception ex)
            {
                LogError($"GLTronGame.initialiseGame: CRITICAL FAILURE: {ex}");
                LogError($"GLTronGame.initialiseGame: Stack trace: {ex.StackTrace}");
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

        private void ResetTime()
        {
            TimeLastFrame = TimeCurrent = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            TimeDt = 0;
            DtHead = 0;
            DtElements = 0;
        }

        private void UpdateTime(GameTime gameTime)
        {
            // Prefer GameTime for MonoGame correctness
            long now = (long)gameTime.TotalGameTime.TotalMilliseconds;
            long last = TimeCurrent;
            if (last == 0)
            {
                last = now;
            }
            TimeLastFrame = last;
            TimeCurrent = now;
            long realDt = (long)gameTime.ElapsedGameTime.TotalMilliseconds;
            TimeDt = realDt;

            // Keep history (optional smoothing, matching Java code idea)
            DtHist[DtHead] = realDt;
            DtHead = (DtHead + 1) % DtHistSize;
            if (DtElements < DtHistSize) DtElements++;
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
                
                // CRITICAL: Update AI timing (like Java version)
                GltronMobileEngine.ComputerAI.UpdateTime(TimeDt, TimeCurrent);

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
                    ResetGame();
                    boProcessReset = false;
                }
                
                // CRITICAL: Run game logic (like Java version)
                if (!boInitialState && !boShowMenu)
                {
                    RunGameLogic();
                }
            }
            catch (System.Exception ex)
            {
                Android.Util.Log.Error("GLTRON", $"RunGame: Exception: {ex}");
            }
        }
        
        private void ResetGame()
        {
            try
            {
                SoundManager.Instance.StopEngine();
                
                // Reinitialize all players
                for (int plyr = 0; plyr < mCurrentPlayers; plyr++)
                {
                    Players[plyr] = new Player(plyr, mCurrentGridSize);
                    Players[plyr].setSpeed(10.0f);
                }
                
                // Reinitialize AI
                GltronMobileEngine.ComputerAI.InitAI(Walls, Players, mCurrentGridSize);
                
                // Reset game state
                boInitialState = true;
                
                // Reset HUD
                tronHUD?.ResetConsole();
                tronHUD?.DisplayInstr(true);
                
                LogInfo("Game reset completed");
            }
            catch (System.Exception ex)
            {
                LogError($"ResetGame failed: {ex}");
            }
        }
        
        private void RunGameLogic()
        {
            try
            {
                bool ownPlayerActive = true;
                bool otherPlayersActive = false;
                bool checkWinner = false;
                
                // Process all players (like Java version)
                for (int player = 0; player < mCurrentPlayers; player++)
                {
                    if (Players[player] == null) continue;
                    
                    // Move player
                    Players[player].doMovement(TimeDt, TimeCurrent, Walls, Players);
                    
                    // Check win/lose conditions
                    if (player == OWN_PLAYER)
                    {
                        if (Players[player].getSpeed() == 0.0f)
                            ownPlayerActive = false;
                    }
                    else
                    {
                        if (Players[player].getSpeed() > 0.0f)
                            otherPlayersActive = true;
                    }
                    
                    checkWinner = true;
                }
                
                // Round robin AI processing (like Java version)
                if (aiCount < mCurrentPlayers && aiCount > 0) // Skip OWN_PLAYER (index 0)
                {
                    if (Players[aiCount] != null && Players[aiCount].getTrailHeight() == Player.TRAIL_HEIGHT)
                    {
                        GltronMobileEngine.ComputerAI.DoComputer(aiCount, OWN_PLAYER);
                    }
                }
                
                aiCount++;
                if (aiCount >= mCurrentPlayers)
                    aiCount = 1; // Skip OWN_PLAYER
                
                // Manage sounds (like Java version)
                ManageSounds();
                
                // Check win/lose conditions
                if (checkWinner)
                {
                    CheckWinLoseConditions(ownPlayerActive, otherPlayersActive);
                }
            }
            catch (System.Exception ex)
            {
                LogError($"RunGameLogic failed: {ex}");
            }
        }
        
        private void ManageSounds()
        {
            try
            {
                if (Players[OWN_PLAYER] == null) return;
                
                if (Players[OWN_PLAYER].getSpeed() == 0.0f)
                {
                    SoundManager.Instance.StopEngine();
                    mEngineStartTime = 0;
                    mEngineSoundModifier = 1.0f;
                }
                else if (!boInitialState)
                {
                    if (mEngineSoundModifier < 1.5f)
                    {
                        if (mEngineStartTime != 0)
                        {
                            if ((TimeCurrent + 1000) > mEngineStartTime)
                            {
                                mEngineSoundModifier += 0.01f;
                                SoundManager.Instance.PlayEngine(mEngineSoundModifier, true);
                            }
                        }
                    }
                }
                
                mEngineStartTime = TimeCurrent;
            }
            catch (System.Exception ex)
            {
                LogError($"ManageSounds failed: {ex}");
            }
        }
        
        private void CheckWinLoseConditions(bool ownPlayerActive, bool otherPlayersActive)
        {
            try
            {
                if (!ownPlayerActive && otherPlayersActive)
                {
                    // Player lost
                    tronHUD?.DisplayLose();
                    LogInfo("Player lost the round");
                }
                else if (ownPlayerActive && !otherPlayersActive)
                {
                    // Player won
                    tronHUD?.DisplayWin();
                    if (Players[OWN_PLAYER] != null)
                    {
                        Players[OWN_PLAYER].setSpeed(0.0f);
                        Players[OWN_PLAYER].addScore(100); // Bonus for winning
                    }
                    LogInfo("Player won the round");
                }
            }
            catch (System.Exception ex)
            {
                LogError($"CheckWinLoseConditions failed: {ex}");
            }
        }
    }
}
