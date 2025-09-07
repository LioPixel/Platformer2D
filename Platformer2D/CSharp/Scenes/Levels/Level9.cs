using Bliss.CSharp.Colors;
using Bliss.CSharp.Textures;
using Sparkle.CSharp.Scenes;

namespace Platformer2D.CSharp.Scenes.Levels;

public class Level9 : LevelScene
{

    public Level9() : base("Level 9")
    {
        
    }
    
    protected override void Init()
    {
        base.Init();
        this.Background = ContentRegistry.Background9;
        
        this.CreatePlatform(0,0,2);
        this.CreatePortal(4,-1,17,-5);
        this.CreatePlatform(17,-4,2);
        this.CreateMovingPlatform(22,-4,1,22,-12,1);
        this.CreatePlatform(24,-12,2);
        this.CreatePortal(21,-2,24,-13);
        this.CreatePlatform(30,-12,1);
        this.CreateStair(34,-13,3,StairType.Up);
        this.CreateMovingPlatform(40,-15,1,48,-15,1);
        this.CreatePlatform(50,-15,3);
        this.CreatePlatform(58,-13,1);
        this.CreatePlatform(63,-13,8);


    }

    protected override void OnLevelWon()
    {
        SceneManager.SetScene(new Level1());
    }

    public override void OnLevelReset()
    {
        SceneManager.SetScene(new Level9());
    }
}