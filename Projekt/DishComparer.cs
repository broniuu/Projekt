using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace consoleasync
{
    class DishComparer : IEqualityComparer<Dish>
    {
        public bool Equals(Dish x, Dish y)
        {
            return string.Equals(x.Name, y.Name, StringComparison.CurrentCultureIgnoreCase);
        }

        public int GetHashCode([DisallowNull] Dish obj)
        {
            return obj.Name.GetHashCode();
        }
    }
}
