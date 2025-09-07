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
        
        this.CreatePlatform(0,0,1);

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