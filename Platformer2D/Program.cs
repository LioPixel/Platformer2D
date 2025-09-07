using Platformer2D.CSharp;
using Sparkle.CSharp;
using Veldrid;

GameSettings settings = new GameSettings()
{
    VSync = false,
    SampleCount = TextureSampleCount.Count8
};

using PlatformerGame game = new PlatformerGame(settings);
game.Run(null);