using System.Numerics;
using Bliss.CSharp.Camera.Dim2;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Textures;
using Bliss.CSharp.Transformations;
using Bliss.CSharp.Windowing;
using Box2D;
using Platformer2D.CSharp.Entities;
using Sparkle.CSharp.Entities;
using Sparkle.CSharp.Entities.Components;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.Physics.Dim2;
using Sparkle.CSharp.Physics.Dim2.Def;
using Sparkle.CSharp.Physics.Dim2.Shapes;
using Sparkle.CSharp.Scenes;
using Veldrid;
using Transform = Bliss.CSharp.Transformations.Transform;

namespace Platformer2D.CSharp.Scenes;

public abstract class LevelScene : Scene
{
    
    public Texture2D? Background;
    public bool WonLevel;
    
    protected LevelScene(string name) : base(name, SceneType.Scene2D, new Simulation2D(new PhysicsSettings2D()
    {
        WorldDef = new WorldDef() 
        {
            Gravity = new Vector2(0, 9.81F), 
        }
    })) { }

    protected override void Init()
    {
        base.Init();
        
        // CAMERA
        Rectangle size = new Rectangle(0, 0, GlobalGraphicsAssets.Window.GetWidth(), GlobalGraphicsAssets.Window.GetHeight());
        Camera2D camera2D = new Camera2D(Vector2.Zero, Vector2.Zero, size, CameraFollowMode.FollowTargetSmooth, zoom: 7);
        this.AddEntity(camera2D);

        // PLAYER
        Player player = new Player(new Transform() { Translation = new Vector3(0, -16 * 2, 0) });
        this.AddEntity(player);
    }
    
    protected override void Update(double delta)
    {
        base.Update(delta);
        
        Camera2D? cam2D = SceneManager.ActiveCam2D;

        if (cam2D == null)
        {
            return;
        }

        Entity? player = this.GetEntity(2);

        if (player != null)
        {
            Vector3 playerPos = player.Transform.Translation;
            cam2D.Target = new Vector2(playerPos.X, playerPos.Y);   
        }
        
        // Level won.
        if (this.WonLevel)
        {
            this.OnLevelWon();
        }
    }

    protected override void Draw(GraphicsContext context, Framebuffer framebuffer)
    {
        // Background
        if (this.Background != null)
        {
            IWindow window = GlobalGraphicsAssets.Window;
            Vector2 backgroundSize = new Vector2((float) window.GetWidth() / this.Background.Width, (float) window.GetHeight() / this.Background.Height);
        
            context.SpriteBatch.Begin(context.CommandList, framebuffer.OutputDescription);
            context.SpriteBatch.DrawTexture(this.Background, Vector2.Zero, scale: backgroundSize);
            context.SpriteBatch.End();
        }
        
        base.Draw(context, framebuffer);
    }

    protected abstract void OnLevelWon();
    
    public abstract void OnLevelReset();

    protected void CreatePlatform(int blockPosX, int blockPosY, int length)
    {
        for (int i = 0; i < length; i++)
        {
            Entity element = new Entity(new Transform() { Translation = new Vector3(blockPosX * 16 + (16 * i), blockPosY * 16, 0) });
            element.AddComponent(new Sprite(ContentRegistry.Sprite, Vector2.Zero));
            element.AddComponent(new RigidBody2D(new BodyDefinition() { Type = BodyType.Static }, new PolygonShape2D(Polygon.MakeBox(8, 8), new ShapeDef()
            {
                EnableContactEvents = true,
                EnableSensorEvents = true
            })));
            this.AddEntity(element);
        }
    }

    protected void CreateStair(int blockPosX, int blockPosY, int length, StairType stairType)
    {
        for (int i = 0; i < length; i++)
        {
            
            Vector2 position;

            if (stairType == StairType.Up)
            {
                position = new Vector2(blockPosX * 16 + (16 * i), blockPosY * 16 - (16 * i));
            }
            else
            {
                position = new Vector2(blockPosX * 16 + (16 * i), blockPosY * 16 + (16 * i));
            }
            
            Entity element = new Entity(new Transform() { Translation = new Vector3(position, 0) });
            element.AddComponent(new Sprite(ContentRegistry.Sprite, Vector2.Zero));
            element.AddComponent(new RigidBody2D(new BodyDefinition() { Type = BodyType.Static }, new PolygonShape2D(Polygon.MakeBox(8, 8), new ShapeDef()
            {
                EnableContactEvents = true,
                EnableSensorEvents = true
            })));
            this.AddEntity(element);
        }
    }

