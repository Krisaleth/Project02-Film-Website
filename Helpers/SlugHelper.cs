using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Project02.Helper
{
    public static class SlugHelper
    {
        public static string GenerateSlug(string title)
        {
            var slug = title.ToLowerInvariant();
            slug = slug.Normalize(System.Text.NormalizationForm.FormD);
            var chars = slug.Where(c => CharUnicodeInfo.GetUnicodeCategory(c)
                       != UnicodeCategory.NonSpacingMark).ToArray();
            slug = new string(chars).Normalize(NormalizationForm.FormC);
            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
            slug = Regex.Replace(slug, @"\s+", "-").Trim('-');
            return slug.Length > 100 ? slug[..100] : slug;
        }
    }
}
