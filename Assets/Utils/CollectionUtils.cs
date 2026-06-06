
using System;
using System.Collections;
using System.Collections.Generic;

namespace CustomUtils
{
    public static class CollectionUtils
    {
        #region Lists
        public static void Shuffle<T>(this IList<T> ts)
        {
            var count = ts.Count;
            var last = count - 1;
            for (var i = 0; i < last; ++i)
            {
                var r = UnityEngine.Random.Range(i, count);
                var tmp = ts[i];
                ts[i] = ts[r];
                ts[r] = tmp;
            }
        }
        
        public static void Shuffle<T>(this IList<T> ts, System.Random seededRandom) {
            var count = ts.Count;
            var last = count - 1;
            for (var i = 0; i < last; ++i) {
                var r = seededRandom.Next(i, count);
                var tmp = ts[i];
                ts[i] = ts[r];
                ts[r] = tmp;
            }
        }

        #endregion

        #region Dictionaries

        public static K FindFirstKeyFromValue<K, V>(this IDictionary<K, V> dictionary, V value) /* where K : IEquatable<K> */
        {
            foreach(var kv in dictionary)
            {
                if (kv.Key.Equals(value)) return kv.Key;
            }
            return default;
        }

        #endregion
    }
}