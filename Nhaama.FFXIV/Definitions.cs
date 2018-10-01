using System;
using System.Net;
using System.Security.Policy;
using Nhaama.Memory;

namespace Nhaama.FFXIV
{
    public class Definitions
    {
        private Definitions(NhaamaProcess process)
        {
            ActorTable = new Pointer(process, 0x199DA38);
            TerritoryType = new Pointer(process, 0x19D55E8, 0x4C);
            Time = new Pointer(process, 0x19815F0, 0x10, 0x8, 0x28, 0x80);
            Weather = new Pointer(process, 0x19579A8, 0x27);
        }

        public Pointer ActorTable;
        public Pointer TerritoryType;
        public Pointer Time;
        public Pointer Weather;

        public int ActorID = 0x74;
        public int Name = 0x30;
        public int BnpcBase = 0x80;
        public int OwnerID = 0x84;
        public int ModelChara = 0x16FC;
        public int Job = 0x1788;
        public int Level = 0x178A;
        public int World = 0x1744;
        public int CompanyTag = 0x16A2;
        public int Customize = 0x1688;
        public int RenderMode = 0x104;
        public int ObjectKind = 0x8C;

        public int Head = 0x15E8;
        public int Body = 0x15EC;
        public int Hands = 0x15F0;
        public int Legs = 0x15F4;
        public int Feet = 0x15F8;
        public int Ear = 0x15FC;
        public int Neck = 0x1600;
        public int Wrist = 0x1604;
        public int RRing = 0x1608;
        public int LRing = 0x160C;

        public int MainWep = 0x1342;
        public int OffWep = 0x13A8;
        
        private static readonly Uri DefinitionStoreUrl = new Uri("https://raw.githubusercontent.com/goaaats/Nhaama/master/definitions/FFXIV");
        
        public static Definitions Get(NhaamaProcess p, string version)
        {
            using (WebClient client = new WebClient())
            {
                var definitionJson = client.DownloadString(new Uri(DefinitionStoreUrl, $"{version}.json"));
                
                var serializer = p.GetSerializer();
                var deserializedDefinition = serializer.DeserializeObject<Definitions>(definitionJson);

                return deserializedDefinition;
            }
        }

        public static string GetJson(NhaamaProcess process)
        {
            var serializer = process.GetSerializer();

            return serializer.SerializeObject(new Definitions(process), Newtonsoft.Json.Formatting.Indented);
        }
    }
}