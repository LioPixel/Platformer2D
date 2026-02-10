using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Graphics.Rendering.Renderers.Batches.Sprites;
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
using Sparkle.CSharp.Scenes;
using Veldrid;

namespace Platformer2D.CSharp.GUIs;

public class JoinGui : Gui
{
    private bool _isConnecting = false;
    private string _errorMessage = "";
    private float _errorDisplayTime = 0f;
    
    public JoinGui () : base("Join", null) { }

    protected override void Init()
    {
        base.Init();
        
        LabelData labelData = new LabelData(ContentRegistry.Fontoe, "Join", 18);
        this.AddElement("Titel", new LabelElement(labelData, Anchor.TopCenter, new Vector2(0, 50), new Vector2(5, 5)));
        
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
        
        // Texture text box.
        TextureTextBoxData textureTextBoxData = new TextureTextBoxData(ContentRegistry.UiMenu, hoverColor: Color.LightGray, resizeMode: ResizeMode.NineSlice, borderInsets: new BorderInsets(12), flip: SpriteFlip.None);
        LabelData textureTextBoxLabelData = new LabelData(ContentRegistry.Fontoe, "", 18, hoverColor: Color.White);
        LabelData textureHintTextBoxLabelData = new LabelData(ContentRegistry.Fontoe, "Type IP adress...", 18, color: Color.Gray);
        
        this.AddElement("Texture-Text-Box", new TextureTextBoxElement(textureTextBoxData, textureTextBoxLabelData, textureHintTextBoxLabelData, Anchor.Center, new Vector2(0, -10), 40, TextAlignment.Center, new Vector2(0, 1), (12, 12), new Vector2(260, 30), rotation: 0, clickFunc: (element) => {
            return true;
        }));
        
        // Join button.
        TextureButtonData createButtonData = new TextureButtonData(ContentRegistry.UiButton, hoverColor: Color.LightGray, resizeMode: ResizeMode.NineSlice, borderInsets: new BorderInsets(12));
        LabelData createButtonLabelData = new LabelData(ContentRegistry.Fontoe, "Join", 18, hoverColor: Color.White);
        
        this.AddElement("Join-Button", new TextureButtonElement(createButtonData, createButtonLabelData, Anchor.Center, new Vector2(0, 60), size: new Vector2(230, 40), textOffset: new Vector2(0, 1), clickFunc: (element) =>
        {
            if (!_isConnecting)
            {
                TryJoinServer();
            }
            return true;
        }));
        
        // Error message label (initially empty)
        LabelData errorLabelData = new LabelData(ContentRegistry.Fontoe, "", 18, color: Color.Red);
        this.AddElement("Error-Label", new LabelElement(errorLabelData, Anchor.Center, new Vector2(0, 110), new Vector2(1, 1)));
    }
    
    private void TryJoinServer()
    {
        // Get IP from text box
        TextureTextBoxElement textBox = (TextureTextBoxElement)this.GetElement("Texture-Text-Box");
        string ipAddress = textBox.LabelData.Text.Trim();
        
        // Use default if empty
        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            ipAddress = "127.0.0.1:7777";
        }
        
        _isConnecting = true;
        _errorMessage = "Connecting...";
        UpdateErrorLabel();
        
        // Set up connection callbacks
        NetworkManager.SetConnectionCallbacks(OnConnectionSuccess, OnConnectionFailed);
        
        // Attempt to join
        NetworkManager.JoinServer(ipAddress);
    }
    
    private void OnConnectionSuccess()
    {
        _isConnecting = false;
        _errorMessage = "";
        
        // Close the GUI on successful connection
        GuiManager.SetGui(null);
    }
    
    private void OnConnectionFailed(string reason)
    {
        _isConnecting = false;
        _errorMessage = $"Connection failed: {reason}";
        _errorDisplayTime = 5f; // Display error for 5 seconds
        UpdateErrorLabel();
    }
    
    private void UpdateErrorLabel()
    {
        LabelElement errorLabel = (LabelElement)this.GetElement("Error-Label");
        if (errorLabel != null)
        {
            errorLabel.Data.Text = _errorMessage;
        }
    }
    
    protected override void Update(double delta)
    {
        base.Update(delta);

        // Countdown error display time
        if (_errorDisplayTime > 0)
        {
            _errorDisplayTime -= (float)delta;
            if (_errorDisplayTime <= 0)
            {
                _errorMessage = "";
                UpdateErrorLabel();
            }
        }

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
}