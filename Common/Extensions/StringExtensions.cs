using System;
using System.IO;
using System.Text;


namespace HttpSample.Common.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Converts a plaintext string into a Base64 encoded string
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns></returns>
        public static string ToBase64(this string plainText)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));
        }

        /// <summary>
        /// Convert a string into a UTF-8 encoded stream
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Stream ToStream(this string value)
        {
            return ToStream(value, Encoding.UTF8);
        }

        /// <summary>
        /// Converts a string value into a stream using the specified Encoding
        /// </summary>
        /// <param name="value"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static Stream ToStream(this string value, Encoding encoding)
        {
            return new MemoryStream(encoding.GetBytes(value ?? string.Empty));
        }
    }
}
