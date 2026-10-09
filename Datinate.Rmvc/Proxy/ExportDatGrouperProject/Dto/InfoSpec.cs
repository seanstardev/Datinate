using Datinate.Shared;

namespace Datinate.Rmvc.Delegate.ExportDatGrouperProject
{

    internal class InfoSpec
    {
        public ulong Size { get; }
        public string Crc { get; }
        public string Md5 { get; }
        public string Sha1 { get; }
        public string? ReleasesSourceId { get; }
        public IReadOnlySet20<string> AssetKeys { get; }

        public InfoSpec(
            ulong size,
            string crc,
            string md5,
            string sha1,
            string? releasesSourceId,
            IReadOnlySet20<string> assetKeys)
        {
            Size = size;
            Crc = crc;
            Md5 = md5;
            Sha1 = sha1;
            ReleasesSourceId = releasesSourceId;
            AssetKeys = ReadOnlySet20.From(
                    new HashSet<string>(assetKeys, StringComparer.OrdinalIgnoreCase));
        }

        public static InfoSpec FromFile(string filePath)
        {
            return FromFile(
                filePath,
                null,
                ReadOnlySet20.Empty<string>());
        }

        public static InfoSpec FromFile(
            string filePath,
            string? releasesSourceId,
            IReadOnlySet20<string> assetKeys)
        {
            byte[] bytes = File.ReadAllBytes(filePath);

            return new InfoSpec(
                (ulong)bytes.LongLength,
                DatinateRmvcHelper.ComputeCrc32Hex(bytes),
                DatinateRmvcHelper.ComputeMd5Hex(bytes),
                DatinateRmvcHelper.ComputeSha1Hex(bytes),
                releasesSourceId,
                assetKeys);
        }
    }
}
