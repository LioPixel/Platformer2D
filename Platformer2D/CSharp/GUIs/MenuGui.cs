using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Textures;
using Bliss.CSharp.Windowing;
using Platformer2D.CSharp.Scenes.Levels;
using Sparkle.CSharp;
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
        
        LabelData fullscreen = new LabelData(ContentRegistry.Fontoe, "please go to full screen", 18, color: Color.White, scale: new Vector2(1.5F, 1.5F));
        this.AddElement("fullscreen", new LabelElement(fullscreen, Anchor.BottomRight, new Vector2(0, 0))); 
        
        LabelData labelData = new LabelData(ContentRegistry.Fontoe, "Platformer2D", 18, scale: new Vector2(5, 5));
        this.AddElement("Test-Label", new LabelElement(labelData, Anchor.TopCenter, new Vector2(0, 100)));

        string controlText = "Controls:\nA/Left: LEFT\nD/Right: RIGHT\nSpace: JUMP";
        LabelData controlLabelData = new LabelData(ContentRegistry.Fontoe, controlText, 18, color: Color.White);
        this.AddElement("Control-Label", new LabelElement(controlLabelData, Anchor.TopLeft, new Vector2(10, 10)));
        
        string creditsText = "Credits:\nLio: Developer/Designer\nMrScautHD: Developer";
        LabelData creditsLabelData = new LabelData(ContentRegistry.Fontoe, creditsText, 18, color: Color.White);
        this.AddElement("Credits-Label", new LabelElement(creditsLabelData, Anchor.BottomLeft, new Vector2(10, -10)));
        
        // Button color.
        Color lightPurpleColor = new Color(147, 112, 219, 180);
        Color purpleColor = new Color(128, 0, 128, 180);
        Color darkPurpleColor = new Color(75, 0, 130, 180);
        
        // Rectangle button.
        RectangleButtonData rectangleButtonData = new RectangleButtonData(lightPurpleColor, lightPurpleColor, 5, darkPurpleColor, purpleColor);
        LabelData rectangleButtonLabelData = new LabelData(ContentRegistry.Fontoe, "Play", 18, hoverColor: Color.White);
        
        this.AddElement("Test-Rectangle-Button", new RectangleButtonElement(rectangleButtonData, rectangleButtonLabelData, Anchor.Center, Vector2.Zero, new Vector2(300, 50), rotation: 0, clickFunc: () => {
            SceneManager.SetScene(new Level1());
            GuiManager.SetGui(null);
            return true;
        }));
        
        // Options button.
        RectangleButtonData optionsButtonData = new RectangleButtonData(lightPurpleColor, lightPurpleColor, 5, darkPurpleColor, purpleColor);
        LabelData optionsButtonLabelData = new LabelData(ContentRegistry.Fontoe, "Options", 18, hoverColor: Color.White);
        
        this.AddElement("Options-Button", new RectangleButtonElement(optionsButtonData, optionsButtonLabelData, Anchor.Center, new Vector2(0, 60), new Vector2(300, 50), rotation: 0, clickFunc: () => {
            GuiManager.SetGui(new OptionsGui());
            return true;
        }));
        
                
        // Exit button.
        RectangleButtonData exitButtonData = new RectangleButtonData(lightPurpleColor, lightPurpleColor, 5, darkPurpleColor, purpleColor);
        LabelData exitButtonLabelData = new LabelData(ContentRegistry.Fontoe, "Exit", 18, hoverColor: Color.White);
        
        this.AddElement("Exit-Button", new RectangleButtonElement(exitButtonData, exitButtonLabelData, Anchor.Center, new Vector2(0, 120), new Vector2(300, 50), rotation: 0, clickFunc: () =>
        {
            ((PlatformerGame) Game.Instance!).ShouldClose = true;
            return true;
        }));
    }

    protected override void Draw(GraphicsContext context, Framebuffer framebuffer)
    {
        IWindow window = GlobalGraphicsAssets.Window;
        
        // Background
        Texture2D backgroundTexture = ContentRegistry.Background2;
        Vector2 backgroundSize = new Vector2((float) window.GetWidth() / backgroundTexture.Width, (float) window.GetHeight() / backgroundTexture.Height);
        
        context.SpriteBatch.Begin(context.CommandList, framebuffer.OutputDescription);
        context.SpriteBatch.DrawTexture(backgroundTexture, Vector2.Zero, scale: backgroundSize);
        context.SpriteBatch.End();
        
        base.Draw(context, framebuffer);
    }
}