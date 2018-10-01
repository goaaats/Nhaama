using System.Collections.Generic;
using Newtonsoft.Json;
using Nhaama.Memory.Serialization.Converters;

namespace Nhaama.Memory.Serialization
{
    public class NhaamaSerializer
    {
        private List<JsonConverter> _converters = new List<JsonConverter>();
        
        public NhaamaSerializer(NhaamaProcess process)
        {
            _converters.Add(new ProcessModulePropertyConverter(process));
            _converters.Add(new PointerPropertyConverter(process));
        }

        public string SerializeObject(object value, Formatting formatting) => JsonConvert.SerializeObject(value, formatting, _converters.ToArray());
        
        public T DeserializeObject<T>(string json) => JsonConvert.DeserializeObject<T>(json, _converters.ToArray());
    }
}