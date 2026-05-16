using datinate.shared;
using RadioLibCore.RadioDat;

namespace datinate.app
{
    public static class HtmlReportsUtil
    {
        public static string? CreatePartReports(
            IGamePart part,
            string? autoGrouperReport,
            ManagedListItemReport? customiseReport,
            Control someControl)
        {
            if (string.IsNullOrWhiteSpace(autoGrouperReport) && customiseReport == null)
                return null;

            string? report1 = null;
            string? report2 = null;

            if (!string.IsNullOrWhiteSpace(autoGrouperReport))
                report1 = DatGrouperPartToolTip.CreateHtmlFragment(autoGrouperReport, someControl);

            if (customiseReport != null)
                report2 = ManagedListItemReportToolTip.CreateHtmlFragment(customiseReport);

            return WrapImageFragmentsAsPage(report1, report2);
        }

        public static string? CreateCuratedImportErrorsReport(Dictionary<string, IGamePart?> importErrorReport)
        {
            if (importErrorReport == null || importErrorReport.Count == 0)
                return null;

            static string H(string? value) => System.Net.WebUtility.HtmlEncode(value ?? string.Empty);

            static void AppendMetaRow(System.Text.StringBuilder sb, string label, string? value, bool mono = false)
            {
                if (string.IsNullOrWhiteSpace(value))
                    return;

                sb.Append("<div class=\"meta-label\">");
                sb.Append(H(label));
                sb.Append("</div>");

                sb.Append("<div class=\"meta-value");
                if (mono)
                    sb.Append(" mono");
                sb.Append("\">");
                sb.Append(H(value));
                sb.Append("</div>");
            }

            var errorCount = importErrorReport.Count;
            var errorCountText = errorCount == 1
                ? "1 error has been identified"
                : $"{errorCount} errors have been identified";

            var sb = new System.Text.StringBuilder(16384);

            sb.Append("<!doctype html><html><head><meta charset=\"utf-8\">");
            sb.Append("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            sb.Append("<style>");
            sb.Append("*{box-sizing:border-box;}");
            sb.Append("html,body{margin:0;padding:0;width:100%;}");
            sb.Append("body{padding:18px;background:#fff8f8;color:#4f2f2f;font-family:'Segoe UI',Arial,sans-serif;}");
            sb.Append(".wrap{max-width:1200px;margin:0 auto;display:flex;flex-direction:column;gap:16px;}");
            sb.Append(".hero{background:linear-gradient(180deg,#fff6f6 0%,#fff1f1 100%);border:1px solid #ebcaca;border-left:6px solid #c96f6f;border-radius:16px;padding:18px 20px;box-shadow:0 8px 24px rgba(120,50,50,0.08);}");
            sb.Append(".hero-title{margin:0 0 6px 0;font-size:28px;line-height:1.15;color:#7a3030;font-weight:700;}");
            sb.Append(".hero-sub{margin:0;font-size:14px;line-height:1.5;color:#7a5959;}");
            sb.Append(".hero-count{margin:14px 0 0 0;display:inline-block;padding:10px 14px;background:#fffdfd;border:1px solid #e7c9c9;border-radius:10px;color:#7a3030;font-size:15px;font-weight:700;box-shadow:0 4px 12px rgba(120,50,50,0.05);}");
            sb.Append(".list{display:grid;grid-template-columns:repeat(auto-fit,minmax(320px,1fr));gap:16px;align-items:start;}");
            sb.Append(".card{background:#fffdfd;border:1px solid #ebd3d3;border-top:4px solid #c96f6f;border-radius:14px;padding:16px;box-shadow:0 8px 22px rgba(120,50,50,0.07);min-width:0;}");
            sb.Append(".card-title{margin:0 0 14px 0;font-size:18px;line-height:1.35;color:#6f2a2a;font-weight:700;overflow-wrap:anywhere;word-break:break-word;}");
            sb.Append(".meta{display:grid;grid-template-columns:minmax(110px,150px) minmax(0,1fr);gap:8px 12px;align-items:start;}");
            sb.Append(".meta-label{font-weight:600;color:#7a3030;}");
            sb.Append(".meta-value{min-width:0;overflow-wrap:anywhere;word-break:break-word;color:#4f2f2f;}");
            sb.Append(".mono{font-family:Consolas,'Courier New',monospace;font-size:12px;background:#fff3f3;border:1px solid #efd9d9;border-radius:8px;padding:7px 9px;}");
            sb.Append(".section{margin-top:14px;padding-top:12px;border-top:1px solid #efdada;}");
            sb.Append(".section-title{margin:0 0 8px 0;font-size:14px;font-weight:700;color:#7a3030;}");
            sb.Append(".alias-list{margin:0;padding-left:20px;}");
            sb.Append(".alias-item{margin:0 0 10px 0;}");
            sb.Append(".alias-name{font-weight:600;color:#4f2f2f;overflow-wrap:anywhere;word-break:break-word;}");
            sb.Append(".alias-meta{margin-top:3px;color:#7a5959;font-size:13px;line-height:1.45;overflow-wrap:anywhere;word-break:break-word;}");
            sb.Append(".empty{color:#7a5959;font-style:italic;}");
            sb.Append("@media (max-width:640px){");
            sb.Append("body{padding:12px;}");
            sb.Append(".hero{padding:16px;border-radius:12px;}");
            sb.Append(".hero-title{font-size:24px;}");
            sb.Append(".card{padding:14px;border-radius:12px;}");
            sb.Append(".meta{grid-template-columns:1fr;gap:4px 0;}");
            sb.Append(".meta-label{margin-top:6px;}");
            sb.Append(".hero-count{display:block;width:100%;}");
            sb.Append("}");
            sb.Append("</style></head><body>");

            sb.Append("<div class=\"wrap\">");
            sb.Append("<section class=\"hero\">");
            sb.Append("<h1 class=\"hero-title\">Curation Import Errors</h1>");
            sb.Append("<p class=\"hero-sub\">The following Curated Game Parts could not be imported. Note that saving this Project will erase this report.</p>");
            sb.Append("<div class=\"hero-count\">");
            sb.Append(H(errorCountText));
            sb.Append("</div>");
            sb.Append("</section>");

            sb.Append("<section class=\"list\">");

            foreach (var kvp in importErrorReport)
            {
                var title = kvp.Key;
                var part = kvp.Value;

                sb.Append("<article class=\"card\">");
                sb.Append("<h2 class=\"card-title\">");
                sb.Append(H(title));
                sb.Append("</h2>");

                if (part == null)
                {
                    sb.Append("<div class=\"empty\">No matching game part details were available.</div>");
                    sb.Append("</article>");
                    continue;
                }

                sb.Append("<div class=\"meta\">");

                var name = part.GetName();
                var displayName = part.GetDisplayName();
                var path = part.GetPath();
                var fullPath = part.GetFullpath();
                var launchName = part.LaunchName;
                var fingerprint = part.Fingerprint;
                var tag = part.Tag;
                var directoryId = part.HasDirectoryId() ? part.GetDirectoryId() : null;

                AppendMetaRow(sb, "Name", name);
                AppendMetaRow(sb, "Display name", part.HasDisplayName() ? displayName : null);
                AppendMetaRow(sb, "Launch name", launchName);
                AppendMetaRow(sb, "Fingerprint", fingerprint, mono: true);
                AppendMetaRow(sb, "Tag", tag);
                AppendMetaRow(sb, "Path", path, mono: true);
                AppendMetaRow(sb, "Full path", fullPath, mono: true);
                AppendMetaRow(sb, "Directory ID", directoryId, mono: true);
                AppendMetaRow(sb, "Excluded", part.Exclude ? "Yes" : "No");

                var checksums = part.GetChecksums();
                if (checksums != null && checksums.Length > 0)
                {
                    var nonBlankChecksums = checksums
                        .Where(x => string.IsNullOrWhiteSpace(x) == false)
                        .ToArray();

                    if (nonBlankChecksums.Length > 0)
                        AppendMetaRow(sb, "Checksums", string.Join("\n", nonBlankChecksums), mono: true);
                }

                sb.Append("</div>");

                var aliases = part.GetSoftwareAliases();
                if (aliases != null && aliases.Length > 0)
                {
                    var visibleAliases = aliases.Where(x => x != null).ToArray();

                    if (visibleAliases.Length > 0)
                    {
                        sb.Append("<div class=\"section\">");
                        sb.Append("<div class=\"section-title\">Aliases</div>");
                        sb.Append("<ul class=\"alias-list\">");

                        for (var i = 0; i < visibleAliases.Length; i++)
                        {
                            var alias = visibleAliases[i];
                            if (alias == null)
                                continue;

                            sb.Append("<li class=\"alias-item\">");

                            var aliasTitle = alias.HasDisplayName() ? alias.GetDisplayName() : alias.GetName();
                            sb.Append("<div class=\"alias-name\">");
                            sb.Append(H(aliasTitle));
                            sb.Append("</div>");

                            var aliasBits = new List<string>();

                            if (string.IsNullOrWhiteSpace(alias.LaunchName) == false)
                                aliasBits.Add("Launch name: " + alias.LaunchName);

                            if (string.IsNullOrWhiteSpace(alias.Tag) == false)
                                aliasBits.Add("Tag: " + alias.Tag);

                            if (string.IsNullOrWhiteSpace(alias.GetPath()) == false)
                                aliasBits.Add("Path: " + alias.GetPath());

                            if (alias.HasDirectoryId() && string.IsNullOrWhiteSpace(alias.GetDirectoryId()) == false)
                                aliasBits.Add("Directory ID: " + alias.GetDirectoryId());

                            if (aliasBits.Count > 0)
                            {
                                sb.Append("<div class=\"alias-meta\">");
                                sb.Append(H(string.Join(" | ", aliasBits)));
                                sb.Append("</div>");
                            }

                            sb.Append("</li>");
                        }

                        sb.Append("</ul>");
                        sb.Append("</div>");
                    }
                }

                sb.Append("</article>");
            }

            sb.Append("</section>");
            sb.Append("</div>");
            sb.Append("</body></html>");

            return sb.ToString();
        }
        private static string WrapImageFragmentsAsPage(params string?[] fragments)
        {
            var sb = new System.Text.StringBuilder(4096);

            sb.Append("<!doctype html><html><head><meta charset=\"utf-8\">");
            sb.Append("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            sb.Append("<style>");
            sb.Append("*{box-sizing:border-box;}");
            sb.Append("html,body{margin:0;padding:0;width:100%;}");
            sb.Append("body{padding:12px;overflow:auto;display:flex;flex-direction:column;align-items:flex-start;justify-content:flex-start;gap:12px;}");
            sb.Append("div{max-width:100%;}");
            sb.Append("img{display:block;max-width:100% !important;height:auto !important;object-fit:contain;}");
            sb.Append("</style></head><body>");

            for (int i = 0; i < fragments.Length; i++)
            {
                var f = fragments[i];
                if (string.IsNullOrWhiteSpace(f))
                    continue;

                sb.Append("<div>");
                sb.Append(f);
                sb.Append("</div>");
            }

            sb.Append("</body></html>");
            return sb.ToString();
        }
    }
}
