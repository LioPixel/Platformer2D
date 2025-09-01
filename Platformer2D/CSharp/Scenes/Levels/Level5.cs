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
        this.CreatePlatform(5, -1, 3); 
        this.CreateStair(11, -1, 3, StairType.Up); 
        this.CreatePlatform(17, -3, 5);
        this.CreatePortal(26, 0, 0, -2, Color.DarkRed);
        this.CreatePlatform(25, -4, 3);
        this.CreatePlatform(31, -5, 2);
        this.CreatePlatform(36, -6, 2);
    }

    protected override void OnLevelWon()
    {
        SceneManager.SetScene(new Level1());
    }

    public override void OnLevelReset()
    {
        SceneManager.SetScene(new Level5());
    }
}