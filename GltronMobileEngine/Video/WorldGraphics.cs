using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using GltronMobileEngine.Interfaces;

namespace GltronMobileEngine.Video;

public class WorldGraphics
{
    public BasicEffect Effect { get; private set; }
    private readonly GraphicsDevice _gd;
    private float _gridSize = 100f;

    private Texture2D? _texFloor;
    private Texture2D? _texWall;
    private readonly Texture2D?[] _skyFaces = new Texture2D?[6];
    
    // FBX Models
    private Model? _bikeModel;
    private Model? _recognizerModel;
    private Matrix[]? _bikeBoneTransforms;
    private Matrix[]? _recognizerBoneTransforms;
    private bool _useFbxModels = false;
    
    // Content loading state
    private bool _contentLoaded = false;
    
    // Fallback procedural geometry
    private const float RECOGNIZER_SIZE = 2.0f;

    public WorldGraphics(GraphicsDevice gd, ContentManager cm)
    {
        try
        {
#if ANDROID
            #if ANDROID
Android.Util.Log.Error("GLTRON", "🏗️ WorldGraphics constructor called!");
#endif
#endif
        }
        catch { }
        
        _gd = gd;
        Effect = new BasicEffect(gd)
        {
            TextureEnabled = true,
            LightingEnabled = false,
            VertexColorEnabled = false
        };
        
        try
        {
#if ANDROID
            #if ANDROID
Android.Util.Log.Error("GLTRON", "✅ WorldGraphics constructor completed!");
#endif
#endif
        }
        catch { }
    }

    public void LoadContent(ContentManager content)
    {
        try
        {
#if ANDROID
            #if ANDROID
Android.Util.Log.Error("GLTRON", "🚀🚀🚀 WorldGraphics.LoadContent() STARTED! 🚀🚀🚀");
#endif
#endif
        }
        catch { }
        
        System.Diagnostics.Debug.WriteLine("GLTRON: LoadContent called!");
        
        try
        {
#if ANDROID
        Android.Util.Log.Error("GLTRON", "About to load textures...");
#endif
        }
        catch { }
        
        // Load textures with XNB-first, PNG-fallback
        _texFloor = TryLoadTexture(content, _gd, "Assets/gltron_floor", "Content/Assets/gltron_floor.png");
        _texWall = TryLoadTexture(content, _gd, "Assets/gltron_wall_1", "Content/Assets/gltron_wall_1.png");
        _skyFaces[0] = TryLoadTexture(content, _gd, "Assets/skybox0", "Content/Assets/skybox0.png");
        _skyFaces[1] = TryLoadTexture(content, _gd, "Assets/skybox1", "Content/Assets/skybox1.png");
        _skyFaces[2] = TryLoadTexture(content, _gd, "Assets/skybox2", "Content/Assets/skybox2.png");
        _skyFaces[3] = TryLoadTexture(content, _gd, "Assets/skybox3", "Content/Assets/skybox3.png");
        _skyFaces[4] = TryLoadTexture(content, _gd, "Assets/skybox4", "Content/Assets/skybox4.png");
        _skyFaces[5] = TryLoadTexture(content, _gd, "Assets/skybox5", "Content/Assets/skybox5.png");
        
        try
        {
#if ANDROID
            #if ANDROID
Android.Util.Log.Info("GLTRON", "✅ Textures loaded successfully!");
#endif
#endif
        }
        catch { }
        
        // Run comprehensive diagnostics
        try
        {
#if ANDROID
        Android.Util.Log.Info("GLTRON", "=== STARTING MODEL LOADING DIAGNOSTICS ===");
#endif
            System.Diagnostics.Debug.WriteLine("GLTRON: === STARTING MODEL LOADING DIAGNOSTICS ===");
        }
        catch { }
        
        ModelDiagnostics.DiagnoseModelLoading(content);
        
        Texture2D? TryLoadTexture(ContentManager cm, GraphicsDevice gd, string xnbPath, string pngPath)
        {
            try { return cm.Load<Texture2D>(xnbPath); } catch {}
            try
            {
                using var s = TitleContainer.OpenStream(pngPath);
                return Texture2D.FromStream(gd, s);
            }
            catch {}
            return null;
        }
        
        // Diagnostics: log content root and expected XNB presence
        string __root = "<null>";
        try
        {
            __root = content.RootDirectory ?? "<null>";
            System.Diagnostics.Debug.WriteLine($"GLTRON: Content.RootDirectory='{__root}'");
        }
        catch {}
        string[] candidates;
        try
        {
            candidates = new [] {
                System.IO.Path.Combine(__root, "Assets/lightcyclehigh.xnb"),
                System.IO.Path.Combine(__root, "Assets/recognizerhigh.xnb"),
                System.IO.Path.Combine(__root, "lightcyclehigh.xnb"),
                System.IO.Path.Combine(__root, "recognizerhigh.xnb")
            };
                foreach (var c in candidates)
                {
                    bool exists = false;
                    try { exists = System.IO.File.Exists(c); } catch {}
                    System.Diagnostics.Debug.WriteLine($"GLTRON: Probe XNB exists? {exists} -> '{c}'");
#if ANDROID
                    Android.Util.Log.Error("GLTRON", $"Probe XNB exists? {exists} -> '{c}'");
#endif 
                }
            } catch { }

        // Try to load models in order of preference: FBX -> OBJ -> Procedural
        _useFbxModels = TryLoadModels(content);

        // Fallback probes: try alternate content names without "Assets/" if initial attempt failed
        if (!_useFbxModels)
        {
            try
            {
                // Try bike
                _bikeModel = null;
                _bikeBoneTransforms = null;
                Model? tmpBike = null;
                try { tmpBike = content.Load<Model>("Assets/lightcyclehigh"); } catch {}
                if (tmpBike == null)
                {
                    try { tmpBike = content.Load<Model>("lightcyclehigh"); } catch {}
                }
                if (tmpBike != null)
                {
                    _bikeModel = tmpBike;
                    _bikeBoneTransforms = new Matrix[_bikeModel.Bones.Count];
                    _bikeModel.CopyAbsoluteBoneTransformsTo(_bikeBoneTransforms);
#if ANDROID
                    #if ANDROID
Android.Util.Log.Info("GLTRON", "FBX model loaded via probe: 'lightcyclehigh' or 'Assets/lightcyclehigh'");
#endif
#endif
                }
                // Try recognizer
                _recognizerModel = null;
                _recognizerBoneTransforms = null;
                Model? tmpRec = null;
                try { tmpRec = content.Load<Model>("Assets/recognizerhigh"); } catch {}
                if (tmpRec == null)
                {
                    try { tmpRec = content.Load<Model>("recognizerhigh"); } catch {}
                }
                if (tmpRec != null)
                {
                    _recognizerModel = tmpRec;
                    _recognizerBoneTransforms = new Matrix[_recognizerModel.Bones.Count];
                    _recognizerModel.CopyAbsoluteBoneTransformsTo(_recognizerBoneTransforms);
#if ANDROID
                    Android.Util.Log.Info("GLTRON", "FBX recognizer loaded via probe: 'recognizerhigh' or 'Assets/recognizerhigh'");
#endif
                }
                _useFbxModels = _bikeModel != null && _recognizerModel != null;
            }
            catch {}
        }
        
        // Mark that LoadContent has been successfully called
        _contentLoaded = true;
        
        try
        {
#if ANDROID
            Android.Util.Log.Info("GLTRON", $"=== MODEL LOADING COMPLETE - Using FBX: {_useFbxModels} ===");
            Android.Util.Log.Info("GLTRON", "🏁 WorldGraphics.LoadContent() FINISHED!");
#endif
            System.Diagnostics.Debug.WriteLine($"GLTRON: === MODEL LOADING COMPLETE - Using FBX: {_useFbxModels} ===");
        }
        catch { }
    }
    
