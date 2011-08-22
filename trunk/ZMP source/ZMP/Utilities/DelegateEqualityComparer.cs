namespace ZMP.Utilities
{
    using System;
    using System.Collections.Generic;

    public class DelegateEqualityComparer<T> : IEqualityComparer<T>
    {
        private readonly Func<T, T, bool> comparer;

        public DelegateEqualityComparer(Func<T, T, bool> comparer)
        {
            if (comparer == null)
            {
                throw new ArgumentNullException("comparer");
            }   

            this.comparer = comparer;
        }

        public bool Equals(T x, T y)
        {
            return this.comparer(x, y);
        }

        public int GetHashCode(T obj)
        {
            return obj.ToString().ToLower().GetHashCode();
        }
    }
}
