using System;

namespace MonadicIT.Common
{
    public static class Throw
    {
        public static void If<T>(bool condition, string message) where T : Exception
        {
            if (condition)
            {
                throw (T) Activator.CreateInstance(typeof (T), message);
            }
        }
    }
}