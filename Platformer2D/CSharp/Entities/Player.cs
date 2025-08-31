using System.Numerics;
using Bliss.CSharp.Interact;
using Bliss.CSharp.Interact.Keyboards;
using Bliss.CSharp.Logging;
using Bliss.CSharp.Transformations;
using Box2D;
using Platformer2D.CSharp.GUIs;
using Platformer2D.CSharp.Scenes;
using Sparkle.CSharp;
using Sparkle.CSharp.Entities;
using Sparkle.CSharp.Entities.Components;
using Sparkle.CSharp.GUI;
using Sparkle.CSharp.Physics.Dim2;
using Sparkle.CSharp.Physics.Dim2.Def;
using Sparkle.CSharp.Physics.Dim2.Shapes;
using Sparkle.CSharp.Scenes;
using Transform = Bliss.CSharp.Transformations.Transform;

namespace Platformer2D.CSharp.Entities;

public class Player : Entity
{
    
    public int IsPlayerOnGround;
    public PlayerPoseType PoseType;
    
    private Sprite _sprite;
    private float _timer;
    private int _frame;
    private float _frameTime = 0.1f;
    private bool _isJumping;
    private float _jumpTime = 0.8F;
    
    private const int TotalFrames = 8;
    
    private readonly HashSet<ulong> _groundContacts = new();
    
    private readonly HashSet<ulong> _leftContacts = new();
    private readonly HashSet<ulong> _rightContacts = new();
    
    public Player(Transform transform) : base(transform, "player") { }

    protected override void Init()
    {
        base.Init();
        this.PoseType = PlayerPoseType.RightIdle;
        this._sprite = new Sprite(ContentRegistry.PlayerIdleRight, new Vector2(168, -2), layerDepth: 0.6F);
        this.AddComponent(this._sprite);
        
        RigidBody2D body = new RigidBody2D(new BodyDefinition()
        {
            Type = BodyType.Dynamic,
            FixedRotation = true,
            GravityScale = 15.5F
        }, new PolygonShape2D(Polygon.MakeBox(7, 8), new ShapeDef()
        {
            Density = 100,
            UserData = "Player",
            EnableContactEvents = true,
            EnableSensorEvents = true
        }));
        
        this.AddComponent(body);
        
        body.CreateShape(new ShapeDef()
        {
            IsSensor = true,
            UserData = "PlayerLeftSensor",
            EnableContactEvents = false,
            EnableSensorEvents = true
        }, Polygon.MakeOffsetBox(2, 7, new Vector2(-7, -1), Rotation.Identity));
        
        body.CreateShape(new ShapeDef()
        {
            IsSensor = true,
            UserData = "PlayerRightSensor",
            EnableContactEvents = false,
            EnableSensorEvents = true
        }, Polygon.MakeOffsetBox(2, 7, new Vector2(7, -1), Rotation.Identity));
        
        // Contact event.
        ((Simulation2D) this.Scene.Simulation).ContactBeginTouch += this.ContactBeginTouch;
        ((Simulation2D) this.Scene.Simulation).ContactEndTouch += this.ContactEndTouch;
        ((Simulation2D) this.Scene.Simulation).SensorBeginTouch += this.ContactBeginSensorTouch;
        ((Simulation2D) this.Scene.Simulation).SensorEndTouch += this.ContactEndSensorTouch;
    }

