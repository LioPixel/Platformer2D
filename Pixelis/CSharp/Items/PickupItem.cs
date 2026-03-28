using System.Numerics;
using Bliss.CSharp.Textures;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Entities;
using Sparkle.CSharp.Entities.Components;
using Pixelis.CSharp.Items;

namespace Pixelis.CSharp.Items;

public class PickupItem : Entity {

    public  ItemType Type { get; }
    public  bool Collected { get; private set; }

    private readonly Vector3 _origin;
    private float _bobTimer;
    private Sprite _sprite;
    private readonly Texture2D _texture;

    public PickupItem(Transform transform, ItemType type, Texture2D texture) : base(transform, "pickupItem") {
        Type     = type;
        _origin  = transform.Translation;
        _texture = texture;
    }

    protected override void Init() {
        base.Init();

        _sprite = new Sprite(_texture, Vector2.Zero, layerDepth: 0.5F);
        this.AddComponent(_sprite);
    }

    protected override void Update(double delta) {
        base.Update(delta);

        if (Collected) return;

        // Bobbing-Animation (hoch/runter)
        _bobTimer += (float)delta * 2.5f;
        this.LocalTransform.Translation = _origin with {
            Y = _origin.Y + MathF.Sin(_bobTimer) * 3f
        };
    }

    /// <summary>
    /// Gibt true zurück wenn der Spieler nah genug ist und das Item
    /// gerade aufgesammelt wurde.
    /// </summary>
    public bool TryCollect(Vector3 playerPos, float radius = 15) {
        if (Collected) return false;

        float dist = Vector2.Distance(
            new Vector2(this.LocalTransform.Translation.X, this.LocalTransform.Translation.Y),
            new Vector2(playerPos.X, playerPos.Y)
        );

        if (dist > radius) return false;

        Collected = true;
        this.Dispose();   // Entity aus der Scene entfernen
        return true;
    }
}