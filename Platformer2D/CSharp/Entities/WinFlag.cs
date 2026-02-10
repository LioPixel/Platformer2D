using System.Numerics;
using Box2D;
using Sparkle.CSharp.Entities;
using Sparkle.CSharp.Entities.Components;
using Sparkle.CSharp.Physics.Dim2.Def;
using Sparkle.CSharp.Physics.Dim2.Shapes;
using Transform = Bliss.CSharp.Transformations.Transform;

namespace Platformer2D.CSharp.Entities;

public class WinFlag : Entity
{
    public WinFlag(Transform transform) : base(transform, "flag") { }

    protected override void Init()
    {
        base.Init();
        
        this.AddComponent(new Sprite(ContentRegistry.WinFlag, Vector2.Zero));
        
        RigidBody2D body = new RigidBody2D(new BodyDefinition()
        {
            Type = BodyType.Static
        }, new PolygonShape2D(Polygon.MakeBox(12, 48 / 2.0F), new ShapeDef()
        {
            Density = 100,
            UserData = "flag",
            EnableContactEvents = true
        }));
        
        this.AddComponent(body);
    }
}