    protected override void Update(double delta)
    {
        base.Update(delta);
        bool isGround = IsPlayerOnGround > 0;
        bool isLeftWallCol = _leftContacts.Count > 0; // CHATGPT HERE.
        bool isRightWallCol = _rightContacts.Count > 0; // CHATGPT HERE.
        
        // Sprite animation.
        // increment timer
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
                
                // update sprite frame (assuming your Sprite supports SetFrame)
                _sprite.SourceRect = new Rectangle(this._frame * 48, 0, 48, 64);
            }
        }
        else
        {
            // check if it's time to switch frame
            if (_timer >= _frameTime)
            {
                _timer = 0f;
                _frame++;

                if (_frame >= TotalFrames)
                    _frame = 0;

                // update sprite frame (assuming your Sprite supports SetFrame)
                _sprite.SourceRect = new Rectangle(this._frame * 48, 0, 48, 64);
            }
        }
        
        RigidBody2D body = this.GetComponent<RigidBody2D>()!;

        // --- PLAYER MOVEMENT ---
        float groundAccel = 3f;    // how fast player accelerates on ground
        float airAccel = 0.5f;     // how fast player accelerates in air
        float maxSpeed = 50f;      // max horizontal speed
        float jumpForce = 90;

        Vector2 velocity = body.LinearVelocity;
        
        // Horizontal input
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
        
        if (!Input.IsKeyDown(KeyboardKey.D) && !Input.IsKeyDown(KeyboardKey.Right) && !Input.IsKeyDown(KeyboardKey.A) && !Input.IsKeyDown(KeyboardKey.Left))
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
            // pick acceleration based on grounded state
            float accel = isGround ? groundAccel : airAccel;

            // accelerate towards desired direction
            velocity.X += input * accel;

            // clamp max horizontal speed
            velocity.X = Math.Clamp(velocity.X, -maxSpeed, maxSpeed);
        }
        else if (isGround)
        {
            // simple friction on ground when no input
            velocity.X *= 0.8f;
        }

        // Jump
        if (Input.IsKeyPressed(KeyboardKey.Space) && isGround && !_isJumping)
        {
            velocity.Y = -jumpForce; // usually negative Y is upward
            
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
        }

        body.LinearVelocity = velocity;
        
        // Game over!
        this.GameOver();
        
        // Set sprite type.
        this.SetSpriteByPoseType();
    }
    
    public void GameOver()
    {
        if (this.Transform.Translation.Y >= 5 * 16)
        {
            GuiManager.SetGui(new GameOverGui());
        }
    }

    private void ContactBeginSensorTouch(SensorBeginTouchEvent e)
    {
        // Check wall left.
        if ((e.SensorShape.UserData?.ToString() == "PlayerLeftSensor") ||
            e.VisitorShape.UserData?.ToString() == "PlayerLeftSensor")
        {
            Logger.Error("LEFT WALLL");
            _leftContacts.Add(ContactKey(e.SensorShape, e.VisitorShape));
        }
        
        // Check wall right.
        if ((e.SensorShape.UserData?.ToString() == "PlayerRightSensor") ||
            e.VisitorShape.UserData?.ToString() == "PlayerRightSensor")
        {
            Logger.Error("RIGHT WALLL");
            _rightContacts.Add(ContactKey(e.SensorShape, e.VisitorShape));
        }
    }

    private void ContactEndSensorTouch(SensorEndTouchEvent e)
    {
        // Check wall.
        ulong key = ContactKey(e.SensorShape, e.VisitorShape);
        _leftContacts.Remove(key);
        _rightContacts.Remove(key);
        
        Logger.Error("Test");
    }

    private void ContactBeginTouch(ContactBeginTouchEvent e)
    {
        // Check ground.
        if (IsGroundContact(e))
        {
            ulong key = ContactKey(e.ShapeA, e.ShapeB);
            if (_groundContacts.Add(key))
            {
                IsPlayerOnGround = _groundContacts.Count;
            }
        }
        
        // Win level.
        if ((e.ShapeA.UserData?.ToString() == "flag") ||
            e.ShapeB.UserData?.ToString() == "flag")
        {
            ((LevelScene) this.Scene).WonLevel = true;
        }
        
        // Portal teleport.
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

    private void ContactEndTouch(ContactEndTouchEvent e)
    {
        // Check ground.
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
        // Must involve the Foot
        bool footIsA = e.ShapeA.UserData?.ToString() == "Player";
        bool footIsB = e.ShapeB.UserData?.ToString() == "Player";
        if (!footIsA && !footIsB) return false;

        // Get world normal
        var n = e.Manifold.Normal;

        // Flip normal if needed so it always points from ground->foot
        if (footIsA) n = -n;

        // Compare against gravity (works for Y-down or Y-up)
        var sim = (Simulation2D)this.Scene.Simulation;
        var g = Vector2.Normalize(sim.World.Gravity);
        if (g.LengthSquared() < 1e-6f) g = new Vector2(0, 1);

        // If normal opposes gravity, it’s a floor
        return Vector2.Dot(n, -g) > 0.6f;
    }
    
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            // Contact event.
            ((Simulation2D) this.Scene.Simulation).ContactBeginTouch -= this.ContactBeginTouch;
            ((Simulation2D) this.Scene.Simulation).ContactEndTouch -= this.ContactEndTouch;
        }
    }
}