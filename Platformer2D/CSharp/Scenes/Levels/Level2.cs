using Sparkle.CSharp.Scenes;

namespace Platformer2D.CSharp.Scenes.Levels;

public class Level2 : LevelScene
{
    public Level2() : base("Level 2") { }
    
    protected override void Init()
    {
        base.Init();
        
        this.Background = ContentRegistry.Background2;
        this.CreatePlatform(0, 0, 3);
        this.CreatePlantSunFlower(2, -1);
        this.CreatePlatform(6, 1, 3);
        this.CreateOakLog(7, 0);
        this.CreatePlatform(11, 0, 3);
        this.CreatePlantSunFlower(11, -1);
        this.CreateBushDead(13, -1);
        this.CreateMovingPlatform(15, 0, 2, 17, -1, 1);
        this.CreatePlatform(20, -1, 2);
        this.CreateRockWithGrass(21, -2);
        this.CreatePlatform(23, 0, 6);
        this.CreatePlantFlowerRed(25, -1);
        this.CreateWinFlag(28, -1);
    }

    protected override void OnLevelWon()
    {
        SceneManager.SetScene(new Level3());
    }
    
    public override void OnLevelReset()
    {
        SceneManager.SetScene(new Level2());
    }
}