using com.RADIO.Datinate.RMVC;
using com.RADIO.Datinate.RMVC.Shared;
using Datinate.Shared.Rb;
using RadioLibCore.RadioDat;
using System.Xml;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace Datinate.Rmvc.Proxy.delegates
{
    public class RadioDatImporterDelegate
    {
        private readonly XmlDocument doc;
        private readonly IReadOnlySet<string> descriptorDefinitions;
        private readonly IReadOnlyDictionary<DAT_GROUP_ENUM, FlagFilterSet> flagFilterSetByGroup;
        private readonly IReadOnlyDictionary<string, ISourceDefinition> sourceIdContentDictionary;

        public RadioDatImporterDelegate(
            string radioDatXmlFullpath, 
            IReadOnlySet<string> descriptorDefinitions,
            IReadOnlyDictionary<DAT_GROUP_ENUM, FlagFilterSet> flagFilterSetByGroup,
            IReadOnlyDictionary<string, ISourceDefinition> sourceIdContentDictionary)
        {
            doc = new XmlDocument();
            doc.Load(radioDatXmlFullpath);
            this.descriptorDefinitions = descriptorDefinitions;
            this.flagFilterSetByGroup = flagFilterSetByGroup;
            this.sourceIdContentDictionary = sourceIdContentDictionary;
        }

        public IReadOnlyDictionary<IGameFamily, IMediaCollectionImportExport?> Import()
        {
            var familyCollection = new Dictionary<IGameFamily, IMediaCollectionImportExport?>();

            var root = doc.DocumentElement;
            if (root == null || !root.Name.Equals("radio_dat", StringComparison.Ordinal))
                return familyCollection;

            XmlElement? collectionEl = root.SelectSingleNode("collection") as XmlElement;
            if (collectionEl == null)
                return familyCollection;

            var familyEls = collectionEl.SelectNodes("family");
            if (familyEls == null || familyEls.Count == 0)
                return familyCollection;

            foreach (XmlNode n in familyEls)
            {
                var familyEl = n as XmlElement;
                if (familyEl == null)
                    continue;

                var displayName = (familyEl.GetAttribute("display") ?? string.Empty).Trim();
                var isIgnored = IsTrue(familyEl.GetAttribute("exclude"));

                var checkedDescriptorCodes = new HashSet<string>(StringComparer.Ordinal);
                var familyNotes = string.Empty;
                ParseFamilyMeta(familyEl, checkedDescriptorCodes, descriptorDefinitions, out familyNotes);

                var assignedItems = new Dictionary<string, IAssignedMediaItem>(StringComparer.Ordinal);
                AddAssignedItemsFromSet(familyEl, "resource", assignedItems);
                AddAssignedItemsFromSet(familyEl, "media", assignedItems);
                AddAssignedItemsFromSet(familyEl, "support", assignedItems);

                var games = ParseGames(familyEl);

                var family = new BaseGameFamily(
                    displayName,
                    games,
                    Array.Empty<IDescriptor>(),
                    BaseResourceCollection.Empty,
                    isIgnored,
                    string.Empty);
                
                Dictionary<string, DAT_GROUP_ENUM> softwareIdDatGroupEnumDictionary = new Dictionary<string, DAT_GROUP_ENUM>();
                
                foreach (var sourceIdContent in sourceIdContentDictionary)
                    if (sourceIdContent.Value.CollectionSetEnum == COLLECTION_SET_ENUM.Software)
                        softwareIdDatGroupEnumDictionary[sourceIdContent.Key] = sourceIdContent.Value.DatGroupEnum;

                family = GameEntityNameBuilder.UpdateNames(family, flagFilterSetByGroup, softwareIdDatGroupEnumDictionary);

                var importedCollection = new ImportedMediaCollection(assignedItems, checkedDescriptorCodes, familyNotes);
                familyCollection.Add(family, importedCollection);
            }

            return familyCollection
                .OrderBy(kp => kp.Key.GetFamilyDisplayName(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(kp => kp.Key, kp => kp.Value);
        }

        private static bool IsTrue(string? value) =>
            value != null && value.Equals("true", StringComparison.OrdinalIgnoreCase);

        private static void ParseFamilyMeta(
            XmlElement familyEl, 
            HashSet<string> checkedDescriptorCodes, 
            IReadOnlySet<string> descriptorDefinitions, 
            out string familyNotes)
        {
            familyNotes = string.Empty;

            var metaEl = familyEl.SelectSingleNode("meta") as XmlElement;
            if (metaEl == null)
                return;

            var descriptorsEl = metaEl.SelectSingleNode("descriptors") as XmlElement;
            if (descriptorsEl != null)
            {
                foreach (XmlAttribute a in descriptorsEl.Attributes)
                {
                    if (descriptorDefinitions.Contains(a.Name) && IsTrue(a.Value))
                        _ = checkedDescriptorCodes.Add(a.Name);
                }
            }

            var commentEl = metaEl.SelectSingleNode("comment") as XmlElement;
            if (commentEl != null)
                familyNotes = (commentEl.InnerText ?? string.Empty).Trim();
        }

        private static void AddAssignedItemsFromSet(XmlElement familyEl, string setElementName, Dictionary<string, IAssignedMediaItem> assignedItems)
        {
            var setEl = familyEl.SelectSingleNode(setElementName) as XmlElement;
            if (setEl == null)
                return;

            var itemEls = setEl.SelectNodes("item|item_set");
            if (itemEls == null || itemEls.Count == 0)
                return;

            foreach (XmlNode n in itemEls)
            {
                var el = n as XmlElement;
                if (el == null)
                    continue;

                var sourceId = (el.GetAttribute("source_id") ?? string.Empty).Trim();
                var lookupName = (el.GetAttribute("name") ?? string.Empty).Trim();

                if (sourceId.Length == 0 || lookupName.Length == 0)
                    continue;

                if (assignedItems.ContainsKey(sourceId))
                    continue;

                assignedItems.Add(sourceId, new ImportAssignedMediaItem(sourceId, lookupName));
            }
        }

        private static IGame[] ParseGames(XmlElement familyEl)
        {
            var membersEl = familyEl.SelectSingleNode("members") as XmlElement;
            if (membersEl == null)
                return Array.Empty<IGame>();

            var memberNodes = membersEl.SelectNodes("member");
            if (memberNodes == null || memberNodes.Count == 0)
                return Array.Empty<IGame>();

            var memberEls = new List<XmlElement>(memberNodes.Count);
            XmlElement? parentEl = null;

            foreach (XmlNode n in memberNodes)
            {
                var el = n as XmlElement;
                if (el == null)
                    continue;

                memberEls.Add(el);

                if (parentEl == null && IsTrue(el.GetAttribute("parent")))
                    parentEl = el;
            }

            if (memberEls.Count == 0)
                return Array.Empty<IGame>();

            parentEl ??= memberEls[0];

            var games = new List<IGame>(memberEls.Count) { ParseGame(parentEl) };

            for (int i = 0; i < memberEls.Count; i++)
            {
                if (ReferenceEquals(memberEls[i], parentEl))
                    continue;

                games.Add(ParseGame(memberEls[i]));
            }

            return games.ToArray();
        }

        private static IGame ParseGame(XmlElement memberEl)
        {
            var gameName = (memberEl.GetAttribute("name") ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(gameName))
                gameName = (memberEl.GetAttribute("display") ?? string.Empty).Trim();

            var memberExcluded = IsTrue(memberEl.GetAttribute("exclude"));

            var parts = new List<IGamePart>();

            var partNodes = memberEl.SelectNodes("part");
            if (partNodes != null && partNodes.Count > 0)
            {
                foreach (XmlNode n in partNodes)
                {
                    var partEl = n as XmlElement;
                    if (partEl == null)
                        continue;

                    parts.Add(ParsePart(partEl, memberExcluded));
                }
            }

            return new BaseGame(gameName, parts.ToArray());
        }

        private static IGamePart ParsePart(XmlElement partEl, bool forceExclude)
        {
            var name = (partEl.GetAttribute("name") ?? string.Empty).Trim();
            var directoryId = (partEl.GetAttribute("source_id") ?? string.Empty).Trim();
            var tag = TrimToNull(partEl.GetAttribute("tag"));
            var launch = TrimToNull(partEl.GetAttribute("launch"));

            var fingerprint = (partEl.GetAttribute("fingerprint") ?? string.Empty).Trim();
            var checksums = fingerprint.Length == 0 ? Array.Empty<string>() : new[] { fingerprint };

            var aliases = ParseAliases(partEl, forceExclude);

            var part = new BaseGamePart(
                name,
                aliases,
                checksums,
                directoryId,
                name,
                launch,
                tag);

            part.Exclude = forceExclude || IsTrue(partEl.GetAttribute("exclude"));
            return part;
        }

        private static IGamePart[] ParseAliases(XmlElement partEl, bool forceExclude)
        {
            var aliasesEl = partEl.SelectSingleNode("aliases") as XmlElement;
            if (aliasesEl == null)
                return Array.Empty<IGamePart>();

            var aliasNodes = aliasesEl.SelectNodes("part");
            if (aliasNodes == null || aliasNodes.Count == 0)
                return Array.Empty<IGamePart>();

            var aliases = new List<IGamePart>(aliasNodes.Count);

            foreach (XmlNode n in aliasNodes)
            {
                var aliasEl = n as XmlElement;
                if (aliasEl == null)
                    continue;

                var name = (aliasEl.GetAttribute("name") ?? string.Empty).Trim();
                var directoryId = (aliasEl.GetAttribute("source_id") ?? string.Empty).Trim();
                var tag = TrimToNull(aliasEl.GetAttribute("tag"));
                var launch = TrimToNull(aliasEl.GetAttribute("launch"));

                var fingerprint = (aliasEl.GetAttribute("fingerprint") ?? string.Empty).Trim();
                var checksums = fingerprint.Length == 0 ? Array.Empty<string>() : new[] { fingerprint };

                var alias = new BaseGamePart(
                    name,
                    Array.Empty<IGamePart>(),
                    checksums,
                    directoryId,
                    name,
                    launch,
                    tag);

                alias.Exclude = forceExclude || IsTrue(aliasEl.GetAttribute("exclude"));
                aliases.Add(alias);
            }

            return aliases.ToArray();
        }

        private static string? TrimToNull(string? s)
        {
            if (s == null)
                return null;

            s = s.Trim();
            return s.Length == 0 ? null : s;
        }

        public sealed class ImportAssignedMediaItem : IAssignedMediaItem
        {
            public ImportAssignedMediaItem(string sourceId, string lookupName)
            {
                SourceId = sourceId;
                LookupName = lookupName;
            }

            public string SourceId { get; }
            public string EntryName => LookupName;
            public string LookupName { get; }
        }

        public sealed class ImportedMediaCollection : IMediaCollectionImportExport
        {
            public ImportedMediaCollection(
                Dictionary<string, IAssignedMediaItem> sourceIdAssignedItemDictionary,
                HashSet<string> checkedDescriptorCodes,
                string familyNotes)
            {
                SourceIdAssignedItemDictionary = sourceIdAssignedItemDictionary;
                CheckedDescriptorCodes = checkedDescriptorCodes;
                FamilyNotes = familyNotes;
            }

            public Dictionary<string, IAssignedMediaItem> SourceIdAssignedItemDictionary { get; }

            public bool IsEmptyForExport => false;

            public IReadOnlySet<string> CheckedDescriptorCodes { get; }

            public string FamilyNotes { get; }
        }
    }
}
