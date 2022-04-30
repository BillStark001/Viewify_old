using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viewify.Base
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public record ValueRecord
    {
        [JsonProperty("bool")]
        public bool? Bool;
        [JsonProperty("int")]
        public int? Int;
        [JsonProperty("double")]
        public double? Double;
        [JsonProperty("str")]
        public string? String;
    }

    public static class ValueUtils
    {
        public static string Serialize(ValueRecord rc)
        {
            return JsonConvert.SerializeObject(rc, new Newtonsoft.Json.Converters.StringEnumConverter());
        }

        public static ValueRecord? Deserialize(string rcs)
        {
            return (ValueRecord?)JsonConvert.DeserializeObject(rcs, typeof(ValueRecord));
        }

        public static string Serialize2(Dictionary<int, ValueRecord> rc)
        {
            return JsonConvert.SerializeObject(rc, new Newtonsoft.Json.Converters.StringEnumConverter());
        }

        public static Dictionary<int, ValueRecord>? Deserialize2(string rcs)
        {
            return (Dictionary<int, ValueRecord>?)JsonConvert.DeserializeObject(rcs, typeof(Dictionary<int, ValueRecord>));
        }

        public static bool ParseBoolean(object? obj, bool defVal = false) 
        {
            if (obj is ValueRecord vr)
                obj = vr.Bool;
            if (obj == null)
                return defVal;
            else if (obj as bool? is bool b)
                return b;
            else if (obj as string is string s)
                return s.ToLowerInvariant() == "true";
            else if (obj as int? is int i)
                return i != 0;
            else if (obj as double? is double d)
                return !double.IsNaN(d) && d != 0;
            return defVal;
        }

        public static int ParseInt(object? obj, int defVal = 0)
        {
            if (obj is ValueRecord vr)
                obj = vr.Int;
            if (obj == null)
                return defVal;
            else if (obj as bool? is bool b)
                return b ? 1 : 0;
            else if (obj as string is string s)
                return int.TryParse(s, out var res) ? res : defVal;
            else if (obj as int? is int i)
                return i;
            else if (obj as double? is double d)
                return (int)d;
            return defVal;
        }

        public static double ParseDouble(object? obj, double defVal = 0)
        {
            if (obj is ValueRecord vr)
                obj = vr.Double;
            if (obj == null)
                return defVal;
            else if (obj as bool? is bool b)
                return b ? 1 : 0;
            else if (obj as string is string s)
                return double.TryParse(s, out var res) ? res : defVal;
            else if (obj as int? is int i)
                return i;
            else if (obj as double? is double d)
                return d;
            return defVal;
        }

        public static string? ParseString(object? obj)
        {
            if (obj is ValueRecord vr)
                obj = vr.String;
            if (obj == null)
                return null;
            return obj.ToString();
        }
    }
}
