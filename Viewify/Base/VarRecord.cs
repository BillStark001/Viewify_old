using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Viewify.Params;

namespace Viewify.Base
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
    public record VarRecord
    {
        // keys

        [JsonProperty("id")]
        public int? Id { get; set; }

        [JsonIgnore]
        private string? _name;

        [JsonProperty("name")]
        [JsonConverter(typeof(StringConverterWithCheck))]
        public string? Name
        {
            get { return _name; }
            set {
                if (!Utils.CheckNameValidity(value))
                    throw new InvalidDataException($"Invalid Name Property: {value}");
                _name = value; 
            }
        }

        [JsonIgnore]
        public bool IsIdEmpty => Id == null;
        [JsonIgnore]
        public bool IsNameEmpty => _name == null;

        public string GetProperPath(string parent)
        {
            return Utils.PathCombine(parent, _name ?? (Id != null ? $"#ID{Id}" : null));
        }

        // basic properties

        [JsonProperty("dispName")]
        public string? DisplayName { get; set; }

        [JsonProperty("desc")]
        public string? Description { get; set; }
        [JsonProperty("paramType")]
        public ParameterType ParameterType { get; set; } = ParameterType.String;
        [JsonProperty("ctrlType")]
        public ControlType ControlType { get; set; } = ControlType.Default;

        [JsonProperty("style")]
        public string? StyleStr { get; set; }

        // vals

        [JsonProperty("defStr")]
        public string? DefaultString { get; set; }
        [JsonProperty("defNum")]
        public (decimal, decimal, decimal)? DefaultNumber { get; set; }
        [JsonProperty("cmd")]
        public string? CommandName { get; set; }


        // enum related

        [JsonProperty("enumVals")]
        public List<EnumRecord>? EnumValues { get; set; }

        // additional

        [JsonProperty("additionalParams")]
        public SortedDictionary<string, string>? AdditionalParameters { get; set; }
        [JsonProperty("subCtrls")]
        public List<VarRecord>? SubControls { get; set; }
    }

    public static class VarRecordUtils
    {
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
