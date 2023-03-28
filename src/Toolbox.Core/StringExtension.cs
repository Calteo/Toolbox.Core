using System.Diagnostics.CodeAnalysis;

namespace Toolbox
{
    /// <summary>
    /// Extension methods for <see cref="string"/>.
    /// </summary>
    public static class StringExtension
    {
        /// <summary>
        /// Checks if <see cref="string"/> is not empty
        /// </summary>
        /// <param name="text"></param>
        /// <returns><c>true</c> if string ist not empty, else <c>false</c></returns>
        public static bool NotEmpty([NotNullWhen(true), MaybeNullWhen(false)] this string text)
        {
            return !string.IsNullOrEmpty(text);
        }

        /// <summary>
        /// Checks if the <see cref="string"/> is empty (or null).
        /// </summary>
        /// <param name="text"></param>
        /// <returns><c>true</c> if string ist empty, else <c>false</c></returns>
        public static bool IsEmpty([NotNullWhen(false), MaybeNullWhen(true)] this string text)
        {
            return string.IsNullOrEmpty(text);
        }
    }
}
