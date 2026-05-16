using Datinate.Shared.Rb;
using RadioLibCore.RadioResource;

namespace com.RADIO.Datinate.RMVC.Shared
{
    public static class DatinateMediaResolver
    {
        public static string? CreateURI(string entry, RadioSourceDTO source, IReadOnlyList<R2DatResourceDTO> resources)
        {
            var fullpath = CreateFullPath(entry, source);
            if (fullpath == null)
                return null;

            return CreateURIFromFullPath(fullpath, source, resources);
        }

        public static string? CreateFullPath(string entry, RadioSourceDTO source)
        {
            if (string.IsNullOrWhiteSpace(source.ContentPath) || string.IsNullOrWhiteSpace(entry))
                return null;

            try
            {
                if (Path.IsPathRooted(entry))
                    return null;

                var baseDir = Path.GetFullPath(source.ContentPath);
                if (!Path.EndsInDirectorySeparator(baseDir))
                    baseDir += Path.DirectorySeparatorChar;

                if (!source.IsRadioResource)
                {
                    var fullpath = Path.GetFullPath(Path.Combine(baseDir, entry));

                    if (!fullpath.StartsWith(baseDir, StringComparison.OrdinalIgnoreCase))
                        return null;

                    if (File.Exists(fullpath))
                        return fullpath;

                    if (File.Exists(fullpath + ".zip"))
                        return fullpath + ".zip";

                    if (File.Exists(fullpath + ".7z"))
                        return fullpath + ".7z";

                    if (Directory.Exists(fullpath))
                    {
                        var firstFile =
                            Directory.EnumerateFiles(fullpath, "*", SearchOption.TopDirectoryOnly)
                                     .FirstOrDefault();

                        return firstFile;
                    }

                    var subPath = Path.GetFileNameWithoutExtension(entry);
                    var fallback = Path.GetFullPath(Path.Combine(baseDir, subPath, entry));

                    if (!fallback.StartsWith(baseDir, StringComparison.OrdinalIgnoreCase))
                        return null;

                    return File.Exists(fallback) ? fallback : null;
                }
                else
                {
                    var infoXml = Path.GetFullPath(Path.Combine(baseDir, entry, "info.xml"));

                    if (!infoXml.StartsWith(baseDir, StringComparison.OrdinalIgnoreCase))
                        return null;

                    return File.Exists(infoXml) ? infoXml : null;
                }
            }
            catch
            {
                return null;
            }
        }

        public static string? CreateURIFromFullPath(
            string fullpath, 
            RadioSourceDTO source, 
            IReadOnlyList<R2DatResourceDTO> resources)
        {
            if (string.IsNullOrWhiteSpace(fullpath))
                return null;

            if (!source.IsRadioResource)
            {
                try
                {
                    return new Uri(fullpath).AbsoluteUri;
                }
                catch
                {
                    return null;
                }
            }

            try
            {
                var info = InfoHelper.LoadInfoVO(fullpath);
                
                if (info == null) return null;

                string system = info.SystemLookup;
                string lookup = info.Lookup;

                R2DatResourceDTO? dto = null;
                foreach (var resource in resources)
                {
                    if (resource.Name == info.ResourceEnum)
                    {
                        dto = resource;
                        break;
                    }
                }

                if (dto == null)
                    return null;

                return dto.GetUrl(lookup, system, info.Name);
            }
            catch
            {
                return null;
            }
        }
    }
}
