
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

        /// <summary>
        /// Creates a distinct stream based on a custom predicate.
        /// </summary>
        /// <typeparam name="TValue">The type of the IEnumerable.</typeparam>
        /// <typeparam name="TDistinct">The type of the value to test for distinctness.</typeparam>
        /// <param name="source">The source IEnumerable.</param>
        /// <param name="predicate">The custom distinct item producer.</param>
        /// <returns>The distinct stream.</returns>
        public static IEnumerable<TValue> DistinctBy<TValue, TDistinct>(this IEnumerable<TValue> source, Func<TValue, TDistinct> predicate)
        {
            HashSet<TDistinct> set = new HashSet<TDistinct>();
            foreach (TValue value in source)
            {
                if (set.Add(predicate(value)))
                {
                    yield return value;
                }
            }
        }
       
        public static IEnumerable<T> AsEnumerable<T>(this IEnumerator<T> enumerator)
        {
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }

        public static T RandomElement<T>(this IEnumerable<T> source, Random rng)
        {
            T current = default(T);
            int count = 0;
            foreach (T element in source)
            {
                count++;
                if (rng.Next(count) == 0)
                {
                    current = element;
                }            
            }
            if (count == 0)
            {
                throw new InvalidOperationException("Sequence was empty");
            }
            return current;
        }
        
        public static bool IndexOf<T>(this IList<T> list, T val, out int index)
        {
            index = list.IndexOf(val);
            return index >= 0;
        }

        public static string Stringify<T>(this IEnumerable<T> list)
        {
            return string.Join(", ", list);
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