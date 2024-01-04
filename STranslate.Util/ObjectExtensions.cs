using Newtonsoft.Json;

namespace STranslate.Util
{
    public static class ObjectExtensions
    {
        public static T DeepClone<T>(this T source)
        {
            if (source == null)
                return default!;

            var json = JsonConvert.SerializeObject(source);
            return JsonConvert.DeserializeObject<T>(json)!;
        }
    }
}
