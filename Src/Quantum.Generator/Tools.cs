using System.Globalization;
using System.Text.RegularExpressions;

namespace Quantum.Generator
{
    public static class Tools
    {
        public static string ToCamelCase(string text)
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            string camelCase = Regex.Replace(text, "(\\B[A-Z])", " $1");
            string titleCase = textInfo.ToTitleCase(camelCase.Replace("_", " "));
            
            string result = titleCase.Replace(" ", string.Empty);

            return result;
        }

        public static string ToPrivateName(string text)
        {            
            return char.ToLowerInvariant(text[0]) + text.Substring(1);
        }        
    }
}
