using Pixelis.CSharp;
using Sparkle.CSharp;
using Sparkle.CSharp.GUI.Loading;
using Veldrid;

GameSettings settings = new GameSettings()
{
    VSync = false,
    SampleCount = TextureSampleCount.Count8,
    IconPath = "content/icon.png",
    Title = "Pixelis - 2D [By LioPixel & MrScautHD]"
};

using PixelisGame game = new PixelisGame(settings);
game.Run(null, new LogoLoadingGui("Startup", "content/Sparkle/images/logo.png"));