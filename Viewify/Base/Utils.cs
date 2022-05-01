using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viewify.Base
{
    public static class Utils
    {
        public static string ReserveDigit(this double value, int digit = 3, bool round = false, bool fill = false)
        {
            if (digit == 0)
                return (round ? (long)(value > 0 ? value + 0.5 : value - 0.5) : (long)value).ToString();
            else if (digit > 0)
            {
                double delta = round ? 0 : 0.5 * Math.Pow(10, -digit);
                var ret = Math.Round((decimal)(value - delta), digit, MidpointRounding.AwayFromZero);
                return fill ? ret.ToString($"n{digit}") : ret.ToString();
            }
            else
            {
                var ndigit = -digit;
                long exp = (long)Math.Pow(10, ndigit);
                long i = value >= 0 ? (long)value : (long)-value;
                var div = (i / exp) * exp;
                var mod = i % exp;
                if (!round || mod < exp / 2)
                    return (value >= 0 ? div : -div).ToString();
                else
                    return (value >= 0 ? div + exp : -div - exp).ToString();
            }
        }

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

        public static string PathCombine(string parent, string? child)
        {
            if (parent == null)
                parent = string.Empty;
            if (child == null)
                child = string.Empty;

            if (parent.Contains(' ') || child.Contains(' '))
                throw new InvalidOperationException("Spaces are not allowed in a path.");

            if (string.IsNullOrWhiteSpace(parent))
                return child;
            if (string.IsNullOrWhiteSpace(child))
                child = "#UNDEF";
            return parent + '.' + child;
        }
    }


    public class StringConverterWithCheck : JsonConverter<string>
    {
        public override void WriteJson(JsonWriter writer, string? value, JsonSerializer serializer)
        {
            writer.WriteValue(value);
        }

        public override string? ReadJson(JsonReader reader, Type objectType, string? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var s = (string)reader.Value!;
            if (!Utils.CheckNameValidity(s))
                throw new InvalidDataException($"Invalid Name Property: {reader.Value}");
            return s;
        }
    }
}
