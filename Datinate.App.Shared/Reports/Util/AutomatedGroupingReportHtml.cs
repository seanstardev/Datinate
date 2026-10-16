using Datinate.Shared;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Linq;

namespace Datinate.App.Shared.Reports
{
    public static class AutomatedGroupingReportHtml
    {
        private const int IndentPx = 14;
        private const int MaximumKeysPerReason = 8;

        public static string? CreateContent(AutomatedGroupingReport? report)
        {
            string? fragment = CreateFragment(report);

            return fragment == null ? null : ReportHtmlStyles.Wrap(fragment);
        }

        private static string? CreateFragment(AutomatedGroupingReport? report)
        {
            if (report == null)
                return null;

            var html = new StringBuilder(4096);
            AutomatedGroupingIdentity identity = report.Identity;
            AutomatedGroupingComparison comparison = report.Comparison;

            html.Append("<article class=\"datinate-report automated-report\"><h2>Automated Grouping Report</h2>");
            Text(html, identity.EntryName);

            foreach (AutomatedGroupingName match in report.FingerprintMatches)
                Text(html, $"{match.Name} [{match.DatGroup}]");

            Header(html, "Identity");
            KeyValue(html, "DAT:", string.IsNullOrWhiteSpace(identity.DatReference)
                ? identity.DatGroup.ToString()
                : $"{identity.DatGroup} | {identity.DatReference}");
            KeyValue(html, "Fingerprint:", identity.Fingerprint);

            Header(html, "Comparison data");
            KeyValue(html, "Title after removing flags:", comparison.FlaglessName);
            KeyValue(html, "Comparison key (title + publisher):", comparison.NamePublisherKey);
            KeyValue(html, "Comparison key (title + region):", comparison.NameRegionKey);
            KeyValue(html, "MAME launch/category name:", comparison.MameLaunchName);
            KeyValue(html, "Clone of:", comparison.ParentName);
            KeyValue(html, "Part owner name:", comparison.PartOwnerName);
            KeyValue(html, "Derived part-owner key:", comparison.DerivedPartOwnerKey);

            // The model records aggregates, not ordered events. Do not call this a timeline.
            Header(html, "Grouping summary");

            if (report.Seed != null)
                Bullet(html, $"Seeded by {FriendlyReason(report.Seed.Reason).ToLowerInvariant()}.");
            else
                Text(html, "The initial grouping seed was not captured.");

            if (report.InitialGroup != null)
                Bullet(html, $"Initial group size: {Count(report.InitialGroup.MemberCount, "item")}.");

            AppendSummary(html, report.SeedJoins, "Additional seed-group joins", "seed-group join", "seed-group joins");
            AppendSummary(html, report.LaterMerges, "Later merges", "merge event", "merge events");

            if (report.LaterMerges.Count == 0)
                Bullet(html, report.HasCapturedTrace ? "No later merges were captured." : "Later merge activity was not captured.");

            if (report.FinalFamily != null)
                Bullet(html, $"Final family size: {Count(report.FinalFamily.MemberCount, "item")}.");

            Header(html, "Initial grouping");
            KeyValue(html, "This item was first grouped using:", report.Seed == null ? "<not captured>" : FriendlyReason(report.Seed.Reason));
            KeyValue(html, "Seed value:", report.Seed?.Value ?? "<not captured>");
            KeyValue(html, "Normalised grouping key:", report.Seed?.Key ?? "<not captured>");

            Header(html, "Grouping result");
            AppendMembership(html, report.InitialGroup, false);
            Header(html, "Final family result");
            AppendMembership(html, report.FinalFamily, true);

            Header(html, "Seed-group joins");
            AppendMatches(html, report.SeedJoins, "seed-group join", "seed-group joins", report.HasCapturedTrace
                ? "No additional seed-group joins were captured after the initial grouping step."
                : "Seed-group join activity was not captured.");

            Header(html, "Later merges");
            AppendMatches(html, report.LaterMerges, "merge event", "merge events", report.HasCapturedTrace
                ? "No later merges were captured."
                : "Later merge activity was not captured.");

            html.Append("</article>");
            return html.ToString();
        }

        private static void AppendSummary(StringBuilder html, IReadOnlyList<AutomatedGroupingMatch> matches,
            string label, string singular, string plural)
        {
            if (matches.Count == 0)
                return;

            string summary = string.Join(", ", matches.GroupBy(match => match.Reason).Select(group =>
            {
                int count = group.Sum(match => match.OccurrenceCount);
                return $"{FriendlyReason(group.Key)} ({count} {(count == 1 ? singular : plural)})";
            }));

            Bullet(html, $"{label}: {summary}.");
        }

