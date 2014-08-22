using System;
using Scalesque;

namespace MonadicIT.Common
{
    public class Catch
    {
        public static Option<T> ToOption<T>(Func<T> action)
        {
            try
            {
                return action().ToSome();
            }
            catch (Exception)
            {
                return Option.None();
            }
        }
        
    }
}