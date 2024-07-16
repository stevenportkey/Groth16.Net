using System.Collections.Generic;
using System.Linq;

namespace Groth16.Net
{
    using InputType = IDictionary<string, IList<string>>;

    public static class Helpers
    {
        internal static string ToJsonString(this InputType input)
        {
            var entries = input.Select((kv, _) => $"\"{kv.Key}\":{kv.Value.ToJsonString()}");
            return "{" + string.Join(",", entries) + "}";
        }

        internal static string ToJsonString(this IList<string> values)
        {
            return "[" + string.Join(",", values.Select(x => $"\"{x}\"")) + "]";
        }

        internal static string ToJsonString(this IList<List<string>> values)
        {
            return "[" + string.Join(",", values.Select(x => x.ToJsonString())) + "]";
        }

        internal static bool IsDecimal(this string value)
        {
            foreach (var c in value)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }

        internal static bool IsHex(this string value)
        {
            foreach (var c in value.ToLower())
            {
                var isHex = (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f');

                if (!isHex)
                    return false;
            }

            return true;
        }
    }
}