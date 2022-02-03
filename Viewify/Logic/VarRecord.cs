using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

namespace Viewify.Logic
{
    public enum ParameterType
    {
        // value type

        String = 0,
        Regex = 1,
        Enum = 2,
        EnumVar = 3,

        Bool = 4, 
        EnumBool = 5, // boolean radiobutton etc.

        Int = 6, 
        Decimal = 7, 

        Vec2 = 8, 
        Vec3 = 9, 
        Vec4 = 10,

        Mat22 = 12,
        Mat33 = 13,
        Mat44 = 14, 

        // command type
        Button = 32, 
        TextField = 33, 
        TextLabel = 34, 
        ProgressBar = 35, 

        // layout type
        HStack = 64, 
        VStack = 65, 
        Separator = 66, 
        Group = 68, 
        CollapsibleGroup = 69, 
        TabView = 70, 
    }

    public enum ControlType
    {
        // general
        Default = 0,
        IgnoreFieldInGroup = 1 << 0,
        // numeric
        ScrollBar = 1 << 1, 
        // vec
        Field2D = 1 << 2,
        // enum & enumvar
        Radio = 1 << 3, 

        // textfield & textlabel
        WithEditor = 1 << 8, 

        // stack
        WithMargin = 1 << 9, 
        // group
        NoMargin = 1 << 10, 
        // collapsible 
        Expand = 1 << 11, 
        // tabview
        
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public record EnumValue
    {
        public EnumValue() { }

        public EnumValue(int v1, string v2)
        {
            Id = v1;
            Description = v2;
        }

        public EnumValue(int v1, string v2, string v3)
        {
            Id = v1;
            StringKey = v2;
            Description = v3;
        }

        [JsonProperty("id")]
        public int Id { get; set; }
        
        [JsonProperty("strKey")]
        public string? StringKey { get; set; }

        [JsonProperty("desc")]
        public string? Description { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public record VarRecord
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        [JsonConverter(typeof(StringConverterWithCheck))]
        private string? _name;
        public string? Name
        {
            get { return _name; }
            set { _name = value; }
        }

        [JsonProperty("dispName")]
        public string? DisplayName { get; set; }

        [JsonProperty("desc")]
        public string? Description { get; set; }
        [JsonProperty("paramType")]
        public ParameterType ParameterType { get; set; } = ParameterType.String;
        [JsonProperty("ctrlType")]
        public ControlType ControlType { get; set; } = ControlType.Default;

        // vals

        [JsonProperty("defStr")]
        public string? DefaultString { get; set; }
        [JsonProperty("defNum")]
        public (decimal, decimal, decimal)? DefaultNumber { get; set; }
        [JsonProperty("cmd")]
        public string? CommandName { get; set; }


        // enum related

        [JsonProperty("enumVals")]
        public List<EnumValue>? EnumValues { get; set; }

        // additional

        [JsonProperty("additionalParams")]
        public SortedDictionary<string, string>? AdditionalParameters { get; set; }
        [JsonProperty("subCtrls")]
        public List<VarRecord>? SubControls { get; set; }
    }

    public class StringConverterWithCheck : JsonConverter<string>
    {
        public override void WriteJson(JsonWriter writer, string? value, JsonSerializer serializer)
        {
            writer.WriteValue(JsonConvert.SerializeObject(value));
        }

        public override string? ReadJson(JsonReader reader, Type objectType, string? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var s = JsonConvert.DeserializeObject(((string) reader.Value) ?? "null", typeof(string)) as string;
            if (!VarRecordUtils.CheckNameValidity(s))
                throw new InvalidDataException($"Invalid Name Property: {reader.Value}");
            return s;
        }
    }

    public static class VarRecordUtils
    {
        public static bool CheckNameValidity(string? s)
        {
            if (s == null || s[0] < 'A' || (s[0] > 'Z' && s[0] < 'a') || s[0] > 'z')
                return false;
            foreach (char c in s)
            {
                if (c < '0' || (c > '9' && c < '@') || (c > 'Z' && c < 'a') || (c > 'z'))
                    return c == '_';
            }
            return true;
        }

        public static string Serialize(VarRecord rc)
        {
            return JsonConvert.SerializeObject(rc, new Newtonsoft.Json.Converters.StringEnumConverter());
        }

        public static VarRecord? Deserialize(string rcs)
        {
            if (string.IsNullOrWhiteSpace(rcs))
                return null;
            return (VarRecord?) JsonConvert.DeserializeObject(rcs, typeof(VarRecord));
        }
    }

}
