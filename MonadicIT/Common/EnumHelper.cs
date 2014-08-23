using System;

namespace MonadicIT.Common
{
    public class EnumHelper<T> where T : /* Enum, */ struct
    {
        public static readonly T[] Values = (T[]) Enum.GetValues(typeof (T));

        public static object ThrowUnlessEnum()
        {
            if (!typeof (T).IsEnum)
            {
                throw new NotSupportedException("T must be an enumeration type for modeling " +
                                                "a discrete vocabulary");
            }
            return null;
        }
    }
}