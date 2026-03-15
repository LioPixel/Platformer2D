using System.Net.Mime;
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

namespace Pixelis.CSharp.GUIs;

public class ExitGui : Gui
{
    
    public ExitGui() : base("Exit")
    {
    }

    protected override void Init()
    {
        base.Init();
        
        LabelData labelData = new LabelData(ContentRegistry.Fontoe, "Exit", 18);
        this.AddElement("Title", new LabelElement(labelData, Anchor.TopCenter, new Vector2(0, 50), new Vector2(5, 5)));

        LabelData textData = new LabelData(ContentRegistry.Fontoe, "Do you want to exit the game?", 18);
        this.AddElement("exit", new LabelElement(textData, Anchor.Center, new Vector2(0, -80), scale: new Vector2(1.5F, 1.5F)));
        
        TextureButtonData yesButtonData = new TextureButtonData(ContentRegistry.UiButton, hoverColor: Color.LightGray, resizeMode: ResizeMode.NineSlice, borderInsets: new BorderInsets(12));
        LabelData yesButtonLabelData = new LabelData(ContentRegistry.Fontoe, "Yes", 18, hoverColor: Color.White);
        
        this.AddElement("Yes-Button", new TextureButtonElement(yesButtonData, yesButtonLabelData, Anchor.Center, new Vector2(-90, 0), size: new Vector2(100, 40), textOffset: new Vector2(0, 1), clickFunc: (element) =>
        {
            Game.Instance?.ShouldClose = true;
            return true;
        }));
        
        TextureButtonData noButtonData = new TextureButtonData(ContentRegistry.UiButton, hoverColor: Color.LightGray, resizeMode: ResizeMode.NineSlice, borderInsets: new BorderInsets(12));
        LabelData noButtonLabelData = new LabelData(ContentRegistry.Fontoe, "No", 18, hoverColor: Color.White);
        
        this.AddElement("No-Button", new TextureButtonElement(noButtonData, noButtonLabelData, Anchor.Center, new Vector2(90, 0), size: new Vector2(100, 40), textOffset: new Vector2(0, 1), clickFunc: (element) =>
        {
            GuiManager.SetGui(new MenuGui());
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
                ((PixelisGame) Game.Instance!).OptionsConfig.SetValue("MasterVolume", slideBarElement.Value);
                AudioContext.MasterVolume = slideBarElement.Value;
            }
        }
        
        base.Dispose(disposing);
    }
}