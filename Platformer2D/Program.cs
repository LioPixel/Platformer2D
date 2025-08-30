using Platformer2D.CSharp;
using Platformer2D.CSharp.Scenes;
using Sparkle.CSharp;

GameSettings settings = new GameSettings()
{
    VSync = false,
};

using PlatformerGame game = new PlatformerGame(settings);
game.Run(null);