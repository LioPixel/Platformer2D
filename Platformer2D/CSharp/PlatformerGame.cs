using Assimp;
using MiniAudioEx.Core.StandardAPI;
using Platformer2D.CSharp.GUIs;
using Platformer2D.CSharp.Overlays;
using Platformer2D.CSharp.Scenes.Levels;
using Sparkle.CSharp;
using Sparkle.CSharp.Content;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.GUI;
using Sparkle.CSharp.IO.Configs.Json;
using Sparkle.CSharp.Overlays;
using Sparkle.CSharp.Registries;
using Sparkle.CSharp.Scenes;
using Scene = Sparkle.CSharp.Scenes.Scene;

namespace Platformer2D.CSharp;

public class PlatformerGame : Game
{
    public JsonConfig OptionsConfig;
    
    public PlatformerGame(GameSettings settings) : base(settings)
    {
        JsonConfigBuilder jsonConfigBuilder = new JsonConfigBuilder("configs", "options");
        jsonConfigBuilder.Add("Vsync", false);
        jsonConfigBuilder.Add("DebugMode", false);
        jsonConfigBuilder.Add("Sounds", true);
        jsonConfigBuilder.Add("MasterVolume", 0.5F);
        this.OptionsConfig = jsonConfigBuilder.Build();
    }

    protected override void OnRun()
    {
        base.OnRun();
        RegistryManager.AddRegistry(new ContentRegistry());
        
        // Apply Config settings:
        GlobalGraphicsAssets.GraphicsDevice.SyncToVerticalBlank = this.OptionsConfig.GetValue<bool>("Vsync");
    }
    
    protected override void Init()
    {
        base.Init();
        GuiManager.SetGui(new MenuGui());
        OverlayManager.AddOverlay(new DebugOverlay("Debug", this.OptionsConfig.GetValue<bool>("DebugMode")));
        AudioContext.MasterVolume = ((PlatformerGame) Game.Instance!).OptionsConfig.GetValue<float>("MasterVolume");
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