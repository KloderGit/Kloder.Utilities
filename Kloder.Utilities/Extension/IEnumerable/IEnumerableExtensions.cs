using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kloder.Utilities.Extension.IEnumerable
{
    public static class IEnumerableExtensions
    {
        public static bool AreSequencesEqual(this System.Collections.IEnumerable first, System.Collections.IEnumerable second)
        {
            if (first == null && second == null) return true;
            if (first == null && second != null) return false;
            if (first != null && second == null) return false;

            var enumerator1 = first.GetEnumerator();
            var enumerator2 = second.GetEnumerator();

            while (enumerator1.MoveNext())
            {
                if (!enumerator2.MoveNext() || !enumerator1.Current.Equals(enumerator2.Current))
                    return false;
            }

            if (enumerator2.MoveNext())
                return false;

            return true;
        }
    
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> array)
        {
            if (array == null) return true;
            if (array.Any() == false) return true;

            return false;
        }
        
        /// <summary>
        /// Коллекция содержит элементы
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <returns></returns>
        public static bool IsFilled<T>(this IEnumerable<T> array)
        {
            return IsNullOrEmpty(array) == false;
        }
    }
}
