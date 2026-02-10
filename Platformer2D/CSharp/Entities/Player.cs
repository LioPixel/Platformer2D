using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Fonts;
using Bliss.CSharp.Interact;
using Bliss.CSharp.Interact.Keyboards;
using Bliss.CSharp.Transformations;
using Box2D;
using MiniAudioEx.Core.StandardAPI;
using Platformer2D.CSharp.GUIs;
using Platformer2D.CSharp.Scenes;
using Platformer2D.CSharp.Scenes.Levels;
using Riptide;
using Sparkle.CSharp;
using Sparkle.CSharp.Entities;
using Sparkle.CSharp.Entities.Components;
using Sparkle.CSharp.GUI;
using Sparkle.CSharp.Physics.Dim2;
using Sparkle.CSharp.Physics.Dim2.Def;
using Sparkle.CSharp.Physics.Dim2.Shapes;
using Sparkle.CSharp.Scenes;
using Sparkle.CSharp.Content;
using Transform = Bliss.CSharp.Transformations.Transform;

namespace Platformer2D.CSharp.Entities;

public class Player : Entity
{
    public int IsPlayerOnGround;
    public PlayerPoseType PoseType;

    // Network sync properties
    public bool IsLocalPlayer;
    public Vector3 NetworkedPosition;
    public PlayerPoseType NetworkedPoseType;

    private Sprite _sprite;
    private float _timer;
    private int _frame;
    private float _frameTime = 0.1f;
    private bool _isJumping;
    private const int TotalFrames = 8;
    private AudioSource _audioSource;

    private readonly HashSet<ulong> _groundContacts = new();
    private readonly HashSet<ulong> _leftContacts = new();
    private readonly HashSet<ulong> _rightContacts = new();

    private Vector2 _previousPlatformVelocity;

    // Network update timer
    private float _networkUpdateTimer;
    private const float NetworkUpdateInterval = 0.05f; // Send updates 20 times per second

    // Debug timer
    private float _networkDebugTimer;

    // Name display
    private Font _nameFont;
    private Vector2 _nameOffset = new Vector2(0, -20);

    // Respawn system
    private const float DEATH_Y = 100f; // Y-Position für Respawn (wenn Spieler runterfällt)
    private Vector3 _spawnPoint;

    // Level completion - prevent triggering multiple times
    private bool _hasCompletedLevel = false;

    public Player(Transform transform, bool isLocalPlayer = true) : base(transform, "player")
    {
        IsLocalPlayer = isLocalPlayer;
        NetworkedPosition = transform.Translation;
        _spawnPoint = transform.Translation; // Merke Spawn-Position
    }

    protected override void Init()
    {
        base.Init();
        this.PoseType = PlayerPoseType.RightIdle;
        this.NetworkedPoseType = PlayerPoseType.RightIdle;
        this._sprite = new Sprite(ContentRegistry.PlayerIdleRight, new Vector2(168, -2), layerDepth: 0.6F);
        this.AddComponent(this._sprite);

        RigidBody2D body = new RigidBody2D(new BodyDefinition()
        {
            Type = IsLocalPlayer ? BodyType.Dynamic : BodyType.Kinematic,
            FixedRotation = true,
            GravityScale = IsLocalPlayer ? 15.5F : 0F
        }, new PolygonShape2D(Polygon.MakeBox(7, 8), new ShapeDef()
        {
            Density = 100,
            UserData = "Player",
            EnableContactEvents = IsLocalPlayer,
            EnableSensorEvents = IsLocalPlayer,
            // Collision filtering: players don't collide with each other but DO collide with blocks
            Filter = new Filter()
            {
                CategoryBits = 0x0002, // This is a player (category 2)
                MaskBits = 0xFFFD // Collide with everything (0xFFFF) EXCEPT other players (NOT 0x0002)
            }
        }));

        this.AddComponent(body);

        body.CreateShape(new ShapeDef()
        {
            IsSensor = true,
            UserData = "PlayerLeftSensor",
            EnableContactEvents = false,
            EnableSensorEvents = true,
            // Sensors detect walls/blocks but NOT other players
            Filter = new Filter()
            {
                CategoryBits = 0x0002,
                MaskBits = 0xFFFD
            }
        }, Polygon.MakeOffsetBox(2, 7, new Vector2(-7, -1), Rotation.Identity));

        body.CreateShape(new ShapeDef()
        {
            IsSensor = true,
            UserData = "PlayerRightSensor",
            EnableContactEvents = false,
            EnableSensorEvents = true,
            // Sensors detect walls/blocks but NOT other players
            Filter = new Filter()
            {
                CategoryBits = 0x0002,
                MaskBits = 0xFFFD
            }
        }, Polygon.MakeOffsetBox(2, 7, new Vector2(7, -1), Rotation.Identity));

        // Only subscribe to contact events for local player
        if (IsLocalPlayer)
        {
            ((Simulation2D)this.Scene.Simulation).ContactBeginTouch += this.ContactBeginTouch;
            ((Simulation2D)this.Scene.Simulation).ContactEndTouch += this.ContactEndTouch;
            ((Simulation2D)this.Scene.Simulation).SensorBeginTouch += this.ContactBeginSensorTouch;
            ((Simulation2D)this.Scene.Simulation).SensorEndTouch += this.ContactEndSensorTouch;
        }

        this._audioSource = new AudioSource();
        
        // Load font for name display - passe den Pfad an deine Font-Datei an
        this._nameFont = ContentRegistry.Fontoe; // Oder lade eine eigene Font
    }

