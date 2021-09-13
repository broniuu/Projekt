using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace consoleasync
{
    class MinimalDishComparer : IEqualityComparer<MinimalDish>
    {
        public bool Equals(MinimalDish x, MinimalDish y)
        {
            if (x.Price == y.Price)
                return true;
            else
                return false ;
        }

        public int GetHashCode([DisallowNull] MinimalDish dish)
        {
            return dish.Price.GetHashCode();
        }
    }
}
