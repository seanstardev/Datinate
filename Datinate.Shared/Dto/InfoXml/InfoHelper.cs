using com.RADIO.Datinate.RMVC.Shared;
using RadioLibCore.RadioDat;
using System.Xml;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace RadioLibCore.RadioResource
{
    public static class InfoHelper 
    {

        public const string InfoXmlVersion = "1.2";

        
        public static InfoVO? LoadInfoVO(string infoXmlFullpath) 
        {
            if (string.IsNullOrWhiteSpace(infoXmlFullpath) || !File.Exists(infoXmlFullpath))
                return null;

            XmlDocument doc = new XmlDocument();
            doc.Load(infoXmlFullpath);

            return LoadInfoVO_Internal(doc);
        }

        private static InfoVO? LoadInfoVO_Internal(XmlDocument? doc) 
        {
            if (doc == null) return null;

            XmlElement rootEl = doc.SelectSingleNode("root") as XmlElement;

            string version = "1.0";
            if (rootEl.HasAttribute("version") && !string.IsNullOrEmpty(rootEl.GetAttribute("version"))) 
                version = rootEl.GetAttribute("version");

            string resourceEnum = string.Empty;
            if (rootEl.HasAttribute("source") && !string.IsNullOrEmpty(rootEl.GetAttribute("source"))) 
            {
                resourceEnum = rootEl.GetAttribute("source");
            }

            InfoVO infoVO = new InfoVO(
                version
                , GetAtt(rootEl, "lookup")
                , GetInnerText(rootEl, "name")
                , resourceEnum
                , GetDescriptionVO(rootEl)
                , GetInnerText(rootEl, "developer")
                , GetInnerText(rootEl, "players")
                , GetReleaseVOs(rootEl)
                , GetInnerText(rootEl, "genre")
                , GetCreditVOs(rootEl)
                , GetAlsoKnownAs(rootEl)
                , GetAlsoOn(rootEl)
                , GetMiscPropertyVOs(rootEl)
                , GetInnerText(rootEl, "startup")                   // MAME only.
                , GetContributors(rootEl)
                , GetCompilations(rootEl)
                , GetEmulationVO(rootEl)                            // MAME only.
            );

            infoVO.SystemLookup = GetAtt(rootEl, "system_lookup");

            return infoVO;
        }

        static DescriptionVO GetDescriptionVO(XmlElement rootEl) 
        {
            XmlElement? el = rootEl.SelectSingleNode("description") as XmlElement;
            if (el == null) return DescriptionVO.EMPTY;

            string att;

            DescriptionVO.TEXT_FORMAT_ENUM format = DescriptionVO.TEXT_FORMAT_ENUM.text;
            att = el.GetAttribute("format");
        
            if (!string.IsNullOrEmpty(att)) 
            {
                format = DatinateHelper.GetEnumFromString<DescriptionVO.TEXT_FORMAT_ENUM>(att);
            }
            att = el.GetAttribute("boilerplate");
            bool boilerplate = false;
            
            if (!string.IsNullOrEmpty(att)) 
            {
                if (att.ToLower() == "true") boilerplate = true;
            }
            return new DescriptionVO(
                el.InnerText
                , format
                , boilerplate
            );
        }

        static string[] GetContributors(XmlElement rootEl) 
        {
            List<string> list = new List<string>();

            XmlNodeList nodeList = rootEl.SelectNodes("contributors/entry");
            XmlElement? entryEl;
            for (int i = 0; i < nodeList.Count; i++) 
            {
                entryEl = nodeList[i] as XmlElement;
                if (entryEl == null) continue;

                list.Add(entryEl.InnerText);
            }
            return list.ToArray();
        }

        static string[] GetAlsoOn(XmlElement rootEl) 
        {
            List<string> list = new List<string>();
            XmlNodeList nodeList = rootEl.SelectNodes("also_on/entry");

            XmlElement? entryEl;
            for (int i = 0; i < nodeList.Count; i++) 
            {
                entryEl = nodeList[i] as XmlElement;
                if (entryEl == null) continue;

                list.Add(entryEl.InnerText);
            }
            return list.ToArray();
        }
        static string[] GetAlsoKnownAs(XmlElement rootEl) 
        {
            List<string> list = new List<string>();
            XmlNodeList nodeList = rootEl.SelectNodes("also_known_as/entry");

            XmlElement? entryEl;
            for (int i = 0; i < nodeList.Count; i++) 
            {
                entryEl = nodeList[i] as XmlElement;
                if (entryEl == null) continue;

                list.Add(entryEl.InnerText);
            }
            return list.ToArray();
        }

        static MiscPropertyVO[] GetMiscPropertyVOs(XmlElement rootEl) 
        {
            List<MiscPropertyVO> list = new List<MiscPropertyVO>();

            XmlNodeList nodeList = rootEl.SelectNodes("miscellaneous/entry");
            XmlElement? entryEl;

            for (int i = 0; i < nodeList.Count; i++) 
            {
                entryEl = nodeList[i] as XmlElement;
                if (entryEl == null) continue;

                list.Add(
                    new MiscPropertyVO(
                        GetInnerText(entryEl, "name")
                        , GetInnerText(entryEl, "value")
                    )
                );
            }
            return list.ToArray();
        }
        static CreditVO[] GetCreditVOs(XmlElement rootEl) 
        {
            List<CreditVO> list = new List<CreditVO>();

            XmlNodeList nodeList = rootEl.SelectNodes("credits/entry");
            XmlElement? entryEl;
            XmlElement? entryName;
            
            for (int i = 0; i < nodeList.Count; i++) 
            {
                entryEl = nodeList[i] as XmlElement;
                if (entryEl == null) continue;

                entryName = entryEl.SelectSingleNode("name") as XmlElement;
                if (entryName == null) continue;

                list.Add(
                    new CreditVO(
                        GetInnerText(entryEl, "role")
                        , GetInnerText(entryEl, "name")
                        , GetInnerText(entryEl, "category")
                        , GetAtt(entryName, "url_part")
                    )
                );
            }
            return list.ToArray();
        }

        static EmulationVO GetEmulationVO(XmlElement rootEl) 
        {
            XmlElement? el = rootEl.SelectSingleNode("emulation/history") as XmlElement;
            if (el == null) return EmulationVO.EMPTY;

            if (el == null) return EmulationVO.EMPTY;

            string history = el.InnerText;
            if (string.IsNullOrEmpty(history)) 
                return EmulationVO.EMPTY;
            else 
                return new EmulationVO(history);
        }

        static CompilationVO[] GetCompilations(XmlElement rootEl) 
        {
            List<CompilationVO> list = new List<CompilationVO>();

            XmlNodeList nodeList = rootEl.SelectNodes("compilations/compilation");
            if (nodeList.Count == 0) return list.ToArray();

            XmlElement? compilationEl;

            for (int i = 0; i < nodeList.Count; i++) 
            {
                compilationEl = nodeList[i] as XmlElement;
                if (compilationEl == null) continue;

                list.Add(
                    new CompilationVO(
                        GetInnerText(compilationEl, "name")
                        , GetInnerText(compilationEl, "system")
                        , GetAtt(compilationEl, "url_part")));
            }

            return list.ToArray();
        }

        static ReleaseVO[] GetReleaseVOs(XmlElement rootEl) 
        {
            List<ReleaseVO> list = new List<ReleaseVO>();
            ReleaseVO releaseVO;

            XmlNodeList? nodeList = rootEl.SelectNodes("releases/release");

            if (nodeList == null)
                return list.ToArray();

            XmlElement? releaseEl;

            XmlElement? emulationStatusEl;

            for (int i = 0; i < nodeList.Count; i++) 
            {
                releaseEl = nodeList[i] as XmlElement;
                if (releaseEl == null) continue;

                emulationStatusEl = releaseEl.SelectSingleNode("emulation_status") as XmlElement;

                releaseVO = new ReleaseVO(
                    GetInnerText(releaseEl, "name")             // Some sources' Release data doesn't include names.
                    , GetAtt(releaseEl, "region")
                    , GetInnerText(releaseEl, "publisher")
                    , GetElAtt(releaseEl, "id", "product_id")
                    , GetElAtt(releaseEl, "id", "distribution_or_barcode")
                    , GetAtt(releaseEl, "date")
                    , GetAtt(releaseEl, "rating")               // I think this is region-specific age rating and such. May only be one source that does this.
                    , GetAtt(releaseEl, "players")              // Probably MAME only.
                    , GetInputVOs(releaseEl)                    // Probably MAME only.
                    , GetAttributePairVOs(emulationStatusEl)    // MAME only - and their xml has updated, so MRB must as well.
                    , GetAtt(releaseEl, "asset_key")
                    , GetAtt(releaseEl, "launch"));

                releaseVO.Distributor   = GetInnerText(releaseEl, "distributor");
                releaseVO.Comment       = GetInnerText(releaseEl, "comment");
                releaseVO.Medium        = GetInnerText(releaseEl, "medium");
                releaseVO.Type          = GetInnerText(releaseEl, "type");

                list.Add(releaseVO);
            }
            return list.ToArray();
        }

        static InputVO[] GetInputVOs(XmlElement releaseEl) 
        {
            List<InputVO> list = new List<InputVO>();

            XmlElement? inputEl = releaseEl.SelectSingleNode("input") as XmlElement;
            if (inputEl == null) return list.ToArray();

            XmlNodeList nodeList = inputEl.SelectNodes("entry");

            if (nodeList == null)
                return list.ToArray();

            XmlElement? el;

            for (int i = 0; i < nodeList.Count; i++) 
            {
                el = nodeList[i] as XmlElement;
                if (el == null) continue;

                list.Add(
                    new InputVO(GetAttributePairVOs(el))
                );
            }
            return list.ToArray();
        }

        static AttributePairVO[] GetAttributePairVOs(XmlElement? el) 
        {
            List<AttributePairVO> list = new List<AttributePairVO>();

            if (el == null) return list.ToArray();

            XmlAttributeCollection atts = el.Attributes;
            
            for (int i = 0; i < atts.Count; i++) 
            {
                list.Add(
                    new AttributePairVO(
                        atts[i].Name, atts[i].Value));
            }
            return list.ToArray();
        }

        static string GetElAtt(XmlElement parentEl, string elementName, string attName) 
        {
            XmlElement? el = parentEl.SelectSingleNode(elementName) as XmlElement;
            if (el == null) return "";

            if (!el.HasAttribute(attName)) return "";
            else return el.GetAttribute(attName);
        }

        static string GetAtt(XmlElement el, string attName) 
        {
            if (!el.HasAttribute(attName)) return "";
            else return el.GetAttribute(attName);
        }
        static string GetInnerText(XmlElement parentEl, string childElementName) 
        {
            XmlElement? el = parentEl.SelectSingleNode(childElementName) as XmlElement;
            if (el == null) return "";
            else return el.InnerText;
        }

        /**
         * Save the InfoVO as XML file on disc: 
         */
        public static void SaveInfoVO(
            InfoVO infoVO
            , string saveFullpath
            , string resourceEnum
        ) {
            
            SaveInfoFile(ConvertInfoVoToXml(infoVO, resourceEnum), saveFullpath);
        }

        public static XmlDocument? ConvertInfoVoToXml(InfoVO infoVO, string resourceEnum)
        {
            var doc = new XmlDocument();
            var xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(xmlDeclaration);

            var rootEl = doc.CreateElement("root");
            doc.AppendChild(rootEl);

            
            rootEl.SetAttribute("lookup", infoVO.Lookup);

            if (!string.IsNullOrEmpty(infoVO.SystemLookup))
                rootEl.SetAttribute("system_lookup", infoVO.SystemLookup);

            rootEl.SetAttribute("source", resourceEnum);
            rootEl.SetAttribute("version", InfoXmlVersion);

            AppendCDataElement(rootEl, "name", infoVO.Name, doc);

            var descriptionVO = infoVO.DescriptionVO!;
            var descriptionEl = AppendCDataElement(rootEl, "description", HtmlDecode(descriptionVO.Description)!, doc);
            if (descriptionVO.IsBoilerplate)
                descriptionEl.SetAttribute("boilerplate", "true");
            if (descriptionVO.TextFormatEnum != DescriptionVO.TEXT_FORMAT_ENUM.text)
                descriptionEl.SetAttribute("format", descriptionVO.TextFormatEnum.ToString());

            AppendCDataElement(rootEl, "developer", infoVO.Developer, doc);
            AppendCDataElement(rootEl, "players", infoVO.Players, doc);
            AppendCDataElement(rootEl, "genre", infoVO.Genre, doc);

            var alsoKnownAsEl = AppendElement(rootEl, "also_known_as", doc);
            foreach (var entry in infoVO.AlsoKnownAs!)
                AppendCDataElement(alsoKnownAsEl, "entry", entry, doc);

            var alsoOnEl = AppendElement(rootEl, "also_on", doc);
            foreach (var entry in infoVO.AlsoOn!)
                AppendCDataElement(alsoOnEl, "entry", entry, doc);

            if (!string.IsNullOrWhiteSpace(infoVO.StartupText))
                AppendCDataElement(rootEl, "startup", infoVO.StartupText, doc);

            var releasesEl = AppendElement(rootEl, "releases", doc);
            foreach (var releaseVO in infoVO.ReleaseVOs!)
            {
                var releaseEl = AppendElement(releasesEl, "release", doc);
                releaseEl.SetAttribute("date", releaseVO.ReleaseDate);
                releaseEl.SetAttribute("region", releaseVO.Region);

                releaseVO.Rating = HtmlDecode(releaseVO.Rating);
                if (!string.IsNullOrWhiteSpace(releaseVO.Rating))
                    releaseEl.SetAttribute("rating", releaseVO.Rating);

                if (!string.IsNullOrWhiteSpace(releaseVO.Players))
                    releaseEl.SetAttribute("players", releaseVO.Players);

                if (!string.IsNullOrWhiteSpace(releaseVO.MameName))
                    releaseEl.SetAttribute("launch", releaseVO.MameName);

                if (!string.IsNullOrWhiteSpace(releaseVO.AssetKey))
                    releaseEl.SetAttribute("asset_key", releaseVO.AssetKey);

                if (!string.IsNullOrWhiteSpace(releaseVO.Name))
                    AppendCDataElement(releaseEl, "name", releaseVO.Name, doc);

                releaseVO.ProductId = HtmlDecode(releaseVO.ProductId);
                releaseVO.DistributionOrBarcode = HtmlDecode(releaseVO.DistributionOrBarcode);

                if (!string.IsNullOrWhiteSpace(releaseVO.ProductId) || !string.IsNullOrWhiteSpace(releaseVO.DistributionOrBarcode))
                {
                    var idEl = AppendElement(releaseEl, "id", doc);

                    if (!string.IsNullOrWhiteSpace(releaseVO.ProductId))
                        idEl.SetAttribute("product_id", releaseVO.ProductId);

                    if (!string.IsNullOrWhiteSpace(releaseVO.DistributionOrBarcode))
                        idEl.SetAttribute("distribution_or_barcode", releaseVO.DistributionOrBarcode);
                }

                AppendCDataElement(releaseEl, "publisher", releaseVO.Publisher, doc);

                if (!string.IsNullOrWhiteSpace(releaseVO.Distributor))
                    AppendCDataElement(releaseEl, "distributor", releaseVO.Distributor, doc);

                if (!string.IsNullOrWhiteSpace(releaseVO.Comment))
                    AppendCDataElement(releaseEl, "comment", releaseVO.Comment, doc);

                if (!string.IsNullOrWhiteSpace(releaseVO.Medium))
                    AppendCDataElement(releaseEl, "medium", releaseVO.Medium, doc);

                if (!string.IsNullOrWhiteSpace(releaseVO.Type))
                    AppendCDataElement(releaseEl, "type", releaseVO.Type, doc);

                if (releaseVO.InputVOs.Length > 0)
                {
                    var inputEl = AppendElement(releaseEl, "input", doc);

                    foreach (var inputVO in releaseVO.InputVOs)
                    {
                        var entryEl = AppendElement(inputEl, "entry", doc);

                        foreach (var attributePairVO in inputVO.attributePairVOs)
                            entryEl.SetAttribute(attributePairVO.name, attributePairVO.value);
                    }
                }

                if (releaseVO.EmulationStatusVOs.Length > 0)
                {
                    var emulationStatusEl = AppendElement(releaseEl, "emulation_status", doc);

                    foreach (var attributePairVO in releaseVO.EmulationStatusVOs)
                        emulationStatusEl.SetAttribute(attributePairVO.name, attributePairVO.value);
                }
            }

            if (infoVO.CompilationVOs != null && infoVO.CompilationVOs.Length > 0)
            {
                var compilationsEl = AppendElement(rootEl, "compilations", doc);

                foreach (var compilationVO in infoVO.CompilationVOs)
                {
                    var compilationEl = AppendElement(compilationsEl, "compilation", doc);
                    AppendCDataElement(compilationEl, "name", compilationVO.Name, doc);
                    AppendCDataElement(compilationEl, "system", compilationVO.System, doc);
                }
            }

            var creditsEl = AppendElement(rootEl, "credits", doc);
            foreach (var creditVO in infoVO.CreditVOs!)
            {
                var entryEl = AppendElement(creditsEl, "entry", doc);
                AppendCDataElement(entryEl, "name", creditVO.Name, doc);
                AppendCDataElement(entryEl, "role", creditVO.Role, doc);

                if (!string.IsNullOrEmpty(creditVO.Category))
                    AppendCDataElement(entryEl, "category", creditVO.Category, doc);
            }

            var miscellaneousEl = AppendElement(rootEl, "miscellaneous", doc);
            foreach (var miscPropertyVO in infoVO.MiscPropertyVOs!)
            {
                var entryEl = AppendElement(miscellaneousEl, "entry", doc);
                AppendCDataElement(entryEl, "name", miscPropertyVO.name, doc);
                AppendCDataElement(entryEl, "value", miscPropertyVO.value, doc);
            }

            var contributorsEl = AppendElement(rootEl, "contributors", doc);
            foreach (var contributor in infoVO.Contributors!)
                AppendCDataElement(contributorsEl, "entry", contributor, doc);

            if (infoVO.EmulationVO != null && !string.IsNullOrEmpty(infoVO.EmulationVO.History))
            {
                var emulationEl = AppendElement(rootEl, "emulation", doc);
                AppendCDataElement(emulationEl, "history", infoVO.EmulationVO.History, doc);
            }

            return doc;
        }

        private static XmlElement AppendElement(XmlNode parent, string name, XmlDocument doc)
        {
            var element = doc.CreateElement(name);
            parent.AppendChild(element);
            return element;
        }

        private static XmlElement AppendCDataElement(XmlNode parent, string name, string value, XmlDocument doc)
        {
            var element = AppendElement(parent, name, doc);
            element.AppendChild(doc.CreateCDataSection(value));
            return element;
        }


        /**
         * Just removes white space... stops stuff like:
         * product_id=" "
         */
        private static string HtmlDecode(string? val) 
        {
            if (string.IsNullOrWhiteSpace(val)) return string.Empty;
            return System.Web.HttpUtility.HtmlDecode(val).Trim();
        }
        private static void SaveInfoFile(XmlDocument doc, string saveFullpath) 
        {
            Directory.CreateDirectory(Path.GetDirectoryName(saveFullpath));
            doc.Save(saveFullpath);
        }
    }
}