    protected override void Update(double delta)
    {
        base.Update(delta);

        // Handle networked player differently
        if (!IsLocalPlayer)
        {
            UpdateNetworkedPlayer(delta);
            return;
        }

        // Check if player fell too far
        //if (this.Transform.Translation.Y > DEATH_Y)
        //{
        //    Respawn();
        //}

        bool isGround = IsPlayerOnGround > 0;
        bool isLeftWallCol = _leftContacts.Count > 0;
        bool isRightWallCol = _rightContacts.Count > 0;

        // Sprite animation.
        _timer += (float)delta;

        if (_isJumping)
        {
            if (_timer >= _frameTime)
            {
                _timer = 0;
                _frame++;

                if (_frame >= TotalFrames)
                {
                    _frame = 7;
                    _timer = 0;

                    if (isGround)
                    {
                        _timer = 0;
                        _frame = 0;
                        _isJumping = false;

                        if (this.PoseType == PlayerPoseType.JumpLeft)
                        {
                            this.PoseType = PlayerPoseType.LeftIdle;
                        }

                        if (this.PoseType == PlayerPoseType.JumpRight)
                        {
                            this.PoseType = PlayerPoseType.RightIdle;
                        }
                    }
                }

                _sprite.SourceRect = new Rectangle(this._frame * 48, 0, 48, 64);
            }
        }
        else
        {
            if (_timer >= _frameTime)
            {
                _timer = 0f;
                _frame++;

                if (_frame >= TotalFrames)
                    _frame = 0;

                _sprite.SourceRect = new Rectangle(this._frame * 48, 0, 48, 64);
            }
        }

        RigidBody2D body = this.GetComponent<RigidBody2D>()!;
        Vector2 velocity = body.LinearVelocity;

        if (GuiManager.ActiveGui == null)
        {
            // --- PLAYER MOVEMENT ---
            float groundAccel = 3f;
            float airAccel = 0.5f;
            float maxSpeed = 50f;
            float jumpForce = 90;

            float input = 0f;
            if ((Input.IsKeyDown(KeyboardKey.A) || Input.IsKeyDown(KeyboardKey.Left)) && !isLeftWallCol)
            {
                input -= 1f;
                if (!_isJumping && isGround)
                {
                    this._frameTime = 0.1F;
                    this.PoseType = PlayerPoseType.LeftWalk;
                }
            }

            if ((Input.IsKeyDown(KeyboardKey.D) || Input.IsKeyDown(KeyboardKey.Right)) && !isRightWallCol)
            {
                input += 1f;
                if (!_isJumping && isGround)
                {
                    this._frameTime = 0.1F;
                    this.PoseType = PlayerPoseType.RightWalk;
                }
            }

            if (!Input.IsKeyDown(KeyboardKey.D) && !Input.IsKeyDown(KeyboardKey.Right) &&
                !Input.IsKeyDown(KeyboardKey.A) && !Input.IsKeyDown(KeyboardKey.Left))
            {
                if (!this._isJumping)
                {
                    if (this.PoseType == PlayerPoseType.LeftWalk)
                    {
                        this._frameTime = 0.2F;
                        this.PoseType = PlayerPoseType.LeftIdle;
                    }

                    if (this.PoseType == PlayerPoseType.RightWalk)
                    {
                        this._frameTime = 0.2F;
                        this.PoseType = PlayerPoseType.RightIdle;
                    }
                }
            }

            if (input != 0f)
            {
                float accel = isGround ? groundAccel : airAccel;
                velocity.X += input * accel;
                velocity.X = Math.Clamp(velocity.X, -maxSpeed, maxSpeed);
            }
            else if (isGround)
            {
                velocity.X *= 0.8f;
            }

            // Jump
            if ((Input.IsKeyPressed(KeyboardKey.Space)
                 || Input.IsKeyPressed(KeyboardKey.W)
                 || Input.IsKeyPressed(KeyboardKey.Up))
                && isGround && !_isJumping)
            {
                velocity.Y = -jumpForce;

                if (this.PoseType == PlayerPoseType.LeftWalk || this.PoseType == PlayerPoseType.LeftIdle)
                {
                    this._frameTime = 0.1F;
                    this._timer = 0;
                    this._frame = 0;
                    this.PoseType = PlayerPoseType.JumpLeft;
                }

                if (this.PoseType == PlayerPoseType.RightWalk || this.PoseType == PlayerPoseType.RightIdle)
                {
                    this._frameTime = 0.1F;
                    this._timer = 0;
                    this._frame = 0;
                    this.PoseType = PlayerPoseType.JumpRight;
                }

                this._isJumping = true;

                if (((PlatformerGame)Game.Instance!).OptionsConfig.GetValue<bool>("Sounds"))
                {
                    this._audioSource.Play(ContentRegistry.Jump);
                }
            }
        }

        body.LinearVelocity = velocity;

        // Network position sync
        _networkUpdateTimer += (float)delta;
        if (_networkUpdateTimer >= NetworkUpdateInterval)
        {
            _networkUpdateTimer = 0;
            NetworkManager.SendPlayerPosition(this.Transform.Translation, this.PoseType);
        }

        // Game over!
        this.GameOver();

        // Set sprite type.
        this.SetSpriteByPoseType();
    }

