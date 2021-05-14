using System.Text.RegularExpressions;

namespace PlantUml.Reflector
{
    public static class PumlProcessingExtensions
    {
        private static readonly Regex Regex = new Regex(@"[^._0-9a-zA-Z]");

        public static string AsSlug(this string text) => Regex.Replace(text, "_");
    }
}