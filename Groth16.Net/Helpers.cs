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
    }
}