using Platformer2D.CSharp;
using Sparkle.CSharp;
using Sparkle.CSharp.GUI.Loading;
using Veldrid;

GameSettings settings = new GameSettings()
{
    VSync = false,
    SampleCount = TextureSampleCount.Count8,
    IconPath = "content/icon.png"
};

using PlatformerGame game = new PlatformerGame(settings);
game.Run(null, new LogoLoadingGui("Startup", "content/Sparkle/images/logo.png"));