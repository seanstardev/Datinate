using System.Security.Cryptography;

namespace com.RADIO.Datinate.RMVC
{
    public sealed class RawFileData
    {
        public string RawText { get; }
        public string? Sha1 { get; }

        public RawFileData(string raw, string? sha1)
        {
            RawText = raw;
            Sha1 = sha1;
        }
    }

    public static class DatLoadingHelper
    {
        private const int BufferSize = 128 * 1024;

        public static string LoadDat(string datPath)
        {
            return GetFileData(datPath, true).RawText;
        }

        public static RawFileData GetFileData(string datFullpath, bool skipSha1Calculation)
        {
            if (skipSha1Calculation)
                return new RawFileData(ReadText(datFullpath), null);

            return ReadTextAndSha1(datFullpath);
        }

        private static string ReadText(string filePath)
        {
            using var fs = OpenReadStream(filePath);
            using var sr = new StreamReader(
                fs,
                detectEncodingFromByteOrderMarks: true,
                bufferSize: BufferSize);

            return sr.ReadToEnd();
        }

        private static RawFileData ReadTextAndSha1(string filePath)
        {
            using var sha1 = SHA1.Create();

            string text;

            using (var fs = OpenReadStream(filePath))
            using (var cryptoStream = new CryptoStream(fs, sha1, CryptoStreamMode.Read))
            using (var sr = new StreamReader(
                cryptoStream,
                detectEncodingFromByteOrderMarks: true,
                bufferSize: BufferSize))
            {
                text = sr.ReadToEnd();
            }

            string sha1Text = Convert
                .ToHexString(sha1.Hash ?? [])
                .ToLowerInvariant();

            return new RawFileData(text, sha1Text);
        }

        private static FileStream OpenReadStream(string filePath)
        {
            return new FileStream(
                filePath,
                new FileStreamOptions
                {
                    Mode = FileMode.Open,
                    Access = FileAccess.Read,
                    Share = FileShare.Read,
                    BufferSize = BufferSize,
                    Options = FileOptions.SequentialScan
                });
        }
    }
}