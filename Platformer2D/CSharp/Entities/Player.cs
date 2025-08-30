using System.Numerics;
using Bliss.CSharp.Interact;
using Bliss.CSharp.Interact.Keyboards;
using Bliss.CSharp.Logging;
using Box2D;
using Platformer2D.CSharp.GUIs;
using Platformer2D.CSharp.Scenes;
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
    
    public Player(Transform transform) : base(transform, "player") { }

    protected override void Init()
    {
        base.Init();
        this.AddComponent(new Sprite(ContentRegistry.Player, Vector2.Zero));
        
        RigidBody2D body = new RigidBody2D(new BodyDefinition()
        {
            Type = BodyType.Dynamic,
            FixedRotation = true,
            GravityScale = 4.5F
        }, new PolygonShape2D(Polygon.MakeBox(4, 8), new ShapeDef()
        {
            Density = 100,
            UserData = "Foot",
            EnableContactEvents = true
        }));
        
        this.AddComponent(body);
        
        // Contact event.
        ((Simulation2D) this.Scene.Simulation).ContactBeginTouch += this.ContactBeginTouch;
        ((Simulation2D) this.Scene.Simulation).ContactEndTouch += this.ContactEndTouch;
    }

    protected override void Update(double delta)
    {
        base.Update(delta);
        
        RigidBody2D body = this.GetComponent<RigidBody2D>()!;

        // --- PLAYER MOVEMENT ---
        float groundAccel = 3f;    // how fast player accelerates on ground
        float airAccel = 0.5f;     // how fast player accelerates in air
        float maxSpeed = 50f;      // max horizontal speed
        float jumpForce = 40;

        Vector2 velocity = body.LinearVelocity;

        bool isGround = IsPlayerOnGround > 0;
        
        // Horizontal input
        float input = 0f;
        if (Input.IsKeyDown(KeyboardKey.A) || Input.IsKeyDown(KeyboardKey.Left)) input -= 1f;
        if (Input.IsKeyDown(KeyboardKey.D) || Input.IsKeyDown(KeyboardKey.Right)) input += 1f;

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
        if (Input.IsKeyPressed(KeyboardKey.Space) && isGround)
        {
            velocity.Y = -jumpForce; // usually negative Y is upward
        }

        body.LinearVelocity = velocity;
        
        // Game over!
        this.GameOver();
    }
    
    public void GameOver()
    {
        if (this.Transform.Translation.Y >= 5 * 16)
        {
            GuiManager.SetGui(new GameOverGui());
        }
    }

    private void ContactBeginTouch(ContactBeginTouchEvent e)
    {
        // Check ground.
        if ((e.ShapeA.UserData?.ToString() == "Foot" && e.ShapeB.Body.Type == BodyType.Static) ||
            e.ShapeB.UserData?.ToString() == "Foot" && e.ShapeA.Body.Type == BodyType.Static)
        {
            IsPlayerOnGround += 1;
        }
        
        // Win level.
        if ((e.ShapeA.UserData?.ToString() == "flag") ||
            e.ShapeB.UserData?.ToString() == "flag")
        {
            ((LevelScene) this.Scene).WonLevel = true;
        }
    }

    private void ContactEndTouch(ContactEndTouchEvent e)
    {
        // Check ground.
        if ((e.ShapeA.UserData?.ToString() == "Foot" && e.ShapeB.Body.Type == BodyType.Static) ||
            e.ShapeB.UserData?.ToString() == "Foot" && e.ShapeA.Body.Type == BodyType.Static)
        {
            IsPlayerOnGround -= 1;
        }
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