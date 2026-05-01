using System.Text.Json;

namespace Aria2Manager.Core.Helpers
{
    public static class CloneHelper
    {
        //深拷贝
        public static T DeepClone<T>(this T source)
        {
            if (source == null) return default!;

            var json = JsonSerializer.Serialize(source);
            return JsonSerializer.Deserialize<T>(json)!;
        }
    }
}