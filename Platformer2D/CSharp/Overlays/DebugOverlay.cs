using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Interact;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Entities;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.Overlays;
using Sparkle.CSharp.Scenes;
using Veldrid;

namespace Platformer2D.CSharp.Overlays;

public class DebugOverlay : Overlay
{
    
    public DebugOverlay(string name, bool enabled = false) : base(name, enabled)
    {
    }

    protected override void Draw(GraphicsContext context, Framebuffer framebuffer)
    {
        Camera2D? cam = SceneManager.ActiveCam2D;

        if (cam == null)
        {
            return;
        }
        
        Vector2 mousePos = Input.GetMousePosition();
        Vector2 mouseWorldPos = cam.GetScreenToWorld(mousePos);
        
        Vector2 mouseBlock = new Vector2(
            (int) Math.Floor((mouseWorldPos.X + 8) / 16f),
            (int) Math.Floor((mouseWorldPos.Y + 8) / 16f)
        );
        
        this.DrawBlockHighlight(context, framebuffer, cam, mouseBlock);
        
        context.SpriteBatch.Begin(context.CommandList, framebuffer.OutputDescription);
        context.SpriteBatch.DrawText(ContentRegistry.Fontoe, $"POS: {mouseBlock}", mousePos, 18, color: Color.Black);
        context.SpriteBatch.End();
    }

    private void DrawBlockHighlight(GraphicsContext context, Framebuffer framebuffer, Camera2D cam, Vector2 mouseBlock)
    {
        Vector2 blockWorldPos = new Vector2(
            mouseBlock.X * 16f, 
            mouseBlock.Y * 16f
        );
        
        Vector2 blockScreenPos = cam.GetWorldToScreen(blockWorldPos);
        blockScreenPos -= new Vector2(8 * 7, 8 * 7);
        
        context.PrimitiveBatch.Begin(context.CommandList, framebuffer.OutputDescription);

        context.PrimitiveBatch.DrawFilledRectangle(
            new RectangleF(blockScreenPos.X, blockScreenPos.Y, 16 * 7, 16 * 7),
            color: new Color(128, 128, 128, 128) // semi-transparent gray
        );

        context.PrimitiveBatch.End();
    }
}