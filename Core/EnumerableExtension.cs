using System;
using System.Collections.Generic;
using System.Linq;

namespace Core
{
    public static class EnumerableExtension
    {
        public static string FormExceptionMessage(this IEnumerable<Exception> exceptions)
        {
            var exceptionMessages = exceptions.Select(e => e.Message).ToArray();
            return string.Join("\n", exceptionMessages);
        }

        public static IEnumerable<T> IfConditionTrue<T>(this IEnumerable<T> source,
            Func<bool> condition, Action throwException)
        {
            if (!condition())
            {
                throwException();
            }

            return source;
        }

        public static IEnumerable<IEnumerable<T>> SplitToChunks<T>(this IEnumerable<T> collection, int batchSize)
        {
            T[] bucket = null;
            var count = 0;

            foreach (var item in collection)
            {
                if (bucket == null)
                {
                    bucket = new T[batchSize];
                }

                bucket[count++] = item;

                if (count != batchSize)
                {
                    continue;
                }

                yield return bucket;

                bucket = null;
                count = 0;
            }

            if (bucket != null)
            {
                yield return bucket.Where(x => !x.Equals(default(T)));
            }
        }

        public static bool StartWith<T>(this T[] source, T[] array, int offset = 0)
        {
            var arrayPosition = 0;

            for (var i = offset; i < source.Length; i++)
            {
                if (!source[i].Equals(array[arrayPosition]))
                {
                    return false;
                }
                arrayPosition++;

                if (arrayPosition == array.Length)
                {
                    return true;
                }
            }

            return false;
        }

        public static void ShiftToStart<T>(this T[] source, int offset, int length)
        {
            var sourcePosition = 0;
            for (var i = offset; i < offset + length; i++)
            {
                source[sourcePosition++] = source[i];
            }
            Array.Clear(source, length, offset);
        }
    }
}