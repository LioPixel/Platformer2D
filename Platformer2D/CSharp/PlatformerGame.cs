using Platformer2D.CSharp.GUIs;
using Platformer2D.CSharp.Overlays;
using Sparkle.CSharp;
using Sparkle.CSharp.Content;
using Sparkle.CSharp.GUI;
using Sparkle.CSharp.Overlays;
using Sparkle.CSharp.Registries;

namespace Platformer2D.CSharp;

public class PlatformerGame : Game
{
    public PlatformerGame(GameSettings settings) : base(settings)
    {
        
    }

    protected override void OnRun()
    {
        base.OnRun();
        RegistryManager.AddRegistry(new ContentRegistry());
    }
    
    protected override void Init()
    {
        base.Init();
        GuiManager.SetGui(new MenuGui());
        OverlayManager.AddOverlay(new DebugOverlay("Debug", true));
    }

    protected override void Load(ContentManager content)
    {
        base.Load(content);
    }

    protected override void Update(double delta)
    {
        base.Update(delta);
    }
}