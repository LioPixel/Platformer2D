﻿using Sparkle.CSharp.Scenes;

namespace Platformer2D.CSharp.Scenes.Levels;

public class Level3 : LevelScene
{
    public Level3(string name) : base(name) { }
    
    protected override void Init()
    {
        base.Init();
        
        this.Background = ContentRegistry.Background4;
        this.CreatePlatform(0, 0, 2);
        this.CreateFlowerOrange(1, -1);
        this.CreatePlatform(3, 1, 3);
        this.CreateTreeDead(4, 0);
        this.CreateStair(9, 1, 5, StairType.Up);
        this.CreatePlantSunFlower(11, -2);
        this.CreatePlatform(16, -4, 3);
        this.CreateTree(17, -5);
        this.CreateStair(23, -4, 4, StairType.Down);
        this.CreatePlatform(33, 0, 1);
        this.CreateStair(36, -1, 8, StairType.Up);
        this.CreateRockWithGrass(26, -2);
        this.CreatePlatform(49, -8, 1);
        this.CreateWinFlag(49, -9);
    }

    protected override void OnLevelWon()
    {
        SceneManager.SetScene(new Level4("Level4"));
    }
    
    public override void OnLevelReset()
    {
        SceneManager.SetScene(new Level4("Level4"));
    }
}