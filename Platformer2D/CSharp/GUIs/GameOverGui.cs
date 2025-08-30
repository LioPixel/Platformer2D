using System.Numerics;
using System.Runtime.CompilerServices;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Transformations;
using Platformer2D.CSharp.Scenes;
using Platformer2D.CSharp.Scenes.Levels;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.GUI;
using Sparkle.CSharp.GUI.Elements;
using Sparkle.CSharp.GUI.Elements.Data;
using Sparkle.CSharp.Scenes;
using Veldrid;

namespace Platformer2D.CSharp.GUIs;

public class GameOverGui : Gui
{
    public GameOverGui() : base("GameOver", null) { }

    protected override void Init()
    {
        base.Init();
        
        LabelData labelData = new LabelData(ContentRegistry.Fontoe, "GAME OVER!", 18, scale: new Vector2(5, 5));
        this.AddElement("Test-Label", new LabelElement(labelData, Anchor.TopCenter, new Vector2(0, 100)));
 
        // Menu button.
        RectangleButtonData menuButtonData = new RectangleButtonData(Color.Orange, Color.LightOrange, 5, Color.DarkOrange, Color.Orange);
        LabelData menuButtonLabelData = new LabelData(ContentRegistry.Fontoe, "Menu", 18, hoverColor: Color.White);
        
        this.AddElement("Menu-Button", new RectangleButtonElement(menuButtonData, menuButtonLabelData, Anchor.Center, Vector2.Zero, new Vector2(300, 50), rotation: 0, clickFunc: () => {
            SceneManager.SetScene(null);
            GuiManager.SetGui(new MenuGui());
            return true;
        }));
        
        // Reset button.
        RectangleButtonData resetButtonData = new RectangleButtonData(Color.Orange, Color.LightOrange, 5, Color.DarkOrange, Color.Orange);
        LabelData resetButtonLabelData = new LabelData(ContentRegistry.Fontoe, "Reset", 18, hoverColor: Color.White);
        
        this.AddElement("Reset-Button", new RectangleButtonElement(resetButtonData, resetButtonLabelData, Anchor.Center, new Vector2(0, 60), new Vector2(300, 50), rotation: 0, clickFunc: () => {
            SceneManager.SetScene(new Level1("Level 1"));
            GuiManager.SetGui(null);
            return true;
        }));
    }

    protected override void Draw(GraphicsContext context, Framebuffer framebuffer)
    {
        context.PrimitiveBatch.Begin(context.CommandList, framebuffer.OutputDescription);
        context.PrimitiveBatch.DrawFilledRectangle(new RectangleF(0, 0, GlobalGraphicsAssets.Window.GetWidth(), GlobalGraphicsAssets.Window.GetHeight()), color: new Color(128, 128, 128, 128));
        context.PrimitiveBatch.End();
        
        base.Draw(context, framebuffer);
    }
}