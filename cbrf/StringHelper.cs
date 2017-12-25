namespace cbrf
{
    public static class StringHelper
    {
        public static string Fmt(this string fmt, params object[] args)
        {
            return string.IsNullOrEmpty(fmt) || args == null || args.Length == 0 ? fmt : string.Format(fmt, args);
        }
    }
}