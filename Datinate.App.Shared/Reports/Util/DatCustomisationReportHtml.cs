using Datinate.Shared;
using System;
using System.Net;
using System.Text;
using static Datinate.Shared.DatFilterHelper;

namespace Datinate.App.Shared.Reports
{
    public static class DatCustomisationReportHtml
    {
        private const int ReportIndentPx = 22;
        private const int MaximumTraceItems = 48;

        // One stylesheet and content builder serve both the report page and tooltip capture.
        public static string CreateContent(DatCustomisationDecisionReport report)
            => ReportHtmlStyles.Wrap(CreateFragment(report));

        private static string CreateFragment(DatCustomisationDecisionReport report)
        {
            var html = new StringBuilder(4096);

            html.Append(report.IsExcluded
                ? "<article class=\"datinate-report managed-report excluded\">"
                : "<article class=\"datinate-report managed-report\">");

            static string Encode(string? text) => WebUtility.HtmlEncode(text ?? string.Empty);

            void AddHeader(string text)
            {
                html.Append("<h2>");
                html.Append(Encode(text));
                html.Append("</h2>");
            }

            void AddParagraph(string text, int indentPx = 0, int extraTopPx = 0)
            {
                html.Append($"<p style=\"margin-left:{indentPx}px;margin-top:{extraTopPx}px\">");
                html.Append(Encode(text));
                html.Append("</p>");
            }

            void AddKeyValues((string Key, string Value)[] items, int indentPx = 0, int extraTopPxFirst = 0)
            {
                if (items.Length == 0)
                    return;

                html.Append($"<dl style=\"margin-left:{indentPx}px;margin-top:{extraTopPxFirst}px\">");

                foreach (var item in items)
                {
                    html.Append("<dt>");
                    html.Append(Encode(item.Key));
                    html.Append("</dt><dd>");
                    html.Append(Encode(item.Value));
                    html.Append("</dd>");
                }

                html.Append("</dl>");
            }

            void AddChip(string text, int indentPx = 0, int extraTopPx = 0)
            {
                html.Append($"<div class=\"report-chip\" style=\"margin-left:{indentPx}px;margin-top:{extraTopPx}px\">");
                html.Append(Encode(text));
                html.Append("</div>");
            }

            string OutcomeDetail()
            {
                if (report.IsExcluded)
                {
                    if (report.HasDecisiveFilter && report.DecisiveFilterIsCategory)
                        return "Excluded by category rule";

                    if (report.HasDecisiveFilter)
                        return "Excluded by explicit filter";

                    if (report.FinalAction == EXPRESSION_ACTION_ENUM.EXCLUDE_CONDITIONAL)
                        return "Conditional reject";

                    return "Rejected";
                }

                if (report.HasDecisiveFilter && report.DecisiveFilterIsCategory)
                    return "Included by category rule";

                if (report.HasDecisiveFilter)
                    return "Included by explicit filter";

                if (report.FinalAction == EXPRESSION_ACTION_ENUM.EXCLUDE_CONDITIONAL)
                    return "Conditional include";

                if (report.FinalAction == EXPRESSION_ACTION_ENUM.INCLUDE)
                    return "Include";

                return "Implicit include";
            }

            AddHeader("Dat Customisation Outcome");

            var outcome = report.IsExcluded ? "EXCLUDED" : "INCLUDED";

            AddKeyValues(
                new[]
                {
                    (outcome, OutcomeDetail()),
                    ("Entry", report.EntryName),
                    ("Category", string.IsNullOrWhiteSpace(report.EntryCategoryLabel) ? "(none)" : report.EntryCategoryLabel!)
                },
                indentPx: 0,
                extraTopPxFirst: 10);

            AddHeader("Decision");

            if (report.HasDecisiveFilter)
            {
                var idx = report.DecisiveFilterPriorityIndex1Based!.Value.ToString();
                var eff = report.DecisiveFilterEffectName ?? string.Empty;
                var expr = report.DecisiveFilterExpression ?? string.Empty;
                var tag = report.DecisiveFilterIsCategory ? " (category)" : string.Empty;

                AddParagraph("Decisive filter.", ReportIndentPx, 6);
                AddKeyValues(
                    new[]
                    {
                        ("Priority", idx),
                        ("Effect", eff + tag),
                        ("Expression", expr)
                    },
                    indentPx: ReportIndentPx,
                    extraTopPxFirst: 4);
            }
            else
            {
                AddParagraph("Resolved by conditional scoring.", ReportIndentPx, 6);
                AddKeyValues(
                    new[]
                    {
                        ("Your strips", report.ConditionalStripCount.ToString()),
                        ("Best in group", report.BestInGroupScore.HasValue ? report.BestInGroupScore.Value.ToString() : "n/a")
                    },
                    indentPx: ReportIndentPx,
                    extraTopPxFirst: 4);
            }

            AddHeader("Name Management");

            AddParagraph("The name after removing conditional segments from the entry name. Entries with the same cleaned key are treated as competing variants.", 0, 6);
            AddParagraph("");

            AddKeyValues(
                new[]
                {
                    ("Original", report.EntryName),
                    ("Cleaned key", report.CleanedKey ?? "n/a")
                },
                indentPx: 0,
                extraTopPxFirst: 6);

            AddHeader("Conditional Expression Matches");

            if (report.MatchedConditionals.Count == 0)
            {
                AddParagraph("(none)", ReportIndentPx, 6);
            }
            else
            {
                for (int i = 0; i < report.MatchedConditionals.Count; i++)
                {
                    var m = report.MatchedConditionals[i];
                    var prefix = m.IsCategory ? "CAT " : string.Empty;
                    var chip = string.Concat(prefix, m.EffectName, "   ", m.Expression);
                    AddChip(chip, ReportIndentPx, 6);
                }
            }

            AddHeader("Matched Expressions");

            AddKeyValues(
                new[]
                {
                    ("Count", report.MatchedFilterCount.ToString())
                },
                indentPx: ReportIndentPx,
                extraTopPxFirst: 6);

            if (report.Trace.Count > 0)
            {
                var max = Math.Min(report.Trace.Count, MaximumTraceItems);
                for (int i = 0; i < max; i++)
                {
                    var t = report.Trace[i];
                    var catTag = t.IsCategory ? " [CAT]" : string.Empty;
                    var line = string.Concat(t.Index1Based.ToString(), ". ", t.ExpressionActionName, catTag, "   ", t.UserFriendlyExpression);
                    AddParagraph(line, ReportIndentPx, i == 0 ? 10 : 4);
                }

                if (report.Trace.Count > max)
                    AddParagraph("...", ReportIndentPx, 4);
            }

            html.Append("</article>");

            return html.ToString();
        }
    }
}
