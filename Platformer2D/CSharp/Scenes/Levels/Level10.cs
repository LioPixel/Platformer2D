using Sparkle.CSharp.Scenes;

namespace Platformer2D.CSharp.Scenes.Levels;

public class Level10 : LevelScene
{

    public Level10() : base("Level 10")
    {
        
    }
    
    protected override void Init()
    {
        base.Init();
        this.Background = ContentRegistry.Background9;
        this.CreatePlatform(0,0,1);
        this.CreatePlatform(5,0,1);
        this.CreateStair(9,-1,3,StairType.Up);
        this.CreatePlatform(15,-1,5);
        this.CreateMovingPlatform(23,-1,1,23,-10,1);
        this.CreatePlatform(27,-10,1);
        this.CreateMovingPlatform(31,-10,2,36,-10,1);
        this.CreatePlatform(40,-10,1);
        this.CreatePlatform(45,-10,1);
        this.CreatePlatform(50,-8,5);
        this.CreateStair(57,-9,5,StairType.Up);
        this.CreatePlatform(65,-13,3);
        this.CreateStair(71,-12,4,StairType.Down);
        this.CreatePlatform(79,-9,3); 
        this.CreateStair(84,-10,4,StairType.Up);
        this.CreateStair(91,-12,3,StairType.Down);
        this.CreateMovingPlatform(97,-10,2,104,-10,1);
        
    }

    protected override void OnLevelWon()
    {
        SceneManager.SetScene(new Level1());
    }

    public override void OnLevelReset()
    {
        SceneManager.SetScene(new Level10());
    }
}