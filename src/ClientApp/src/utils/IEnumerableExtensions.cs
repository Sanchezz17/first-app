using System.Collections.Generic;
using System.Linq;

namespace covidSim.ClientApp.src.utils
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<(T, T)> Multiply<T>(this IEnumerable<T> enumerable, IEnumerable<T> other)
        {
            return
                from first in enumerable
                from second in other
                select (first, second);
        }
    }
}