    /// <summary>
    /// Create a distinctive motorcycle shape using procedural geometry
    /// </summary>
    private void DrawProceduralMotorcycle(BasicEffect fx)
    {
        var allVertices = new List<VertexPositionNormalTexture>();
        
        // MAIN BODY - sleek motorcycle chassis
        var mainBodyVertices = CreateBoxVertices(new Vector3(0f, 0.3f, 0f), new Vector3(0.8f, 0.4f, 3.5f));
        allVertices.AddRange(mainBodyVertices);
        
        // ENGINE BLOCK - prominent center piece
        var engineVertices = CreateBoxVertices(new Vector3(0f, 0.5f, 0.2f), new Vector3(1.0f, 0.7f, 1.2f));
        allVertices.AddRange(engineVertices);
        
        // SEAT - raised rider position
        var seatVertices = CreateBoxVertices(new Vector3(0f, 0.9f, -0.8f), new Vector3(0.9f, 0.3f, 1.0f));
        allVertices.AddRange(seatVertices);
        
        // FRONT FAIRING - aerodynamic nose
        var fairingVertices = CreateBoxVertices(new Vector3(0f, 0.6f, 1.6f), new Vector3(0.7f, 0.5f, 0.8f));
        allVertices.AddRange(fairingVertices);
        
        // WHEELS - most distinctive motorcycle feature
        var frontWheelVertices = CreateWheelVertices(new Vector3(0f, 0.0f, 1.5f), 0.7f, 0.2f);
        allVertices.AddRange(frontWheelVertices);
        
        var rearWheelVertices = CreateWheelVertices(new Vector3(0f, 0.0f, -1.5f), 0.7f, 0.2f);
        allVertices.AddRange(rearWheelVertices);
        
        // HANDLEBARS - wide, visible steering
        var handlebarsVertices = CreateBoxVertices(new Vector3(0f, 1.3f, 1.3f), new Vector3(1.4f, 0.1f, 0.1f));
        allVertices.AddRange(handlebarsVertices);
        
        // FRONT FORK - connects wheel to handlebars
        var forkVertices = CreateBoxVertices(new Vector3(0f, 0.65f, 1.4f), new Vector3(0.15f, 1.3f, 0.15f));
        allVertices.AddRange(forkVertices);
        
        // EXHAUST PIPES - dual pipes for authenticity
        var exhaust1Vertices = CreateBoxVertices(new Vector3(0.4f, 0.2f, -0.5f), new Vector3(0.1f, 0.1f, 1.8f));
        allVertices.AddRange(exhaust1Vertices);
        
        var exhaust2Vertices = CreateBoxVertices(new Vector3(-0.4f, 0.2f, -0.5f), new Vector3(0.1f, 0.1f, 1.8f));
        allVertices.AddRange(exhaust2Vertices);
        
        // Render all geometry
        RenderVertexList(fx, allVertices);
    }
    
    /// <summary>
    /// Create an alien-looking recognizer using procedural geometry
    /// </summary>
    private void DrawProceduralRecognizer(BasicEffect fx)
    {
        var allVertices = new List<VertexPositionNormalTexture>();
        
        // MAIN BODY - central alien core (octahedron)
        var coreVertices = CreateOctahedronVertices(Vector3.Zero, 1.2f);
        allVertices.AddRange(coreVertices);
        
        // FLOATING RINGS - alien technology aesthetic
        var ring1Vertices = CreateTorusVertices(new Vector3(0f, 0.8f, 0f), 0.8f, 0.15f, 12, 6);
        allVertices.AddRange(ring1Vertices);
        
        var ring2Vertices = CreateTorusVertices(new Vector3(0f, -0.8f, 0f), 0.8f, 0.15f, 12, 6);
        allVertices.AddRange(ring2Vertices);
        
        // ENERGY SPIKES - protruding alien elements
        var spike1Vertices = CreateSpikeVertices(new Vector3(1.0f, 0f, 0f), 0.6f, 0.1f);
        allVertices.AddRange(spike1Vertices);
        
        var spike2Vertices = CreateSpikeVertices(new Vector3(-1.0f, 0f, 0f), 0.6f, 0.1f);
        allVertices.AddRange(spike2Vertices);
        
        var spike3Vertices = CreateSpikeVertices(new Vector3(0f, 0f, 1.0f), 0.6f, 0.1f);
        allVertices.AddRange(spike3Vertices);
        
        var spike4Vertices = CreateSpikeVertices(new Vector3(0f, 0f, -1.0f), 0.6f, 0.1f);
        allVertices.AddRange(spike4Vertices);
        
        // CENTRAL CRYSTAL - glowing alien core
        var crystalVertices = CreateOctahedronVertices(Vector3.Zero, 0.4f);
        allVertices.AddRange(crystalVertices);
        
        // Render all geometry
        RenderVertexList(fx, allVertices);
    }

    public void SetGridSize(float gridSize)
    {
        _gridSize = gridSize;
        System.Diagnostics.Debug.WriteLine($"GLTRON: WorldGraphics grid size set to {gridSize}");
    }

    public void BeginDraw(Matrix view, Matrix proj)
    {
        // Test if LoadContent was actually called
        if (!_contentLoaded)
        {
            try
            {
#if ANDROID
                Android.Util.Log.Error("GLTRON", "❌ CRITICAL: WorldGraphics.LoadContent() was never called!");
#endif
            }
            catch { }
        }
        
        Effect.View = view;
        Effect.Projection = proj;
        Effect.World = Matrix.Identity;
    }

    public void DrawFloor()
    {
        if (_texFloor == null) return;
        
        // CRITICAL FIX: Ensure proper render states for floor
        _gd.BlendState = BlendState.AlphaBlend;
        _gd.DepthStencilState = DepthStencilState.Default;
        _gd.RasterizerState = RasterizerState.CullNone;
        Effect.Texture = _texFloor;
        Effect.World = Matrix.Identity;
        
        // CRITICAL FIX: Floor should be neutral colored
        Effect.DiffuseColor = Vector3.One;
        Effect.Alpha = 1.0f;

        // SIMPLE FIX: Use player coordinate system directly
        // Players use coordinates from 0 to gridSize, so floor should match exactly
        float arenaSize = _gridSize;
        int tileSize = Math.Max(1, (int)(arenaSize / 10f)); // 10 tiles across for good detail
        float uvScale = 1.0f;

        System.Diagnostics.Debug.WriteLine($"GLTRON: Floor rendering - ArenaSize: {arenaSize}, TileSize: {tileSize}");

        foreach (var pass in Effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            
            for (int x = 0; x < (int)arenaSize; x += tileSize)
            {
                for (int z = 0; z < (int)arenaSize; z += tileSize)
                {
                    // Ensure we don't go beyond arena bounds
                    int actualTileX = Math.Min(tileSize, (int)arenaSize - x);
                    int actualTileZ = Math.Min(tileSize, (int)arenaSize - z);
                    
                    var verts = new VertexPositionTexture[4];
                    verts[0] = new VertexPositionTexture(new Vector3(x, 0, z), new Vector2(0f, 0f));
                    verts[1] = new VertexPositionTexture(new Vector3(x + actualTileX, 0, z), new Vector2(uvScale, 0f));
                    verts[2] = new VertexPositionTexture(new Vector3(x, 0, z + actualTileZ), new Vector2(0f, uvScale));
                    verts[3] = new VertexPositionTexture(new Vector3(x + actualTileX, 0, z + actualTileZ), new Vector2(uvScale, uvScale));

                    DrawQuad(verts);
                }
            }
        }
    }

    public void DrawWalls(ISegment[] walls)
    {
        if (_texWall == null) return;

        Effect.World = Matrix.Identity;
        Effect.Texture = _texWall;
        
        // CRITICAL FIX: Walls should be white/neutral colored
        Effect.DiffuseColor = Vector3.One;
        Effect.Alpha = 1.0f;

        // CRITICAL FIX: Use CullNone for proper wall visibility
        _gd.RasterizerState = RasterizerState.CullNone;
        _gd.BlendState = BlendState.AlphaBlend;
        _gd.DepthStencilState = DepthStencilState.Default;

        // SIMPLE FIX: Draw walls around the arena perimeter (ignore buggy collision data)
        // Create a simple square arena that matches the floor
        float arenaSize = _gridSize;
        float wallHeight = arenaSize * 0.3f; // Proportional wall height
        
        System.Diagnostics.Debug.WriteLine($"GLTRON: Drawing simple arena walls - Size: {arenaSize}, Height: {wallHeight}");

        foreach (var pass in Effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            
            // Draw 4 simple walls around the arena perimeter
            DrawSimpleWall(0, 0, arenaSize, 0, wallHeight);           // Bottom wall
            DrawSimpleWall(arenaSize, 0, arenaSize, arenaSize, wallHeight); // Right wall  
            DrawSimpleWall(arenaSize, arenaSize, 0, arenaSize, wallHeight); // Top wall
            DrawSimpleWall(0, arenaSize, 0, 0, wallHeight);           // Left wall
        }
    }
    
    /// <summary>
    /// Draw a simple wall segment
    /// </summary>
    private void DrawSimpleWall(float x1, float z1, float x2, float z2, float height)
    {
        // Create wall quad vertices
        var verts = new VertexPositionTexture[4];
        
        // Bottom edge (on ground)
        verts[0] = new VertexPositionTexture(new Vector3(x1, 0, z1), new Vector2(0, 1));
        verts[1] = new VertexPositionTexture(new Vector3(x2, 0, z2), new Vector2(1, 1));
        
        // Top edge (at wall height)
        verts[2] = new VertexPositionTexture(new Vector3(x1, height, z1), new Vector2(0, 0));
        verts[3] = new VertexPositionTexture(new Vector3(x2, height, z2), new Vector2(1, 0));
        
        DrawQuad(verts);
    }

