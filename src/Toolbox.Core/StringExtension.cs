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
        public static bool NotEmpty(this string text)
        {
            return !string.IsNullOrEmpty(text);
        }

        /// <summary>
        /// Checks if the <see cref="string"/> is empty (or null).
        /// </summary>
        /// <param name="text"></param>
        /// <returns><c>true</c> if string ist empty, else <c>false</c></returns>
        public static bool IsEmpty(this string text)
        {
            return string.IsNullOrEmpty(text);
        }
    }
}
