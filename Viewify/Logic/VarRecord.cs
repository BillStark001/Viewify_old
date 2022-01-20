﻿using Newtonsoft.Json;
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

        Int = 5, 
        Long = 6, 
        Double = 7, 

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

        // layout type
        HStack = 64, 
        VStack = 65, 
        Separator = 66, 
        Group = 68, 
        CollapsibleGroup = 69, 
    }

    public enum ControlType
    {
        Default,
        // numeric
        ProgressBar, 
        ScrollBar, 
        ScrollBarWithoutTextBox, 
        // vec
        Field2D, 
        // enum
        List,
        Radio, 
        // textfield
        MultiLine, 
        // stack
        Margin,
        // group
        NoMargin, 
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
            return (VarRecord?) JsonConvert.DeserializeObject(rcs, typeof(VarRecord));
        }
    }

}