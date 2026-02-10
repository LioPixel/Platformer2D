using Bliss.CSharp.Colors;
using Bliss.CSharp.Textures;
using Sparkle.CSharp.Scenes;

namespace Platformer2D.CSharp.Scenes.Levels;

public class Level5 : LevelScene
{
    
    public Level5() : base("Level 5") {}
    
    protected override void Init()
    {
        base.Init();
        this.Background = ContentRegistry.Background10;
        
        this.CreatePlatform(0, 0, 2); 
        this.CreateRockWithGrass(1,-1);
        this.CreatePlatform(5, -1, 3); 
        this.CreatePlantSunFlower(6,-2);
        this.CreatePlantFlowerRed(13,-4);
        this.CreateStair(11, -1, 3, StairType.Up); 
        this.CreateBushDead(21,-4);
        this.CreatePlatform(17, -3, 5);
        this.CreatePlantSunFlower(31,-6);
        this.CreateRockWithGrass(49,-8);
        this.CreatePortal(26, 0, 0, -2, Color.DarkRed);
        this.CreatePlatform(25, -4, 3);
        this.CreatePlatform(31, -5, 2);
        this.CreatePlatform(36, -6, 2);
        this.CreateStair(42, -5, 2, StairType.Down);
        this.CreateStair(47, -5, 4, StairType.Up);
        this.CreatePlatform(55, -8, 3);
        this.CreatePlatform(62, -8, 1);
        this.CreatePlatform(66, -9, 1); 
        this.CreateBushDead(66,-10);
        this.CreateStair(71, -8, 3, StairType.Down);
        this.CreatePlatform(78, -6, 8);
        this.CreateTree(79,-7);
        this.CreateTree(81,-7);
        this.CreateTree(83,-7);
        this.CreateWinFlag(84, -7);
        
    }

    protected override void OnLevelWon()
    {
        if (NetworkManager.Client == null || !NetworkManager.Client.IsConnected)
        {
            SceneManager.SetScene(new Level6());
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