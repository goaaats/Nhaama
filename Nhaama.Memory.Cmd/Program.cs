using System;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using Nhaama.Memory;
using Nhaama.Memory.Serialization;
using Nhaama.Memory.Serialization.Converters;

namespace Nhaama.Memory.Cmd
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var proc = Process.GetProcessesByName("ffxiv_dx11")[0].GetNhaamaProcess();
            
            Pointer p = new Pointer(proc, 0x19D55E8, 0x4c);
            Console.WriteLine(p.Address.ToUInt64().ToString("X"));
            Console.WriteLine(proc.ReadByte(p.Address));

            var serializer = proc.GetSerializer();
            
            var pointerJson = serializer.SerializeObject(p, Formatting.Indented);
            Console.WriteLine(pointerJson);
            
            var p2 = serializer.DeserializeObject<Memory.Pointer>(pointerJson);
            
            Console.WriteLine(p2.Address.ToUInt64().ToString("X"));
            
            var p3 = new Pointer(proc, "ffxiv_dx11.exe+19D55E8,4C");
            Console.WriteLine(proc.ReadByte(p3.Address));
        }
    }
}