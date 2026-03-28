using System.Numerics;
using Bliss.CSharp.Textures;
using Bliss.CSharp.Transformations;
using Pixelis.CSharp.Entities;
using Pixelis.CSharp.GUIs.Loading;
using Pixelis.CSharp.Items;
using Sparkle.CSharp.Scenes;
using Sparkle.CSharp.Utils.Async;
using Transform = Bliss.CSharp.Transformations.Transform;

namespace Pixelis.CSharp.Scenes.Levels;

public class Level1 : LevelScene
{
    private readonly List<PickupItem> _items = new();

    public Level1() : base("Level 1") {}

    protected override void Init()
    {
        base.Init();
        this.Background = ContentRegistry.Background;

        this.CreatePlatform(0, 0, 4);
        this.CreatePlantSunFlower(2, -1);
        this.CreatePlatform(6, -1, 2);
        this.CreateRockWithGrass(7, -2);
        this.CreateStair(10, -1, 4, StairType.Up);
        this.CreateBushDead(11, -3);
        this.CreatePlatform(13, -4, 6);
        this.CreateOakLog(15, -5);
        this.CreateWinFlag(18, -5);

        // Items spawnen
        this.SpawnItem(2,  -2, ItemType.SpeedBoost);
        this.SpawnItem(7,  -3, ItemType.GrowBig);
        this.SpawnItem(15, -6, ItemType.ShrinkSmall);
    }

    private void SpawnItem(int blockPosX, int blockPosY, ItemType type)
    {
        Texture2D texture = type switch
        {
            ItemType.GrowBig     => ContentRegistry.ItemBig,
            ItemType.ShrinkSmall => ContentRegistry.ItemMini,
            ItemType.SpeedBoost  => ContentRegistry.ItemFast,
            _                    => ContentRegistry.ItemBig
        };

        var transform = new Transform() { Translation = new Vector3(blockPosX * 16, blockPosY * 16, 0) };
        var item = new PickupItem(transform, type, texture);
        _items.Add(item);
        this.AddEntity(item);
    }

    protected override void Update(double delta)
    {
        base.Update(delta);

        // Pickup prüfen
        foreach (var entity in this.GetEntities())
        {
            if (entity is not Player player || !player.IsLocalPlayer) continue;

            foreach (var item in _items)
            {
                if (item.TryCollect(player.LocalTransform.Translation))
                    player.ApplyItem(new ItemEffect(item.Type, duration: 7f));
            }

            break; // Nur lokalen Spieler prüfen
        }
    }

    protected override void OnLevelWon()
    {
        // Only transition locally if we're NOT in a network game
        // In network games, the server handles the transition
        if (NetworkManager.Client == null || !NetworkManager.Client.IsConnected)
        {
            AsyncOperation operation10 = SceneManager.LoadSceneAsync(new Level2(), new ProgressBarLoadingGui("Loading"));
            operation10.Completed += success =>
            {
                Player player = new Player(new Transform() { Translation = new Vector3(0, -16 * 2, 0) });
                SceneManager.ActiveScene?.AddEntity(player);
            };
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Only cleanup network if we're actually quitting, not during level transitions
            if (!NetworkManager.IsLevelTransition)
            {
                NetworkManager.Cleanup();
            }

            base.Dispose(disposing);
        }
    }
}