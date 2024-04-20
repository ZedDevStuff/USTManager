using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
        public static bool FastStartsWith(this string str, string target)
        {
            if (str.Length < target.Length) return false;
            else
            {
                for (int i = 0; i < target.Length; i++)
                {
                    if (str[i] != target[i]) return false;
                }
            }
            return true;
        }

        // Polyfill for something that .NET Core has had since 2017
        public static void Deconstruct<K, V>(this KeyValuePair<K, V> pair, out K key, out V val)
        {
            key = pair.Key;
            val = pair.Value;
        }

        public static string GetPath(this Transform transform)
        {
            List<string> path = new();
            Transform current = transform; 
            do
            {
                path.Add(current.name);
                current = current.parent;
            }
            while(current != null);
            path.Reverse();
            return path.Aggregate((a,b) => $"{a}/{b}");
        }
    }
}