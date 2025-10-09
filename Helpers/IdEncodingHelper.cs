namespace Project02.Helpers
{
    public static class IdEncodingHelper
    {
        public static string EncodeId(long id)
        {
            var bytes = BitConverter.GetBytes(id);
            return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        public static long DecodeId(string encoded)
        {
            encoded = encoded.Replace('-', '+').Replace('_', '/');
            switch (encoded.Length % 4)
            {
                case 2: encoded += "=="; break;
                case 3: encoded += "="; break;
            }
            var bytes = Convert.FromBase64String(encoded);
            return BitConverter.ToInt64(bytes, 0);
        }
    }

}
