using System;
using System.Collections;
using System.Collections.Generic;
using Nhaama.FFXIV.Actor.Model;
using Nhaama.Memory;

namespace Nhaama.FFXIV.Actor
{
    public class ActorTableCollection : ICollection
    {
        private Game _game;
        private ActorEntry[] _currentEntries;

        public ActorTableCollection(Game game)
        {
            _game = game;
        }

        public void Update()
        {
            var numEntries = _game.Process.ReadUInt32(_game.Definitions.ActorTable);

            _currentEntries = new ActorEntry[numEntries];

            for (int i = 0; i < numEntries; i++)
            {
                ulong offset = 8 + (numEntries * 8);

                var address = new Pointer(_game.Process, _game.Definitions.ActorTable + offset, 0);

                Console.WriteLine(_game.Process.ReadString(address + 48));
            }
        }

        public IEnumerator GetEnumerator()
        {
            return new ActorTableEnumerator(_currentEntries);
        }

        int ICollection.Count => _currentEntries.Length;

        void ICollection.CopyTo(Array array, int index)
        {
            foreach (var entry in _currentEntries)
            {
                array.SetValue(entry, index);
                index++;
            }
        }

        bool ICollection.IsSynchronized => false;

        object ICollection.SyncRoot => this;
    }
}