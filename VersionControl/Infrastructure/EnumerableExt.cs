using System.Collections.Generic;

namespace VersionControl.Infrastructure;

internal static class EnumerableExt
{
    public static void Each<T>(this IEnumerable<T> collection, Action<T> action)
    {
        foreach (var item in collection)
        {
            action(item);
        }
    }
}
