﻿using Bliss.CSharp.Textures;
using Sparkle.CSharp.Scenes;

namespace Platformer2D.CSharp.Scenes.Levels;

public class Level1 : LevelScene
{
    
    public Level1() : base("Level 1") {}
    
    protected override void Init()
    {
        base.Init();
        this.Background = ContentRegistry.Background;
        
        this.CreatePlatform(0, 0, 4);
        this.CreatePlantSunFlower(2, -1);
        this.CreatePlatform(6, -1, 2);
        this.CreateRockWithGrass(7, -2);
        this.CreateStair(10, -1, 4, StairType.Up);
        this.CreateBushDead(11, -3);
        this.CreatePlatform(13, -4, 6);
        this.CreateOakLog(15, -5);
        this.CreateWinFlag(18, -5);
    }

    protected override void OnLevelWon()
    {
        SceneManager.SetScene(new Level2());
    }

    public override void OnLevelReset()
    {
        SceneManager.SetScene(new Level1());
    }
}

