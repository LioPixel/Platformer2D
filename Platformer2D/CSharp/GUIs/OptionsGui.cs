using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Interact;
using Bliss.CSharp.Interact.Keyboards;
using Bliss.CSharp.Textures;
using Bliss.CSharp.Transformations;
using Bliss.CSharp.Windowing;
using MiniAudioEx.Core.StandardAPI;
using Sparkle.CSharp;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.GUI;
using Sparkle.CSharp.GUI.Elements;
using Sparkle.CSharp.GUI.Elements.Data;
using Sparkle.CSharp.Overlays;
using Sparkle.CSharp.Scenes;
using Veldrid;

namespace Platformer2D.CSharp.GUIs;

public class OptionsGui : Gui
{
    
    public OptionsGui() : base("Options")
    {
    }

    protected override void Init()
    {
        base.Init();
        
        LabelData labelData = new LabelData(ContentRegistry.Fontoe, "Options", 18);
        this.AddElement("Title", new LabelElement(labelData, Anchor.TopCenter, new Vector2(0, 50), new Vector2(5, 5)));

        TextureButtonData backButtonData = new TextureButtonData(ContentRegistry.UiButton, hoverColor: Color.LightGray, resizeMode: ResizeMode.NineSlice, borderInsets: new BorderInsets(12));
        LabelData backButtonLabelData = new LabelData(ContentRegistry.Fontoe, "Back", 18, hoverColor: Color.White);
        
        this.AddElement("Options-Button", new TextureButtonElement(backButtonData, backButtonLabelData, Anchor.Center, new Vector2(-200, -120), size: new Vector2(100, 40), textOffset: new Vector2(0, 1), clickFunc: (element) => {
            if (SceneManager.ActiveScene != null)
            {
                GuiManager.SetGui(new PauseMenuGui());
            }
            else
            {
                GuiManager.SetGui(new MenuGui());
            }
            return true;
        }));
        
        // Toggle Vsync.
        ToggleData toggleDataVsync = new ToggleData(ContentRegistry.ToggleBackground, ContentRegistry.ToggleCheckmark, checkboxHoverColor: Color.LightGray, checkmarkHoverColor: Color.LightGray);
        LabelData toggleLabelDataVsync = new LabelData(ContentRegistry.Fontoe, "V-Sync", 18);
        
        this.AddElement("Toggle-Vsync", new ToggleElement(toggleDataVsync, toggleLabelDataVsync, Anchor.Center, new Vector2(-5, -120), 5, toggleState: GlobalGraphicsAssets.GraphicsDevice.SyncToVerticalBlank, clickFunc: (element) => {
            GlobalGraphicsAssets.GraphicsDevice.SyncToVerticalBlank = !GlobalGraphicsAssets.GraphicsDevice.SyncToVerticalBlank;
            ((PlatformerGame) Game.Instance!).OptionsConfig.SetValue("Vsync", GlobalGraphicsAssets.GraphicsDevice.SyncToVerticalBlank);
            return true;
        }));
        
        // Toggle Debug mode.
        ToggleData debugModeToggleData = new ToggleData(ContentRegistry.ToggleBackground, ContentRegistry.ToggleCheckmark, checkboxHoverColor: Color.LightGray, checkmarkHoverColor: Color.LightGray);
        LabelData debugModeToggleLabelData = new LabelData(ContentRegistry.Fontoe, "Debug Mode", 18);
        
        this.AddElement("Toggle-DebugMode", new ToggleElement(debugModeToggleData, debugModeToggleLabelData, Anchor.Center, new Vector2(19, -70), 5, toggleState: OverlayManager.GetOverlays().First(overlay => overlay.Name == "Debug").Enabled, clickFunc: (element) =>
        {
            bool condition = !OverlayManager.GetOverlays().First(overlay => overlay.Name == "Debug").Enabled;
            OverlayManager.GetOverlays().First(overlay => overlay.Name == "Debug").Enabled = condition;
            ((PlatformerGame) Game.Instance!).OptionsConfig.SetValue("DebugMode", condition);
            return true;
        }));
        
        // Toggle Sound.
        ToggleData toggleDataSound = new ToggleData(ContentRegistry.ToggleBackground, ContentRegistry.ToggleCheckmark, checkboxHoverColor: Color.LightGray, checkmarkHoverColor: Color.LightGray);
        LabelData toggleLabelDataSound = new LabelData(ContentRegistry.Fontoe, "Sounds", 18);
        
        this.AddElement("Toggle-Sounds", new ToggleElement(toggleDataSound, toggleLabelDataSound, Anchor.Center, new Vector2(0, -20), 5, toggleState: ((PlatformerGame) Game.Instance!).OptionsConfig.GetValue<bool>("Sounds"), clickFunc: (element) => {
            ((PlatformerGame) Game.Instance).OptionsConfig.SetValue("Sounds", !((PlatformerGame) Game.Instance!).OptionsConfig.GetValue<bool>("Sounds"));
            return true;
        }));
        
        LabelData masterVolumeLabelData = new LabelData(ContentRegistry.Fontoe, "Master Volume", 18);
        this.AddElement("Master-Volume", new LabelElement(masterVolumeLabelData, Anchor.Center, new Vector2(0, 100)));
        
        // Texture slider bar.
        TextureSlideBarData textureSlideBarData = new TextureSlideBarData(
            ContentRegistry.UiBar,
            null,
            ContentRegistry.UiSliderLowRes,
            barResizeMode: ResizeMode.NineSlice,
            filledBarResizeMode: ResizeMode.NineSlice,
            barBorderInsets: new BorderInsets(3),
            filledBarBorderInsets: new BorderInsets(3));
        
        this.AddElement("Texture-Slider-Bar", new TextureSlideBarElement(textureSlideBarData, Anchor.Center, new Vector2(0, 130), 0, 1, value: ((PlatformerGame) Game.Instance!).OptionsConfig.GetValue<float>("MasterVolume"), wholeNumbers: false, size: new Vector2(140, 8), scale: new Vector2(2, 2), clickFunc: (element) => {
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
        
        float scale = this.ScaleFactor;

        // Define base virtual size (e.g., half of 1280x720) and scale it
        Vector2 baseSize = new Vector2(550, 310);
        Vector2 scaledSize = baseSize * scale;

        // Snap window dimensions to scale grid to find the "scaled" center
        float screenWidth = MathF.Floor(GlobalGraphicsAssets.Window.GetWidth() / (float) scale) * scale;
        float screenHeight = MathF.Floor(GlobalGraphicsAssets.Window.GetHeight() / (float) scale) * scale;
        
        Vector2 pos = new Vector2(
            MathF.Floor((screenWidth / 2.0F - scaledSize.X / 2.0F) / scale) * scale,
            MathF.Floor((screenHeight / 2.0F - scaledSize.Y / 2.0F) / scale) * scale
        );
        
        // Draw gui rectangle
        context.PrimitiveBatch.Begin(context.CommandList, framebuffer.OutputDescription);
            
        // Overlay (Full screen)
        context.PrimitiveBatch.DrawFilledRectangle(new RectangleF(0, 0, GlobalGraphicsAssets.Window.GetWidth(), GlobalGraphicsAssets.Window.GetHeight()), color: new Color(128, 128, 128, 128));
            
        // Background Box
        context.PrimitiveBatch.DrawFilledRectangle(new RectangleF(pos.X, pos.Y, scaledSize.X, scaledSize.Y), color: new Color(128, 128, 128, 128));
            
        // Border
        context.PrimitiveBatch.DrawEmptyRectangle(new RectangleF(pos.X, pos.Y, scaledSize.X, scaledSize.Y), 4 * scale, color: new Color(64, 64, 64, 128));
            
        context.PrimitiveBatch.End();
        
        
        
        base.Draw(context, framebuffer);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (this.TryGetElement("Texture-Slider-Bar", out GuiElement? element))
            {
                TextureSlideBarElement slideBarElement = (TextureSlideBarElement) element!;
                ((PlatformerGame) Game.Instance!).OptionsConfig.SetValue("MasterVolume", slideBarElement.Value);
                AudioContext.MasterVolume = slideBarElement.Value;
            }
        }
        
        base.Dispose(disposing);
    }
}