    public void DrawSkybox()
    {
        if (_skyFaces[0] == null) return;

        // CRITICAL FIX: Proper skybox rendering like Java version
        var prevDepth = _gd.DepthStencilState;
        _gd.DepthStencilState = DepthStencilState.None; // Disable depth testing
        _gd.RasterizerState = RasterizerState.CullNone; // Draw both sides
        _gd.BlendState = BlendState.Opaque; // No blending for skybox
        Effect.World = Matrix.Identity;
        
        // CRITICAL FIX: Skybox should be neutral colored
        Effect.DiffuseColor = Vector3.One; // White/neutral color
        Effect.Alpha = 1.0f;

        // CRITICAL FIX: Skybox positioning like Java version
        float d = _gridSize * 3f;

        var faces = new Vector3[][]
        {
            // Match Java skybox face order and positioning
            new [] { new Vector3(d,d,-d), new Vector3(d,-d,-d), new Vector3(d,-d,d), new Vector3(d,d,d) },      // Right
            new [] { new Vector3(d,d,d), new Vector3(-d,d,d), new Vector3(-d,-d,d), new Vector3(d,-d,d) },      // Front
            new [] { new Vector3(-d,d,-d), new Vector3(d,d,-d), new Vector3(d,d,d), new Vector3(-d,d,d) },      // Top
            new [] { new Vector3(d,-d,-d), new Vector3(-d,-d,-d), new Vector3(-d,-d,d), new Vector3(d,-d,d) },  // Bottom
            new [] { new Vector3(-d,d,-d), new Vector3(-d,-d,-d), new Vector3(d,-d,-d), new Vector3(d,d,-d) },  // Back
            new [] { new Vector3(-d,-d,-d), new Vector3(-d,d,-d), new Vector3(-d,d,d), new Vector3(-d,-d,d) },  // Left
        };

        // CRITICAL FIX: Proper UV coordinates for skybox (match Java version)
        var uvs = new [] { new Vector2(1,1), new Vector2(0,1), new Vector2(0,0), new Vector2(1,0) };

        for (int i = 0; i < 6; i++)
        {
            var tex = _skyFaces[i];
            if (tex == null) continue;
            Effect.Texture = tex;
            foreach (var pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                var f = faces[i];
                var verts = new VertexPositionTexture[4];
                verts[0] = new VertexPositionTexture(f[0], uvs[0]);
                verts[1] = new VertexPositionTexture(f[1], uvs[1]);
                verts[2] = new VertexPositionTexture(f[2], uvs[2]);
                verts[3] = new VertexPositionTexture(f[3], uvs[3]);
                using var vb = new VertexBuffer(_gd, typeof(VertexPositionTexture), 4, BufferUsage.WriteOnly);
                vb.SetData(verts);
                _gd.SetVertexBuffer(vb);
                _gd.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            }
        }

        _gd.DepthStencilState = prevDepth;
    }

    public void DrawBike(BasicEffect fx, IPlayer? p)
    {
        if (p == null) return;
        
        try
        {
            float x = p.getXpos();
            float y = p.getYpos();
            int direction = p.getDirection();
            
            // Get player color
            int colorIndex = p.getPlayerNum();
            if (p is GltronMobileEngine.Player concretePlayer)
            {
                colorIndex = concretePlayer.getPlayerColorIndex();
            }
            Vector3 playerColor = GetPlayerColor(colorIndex);
            
            const float BIKE_SCALE = 1.0f;
            var world = Matrix.CreateScale(BIKE_SCALE) *
                       Matrix.CreateRotationY(direction * MathHelper.PiOver2) *
                       Matrix.CreateTranslation(x, 0f, y);
            
            // Try to draw FBX model first
            if (_useFbxModels && _bikeModel != null && _bikeBoneTransforms != null)
            {
                try
                {
#if ANDROID
                    Android.Util.Log.Debug("GLTRON", $"Drawing FBX lightcycle for player {p.getPlayerNum()} at ({x:F1}, {y:F1})");
#endif
                }
                catch { }
                
                // CRITICAL FIX: Adjust scale and positioning for FBX models
                const float FBX_BIKE_SCALE = 0.5f; // FBX models might be larger than expected
                var fbxWorld = Matrix.CreateScale(FBX_BIKE_SCALE) *
                              Matrix.CreateRotationY(direction * MathHelper.PiOver2) *
                              Matrix.CreateTranslation(x, 0f, y);
                
                DrawFbxModel(_bikeModel, _bikeBoneTransforms, fbxWorld, playerColor);
                return;
            }
            
            // Fallback to procedural motorcycle
            try
            {
#if ANDROID
                Android.Util.Log.Debug("GLTRON", $"Drawing procedural motorcycle for player {p.getPlayerNum()} (FBX not available)");
#endif
            }
            catch { }
            
            fx.World = world;
            fx.DiffuseColor = playerColor;
            fx.Alpha = 1.0f;
            
            // Set up effect for model rendering
            fx.TextureEnabled = false;
            fx.VertexColorEnabled = false;
            fx.LightingEnabled = true;
            fx.EnableDefaultLighting();
            
            // Draw the procedural motorcycle
            DrawProceduralMotorcycle(fx);
            
            // Reset effect state
            fx.TextureEnabled = true;
            fx.LightingEnabled = false;
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GLTRON: DrawBike error: {ex.Message}");
        }
    }
    


