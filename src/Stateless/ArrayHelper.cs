using System;

namespace Stateless; 

internal static class ArrayHelper {

    public static T[] Empty<T>() {
#if NETSTANDARD1_0
            return new T[0];
#else
        return Array.Empty<T>();
#endif
    }

}