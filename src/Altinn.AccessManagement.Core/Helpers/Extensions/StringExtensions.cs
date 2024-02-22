using System.Globalization;
using System.Text;

namespace Altinn.AccessManagement.Core.Helpers.Extensions
{
    /// <summary>
    /// Extensions to facilitate sanitization of string values
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Sanitize the input as a file name. 
        /// </summary>
        /// <param name="input">The input variable to be sanitized</param>
        /// <param name="throwExceptionOnInvalidCharacters">Throw exception instead of replacing invalid characters with '-'</param>
        /// <returns></returns>
        public static string AsFileName(this string input, bool throwExceptionOnInvalidCharacters = true)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return input;
            }

            char[] illegalFileNameCharacters = Path.GetInvalidFileNameChars();
            if (throwExceptionOnInvalidCharacters)
            {
                if (illegalFileNameCharacters.Any(ic => input.Any(i => ic == i)))
                {                    
                    throw new ArgumentOutOfRangeException(nameof(input));
                }

                return input;
            }

            return illegalFileNameCharacters.Aggregate(input, (current, c) => current.Replace(c, '-'));
        }

        /// <summary>
        /// Compare to strings doing a loose compare ignoring case and diacritics
        /// </summary>
        /// <param name="text1">First text</param>
        /// <param name="text2">Second text</param>
        /// <returns>true if the texts are similar.</returns>
        public static bool IsSimilarTo(this string text1, string text2)
        {
            const int CompareLength = 4;

            text1 ??= string.Empty;
            text2 ??= string.Empty;

            text1 = text1.Trim().Length > CompareLength ? text1.Remove(CompareLength).Trim() : text1.Trim();
            text2 = text2.Trim().Length > CompareLength ? text2.Remove(CompareLength).Trim() : text2.Trim();

            text1 = text1.RemoveDiacritics();
            text2 = text2.RemoveDiacritics();

            return text1.Equals(text2, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Remove diacritics from a string while normalizing it.
        /// </summary>
        /// <param name="text">The text to normalize.</param>
        /// <returns>The normalized text.</returns>
        public static string RemoveDiacritics(this string text)
        {
            string normalizedText;

            // � is not a special character in Norwegian hence handling this character differently
            if (text.ToUpper().Contains('�'))
            {
                StringBuilder firstPassBuilder = new();
                foreach (char ch in text)
                {
                    if (ch == '�' || ch == '�')
                    {
                        // NormalizationForm C doesn't convert � to A
                        firstPassBuilder.Append(ch.ToString().Normalize(NormalizationForm.FormC));
                    }
                    else
                    {
                        firstPassBuilder.Append(ch.ToString().Normalize(NormalizationForm.FormD));
                    }
                }

                normalizedText = firstPassBuilder.ToString();
            }
            else
            {
                normalizedText = text.Normalize(NormalizationForm.FormD);
            }

            StringBuilder secondPassBuilder = new();
            foreach (char ch in normalizedText)
            {
                // Unicode information of the characters, e.g. Uppercase, Lowercase, NonSpacingMark, etc.
                UnicodeCategory unicode = CharUnicodeInfo.GetUnicodeCategory(ch);

                if (unicode != UnicodeCategory.NonSpacingMark)
                {
                    // StringBuilder is appended with the characters that are not diacritics
                    secondPassBuilder.Append(ch);
                }
            }

            return secondPassBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
