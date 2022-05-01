using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viewify.Base
{

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public record EnumRecord
    {
        public EnumRecord() { }

        public EnumRecord(int v1, string v2)
        {
            Id = v1;
            Description = v2;
        }

        public EnumRecord(int v1, string v2, string v3)
        {
            Id = v1;
            StringKey = v2;
            Description = v3;
        }

        [JsonProperty("id")]
        public int Id { get; set; }


        [JsonIgnore]
        private string? _key;

        [JsonProperty("strKey")]
        [JsonConverter(typeof(StringConverterWithCheck))]
        public string? StringKey
        {
            get { return _key; }
            set
            {
                if (!Utils.CheckNameValidity(value))
                    throw new InvalidDataException($"Invalid Name Property: {value}");
                _key = value;
            }
        }

        [JsonProperty("desc")]
        public string? Description { get; set; }

        // TODO
        // add optional id to enum value
        // [JsonIgnore]
        // public bool IsIdEmpty => Id == null;

        [JsonIgnore]
        public bool IsKeyEmpty => _key == null;

        public string GetProperPath(string parent)
        {
            return Utils.PathCombine(parent, _key ?? (Id != null ? $"#ID{Id}" : null));
        }
    }
}
