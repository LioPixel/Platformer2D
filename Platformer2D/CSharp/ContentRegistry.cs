using Bliss.CSharp.Fonts;
using Bliss.CSharp.Textures;
using Sparkle.CSharp.Content;
using Sparkle.CSharp.Content.Types;
using Sparkle.CSharp.Registries;

namespace Platformer2D.CSharp;

public class ContentRegistry : Registry {
    
    public static Texture2D Sprite { get; private set; }
    public static Texture2D OakLog { get; private set; }
    public static Texture2D PlantFlowerRed { get; private set; }
    public static Texture2D RockGrass { get; private set; }
    public static Texture2D BushDead { get; private set; }
    public static Texture2D PlantSunFlower { get; private set; }
    public static Texture2D Bush { get; private set; }
    public static Texture2D PlantFlower { get; private set; }
    public static Texture2D TreeBigDead { get; private set; }
    public static Texture2D TreeBig { get; private set; }
    public static Texture2D WinFlag { get; private set; }
    public static Texture2D MenuBackground { get; private set; }
    public static Texture2D Player { get; private set; } 
    public static Font Fontoe { get; private set; }

    protected override void Load(ContentManager content)
    {
        base.Load(content);

        Sprite = content.Load(new TextureContent("content/sprite.png"));
        OakLog = content.Load(new TextureContent("content/oak_log.png"));
        PlantFlowerRed = content.Load(new TextureContent("content/plant_flower_red.png"));
        RockGrass = content.Load(new TextureContent("content/rock_grass.png"));
        BushDead = content.Load(new TextureContent("content/bush_dead.png"));
        PlantSunFlower = content.Load(new TextureContent("content/plant_sun_flower.png"));
        Bush = content.Load(new TextureContent("content/bush.png"));
        PlantFlower = content.Load(new TextureContent("content/plant_flower.png"));
        TreeBig = content.Load(new TextureContent("content/tree_big.png"));
        TreeBigDead = content.Load(new TextureContent("content/tree_big_dead.png"));
        WinFlag = content.Load(new TextureContent("content/win_flag.png"));
        MenuBackground = content.Load(new TextureContent("content/menu_background.png"));
        Player = content.Load(new TextureContent("content/player.png")); 
        Fontoe = content.Load(new FontContent("content/fontoe.ttf"));
    }
}