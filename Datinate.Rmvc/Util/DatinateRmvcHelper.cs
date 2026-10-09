namespace Datinate.Rmvc
{
    internal static class DatinateRmvcHelper
    {
        public static string ComputeCrc32Hex(byte[] bytes)
        {
            uint crc = 0xFFFFFFFF;

            foreach (byte value in bytes)
            {
                crc ^= value;

                for (int i = 0; i < 8; i++)
                {
                    if ((crc & 1) != 0)
                        crc = (crc >> 1) ^ 0xEDB88320u;
                    else
                        crc >>= 1;
                }
            }

            crc ^= 0xFFFFFFFF;

            return crc.ToString("x8");
        }
    
        public static string ToLowerHex(byte[] bytes)
        {
            const string digits = "0123456789abcdef";
            var chars = new char[bytes.Length * 2];

            for (int i = 0; i < bytes.Length; i++)
            {
                byte value = bytes[i];
                chars[i * 2] = digits[value >> 4];
                chars[i * 2 + 1] = digits[value & 0x0F];
            }

            return new string(chars);
        }

        public static string ComputeMd5Hex(byte[] bytes)
        {
            using var md5 = System.Security.Cryptography.MD5.Create();
            return ToLowerHex(md5.ComputeHash(bytes));
        }

        public static string ComputeSha1Hex(byte[] bytes)
        {
            using var sha1 = System.Security.Cryptography.SHA1.Create();
            return ToLowerHex(sha1.ComputeHash(bytes));
        }
    }
}
