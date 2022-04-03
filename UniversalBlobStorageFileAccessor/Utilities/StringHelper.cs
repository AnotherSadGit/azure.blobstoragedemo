using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalBlobStorageFileAccessor.Utilities
{
    public static class StringHelper
    {
        public static string GetDebugDisplayText(string rawText)
        {
            if (rawText == null) return "[NULL]";
            if (rawText.Length == 0) return "[EMPTY TEXT]";
            if (rawText.Trim().Length == 0) return "[BLANK TEXT]";

            return rawText;
        }
        public static string GetUserFriendlyDisplayText(string? rawText)
        {
            if (string.IsNullOrWhiteSpace(rawText)) return "[VALUE NOT PROVIDED]";

            return rawText;
        }

        public static int? ConvertTextToNullableInt(string? text)
        {
            return ConvertTextToNullableInt(text, null);
        }

        public static int? ConvertTextToNullableInt(string? text, int? defaultValue)
        {
            if (int.TryParse(text, out int value))
            {
                return value;
            }

            return defaultValue;
        }

        public static decimal? ConvertTextToNullableDecimal(string? text)
        {
            return ConvertTextToNullableDecimal(text, null);
        }

        public static decimal? ConvertTextToNullableDecimal(string? text, decimal? defaultValue)
        {
            if (decimal.TryParse(text, out decimal value))
            {
                return value;
            }

            return defaultValue;
        }

        public static ReadOnlySpan<byte> ConvertStringToByteSpan(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            var bytes = Encoding.UTF8.GetBytes(text);

            return new ReadOnlySpan<byte>(bytes);
        }
    }
}
