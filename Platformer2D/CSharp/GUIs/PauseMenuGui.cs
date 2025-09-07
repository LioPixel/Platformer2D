using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Interact;
using Bliss.CSharp.Interact.Keyboards;
using Bliss.CSharp.Transformations;
using Platformer2D.CSharp.Scenes;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.GUI;
using Sparkle.CSharp.GUI.Elements;
using Sparkle.CSharp.GUI.Elements.Data;
using Sparkle.CSharp.Scenes;
using Veldrid;

namespace Platformer2D.CSharp.GUIs;

public class PauseMenuGui : Gui
{
    private bool _opendRightNow;
    
    public PauseMenuGui() : base("PauseMenu") { }

    protected override void Init()
    {
        base.Init();
        this._opendRightNow = true;
        
        LabelData labelData = new LabelData(ContentRegistry.Fontoe, "Pause Menu", 18, scale: new Vector2(5, 5));
        this.AddElement("Test-Label", new LabelElement(labelData, Anchor.TopCenter, new Vector2(0, 100)));
        
        // Button color.
        Color lightPurpleColor = new Color(147, 112, 219, 180);
        Color purpleColor = new Color(128, 0, 128, 180);
        Color darkPurpleColor = new Color(75, 0, 130, 180);
 
        // Menu button.
        RectangleButtonData menuButtonData = new RectangleButtonData(lightPurpleColor, lightPurpleColor, 5, darkPurpleColor, purpleColor);
        LabelData menuButtonLabelData = new LabelData(ContentRegistry.Fontoe, "Menu", 18, hoverColor: Color.White);
        
        this.AddElement("Menu-Button", new RectangleButtonElement(menuButtonData, menuButtonLabelData, Anchor.Center, Vector2.Zero, new Vector2(300, 50), rotation: 0, clickFunc: () => {
            SceneManager.SetScene(null);
            GuiManager.SetGui(new MenuGui());
            return true;
        }));
        
        // Options button.
        RectangleButtonData optionsButtonData = new RectangleButtonData(lightPurpleColor, lightPurpleColor, 5, darkPurpleColor, purpleColor);
        LabelData optionsButtonLabelData = new LabelData(ContentRegistry.Fontoe, "Options", 18, hoverColor: Color.White);
        
        this.AddElement("Options-Button", new RectangleButtonElement(optionsButtonData, optionsButtonLabelData, Anchor.Center, new Vector2(0, 60), new Vector2(300, 50), rotation: 0, clickFunc: () => {
            GuiManager.SetGui(new OptionsGui());
            return true;
        }));
        
        // Reset button.
        RectangleButtonData resetButtonData = new RectangleButtonData(lightPurpleColor, lightPurpleColor, 5, darkPurpleColor, purpleColor);
        LabelData resetButtonLabelData = new LabelData(ContentRegistry.Fontoe, "Reset", 18, hoverColor: Color.White);
        
        this.AddElement("Reset-Button", new RectangleButtonElement(resetButtonData, resetButtonLabelData, Anchor.Center, new Vector2(0, 120), new Vector2(300, 50), rotation: 0, clickFunc: () => {
            if (SceneManager.ActiveScene is LevelScene level)
            {
                level.OnLevelReset();
            }
            
            GuiManager.SetGui(null);
            return true;
        }));
    }

    protected override void Update(double delta)
    {
        base.Update(delta);

        if (!this._opendRightNow)
        {
            if (Input.IsKeyPressed(KeyboardKey.Escape))
            {
                if (SceneManager.ActiveScene == null)
                {
                    GuiManager.SetGui(new MenuGui());
                }
                else
                {
                    GuiManager.SetGui(null);
                }
            }
        }

        this._opendRightNow = false;
    }
    protected override void Draw(GraphicsContext context, Framebuffer framebuffer)
    {
        context.PrimitiveBatch.Begin(context.CommandList, framebuffer.OutputDescription);
        context.PrimitiveBatch.DrawFilledRectangle(new RectangleF(0, 0, GlobalGraphicsAssets.Window.GetWidth(), GlobalGraphicsAssets.Window.GetHeight()), color: new Color(128, 128, 128, 128));
        context.PrimitiveBatch.End();
        
        base.Draw(context, framebuffer);
    }
}