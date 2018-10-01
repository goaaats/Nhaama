using System;
using System.Diagnostics;
using System.IO;
using Nhaama.FFXIV.Model.Actor;
using Nhaama.Memory;

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

        private NhaamaProcess _process;
        private Definitions _definitions;

        public Game(Process process)
        {
            Type = process.MainModule.ModuleName.Contains("ffxiv_dx11") ? GameType.Dx11 : GameType.Dx9;
            _process = process.GetNhaamaProcess();

            var gameDirectory = new DirectoryInfo(process.MainModule.FileName);
            Version = File.ReadAllText(Path.Combine(gameDirectory.Parent.FullName, "ffxivgame.ver"));
            
            _definitions = Definitions.Get(_process, Version, Type);
        }

        #region Actors

        public ActorEntry[] GetLoadedActors()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}