using Bliss.CSharp.Colors;
using Bliss.CSharp.Textures;
using Sparkle.CSharp.Scenes;

namespace Platformer2D.CSharp.Scenes.Levels;

public class Level8 : LevelScene
{

    public Level8() : base("Level 8")
    {
        
    }
    
    protected override void Init()
    {
        base.Init();
        this.Background = ContentRegistry.Background8;
        
        this.CreatePlatform(0,0,1);
        this.CreateBushDead(0,-1);
        this.CreateMovingPlatform(3,0,2,7,0,1);
        this.CreatePlatform(10,0,3);
        this.CreateTree(11,-1);
        this.CreateMovingPlatform(17,0,1,17,-7,1);
        this.CreateStair(20,-8,3,StairType.Up);
        this.CreatePlatform(27,-10,1);
        this.CreatePlantSunFlower(27,-11);
        this.CreatePlatform(32,-10,1);
        this.CreateMovingPlatform(36,-10,2,36,-5,1);
        this.CreatePlatform(42,-10,3);
        this.CreateBush(43,-11);
        this.CreatePlantFlowerRed(54,-12);
        this.CreateStair(48,-11,2,StairType.Up);
        this.CreateStair(54,-11,3,StairType.Down);
        this.CreatePlatform(59,-9,5);
        this.CreateOakLog(60,-10);
        this.CreatePlatform(68,-9,1);
        this.CreateWinFlag(73,-11);
    }

    protected override void OnLevelWon()
    {
        if (NetworkManager.Client == null || !NetworkManager.Client.IsConnected)
        {
            SceneManager.SetScene(new Level9());
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