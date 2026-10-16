namespace Datinate.App.Shared.Reports
{
    // Each report carries the same scoped stylesheet, so it can stand alone or
    // share a page. Only theme colours differ; neither report depends on host CSS.
    internal static class ReportHtmlStyles
    {
        internal static string Wrap(string fragment) => "<style>" + Styles + "</style>" + fragment;

        private const string Styles = @"
            .datinate-report {
                --report-border:#b9a24d; --report-background:#fffbea;
                --report-accent:#68541b; --report-chip:#fff3c3;
                box-sizing:border-box; display:grid;
                grid-template-columns:fit-content(40%) minmax(0,1fr); column-gap:14px;
                width:100%; max-width:918px; min-height:40px; padding:10px 12px 12px;
                border:1px solid var(--report-border); border-radius:6px;
                background:var(--report-background); color:#292b29;
                font:12px 'Segoe UI',Arial,sans-serif; line-height:1.45;
            }
            .datinate-report.managed-report {
                --report-border:#6aa67b; --report-background:#f1fbf3;
                --report-accent:#285c38; --report-chip:#e1f3e6;
            }
            .datinate-report.managed-report.excluded {
                --report-border:#c77d7d; --report-background:#fff3f2;
                --report-accent:#853d3d; --report-chip:#ffe3e0;
            }
            .datinate-report > * { grid-column:1 / -1; min-width:0; margin:0; }
            .datinate-report h2, .datinate-report h3 {
                margin-top:12px; margin-bottom:6px; padding:5px 7px;
                border-bottom:1px solid var(--report-border); border-radius:3px 3px 0 0;
                background:rgba(255,255,255,.65); color:var(--report-accent);
                font-size:12px; font-weight:600; line-height:1.4;
            }
            .datinate-report > h2:first-child {
                margin-top:0; margin-bottom:8px; padding:2px 0 7px;
                background:transparent; border-radius:0; font-size:14px; font-weight:600;
            }
            .datinate-report h4 { padding:5px 0 3px; color:var(--report-accent); font-size:12px; font-weight:600; }
            .datinate-report p { margin:4px 0; white-space:pre-wrap; overflow-wrap:anywhere; }
            .datinate-report dl {
                display:grid; grid-template-columns:fit-content(40%) minmax(0,1fr);
                column-gap:14px; row-gap:5px; margin:4px 0 0;
            }
            .datinate-report dt, .datinate-report .report-key {
                margin:0; text-align:right; font-weight:600;
                white-space:pre-wrap; overflow-wrap:anywhere;
            }
            .datinate-report dd, .datinate-report .report-value {
                min-width:0; margin:0; white-space:pre-wrap; overflow-wrap:anywhere;
            }
            .datinate-report .report-key { grid-column:1; padding-bottom:4px; }
            .datinate-report .report-value { grid-column:2; padding-bottom:4px; }
            .datinate-report .report-bullet { display:flex; gap:7px; padding:2px 0; }
            .datinate-report .report-bullet > span:first-child { color:var(--report-accent); }
            .datinate-report .report-bullet > span:last-child { min-width:0; white-space:pre-wrap; overflow-wrap:anywhere; }
            .datinate-report .report-blank { height:6px; }
            .datinate-report .report-blank:has(+ h3) { display:none; }
            .datinate-report .report-chip {
                box-sizing:border-box; justify-self:start; max-width:100%; padding:4px 9px;
                border:1px solid var(--report-border); border-radius:4px;
                background:var(--report-chip); font-weight:600;
                white-space:pre-wrap; overflow-wrap:anywhere;
            }
        ";
    }
}
