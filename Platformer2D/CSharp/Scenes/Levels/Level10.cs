using Bliss.CSharp.Colors;
using Bliss.CSharp.Textures;
using Sparkle.CSharp.Scenes;

namespace Platformer2D.CSharp.Scenes.Levels;

public class Level10 : LevelScene
{
    public Level10() : base("Level 10") {}
    
    protected override void Init()
    {
        base.Init();
        this.Background = ContentRegistry.Background11;
        
        this.CreatePlatform(0, 0, 2);
        this.CreatePlatform(6, 0, 1);
        this.CreatePlatform(9, -1, 2);
        this.CreatePlantFlowerRed(9, -2); 
        this.CreatePlatform(13, -2, 2);
        this.CreatePlatform(18, 0, 3);
        this.CreateTree(19, -1);
        this.CreatePlatform(25, 0, 1);
        this.CreateBush(25, -1);
        this.CreatePlatform(29, -1, 1);
        this.CreatePlatform(33, -2, 1);
        this.CreatePlatform(37, -3, 3);
        this.CreateStair(42, -2, 3, StairType.Down);
        this.CreateRockWithGrass(44, -1);
        this.CreatePlatform(45, 1, 2);
        this.CreatePlatform(51, 1, 2);
        this.CreatePlantSunFlower(51, 0);
        this.CreatePlatform(55, 0, 1);
        this.CreatePlatform(59, -1, 1);
        this.CreatePlatform(63, -2, 4);
        this.CreateWinFlag(65, -3);
    }

    protected override void OnLevelWon()
    {
        // Da dies das neueste Level ist, laden wir es bei Sieg neu oder gehen zu Level 8
        if (NetworkManager.Client == null || !NetworkManager.Client.IsConnected)
        {
            SceneManager.SetScene(new Level1());
        }
    }

    public override void OnLevelReset()
    {
        SceneManager.SetScene(new Level10());
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