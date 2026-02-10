using Bliss.CSharp.Colors;
using Bliss.CSharp.Textures;
using Platformer2D.CSharp.GUIs;
using Sparkle.CSharp.GUI;
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
        this.Background = ContentRegistry.Background7;
        this.CreatePlatform(0,0,1);
        this.CreateMovingPlatform(3,0,2,10,0,1);
        this.CreatePlatform(14,0,2);
        this.CreateMovingPlatform(19,0,1,19,-10,1);
        this.CreatePlatform(22,-10,3);
        this.CreateStair(28,-11,2,StairType.Up);
        this.CreatePortal(31,-13,22,-12, Color.DarkRed);
        this.CreatePlatform(26,-6,1);
        this.CreatePlatform(30,-5,3);
        this.CreatePlatform(37,-5,1);
        this.CreateStair(41,-6,5,StairType.Up);
        this.CreatePlatform(49,-10,1);
        this.CreatePortal(53,-11,45,-13);
        this.CreatePortal(53,-7,47,3);
        this.CreatePlatform(47,5,1);
        this.CreateWinFlag(51,4);
        this.CreatePlantSunFlower(15,-1);
        this.CreateRockWithGrass(24,-11);
        this.CreateBush(29,-13);
        this.CreateFlowerOrange(43,-9);
    }

    protected override void OnLevelWon()
    {
        if (NetworkManager.Client == null || !NetworkManager.Client.IsConnected)
        {
            SceneManager.SetScene(new Level10());
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