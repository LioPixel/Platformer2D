using Sparkle.CSharp.Scenes;

namespace Platformer2D.CSharp.Scenes.Levels;

public class Level4 : LevelScene
{
    public Level4() : base("Level 4") { }
    
    protected override void Init()
    {
        base.Init();


        this.Background = ContentRegistry.Background7;
        this.CreatePlatform(0, 0, 1);
        this.CreateFlowerOrange(5, -2);
        this.CreatePlatform(3, -1, 3);
        this.CreateRockWithGrass(11, -2);
        this.CreateTreeDead(17, -1);
        this.CreateBush(23, -2);
        this.CreateStair(9, 1, 4, StairType.Up);
        this.CreatePlatform(15, 0, 4);
        this.CreatePortal(17, -4, 31, 2);
        this.CreatePlatform(21, -1, 3);
        this.CreateStair(28, 0, 4, StairType.Down);
        this.CreateBushDead(29, 0);
        this.CreatePlatform(34, 3, 1);
        this.CreatePortal(34, 2, 39, -1);
        this.CreatePlatform(39, 1, 4);
        this.CreatePlatform(46, 0, 2);
        this.CreateRockWithGrass(47, -1); 
        this.CreatePortal(51, -2, 65, 2 );
        this.CreatePlatform(65, 3, 6); 
        this.CreateTreeDead(66, 2);
        this.CreateWinFlag(69, 2);
    }

    protected override void OnLevelWon()
    {
        if (NetworkManager.Client == null || !NetworkManager.Client.IsConnected)
        {
            SceneManager.SetScene(new Level5());
        }    }
    
    public override void OnLevelReset()
    {
        SceneManager.SetScene(new Level4());
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