        private static void AppendMembership(StringBuilder html, AutomatedGroupingMembership? membership, bool final)
        {
            if (membership == null)
            {
                Text(html, final ? "The final family outcome was not captured." : "The initial grouping outcome was not captured.");
                return;
            }

            KeyValue(html, final ? "This item ended in a final family of:" : "This item started in an initial group of:",
                Count(membership.MemberCount, "item"));

            if (membership.MemberCount <= 1)
            {
                Text(html, final ? "No other items ended up grouped with this item." : "No other items matched this item's initial grouping seed.");
                return;
            }

            if (membership.SampleOtherNames.Count == 0)
            {
                Text(html, final ? "Other final family members existed, but no example item names were captured."
                    : "This item grouped with other matching items, but no example item names were captured.");
                return;
            }

            Blank(html);
            Text(html, final ? "Sample final family members:" : "Other matching items in this initial group:");

            foreach (string name in membership.SampleOtherNames)
                Bullet(html, name, 3);

            int remaining = membership.MemberCount - 1 - membership.SampleOtherNames.Count;

            if (remaining > 0)
                Text(html, $"... and {remaining} other {Plural(remaining, "item", "items")}.", 3);
        }

        private static void AppendMatches(StringBuilder html, IReadOnlyList<AutomatedGroupingMatch> matches,
            string singular, string plural, string emptyMessage)
        {
            if (matches.Count == 0)
            {
                Text(html, emptyMessage);
                return;
            }

            bool first = true;

            foreach (var group in matches.GroupBy(match => match.Reason))
            {
                if (!first)
                    Blank(html);

                first = false;
                html.Append("<h4").Append(Indent(1)).Append('>').Append(Encode(FriendlyReason(group.Key) + ":")).Append("</h4>");
                AutomatedGroupingMatch[] values = group.ToArray();
                int shown = Math.Min(values.Length, MaximumKeysPerReason);

                for (int i = 0; i < shown; i++)
                {
                    if (i > 0)
                        Blank(html);

                    AutomatedGroupingMatch match = values[i];
                    KeyValue(html, "Key:", string.IsNullOrWhiteSpace(match.Key) ? "<empty>" : match.Key, 2);
                    KeyValue(html, "Seen:", $"{match.OccurrenceCount} {(match.OccurrenceCount == 1 ? singular : plural)}", 2);

                    if (match.SampleMatchingNames.Count > 0)
                    {
                        Text(html, "Sample items from matching group:", 2);

                        foreach (string name in match.SampleMatchingNames)
                            Bullet(html, name, 3);
                    }

                    if (match.FirstMatchingGroupSize.HasValue)
                        KeyValue(html, "Size of matching group at that step:", Count(match.FirstMatchingGroupSize.Value, "item"), 2);
                }

                int omitted = values.Length - shown;

                if (omitted > 0)
                {
                    Blank(html);
                    Text(html, $"... and {omitted} other matching {Plural(omitted, "key", "keys")}.", 2);
                }
            }
        }

        private static string FriendlyReason(AUTO_GROUP_TRACE_REASON_ENUM reason)
            => reason switch
            {
                AUTO_GROUP_TRACE_REASON_ENUM.SeedJoinParentClone => "Parent/clone relationship",
                AUTO_GROUP_TRACE_REASON_ENUM.SeedJoinMameLaunchCategory => "MAME launch/category name",
                AUTO_GROUP_TRACE_REASON_ENUM.SeedJoinNormalisedName => "Normalised name",
                AUTO_GROUP_TRACE_REASON_ENUM.MergeByNamePublisher => "Normalised title + publisher",
                AUTO_GROUP_TRACE_REASON_ENUM.MergeByNameRegion => "Normalised title + region",
                AUTO_GROUP_TRACE_REASON_ENUM.MergeByNormalisedName => "Normalised name",
                AUTO_GROUP_TRACE_REASON_ENUM.MergeByFingerprint => "Exact fingerprint",
                AUTO_GROUP_TRACE_REASON_ENUM.CollapseFamilyByRegionNormalisedTitle => "Collapsed family by region-normalised title",
                _ => reason.ToString()
            };

        private static string Count(int count, string singular) => $"{count} {Plural(count, singular, singular + "s")}";
        private static string Plural(int count, string singular, string plural) => count == 1 ? singular : plural;
        private static string Encode(string text) => WebUtility.HtmlEncode(text);
        private static string Indent(int level) => $" style=\"margin-left:{level * IndentPx}px\"";

        private static void Header(StringBuilder html, string text)
        {
            Blank(html);
            html.Append("<h3>").Append(Encode(text)).Append("</h3>");
        }

        private static void Blank(StringBuilder html)
            => html.Append("<div class=\"report-blank\" aria-hidden=\"true\"></div>");

        private static void Text(StringBuilder html, string? text, int indent = 0)
        {
            if (!string.IsNullOrWhiteSpace(text))
                html.Append("<p").Append(Indent(indent)).Append('>').Append(Encode(text)).Append("</p>");
        }

        private static void Bullet(StringBuilder html, string text, int indent = 1)
            => html.Append("<div class=\"report-bullet\"").Append(Indent(indent)).Append('>')
                .Append("<span aria-hidden=\"true\">•</span><span>").Append(Encode(text)).Append("</span></div>");

        private static void KeyValue(StringBuilder html, string label, string? value, int indent = 1)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            html.Append("<div class=\"report-key\"").Append(Indent(indent)).Append('>').Append(Encode(label)).Append("</div>")
                .Append("<div class=\"report-value\">").Append(Encode(value)).Append("</div>");
        }
    }
}
