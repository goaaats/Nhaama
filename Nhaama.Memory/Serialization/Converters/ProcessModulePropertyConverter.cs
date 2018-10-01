using System;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;

namespace Nhaama.Memory.Serialization.Converters
{
    public class ProcessModulePropertyConverter : JsonConverter
    {
        private readonly NhaamaProcess _process;
        
        public ProcessModulePropertyConverter(NhaamaProcess process)
        {
            _process = process;
        }
        
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ProcessModule);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                serializer.Serialize(writer, null);
                return;
            }

            var module = value as ProcessModule;
            
            writer.WriteStartObject();
            
            writer.WritePropertyName("ModuleName");
            serializer.Serialize(writer, module.ModuleName);
            
            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // Skip object start
            reader.Read();
            
            // Read the module name
            var moduleName = reader.ReadAsString();

            return _process.BaseProcess.Modules.Cast<ProcessModule>().First(x => x.ModuleName == moduleName);
        }
    }
}