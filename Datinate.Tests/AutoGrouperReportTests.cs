using Datinate.Rmvc.Dto;
using Datinate.Shared;
using Datinate.Shared.Dat;
using Datinate.Shared.Radio;
using static Datinate.Shared.DatFilterHelper;
using static Datinate.Shared.DatinateEnums;

namespace Datinate.Tests
{
    public class AutoGrouperReportTests
    {
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ReportsRemainAssociatedWithTheirSourceWhenFingerprintsMatch(bool useShallowCopies)
        {
            var store = new AutoGrouperTraceStore(new Dictionary<DAT_GROUP_ENUM, FlagFilterSet>());
            GamePartVO first = CreatePart("First entry", "shared-fingerprint");
            GamePartVO second = CreatePart("Second entry", "shared-fingerprint");

            store.AttachPartReport(first, first.DatGameSource, first.DatGroupEnum, first.DatSourceID, "First DAT", null);
            Assert.True(store.TryGetPartReport(first, out AutomatedGroupingReport? expectedFirst));

            store.AttachPartReport(second, second.DatGameSource, second.DatGroupEnum, second.DatSourceID, "Second DAT", null);
            Assert.True(store.TryGetPartReport(second, out AutomatedGroupingReport? expectedSecond));
            Assert.NotNull(expectedFirst);
            Assert.NotNull(expectedSecond);
            Assert.NotEqual(expectedFirst.Identity.EntryName, expectedSecond.Identity.EntryName);

            if (useShallowCopies)
            {
                first = CreateShallowCopy(first);
                second = CreateShallowCopy(second);
            }

            // Registering another entry with the same ROMs must not replace this
            // entry's report, including when it is reached through a shallow part.
            Assert.True(store.TryGetPartReport(first, out AutomatedGroupingReport? actualFirst));
            Assert.NotNull(actualFirst);
            Assert.Same(expectedFirst.Identity, actualFirst.Identity);
            Assert.Equal("Second entry", Assert.Single(actualFirst.FingerprintMatches).Name);

            Assert.True(store.TryGetPartReport(second, out AutomatedGroupingReport? actualSecond));
            Assert.NotNull(actualSecond);
            Assert.Same(expectedSecond.Identity, actualSecond.Identity);
            Assert.Equal("First entry", Assert.Single(actualSecond.FingerprintMatches).Name);
        }

        [Fact]
        public void ReportsSnapshotAllTraceKeysAndDistinguishMissingTrace()
        {
            var store = new AutoGrouperTraceStore(new Dictionary<DAT_GROUP_ENUM, FlagFilterSet>());
            GamePartVO part = CreatePart("Tracked", "fingerprint");
            DatGameVO entry = part.DatGameSource;

            store.RecordSeed(entry, "Raw seed", "normalised-seed");
            store.RecordInitialSeedGroup(new[] { entry });
            store.RecordFinalFamily(new[] { entry });
            var names = new List<string> { "Matching name" };

            for (int i = 0; i < 10; i++)
                store.RecordSeedJoin(entry, AUTO_GROUP_TRACE_REASON_ENUM.MergeByFingerprint, "key-" + i, 4, names);

            store.AttachPartReport(part, entry, part.DatGroupEnum, part.DatSourceID, "DAT", null);
            Assert.True(store.TryGetPartReport(part, out AutomatedGroupingReport? report));
            Assert.NotNull(report);
            Assert.True(report.HasCapturedTrace);
            Assert.Equal("normalised-seed", report.Seed!.Key);
            Assert.Equal(1, report.InitialGroup!.MemberCount);
            Assert.Equal(1, report.FinalFamily!.MemberCount);
            Assert.Equal(10, report.SeedJoins.Count);
            Assert.All(report.SeedJoins, match => Assert.Equal(4, match.FirstMatchingGroupSize));

            // The exported report must not change when its recording buffers change.
            names.Clear();
            store.RecordSeedJoin(entry, AUTO_GROUP_TRACE_REASON_ENUM.MergeByFingerprint, "key-0", 9, new[] { "Later name" });
            Assert.Equal(1, report.SeedJoins[0].OccurrenceCount);
            Assert.Equal("Matching name", Assert.Single(report.SeedJoins[0].SampleMatchingNames));

            GamePartVO untracked = CreatePart("Untracked", "untracked-fingerprint");
            store.AttachPartReport(untracked, untracked.DatGameSource, untracked.DatGroupEnum, untracked.DatSourceID, "DAT", null);
            Assert.True(store.TryGetPartReport(untracked, out AutomatedGroupingReport? missing));
            Assert.NotNull(missing);
            Assert.False(missing.HasCapturedTrace);
            Assert.Null(missing.Seed);
            Assert.Null(missing.InitialGroup);
            Assert.Null(missing.FinalFamily);
        }

