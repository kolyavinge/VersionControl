using System.Collections.Generic;

namespace VersionControl.Infrastructure;

internal static class SetExt
{
    public static void RemoveRange<T>(this ISet<T> set, IEnumerable<T> collection)
    {
        foreach (var item in collection)
        {
            set.Remove(item);
        }
    }
}
