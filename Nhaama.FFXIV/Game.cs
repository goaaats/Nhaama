using System.Diagnostics;
using System.IO;

namespace Nhaama.FFXIV
{
    public partial class Game
    {
        public enum GameType
        {
            Dx9,
            Dx11
        }

        public readonly GameType Type;
        public readonly string Version;

        public Game(Process process)
        {
            Type = process.MainModule.ModuleName.Contains("ffxiv_dx11") ? GameType.Dx11 : GameType.Dx9;

            var gameDirectory = new DirectoryInfo(process.MainModule.FileName);
            Version = File.ReadAllText(Path.Combine(gameDirectory.Parent.FullName, "ffxivgame.ver"));
        }
    }
}