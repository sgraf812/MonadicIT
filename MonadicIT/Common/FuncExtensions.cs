using System;

namespace MonadicIT.Common
{
    public static class FuncExtensions
    {
        public static Func<A, C> Then<A, B, C>(this Func<A, B> f, Func<B, C> g)
        {
            return a => g(f(a));
        } 
    }
}