    public void DrawExplosion(BasicEffect fx, IPlayer? p)
    {
        if (p == null || !p.getExplode()) return;
        
        try
        {
            float x = p.getXpos();
            float y = p.getYpos();
            
            // TODO: Implement explosion effect
            // For now, draw a bright colored effect at crash location
            var world = Matrix.CreateScale(3f) * Matrix.CreateTranslation(x, 3f, y);
            fx.World = world;
            fx.DiffuseColor = Vector3.One; // White explosion
            fx.EmissiveColor = Vector3.One * 0.5f; // Glowing effect
            
            foreach (var pass in fx.CurrentTechnique.Passes)
            {
                pass.Apply();
                // TODO: Draw actual explosion particles/effects here
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GLTRON: DrawExplosion error: {ex.Message}");
        }
    }
    
    private Vector3 GetPlayerColor(int playerNum)
    {
        // CRITICAL FIX: Match Java player colors exactly (from ColourDiffuse array)
        Vector3[] colors = {
            new Vector3(0.0f, 0.1f, 0.900f),      // Blue - Player 0 (human)
            new Vector3(1.00f, 0.550f, 0.140f),   // Orange/Yellow - Player 1 (AI)
            new Vector3(0.750f, 0.020f, 0.020f),  // Red - Player 2 (AI)
            new Vector3(0.800f, 0.800f, 0.800f),  // Grey - Player 3 (AI)
            new Vector3(0.120f, 0.750f, 0.0f),    // Green - Player 4 (AI)
            new Vector3(0.750f, 0.0f, 0.35f)      // Purple - Player 5 (AI)
        };
        
        var color = colors[playerNum % colors.Length];
        
        try
        {
#if ANDROID
            Android.Util.Log.Debug("GLTRON", $"Player {playerNum} color: R={color.X:F2} G={color.Y:F2} B={color.Z:F2}");
#endif
        }
        catch { }
        
        return color;
    }
    


    public void DrawRecognizer(BasicEffect fx, Recognizer recognizer)
    {
        if (recognizer == null) 
        {
            System.Diagnostics.Debug.WriteLine("GLTRON: DrawRecognizer called with null recognizer");
            return;
        }
        
        try
        {
            // Set up recognizer rendering state
            _gd.BlendState = BlendState.AlphaBlend;
            _gd.DepthStencilState = DepthStencilState.Default;
            _gd.RasterizerState = RasterizerState.CullCounterClockwise;
            
            // Get recognizer color
            Vector4 recognizerColor = recognizer.GetColor();
            Vector3 color = new Vector3(recognizerColor.X, recognizerColor.Y, recognizerColor.Z);
            
            // Get recognizer's actual calculated position and transformation
            Vector3 recognizerSize = new Vector3(RECOGNIZER_SIZE, RECOGNIZER_SIZE, RECOGNIZER_SIZE);
            
            // Try to draw FBX model first
            if (_useFbxModels && _recognizerModel != null && _recognizerBoneTransforms != null)
            {
                try
                {
#if ANDROID
                    Android.Util.Log.Debug("GLTRON", "Drawing FBX recognizer with original GLTron movement");
#endif
                }
                catch { }
                
                // Use the recognizer's actual position calculation (original GLTron movement)
                Matrix fbxWorldMatrix = recognizer.GetWorldMatrix(recognizerSize);
                
                // Adjust scale for FBX model if needed (recognizer already has SCALE_FACTOR applied)
                const float FBX_RECOGNIZER_SCALE = 0.5f; // Additional scale adjustment for FBX model
                fbxWorldMatrix = Matrix.CreateScale(FBX_RECOGNIZER_SCALE) * fbxWorldMatrix;
                
                // Draw the FBX recognizer
                DrawFbxModel(_recognizerModel, _recognizerBoneTransforms, fbxWorldMatrix, color);
                
                // Draw shadow at actual position
                Vector3 position = recognizer.GetPosition(recognizerSize);
                DrawRecognizerShadowFlatAt(new Vector3(position.X, 0, position.Z));
                return;
            }
            
            // Fallback to procedural recognizer
            try
            {
#if ANDROID
                Android.Util.Log.Debug("GLTRON", "Drawing procedural recognizer with original GLTron movement");
#endif
            }
            catch { }
            
            // Use the recognizer's actual calculated world matrix (original GLTron movement)
            Matrix worldMatrix = recognizer.GetWorldMatrix(recognizerSize);
            fx.World = worldMatrix;
            
            fx.DiffuseColor = color;
            fx.Alpha = recognizerColor.W;
            
            // Set up effect for model rendering
            fx.TextureEnabled = false;
            fx.VertexColorEnabled = false;
            fx.LightingEnabled = true;
            fx.EnableDefaultLighting();
            
            // Set special lighting for recognizer (like Java version)
            fx.EmissiveColor = new Vector3(0.1f, 0.05f, 0.05f); // Slight red glow
            fx.SpecularColor = color;
            fx.SpecularPower = 16.0f;
            
            // Draw the procedural recognizer
            DrawProceduralRecognizer(fx);
            
            // Draw shadow (simplified version)
            DrawRecognizerShadow(fx, recognizer);
            
            // Reset effect state
            fx.EmissiveColor = Vector3.Zero;
            fx.SpecularColor = Vector3.Zero;
            fx.TextureEnabled = true;
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GLTRON: DrawRecognizer error: {ex.Message}");
        }
    }
    
    private void DrawRecognizerShadow(BasicEffect fx, Recognizer recognizer)
    {
        try
        {
            // Simplified shadow - draw a darker version on the ground
            var prevBlend = _gd.BlendState;
            _gd.BlendState = BlendState.AlphaBlend;
            
            // Get shadow transformation (projected to ground)
            Vector3 recognizerSize = new Vector3(RECOGNIZER_SIZE, RECOGNIZER_SIZE, RECOGNIZER_SIZE);
            Vector3 position = recognizer.GetPosition(recognizerSize);
            float angle = recognizer.GetAngle();
            
            // Project shadow onto the ground (Y = 0) with same scale as recognizer
            Matrix shadowWorld = Matrix.CreateScale(0.15f) * // Match recognizer scale
                                Matrix.CreateRotationY(MathHelper.ToRadians(angle)) *
                                Matrix.CreateTranslation(position.X, 0.1f, position.Z); // Slightly above ground
            
            fx.World = shadowWorld;
            fx.DiffuseColor = Vector3.Zero; // Black shadow
            fx.Alpha = 0.3f; // Semi-transparent
            fx.LightingEnabled = false; // No lighting for shadow
            
            // Draw procedural shadow
            DrawProceduralRecognizer(fx);
            
            _gd.BlendState = prevBlend;
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GLTRON: DrawRecognizerShadow error: {ex.Message}");
        }
    }
    

    /// <summary>
    /// Helper method to draw a quad from 4 vertices
    /// </summary>
    private void DrawQuad(VertexPositionTexture[] verts)
    {
        if (verts.Length != 4) return;
        
        using var vb = new VertexBuffer(_gd, typeof(VertexPositionTexture), 4, BufferUsage.WriteOnly);
        vb.SetData(verts);
        _gd.SetVertexBuffer(vb);
        _gd.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
    }

    public void EndDraw() { }


    
    /// <summary>
    /// Draw a distinctive recognizer shape - floating alien probe, COMPLETELY different from motorcycle
    /// </summary>
    private void DrawRecognizerShape(BasicEffect fx)
    {
        var allVertices = new List<VertexPositionNormalTexture>();
        
        // CRITICAL FIX: Create a FLOATING ALIEN PROBE - nothing like a motorcycle
        
        // MAIN SPHERE-LIKE BODY - central floating orb
        var mainBodyVertices = CreateBoxVertices(new Vector3(0f, 2.0f, 0f), new Vector3(1.2f, 1.2f, 1.2f));
        allVertices.AddRange(mainBodyVertices);
        
        // TOP DOME - alien probe dome
        var topDomeVertices = CreatePyramidVertices(new Vector3(0f, 3.0f, 0f), 1.0f, 0.8f);
        allVertices.AddRange(topDomeVertices);
        
        // BOTTOM SENSOR ARRAY - inverted cone
        var bottomSensorVertices = CreateInvertedPyramidVertices(new Vector3(0f, 1.0f, 0f), 0.8f, 1.0f);
        allVertices.AddRange(bottomSensorVertices);
        
        // FLOATING RINGS - multiple orbital rings (very alien)
        var ring1Vertices = CreateRingVertices(new Vector3(0f, 1.5f, 0f), 2.0f, 1.6f, 0.1f);
        var ring2Vertices = CreateRingVertices(new Vector3(0f, 2.0f, 0f), 1.8f, 1.4f, 0.1f);
        var ring3Vertices = CreateRingVertices(new Vector3(0f, 2.5f, 0f), 2.2f, 1.8f, 0.1f);
        allVertices.AddRange(ring1Vertices);
        allVertices.AddRange(ring2Vertices);
        allVertices.AddRange(ring3Vertices);
        
        // CENTRAL ENERGY CORE - glowing center
        var coreVertices = CreateBoxVertices(new Vector3(0f, 2.0f, 0f), new Vector3(0.4f, 0.4f, 0.4f));
        allVertices.AddRange(coreVertices);
        
        // SENSOR SPIKES - extending in all directions (very alien)
        var spike1Vertices = CreateBoxVertices(new Vector3(1.5f, 2.0f, 0f), new Vector3(1.0f, 0.1f, 0.1f));
        var spike2Vertices = CreateBoxVertices(new Vector3(-1.5f, 2.0f, 0f), new Vector3(1.0f, 0.1f, 0.1f));
        var spike3Vertices = CreateBoxVertices(new Vector3(0f, 2.0f, 1.5f), new Vector3(0.1f, 0.1f, 1.0f));
        var spike4Vertices = CreateBoxVertices(new Vector3(0f, 2.0f, -1.5f), new Vector3(0.1f, 0.1f, 1.0f));
        var spike5Vertices = CreateBoxVertices(new Vector3(0f, 3.5f, 0f), new Vector3(0.1f, 1.0f, 0.1f));
        var spike6Vertices = CreateBoxVertices(new Vector3(0f, 0.5f, 0f), new Vector3(0.1f, 1.0f, 0.1f));
        allVertices.AddRange(spike1Vertices);
        allVertices.AddRange(spike2Vertices);
        allVertices.AddRange(spike3Vertices);
        allVertices.AddRange(spike4Vertices);
        allVertices.AddRange(spike5Vertices);
        allVertices.AddRange(spike6Vertices);
        
        // ANTENNA ARRAY - multiple thin antennae
        var antenna1Vertices = CreateBoxVertices(new Vector3(0.8f, 3.2f, 0.8f), new Vector3(0.05f, 0.8f, 0.05f));
        var antenna2Vertices = CreateBoxVertices(new Vector3(-0.8f, 3.2f, 0.8f), new Vector3(0.05f, 0.8f, 0.05f));
        var antenna3Vertices = CreateBoxVertices(new Vector3(0.8f, 3.2f, -0.8f), new Vector3(0.05f, 0.8f, 0.05f));
        var antenna4Vertices = CreateBoxVertices(new Vector3(-0.8f, 3.2f, -0.8f), new Vector3(0.05f, 0.8f, 0.05f));
        allVertices.AddRange(antenna1Vertices);
        allVertices.AddRange(antenna2Vertices);
        allVertices.AddRange(antenna3Vertices);
        allVertices.AddRange(antenna4Vertices);
        
        RenderVertices(fx, allVertices);
    }
    
    


    /// <summary>
    /// Create wheel-like geometry (thin cylinder)
    /// </summary>
    private List<VertexPositionNormalTexture> CreateWheelVertices(Vector3 center, float radius, float thickness)
    {
        var vertices = new List<VertexPositionNormalTexture>();
        
        // Create a thin cylinder for wheel
        int segments = 8; // Octagonal wheel for performance
        for (int i = 0; i < segments; i++)
        {
            float angle1 = (float)(i * 2 * Math.PI / segments);
            float angle2 = (float)((i + 1) * 2 * Math.PI / segments);
            
            float x1 = (float)Math.Cos(angle1) * radius;
            float y1 = (float)Math.Sin(angle1) * radius;
            float x2 = (float)Math.Cos(angle2) * radius;
            float y2 = (float)Math.Sin(angle2) * radius;
            
            // Front face triangle
            Vector3 v1 = center + new Vector3(x1, y1, thickness/2);
            Vector3 v2 = center + new Vector3(x2, y2, thickness/2);
            Vector3 v3 = center + new Vector3(0, 0, thickness/2);
            Vector3 normal = Vector3.Forward;
            
            vertices.Add(new VertexPositionNormalTexture(v1, normal, Vector2.Zero));
            vertices.Add(new VertexPositionNormalTexture(v2, normal, Vector2.Zero));
            vertices.Add(new VertexPositionNormalTexture(v3, normal, Vector2.Zero));
            
            // Back face triangle
            v1 = center + new Vector3(x1, y1, -thickness/2);
            v2 = center + new Vector3(x2, y2, -thickness/2);
            v3 = center + new Vector3(0, 0, -thickness/2);
            normal = Vector3.Backward;
            
            vertices.Add(new VertexPositionNormalTexture(v1, normal, Vector2.Zero));
            vertices.Add(new VertexPositionNormalTexture(v3, normal, Vector2.Zero));
            vertices.Add(new VertexPositionNormalTexture(v2, normal, Vector2.Zero));
        }
        
        return vertices;
    }
    
    /// <summary>
    /// Render a list of vertices
    /// </summary>
    private void RenderVertices(BasicEffect fx, List<VertexPositionNormalTexture> vertices)
    {
        if (vertices.Count == 0) return;
        
        foreach (var pass in fx.CurrentTechnique.Passes)
        {
            pass.Apply();
            
            using var vb = new VertexBuffer(_gd, typeof(VertexPositionNormalTexture), vertices.Count, BufferUsage.WriteOnly);
            vb.SetData(vertices.ToArray());
            _gd.SetVertexBuffer(vb);
            
            _gd.DrawPrimitives(PrimitiveType.TriangleList, 0, vertices.Count / 3);
        }
    }
    
    /// <summary>
    /// Create pyramid vertices
    /// </summary>
    private List<VertexPositionNormalTexture> CreatePyramidVertices(Vector3 center, float baseSize, float height)
    {
        var vertices = new List<VertexPositionNormalTexture>();
        float halfBase = baseSize * 0.5f;
        
        // Base vertices
        Vector3 v1 = center + new Vector3(-halfBase, 0, -halfBase);
        Vector3 v2 = center + new Vector3(halfBase, 0, -halfBase);
        Vector3 v3 = center + new Vector3(halfBase, 0, halfBase);
        Vector3 v4 = center + new Vector3(-halfBase, 0, halfBase);
        Vector3 apex = center + new Vector3(0, height, 0);
        
        // Create triangular faces
        AddTriangleToVertices(vertices, v1, v2, apex, Vector3.Normalize(Vector3.Cross(v2 - v1, apex - v1)));
        AddTriangleToVertices(vertices, v2, v3, apex, Vector3.Normalize(Vector3.Cross(v3 - v2, apex - v2)));
        AddTriangleToVertices(vertices, v3, v4, apex, Vector3.Normalize(Vector3.Cross(v4 - v3, apex - v3)));
        AddTriangleToVertices(vertices, v4, v1, apex, Vector3.Normalize(Vector3.Cross(v1 - v4, apex - v4)));
        
        return vertices;
    }
    
    /// <summary>
    /// Create inverted pyramid vertices
    /// </summary>
    private List<VertexPositionNormalTexture> CreateInvertedPyramidVertices(Vector3 center, float baseSize, float height)
    {
        var vertices = new List<VertexPositionNormalTexture>();
        float halfBase = baseSize * 0.5f;
        
        // Base vertices (at top)
        Vector3 v1 = center + new Vector3(-halfBase, 0, -halfBase);
        Vector3 v2 = center + new Vector3(halfBase, 0, -halfBase);
        Vector3 v3 = center + new Vector3(halfBase, 0, halfBase);
        Vector3 v4 = center + new Vector3(-halfBase, 0, halfBase);
        Vector3 apex = center + new Vector3(0, -height, 0);
        
        // Create triangular faces (inverted)
        AddTriangleToVertices(vertices, v1, apex, v2, Vector3.Normalize(Vector3.Cross(apex - v1, v2 - v1)));
        AddTriangleToVertices(vertices, v2, apex, v3, Vector3.Normalize(Vector3.Cross(apex - v2, v3 - v2)));
        AddTriangleToVertices(vertices, v3, apex, v4, Vector3.Normalize(Vector3.Cross(apex - v3, v4 - v3)));
        AddTriangleToVertices(vertices, v4, apex, v1, Vector3.Normalize(Vector3.Cross(apex - v4, v1 - v4)));
        
        return vertices;
    }
    
    /// <summary>
    /// Create ring vertices (torus-like)
    /// </summary>
    private List<VertexPositionNormalTexture> CreateRingVertices(Vector3 center, float outerRadius, float innerRadius, float thickness)
    {
        var vertices = new List<VertexPositionNormalTexture>();
        int segments = 12;
        
        for (int i = 0; i < segments; i++)
        {
            float angle1 = (float)(i * 2 * Math.PI / segments);
            float angle2 = (float)((i + 1) * 2 * Math.PI / segments);
            
            // Outer ring
            float x1o = (float)Math.Cos(angle1) * outerRadius;
            float z1o = (float)Math.Sin(angle1) * outerRadius;
            float x2o = (float)Math.Cos(angle2) * outerRadius;
            float z2o = (float)Math.Sin(angle2) * outerRadius;
            
            // Inner ring
            float x1i = (float)Math.Cos(angle1) * innerRadius;
            float z1i = (float)Math.Sin(angle1) * innerRadius;
            float x2i = (float)Math.Cos(angle2) * innerRadius;
            float z2i = (float)Math.Sin(angle2) * innerRadius;
            
            // Create ring segment (top face)
            Vector3 v1 = center + new Vector3(x1o, thickness/2, z1o);
            Vector3 v2 = center + new Vector3(x2o, thickness/2, z2o);
            Vector3 v3 = center + new Vector3(x2i, thickness/2, z2i);
            Vector3 v4 = center + new Vector3(x1i, thickness/2, z1i);
            
            AddQuadToVertices(vertices, v1, v2, v3, v4, Vector3.Up);
            
            // Bottom face
            v1 = center + new Vector3(x1o, -thickness/2, z1o);
            v2 = center + new Vector3(x2o, -thickness/2, z2o);
            v3 = center + new Vector3(x2i, -thickness/2, z2i);
            v4 = center + new Vector3(x1i, -thickness/2, z1i);
            
            AddQuadToVertices(vertices, v1, v4, v3, v2, Vector3.Down);
        }
        
        return vertices;
    }
    
    /// <summary>
    /// Add a triangle to the vertex list
    /// </summary>
    private void AddTriangleToVertices(List<VertexPositionNormalTexture> vertices, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 normal)
    {
        vertices.Add(new VertexPositionNormalTexture(v1, normal, Vector2.Zero));
        vertices.Add(new VertexPositionNormalTexture(v2, normal, Vector2.Zero));
        vertices.Add(new VertexPositionNormalTexture(v3, normal, Vector2.Zero));
    }
    
    /// <summary>
    /// Add a quad to the vertex list
    /// </summary>
    private void AddQuadToVertices(List<VertexPositionNormalTexture> vertices, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector3 normal)
    {
        // Triangle 1: v1, v2, v3
        vertices.Add(new VertexPositionNormalTexture(v1, normal, Vector2.Zero));
        vertices.Add(new VertexPositionNormalTexture(v2, normal, Vector2.Zero));
        vertices.Add(new VertexPositionNormalTexture(v3, normal, Vector2.Zero));
        
        // Triangle 2: v1, v3, v4
        vertices.Add(new VertexPositionNormalTexture(v1, normal, Vector2.Zero));
        vertices.Add(new VertexPositionNormalTexture(v3, normal, Vector2.Zero));
        vertices.Add(new VertexPositionNormalTexture(v4, normal, Vector2.Zero));
    }

    /// <summary>
    /// Create vertices for a box/cube at the given position and size
    /// </summary>
    private List<VertexPositionNormalTexture> CreateBoxVertices(Vector3 center, Vector3 size)
    {
        var vertices = new List<VertexPositionNormalTexture>();
        Vector3 halfSize = size * 0.5f;
        
        // Define the 8 corners of the box
        Vector3[] corners = {
            center + new Vector3(-halfSize.X, -halfSize.Y, -halfSize.Z), // 0: bottom-back-left
            center + new Vector3( halfSize.X, -halfSize.Y, -halfSize.Z), // 1: bottom-back-right
            center + new Vector3( halfSize.X,  halfSize.Y, -halfSize.Z), // 2: top-back-right
            center + new Vector3(-halfSize.X,  halfSize.Y, -halfSize.Z), // 3: top-back-left
            center + new Vector3(-halfSize.X, -halfSize.Y,  halfSize.Z), // 4: bottom-front-left
            center + new Vector3( halfSize.X, -halfSize.Y,  halfSize.Z), // 5: bottom-front-right
            center + new Vector3( halfSize.X,  halfSize.Y,  halfSize.Z), // 6: top-front-right
            center + new Vector3(-halfSize.X,  halfSize.Y,  halfSize.Z), // 7: top-front-left
        };
        
        // Define faces with proper normals
        var faces = new[] {
            // Back face (facing -Z)
            new { indices = new[] { 0, 1, 2, 0, 2, 3 }, normal = Vector3.Backward },
            // Front face (facing +Z)
            new { indices = new[] { 4, 6, 5, 4, 7, 6 }, normal = Vector3.Forward },
            // Bottom face (facing -Y)
            new { indices = new[] { 0, 4, 5, 0, 5, 1 }, normal = Vector3.Down },
            // Top face (facing +Y)
            new { indices = new[] { 3, 2, 6, 3, 6, 7 }, normal = Vector3.Up },
            // Left face (facing -X)
            new { indices = new[] { 0, 3, 7, 0, 7, 4 }, normal = Vector3.Left },
            // Right face (facing +X)
            new { indices = new[] { 1, 5, 6, 1, 6, 2 }, normal = Vector3.Right }
        };
        
        foreach (var face in faces)
        {
            for (int i = 0; i < face.indices.Length; i++)
            {
                vertices.Add(new VertexPositionNormalTexture(corners[face.indices[i]], face.normal, Vector2.Zero));
            }
        }
        
        return vertices;
    }

    /// <summary>
    /// Create vertices for an octahedron (8-sided polyhedron) - perfect for alien recognizer core
    /// </summary>
    private List<VertexPositionNormalTexture> CreateOctahedronVertices(Vector3 center, float size)
    {
        var vertices = new List<VertexPositionNormalTexture>();
        
        // Define octahedron vertices
        Vector3[] points = {
            center + new Vector3(0, size, 0),    // Top
            center + new Vector3(0, -size, 0),   // Bottom
            center + new Vector3(size, 0, 0),    // Right
            center + new Vector3(-size, 0, 0),   // Left
            center + new Vector3(0, 0, size),    // Front
            center + new Vector3(0, 0, -size),   // Back
        };
        
        // Define the 8 triangular faces of the octahedron
        var faces = new[] {
            new[] { 0, 2, 4 }, // Top-Right-Front
            new[] { 0, 4, 3 }, // Top-Front-Left
            new[] { 0, 3, 5 }, // Top-Left-Back
            new[] { 0, 5, 2 }, // Top-Back-Right
            new[] { 1, 4, 2 }, // Bottom-Front-Right
            new[] { 1, 3, 4 }, // Bottom-Left-Front
            new[] { 1, 5, 3 }, // Bottom-Back-Left
            new[] { 1, 2, 5 }, // Bottom-Right-Back
        };
        
        foreach (var face in faces)
        {
            Vector3 v1 = points[face[0]];
            Vector3 v2 = points[face[1]];
            Vector3 v3 = points[face[2]];
            
            Vector3 normal = Vector3.Normalize(Vector3.Cross(v2 - v1, v3 - v1));
            
            vertices.Add(new VertexPositionNormalTexture(v1, normal, Vector2.Zero));
            vertices.Add(new VertexPositionNormalTexture(v2, normal, Vector2.Zero));
            vertices.Add(new VertexPositionNormalTexture(v3, normal, Vector2.Zero));
        }
        
        return vertices;
    }

    /// <summary>
    /// Create vertices for a torus (donut shape) - perfect for alien recognizer rings
    /// </summary>
    private List<VertexPositionNormalTexture> CreateTorusVertices(Vector3 center, float majorRadius, float minorRadius, int majorSegments, int minorSegments)
    {
        var vertices = new List<VertexPositionNormalTexture>();
        
        for (int i = 0; i < majorSegments; i++)
        {
            float majorAngle1 = (float)i / majorSegments * MathHelper.TwoPi;
            float majorAngle2 = (float)(i + 1) / majorSegments * MathHelper.TwoPi;
            
            for (int j = 0; j < minorSegments; j++)
            {
                float minorAngle1 = (float)j / minorSegments * MathHelper.TwoPi;
                float minorAngle2 = (float)(j + 1) / minorSegments * MathHelper.TwoPi;
                
                // Calculate the four vertices of this quad
                Vector3 v1 = GetTorusVertex(center, majorRadius, minorRadius, majorAngle1, minorAngle1);
                Vector3 v2 = GetTorusVertex(center, majorRadius, minorRadius, majorAngle2, minorAngle1);
                Vector3 v3 = GetTorusVertex(center, majorRadius, minorRadius, majorAngle2, minorAngle2);
                Vector3 v4 = GetTorusVertex(center, majorRadius, minorRadius, majorAngle1, minorAngle2);
                
                // Calculate normals
                Vector3 n1 = GetTorusNormal(majorAngle1, minorAngle1);
                Vector3 n2 = GetTorusNormal(majorAngle2, minorAngle1);
                Vector3 n3 = GetTorusNormal(majorAngle2, minorAngle2);
                Vector3 n4 = GetTorusNormal(majorAngle1, minorAngle2);
                
                // Create two triangles for this quad
                vertices.Add(new VertexPositionNormalTexture(v1, n1, Vector2.Zero));
                vertices.Add(new VertexPositionNormalTexture(v2, n2, Vector2.Zero));
                vertices.Add(new VertexPositionNormalTexture(v3, n3, Vector2.Zero));
                
                vertices.Add(new VertexPositionNormalTexture(v1, n1, Vector2.Zero));
                vertices.Add(new VertexPositionNormalTexture(v3, n3, Vector2.Zero));
                vertices.Add(new VertexPositionNormalTexture(v4, n4, Vector2.Zero));
            }
        }
        
        return vertices;
    }

    private Vector3 GetTorusVertex(Vector3 center, float majorRadius, float minorRadius, float majorAngle, float minorAngle)
    {
        float x = (majorRadius + minorRadius * (float)Math.Cos(minorAngle)) * (float)Math.Cos(majorAngle);
        float y = minorRadius * (float)Math.Sin(minorAngle);
        float z = (majorRadius + minorRadius * (float)Math.Cos(minorAngle)) * (float)Math.Sin(majorAngle);
        return center + new Vector3(x, y, z);
    }

    private Vector3 GetTorusNormal(float majorAngle, float minorAngle)
    {
        float x = (float)Math.Cos(minorAngle) * (float)Math.Cos(majorAngle);
        float y = (float)Math.Sin(minorAngle);
        float z = (float)Math.Cos(minorAngle) * (float)Math.Sin(majorAngle);
        return Vector3.Normalize(new Vector3(x, y, z));
    }

    /// <summary>
    /// Create vertices for a spike/cone - perfect for alien recognizer energy spikes
    /// </summary>
    private List<VertexPositionNormalTexture> CreateSpikeVertices(Vector3 center, float height, float baseRadius)
    {
        var vertices = new List<VertexPositionNormalTexture>();
        int segments = 8; // Number of segments around the base
        
        Vector3 tip = center + Vector3.Normalize(center) * height;
        
        float angleStep = MathHelper.TwoPi / segments;
        
        for (int i = 0; i < segments; i++)
        {
            float angle1 = i * angleStep;
            float angle2 = ((i + 1) % segments) * angleStep;
            
            Vector3 base1 = center + new Vector3(baseRadius * (float)Math.Cos(angle1), baseRadius * (float)Math.Sin(angle1), 0);
            Vector3 base2 = center + new Vector3(baseRadius * (float)Math.Cos(angle2), baseRadius * (float)Math.Sin(angle2), 0);
            
            // Calculate normal for this face
            Vector3 normal = Vector3.Normalize(Vector3.Cross(base2 - tip, base1 - tip));
            
            // Create triangle from tip to base edge
            vertices.Add(new VertexPositionNormalTexture(tip, normal, Vector2.Zero));
            vertices.Add(new VertexPositionNormalTexture(base1, normal, Vector2.Zero));
            vertices.Add(new VertexPositionNormalTexture(base2, normal, Vector2.Zero));
        }
        
        return vertices;
    }

    /// <summary>
    /// Render a list of vertices using the current effect
    /// </summary>
    private void RenderVertexList(BasicEffect fx, List<VertexPositionNormalTexture> vertices)
    {
        if (vertices.Count == 0) return;
        
        foreach (var pass in fx.CurrentTechnique.Passes)
        {
            pass.Apply();
            
            using var vb = new VertexBuffer(_gd, typeof(VertexPositionNormalTexture), vertices.Count, BufferUsage.WriteOnly);
            vb.SetData(vertices.ToArray());
            _gd.SetVertexBuffer(vb);
            
            _gd.DrawPrimitives(PrimitiveType.TriangleList, 0, vertices.Count / 3);
        }
    }
    
    /// <summary>
    /// Draw an FBX model with proper lighting and color
    /// </summary>
    private void DrawFbxModel(Model model, Matrix[] boneTransforms, Matrix world, Vector3 tint)
    {
        // CRITICAL FIX: Set proper render states for FBX models
        var oldRasterizer = _gd.RasterizerState;
        var oldDepthStencil = _gd.DepthStencilState;
        var oldBlend = _gd.BlendState;
        
        _gd.RasterizerState = RasterizerState.CullCounterClockwise; // Proper culling
        _gd.DepthStencilState = DepthStencilState.Default; // Enable depth testing
        _gd.BlendState = BlendState.Opaque; // No blending for solid models
        
        Matrix view = Effect.View;
        Matrix proj = Effect.Projection;
        
        foreach (var mesh in model.Meshes)
        {
            // CRITICAL FIX: Combine transforms properly
            Matrix meshWorld = boneTransforms[mesh.ParentBone.Index] * world;
            
            foreach (var effect in mesh.Effects)
            {
                if (effect is BasicEffect basicEffect)
                {
                    // CRITICAL FIX: Set all required properties
                    basicEffect.World = meshWorld;
                    basicEffect.View = view;
                    basicEffect.Projection = proj;
                    
                    // CRITICAL FIX: Proper lighting setup
                    basicEffect.LightingEnabled = true;
                    basicEffect.EnableDefaultLighting();
                    
                    // CRITICAL FIX: Color and material setup
                    basicEffect.DiffuseColor = tint;
                    basicEffect.SpecularColor = Vector3.One * 0.2f; // Slight specular
                    basicEffect.SpecularPower = 16.0f;
                    basicEffect.Alpha = 1.0f;
                    
                    // CRITICAL FIX: Texture handling
                    if (basicEffect.Texture != null)
                    {
                        basicEffect.TextureEnabled = true;
                    }
                    else
                    {
                        basicEffect.TextureEnabled = false;
                    }
                    
                    // CRITICAL FIX: Disable vertex colors to use our tint
                    basicEffect.VertexColorEnabled = false;
                }
            }
            
            // CRITICAL FIX: Draw the mesh
            mesh.Draw();
        }
        
        // Restore render states
        _gd.RasterizerState = oldRasterizer;
        _gd.DepthStencilState = oldDepthStencil;
        _gd.BlendState = oldBlend;
    }
    
    /// <summary>
    /// Compute approximate bounding box size from model mesh bounding spheres
    /// </summary>
    private Vector3 ComputeModelSize(Model model)
    {
        var min = new Vector3(float.MaxValue);
        var max = new Vector3(float.MinValue);
        
        foreach (var mesh in model.Meshes)
        {
            var sphere = mesh.BoundingSphere;
            var center = sphere.Center;
            var radius = sphere.Radius;
            min = Vector3.Min(min, center - new Vector3(radius));
            max = Vector3.Max(max, center + new Vector3(radius));
        }
        
        return max - min;
    }
    
    /// <summary>
    /// Draw a flattened shadow of the FBX recognizer on the ground
    /// </summary>
    private void DrawRecognizerShadowWithFbx(Recognizer recognizer, Vector3 modelSize)
    {
        // kept for compatibility; we now prefer flat center shadow
        if (_recognizerModel == null || _recognizerBoneTransforms == null) return;
        Vector3 pos = new Vector3(_gridSize * 0.5f, 0f, _gridSize * 0.5f);
        DrawRecognizerShadowFlatAt(pos);
    }

    private void DrawRecognizerShadowFlatAt(Vector3 centerPos)
    {
        try
        {
            var oldBlend = _gd.BlendState;
            var oldDepth = _gd.DepthStencilState;
            var oldRaster = _gd.RasterizerState;

            _gd.BlendState = BlendState.AlphaBlend;
            _gd.DepthStencilState = DepthStencilState.DepthRead;
            _gd.RasterizerState = RasterizerState.CullNone;

            // Render a tiny flat quad as shadow under the recognizer center
            float r = Math.Max(0.8f, _gridSize * 0.02f);
            var verts = new VertexPositionTexture[4];
            verts[0] = new VertexPositionTexture(new Vector3(centerPos.X - r, 0.1f, centerPos.Z - r), new Vector2(0, 0));
            verts[1] = new VertexPositionTexture(new Vector3(centerPos.X + r, 0.1f, centerPos.Z - r), new Vector2(1, 0));
            verts[2] = new VertexPositionTexture(new Vector3(centerPos.X - r, 0.1f, centerPos.Z + r), new Vector2(0, 1));
            verts[3] = new VertexPositionTexture(new Vector3(centerPos.X + r, 0.1f, centerPos.Z + r), new Vector2(1, 1));

            // Use Effect with black color, no texture
            var prevTexEnabled = Effect.TextureEnabled;
            var prevDiffuse = Effect.DiffuseColor;
            var prevAlpha = Effect.Alpha;
            Effect.TextureEnabled = false;
            Effect.DiffuseColor = Vector3.Zero;
            Effect.Alpha = 0.18f;

            foreach (var pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                DrawQuad(verts);
            }

            Effect.TextureEnabled = prevTexEnabled;
            Effect.DiffuseColor = prevDiffuse;
            Effect.Alpha = prevAlpha;

            _gd.BlendState = oldBlend;
            _gd.DepthStencilState = oldDepth;
            _gd.RasterizerState = oldRaster;
        }
        catch { }
    }
    
    /// <summary>
    /// Try to load models in order of preference: FBX -> OBJ -> Fallback to procedural
    /// </summary>
        private Model? SafeLoadModel(ContentManager content, string logicalName)
        {
            try { return content.Load<Model>(logicalName); }
            catch (ContentLoadException ex)
            {
                try {
#if ANDROID
                    Android.Util.Log.Error("GLTRON", $"ContentLoadException for '{logicalName}': {ex.Message} | Inner: {ex.InnerException?.Message}");
#endif
                    System.Diagnostics.Debug.WriteLine($"GLTRON: ContentLoadException for '{logicalName}': {ex}");
                } catch {}
                return null;
            }
            catch (System.Exception ex)
            {
                try {
#if ANDROID
                    Android.Util.Log.Error("GLTRON", $"Unexpected load error for '{logicalName}': {ex.Message}");
#endif
                    System.Diagnostics.Debug.WriteLine($"GLTRON: Unexpected load error for '{logicalName}': {ex}");
                } catch {}
                return null;
            }
        }

        private Model? TryLoadWithAlternates(ContentManager content, string primary)
        {
            string[] names = primary.StartsWith("Assets/")
                ? new[] { primary, primary.Substring("Assets/".Length) }
                : new[] { primary, "Assets/" + primary };

            foreach (var name in names)
            {
                var mdl = SafeLoadModel(content, name);
                if (mdl != null) return mdl;
            }

            // Diagnostics: probe expected XNB path under RootDirectory
            try
            {
                var root = content.RootDirectory ?? "Content";
                var baseName = primary.StartsWith("Assets/") ? primary : ("Assets/" + primary);
                var candidate = System.IO.Path.Combine(root, baseName + ".xnb");
                bool exists = false;
                try { exists = System.IO.File.Exists(candidate); } catch {}
#if ANDROID
                Android.Util.Log.Error("GLTRON", $"Probe XNB exists? {exists} -> '{candidate}'");
#endif
                System.Diagnostics.Debug.WriteLine($"GLTRON: Probe XNB exists? {exists} -> '{candidate}'");
            }
            catch {}

            return null;
        }

        private bool TryLoadModels(ContentManager content)
        {
        // Start log
        try
        {
#if ANDROID
            #if ANDROID
Android.Util.Log.Info("GLTRON", "🚀 Starting model loading process...");
#endif
#endif
            System.Diagnostics.Debug.WriteLine("GLTRON: 🚀 Starting model loading process...");
        }
        catch { }

        // Helper to log exceptions with stack
        void LogEx(string tag, System.Exception ex)
        {
            try { 
#if ANDROID
Android.Util.Log.Error("GLTRON", $"{tag}: {ex.GetType().Name} - {ex.Message}\n{ex}");
#endif 
            } 
            catch { }
                System.Diagnostics.Debug.WriteLine($"GLTRON: {tag}: {ex}");
            }

        // Try to load models using Content pipeline names (with and without Assets/)
        string[] bikeNames = new[] { "Assets/lightcyclehigh", "lightcyclehigh" }; // legacy, replaced by TryLoadWithAlternates
        // Prefer robust loader with alternates and diagnostics
_bikeModel = TryLoadWithAlternates(content, "Assets/lightcyclehigh");
if (_bikeModel != null) {
    _bikeBoneTransforms = new Matrix[_bikeModel.Bones.Count];
    _bikeModel.CopyAbsoluteBoneTransformsTo(_bikeBoneTransforms);
}
_recognizerModel = TryLoadWithAlternates(content, "Assets/recognizerhigh");
if (_recognizerModel != null) {
    _recognizerBoneTransforms = new Matrix[_recognizerModel.Bones.Count];
    _recognizerModel.CopyAbsoluteBoneTransformsTo(_recognizerBoneTransforms);
}

bool ok = _bikeModel != null && _recognizerModel != null;
try {
#if ANDROID
    Android.Util.Log.Info("GLTRON", $"Model load result (robust): bike={_bikeModel!=null}, rec={_recognizerModel!=null}");
#endif
} catch {}
return ok;
    }
    
    
    private bool TryLoadFbxModels(ContentManager content)
    {
        try
        {
            try
            {
#if ANDROID
                #if ANDROID
Android.Util.Log.Info("GLTRON", "🔄 Attempting to load FBX models...");
#endif
                #if ANDROID
Android.Util.Log.Info("GLTRON", "Loading lightcyclehigh (FBX)...");
#endif
#endif
            }
            catch { }
            
            // Use centralized loader
            _bikeModel = ModelLoader.LoadLightcycle(content, 0);
            
            try
            {
#if ANDROID
                #if ANDROID
Android.Util.Log.Info("GLTRON", "Loading recognizerhigh (FBX) via ModelLoader...");
#endif
#endif
            }
            catch { }
            
            _recognizerModel = ModelLoader.LoadRecognizer(content);
            
            try
            {
#if ANDROID
                #if ANDROID
Android.Util.Log.Info("GLTRON", "✅ FBX loads attempted");
#endif
#endif
            }
            catch { }
            
            if (_bikeModel != null && _recognizerModel != null)
            {
                try
                {
#if ANDROID
                    #if ANDROID
Android.Util.Log.Info("GLTRON", "Setting up bone transforms...");
#endif
#endif
                }
                catch { }
                
                _bikeBoneTransforms = new Matrix[_bikeModel.Bones.Count];
                _bikeModel.CopyAbsoluteBoneTransformsTo(_bikeBoneTransforms);
                
                _recognizerBoneTransforms = new Matrix[_recognizerModel.Bones.Count];
                _recognizerModel.CopyAbsoluteBoneTransformsTo(_recognizerBoneTransforms);
                
                try
                {
#if ANDROID
                    #if ANDROID
Android.Util.Log.Info("GLTRON", $"🎉 FBX SUCCESS! Bike: {_bikeModel.Meshes.Count} meshes, {_bikeModel.Bones.Count} bones");
#endif
                    #if ANDROID
Android.Util.Log.Info("GLTRON", $"🎉 FBX SUCCESS! Recognizer: {_recognizerModel.Meshes.Count} meshes, {_recognizerModel.Bones.Count} bones");
#endif
#endif
                }
                catch { }
                
                return true;
            }
            else
            {
                try
                {
#if ANDROID
                    #if ANDROID
Android.Util.Log.Error("GLTRON", "❌ FBX models loaded but are null!");
#endif
#endif
                }
                catch { }
                return false;
            }
        }
        catch (ContentLoadException ex)
        {
            try
            {
#if ANDROID
                #if ANDROID
Android.Util.Log.Error("GLTRON", $"❌ FBX ContentLoadException: {ex.Message}");
#endif
                #if ANDROID
Android.Util.Log.Error("GLTRON", $"❌ Inner exception: {ex.InnerException?.Message}");
#endif
                #if ANDROID
Android.Util.Log.Error("GLTRON", "❌ This means the FBX files weren't processed correctly by content pipeline");
#endif
#endif
            }
            catch { }
            return false;
        }
        catch (Exception ex)
        {
            try
            {
#if ANDROID
                #if ANDROID
Android.Util.Log.Error("GLTRON", $"❌ FBX loading failed with unexpected error: {ex.Message}");
#endif
                #if ANDROID
Android.Util.Log.Error("GLTRON", $"❌ Exception type: {ex.GetType().Name}");
#endif
                #if ANDROID
Android.Util.Log.Error("GLTRON", $"❌ Stack trace: {ex.StackTrace}");
#endif
#endif
            }
            catch { }
            return false;
        }
    }

    // Attempt per-asset loads with alternate names, logging detailed errors
    private void TryLoadFbxPerAsset(ContentManager content)
    {
        if (_bikeModel == null)
        {
            _bikeModel = SafeLoadModel(content, new [] { "Assets/lightcyclehigh", "Assets/lightcyclehigh.fbx" }, "FBX bike (fallback) ");
        }
        if (_recognizerModel == null)
        {
            _recognizerModel = SafeLoadModel(content, new [] { "Assets/recognizerhigh", "Assets/recognizerhigh.fbx" }, "FBX recognizer (fallback) ");
        }
    }
    
    private Model? SafeLoadModel(ContentManager content, string[] names, string label)
    {
        foreach (var name in names)
        {
            try
            {
                var m = content.Load<Model>(name);
                System.Diagnostics.Debug.WriteLine($"GLTRON: ✅ Loaded {label} as '{name}'");
#if ANDROID
                try {
Android.Util.Log.Error("GLTRON", $"✅ Loaded {label} as '{name}'");
                } catch {}
#endif
                return m;
            }
            catch (ContentLoadException ex)
            {
                System.Diagnostics.Debug.WriteLine($"GLTRON: ❌ Failed to load {label} as '{name}': {ex.Message}");
#if ANDROID
                Android.Util.Log.Error("GLTRON", $"❌ Failed to load {label} as '{name}': {ex.Message}");
#endif
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GLTRON: ❌ Unexpected error loading {label} as '{name}': {ex.Message}");
#if ANDROID
                Android.Util.Log.Error("GLTRON", $"❌ Unexpected error loading {label} as '{name}': {ex.Message}");
#endif
            }
        }
        return null;
    }
    
    private bool TryLoadObjModels(ContentManager content)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("GLTRON: 🔄 Attempting to load OBJ models as fallback...");
            
            var bikeModel = SafeLoadModel(content, new [] { "Assets/lightcyclehigh", "Assets/lightcyclehigh.obj" }, "OBJ bike");
            var recognizerModel = SafeLoadModel(content, new [] { "Assets/recognizerhigh", "Assets/recognizerhigh.obj" }, "OBJ recognizer");
            
            if (bikeModel != null && recognizerModel != null)
            {
                _bikeModel = bikeModel;
                _recognizerModel = recognizerModel;
                
                System.Diagnostics.Debug.WriteLine("GLTRON: Setting up OBJ bone transforms...");
                _bikeBoneTransforms = new Matrix[_bikeModel.Bones.Count];
                _bikeModel.CopyAbsoluteBoneTransformsTo(_bikeBoneTransforms);
                
                _recognizerBoneTransforms = new Matrix[_recognizerModel.Bones.Count];
                _recognizerModel.CopyAbsoluteBoneTransformsTo(_recognizerBoneTransforms);
                
                System.Diagnostics.Debug.WriteLine($"GLTRON: 🎉 OBJ SUCCESS! Bike: {_bikeModel.Meshes.Count} meshes, {_bikeModel.Bones.Count} bones");
                System.Diagnostics.Debug.WriteLine($"GLTRON: 🎉 OBJ SUCCESS! Recognizer: {_recognizerModel.Meshes.Count} meshes, {_recognizerModel.Bones.Count} bones");
                
                return true;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("GLTRON: ❌ OBJ models loaded but are null!");
                return false;
            }
        }
        catch (ContentLoadException ex)
        {
            System.Diagnostics.Debug.WriteLine($"GLTRON: ❌ OBJ ContentLoadException: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GLTRON: ❌ OBJ loading failed: {ex.Message}");
            return false;
        }
    }
}
