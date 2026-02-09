using Bliss.CSharp.Colors;
using Bliss.CSharp.Textures;
using Sparkle.CSharp.Scenes;

namespace Platformer2D.CSharp.Scenes.Levels;

public class Level7 : LevelScene
{

    public Level7() : base("Level 7")
    {
        
    }
    
    protected override void Init()
    {
        base.Init();
        this.Background = ContentRegistry.Background11;
        
        this.CreatePlatform(0,0,1);
        this.CreatePlantFlowerRed(4,-2);
        this.CreatePlatform(4,-1,2);
        this.CreateStair(9,0,3,StairType.Down);
        this.CreatePlantSunFlower(11,1);
        this.CreateBushDead(15,1);
        this.CreatePlatform(15,2,1);
        this.CreatePlatform(20,2,1);
        this.CreateRockWithGrass(25,1);
        this.CreatePlatform(25,2,1);
        this.CreatePlatform(30,2,1);
        this.CreatePlantFlowerRed(30,1);
        this.CreatePlatform(35,2,1);
        this.CreateBush(50,1);
        this.CreatePlatform(40,2,1);
        this.CreateOakLog(65,-3);
        this.CreatePlatform(45,2,1);
        this.CreatePlatform(50,2,1);
        this.CreateTree(69,-3);
        this.CreateStair(54,1,2,StairType.Up);
        this.CreateStair(59,-1,2,StairType.Up);
        this.CreatePlatform(63, -2, 10);
        this.CreatePlatform(77, -1,2);
        this.CreatePlatform(81, 0, 1);
        this.CreatePlantSunFlower(81,-1);
        this.CreatePlatform(84,-1, 1);
        this.CreateRockWithGrass(87,-3);
        this.CreatePlatform(87, -2, 1);
        this.CreatePlatform(90,-1,1);
        this.CreatePlatform(93,-2,1);
        this.CreateStair(95,-3,3,StairType.Up);
        this.CreateBushDead(96,-5);
        this.CreateWinFlag(102,-6);
    }

    protected override void OnLevelWon()
    {
        SceneManager.SetScene(new Level8());
    }

    public override void OnLevelReset()
    {
        SceneManager.SetScene(new Level7());
    }
    
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            NetworkManager.Cleanup();
            
            base.Dispose(disposing);
        }
    }
}