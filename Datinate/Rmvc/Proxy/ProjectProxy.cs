using com.RADIO.Datinate.RMVC.Shared;
using RMVC;
using System.Xml;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC
{
    public class ProjectProxy : RModel 
    {
        const string XML_VERSION = "1.0";

        private string? projectsPath;


        public void SetProjectRootPath(string projectRoot)
        {
            projectsPath = Path.Combine(projectRoot, "Project");
            Directory.CreateDirectory(projectsPath);
        }

        public void SaveProject(DatGrouperProjectDTO project) 
        {
            if (string.IsNullOrWhiteSpace(projectsPath)) return;

            string projectFullpath = Path.Combine(projectsPath, project.ProjectName + ".xml");
            
            if (File.Exists(projectFullpath))
                File.Delete(projectFullpath);

            XmlDocument doc = new XmlDocument();

            XmlElement rootEl = (doc.AppendChild(doc.CreateElement("root")) as XmlElement)!;
            rootEl.SetAttribute("version", XML_VERSION);

            XmlElement datsEl = (rootEl.AppendChild(doc.CreateElement("dats")) as XmlElement)!;

            XmlElement gamesEl = (datsEl.AppendChild(doc.CreateElement("games")) as XmlElement)!;
            XmlElement mediaEl = (datsEl.AppendChild(doc.CreateElement("media")) as XmlElement)!;

            AddDats(
                gamesEl, 
                "include", 
                doc, 
                project.SoftwareEntries.ToArray(),
                false
            );
            AddDats(
                gamesEl, 
                "ignore", 
                doc, 
                project.SoftwareIgnoreEntries.ToArray(),
                false
            );
            AddDats(
                mediaEl, 
                "include", 
                doc, 
                project.AuxEntries.ToArray(),
                true
            );

            // Comment
            XmlElement commentEl = (rootEl.AppendChild(doc.CreateElement("comment")) as XmlElement)!;
            XmlCDataSection cData = doc.CreateCDataSection(string.IsNullOrWhiteSpace(project.Comment) 
                ? string.Empty 
                : project.Comment);

            commentEl.AppendChild(cData);

            // Settings:
            if (project.ScoringMediaTypes.Any() || project.ExcludedDescriptorCodes.Any())
            {
                XmlElement settingsEl = (rootEl.AppendChild(doc.CreateElement("settings")) as XmlElement)!;

                if (project.ExcludedDescriptorCodes.Any())
                {
                    var descriptorsEl = (XmlElement)settingsEl.AppendChild(doc.CreateElement("excluded_descriptors"))!;
                    descriptorsEl.InnerText = string.Join(
                        ", ",
                        project.ExcludedDescriptorCodes.Select(s => s?.Trim())
                        .Where(s => !string.IsNullOrWhiteSpace(s)));
                }

                if (project.ScoringMediaTypes.Any())
                {
                    var scoringMediaEl = (XmlElement)settingsEl.AppendChild(doc.CreateElement("scoring_media"))!;
                    scoringMediaEl.InnerText = string.Join(
                        ", ",
                        project.ScoringMediaTypes.Select(s => s?.Trim())
                        .Where(s => !string.IsNullOrWhiteSpace(s)));
                }
            }
            var exportEl = (XmlElement)rootEl.AppendChild(doc.CreateElement("export"))!;

            AddSoftwareExportOptions(
                exportEl,
                doc,
                project.ExportSoftwareOptionsDTO);

            AddMediaExports(
                exportEl,
                doc,
                project.MediaExports);

            if (exportEl.ChildNodes.Count == 0)
                rootEl.RemoveChild(exportEl);

            doc.Save(projectFullpath);

            MessageBox.Show(
                "The Project '" + project.ProjectName + "' has been Saved."
                , "OK"
                , MessageBoxButtons.OK
                , MessageBoxIcon.Information
            );
            base.ExecuteCommand(new SetDatGrouperLoaderViewCmd(project.ProjectName, true));
        }
        private void AddSoftwareExportOptions(
            XmlElement exportEl,
            XmlDocument doc,
            ExportSoftwareOptionsDTO softwareOptions)
        {
            var softwareEl = (XmlElement)exportEl.AppendChild(doc.CreateElement("software"))!;

            softwareEl.SetAttribute("export_1g1r", softwareOptions.ExportAs1G1R ? "true" : "false");
            softwareEl.SetAttribute("skip_scoring_exempt_families", softwareOptions.SkipScoringExemptFamilies ? "true" : "false");
            softwareEl.SetAttribute("skip_excluded_games", softwareOptions.SkipExcludedGames ? "true" : "false");
            softwareEl.SetAttribute("export_with_m3us", softwareOptions.ExportM3Us ? "true" : "false");
        }
        private void AddMediaExports(
            XmlElement exportEl,
            XmlDocument doc,
            IReadOnlyList<DatGrouperMediaExportEntryDTO> mediaExports)
        {
            if (mediaExports.Count == 0)
                return;

            var mediaContainerEl = (XmlElement)exportEl.AppendChild(doc.CreateElement("media"))!;

            foreach (var mediaExport in mediaExports)
            {
                if (mediaExport.MediaTypeEnum == MEDIA_TYPE_ENUM.NOT_SET ||
                    mediaExport.MediaTypeEnum == MEDIA_TYPE_ENUM.Unspecified)
                    continue;

                if (mediaExport.Sources.Count == 0)
                    continue;

                var mediaEl = (XmlElement)mediaContainerEl.AppendChild(doc.CreateElement("media"))!;
                mediaEl.SetAttribute("media_type", mediaExport.MediaTypeEnum.ToString());

                var seenSourceIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var source in mediaExport.Sources)
                {
                    if (string.IsNullOrWhiteSpace(source.SourceId))
                        continue;

                    var sourceId = source.SourceId.Trim();

                    if (!seenSourceIds.Add(sourceId))
                        continue;

                    var entryEl = (XmlElement)mediaEl.AppendChild(doc.CreateElement("entry"))!;
                    entryEl.SetAttribute("id", sourceId);
                    entryEl.SetAttribute("include", source.Include ? "true" : "false");
                }

                if (mediaEl.ChildNodes.Count == 0)
                    mediaContainerEl.RemoveChild(mediaEl);
            }

            if (mediaContainerEl.ChildNodes.Count == 0)
                exportEl.RemoveChild(mediaContainerEl);
        }
        public DatGrouperProjectDTO? LoadProject(string projectName)
        {
            if (string.IsNullOrWhiteSpace(projectsPath)) return null;

            var projectFullpath = Path.Combine(projectsPath, projectName + ".xml");
            if (!File.Exists(projectFullpath)) return null;
            
            var doc = new XmlDocument();
            doc.Load(projectFullpath);
            var project = ConvertXml(doc, Path.GetFileNameWithoutExtension(projectFullpath));

            return project;
        }

        public bool AnyProjectsExist()
        {
            if (string.IsNullOrWhiteSpace(projectsPath)) return false;

            string[] files = Directory.GetFiles(projectsPath, "*.xml");
            return files.Any();
        }
        public DatGrouperProjectDTO[] LoadAllProjectVOs() 
        {
            if (string.IsNullOrWhiteSpace(projectsPath)) return [];

            string[] files = Directory.GetFiles(projectsPath, "*.xml");

            List<DatGrouperProjectDTO> list = new List<DatGrouperProjectDTO>();

            for (int i = 0; i < files.Length; i++) 
            {
                var doc = new XmlDocument();
                doc.Load(files[i]);
                var projectVO = ConvertXml(doc, Path.GetFileNameWithoutExtension(files[i]));
                list.Add(projectVO);
            }

            return list.ToArray();
        }

        void AddDats(
            XmlElement parentEl, 
            string part, 
            XmlDocument doc, 
            DatGrouperProjectEntry[] projectEntries,
            bool isMedia) 
        {
            XmlElement partEl;

            if (!string.IsNullOrWhiteSpace(part))
                partEl = (parentEl.AppendChild(doc.CreateElement(part)) as XmlElement)!;
            else
                partEl = parentEl;
            
            for (int i = 0; i < projectEntries.Length; i++) 
            {
                XmlElement? datEl = null;

                var projectEntry = projectEntries[i];

                if (!isMedia)
                    datEl = (partEl.AppendChild(doc.CreateElement("dat")) as XmlElement)!;
                else if (projectEntry.CollectionSetEnum == COLLECTION_SET_ENUM.Media)
                    datEl = (partEl.AppendChild(doc.CreateElement("media_dat")) as XmlElement)!;
                else if (projectEntry.CollectionSetEnum == COLLECTION_SET_ENUM.Resource)
                    datEl = (partEl.AppendChild(doc.CreateElement("resource_dat")) as XmlElement)!;

                if (datEl == null) return;

                datEl.SetAttribute("dat_type", projectEntry.DatGroupEnum.ToString());

                if (!string.IsNullOrWhiteSpace(projectEntry.FriendlyName))
                    datEl.SetAttribute("friendly_name", projectEntry.FriendlyName);

                if (!string.IsNullOrWhiteSpace(projectEntry.InternalDescriptor))
                    datEl.SetAttribute("internal_descriptor", projectEntry.InternalDescriptor);

                datEl.SetAttribute("dat_fullpath", projectEntry.DatFullpath);

                if (!string.IsNullOrWhiteSpace(projectEntry.ExpressionsXmlFullpath))
                    datEl.SetAttribute("expressions_fullpath", projectEntry.ExpressionsXmlFullpath);

                if (!string.IsNullOrWhiteSpace(projectEntry.ContentPath))
                    datEl.SetAttribute("content_path", projectEntry.ContentPath);

                if (projectEntry.HideInUi)
                    datEl.SetAttribute("hide_in_ui", "true");


                // TODO: DatSubsetFilter needs a rewrite
                if (projectEntry.DatSubsetFilter == null || string.IsNullOrWhiteSpace(projectEntry.DatSubsetFilter.Entry) || projectEntry.DatSubsetFilter.Entry.ToLower() == "[none]")
                {
                }
                else 
                { 
                    var subsetEl = (datEl.AppendChild(doc.CreateElement("subset")) as XmlElement)!;
                    subsetEl.SetAttribute("item", projectEntry.DatSubsetFilter.Entry.Trim());
                    
                    if (!string.IsNullOrWhiteSpace(projectEntry.DatSubsetFilter.Path))
                        subsetEl.SetAttribute("folder", projectEntry.DatSubsetFilter.Path.Trim());
                }

                if (!string.IsNullOrWhiteSpace(projectEntry.Comment)) 
                {
                    var commentEl = (datEl.AppendChild(doc.CreateElement("comment")) as XmlElement)!;
                    XmlCDataSection cData = doc.CreateCDataSection(projectEntry.Comment.Trim());
                    commentEl.AppendChild(cData);
                }
            }
        }

        private DatGrouperProjectDTO ConvertXml(XmlDocument doc, string projectFilenameWithoutExt) 
        {
            return new DatGrouperProjectDTO(
                projectFilenameWithoutExt,
                GetDatSet(doc, "include", true),
                GetDatSet(doc, "ignore", true),
                GetDatSet(doc, "include", false),
                GetComment(doc),
                GetExcludedDescriptors(doc),
                GetScoringMedia(doc),
                GetSoftwareExportOptions(doc),
                GetMediaExports(doc));
        }
        private ExportSoftwareOptionsDTO GetSoftwareExportOptions(XmlDocument doc)
        {
            var defaultDTO = ExportSoftwareOptionsDTO.CreateDefault();
            var exportEl = doc.SelectSingleNode("/root/export/software") as XmlElement;

            if (exportEl == null) return defaultDTO;

            bool export_1g1r = defaultDTO.ExportAs1G1R;
            _ = bool.TryParse(exportEl.GetAttribute("export_1g1r"), out export_1g1r);

            bool skip_scoring_exempt_families = defaultDTO.SkipScoringExemptFamilies;
            _ = bool.TryParse(exportEl.GetAttribute("skip_scoring_exempt_families"), out skip_scoring_exempt_families);

            bool skip_excluded_games = defaultDTO.SkipExcludedGames;
            _ = bool.TryParse(exportEl.GetAttribute("skip_excluded_games"), out skip_excluded_games);

            bool export_with_m3us = defaultDTO.ExportM3Us;
            _ = bool.TryParse(exportEl.GetAttribute("export_with_m3us"), out export_with_m3us);

            return new ExportSoftwareOptionsDTO(
                export_1g1r, 
                skip_scoring_exempt_families, 
                skip_excluded_games, 
                export_with_m3us);
        }

        private IReadOnlyList<DatGrouperMediaExportEntryDTO> GetMediaExports(XmlDocument doc)
        {
            var list = new List<DatGrouperMediaExportEntryDTO>();
            var exportMediaNodes = doc.SelectNodes("/root/export/media/media");

            if (exportMediaNodes == null || exportMediaNodes.Count == 0)
                return list;

            foreach (var setNode in exportMediaNodes)
            {
                if (setNode is not XmlElement setEl)
                    continue;

                var mediaTypeEnum = DatinateHelper.GetEnumFromString<MEDIA_TYPE_ENUM>(
                    setEl.GetAttribute("media_type"),
                    MEDIA_TYPE_ENUM.NOT_SET);

                if (mediaTypeEnum == MEDIA_TYPE_ENUM.NOT_SET)
                    continue;

                var mediaEntryNodes = setEl.SelectNodes("entry");

                if (mediaEntryNodes == null || mediaEntryNodes.Count == 0)
                    continue;

                var entriesList = new List<DatGrouperMediaExportSourceDTO>();
                var seenSourceIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var entryNode in mediaEntryNodes)
                {
                    if (entryNode is not XmlElement entryEl)
                        continue;

                    var sourceIdVal = entryEl.GetAttribute("id");

                    if (string.IsNullOrWhiteSpace(sourceIdVal))
                        continue;

                    if (!seenSourceIds.Add(sourceIdVal))
                        continue;

                    bool includeVal = false;
                    _ = bool.TryParse(entryEl.GetAttribute("include"), out includeVal);

                    entriesList.Add(new DatGrouperMediaExportSourceDTO(
                        sourceIdVal,
                        includeVal));
                }

                if (entriesList.Count > 0)
                    list.Add(new DatGrouperMediaExportEntryDTO(mediaTypeEnum, entriesList));
            }

            return list;
        }
        private IReadOnlySet<string> GetExcludedDescriptors(XmlDocument doc)
        {
            var descriptors = doc.SelectSingleNode("/root/settings/excluded_descriptors") as XmlElement;

            var list = (descriptors?.InnerText ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => s.Length > 0)
                .ToHashSet();

            return list;
        }
        private IReadOnlySet<string> GetScoringMedia(XmlDocument doc)
        {
            var media = doc.SelectSingleNode("/root/settings/scoring_media") as XmlElement;

            var list = (media?.InnerText ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => s.Length > 0)
                .ToHashSet();

            return list;
        }
        private string GetComment(XmlDocument doc) 
        {
            XmlElement? el = doc.SelectSingleNode("root/comment") as XmlElement;
            
            if (el == null) 
                return string.Empty;
            else 
                return el.InnerText.Trim();
        }


        private DatGrouperProjectEntry[] GetDatSet(
            XmlDocument doc, 
            string part,
            bool isSoftware) 
        {
            XmlNodeList? nodeList = null;

            if (isSoftware)
                nodeList = doc.SelectNodes("root/dats/games/" + part + "/dat");
            else
                nodeList = doc.SelectNodes("root/dats/media/" + part + "/*[self::media_dat or self::resource_dat]");

            if (nodeList == null || nodeList.Count == 0)
                return Array.Empty<DatGrouperProjectEntry>();
            
            List<DatGrouperProjectEntry> list = new List<DatGrouperProjectEntry>();

            for (int i = 0; i < nodeList.Count; i++) 
            {
                var el = nodeList[i] as XmlElement;
                
                if (el == null) continue;

                var subsetEl = el.SelectSingleNode("subset") as XmlElement;

                DatSubsetFilter? subset;
                if (subsetEl == null) 
                    subset = null;
                else 
                {
                    var item = subsetEl.GetAttribute("item");
                    if (string.IsNullOrWhiteSpace(item) || item.ToLower().Trim() == "[none]") { 
                        subset = null;
                    }
                    else
                    {
                        subset = new DatSubsetFilter(
                            subsetEl.GetAttribute("item")
                            , subsetEl.HasAttribute("folder") ? subsetEl.GetAttribute("folder") : null
                        );
                    }
                }

                string? comment = null;
                var commentEl = el.SelectSingleNode("comment") as XmlElement;
                if (commentEl != null) 
                    comment = commentEl.InnerText.Trim();

                DAT_GROUP_ENUM datEnum = DatinateHelper.GetEnumFromString<DAT_GROUP_ENUM>(el.GetAttribute("dat_type"));
                string internalDescriptor = el.HasAttribute("internal_descriptor") ? el.GetAttribute("internal_descriptor") : string.Empty;
                string friendly = el.HasAttribute("friendly_name") ? el.GetAttribute("friendly_name") : string.Empty;

                string? contentPath = el.HasAttribute("content_path") ? el.GetAttribute("content_path") : null;
                bool hideInUi = el.HasAttribute("hide_in_ui") ? el.GetAttribute("hide_in_ui").ToLower() == "true" : false;

                COLLECTION_SET_ENUM collectionSetEnum = COLLECTION_SET_ENUM.NOT_SET;
                
                if (isSoftware)
                    collectionSetEnum = COLLECTION_SET_ENUM.Software;
                else if (string.Equals(el.LocalName, "media_dat", StringComparison.OrdinalIgnoreCase))
                    collectionSetEnum = COLLECTION_SET_ENUM.Media;
                else if (string.Equals(el.LocalName, "resource_dat", StringComparison.OrdinalIgnoreCase))
                    collectionSetEnum = COLLECTION_SET_ENUM.Resource;

                var pointer = DatinateHelper.BuildPointerId(
                        collectionSetEnum,
                        datEnum,
                        internalDescriptor,
                        subset,
                        friendly);
                
                var vo = new DatGrouperProjectEntry(
                    collectionSetEnum,
                    el.GetAttribute("dat_fullpath"), 
                    pointer, 
                    el.HasAttribute("expressions_fullpath") ? el.GetAttribute("expressions_fullpath") : string.Empty, 
                    datEnum, 
                    friendly, 
                    subset, 
                    internalDescriptor, 
                    comment,
                    contentPath,
                    hideInUi);

                list.Add(vo);
            }

            return list.ToArray();
        }
    }
}
