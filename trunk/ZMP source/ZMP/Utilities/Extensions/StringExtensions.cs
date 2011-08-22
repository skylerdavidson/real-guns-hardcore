namespace ZMP.Utilities.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public static class StringExtensions
    {
        public static string TrimQuotes(this string original)
        {         
            return original.Trim('\"');
        }

        public static bool IsPrefix(this string whole, string prefix)
        {
            for (int i = 0; i < prefix.Length; i++)
            {
                if (whole[i] != prefix[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
