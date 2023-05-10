namespace LsideWPF.Utils
{
    using System.Collections;

    public static class Extensions
    {
        public static IEnumerable Iterate(this IEnumerator iterator)
        {
            while (iterator.MoveNext())
            {
                yield return iterator.Current;
            }
        }
    }
}
