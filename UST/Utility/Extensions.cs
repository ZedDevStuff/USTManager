using System.Collections.Generic;

namespace USTManager.Utility
{
    public static class Extensions
    {
        public static V GetOrAdd<K, V>(this Dictionary<K, V> dict, K key, V toAdd)
        {
            if(dict.TryGetValue(key, out V existing))
            {
                return existing;
            }
            else
            {
                dict.Add(key, toAdd);
                return toAdd;
            }
        }

        // Polyfill for something that .NET Core has had since 2017
        public static void Deconstruct<K, V>(this KeyValuePair<K, V> pair, out K key, out V val)
        {
            key = pair.Key;
            val = pair.Value;
        }
    }
}