        [Fact]
        public void MissingAssociationDoesNotBorrowAnotherPartsReport()
        {
            var store = new AutoGrouperTraceStore(new Dictionary<DAT_GROUP_ENUM, FlagFilterSet>());
            GamePartVO registered = CreatePart("Registered", "shared-fingerprint");
            store.AttachPartReport(registered, registered.DatGameSource, registered.DatGroupEnum, registered.DatSourceID, "DAT", null);

            Assert.False(store.TryGetPartReport(CreatePart("Different source", "shared-fingerprint"), out var missing));
            Assert.Null(missing);

            var unregisteredOriginal = new GamePartVO(registered.Name, registered.DatSourceID,
                registered.DatGameSource, registered.ExpressionActionEnum, registered.Fingerprint,
                registered.DatGroupEnum, registered.DatReference, null, null, false, null, null);
            Assert.False(store.TryGetPartReport(unregisteredOriginal, out _));
        }

        [Fact]
        public void UiConversionPreservesReportsForPartsAliasesAndShallowCopies()
        {
            var store = new AutoGrouperTraceStore(new Dictionary<DAT_GROUP_ENUM, FlagFilterSet>());
            GamePartVO first = CreatePart("First entry", "shared-fingerprint");
            GamePartVO alias = CreatePart("Alias entry", "shared-fingerprint");
            GamePartVO second = CreatePart("Second entry", "shared-fingerprint");

            foreach (GamePartVO part in new[] { first, alias, second })
                store.AttachPartReport(part, part.DatGameSource, part.DatGroupEnum, part.DatSourceID, part.Name, null);

            Assert.True(first.AddAlias(alias));
            var firstGame = new GameVO(first, "First game");
            var secondGame = new GameVO(second, "Second game");
            var shallowGame = new GameVO(CreatePart("Destination", "different-fingerprint"), "Shallow game");
            Assert.True(shallowGame.TryAddShallowPart(first));
            var family = new GameFamilyVO("Family");
            family.ReplaceGames(new[] { firstGame, secondGame, shallowGame });

            var converted = DatinateFamilyConverter.Convert(new[] { family }, out _, store.AssociateConvertedPartReport).ToArray();
            IGamePart[] uiParts = Assert.Single(converted).GetAllGameParts(true).ToArray();

            foreach (IGamePart part in uiParts.Where(part => part.GetName() != "Destination"))
            {
                Assert.IsType<BaseGamePart>(part);
                Assert.True(store.TryGetPartReport(part, out var report), $"Missing converted report: {part.GetName()}");
                Assert.NotNull(report);
                Assert.Equal(part.GetName(), report.Identity.EntryName);
            }

            Assert.Equal(2, uiParts.Count(part => part.GetName() == "First entry"));
            Assert.Equal(2, uiParts.Count(part => part.GetName() == "Alias entry"));
        }

        private static GamePartVO CreateShallowCopy(GamePartVO original)
        {
            var destination = new GameVO(CreatePart("Destination", "different-fingerprint"), "Destination");

            Assert.True(destination.TryAddShallowPart(original));

            GamePartVO shallow = Assert.Single(destination.Parts, part => part.IsShallowPartReference);
            Assert.NotSame(original, shallow);
            Assert.Same(original.DatGameSource, shallow.DatGameSource);

            return shallow;
        }

        private static GamePartVO CreatePart(string name, string fingerprint)
        {
            var entry = new DatGameVO(name, null, null, null, null, null, null, null, Array.Empty<DatRomVO>(), null);

            return new GamePartVO(name, name, entry, default(EXPRESSION_ACTION_ENUM), fingerprint,
                default(DAT_GROUP_ENUM), name, null, null, false, null, null);
        }
    }
}
