using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Textures;
using Bliss.CSharp.Windowing;
using Platformer2D.CSharp.Scenes;
using Platformer2D.CSharp.Scenes.Levels;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.GUI;
using Sparkle.CSharp.GUI.Elements;
using Sparkle.CSharp.GUI.Elements.Data;
using Sparkle.CSharp.Scenes;
using Veldrid;

namespace Platformer2D.CSharp.GUIs;

public class MenuGui : Gui
{
    public MenuGui() : base("Menu", null) { }

    protected override void Init()
    {
        base.Init();
        
        LabelData labelData = new LabelData(ContentRegistry.Fontoe, "Platformer2D", 18, scale: new Vector2(5, 5));
        this.AddElement("Test-Label", new LabelElement(labelData, Anchor.TopCenter, new Vector2(0, 100)));

        string controlText = "Controls:\nA: LEFT\nD: RIGHT\nSpace: JUMP";
        LabelData controlLabelData = new LabelData(ContentRegistry.Fontoe, controlText, 18, color: Color.Gray);
        this.AddElement("Control-Label", new LabelElement(controlLabelData, Anchor.TopLeft, new Vector2(10, 10)));
        
        string creditsText = "Credits:\nLio: Developer/Designer\nMrScautHD: Developer";
        LabelData creditsLabelData = new LabelData(ContentRegistry.Fontoe, creditsText, 18, color: Color.Gray);
        this.AddElement("Credits-Label", new LabelElement(creditsLabelData, Anchor.BottomLeft, new Vector2(10, -10)));
        
        // Rectangle button.
        RectangleButtonData rectangleButtonData = new RectangleButtonData(Color.Orange, Color.LightOrange, 5, Color.DarkOrange, Color.Orange);
        LabelData rectangleButtonLabelData = new LabelData(ContentRegistry.Fontoe, "Play", 18, hoverColor: Color.White);
        
        this.AddElement("Test-Rectangle-Button", new RectangleButtonElement(rectangleButtonData, rectangleButtonLabelData, Anchor.Center, Vector2.Zero, new Vector2(300, 50), rotation: 0, clickFunc: () => {
            SceneManager.SetScene(new Level1("Level 1"));
            GuiManager.SetGui(null);
            return true;
        }));
    }

    protected override void Draw(GraphicsContext context, Framebuffer framebuffer)
    {
        IWindow window = GlobalGraphicsAssets.Window;
        
        // Background
        Texture2D backgroundTexture = ContentRegistry.MenuBackground;
        Vector2 backgroundSize = new Vector2((float) window.GetWidth() / backgroundTexture.Width, (float) window.GetHeight() / backgroundTexture.Height);
        
        context.SpriteBatch.Begin(context.CommandList, framebuffer.OutputDescription);
        context.SpriteBatch.DrawTexture(ContentRegistry.MenuBackground, Vector2.Zero, scale: backgroundSize);
        context.SpriteBatch.End();
        
        base.Draw(context, framebuffer);
    }
}