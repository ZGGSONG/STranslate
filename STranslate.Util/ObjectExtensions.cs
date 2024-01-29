using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

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

        /// <summary>
        /// Adds all the data to a binding list
        /// </summary>
        public static void AddRange<T>(this BindingList<T>? list, IEnumerable<T>? data)
        {
            if (list == null || data == null)
            {
                return;
            }

            foreach (T t in data)
            {
                list.Add(t);
            }
        }

        public static void Insert<T>(this BindingList<T>? list, int index, IEnumerable<T>? data)
        {
            if (list == null || data == null || data.Count() < index)
            {
                return;
            }

            foreach (T t in data)
            {
                list.Insert(index++, t);
            }
        }

        /// <summary>
        /// 比较两个List是否相同
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool SetwiseEquivalentTo<T>(this IList<T> list, IList<T> other)
            where T : class
        {
            if (list.Except(other).Any())
                return false;
            if (other.Except(list).Any())
                return false;
            return true;
        }
    }
}
