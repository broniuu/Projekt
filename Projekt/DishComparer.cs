using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace consoleasync
{
    class DishComparer : IEqualityComparer<Dish>
    {
        public bool Equals(Dish x, Dish y)
        {
            if (x.Price == y.Price)
                return true;
            else
                return false ;
        }

        public int GetHashCode([DisallowNull] Dish dish)
        {
            return dish.Price.GetHashCode();
        }
    }
}
