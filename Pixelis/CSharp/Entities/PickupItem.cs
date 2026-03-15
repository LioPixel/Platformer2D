using System.Numerics;
using Bliss.CSharp.Colors;
using Box2D;
using Sparkle.CSharp.Entities;
using Sparkle.CSharp.Entities.Components;
using Sparkle.CSharp.Physics.Dim2.Def;
using Sparkle.CSharp.Physics.Dim2.Shapes;
using Transform = Bliss.CSharp.Transformations.Transform;

namespace Pixelis.CSharp.Entities;

public enum PickupType
{
    Grow,
    Shrink,
    Speed
}

public class PickupItem : Entity
{
    public PickupType Type { get; }

    private bool _consumed;

    public PickupItem(Transform transform, PickupType type) : base(transform, "pickup")
    {
        Type = type;
    }

    protected override void Init()
    {
        base.Init();

        Color tint = Type switch
        {
            PickupType.Grow => new Color(80, 200, 255),
            PickupType.Shrink => new Color(255, 120, 120),
            PickupType.Speed => new Color(120, 255, 120),
            _ => Color.White
        };

        Sprite sprite = new Sprite(ContentRegistry.Sprite, Vector2.Zero, layerDepth: 0.45F, color: tint);
        this.AddComponent(sprite);

        this.LocalTransform.Scale = new Vector3(0.7F, 0.7F, 1.0F);

        RigidBody2D body = new RigidBody2D(new BodyDefinition()
        {
            Type = BodyType.Static
        }, new PolygonShape2D(Polygon.MakeBox(6, 6), new ShapeDef()
        {
            IsSensor = true,
            UserData = this,
            EnableSensorEvents = true,
            EnableContactEvents = false
        }));

        this.AddComponent(body);
    }

    public void TryConsume(Player player)
    {
        if (_consumed)
        {
            return;
        }

        _consumed = true;
        player.ApplyPickup(Type);
        this.Scene.RemoveEntity(this);
    }
}
