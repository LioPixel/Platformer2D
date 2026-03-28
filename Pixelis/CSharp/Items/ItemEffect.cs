namespace Pixelis.CSharp.Items;

public class ItemEffect {

    public ItemType Type      { get; }
    public float    Duration  { get; }
    private float   _remaining;

    public ItemEffect(ItemType type, float duration = 6f) {
        Type       = type;
        Duration   = duration;
        _remaining = duration;
    }

    public void Update(float dt) => _remaining -= dt;
    public bool IsExpired()      => _remaining <= 0f;
    
    public float Progress        => Math.Clamp(_remaining / Duration, 0f, 1f);
}