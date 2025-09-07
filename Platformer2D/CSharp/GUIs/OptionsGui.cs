using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Interact;
using Bliss.CSharp.Interact.Keyboards;
using Bliss.CSharp.Textures;
using Bliss.CSharp.Transformations;
using Bliss.CSharp.Windowing;
using Sparkle.CSharp;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.GUI;
using Sparkle.CSharp.GUI.Elements;
using Sparkle.CSharp.GUI.Elements.Data;
using Sparkle.CSharp.IO.Configs.Json;
using Sparkle.CSharp.Overlays;
using Sparkle.CSharp.Scenes;
using Veldrid;

namespace Platformer2D.CSharp.GUIs;

public class OptionsGui : Gui
{
    
    public OptionsGui() : base("Options")
    {
        JsonConfigBuilder jsonConfigBuilder = new JsonConfigBuilder("configs", "options");
        jsonConfigBuilder.Add("Vsync", false);
    }

    protected override void Init()
    {
        base.Init();
        
        LabelData labelData = new LabelData(ContentRegistry.Fontoe, "Options", 18, scale: new Vector2(5, 5));
        this.AddElement("Title", new LabelElement(labelData, Anchor.TopCenter, new Vector2(0, 50)));
        
        // Toggle Vsync.
        ToggleData toggleDataVsync = new ToggleData(ContentRegistry.ToggleBackground, ContentRegistry.ToggleCheckmark, backgroundHoverColor: Color.LightGray, checkmarkHoverColor: Color.LightGray);
        LabelData toggleLabelDataVsync = new LabelData(ContentRegistry.Fontoe, "V-Sync", 18);
        
        this.AddElement("Toggle-Vsync", new ToggleElement(toggleDataVsync, toggleLabelDataVsync, Anchor.Center, new Vector2(0, -120), 5, toggleState: GlobalGraphicsAssets.GraphicsDevice.SyncToVerticalBlank, clickFunc: () => {
            GlobalGraphicsAssets.GraphicsDevice.SyncToVerticalBlank = !GlobalGraphicsAssets.GraphicsDevice.SyncToVerticalBlank;
            ((PlatformerGame) Game.Instance!).OptionsConfig.SetValue("Vsync", GlobalGraphicsAssets.GraphicsDevice.SyncToVerticalBlank);
            return true;
        }));
        
        // Toggle Debug mode.
        ToggleData debugModeToggleData = new ToggleData(ContentRegistry.ToggleBackground, ContentRegistry.ToggleCheckmark, backgroundHoverColor: Color.LightGray, checkmarkHoverColor: Color.LightGray);
        LabelData debugModeToggleLabelData = new LabelData(ContentRegistry.Fontoe, "Debug Mode", 18);
        
        this.AddElement("Toggle-DebugMode", new ToggleElement(debugModeToggleData, debugModeToggleLabelData, Anchor.Center, new Vector2(0, -70), 5, toggleState: OverlayManager.GetOverlays().First(overlay => overlay.Name == "Debug").Enabled, clickFunc: () =>
        {
            bool condition = !OverlayManager.GetOverlays().First(overlay => overlay.Name == "Debug").Enabled;
            OverlayManager.GetOverlays().First(overlay => overlay.Name == "Debug").Enabled = condition;
            ((PlatformerGame) Game.Instance!).OptionsConfig.SetValue("DebugMode", condition);
            return true;
        }));
        
        // Toggle Sound.
        ToggleData toggleDataSound = new ToggleData(ContentRegistry.ToggleBackground, ContentRegistry.ToggleCheckmark, backgroundHoverColor: Color.LightGray, checkmarkHoverColor: Color.LightGray);
        LabelData toggleLabelDataSound = new LabelData(ContentRegistry.Fontoe, "Sounds", 18);
        
        this.AddElement("Toggle-Sounds", new ToggleElement(toggleDataSound, toggleLabelDataSound, Anchor.Center, new Vector2(0, -20), 5, toggleState: ((PlatformerGame) Game.Instance!).OptionsConfig.GetValue<bool>("Sounds"), clickFunc: () => {
            ((PlatformerGame) Game.Instance).OptionsConfig.SetValue("Sounds", !((PlatformerGame) Game.Instance!).OptionsConfig.GetValue<bool>("Sounds"));
            return true;
        }));
    }

    protected override void Update(double delta)
    {
        base.Update(delta);

        if (Input.IsKeyPressed(KeyboardKey.Escape))
        {

            if (SceneManager.ActiveScene == null)
            {
                GuiManager.SetGui(new MenuGui());
            }
            else
            {
                GuiManager.SetGui(new PauseMenuGui());
            }
        }
    }

    protected override void Draw(GraphicsContext context, Framebuffer framebuffer)
    {
        if (SceneManager.ActiveScene == null)
        {
            IWindow window = GlobalGraphicsAssets.Window;
        
            // Background
            Texture2D backgroundTexture = ContentRegistry.Background2;
            Vector2 backgroundSize = new Vector2((float) window.GetWidth() / backgroundTexture.Width, (float) window.GetHeight() / backgroundTexture.Height);
        
            context.SpriteBatch.Begin(context.CommandList, framebuffer.OutputDescription);
            context.SpriteBatch.DrawTexture(backgroundTexture, Vector2.Zero, scale: backgroundSize);
            context.SpriteBatch.End();
        }
        
        Vector2 size = new Vector2(GlobalGraphicsAssets.Window.GetWidth() / 2.0F, GlobalGraphicsAssets.Window.GetHeight() / 2.0F);
        Vector2 pos = new Vector2( GlobalGraphicsAssets.Window.GetWidth() / 2.0F - size.X / 2.0F, GlobalGraphicsAssets.Window.GetHeight() / 2.0F - size.Y / 2.0F);
        
        context.PrimitiveBatch.Begin(context.CommandList, framebuffer.OutputDescription);
        context.PrimitiveBatch.DrawFilledRectangle(new RectangleF(pos.X, pos.Y, size.X, size.Y), color: new Color(128, 128, 128, 128));
        context.PrimitiveBatch.DrawFilledRectangle(new RectangleF(0, 0, GlobalGraphicsAssets.Window.GetWidth(), GlobalGraphicsAssets.Window.GetHeight()), color: new Color(128, 128, 128, 128));
        context.PrimitiveBatch.DrawEmptyRectangle(new RectangleF(pos.X, pos.Y, size.X, size.Y), 4, color: new Color(64, 64, 64, 128));
        context.PrimitiveBatch.End();
        
        base.Draw(context, framebuffer);
    }
}