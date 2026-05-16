using com.RADIO.Datinate.RMVC.Shared;
using Datinate.Shared.Rb;
using Datinate.Shared.Util;
using RadioLibCore.RadioDat;
using System.Diagnostics;
using System.Xml;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace Datinate.Rmvc.Proxy.delegates
{
    public class RadioDatExporterDelegate
    {
        public const string DAT_EXT_WITH_FULLSTOP = ".xml";
        public const string DAT_VERSION = "2026-02-05";

        private readonly RadioDatMeta datMeta;
        private readonly string radioDatFullpath;

        private readonly string radioDatNameWithoutExt;
        private readonly IReadOnlyDictionary<IGameFamily, IMediaCollectionImportExport?> familyCollection;
        private readonly IReadOnlyDictionary<string, ISourceDefinition> sourceIdContentDictionary;
        private readonly IReadOnlyDictionary<string, DatVO> sourceIdDatDictionary;

        private readonly Dictionary<COLLECTION_SET_ENUM, List<ISourceDefinition>> sourcesBySet =
            new Dictionary<COLLECTION_SET_ENUM, List<ISourceDefinition>>();

        private readonly Dictionary<COLLECTION_SET_ENUM, Dictionary<string, ISourceDefinition>> sourcesByIdBySet =
            new Dictionary<COLLECTION_SET_ENUM, Dictionary<string, ISourceDefinition>>();

        private readonly Dictionary<string, DatEntriesLookup> sourceIdByDatEntry =
            new Dictionary<string, DatEntriesLookup>(StringComparer.Ordinal);


        public RadioDatExporterDelegate(string projectName, string projectsPath)
            : this(
                radioDatNameWithoutExt: projectName,
                collection: new Dictionary<IGameFamily, IMediaCollectionImportExport?>(),
                sourceIdContentDictionary: new Dictionary<string, ISourceDefinition>(StringComparer.Ordinal),
                sourceIdDatDictionary: new Dictionary<string, DatVO>(StringComparer.Ordinal),
                datMeta: RadioDatMeta.Empty,
                radioDatPath: projectsPath) { }

        public RadioDatExporterDelegate(
            string radioDatNameWithoutExt,
            IReadOnlyDictionary<IGameFamily, IMediaCollectionImportExport?> collection,
            IReadOnlyDictionary<string, ISourceDefinition> sourceIdContentDictionary,
            IReadOnlyDictionary<string, DatVO> sourceIdDatDictionary,
            RadioDatMeta datMeta,
            string radioDatPath)
        {
            this.radioDatNameWithoutExt = radioDatNameWithoutExt;
            this.familyCollection = collection;
            this.sourceIdContentDictionary = sourceIdContentDictionary;
            this.sourceIdDatDictionary = sourceIdDatDictionary;
            this.datMeta = datMeta;

            radioDatFullpath = Path.Combine(radioDatPath, radioDatNameWithoutExt + DAT_EXT_WITH_FULLSTOP);

            foreach (var kvp in sourceIdDatDictionary)
                _ = sourceIdByDatEntry.TryAdd(kvp.Key, new DatEntriesLookup(kvp.Key, kvp.Value, sourceIdContentDictionary[kvp.Key]));

            BuildSourceIndexes();
        }

        private void BuildSourceIndexes()
        {
            foreach (var dto in sourceIdContentDictionary.Values)
            {
                if (!sourcesBySet.TryGetValue(dto.CollectionSetEnum, out var list))
                {
                    list = new List<ISourceDefinition>();
                    sourcesBySet[dto.CollectionSetEnum] = list;
                }

                list.Add(dto);

                if (!sourcesByIdBySet.TryGetValue(dto.CollectionSetEnum, out var byId))
                {
                    byId = new Dictionary<string, ISourceDefinition>(StringComparer.Ordinal);
                    sourcesByIdBySet[dto.CollectionSetEnum] = byId;
                }

                byId[dto.Id] = dto;
            }
        }

        public void Export()
        {
            XmlDocument doc = CreateXml();
            doc.Save(radioDatFullpath);
        }

        private XmlDocument CreateXml()
        {
            var doc = new XmlDocument();
            var xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);

            var rootEl = doc.CreateElement("radio_dat");
            doc.AppendChild(rootEl);

            rootEl.SetAttribute("name", radioDatNameWithoutExt);

            rootEl.SetAttribute("version", DAT_VERSION);
            rootEl.SetAttribute("created", DatinateHelper.GetDateTimeNowUtcString());
            doc.InsertBefore(xmlDeclaration, rootEl);

            AttachMeta(rootEl, doc);

            AttachSourcesAll(rootEl, doc);

            var collectionEl = doc.CreateElement("collection");
            rootEl.AppendChild(collectionEl);

            AttachFamilies(doc, collectionEl);

            return doc;
        }

        private void AttachMeta(XmlElement rootEl, XmlDocument doc)
        {
            if (!string.IsNullOrWhiteSpace(datMeta.Notes))
            {
                var metaEL = doc.CreateElement("meta");
                rootEl.AppendChild(metaEL);

                var commentEl = doc.CreateElement("comment");
                metaEL.AppendChild(commentEl);

                var cdata = doc.CreateCDataSection(datMeta.Notes.Trim());
                commentEl.AppendChild(cdata);
            }
        }

        void AttachFamilies(XmlDocument doc, XmlElement collectionEl)
        {
            foreach (var kvp in familyCollection)
            {
                var family = kvp.Key;
                var collection = kvp.Value;

                var familyEl = doc.CreateElement("family");
                collectionEl.AppendChild(familyEl);

                familyEl.SetAttribute("display", family.GetFamilyDisplayName());
                // NOTE: Ditching member_parent.attribute here.

                if (DatinateFamilyHelper.GetAllPartsIgnored(family))
                    familyEl.SetAttribute("exclude", "true");

                if (collection != null)
                {
                    bool hasDescriptors = collection.CheckedDescriptorCodes.Any();
                    bool hasNotes = !string.IsNullOrWhiteSpace(collection.FamilyNotes);

                    if (hasDescriptors || hasNotes)
                    {
                        var metaEl = doc.CreateElement("meta");
                        familyEl.AppendChild(metaEl);

                        if (hasDescriptors)
                        {
                            var descriptorsEl = doc.CreateElement("descriptors");
                            metaEl.AppendChild(descriptorsEl);

                            foreach (var desc in collection.CheckedDescriptorCodes)
                                descriptorsEl.SetAttribute(desc, "true");
                        }

                        if (hasNotes)
                        {
                            var commentEl = doc.CreateElement("comment");
                            metaEl.AppendChild(commentEl);

                            var cdata = doc.CreateCDataSection(collection.FamilyNotes!.Trim());
                            commentEl.AppendChild(cdata);
                        }
                    }
                }

                var membersEl = doc.CreateElement("members");
                familyEl.AppendChild(membersEl);

                var games = family.GetAllGames();
                var isFirst = true;

                foreach (var game in games)
                {
                    var memberEl = doc.CreateElement("member");
                    membersEl.AppendChild(memberEl);

                    memberEl.SetAttribute("display", game.GetNameWithoutExt());

                    if (DatinateFamilyHelper.GetAllPartsIgnored(game))
                        memberEl.SetAttribute("exclude", "true");
                    
                    if (isFirst)
                    {
                        isFirst = false;
                        memberEl.SetAttribute("parent", "true");
                    }

                    var parts = game.GetGameParts(false);
                    AttachGameParts(parts, memberEl, doc);
                }

                if (collection != null && collection.SourceIdAssignedItemDictionary.Any())
                {
                    AttachAux(collection, family, familyEl, doc);
                }
            }
        }

        private void AttachAux(IMediaCollectionImportExport collection, IGameFamily family, XmlElement familyEl, XmlDocument doc)
        {
            AttachAuxSet(collection, family, familyEl, doc, COLLECTION_SET_ENUM.Resource, "resource");
            AttachAuxSet(collection, family, familyEl, doc, COLLECTION_SET_ENUM.Media, "media");
            AttachAuxSet(collection, family, familyEl, doc, COLLECTION_SET_ENUM.Support, "support");
        }

        private void AttachAuxSet(
            IMediaCollectionImportExport collection,
            IGameFamily family,
            XmlElement familyEl,
            XmlDocument doc,
            COLLECTION_SET_ENUM setEnum,
            string elementName)
        {
            if (!sourcesByIdBySet.TryGetValue(setEnum, out var sourcesById) || sourcesById.Count == 0)
                return;

            var map = new Dictionary<IAssignedMediaItem, ISourceDefinition>(collection.SourceIdAssignedItemDictionary.Count);

            foreach (var a in collection.SourceIdAssignedItemDictionary.Values)
            {
                if (sourcesById.TryGetValue(a.SourceId, out var src))
                    map.Add(a, src);
            }

            if (map.Count == 0)
                return;

            var el = doc.CreateElement(elementName);
            familyEl.AppendChild(el);

            AttachAuxItem(doc, el, family, map);
        }

        private void AttachAuxItem(
            XmlDocument doc,
            XmlElement auxParentEl,
            IGameFamily family,
            Dictionary<IAssignedMediaItem, ISourceDefinition> map)
        {
            foreach (var kvp in map)
            {
                var sourceDef = kvp.Value;
                var assignment = kvp.Key;

                bool isResourceCollection = sourceDef.CollectionSetEnum == COLLECTION_SET_ENUM.Resource;

                var entryName = assignment.EntryName.Replace("\\", "/");
                var lookupName = assignment.LookupName.Replace("\\", "/");

                if (sourceIdByDatEntry.TryGetValue(sourceDef.Id, out var entrySet))
                {
                    var entryTrueName = BuildTrueEntryName(
                        entryName,
                        lookupName,
                        entrySet,
                        sourceDef);


                    if (!string.IsNullOrWhiteSpace(entryTrueName) && entrySet.EntryNameByRomLookupDictionary.TryGetValue(entryTrueName, out var romLookupList))
                    {
                        var entryDisplayName = entryName;

                        if (isResourceCollection)
                            entryDisplayName = R2DatWebFlag.Media.RemoveLookupFlagFromResourceItemName(entryName);

                        XmlElement parentEl = auxParentEl;
                        if (romLookupList.Count > 1)
                        {
                            var el = doc.CreateElement("item_set");
                            el.SetAttribute("type", sourceDef.Source);
                            el.SetAttribute("display", entryDisplayName);
                            el.SetAttribute("name", lookupName);
                            el.SetAttribute("source_id", sourceDef.Id);
                            auxParentEl.AppendChild(el);
                            parentEl = el;
                        }

                        foreach (var romLookup in romLookupList)
                        {
                            var el = doc.CreateElement("item");
                            parentEl.AppendChild(el);

                            string type_ = sourceDef.Source!;
                            if (isResourceCollection)
                            {
                                type_ = R2DatWebFlag.Media.GetTypeEnum(romLookup.Filename).ToString();
                            }

                            el.SetAttribute("type", type_);
                            el.SetAttribute("display", entryDisplayName);
                            el.SetAttribute("source_id", sourceDef.Id);
                            el.SetAttribute("name", lookupName);
                            el.SetAttribute("filename", romLookup.Filename);

                            if (!string.IsNullOrWhiteSpace(romLookup.Fingerprint))
                                el.SetAttribute("fingerprint", romLookup.Fingerprint);
                        }
                    }
                    else Debug.WriteLine($"[WARN] Exporter: Could not find '{entryTrueName}', '{entryName}', '{lookupName}' => '{sourceDef.Id}'");
                }
            }
        }
        private string? BuildTrueEntryName(string entryName, string lookupName, DatEntriesLookup entriesLookup, ISourceDefinition def)
        {
            entryName = entryName.Replace("\\", "/");
            lookupName = lookupName.Replace("\\", "/");

            if (def.DatSubset is { } subset && !string.IsNullOrWhiteSpace(subset.Entry))
            {
                var subsetEntry = subset.Entry.Replace("\\", "/");
                string build = string.Empty;

                build += subsetEntry + "/";

                if (!string.IsNullOrWhiteSpace(subset.Path))
                {
                    var subsetPath = subset.Path.Replace("\\", "/");
                    build += subsetPath + "/";
                }

                build += lookupName;
                return build;
            }
            else
            {
                return entryName;
            }
        }

        void AttachGameParts(IGamePart[] gameParts, XmlElement parentEl, XmlDocument doc)
        {
            foreach (var gamePart in gameParts)
            {
                var partEl = doc.CreateElement("part");
                parentEl.AppendChild(partEl);

                partEl.SetAttribute("name", gamePart.GetName());

                if (!string.IsNullOrWhiteSpace(gamePart.Tag))
                    partEl.SetAttribute("tag", gamePart.Tag);

                if (!string.IsNullOrWhiteSpace(gamePart.LaunchName))
                    partEl.SetAttribute("launch", gamePart.LaunchName);

                if (gamePart.Exclude)
                    partEl.SetAttribute("exclude", "true");

                partEl.SetAttribute("source_id", gamePart.GetDirectoryId());

                var fingerprint = FingerprintHelper.GetFingerprint(gamePart);

                if (!string.IsNullOrWhiteSpace(fingerprint))
                    partEl.SetAttribute("fingerprint", fingerprint);

                var gamePartAliases = gamePart.GetSoftwareAliases();

                if (gamePartAliases.Length > 0)
                {
                    var gamePartAliasesEl = doc.CreateElement("aliases");
                    partEl.AppendChild(gamePartAliasesEl);

                    foreach (var gamePartAlias in gamePartAliases)
                    {
                        var gamePartAliasEl = doc.CreateElement("part");
                        gamePartAliasesEl.AppendChild(gamePartAliasEl);

                        gamePartAliasEl.SetAttribute("name", gamePartAlias.GetName());

                        if (!string.IsNullOrWhiteSpace(gamePartAlias.Tag))
                            gamePartAliasEl.SetAttribute("tag", gamePartAlias.Tag);

                        if (!string.IsNullOrWhiteSpace(gamePartAlias.LaunchName))
                            gamePartAliasEl.SetAttribute("launch", gamePartAlias.LaunchName);

                        if (gamePartAlias.Exclude)
                            gamePartAliasEl.SetAttribute("exclude", "true");

                        gamePartAliasEl.SetAttribute("source_id", gamePartAlias.GetDirectoryId());
                    }
                }
            }
        }

        private void CreateSources(XmlElement sourcesEl, IReadOnlyList<ISourceDefinition> sources, XmlDocument doc)
        {
            foreach (var dto in sources)
            {
                var sourceEl = doc.CreateElement("source");
                sourcesEl.AppendChild(sourceEl);

                sourceEl.SetAttribute("id", dto.Id);
                sourceEl.SetAttribute("group", dto.DatGroupEnum.ToString());

                if (!string.IsNullOrWhiteSpace(dto.Source))
                    sourceEl.SetAttribute("type", dto.Source);

                var datEl = doc.CreateElement("dat");
                sourceEl.AppendChild(datEl);

                datEl.SetAttribute("fullpath", dto.DatFullpath);

                if (sourceIdDatDictionary.TryGetValue(dto.Id, out var dat))
                {
                    if (!string.IsNullOrWhiteSpace(dat.DatHeaderVO.Name))
                        datEl.SetAttribute("header_name", dat.DatHeaderVO.Name.Trim());

                    if (!string.IsNullOrWhiteSpace(dat.DatHeaderVO.Version))
                        datEl.SetAttribute("version", dat.DatHeaderVO.Version.Trim());

                    if (!string.IsNullOrWhiteSpace(dat.DatHeaderVO.Author))
                        datEl.SetAttribute("author", dat.DatHeaderVO.Author.Trim());
                }

                if (dto.DatSubset != null)
                {
                    var subsetEl = doc.CreateElement("subset");
                    datEl.AppendChild(subsetEl);

                    subsetEl.SetAttribute("entry", dto.DatSubset.Entry);

                    if (!string.IsNullOrWhiteSpace(dto.DatSubset.Path))
                        subsetEl.SetAttribute("path", dto.DatSubset.Path);
                }

                if (!string.IsNullOrWhiteSpace(dto.ContentPath))
                {
                    var contentEl = doc.CreateElement("content");
                    sourceEl.AppendChild(contentEl);

                    contentEl.SetAttribute("path", dto.ContentPath);
                }
            }
        }

        private void AttachSourcesAll(XmlElement rootEl, XmlDocument doc)
        {
            var parentSourcesEl = doc.CreateElement("sources");
            rootEl.AppendChild(parentSourcesEl);

            if (sourcesBySet.TryGetValue(COLLECTION_SET_ENUM.Software, out var software) && software.Count > 0)
            {
                var sourcesEl = doc.CreateElement("software");
                parentSourcesEl.AppendChild(sourcesEl);
                CreateSources(sourcesEl, software, doc);
            }

            if (sourcesBySet.TryGetValue(COLLECTION_SET_ENUM.Resource, out var resource) && resource.Count > 0)
            {
                var sourcesEl = doc.CreateElement("resource");
                parentSourcesEl.AppendChild(sourcesEl);
                CreateSources(sourcesEl, resource, doc);
            }

            if (sourcesBySet.TryGetValue(COLLECTION_SET_ENUM.Media, out var media) && media.Count > 0)
            {

                var sourcesEl = doc.CreateElement("media");
                parentSourcesEl.AppendChild(sourcesEl);
                CreateSources(sourcesEl, media, doc);
            }

            if (sourcesBySet.TryGetValue(COLLECTION_SET_ENUM.Support, out var support) && support.Count > 0)
            {
                var sourcesEl = doc.CreateElement("support");
                parentSourcesEl.AppendChild(sourcesEl);
                CreateSources(sourcesEl, support, doc);
            }
        }

        // Helper Classes
        //
        //
        private sealed class SourceEntryKeyComparer : IEqualityComparer<(string SourceId, string EntryName)>
        {
            public bool Equals((string SourceId, string EntryName) x, (string SourceId, string EntryName) y) =>
                StringComparer.Ordinal.Equals(x.SourceId, y.SourceId) &&
                StringComparer.Ordinal.Equals(x.EntryName, y.EntryName);

            public int GetHashCode((string SourceId, string EntryName) obj)
            {
                unchecked
                {
                    int h1 = StringComparer.Ordinal.GetHashCode(obj.SourceId);
                    int h2 = StringComparer.Ordinal.GetHashCode(obj.EntryName);
                    return (h1 * 397) ^ h2;
                }
            }
        }
        private sealed class RomLookup
        {
            public string Filename { get; }
            public string Fingerprint { get; }
            public RomLookup(string filename, string fingerprint)
            {
                Filename = filename;
                Fingerprint = fingerprint;
            }
        }
        private sealed class DatEntriesLookup
        {
            public string SourceId { get; }
            public DatVO Dat { get; }
            public IReadOnlyDictionary<string, IReadOnlyCollection<RomLookup>> EntryNameByRomLookupDictionary { get; }

            public DatEntriesLookup(string sourceId, DatVO dat, ISourceDefinition definition)
            {
                SourceId = sourceId;
                Dat = dat;

                var dic = new Dictionary<string, IReadOnlyCollection<RomLookup>>(StringComparer.Ordinal);
                var subset = definition.DatSubset;

                if (subset?.Entry != null)
                {
                    var subsetEntryName = subset.Entry.Replace("\\", "/");

                    string? subsetPathName = null;

                    if (!string.IsNullOrWhiteSpace(subset.Path))
                        subsetPathName = subset.Path.Replace("\\", "/");

                    foreach (var entry in dat.Entries)
                    {
                        var entryName = entry.Name.Replace("\\", "/");
                        
                        if (entryName == subsetEntryName)
                        {
                            foreach (var rom in entry.Roms)
                            {
                                var romName = rom.Name.Replace("\\", "/");

                                if (!string.IsNullOrWhiteSpace(subsetPathName))
                                {
                                    if (romName.StartsWith(subsetPathName + "/")) 
                                    {
                                        var filename = entryName + "/" + romName;
                                        _ = dic.TryAdd(
                                            filename, 
                                            [new RomLookup(filename, FingerprintHelper.GetFingerprint(rom.Sha1))]);
                                    }
                                }
                                else
                                {
                                    var filename = entryName + "/" + romName;
                                    _ = dic.TryAdd(
                                        filename, 
                                        [new RomLookup(filename, FingerprintHelper.GetFingerprint(rom.Sha1))]);
                                }
                            }
                        }
                    }
                }
                else
                {
                    foreach (var entry in dat.Entries)
                    {
                        var entryName = entry.Name.Replace("\\", "/");
                        List<RomLookup> romLookupList = [];

                        foreach (var rom in entry.Roms)
                        {
                            var romName = rom.Name.Replace("\\", "/");

                            var filename = entryName + "/" + romName;
         
                            romLookupList.Add(new RomLookup(
                                filename, FingerprintHelper.GetFingerprint(rom.Sha1)));
                        }
                        _ = dic.TryAdd(entryName, romLookupList);

                    }
                }

                EntryNameByRomLookupDictionary = dic;
            }
        }
    }
}