    private void Respawn()
    {
        this.Transform.Translation = _spawnPoint;
        RigidBody2D body = this.GetComponent<RigidBody2D>()!;
        body.LinearVelocity = Vector2.Zero; // Reset velocity
        
        // Optional: Spiele einen Sound ab
        // if (((PlatformerGame)Game.Instance!).OptionsConfig.GetValue<bool>("Sounds"))
        // {
        //     this._audioSource.Play(ContentRegistry.RespawnSound);
        // }
    }
    
    public void SetSpawnPoint(Vector3 spawnPoint)
    {
        _spawnPoint = spawnPoint;
    }

    private void UpdateNetworkedPlayer(double delta)
    {
        // Update pose type
        this.PoseType = NetworkedPoseType;

        // Directly set position from network data
        // For kinematic bodies, we set the transform directly
        this.Transform.Translation = NetworkedPosition;

        // Debug every second
        _networkDebugTimer += (float)delta;
        if (_networkDebugTimer >= 1.0f)
        {
            _networkDebugTimer = 0;
        }

        // Update sprite animation
        _timer += (float)delta;

        if (_timer >= _frameTime)
        {
            _timer = 0f;
            _frame++;

            if (_frame >= TotalFrames)
                _frame = 0;

            _sprite.SourceRect = new Rectangle(this._frame * 48, 0, 48, 64);
        }

        // Set sprite type
        this.SetSpriteByPoseType();
    }

    protected override void FixedUpdate(double fixedStep)
    {
        base.FixedUpdate(fixedStep);

        // Only apply physics to local player
        if (!IsLocalPlayer) return;

        RigidBody2D body = this.GetComponent<RigidBody2D>()!;

        Vector2 platformVelocity = Vector2.Zero;

        foreach (ContactData contact in body.Contacts)
        {
            if (contact.ShapeA.UserData?.ToString() == "MovingBlock")
            {
                platformVelocity = contact.ShapeA.Body.LinearVelocity;
            }
            else if (contact.ShapeB.UserData?.ToString() == "MovingBlock")
            {
                platformVelocity = contact.ShapeB.Body.LinearVelocity;
            }
        }

        body.LinearVelocity += new Vector2(platformVelocity.X, 0);
    }

    public void GameOver()
    {
        if (!IsLocalPlayer) return;

        if (this.Transform.Translation.Y >= 5 * 16)
        {
            GuiManager.SetGui(new GameOverGui());
        }
    }

    private void ContactBeginSensorTouch(SensorBeginTouchEvent e)
    {
        if ((e.SensorShape.UserData?.ToString() == "PlayerLeftSensor") ||
            e.VisitorShape.UserData?.ToString() == "PlayerLeftSensor")
        {
            _leftContacts.Add(ContactKey(e.SensorShape, e.VisitorShape));
        }

        if ((e.SensorShape.UserData?.ToString() == "PlayerRightSensor") ||
            e.VisitorShape.UserData?.ToString() == "PlayerRightSensor")
        {
            _rightContacts.Add(ContactKey(e.SensorShape, e.VisitorShape));
        }
    }

    private void ContactEndSensorTouch(SensorEndTouchEvent e)
    {
        ulong key = ContactKey(e.SensorShape, e.VisitorShape);
        _leftContacts.Remove(key);
        _rightContacts.Remove(key);
    }

