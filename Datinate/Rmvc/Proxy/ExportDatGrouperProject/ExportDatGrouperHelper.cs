using System.Text;
using System.Xml;

namespace com.RADIO.Datinate.RMVC
{
    public static class ExportDatGrouperHelper
    {
        public static string HeaderDate
        {
            get
            {
                DateTime nowUtc = DateTime.UtcNow;

                DateTime roundedUpUtc =
                    nowUtc.Second == 0 && nowUtc.Millisecond == 0
                        ? new DateTime(nowUtc.Year, nowUtc.Month, nowUtc.Day, nowUtc.Hour, nowUtc.Minute, 0, DateTimeKind.Utc)
                        : new DateTime(nowUtc.Year, nowUtc.Month, nowUtc.Day, nowUtc.Hour, nowUtc.Minute, 0, DateTimeKind.Utc).AddMinutes(1);

                return roundedUpUtc.ToString("O", System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        public static void ExportDat(XmlDocument doc, string projectPath, string datFilenameWithoutExt)
        {
            Directory.CreateDirectory(projectPath);

            string filePath = Path.Combine(projectPath, datFilenameWithoutExt + ".xml");

            XmlWriterSettings settings = new XmlWriterSettings
            {
                Encoding = new UTF8Encoding(false),
                Indent = true,
                NewLineChars = Environment.NewLine,
                NewLineHandling = NewLineHandling.Replace
            };

            using XmlWriter writer = XmlWriter.Create(filePath, settings);
            doc.Save(writer);
        }

        public static XmlElement CreateDatafileDocument(
            XmlDocument doc,
            string name,
            string description,
            string date,
            string author,
            string? version = null,
            bool forcePackingUnzip = false)
        {
            XmlDeclaration declaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            _ = doc.AppendChild(declaration);

            XmlDocumentType docType = doc.CreateDocumentType(
                "datafile",
                "-//Logiqx//DTD ROM Management Datafile//EN",
                "http://www.logiqx.com/Dats/datafile.dtd",
                string.Empty);

            _ = doc.AppendChild(docType);

            XmlElement datafile = doc.CreateElement("datafile");
            _ = doc.AppendChild(datafile);

            XmlElement header = doc.CreateElement("header");
            _ = datafile.AppendChild(header);

            AppendElement(doc, header, "name", name);
            AppendElement(doc, header, "description", description);
            AppendElement(doc, header, "version", version);
            AppendElement(doc, header, "date", date);
            AppendElement(doc, header, "author", author);

            XmlElement clrmamepro = doc.CreateElement("clrmamepro");
            if (forcePackingUnzip)
                clrmamepro.SetAttribute("forcepacking", "unzip");
            else
                clrmamepro.SetAttribute("forcepacking", "zip");

            _ = header.AppendChild(clrmamepro);

            return datafile;
        }

        private static void AppendElement(XmlDocument doc, XmlElement parent, string elementName, string? value)
        {
            XmlElement element = doc.CreateElement(elementName);

            if (string.IsNullOrEmpty(value) == false)
                element.InnerText = value;

            _ = parent.AppendChild(element);
        }

        public static (ulong size, string crc, string md5, string sha1) BuildTextRomInfo(string content)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(content);

            ulong size = (ulong)bytes.LongLength;
            string crc = ComputeCrc32Hex(bytes);
            string md5 = Convert.ToHexString(System.Security.Cryptography.MD5.HashData(bytes)).ToLowerInvariant();
            string sha1 = Convert.ToHexString(System.Security.Cryptography.SHA1.HashData(bytes)).ToLowerInvariant();

            return (size, crc, md5, sha1);
        }

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

        public static void AppendMachineEntryToDatXml(
            XmlElement machinesRootEl,
            XmlElement machineEl,
            HashSet<string> allocatedMachineNames,
            string datKind)
        {
            string machineName = machineEl.GetAttribute("name");

            if (string.IsNullOrWhiteSpace(machineName))
            {
                System.Diagnostics.Debug.WriteLine("ERROR: Attempted to append a " + datKind + " DAT machine with no name.");
                throw new InvalidOperationException("Attempted to append a " + datKind + " DAT machine with no name.");
            }

            if (allocatedMachineNames.Add(machineName) == false)
            {
                System.Diagnostics.Debug.WriteLine("ERROR: Duplicate " + datKind + " DAT machine name detected during software export: " + machineName);
                throw new InvalidOperationException("Duplicate " + datKind + " DAT machine name detected during software export: " + machineName);
            }

            _ = machinesRootEl.AppendChild(machineEl);
        }
    }
}
