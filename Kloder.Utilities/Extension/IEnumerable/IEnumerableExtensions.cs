using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kloder.Utilities.Extension.IEnumerable
{
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Коллекция содержит элементы
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <returns></returns>
        public static bool IsFilled<T>(this IEnumerable<T> array)
        {
            return array != null && array.Any() != false;
        }

        /// <summary>
        /// Коллекция не содержит элементов или не создана
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> array)
        {
            return array == null || array.Any() == false;
        }
    }
}