    private void ContactBeginTouch(ContactBeginTouchEvent e)
    {
        if (IsGroundContact(e))
        {
            ulong key = ContactKey(e.ShapeA, e.ShapeB);
            if (_groundContacts.Add(key))
            {
                IsPlayerOnGround = _groundContacts.Count;
            }
        }

        if ((e.ShapeA.UserData?.ToString() == "flag") ||
            e.ShapeB.UserData?.ToString() == "flag")
        {
            ((LevelScene)this.Scene).WonLevel = true;
            
            // Only the local player should trigger network level completion
            if (IsLocalPlayer && !_hasCompletedLevel)
            {
                _hasCompletedLevel = true;
                OnLevelComplete();
            }
        }

        if (e.ShapeA.UserData is Portal entity1)
        {
            this.Transform.Translation = new Vector3(entity1.TeleportPos, 0);
            SceneManager.ActiveCam2D!.Position = entity1.TeleportPos;
        }

        if (e.ShapeB.UserData is Portal entity2)
        {
            this.Transform.Translation = new Vector3(entity2.TeleportPos, 0);
            SceneManager.ActiveCam2D!.Position = entity2.TeleportPos;
        }
    }

    private void OnLevelComplete()
    {
        // Determine the next level based on current scene
        string nextLevel = DetermineNextLevel();
        
        Bliss.CSharp.Logging.Logger.Info($"[PLAYER] Level completed! Moving all players to: {nextLevel}");
        
        // Notify the server to move all players to the next level
        NetworkManager.NotifyLevelComplete(nextLevel);
    }

    private string DetermineNextLevel()
    {
        // Check what the current scene is and determine next level
        if (this.Scene is Level1)
        {
            return "Level 2";
        }
        else if (this.Scene is Level2)
        {
            return "Level 3";
        }
        else if (this.Scene is Level3)
        {
            return "Level 4";
        }
        else if (this.Scene is Level4)
        {
            return "Level 5";
        }
        else if (this.Scene is Level5)
        {
            return "Level 6";
        }
        else if (this.Scene is Level6)
        {
            return "Level 7";
        }
        else if (this.Scene is Level7)
        {
            return "Level 8";
        }
        else if (this.Scene is Level8)
        {
            return "Level 9";
        }
        else if (this.Scene is Level9)
        {
            return "Level 10";
        }
        
        // Default fallback
        return "Level 1";
    }

    private void ContactEndTouch(ContactEndTouchEvent e)
    {
        ulong key = ContactKey(e.ShapeA, e.ShapeB);
        if (_groundContacts.Remove(key))
        {
            IsPlayerOnGround = _groundContacts.Count;
        }
    }

    private void SetSpriteByPoseType()
    {
        switch (this.PoseType)
        {
            case PlayerPoseType.LeftIdle:
                this._sprite.Texture = ContentRegistry.PlayerIdleLeft;
                break;
            case PlayerPoseType.RightIdle:
                this._sprite.Texture = ContentRegistry.PlayerIdleRight;
                break;
            case PlayerPoseType.LeftWalk:
                this._sprite.Texture = ContentRegistry.PlayerWalkLeft;
                break;
            case PlayerPoseType.RightWalk:
                this._sprite.Texture = ContentRegistry.PlayerWalkRight;
                break;
            case PlayerPoseType.JumpLeft:
                this._sprite.Texture = ContentRegistry.PlayerJumpLeft;
                break;
            case PlayerPoseType.JumpRight:
                this._sprite.Texture = ContentRegistry.PlayerJumpRight;
                break;
        }
    }

    private ulong ContactKey(Shape a, Shape b)
    {
        unchecked
        {
            ulong ha = (ulong)a.GetHashCode();
            ulong hb = (ulong)b.GetHashCode();
            return ha < hb ? (ha << 32) ^ hb : (hb << 32) ^ ha;
        }
    }

    private bool IsGroundContact(ContactBeginTouchEvent e)
    {
        bool footIsA = e.ShapeA.UserData?.ToString() == "Player";
        bool footIsB = e.ShapeB.UserData?.ToString() == "Player";
        if (!footIsA && !footIsB) return false;

        var n = e.Manifold.Normal;
        if (footIsA) n = -n;

        var sim = (Simulation2D)this.Scene.Simulation;
        var g = Vector2.Normalize(sim.World.Gravity);
        if (g.LengthSquared() < 1e-6f) g = new Vector2(0, 1);

        return Vector2.Dot(n, -g) > 0.6f;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing && IsLocalPlayer)
        {
            ((Simulation2D)this.Scene.Simulation).ContactBeginTouch -= this.ContactBeginTouch;
            ((Simulation2D)this.Scene.Simulation).ContactEndTouch -= this.ContactEndTouch;
            ((Simulation2D)this.Scene.Simulation).SensorBeginTouch -= this.ContactBeginSensorTouch;
            ((Simulation2D)this.Scene.Simulation).SensorEndTouch -= this.ContactEndSensorTouch;
        }
    }
}