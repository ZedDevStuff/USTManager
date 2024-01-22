using System.Collections.Generic;

namespace USTManager.Utility
{
    public static class Extensions
    {
        public static bool FastStartsWith(this string str, string value)
        {
            if(str.Length < value.Length) return false;
            for(int i = 0; i < value.Length; i++)
            {
                if(str[i] != value[i]) return false;
            }
            return true;
        }

        public static V GetOrAdd<K, V>(this Dictionary<K, V> dict, K key, V value)
        {
            if(dict.TryGetValue(key, out V existing))
            {
                return existing;
            }
            else
            {
                dict.Add(key, value);
                return value;
            }
        }
    }
}