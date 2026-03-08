using System.Numerics;
using Bliss.CSharp.Transformations;
using Pixelis.CSharp.Entities;
using Pixelis.CSharp.GUIs.Loading;
using Sparkle.CSharp.Scenes;
using Sparkle.CSharp.Utils.Async;

namespace Pixelis.CSharp.Scenes.Levels;

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
            AsyncOperation operation10 = SceneManager.LoadSceneAsync(new Level5(), new ProgressBarLoadingGui("Loading"));
            operation10.Completed += success =>
            {
                Player player = new Player(new Transform() { Translation = new Vector3(0, -16 * 2, 0) });
                SceneManager.ActiveScene?.AddEntity(player);
            };
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