    protected void CreateWinFlag(int blockPosX, int blockPosY)
    {
        WinFlag winFlag = new WinFlag(new Transform() { Translation = new Vector3(blockPosX * 16 + 7, (blockPosY * 16) - 16, 0) });
        this.AddEntity(winFlag);
    }

    protected void CreateTree(int blockPosX, int blockPosY)
    {
        Entity entity = new Entity(new Transform() { Translation = new Vector3(blockPosX * 16, blockPosY * 16 - 20.5F, 0) });
        entity.AddComponent(new Sprite(ContentRegistry.TreeBig, Vector2.Zero, layerDepth: 0.4F));
        this.AddEntity(entity);
    }

    protected void CreateTreeDead(int blockPosX, int blockPosY)
    {
        Entity entity = new Entity(new Transform() { Translation = new Vector3(blockPosX * 16, blockPosY * 16 - 11, 0) });
        entity.AddComponent(new Sprite(ContentRegistry.TreeBigDead, Vector2.Zero, layerDepth: 0.4F));
        this.AddEntity(entity);
    }
    
    protected void CreateFlowerOrange(int blockPosX, int blockPosY)
    {
        Entity entity = new Entity(new Transform() { Translation = new Vector3(blockPosX * 16, blockPosY * 16 - 8, 0) });
        entity.AddComponent(new Sprite(ContentRegistry.PlantFlower, Vector2.Zero, layerDepth: 0.4F));
        this.AddEntity(entity);
    }
    
    protected void CreateBush(int blockPosX, int blockPosY)
    {
        Entity entity = new Entity(new Transform() { Translation = new Vector3(blockPosX * 16, blockPosY * 16 + 1, 0) });
        entity.AddComponent(new Sprite(ContentRegistry.Bush, Vector2.Zero, layerDepth: 0.4F));
        this.AddEntity(entity);
    }
    
    protected void CreatePlantSunFlower(int blockPosX, int blockPosY)
    {
        Entity entity = new Entity(new Transform() { Translation = new Vector3(blockPosX * 16, blockPosY * 16 - 8, 0) });
        entity.AddComponent(new Sprite(ContentRegistry.PlantSunFlower, Vector2.Zero, layerDepth: 0.4F));
        this.AddEntity(entity);
    }
    
    protected void CreateBushDead(int blockPosX, int blockPosY)
    {
        Entity entity = new Entity(new Transform() { Translation = new Vector3(blockPosX * 16, blockPosY * 16, 0) });
        entity.AddComponent(new Sprite(ContentRegistry.BushDead, Vector2.Zero, layerDepth: 0.4F));
        this.AddEntity(entity);
    }
    
    protected void CreateRockWithGrass(int blockPosX, int blockPosY)
    {
        Entity entity = new Entity(new Transform() { Translation = new Vector3(blockPosX * 16, blockPosY * 16, 0) });
        entity.AddComponent(new Sprite(ContentRegistry.RockGrass, Vector2.Zero, layerDepth: 0.4F));
        this.AddEntity(entity);
    }
    
    protected void CreatePlantFlowerRed(int blockPosX, int blockPosY)
    {
        Entity entity = new Entity(new Transform() { Translation = new Vector3(blockPosX * 16, blockPosY * 16, 0) });
        entity.AddComponent(new Sprite(ContentRegistry.PlantFlowerRed, Vector2.Zero, layerDepth: 0.4F));
        this.AddEntity(entity);
    }
    
    protected void CreateOakLog(int blockPosX, int blockPosY)
    {
        Entity entity = new Entity(new Transform() { Translation = new Vector3(blockPosX * 16, blockPosY * 16, 0) });
        entity.AddComponent(new Sprite(ContentRegistry.OakLog, Vector2.Zero, layerDepth: 0.4F));
        this.AddEntity(entity);
    }

    protected void CreatePortal(int blockPosX, int blockPosY, int teleportPosX, int teleportPosY, Color? color = null)
    {
        
        Portal portal = new Portal(new Transform() { Translation = new Vector3(blockPosX * 16, blockPosY * 16, 0) }, new Vector2(teleportPosX * 16, teleportPosY * 16), color);
        this.AddEntity(portal);
    }
    
    protected enum StairType
    {
        Up,
        Down
    }
}
