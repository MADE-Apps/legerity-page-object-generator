namespace Legerity.PageObjectGenerator.Infrastructure.Extensions
{
    using System.Linq;

    public static class StringExtensions
    {
        public static string Capitalize(this string value)
        {
            return value.First().ToString().ToUpper() + value.Substring(1);
        }
    }
}
