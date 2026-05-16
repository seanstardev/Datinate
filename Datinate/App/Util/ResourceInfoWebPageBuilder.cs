using com.RADIO.Datinate.RMVC.Shared;
using RadioLibCore.RadioResource;
using System.Globalization;
using System.Net;
using System.Text;

namespace datinate.app
{
    public static class ResourceInfoWebPageBuilder
    {
        private const bool CreateHeroBackdropCompositionIfPossible = true;
        private const int HeroBackdropCompositionImageLimit = 8;
        private const int HeroBackdropCompositionGapPx = 12;

        private const int HeroPosterWidthPx = 150;
        private const int HeroPosterMaxHeightPx = 260;
        private const int LightboxDefaultScalePercent = 100;
        private const int LightboxScreenshotScalePercent = 200;

        // Original - Blue:
        private const string PrimaryHex = "#7FB2FF";
        private const string BackgroundHex = "#0B0D12";
        private const string BackgroundAltHex = "#131823";
        private const string PageTopHex = "#0A0C11";
        private const string PageBottomHex = "#0D1118";
        private const string PanelHex = "#161C27";
        private const string Panel2Hex = "#1C2431";
        private const string TextHex = "#EEF3FB";
        private const string MutedHex = "#A7B4C8";

        // Emerald
        //private const string PrimaryHex = "#61CFA2";
        //private const string BackgroundHex = "#0A100E";
        //private const string BackgroundAltHex = "#13201C";
        //private const string PageTopHex = "#090D0C";
        //private const string PageBottomHex = "#0E1512";
        //private const string PanelHex = "#17231F";
        //private const string Panel2Hex = "#20302A";
        //private const string TextHex = "#EFFBF5";
        //private const string MutedHex = "#A8C3B7";

        // Amber
        //private const string PrimaryHex = "#D9AE5F";
        //private const string BackgroundHex = "#120F0A";
        //private const string BackgroundAltHex = "#211A12";
        //private const string PageTopHex = "#0E0B07";
        //private const string PageBottomHex = "#17120C";
        //private const string PanelHex = "#271F16";
        //private const string Panel2Hex = "#34291D";
        //private const string TextHex = "#FFF7EC";
        //private const string MutedHex = "#C8B79E";

        public static string BuildDocumentHtml(
            InfoVO? info,
            string? assetRootRelativePath,
            string url,
            bool createSampleVersion)
        {
            if (info == null ||
                string.IsNullOrWhiteSpace(assetRootRelativePath) ||
                !Directory.Exists(assetRootRelativePath) ||
                string.IsNullOrWhiteSpace(url))
                return string.Empty;

            string pageTitle = H(FirstNonEmpty(info.Name, info.Lookup, "Resource"));

            var result =
                $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"" />
    <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <title>{pageTitle}</title>
    <style>
{BuildCss()}
    </style>
</head>
{BuildBodyHtml(info, assetRootRelativePath, url, createSampleVersion)}
</html>";

            return result;
        }
        private static string BuildBodyHtml(
            InfoVO info,
            string assetRootRelativePath,
            string url,
            bool createSampleVersion)
        {
            BodyRenderData model = BuildBodyRenderData(info, assetRootRelativePath, url);

            var sb = new StringBuilder();

            sb.AppendLine("<body>");
            sb.AppendLine("<div class=\"page\">");

            AppendHeroSection(sb, model);

            sb.AppendLine($"  <div class=\"{model.PageGridClass}\">");
            sb.AppendLine("    <div class=\"stack\">");

            AppendOverviewSection(sb, model);

            if (createSampleVersion == false)
                AppendReleasesSection(sb, model);

            if (createSampleVersion == false)
                AppendAssortedMediaSection(sb, model);

            if (createSampleVersion == false)
                AppendVideosSection(sb, model);

            if (createSampleVersion == false)
                AppendManualsSection(sb, model);

            if (createSampleVersion == false)
                AppendScreenshotsSection(sb, model);

            if (createSampleVersion == false)
                AppendCreditsSection(sb, model);

            sb.AppendLine("    </div>");

            AppendAsideColumn(sb, model);

            sb.AppendLine("  </div>");

            AppendLightbox(sb);

            sb.AppendLine(BuildScriptBlock());

            sb.AppendLine("</div>");
            sb.AppendLine("</body>");

            return sb.ToString();
        }

        private static BodyRenderData BuildBodyRenderData(InfoVO info, string assetRootRelativePath, string url)
        {
            string title = FirstNonEmpty(info.Name, info.Lookup, "Untitled Resource");
            string lookup = FirstNonEmpty(info.Lookup, "");
            string systemLookup = FirstNonEmpty(info.SystemLookup, "");
            string sourceName = info.ResourceEnum.ToString();
            string sourceDisplayName = string.Equals(sourceName, "NOT_SET", StringComparison.OrdinalIgnoreCase) ? "" : sourceName;
            string developer = FirstNonEmpty(info.Developer, "");
            string genre = FirstNonEmpty(info.Genre, "");
            string players = FirstNonEmpty(info.Players, "");
            string startupText = FirstNonEmpty(info.StartupText, "");

            string description = FirstNonEmpty(info.DescriptionVO?.Description, "");
            bool descriptionIsHtml = info.DescriptionVO?.TextFormatEnum == DescriptionVO.TEXT_FORMAT_ENUM.html;
            bool descriptionIsBoilerplate = info.DescriptionVO?.IsBoilerplate ?? false;
            string emulationHistory = FirstNonEmpty(info.EmulationVO?.History, "");

            var aka = (info.AlsoKnownAs ?? Array.Empty<string>())
                .WhereNotBlank()
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var alsoOn = (info.AlsoOn ?? Array.Empty<string>())
                .WhereNotBlank()
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var releases = (info.ReleaseVOs ?? Array.Empty<ReleaseVO>())
                .Select(ReleaseVm.From)
                .Where(x => !x.IsEmpty)
                .OrderBy(x => x.SortDate ?? DateTime.MaxValue)
                .ThenBy(x => x.Region, StringComparer.OrdinalIgnoreCase)
                .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var misc = (info.MiscPropertyVOs ?? Array.Empty<MiscPropertyVO>())
                .Select(MiscVm.From)
                .Where(x => !x.IsEmpty)
                .ToList();

            var compilations = (info.CompilationVOs ?? Array.Empty<CompilationVO>())
                .Select(CompilationVm.From)
                .Where(x => !x.IsEmpty)
                .ToList();

            var credits = (info.CreditVOs ?? Array.Empty<CreditVO>())
                .Select(CreditVm.From)
                .Where(x => !x.IsEmpty)
                .ToList();

            var creditCategories = BuildCreditCategories(credits);

            FileAssetIndex assetIndex = BuildFileAssetIndex(assetRootRelativePath);

            string heroThumbUri = FirstNonEmpty(
                assetIndex.ThumbUri,
                assetIndex.RootAssets.FirstOrDefault(x => IsImageRootAsset(x) && string.Equals(x.Kind, R2DatWebFlag.Media.BOX, StringComparison.OrdinalIgnoreCase))?.Uri,
                assetIndex.RootAssets.FirstOrDefault(x => IsImageRootAsset(x) && string.Equals(x.Kind, R2DatWebFlag.Media.MEDIA, StringComparison.OrdinalIgnoreCase))?.Uri,
                assetIndex.RootAssets.FirstOrDefault(x => IsImageRootAsset(x) && string.Equals(x.Kind, R2DatWebFlag.Media.THUMB_RELEASE, StringComparison.OrdinalIgnoreCase))?.Uri,
                assetIndex.RootAssets.FirstOrDefault(IsImageRootAsset)?.Uri,
                "");

            List<string> heroBackdropUris = BuildHeroBackdropUris(assetIndex, heroThumbUri);

            string heroBackdropUri = FirstNonEmpty(
                heroBackdropUris.FirstOrDefault(),
                heroThumbUri,
                "");

            bool hasHeroThumb = !string.IsNullOrWhiteSpace(heroThumbUri);
            string descriptionHtml = BuildDescriptionHtml(description, descriptionIsHtml);

            bool hasOverviewContent =
                !string.IsNullOrWhiteSpace(developer) ||
                !string.IsNullOrWhiteSpace(players) ||
                !string.IsNullOrWhiteSpace(genre) ||
                !string.IsNullOrWhiteSpace(startupText) ||
                aka.Count > 0 ||
                alsoOn.Count > 0;

            bool hasAsideContent =
                !string.IsNullOrWhiteSpace(emulationHistory) ||
                misc.Count > 0 ||
                compilations.Count > 0;

            bool showSideScreenshotFeature =
                hasAsideContent &&
                assetIndex.ScreenshotUris.Count > 0 &&
                !string.IsNullOrWhiteSpace(assetIndex.ScreenshotUris[0]);

            string pageGridClass = hasAsideContent ? "page-grid has-aside" : "page-grid no-aside";

            var releaseAssetItemsByRelease = new List<List<ReleaseAssetVm>>(releases.Count);
            var usedReleaseAssetUris = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (ReleaseVm release in releases)
            {
                var releaseAssetItems = BuildReleaseAssetItems(assetIndex, release, title);
                releaseAssetItemsByRelease.Add(releaseAssetItems);

                foreach (ReleaseAssetVm item in releaseAssetItems)
                {
                    if (!string.IsNullOrWhiteSpace(item.Uri))
                        usedReleaseAssetUris.Add(item.Uri);
                }
            }
            var otherMediaGroups = BuildOtherMediaGroups(assetIndex, usedReleaseAssetUris);
            List<StandaloneMediaAssetVm> videoAssets = BuildStandaloneMediaAssets(assetIndex.RootAssets, R2DatWebFlag.Media.VIDEO, ".mp4");
            List<StandaloneMediaAssetVm> manualAssets = BuildStandaloneMediaAssets(assetIndex.RootAssets, R2DatWebFlag.Media.MANUAL, ".pdf");

            return new BodyRenderData
            {
                Title = title,
                Lookup = lookup,
                SystemLookup = systemLookup,
                SourceDisplayName = sourceDisplayName,
                Developer = developer,
                Genre = genre,
                Players = players,
                StartupText = startupText,
                DescriptionHtml = descriptionHtml,
                DescriptionIsBoilerplate = descriptionIsBoilerplate,
                EmulationHistory = emulationHistory,
                Aka = aka,
                AlsoOn = alsoOn,
                Releases = releases,
                Misc = misc,
                Compilations = compilations,
                CreditCategories = creditCategories,
                AssetIndex = assetIndex,
                HeroThumbUri = heroThumbUri,
                HeroBackdropUris = heroBackdropUris,
                HeroBackdropUri = heroBackdropUri,
                HasHeroThumb = hasHeroThumb,
                HasOverviewContent = hasOverviewContent,
                HasAsideContent = hasAsideContent,
                ShowSideScreenshotFeature = showSideScreenshotFeature,
                PageGridClass = pageGridClass,
                ReleaseAssetItemsByRelease = releaseAssetItemsByRelease,
                OtherMediaGroups = otherMediaGroups,
                Url = url,
                VideoAssets = videoAssets,
                ManualAssets = manualAssets
            };
        }

        private static List<string> BuildHeroBackdropUris(FileAssetIndex assetIndex, string heroThumbUri)
        {
            var screenshotUris = assetIndex.ScreenshotUris
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(CreateHeroBackdropCompositionIfPossible ? HeroBackdropCompositionImageLimit : 1)
                .ToList();

            if (screenshotUris.Count > 0)
                return screenshotUris;

            if (!string.IsNullOrWhiteSpace(heroThumbUri))
                return new List<string> { heroThumbUri };

            return new List<string>();
        }

        private static List<CreditCategoryVm> BuildCreditCategories(List<CreditVm> credits)
        {
            return credits
                .GroupBy(x => FirstNonEmpty(x.Category, "Credits"))
                .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase)
                .Select(g => new CreditCategoryVm
                {
                    Category = g.Key,
                    Roles = g
                        .GroupBy(x => FirstNonEmpty(x.Role, "Other"))
                        .OrderBy(rg => rg.Key, StringComparer.OrdinalIgnoreCase)
                        .Select(rg => new CreditRoleVm
                        {
                            Role = rg.Key,
                            Credits = rg
                                .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                                .ToList()
                        })
                        .Where(x => x.Credits.Count > 0)
                        .ToList()
                })
                .Where(x => x.Roles.Count > 0)
                .ToList();
        }

        private static void AppendHeroSection(StringBuilder sb, BodyRenderData model)
        {
            sb.AppendLine("  <header class=\"hero\">");

            if (model.HeroBackdropUris.Count > 1)
            {
                sb.AppendLine("    <div class=\"hero-backdrop-wrap hero-backdrop-wrap-composite\">");
                sb.AppendLine($"      <div class=\"hero-backdrop-collage hero-backdrop-collage-{model.HeroBackdropUris.Count.ToString(CultureInfo.InvariantCulture)}\">");

                for (int i = 0; i < model.HeroBackdropUris.Count; i++)
                {
                    string backdropUri = model.HeroBackdropUris[i];
                    string backdropFileName = ExtractFileNameFromUri(backdropUri);
                    sb.AppendLine("        <div class=\"hero-backdrop-tile\">");
                    sb.AppendLine($"          <img class=\"hero-backdrop-tile-image\" data-asset=\"1\" data-no-tooltip=\"1\" data-lightbox-group=\"screenshots\"{GetLightboxScaleAttribute(LightboxScreenshotScalePercent)} data-filename=\"{HA(backdropFileName)}\" src=\"{HA(backdropUri)}\" alt=\"Backdrop screenshot {i + 1}\" loading=\"lazy\" />");
                    sb.AppendLine("        </div>");
                }

                sb.AppendLine("      </div>");
                sb.AppendLine("      <div class=\"hero-backdrop-fade\"></div>");
                sb.AppendLine("    </div>");
            }
            else if (!string.IsNullOrWhiteSpace(model.HeroBackdropUri))
            {
                sb.AppendLine("    <div class=\"hero-backdrop-wrap\">");
                sb.AppendLine($"      <img class=\"hero-backdrop\" data-asset=\"1\" data-no-tooltip=\"1\" data-lightbox-group=\"screenshots\"{GetLightboxScaleAttribute(LightboxScreenshotScalePercent)} data-filename=\"{HA(ExtractFileNameFromUri(model.HeroBackdropUri))}\" src=\"{HA(model.HeroBackdropUri)}\" alt=\"Backdrop\" loading=\"lazy\" />");
                sb.AppendLine("      <div class=\"hero-backdrop-fade\"></div>");
                sb.AppendLine("    </div>");
            }

            sb.AppendLine($"    <div class=\"hero-content {(model.HasHeroThumb ? "has-poster" : "no-poster")}\">");
            if (model.HasHeroThumb)
            {
                sb.AppendLine("      <div class=\"hero-media-column\">");
                sb.AppendLine("        <div class=\"poster-wrap\">");
                sb.AppendLine($"          <img class=\"poster clickable\" data-asset=\"1\" data-no-tooltip=\"1\" data-filename=\"{HA(ExtractFileNameFromUri(model.HeroThumbUri))}\" src=\"{HA(model.HeroThumbUri)}\" alt=\"{HA(model.Title)} cover\" />"); sb.AppendLine("        </div>");
                sb.AppendLine("        <div class=\"hero-poster-actions\">");
                sb.AppendLine($"          <a class=\"source-link-btn\" href=\"{HA(model.Url)}\" target=\"_blank\" rel=\"noopener noreferrer\">Open Source Page</a>");
                sb.AppendLine("        </div>");
                sb.AppendLine("      </div>");
            }

            sb.AppendLine("      <div>");
            sb.AppendLine($"        <h1 class=\"hero-title\">{H(model.Title)}</h1>");

            var subtitleParts = new List<string>();
            if (!string.IsNullOrWhiteSpace(model.SourceDisplayName))
                subtitleParts.Add($"Source: {H(model.SourceDisplayName)}");
            if (!string.IsNullOrWhiteSpace(model.Lookup))
                subtitleParts.Add($"Lookup: {H(model.Lookup)}");
            if (!string.IsNullOrWhiteSpace(model.SystemLookup))
                subtitleParts.Add($"System: {H(model.SystemLookup)}");

            if (subtitleParts.Count > 0)
                sb.AppendLine($"        <p class=\"hero-subtitle\">{string.Join(" &nbsp;&middot;&nbsp; ", subtitleParts)}</p>");

            if (!model.HasHeroThumb)
            {
                sb.AppendLine("        <div class=\"hero-actions\">");
                sb.AppendLine($"          <a class=\"source-link-btn\" href=\"{HA(model.Url)}\" target=\"_blank\" rel=\"noopener noreferrer\">Open Source Page</a>");
                sb.AppendLine("        </div>");
            }

            sb.AppendLine("        <div class=\"meta-chips\">");
            AppendChipIfPresent(sb, "Developer", model.Developer);
            AppendChipIfPresent(sb, "Players", model.Players);
            AppendChipIfPresent(sb, "Genre", model.Genre);
            AppendChipIfPresent(sb, "Releases", model.Releases.Count > 0 ? model.Releases.Count.ToString(CultureInfo.InvariantCulture) : "");
            AppendChipIfPresent(sb, "Credits", model.CreditCategories.Sum(x => x.Roles.Sum(r => r.Credits.Count)) > 0 ? model.CreditCategories.Sum(x => x.Roles.Sum(r => r.Credits.Count)).ToString(CultureInfo.InvariantCulture) : "");
            AppendChipIfPresent(sb, "Description", model.DescriptionIsBoilerplate ? "Boilerplate" : "");
            sb.AppendLine("        </div>");

            if (!string.IsNullOrWhiteSpace(model.DescriptionHtml))
                sb.AppendLine($"        <div class=\"hero-description\">{model.DescriptionHtml}</div>");
            else
                sb.AppendLine("        <p class=\"hero-description empty-note\">No description was available in the info data.</p>");

            sb.AppendLine("      </div>");
            sb.AppendLine("    </div>");
            sb.AppendLine("  </header>");
        }

        private static void AppendOverviewSection(StringBuilder sb, BodyRenderData model)
        {
            if (!model.HasOverviewContent)
                return;

            sb.AppendLine("      <section class=\"section\">");
            sb.AppendLine("        <div class=\"section-header\">");
            sb.AppendLine("          <h2 class=\"section-title\">Overview</h2>");
            sb.AppendLine("        </div>");
            sb.AppendLine("        <div class=\"section-body\">");
            sb.AppendLine("          <div class=\"fact-grid\">");
            AppendFactCard(sb, "Developer", model.Developer);
            AppendFactCard(sb, "Players", model.Players);
            AppendFactCard(sb, "Genre", model.Genre);
            AppendFactCard(sb, "Startup Text", model.StartupText);
            sb.AppendLine("          </div>");

            if (model.Aka.Count > 0)
            {
                sb.AppendLine("          <div style=\"margin-top:18px;\">");
                sb.AppendLine("            <div class=\"kv\">");
                sb.AppendLine("              <div class=\"kv-key\">Also Known As</div>");
                sb.AppendLine("              <div class=\"kv-value\"><div class=\"list-chips\">");
                foreach (string item in model.Aka)
                    sb.AppendLine($"<span class=\"chip\">{H(item)}</span>");
                sb.AppendLine("              </div></div>");
                sb.AppendLine("            </div>");
                sb.AppendLine("          </div>");
            }

            if (model.AlsoOn.Count > 0)
            {
                sb.AppendLine("          <div style=\"margin-top:18px;\">");
                sb.AppendLine("            <div class=\"kv\">");
                sb.AppendLine("              <div class=\"kv-key\">Also On</div>");
                sb.AppendLine("              <div class=\"kv-value\"><div class=\"list-chips\">");
                foreach (string item in model.AlsoOn)
                    sb.AppendLine($"<span class=\"chip\">{H(item)}</span>");
                sb.AppendLine("              </div></div>");
                sb.AppendLine("            </div>");
                sb.AppendLine("          </div>");
            }

            sb.AppendLine("        </div>");
            sb.AppendLine("      </section>");
        }

        private static void AppendReleasesSection(StringBuilder sb, BodyRenderData model)
        {
            if (model.Releases.Count == 0)
                return;

            sb.AppendLine("      <section class=\"section\" id=\"releases-section\">");
            sb.AppendLine("        <button type=\"button\" class=\"section-header section-toggle\" data-toggle-target=\"releases-body\" aria-expanded=\"true\">");
            sb.AppendLine("          <span class=\"section-toggle-left\">");
            sb.AppendLine("            <h2 class=\"section-title\">Releases</h2>");
            sb.AppendLine("          </span>");
            sb.AppendLine("          <span class=\"toggle-caret\">▾</span>");
            sb.AppendLine("        </button>");
            sb.AppendLine("        <div id=\"releases-body\" class=\"section-body\">");
            sb.AppendLine("          <div class=\"release-list\">");

            for (int releaseIndex = 0; releaseIndex < model.Releases.Count; releaseIndex++)
                AppendReleaseCard(sb, model, releaseIndex);

            sb.AppendLine("          </div>");
            sb.AppendLine("        </div>");
            sb.AppendLine("      </section>");
        }
        private static void AppendReleaseCard(StringBuilder sb, BodyRenderData model, int releaseIndex)
        {
            ReleaseVm release = model.Releases[releaseIndex];
            List<ReleaseAssetVm> releaseAssetItems = model.ReleaseAssetItemsByRelease[releaseIndex];
            string releaseLightboxGroup = $"release-{releaseIndex.ToString(CultureInfo.InvariantCulture)}";

            ReleaseAssetVm? releaseThumbAsset = FindReleaseThumbAsset(releaseAssetItems);
            var galleryAssetItems = releaseAssetItems
                .Where(x => !string.Equals(x.Kind, R2DatWebFlag.Media.THUMB_RELEASE, StringComparison.OrdinalIgnoreCase))
                .ToList();

            bool hasRealReleaseThumb = releaseThumbAsset is not null && !string.IsNullOrWhiteSpace(releaseThumbAsset.Uri);

            string releaseTopPersonLabel = !string.IsNullOrWhiteSpace(model.Developer)
                ? "Developer"
                : !string.IsNullOrWhiteSpace(release.Publisher)
                    ? "Publisher"
                    : "";

            string releaseTopPersonValue = !string.IsNullOrWhiteSpace(model.Developer)
                ? model.Developer
                : release.Publisher;

            string releaseTopLeftClass = hasRealReleaseThumb ? "release-top-left" : "release-top-left no-thumb";
            string releaseBodyClass = galleryAssetItems.Count > 0 ? "release-body" : "release-body no-assets";

            sb.AppendLine("            <article class=\"release-card\">");
            sb.AppendLine("              <div class=\"release-top\">");
            sb.AppendLine($"                <div class=\"{releaseTopLeftClass}\">");

            if (hasRealReleaseThumb)
            {
                sb.AppendLine("                  <div class=\"release-thumb-wrap\">");
                sb.AppendLine($"                    <img class=\"release-thumb clickable\" data-asset=\"1\" data-no-tooltip=\"1\" data-lightbox-group=\"{HA(releaseLightboxGroup)}\" data-filename=\"{HA(ExtractFileNameFromUri(releaseThumbAsset!.Uri))}\" src=\"{HA(releaseThumbAsset.Uri)}\" alt=\"{HA(FirstNonEmpty(release.Name, model.Title))} thumbnail\" loading=\"lazy\" />");
                sb.AppendLine("                  </div>");
            }

            sb.AppendLine("                  <div class=\"release-top-text\">");
            sb.AppendLine($"                    <h3 class=\"release-title\">{H(FirstNonEmpty(release.Name, model.Title))}</h3>");
            sb.AppendLine("                    <div class=\"release-summary-line\">");
            AppendReleaseSummaryChip(sb, "Region", release.Region);
            AppendReleaseSummaryChip(sb, "Date", release.DateRaw);
            AppendReleaseSummaryChip(sb, releaseTopPersonLabel, releaseTopPersonValue);
            sb.AppendLine("                    </div>");
            sb.AppendLine("                  </div>");
            sb.AppendLine("                </div>");
            sb.AppendLine("              </div>");

            sb.AppendLine($"              <div class=\"{releaseBodyClass}\">");

            if (galleryAssetItems.Count > 0)
            {
                sb.AppendLine("                <div class=\"release-asset-stack\">");

                foreach (ReleaseAssetVm item in galleryAssetItems)
                {
                    sb.AppendLine("                  <div class=\"release-asset\">");
                    sb.AppendLine($"                    <img class=\"clickable\" data-asset=\"1\" data-lightbox-group=\"{HA(releaseLightboxGroup)}\" data-filename=\"{HA(ExtractFileNameFromUri(item.Uri))}\" src=\"{HA(item.Uri)}\" alt=\"{HA(item.Label)}\" loading=\"lazy\" />");
                    sb.AppendLine($"                    <div class=\"asset-label\">{H(item.Label)}</div>");
                    sb.AppendLine("                  </div>");
                }

                sb.AppendLine("                </div>");
            }

            sb.AppendLine("                <div class=\"release-details\">");
            sb.AppendLine("                  <div class=\"details-table\">");
            AppendDetailsRow(sb, "Region", release.Region);
            AppendDetailsRow(sb, "Date", release.DateRaw);
            AppendDetailsRow(sb, "Publisher", release.Publisher);
            AppendDetailsRow(sb, "Distributor", release.Distributor);
            AppendDetailsRow(sb, "Rating", release.Rating);
            AppendDetailsRow(sb, "Players", release.Players);
            AppendDetailsRow(sb, "Medium", release.Medium);
            AppendDetailsRow(sb, "Type", release.Type);
            AppendDetailsRow(sb, "Product ID", release.ProductId);
            AppendDetailsRow(sb, "Distribution / Barcode", release.DistributionOrBarcode);
            AppendDetailsRow(sb, "MAME Name", release.MameName);
            AppendDetailsRow(sb, "Comment", release.Comment);
            sb.AppendLine("                  </div>");

            if (release.InputAttributes.Count > 0)
            {
                sb.AppendLine("                  <div class=\"subsection-title\">Inputs</div>");
                sb.AppendLine("                  <div class=\"details-table\">");
                foreach (KvVm item in release.InputAttributes)
                    AppendDetailsRow(sb, item.Name, item.Value);
                sb.AppendLine("                  </div>");
            }

            if (release.EmulationStatuses.Count > 0)
            {
                sb.AppendLine("                  <div class=\"subsection-title\">Emulation Status</div>");
                sb.AppendLine("                  <div class=\"details-table\">");
                foreach (KvVm item in release.EmulationStatuses)
                    AppendDetailsRow(sb, item.Name, item.Value);
                sb.AppendLine("                  </div>");
            }

            sb.AppendLine("                </div>");
            sb.AppendLine("              </div>");
            sb.AppendLine("            </article>");
        }
        private static void AppendAssortedMediaSection(StringBuilder sb, BodyRenderData model)
        {
            if (model.OtherMediaGroups.Count == 0)
                return;

            sb.AppendLine("      <section class=\"section\" id=\"other-media-section\">");
            sb.AppendLine("        <button type=\"button\" class=\"section-header section-toggle\" data-toggle-target=\"other-media-body\" aria-expanded=\"true\">");
            sb.AppendLine("          <span class=\"section-toggle-left\">");
            sb.AppendLine("            <h2 class=\"section-title\">Assorted Media</h2>");
            sb.AppendLine("          </span>");
            sb.AppendLine("          <span class=\"toggle-caret\">▾</span>");
            sb.AppendLine("        </button>");
            sb.AppendLine("        <div id=\"other-media-body\" class=\"section-body\">");
            sb.AppendLine("          <div class=\"other-media-groups\">");

            for (int groupIndex = 0; groupIndex < model.OtherMediaGroups.Count; groupIndex++)
            {
                OtherMediaGroupVm group = model.OtherMediaGroups[groupIndex];
                string lightboxGroup = $"other-media-{groupIndex.ToString(CultureInfo.InvariantCulture)}";

                sb.AppendLine("            <div class=\"other-media-group\">");
                sb.AppendLine("              <div class=\"other-media-group-header\">");
                AppendOtherMediaGroupChip(sb, group.Title);
                sb.AppendLine("              </div>");
                sb.AppendLine("              <div class=\"other-media-grid\">");

                foreach (ReleaseAssetVm item in group.Items)
                {
                    sb.AppendLine("                <div class=\"release-asset\">");
                    sb.AppendLine($"                  <img class=\"clickable\" data-asset=\"1\" data-lightbox-group=\"{HA(lightboxGroup)}\" data-filename=\"{HA(ExtractFileNameFromUri(item.Uri))}\" src=\"{HA(item.Uri)}\" alt=\"{HA(item.Label)}\" loading=\"lazy\" />");
                    sb.AppendLine($"                  <div class=\"asset-label\">{H(item.Label)}</div>");
                    sb.AppendLine("                </div>");
                }

                sb.AppendLine("              </div>");
                sb.AppendLine("            </div>");
            }

            sb.AppendLine("          </div>");
            sb.AppendLine("        </div>");
            sb.AppendLine("      </section>");
        }
        private static void AppendOtherMediaGroupChip(StringBuilder sb, string value)
        {
            string safeValue = FirstNonEmpty(value, "Miscellaneous");

            if (string.Equals(safeValue, "Miscellaneous", StringComparison.OrdinalIgnoreCase))
            {
                sb.AppendLine($"<span class=\"release-summary-chip\">{H(safeValue)}</span>");
                return;
            }

            sb.AppendLine($"<span class=\"release-summary-chip\"><span class=\"release-summary-label\">Region:</span> {H(safeValue)}</span>");
        }
        private static void AppendVideosSection(StringBuilder sb, BodyRenderData model)
        {
            if (model.VideoAssets.Count == 0)
                return;

            sb.AppendLine("      <section class=\"section media-section\" id=\"videos-section\">");
            sb.AppendLine("        <button type=\"button\" class=\"section-header section-toggle\" data-toggle-target=\"videos-body\" aria-expanded=\"true\">");
            sb.AppendLine("          <span class=\"section-toggle-left\">");
            sb.AppendLine("            <h2 class=\"section-title\">Videos</h2>");
            sb.AppendLine("          </span>");
            sb.AppendLine("          <span class=\"toggle-caret\">▾</span>");
            sb.AppendLine("        </button>");
            sb.AppendLine("        <div id=\"videos-body\" class=\"section-body\">");
            sb.AppendLine("          <div class=\"standalone-media-grid video-media-grid\">");

            foreach (StandaloneMediaAssetVm video in model.VideoAssets)
            {
                sb.AppendLine("            <article class=\"standalone-media-card video-media-card\">");
                sb.AppendLine("              <div class=\"video-preview-shell\">");
                sb.AppendLine($"                <video class=\"video-preview\" controls preload=\"metadata\" src=\"{HA(video.Uri)}\"></video>");
                sb.AppendLine("              </div>");
                sb.AppendLine("              <div class=\"standalone-media-body\">");
                sb.AppendLine($"                <div class=\"standalone-media-title\">{H(video.Title)}</div>");
                sb.AppendLine($"                <a class=\"standalone-media-link\" href=\"{HA(video.Uri)}\" target=\"_blank\" rel=\"noopener\">Open video</a>");
                sb.AppendLine("              </div>");
                sb.AppendLine("            </article>");
            }

            sb.AppendLine("          </div>");
            sb.AppendLine("        </div>");
            sb.AppendLine("      </section>");
        }

        private static void AppendManualsSection(StringBuilder sb, BodyRenderData model)
        {
            if (model.ManualAssets.Count == 0)
                return;

            sb.AppendLine("      <section class=\"section media-section\" id=\"manuals-section\">");
            sb.AppendLine("        <button type=\"button\" class=\"section-header section-toggle\" data-toggle-target=\"manuals-body\" aria-expanded=\"true\">");
            sb.AppendLine("          <span class=\"section-toggle-left\">");
            sb.AppendLine("            <h2 class=\"section-title\">Manuals</h2>");
            sb.AppendLine("          </span>");
            sb.AppendLine("          <span class=\"toggle-caret\">▾</span>");
            sb.AppendLine("        </button>");
            sb.AppendLine("        <div id=\"manuals-body\" class=\"section-body\">");
            sb.AppendLine("          <div class=\"standalone-media-grid manual-media-grid\">");

            foreach (StandaloneMediaAssetVm manual in model.ManualAssets)
            {
                sb.AppendLine("            <article class=\"standalone-media-card manual-media-card\">");
                sb.AppendLine("              <div class=\"manual-preview-shell\">");
                sb.AppendLine($"                <iframe class=\"manual-preview\" src=\"{HA(manual.Uri)}\" title=\"{HA(manual.Title)}\" loading=\"lazy\"></iframe>");
                sb.AppendLine("              </div>");
                sb.AppendLine("              <div class=\"standalone-media-body\">");
                sb.AppendLine($"                <div class=\"standalone-media-title\">{H(manual.Title)}</div>");
                sb.AppendLine($"                <a class=\"standalone-media-link\" href=\"{HA(manual.Uri)}\" target=\"_blank\" rel=\"noopener\">Open manual</a>");
                sb.AppendLine("              </div>");
                sb.AppendLine("            </article>");
            }

            sb.AppendLine("          </div>");
            sb.AppendLine("        </div>");
            sb.AppendLine("      </section>");
        }
        private static void AppendScreenshotsSection(StringBuilder sb, BodyRenderData model)
        {
            if (model.AssetIndex.ScreenshotUris.Count == 0)
                return;

            sb.AppendLine("      <section class=\"section\" id=\"screenshots-section\">");
            sb.AppendLine("        <button type=\"button\" class=\"section-header section-toggle\" data-toggle-target=\"screenshots-body\" aria-expanded=\"true\">");
            sb.AppendLine("          <span class=\"section-toggle-left\">");
            sb.AppendLine("            <h2 class=\"section-title\">Screenshots</h2>");
            sb.AppendLine("          </span>");
            sb.AppendLine("          <span class=\"toggle-caret\">▾</span>");
            sb.AppendLine("        </button>");
            sb.AppendLine("        <div id=\"screenshots-body\" class=\"section-body\">");
            sb.AppendLine("          <div id=\"screenshots-grid\" class=\"media-grid\">");

            for (int i = 0; i < model.AssetIndex.ScreenshotUris.Count; i++)
            {
                string screenshotUri = model.AssetIndex.ScreenshotUris[i];
                string screenshotFileName = ExtractFileNameFromUri(screenshotUri);
                sb.AppendLine("            <article class=\"media-card\">");
                sb.AppendLine($"              <img class=\"clickable\" data-asset=\"1\" data-no-tooltip=\"1\" data-lightbox-group=\"screenshots\"{GetLightboxScaleAttribute(LightboxScreenshotScalePercent)} data-filename=\"{HA(screenshotFileName)}\" src=\"{HA(screenshotUri)}\" alt=\"Screenshot {i + 1}\" loading=\"lazy\" />");
                sb.AppendLine($"              <div class=\"media-caption\">Screenshot {i + 1}</div>");
                sb.AppendLine("            </article>");
            }

            sb.AppendLine("          </div>");
            sb.AppendLine("        </div>");
            sb.AppendLine("      </section>");
        }
        private static void AppendCreditsSection(StringBuilder sb, BodyRenderData model)
        {
            if (model.CreditCategories.Count == 0)
                return;

            sb.AppendLine("      <section class=\"section\" id=\"credits-section\">");
            sb.AppendLine("        <button type=\"button\" class=\"section-header section-toggle\" data-toggle-target=\"credits-body\" aria-expanded=\"true\">");
            sb.AppendLine("          <span class=\"section-toggle-left\">");
            sb.AppendLine("            <h2 class=\"section-title\">Credits</h2>");
            sb.AppendLine("          </span>");
            sb.AppendLine("          <span class=\"toggle-caret\">▾</span>");
            sb.AppendLine("        </button>");
            sb.AppendLine("        <div id=\"credits-body\" class=\"section-body\">");

            foreach (CreditCategoryVm category in model.CreditCategories)
            {
                sb.AppendLine("          <article class=\"credit-category-block\">");
                sb.AppendLine($"            <h3 class=\"credit-category\">{H(category.Category)}</h3>");
                sb.AppendLine("            <div class=\"credit-role-grid\">");

                foreach (CreditRoleVm role in category.Roles)
                {
                    sb.AppendLine("              <div class=\"credit-role-card\">");
                    sb.AppendLine($"                <h4 class=\"credit-role\">{H(role.Role)}</h4>");
                    sb.AppendLine("                <div class=\"credit-names\">");
                    foreach (CreditVm credit in role.Credits)
                        sb.AppendLine($"                  <span class=\"credit-name\">{H(credit.Name)}</span>");
                    sb.AppendLine("                </div>");
                    sb.AppendLine("              </div>");
                }

                sb.AppendLine("            </div>");
                sb.AppendLine("          </article>");
            }

            sb.AppendLine("        </div>");
            sb.AppendLine("      </section>");
        }

        private static void AppendAsideColumn(StringBuilder sb, BodyRenderData model)
        {
            if (!model.HasAsideContent)
                return;

            sb.AppendLine("    <div class=\"stack\">");

            AppendSideScreenshotFeature(sb, model);
            AppendEmulationSection(sb, model);
            AppendMiscSection(sb, model);
            AppendCompilationsSection(sb, model);

            sb.AppendLine("    </div>");
        }

        private static void AppendSideScreenshotFeature(StringBuilder sb, BodyRenderData model)
        {
            if (!model.ShowSideScreenshotFeature)
                return;

            string sideScreenshotUri = model.AssetIndex.ScreenshotUris[0];
            string sideScreenshotFileName = ExtractFileNameFromUri(sideScreenshotUri);

            sb.AppendLine("      <section class=\"section side-feature-shot\">");
            sb.AppendLine("        <div class=\"section-body side-feature-shot-body\">");
            sb.AppendLine($"          <img class=\"clickable side-feature-shot-image\" data-asset=\"1\" data-no-tooltip=\"1\"{GetLightboxScaleAttribute(LightboxScreenshotScalePercent)} data-filename=\"{HA(sideScreenshotFileName)}\" src=\"{HA(sideScreenshotUri)}\" alt=\"Featured screenshot\" loading=\"lazy\" />");
            sb.AppendLine("        </div>");
            sb.AppendLine("      </section>");
        }
        private static void AppendEmulationSection(StringBuilder sb, BodyRenderData model)
        {
            if (string.IsNullOrWhiteSpace(model.EmulationHistory))
                return;

            sb.AppendLine("      <section class=\"section\">");
            sb.AppendLine("        <div class=\"section-header\">");
            sb.AppendLine("          <h2 class=\"section-title\">Emulation</h2>");
            sb.AppendLine("          <div class=\"section-subtitle\">History / notes</div>");
            sb.AppendLine("        </div>");
            sb.AppendLine("        <div class=\"section-body\">");
            sb.AppendLine($"          <div class=\"kv-value\">{H(model.EmulationHistory).Replace("\r\n", "<br/>").Replace("\n", "<br/>")}</div>");
            sb.AppendLine("        </div>");
            sb.AppendLine("      </section>");
        }

        private static void AppendMiscSection(StringBuilder sb, BodyRenderData model)
        {
            if (model.Misc.Count == 0)
                return;

            sb.AppendLine("      <section class=\"section\">");
            sb.AppendLine("        <div class=\"section-header\">");
            sb.AppendLine("          <h2 class=\"section-title\">Miscellaneous</h2>");
            sb.AppendLine("        </div>");
            sb.AppendLine("        <div class=\"section-body\">");

            foreach (MiscVm item in model.Misc)
            {
                sb.AppendLine("          <div class=\"kv\">");
                sb.AppendLine($"            <div class=\"kv-key\">{H(item.Name)}</div>");
                sb.AppendLine($"            <div class=\"kv-value\">{H(item.Value)}</div>");
                sb.AppendLine("          </div>");
            }

            sb.AppendLine("        </div>");
            sb.AppendLine("      </section>");
        }

        private static void AppendCompilationsSection(StringBuilder sb, BodyRenderData model)
        {
            if (model.Compilations.Count == 0)
                return;

            sb.AppendLine("      <section class=\"section\">");
            sb.AppendLine("        <div class=\"section-header\">");
            sb.AppendLine("          <h2 class=\"section-title\">Compilations</h2>");
            sb.AppendLine("        </div>");
            sb.AppendLine("        <div class=\"section-body\">");

            foreach (CompilationVm item in model.Compilations)
            {
                sb.AppendLine("          <div class=\"kv\">");
                sb.AppendLine($"            <div class=\"kv-key\">{H(item.Name)}</div>");
                sb.AppendLine($"            <div class=\"kv-value\">{H(item.System)}</div>");
                sb.AppendLine("          </div>");
            }

            sb.AppendLine("        </div>");
            sb.AppendLine("      </section>");
        }

        private static void AppendLightbox(StringBuilder sb)
        {
            sb.AppendLine("  <div id=\"lightbox\" class=\"lightbox\">");
            sb.AppendLine("    <button id=\"lightbox-prev\" type=\"button\" class=\"lightbox-nav lightbox-nav-prev hidden\" aria-label=\"Previous image\">&#8249;</button>");
            sb.AppendLine("    <div class=\"lightbox-inner\">");
            sb.AppendLine("      <img id=\"lightbox-image\" alt=\"Expanded image\" />");
            sb.AppendLine("      <div id=\"lightbox-caption\" class=\"lightbox-caption\"></div>");
            sb.AppendLine("    </div>");
            sb.AppendLine("    <button id=\"lightbox-next\" type=\"button\" class=\"lightbox-nav lightbox-nav-next hidden\" aria-label=\"Next image\">&#8250;</button>");
            sb.AppendLine("  </div>");
        }
        private sealed class BodyRenderData
        {
            public string Title { get; set; } = "";
            public string Lookup { get; set; } = "";
            public string SystemLookup { get; set; } = "";
            public string SourceDisplayName { get; set; } = "";
            public string Developer { get; set; } = "";
            public string Genre { get; set; } = "";
            public string Players { get; set; } = "";
            public string StartupText { get; set; } = "";
            public string DescriptionHtml { get; set; } = "";
            public bool DescriptionIsBoilerplate { get; set; }
            public string EmulationHistory { get; set; } = "";
            public List<string> Aka { get; set; } = new();
            public List<string> AlsoOn { get; set; } = new();
            public List<ReleaseVm> Releases { get; set; } = new();
            public List<MiscVm> Misc { get; set; } = new();
            public List<CompilationVm> Compilations { get; set; } = new();
            public List<CreditCategoryVm> CreditCategories { get; set; } = new();
            public FileAssetIndex AssetIndex { get; set; } = new FileAssetIndex();
            public string HeroThumbUri { get; set; } = "";
            public List<string> HeroBackdropUris { get; set; } = new();
            public string HeroBackdropUri { get; set; } = "";
            public bool HasHeroThumb { get; set; }
            public bool HasOverviewContent { get; set; }
            public bool HasAsideContent { get; set; }
            public bool ShowSideScreenshotFeature { get; set; }
            public string PageGridClass { get; set; } = "";
            public List<List<ReleaseAssetVm>> ReleaseAssetItemsByRelease { get; set; } = new();
            public List<OtherMediaGroupVm> OtherMediaGroups { get; set; } = new();
            public string Url { get; set; } = "";

            public IReadOnlyList<StandaloneMediaAssetVm> VideoAssets { get; init; } = Array.Empty<StandaloneMediaAssetVm>();
            public IReadOnlyList<StandaloneMediaAssetVm> ManualAssets { get; init; } = Array.Empty<StandaloneMediaAssetVm>();
        }
        private static List<StandaloneMediaAssetVm> BuildStandaloneMediaAssets(
            IReadOnlyList<RootAssetFile> rootAssets,
            string kind,
            string extension)
        {
            return rootAssets
                .Where(asset =>
                    string.Equals(asset.Kind, kind, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(Path.GetExtension(asset.FullPath), extension, StringComparison.OrdinalIgnoreCase))
                .Select(asset => new StandaloneMediaAssetVm(
                    MakeStandaloneMediaTitle(asset),
                    Path.GetFileName(asset.FullPath),
                    asset.Uri))
                .OrderBy(asset => asset.Title, StringComparer.OrdinalIgnoreCase)
                .ThenBy(asset => asset.FileName, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static string MakeStandaloneMediaTitle(RootAssetFile asset)
        {
            string title = FirstNonEmpty(asset.Title, Path.GetFileNameWithoutExtension(asset.FullPath), "Untitled");
            string region = FirstNonEmpty(asset.Region, "");

            if (string.IsNullOrWhiteSpace(region))
                return title;

            return $"{title} [{region}]";
        }
        private static List<OtherMediaGroupVm> BuildOtherMediaGroups(
            FileAssetIndex assetIndex,
            ISet<string> usedReleaseAssetUris)
        {
            return assetIndex.RootAssets
                .Where(IsImageRootAsset)
                .Where(x => !string.IsNullOrWhiteSpace(x.Uri))
                .Where(x => !usedReleaseAssetUris.Contains(x.Uri))
                .Where(x => !IsThumbAssetKind(x.Kind))
                            .GroupBy(x => GetOtherMediaGroupTitle(x))
                .Select(g => new OtherMediaGroupVm
                {
                    Title = g.Key,
                    Items = g
                        .OrderBy(x => GetOrderedReleaseAssetKindIndex(x.Kind))
                        .ThenBy(x => x.FileName, StringComparer.OrdinalIgnoreCase)
                        .Select(x => new ReleaseAssetVm
                        {
                            Kind = x.Kind,
                            Label = MakeAssetLabel(x.Kind),
                            Uri = x.Uri
                        })
                        .ToList()
                })
                .Where(x => x.Items.Count > 0)
                .OrderBy(x => string.Equals(x.Title, "Miscellaneous", StringComparison.OrdinalIgnoreCase) ? 1 : 0)
                .ThenBy(x => x.Title, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static string GetOtherMediaGroupTitle(RootAssetFile asset)
        {
            string region = NormalizeRegionGroupLabel(asset.Region);

            if (!string.IsNullOrWhiteSpace(region))
                return region;

            return "Miscellaneous";
        }

        private static string NormalizeRegionGroupLabel(string? value)
        {
            string raw = FirstNonEmpty(value, "");
            if (string.IsNullOrWhiteSpace(raw))
                return "";

            raw = raw.Trim();

            if (IsLikelyRegionCode(raw))
                return raw.ToUpperInvariant();

            string[] parts = raw
                .Split(new[] { ',', '/', '+', '&' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToArray();

            if (parts.Length == 0)
                return "";

            if (parts.All(IsLikelyRegionCode))
                return string.Join(",", parts.Select(x => x.ToUpperInvariant()));

            return "";
        }

        private static bool IsLikelyRegionCode(string? value)
        {
            string raw = FirstNonEmpty(value, "");
            if (string.IsNullOrWhiteSpace(raw))
                return false;

            raw = raw.Trim();

            if (raw.Length < 2 || raw.Length > 3)
                return false;

            foreach (char c in raw)
            {
                if (!char.IsLetter(c))
                    return false;
            }

            return true;
        }
        private static bool IsThumbAssetKind(string? kind)
        {
            string value = FirstNonEmpty(kind, "");

            if (string.IsNullOrWhiteSpace(value))
                return false;

            return string.Equals(value, R2DatWebFlag.Media.THUMB_RELEASE, StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "thumb", StringComparison.OrdinalIgnoreCase)
                || value.IndexOf("thumb", StringComparison.OrdinalIgnoreCase) >= 0;
        }
        private static int GetOrderedReleaseAssetKindIndex(string? kind)
        {
            string value = FirstNonEmpty(kind, "");

            for (int i = 0; i < OrderedReleaseAssetKinds.Length; i++)
            {
                if (string.Equals(OrderedReleaseAssetKinds[i], value, StringComparison.OrdinalIgnoreCase))
                    return i;
            }

            return int.MaxValue;
        }

        private static readonly string[] OrderedReleaseAssetKinds =
        {
            R2DatWebFlag.Media.THUMB_RELEASE,
            R2DatWebFlag.Media.BOX,
            R2DatWebFlag.Media.BOX_BACK,
            R2DatWebFlag.Media.BOX_SIDE,
            R2DatWebFlag.Media.BOX_TOP,
            R2DatWebFlag.Media.BOX_BOTTOM,
            R2DatWebFlag.Media.BOX_INLAY,
            R2DatWebFlag.Media.MEDIA,
            R2DatWebFlag.Media.MEDIA_BACK,
            R2DatWebFlag.Media.MANUAL_FRONT,
            R2DatWebFlag.Media.MANUAL_BACK,
            R2DatWebFlag.Media.MANUAL,
            R2DatWebFlag.Media.VIDEO,
            R2DatWebFlag.Media.OTHER_ADVERT,
            R2DatWebFlag.Media.OTHER_REFERENCE_CARD,
            R2DatWebFlag.Media.OTHER_MAP,
            R2DatWebFlag.Media.OTHER_HARDWARE,
            R2DatWebFlag.Media.OTHER
        };
        private static string BuildScriptBlock()
        {
            return @"<script>
(function(){
    function getFileNameFromImage(img) {
        var explicit = img.getAttribute('data-filename');
        if (explicit) return explicit;

        try {
            var src = img.getAttribute('src') || '';
            if (!src || src.indexOf('data:') === 0) return '';
            var url = new URL(src, window.location.href);
            var path = decodeURIComponent(url.pathname || '');
            var parts = path.split('/');
            return parts.length ? parts[parts.length - 1] : '';
        } catch (e) {
            return '';
        }
    }

    function getLightboxScalePercent(img) {
        var raw = img.getAttribute('data-lightbox-scale');
        var value = parseInt(raw || '', 10);
        if (!isFinite(value) || value <= 0) return __LIGHTBOX_DEFAULT_SCALE_PERCENT__;
        return value;
    }

    function clearLightboxImagePresentation(lightboxImage) {
        lightboxImage.style.width = '';
        lightboxImage.style.height = '';
    }

    function applyLightboxImagePresentation(lightboxImage) {
        var raw = lightboxImage.getAttribute('data-lightbox-scale-active');
        var scalePercent = parseInt(raw || '', 10);
        if (!isFinite(scalePercent) || scalePercent <= 0) scalePercent = __LIGHTBOX_DEFAULT_SCALE_PERCENT__;

        if (!lightboxImage.naturalWidth || !lightboxImage.naturalHeight) {
            clearLightboxImagePresentation(lightboxImage);
            return;
        }

        var targetWidth = Math.round(lightboxImage.naturalWidth * scalePercent / 100);
        lightboxImage.style.width = targetWidth + 'px';
        lightboxImage.style.height = 'auto';
    }

    function getImageIdentity(img) {
        return (img.getAttribute('src') || '') + '|' + getFileNameFromImage(img);
    }

    function getGroupedLightboxItems(img) {
        var group = img.getAttribute('data-lightbox-group') || '';
        if (!group) return [img];

        var seen = Object.create(null);
        var items = [];

        document.querySelectorAll('img.clickable[data-lightbox-group=""' + group + '""]').forEach(function(candidate) {
            if (!candidate || !candidate.src) return;

            var key = getImageIdentity(candidate);
            if (seen[key]) return;

            seen[key] = true;
            items.push(candidate);
        });

        if (!items.length)
            items.push(img);

        return items;
    }

    function isSelectionSensitiveTarget(target) {
        return !!(target && target.closest('img[data-asset], #lightbox-image, .lightbox-nav'));
    }

    function clearSelection() {
        if (!window.getSelection) return;

        var selection = window.getSelection();
        if (!selection) return;

        try {
            selection.removeAllRanges();
        } catch (e) {
        }
    }

    function setLightboxScrollLock(isLocked) {
        if (!document.documentElement || !document.body) return;

        document.documentElement.classList.toggle('lightbox-open', isLocked);
        document.body.classList.toggle('lightbox-open', isLocked);
    }

    var assetTooltip = document.createElement('div');
    assetTooltip.id = 'asset-tooltip';
    assetTooltip.className = 'asset-tooltip';
    document.body.appendChild(assetTooltip);

    function hideAssetTooltip() {
        assetTooltip.classList.remove('open');
        assetTooltip.textContent = '';
    }

    function positionAssetTooltip(evt) {
        if (!assetTooltip.classList.contains('open')) return;

        var margin = 14;
        var offsetX = 16;
        var offsetY = 20;

        var left = evt.clientX + offsetX;
        var top = evt.clientY + offsetY;

        var tooltipWidth = assetTooltip.offsetWidth;
        var tooltipHeight = assetTooltip.offsetHeight;

        var maxLeft = window.innerWidth - tooltipWidth - margin;
        var maxTop = window.innerHeight - tooltipHeight - margin;

        if (left > maxLeft)
            left = Math.max(margin, evt.clientX - tooltipWidth - offsetX);

        if (top > maxTop)
            top = Math.max(margin, evt.clientY - tooltipHeight - offsetY);

        assetTooltip.style.left = left + 'px';
        assetTooltip.style.top = top + 'px';
    }

    function showAssetTooltip(img, evt) {
        var text = getFileNameFromImage(img);
        if (!text) {
            hideAssetTooltip();
            return;
        }

        assetTooltip.textContent = text;
        assetTooltip.classList.add('open');
        positionAssetTooltip(evt);
    }

    document.querySelectorAll('img[data-asset]').forEach(function(img) {
        img.addEventListener('load', function() {
            console.log('IMG LOAD OK:', img.src, 'natural=', img.naturalWidth + 'x' + img.naturalHeight);
        });

        img.addEventListener('error', function() {
            console.error('IMG LOAD FAIL:', img.src);
            img.style.outline = '3px solid red';
            img.style.minHeight = '80px';
            img.style.background = 'rgba(255,0,0,.15)';
        });

        if (img.getAttribute('data-no-tooltip') !== '1') {
            img.addEventListener('mouseenter', function(evt) {
                showAssetTooltip(img, evt);
            });

            img.addEventListener('mousemove', function(evt) {
                positionAssetTooltip(evt);
            });

            img.addEventListener('mouseleave', function() {
                hideAssetTooltip();
            });
        }
    });

    document.addEventListener('dragstart', function(evt) {
        if (isSelectionSensitiveTarget(evt.target)) {
            evt.preventDefault();
            clearSelection();
        }
    });

    document.addEventListener('selectstart', function(evt) {
        if (isSelectionSensitiveTarget(evt.target)) {
            evt.preventDefault();
            clearSelection();
        }
    });

    document.addEventListener('mousedown', function(evt) {
        if (isSelectionSensitiveTarget(evt.target)) {
            clearSelection();
        }
    });

    document.querySelectorAll('.section-toggle').forEach(function(btn) {
        btn.addEventListener('click', function() {
            var targetId = btn.getAttribute('data-toggle-target');
            if (!targetId) return;

            var target = document.getElementById(targetId);
            if (!target) return;

            var isCollapsed = target.classList.contains('collapsed');
            target.classList.toggle('collapsed', !isCollapsed);
            btn.classList.toggle('collapsed', !isCollapsed);
            btn.setAttribute('aria-expanded', isCollapsed ? 'true' : 'false');
        });
    });

    var lightbox = document.getElementById('lightbox');
    var lightboxImage = document.getElementById('lightbox-image');
    var lightboxCaption = document.getElementById('lightbox-caption');
    var lightboxPrev = document.getElementById('lightbox-prev');
    var lightboxNext = document.getElementById('lightbox-next');

    var lightboxItems = [];
    var currentLightboxIndex = -1;

    function updateLightboxNav() {
        if (!lightboxPrev || !lightboxNext) return;

        var canGoPrev = currentLightboxIndex > 0;
        var canGoNext = currentLightboxIndex >= 0 && currentLightboxIndex < lightboxItems.length - 1;
        var showNav = lightboxItems.length > 1;

        lightboxPrev.classList.toggle('hidden', !showNav || !canGoPrev);
        lightboxNext.classList.toggle('hidden', !showNav || !canGoNext);
    }

    function renderCurrentLightboxItem() {
        if (!lightboxImage || !lightboxCaption) return;
        if (currentLightboxIndex < 0 || currentLightboxIndex >= lightboxItems.length) return;

        var img = lightboxItems[currentLightboxIndex];
        if (!img || !img.src) return;

        var scalePercent = getLightboxScalePercent(img);

        clearLightboxImagePresentation(lightboxImage);
        lightboxImage.setAttribute('data-lightbox-scale-active', String(scalePercent));
        lightboxImage.onload = function() {
            applyLightboxImagePresentation(lightboxImage);
        };

        lightboxImage.src = img.src;
        lightboxCaption.textContent = getFileNameFromImage(img);

        if (lightboxImage.complete) {
            applyLightboxImagePresentation(lightboxImage);
        }

        updateLightboxNav();
    }

    function openLightboxFromImage(img) {
        if (!lightbox || !lightboxImage || !lightboxCaption) return;

        hideAssetTooltip();
        clearSelection();

        lightboxItems = getGroupedLightboxItems(img);
        currentLightboxIndex = 0;

        var clickedIdentity = getImageIdentity(img);
        for (var i = 0; i < lightboxItems.length; i++) {
            if (getImageIdentity(lightboxItems[i]) === clickedIdentity) {
                currentLightboxIndex = i;
                break;
            }
        }

        renderCurrentLightboxItem();
        lightbox.classList.add('open');
        setLightboxScrollLock(true);
    }

    function closeLightbox() {
        if (!lightbox || !lightboxImage || !lightboxCaption) return;

        lightbox.classList.remove('open');
        setLightboxScrollLock(false);
        lightboxItems = [];
        currentLightboxIndex = -1;
        lightboxImage.removeAttribute('src');
        lightboxImage.removeAttribute('data-lightbox-scale-active');
        lightboxImage.onload = null;
        clearLightboxImagePresentation(lightboxImage);
        lightboxCaption.textContent = '';
        clearSelection();
        updateLightboxNav();
    }

    function showPreviousLightboxItem() {
        if (currentLightboxIndex <= 0) return;
        currentLightboxIndex--;
        clearSelection();
        renderCurrentLightboxItem();
    }

    function showNextLightboxItem() {
        if (currentLightboxIndex < 0 || currentLightboxIndex >= lightboxItems.length - 1) return;
        currentLightboxIndex++;
        clearSelection();
        renderCurrentLightboxItem();
    }

    if (lightbox && lightboxImage && lightboxCaption) {
        document.addEventListener('click', function(evt) {
            var img = evt.target.closest('img.clickable');
            if (!img || !img.src) return;

            openLightboxFromImage(img);
        });

        lightboxImage.addEventListener('click', function(evt) {
            if (evt.button === 0) {
                closeLightbox();
            }
        });

        if (lightboxPrev) {
            lightboxPrev.addEventListener('click', function(evt) {
                evt.preventDefault();
                evt.stopPropagation();
                showPreviousLightboxItem();
            });
        }

        if (lightboxNext) {
            lightboxNext.addEventListener('click', function(evt) {
                evt.preventDefault();
                evt.stopPropagation();
                showNextLightboxItem();
            });
        }

        lightbox.addEventListener('click', function(evt) {
            if (evt.target === lightbox || evt.target.closest('.lightbox-close')) {
                closeLightbox();
            }
        });

        lightbox.addEventListener('wheel', function(evt) {
            if (!lightbox.classList.contains('open'))
                return;

            evt.preventDefault();
            evt.stopPropagation();
        }, { passive: false });

        document.addEventListener('keydown', function(evt) {
            if (evt.key === 'Escape') {
                hideAssetTooltip();
                closeLightbox();
                return;
            }

            if (!lightbox.classList.contains('open'))
                return;

            if (evt.key === 'ArrowLeft') {
                evt.preventDefault();
                showPreviousLightboxItem();
                return;
            }

            if (evt.key === 'ArrowRight') {
                evt.preventDefault();
                showNextLightboxItem();
            }
        });
    }

    window.addEventListener('scroll', hideAssetTooltip, true);
    window.addEventListener('blur', function() {
        hideAssetTooltip();
        if (!document.hasFocus()) {
            setLightboxScrollLock(false);
        }
    });
})();
</script>"
                .Replace("__LIGHTBOX_DEFAULT_SCALE_PERCENT__", LightboxDefaultScalePercent.ToString(CultureInfo.InvariantCulture));
        }
        private static FileAssetIndex BuildFileAssetIndex(string? assetRootPath)
        {
            var index = new FileAssetIndex();

            if (string.IsNullOrWhiteSpace(assetRootPath))
                return index;

            string rootPath;

            try
            {
                rootPath = Path.GetFullPath(assetRootPath);
            }
            catch
            {
                return index;
            }

            if (!Directory.Exists(rootPath))
                return index;

            index.RootPath = rootPath;

            foreach (string filePath in Directory.GetFiles(rootPath))
            {
                string fileName = Path.GetFileName(filePath);
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                string extension = Path.GetExtension(fileName);

                if (!IsSupportedRootAssetExtension(extension))
                    continue;

                string uri = BuildBrowserSafeFileUri(filePath);

                if (string.Equals(fileNameWithoutExtension, "[thumb]", StringComparison.OrdinalIgnoreCase) &&
                    IsSupportedImageExtension(extension))
                {
                    if (string.IsNullOrWhiteSpace(index.ThumbUri))
                        index.ThumbUri = uri;
                    continue;
                }

                RootAssetFile? parsed = TryParseRootAssetFile(filePath);
                if (parsed is not null)
                {
                    parsed.Uri = uri;
                    index.RootAssets.Add(parsed);
                    index.RootAssetsByFileName[parsed.FileName] = parsed;
                }
            }

            string screenFolderPath = Path.Combine(rootPath, "screen");

            if (Directory.Exists(screenFolderPath))
            {
                foreach (string filePath in Directory
                             .GetFiles(screenFolderPath)
                             .Where(x => IsSupportedImageExtension(Path.GetExtension(x)))
                             .OrderBy(GetScreenSortKey)
                             .ThenBy(x => Path.GetFileName(x), StringComparer.OrdinalIgnoreCase))
                {
                    index.ScreenshotUris.Add(BuildBrowserSafeFileUri(filePath));
                }
            }

            return index;
        }

        private static RootAssetFile? TryParseRootAssetFile(string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            string baseName = Path.GetFileNameWithoutExtension(fileName);
            return TryParseRootAssetFileName(baseName, fileName, filePath);
        }

        private static RootAssetFile? TryParseRootAssetFileName(string baseName, string fileName, string fullPath)
        {
            if (string.IsNullOrWhiteSpace(baseName))
                return null;

            if (!baseName.StartsWith("[", StringComparison.Ordinal))
                return null;

            string remaining = baseName;
            var tokens = new List<string>();
            bool hasImmediateSecondBracketToken = false;

            while (remaining.StartsWith("[", StringComparison.Ordinal))
            {
                int closeIndex = remaining.IndexOf(']');
                if (closeIndex <= 1)
                    break;

                tokens.Add(remaining.Substring(1, closeIndex - 1).Trim());

                string afterToken = remaining[(closeIndex + 1)..];

                if (tokens.Count == 1)
                    hasImmediateSecondBracketToken = afterToken.StartsWith("[", StringComparison.Ordinal);

                remaining = afterToken.TrimStart();
            }

            if (tokens.Count == 0)
                return null;

            string kind = NormalizeMediaKind(tokens[0]);
            string region = tokens.Count > 1 ? tokens[1].Trim() : "";
            string dateKey = tokens.Count > 2 ? NormalizeDateToken(tokens[2]) : "";
            string title = remaining.Trim();

            return new RootAssetFile
            {
                FileName = fileName,
                FullPath = fullPath,
                Uri = "",
                Kind = kind,
                Region = region,
                DateKey = dateKey,
                Title = title,
                NormalizedTitle = NormalizeIdentity(title),
                Tokens = tokens,
                HasImmediateSecondBracketToken = hasImmediateSecondBracketToken
            };
        }
        private static List<ReleaseAssetVm> BuildReleaseAssetItems(FileAssetIndex assetIndex, ReleaseVm release, string fallbackInfoTitle)
        {
            var result = new List<ReleaseAssetVm>();

            RootAssetFile? anchor = FindReleaseAnchorAsset(assetIndex, release);
            bool hasExplicitAssetKey = !string.IsNullOrWhiteSpace(release.AssetKey);

            string anchorNormalizedTitle = FirstNonEmpty(anchor?.NormalizedTitle, "");
            string releaseRegion = FirstNonEmpty(anchor?.Region, release.Region);
            string releaseDateKey = FirstNonEmpty(anchor?.DateKey, NormalizeDateToken(release.DateRaw));
            string exactAssetFileName = hasExplicitAssetKey
                ? Path.GetFileName(release.AssetKey)
                : "";

            var titleCandidates = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            AddNormalizedIfPresent(titleCandidates, anchor?.Title);
            AddNormalizedIfPresent(titleCandidates, release.Name);
            AddNormalizedIfPresent(titleCandidates, fallbackInfoTitle);

            List<RootAssetFile> candidatePool = assetIndex.RootAssets
                .Where(IsImageRootAsset)
                .Where(asset => hasExplicitAssetKey || !string.IsNullOrWhiteSpace(asset.NormalizedTitle))
                .ToList();

            if (!hasExplicitAssetKey)
            {
                candidatePool = candidatePool
                    .Where(x => x.HasImmediateSecondBracketToken)
                    .ToList();

                if (candidatePool.Count == 0)
                    return result;
            }

            if (anchor is not null)
            {
                var siblingPool = candidatePool
                    .Where(x =>
                        SameOrBlank(anchor.Region, x.Region) &&
                        SameOrBlank(anchor.DateKey, x.DateKey) &&
                        SameOrBlank(anchorNormalizedTitle, x.NormalizedTitle))
                    .ToList();

                if (siblingPool.Count > 0)
                    candidatePool = siblingPool;
            }

            foreach (string kind in OrderedReleaseAssetKinds)
            {
                RootAssetFile? best = candidatePool
                    .Where(x => string.Equals(x.Kind, kind, StringComparison.OrdinalIgnoreCase))
                    .Select(x => new
                    {
                        Asset = x,
                        Score = ScoreReleaseAsset(x, exactAssetFileName, releaseRegion, releaseDateKey, titleCandidates)
                    })
                    .Where(x => x.Score > 0)
                    .OrderByDescending(x => x.Score)
                    .ThenBy(x => x.Asset.FileName, StringComparer.OrdinalIgnoreCase)
                    .Select(x => x.Asset)
                    .FirstOrDefault();

                if (best is not null)
                {
                    result.Add(new ReleaseAssetVm
                    {
                        Kind = best.Kind,
                        Label = MakeAssetLabel(best.Kind),
                        Uri = best.Uri
                    });
                }
            }

            return result;
        }
        private static RootAssetFile? FindReleaseAnchorAsset(FileAssetIndex assetIndex, ReleaseVm release)
        {
            if (!string.IsNullOrWhiteSpace(release.AssetKey))
            {
                string assetKeyFileName = Path.GetFileName(release.AssetKey);

                if (assetIndex.RootAssetsByFileName.TryGetValue(assetKeyFileName, out RootAssetFile? exact))
                    return exact;

                string baseName = Path.GetFileNameWithoutExtension(assetKeyFileName);
                RootAssetFile? parsed = TryParseRootAssetFileName(baseName, assetKeyFileName, release.AssetKey);
                if (parsed is not null)
                    return parsed;
            }

            return null;
        }

        private static int ScoreReleaseAsset(
            RootAssetFile asset,
            string exactAssetFileName,
            string releaseRegion,
            string releaseDateKey,
            HashSet<string> normalizedTitleCandidates)
        {
            int score = 1;

            if (!string.IsNullOrWhiteSpace(exactAssetFileName) &&
                string.Equals(asset.FileName, exactAssetFileName, StringComparison.OrdinalIgnoreCase))
                score += 1000;

            if (!string.IsNullOrWhiteSpace(releaseRegion))
            {
                if (string.Equals(asset.Region, releaseRegion, StringComparison.OrdinalIgnoreCase))
                    score += 200;
                else if (!string.IsNullOrWhiteSpace(asset.Region))
                    score -= 200;
            }

            if (!string.IsNullOrWhiteSpace(releaseDateKey))
            {
                if (string.Equals(asset.DateKey, releaseDateKey, StringComparison.OrdinalIgnoreCase))
                    score += 150;
                else if (!string.IsNullOrWhiteSpace(asset.DateKey))
                    score -= 120;
            }

            if (!string.IsNullOrWhiteSpace(asset.NormalizedTitle) &&
                normalizedTitleCandidates.Contains(asset.NormalizedTitle))
                score += 120;

            return score;
        }

        private static bool IsSupportedImageExtension(string extension)
        {
            return
                string.Equals(extension, ".jpg", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(extension, ".jpeg", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(extension, ".png", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(extension, ".webp", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(extension, ".gif", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsSupportedRootAssetExtension(string extension)
        {
            return
                IsSupportedImageExtension(extension) ||
                string.Equals(extension, ".mp4", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase);
        }
        private static bool IsImageRootAsset(RootAssetFile asset)
        {
            return IsSupportedImageExtension(Path.GetExtension(asset.FullPath));
        }

        private static int GetScreenSortKey(string filePath)
        {
            string name = Path.GetFileNameWithoutExtension(filePath);
            string digits = new string(name.Where(char.IsDigit).ToArray());

            if (int.TryParse(digits, out int value))
                return value;

            return int.MaxValue;
        }

        private static DateTime? TryParseLooseDate(string? rawDate)
        {
            if (string.IsNullOrWhiteSpace(rawDate))
                return null;

            string date = rawDate.Trim();

            if (date.All(char.IsDigit) && date.Length == 4)
            {
                if (int.TryParse(date, out int year))
                    return new DateTime(year, 1, 1);
            }

            string[] formats =
            {
                "MM/dd/yy", "M/d/yy", "MM/d/yy", "M/dd/yy",
                "MM/dd/yyyy", "M/d/yyyy", "MM/d/yyyy", "M/dd/yyyy"
            };

            if (DateTime.TryParseExact(date, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsed))
                return parsed;

            return null;
        }

        private static string NormalizeMediaKind(string value)
        {
            if (IsVideoMediaKindToken(value))
                return R2DatWebFlag.Media.VIDEO;

            if (IsManualMediaKindToken(value))
                return R2DatWebFlag.Media.MANUAL;

            string? match = R2DatWebFlag.Media.GetMatch(value);

            if (!string.IsNullOrWhiteSpace(match))
                return match;

            return value.Trim();
        }

        private static bool IsVideoMediaKindToken(string value)
        {
            string token = FirstNonEmpty(value, "").Trim();

            return
                string.Equals(token, "video", StringComparison.OrdinalIgnoreCase) ||
                token.StartsWith("video - ", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsManualMediaKindToken(string value)
        {
            string token = FirstNonEmpty(value, "").Trim();

            return
                string.Equals(token, "manual", StringComparison.OrdinalIgnoreCase) ||
                token.StartsWith("manual - ", StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeDateToken(string? value)
        {
            string raw = FirstNonEmpty(value, "");
            if (string.IsNullOrWhiteSpace(raw))
                return "";

            if (raw.All(char.IsDigit) && raw.Length == 4)
                return raw;

            raw = raw.Replace('/', '-').Replace('.', '-').Trim();

            if (DateTime.TryParseExact(
                    raw,
                    new[] { "MM-dd-yy", "M-d-yy", "MM-d-yy", "M-dd-yy", "MM-dd-yyyy", "M-d-yyyy", "MM-d-yyyy", "M-dd-yyyy" },
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime parsed))
            {
                return parsed.ToString("MM-dd-yy", CultureInfo.InvariantCulture);
            }

            return raw;
        }

        private static string NormalizeIdentity(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "";

            var sb = new StringBuilder(value.Length);

            foreach (char c in value)
            {
                if (char.IsLetterOrDigit(c))
                    sb.Append(char.ToLowerInvariant(c));
            }

            return sb.ToString();
        }

        private static void AddNormalizedIfPresent(HashSet<string> set, string? value)
        {
            string normalized = NormalizeIdentity(value);
            if (!string.IsNullOrWhiteSpace(normalized))
                set.Add(normalized);
        }

        private static bool SameOrBlank(string left, string right)
        {
            if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
                return true;

            return string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
        }

        private static string BuildDescriptionHtml(string description, bool isHtml)
        {
            if (string.IsNullOrWhiteSpace(description))
                return "";

            if (isHtml)
                return description;

            return H(description)
                .Replace("\r\n", "<br/>")
                .Replace("\n", "<br/>");
        }

        private static string MakeAssetLabel(string assetKind)
        {
            return assetKind.Replace("-", " ").Trim() switch
            {
                "thumb release" => "Release Thumb",
                "box front" => "Box Front",
                "box back" => "Box Back",
                "box side" => "Box Side",
                "box top" => "Box Top",
                "box bottom" => "Box Bottom",
                "box inside" => "Box Inside",
                "manual front" => "Manual Front",
                "manual back" => "Manual Back",
                "media back" => "Media Back",
                "other advertisement" => "Advertisement",
                "other reference_card" => "Reference Card",
                "other map" => "Map",
                "other hardware" => "Hardware",
                _ => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(assetKind.Replace("-", " ").Replace("_", " "))
            };
        }

        private static void AppendChipIfPresent(StringBuilder sb, string label, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            sb.AppendLine($"<span class=\"chip\"><span class=\"label\">{H(label)}:</span> {H(value)}</span>");
        }

        private static void AppendFactCard(StringBuilder sb, string label, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            sb.AppendLine("<div class=\"fact-card\">");
            sb.AppendLine($"  <div class=\"fact-label\">{H(label)}</div>");
            sb.AppendLine($"  <div class=\"fact-value\">{H(value)}</div>");
            sb.AppendLine("</div>");
        }

        private static void AppendDetailsRow(StringBuilder sb, string label, string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            sb.AppendLine($"<div class=\"dt\">{H(label)}</div><div class=\"dd\">{H(value)}</div>");
        }

        private static void AppendReleaseSummaryChip(StringBuilder sb, string label, string value)
        {
            if (string.IsNullOrWhiteSpace(label) || string.IsNullOrWhiteSpace(value))
                return;

            sb.AppendLine($"<span class=\"release-summary-chip\"><span class=\"release-summary-label\">{H(label)}:</span> {H(value)}</span>");
        }

        private static string ExtractFileNameFromUri(string? uri)
        {
            if (string.IsNullOrWhiteSpace(uri))
                return "";

            try
            {
                var parsed = new Uri(uri, UriKind.Absolute);
                if (parsed.IsFile)
                    return Path.GetFileName(parsed.LocalPath);

                return Path.GetFileName(Uri.UnescapeDataString(parsed.AbsolutePath));
            }
            catch
            {
                try
                {
                    return Path.GetFileName(uri);
                }
                catch
                {
                    return "";
                }
            }
        }

        private static ReleaseAssetVm? FindReleaseThumbAsset(IEnumerable<ReleaseAssetVm> assets)
        {
            return assets.FirstOrDefault(x =>
                string.Equals(x.Kind, R2DatWebFlag.Media.THUMB_RELEASE, StringComparison.OrdinalIgnoreCase));
        }

        private static string FirstNonEmpty(params string?[] values)
        {
            foreach (string? value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                    return value.Trim();
            }

            return "";
        }

        private static string H(string? value) =>
            WebUtility.HtmlEncode(value ?? "");

        private static string HA(string? value) =>
            WebUtility.HtmlEncode(value ?? "");

        private sealed class FileAssetIndex
        {
            public string RootPath { get; set; } = "";
            public string ThumbUri { get; set; } = "";
            public List<string> ScreenshotUris { get; } = new List<string>();
            public List<RootAssetFile> RootAssets { get; } = new List<RootAssetFile>();
            public Dictionary<string, RootAssetFile> RootAssetsByFileName { get; } = new Dictionary<string, RootAssetFile>(StringComparer.OrdinalIgnoreCase);
        }

        private sealed class RootAssetFile
        {
            public string FileName { get; set; } = "";
            public string FullPath { get; set; } = "";
            public string Uri { get; set; } = "";
            public string Kind { get; set; } = "";
            public string Region { get; set; } = "";
            public string DateKey { get; set; } = "";
            public string Title { get; set; } = "";
            public string NormalizedTitle { get; set; } = "";
            public List<string> Tokens { get; set; } = new List<string>();
            public bool HasImmediateSecondBracketToken { get; set; }
        }
        private sealed class StandaloneMediaAssetVm
        {
            public StandaloneMediaAssetVm(string title, string fileName, string uri)
            {
                Title = title;
                FileName = fileName;
                Uri = uri;
            }

            public string Title { get; }
            public string FileName { get; }
            public string Uri { get; }
        }
        private sealed class ReleaseAssetVm
        {
            public string Kind { get; set; } = "";
            public string Label { get; set; } = "";
            public string Uri { get; set; } = "";
        }

        private sealed class KvVm
        {
            public string Name { get; init; } = "";
            public string Value { get; init; } = "";
        }

        private sealed class ReleaseVm
        {
            public string Name { get; init; } = "";
            public string Region { get; init; } = "";
            public string DateRaw { get; init; } = "";
            public string Rating { get; init; } = "";
            public string Publisher { get; init; } = "";
            public string Players { get; init; } = "";
            public string Distributor { get; init; } = "";
            public string Comment { get; init; } = "";
            public string Medium { get; init; } = "";
            public string Type { get; init; } = "";
            public string AssetKey { get; init; } = "";
            public string ProductId { get; init; } = "";
            public string DistributionOrBarcode { get; init; } = "";
            public string MameName { get; init; } = "";
            public List<KvVm> InputAttributes { get; } = new List<KvVm>();
            public List<KvVm> EmulationStatuses { get; } = new List<KvVm>();
            public DateTime? SortDate { get; init; }

            public bool IsEmpty =>
                string.IsNullOrWhiteSpace(Name) &&
                string.IsNullOrWhiteSpace(Region) &&
                string.IsNullOrWhiteSpace(DateRaw) &&
                string.IsNullOrWhiteSpace(Publisher) &&
                string.IsNullOrWhiteSpace(AssetKey);

            public static ReleaseVm From(ReleaseVO? releaseVo)
            {
                if (releaseVo is null)
                    return new ReleaseVm();

                var vm = new ReleaseVm
                {
                    Name = FirstNonEmpty(releaseVo.Name, ""),
                    Region = FirstNonEmpty(releaseVo.Region, ""),
                    DateRaw = FirstNonEmpty(releaseVo.ReleaseDate, ""),
                    Rating = FirstNonEmpty(releaseVo.Rating, ""),
                    Publisher = FirstNonEmpty(releaseVo.Publisher, ""),
                    Players = FirstNonEmpty(releaseVo.Players, ""),
                    Distributor = FirstNonEmpty(releaseVo.Distributor, ""),
                    Comment = FirstNonEmpty(releaseVo.Comment, ""),
                    Medium = FirstNonEmpty(releaseVo.Medium, ""),
                    Type = FirstNonEmpty(releaseVo.Type, ""),
                    AssetKey = FirstNonEmpty(releaseVo.AssetKey, ""),
                    ProductId = FirstNonEmpty(releaseVo.ProductId, ""),
                    DistributionOrBarcode = FirstNonEmpty(releaseVo.DistributionOrBarcode, ""),
                    MameName = FirstNonEmpty(releaseVo.MameName, ""),
                    SortDate = TryParseLooseDate(releaseVo.ReleaseDate)
                };

                foreach (InputVO input in releaseVo.InputVOs ?? Array.Empty<InputVO>())
                {
                    foreach (AttributePairVO pair in input.attributePairVOs ?? Array.Empty<AttributePairVO>())
                    {
                        string name = FirstNonEmpty(pair.name, "");
                        string value = FirstNonEmpty(pair.value, "");
                        if (!string.IsNullOrWhiteSpace(name) || !string.IsNullOrWhiteSpace(value))
                            vm.InputAttributes.Add(new KvVm { Name = name, Value = value });
                    }
                }

                foreach (AttributePairVO pair in releaseVo.EmulationStatusVOs ?? Array.Empty<AttributePairVO>())
                {
                    string name = FirstNonEmpty(pair.name, "");
                    string value = FirstNonEmpty(pair.value, "");
                    if (!string.IsNullOrWhiteSpace(name) || !string.IsNullOrWhiteSpace(value))
                        vm.EmulationStatuses.Add(new KvVm { Name = name, Value = value });
                }

                return vm;
            }
        }
        private sealed class OtherMediaGroupVm
        {
            public string Title { get; set; } = "";
            public List<ReleaseAssetVm> Items { get; set; } = new();
        }
        private sealed class MiscVm
        {
            public string Name { get; init; } = "";
            public string Value { get; init; } = "";

            public bool IsEmpty =>
                string.IsNullOrWhiteSpace(Name) &&
                string.IsNullOrWhiteSpace(Value);

            public static MiscVm From(MiscPropertyVO? miscVo)
            {
                if (miscVo is null)
                    return new MiscVm();

                return new MiscVm
                {
                    Name = FirstNonEmpty(miscVo.name, ""),
                    Value = FirstNonEmpty(miscVo.value, "")
                };
            }
        }

        private sealed class CreditVm
        {
            public string Category { get; init; } = "";
            public string Role { get; init; } = "";
            public string Name { get; init; } = "";
            public string UrlPart { get; init; } = "";

            public bool IsEmpty =>
                string.IsNullOrWhiteSpace(Name) &&
                string.IsNullOrWhiteSpace(Role) &&
                string.IsNullOrWhiteSpace(Category);

            public static CreditVm From(CreditVO? creditVo)
            {
                if (creditVo is null)
                    return new CreditVm();

                return new CreditVm
                {
                    Category = FirstNonEmpty(creditVo.Category, ""),
                    Role = FirstNonEmpty(creditVo.Role, ""),
                    Name = FirstNonEmpty(creditVo.Name, ""),
                    UrlPart = FirstNonEmpty(creditVo.UrlPart, "")
                };
            }
        }

        private sealed class CreditRoleVm
        {
            public string Role { get; init; } = "";
            public List<CreditVm> Credits { get; init; } = new List<CreditVm>();
        }

        private sealed class CreditCategoryVm
        {
            public string Category { get; init; } = "";
            public List<CreditRoleVm> Roles { get; init; } = new List<CreditRoleVm>();
        }

        private sealed class CompilationVm
        {
            public string Name { get; init; } = "";
            public string System { get; init; } = "";
            public string UrlPart { get; init; } = "";

            public bool IsEmpty =>
                string.IsNullOrWhiteSpace(Name) &&
                string.IsNullOrWhiteSpace(System) &&
                string.IsNullOrWhiteSpace(UrlPart);

            public static CompilationVm From(CompilationVO? compilationVo)
            {
                if (compilationVo is null)
                    return new CompilationVm();

                return new CompilationVm
                {
                    Name = FirstNonEmpty(compilationVo.Name, ""),
                    System = FirstNonEmpty(compilationVo.System, ""),
                    UrlPart = FirstNonEmpty(compilationVo.UrlPart, "")
                };
            }
        }

        private static IEnumerable<string> WhereNotBlank(this IEnumerable<string?> source)
        {
            foreach (string? item in source)
            {
                if (!string.IsNullOrWhiteSpace(item))
                    yield return item.Trim();
            }
        }

        private static string BuildBrowserSafeFileUri(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return "";

            try
            {
                return new Uri(Path.GetFullPath(filePath)).AbsoluteUri;
            }
            catch
            {
                return "";
            }
        }
        private static string GetLightboxScaleAttribute(int scalePercent)
        {
            int safePercent = Math.Max(1, scalePercent);
            return $" data-lightbox-scale=\"{safePercent.ToString(CultureInfo.InvariantCulture)}\"";
        }
        private static string BuildCss()
        {
            return CssTemplate
                .Replace("__THEME_VARS__", BuildThemeCssVariables())
                .Replace("__HERO_BACKDROP_COMPOSITION_GAP_PX__", HeroBackdropCompositionGapPx.ToString(CultureInfo.InvariantCulture))
                .Replace("__HERO_POSTER_WIDTH_PX__", HeroPosterWidthPx.ToString(CultureInfo.InvariantCulture))
                .Replace("__HERO_POSTER_MAX_HEIGHT_PX__", HeroPosterMaxHeightPx.ToString(CultureInfo.InvariantCulture));
        }

        private static string BuildThemeCssVariables()
        {
            string accentStrong = MixHex(PrimaryHex, "#FFFFFF", 0.18);
            string textSoft = MixHex(TextHex, PrimaryHex, 0.10);
            string chip = MixHex(PanelHex, PrimaryHex, 0.18);
            string chip2 = MixHex(Panel2Hex, PrimaryHex, 0.28);
            string pill = MixHex(Panel2Hex, PrimaryHex, 0.16);
            string tooltipTop = MixHex(Panel2Hex, PrimaryHex, 0.34);
            string tooltipBottom = MixHex(PanelHex, PrimaryHex, 0.24);

            return
                $"    --bg:{BackgroundHex};\n" +
                $"    --bg2:{BackgroundAltHex};\n" +
                $"    --page-top:{PageTopHex};\n" +
                $"    --page-bottom:{PageBottomHex};\n" +
                $"    --panel:{PanelHex};\n" +
                $"    --panel2:{Panel2Hex};\n" +
                $"    --text:{TextHex};\n" +
                $"    --text-soft:{textSoft};\n" +
                $"    --muted:{MutedHex};\n" +
                $"    --accent:{PrimaryHex};\n" +
                $"    --accent-strong:{accentStrong};\n" +
                $"    --accent-rgb:{ToRgbCss(PrimaryHex)};\n" +
                $"    --accent-glow-soft:{ToRgbaCss(PrimaryHex, 0.08)};\n" +
                $"    --accent-glow:{ToRgbaCss(PrimaryHex, 0.14)};\n" +
                $"    --accent-fill-soft:{ToRgbaCss(PrimaryHex, 0.14)};\n" +
                $"    --accent-fill:{ToRgbaCss(PrimaryHex, 0.18)};\n" +
                $"    --accent-fill-strong:{ToRgbaCss(PrimaryHex, 0.24)};\n" +
                $"    --accent-fill-hover:{ToRgbaCss(PrimaryHex, 0.32)};\n" +
                $"    --accent-line:{ToRgbaCss(PrimaryHex, 0.22)};\n" +
                $"    --accent-line-strong:{ToRgbaCss(PrimaryHex, 0.34)};\n" +
                $"    --accent-ring:{ToRgbaCss(PrimaryHex, 0.10)};\n" +
                $"    --chip:{chip};\n" +
                $"    --chip2:{chip2};\n" +
                $"    --pill:{pill};\n" +
                $"    --tooltip-top:{tooltipTop};\n" +
                $"    --tooltip-bottom:{tooltipBottom};\n" +
                $"    --hero-backdrop-composition-gap:{HeroBackdropCompositionGapPx.ToString(CultureInfo.InvariantCulture)}px;\n";
        }

        private static (int Red, int Green, int Blue) ParseHexColor(string hex)
        {
            string raw = FirstNonEmpty(hex, "").Trim();

            if (raw.StartsWith("#", StringComparison.Ordinal))
                raw = raw[1..];

            if (raw.Length == 3)
                raw = string.Concat(raw.Select(c => new string(c, 2)));

            if (raw.Length != 6)
                throw new ArgumentException("Expected a 3 or 6 digit hex colour.", nameof(hex));

            return (
                Convert.ToInt32(raw.Substring(0, 2), 16),
                Convert.ToInt32(raw.Substring(2, 2), 16),
                Convert.ToInt32(raw.Substring(4, 2), 16));
        }

        private static string MixHex(string baseHex, string mixHex, double mixAmount)
        {
            mixAmount = Math.Clamp(mixAmount, 0d, 1d);

            var (baseRed, baseGreen, baseBlue) = ParseHexColor(baseHex);
            var (mixRed, mixGreen, mixBlue) = ParseHexColor(mixHex);

            int red = (int)Math.Round(baseRed + ((mixRed - baseRed) * mixAmount));
            int green = (int)Math.Round(baseGreen + ((mixGreen - baseGreen) * mixAmount));
            int blue = (int)Math.Round(baseBlue + ((mixBlue - baseBlue) * mixAmount));

            return $"#{red:X2}{green:X2}{blue:X2}";
        }

        private static string ToRgbCss(string hex)
        {
            var (red, green, blue) = ParseHexColor(hex);
            return $"{red.ToString(CultureInfo.InvariantCulture)}, {green.ToString(CultureInfo.InvariantCulture)}, {blue.ToString(CultureInfo.InvariantCulture)}";
        }

        private static string ToRgbaCss(string hex, double alpha)
        {
            alpha = Math.Clamp(alpha, 0d, 1d);

            var (red, green, blue) = ParseHexColor(hex);

            return string.Format(
                CultureInfo.InvariantCulture,
                "rgba({0}, {1}, {2}, {3:0.##})",
                red,
                green,
                blue,
                alpha);
        }

        private static string LayoutCss => @"
.page{
    width:min(var(--maxw), 100%);
    margin:0 auto;
}
.page-grid{
    display:grid;
    gap:22px;
}
.page-grid.has-aside{
    grid-template-columns:2.2fr 1fr;
}
.page-grid.no-aside{
    grid-template-columns:1fr;
}
.stack{
    display:flex;
    flex-direction:column;
    gap:22px;
}";

        private static string GlobalCss => @"
:root{
__THEME_VARS__
    --line:rgba(255,255,255,.08);
    --ok:#89d185;
    --warn:#dfbc6f;
    --shadow:0 10px 28px rgba(0,0,0,.35);
    --radius:18px;
    --radius-sm:12px;
    --maxw:1400px;
    --font:Segoe UI, Inter, Arial, sans-serif;
}
*{box-sizing:border-box}
html,body{
    margin:0;
    padding:0;
    background-color:var(--bg);
    background:
        radial-gradient(1000px 500px at 0% -10%, var(--accent-glow-soft), transparent 50%),
        radial-gradient(1000px 500px at 100% 0%, var(--accent-glow), transparent 50%),
        linear-gradient(180deg, var(--page-top) 0%, var(--page-bottom) 100%);
    color:var(--text);
    font-family:var(--font);
}
html{
    min-height:100%;
}
body{
    min-height:100%;
    padding:24px;
    background-color:var(--bg);
}
";

        private static string HeroCss => @"
.hero{
    position:relative;
    overflow:hidden;
    border:1px solid var(--line);
    border-radius:28px;
    background-color:var(--panel);
    background:linear-gradient(180deg, rgba(255,255,255,.02), rgba(255,255,255,.01)), var(--panel);
    box-shadow:var(--shadow);
    min-height:320px;
    margin-bottom:22px;
}
.hero-backdrop-wrap{
    position:absolute;
    inset:0;
    overflow:hidden;
    background-color:var(--panel);
    pointer-events:none;
}
.hero-backdrop-wrap-composite{
    padding:__HERO_BACKDROP_COMPOSITION_GAP_PX__px;
}
.hero-backdrop{
    width:100%;
    height:100%;
    object-fit:cover;
    opacity:.58;
    filter:saturate(1.02) contrast(1.02);
    background-color:var(--panel);
}
.hero-backdrop-collage{
    position:absolute;
    inset:__HERO_BACKDROP_COMPOSITION_GAP_PX__px;
    display:grid;
    grid-template-columns:repeat(4, minmax(0, 1fr));
    grid-auto-rows:minmax(0, 1fr);
    gap:__HERO_BACKDROP_COMPOSITION_GAP_PX__px;
}
.hero-backdrop-tile{
    overflow:hidden;
    border-radius:18px;
    background-color:var(--panel2);
    box-shadow:0 12px 28px rgba(0,0,0,.22);
}
.hero-backdrop-tile:nth-child(1){
    grid-column:span 2;
    grid-row:span 2;
}
.hero-backdrop-tile:nth-child(5n){
    grid-column:span 2;
}
.hero-backdrop-tile:nth-child(7n){
    grid-row:span 2;
}
.hero-backdrop-tile-image{
    width:100%;
    height:100%;
    object-fit:cover;
    opacity:.44;
    transform:scale(1.08);
    filter:saturate(1.04) contrast(1.03);
    background-color:var(--panel);
}
.hero-backdrop-fade{
    position:absolute;
    inset:0;
    background:
        linear-gradient(90deg, rgba(11,13,18,.82) 0%, rgba(11,13,18,.68) 30%, rgba(11,13,18,.30) 58%, rgba(11,13,18,.72) 100%),
        linear-gradient(180deg, rgba(11,13,18,.08) 0%, rgba(11,13,18,.42) 100%);
}
.hero-content{
    position:relative;
    z-index:2;
    display:grid;
    gap:24px;
    padding:28px;
    align-items:start;
}
.hero-content.has-poster{
    grid-template-columns:__HERO_POSTER_WIDTH_PX__px 1fr;
}
.hero-content.no-poster{
    grid-template-columns:1fr;
}
.hero-media-column{
    display:flex;
    flex-direction:column;
    align-items:flex-start;
    gap:12px;
    width:__HERO_POSTER_WIDTH_PX__px;
}
.poster-wrap{
    width:__HERO_POSTER_WIDTH_PX__px;
    min-height:0;
    background-color:transparent;
}
.poster{
    width:100%;
    max-height:__HERO_POSTER_MAX_HEIGHT_PX__px;
    object-fit:contain;
    display:block;
    border-radius:18px;
    border:1px solid var(--accent-line);
    background-color:var(--panel2);
    background:rgba(255,255,255,.03);
    box-shadow:var(--shadow);
}
.hero-poster-actions{
    display:flex;
    align-items:flex-start;
}
.hero-title{
    margin:0 0 8px 0;
    font-size:clamp(32px, 4vw, 52px);
    line-height:1.02;
    letter-spacing:-.03em;
}
.hero-subtitle{
    margin:0 0 14px 0;
    color:var(--muted);
    font-size:15px;
}
.hero-actions{
    display:flex;
    flex-wrap:wrap;
    gap:10px;
    margin:0 0 14px 0;
}
.source-link-btn{
    display:inline-flex;
    align-items:center;
    justify-content:center;
    min-height:40px;
    padding:0 16px;
    border-radius:999px;
    border:1px solid rgba(255,255,255,.12);
    background-color:var(--accent-fill);
    background:linear-gradient(180deg, var(--accent-fill-strong), var(--accent-fill-soft));
    color:var(--text);
    text-decoration:none;
    font-size:14px;
    font-weight:600;
    box-shadow:0 8px 22px rgba(0,0,0,.24);
    transition:transform .16s ease, border-color .16s ease, background .16s ease;
    white-space:nowrap;
}
.source-link-btn:hover{
    transform:translateY(-1px);
    border-color:var(--accent-line-strong);
    background:linear-gradient(180deg, var(--accent-fill-hover), var(--accent-fill));
}
@media (max-width: 900px){
    .hero-backdrop-collage{
        grid-template-columns:repeat(3, minmax(0, 1fr));
    }
}
@media (max-width: 640px){
    .hero-backdrop-collage{
        grid-template-columns:repeat(2, minmax(0, 1fr));
    }
}
.meta-chips,
.inline-chips,
.list-chips{
    display:flex;
    flex-wrap:wrap;
    gap:8px;
}
.chip{
    display:inline-flex;
    align-items:center;
    gap:8px;
    min-height:30px;
    padding:7px 11px;
    border-radius:999px;
    background-color:var(--chip);
    background:linear-gradient(180deg, var(--chip2), var(--chip));
    border:1px solid rgba(255,255,255,.08);
    color:var(--text);
    font-size:13px;
    line-height:1;
    white-space:nowrap;
}
.chip .label{
    color:var(--muted);
}
.hero-description{
    margin:16px 0 0 0;
    max-width:950px;
    color:var(--text-soft);
    line-height:1.55;
    font-size:15px;
}
.hero-description p{
    margin:0 0 12px 0;
}
.hero-description p:last-child{
    margin-bottom:0;
}";
        private static string SectionCss => @"
.section{
    border:1px solid var(--line);
    border-radius:var(--radius);
    background-color:var(--panel);
    background:linear-gradient(180deg, rgba(255,255,255,.025), rgba(255,255,255,.012)), var(--panel);
    box-shadow:var(--shadow);
    overflow:hidden;
}
.section-header{
    display:flex;
    align-items:center;
    justify-content:space-between;
    gap:16px;
    padding:16px 18px;
    border-bottom:1px solid var(--line);
    background-color:transparent;
}
.section-toggle{
    width:100%;
    border:0;
    background:transparent;
    color:inherit;
    text-align:left;
    cursor:pointer;
}
.section-toggle:hover{
    background:rgba(255,255,255,.02);
}
.section-toggle-left{
    display:flex;
    flex-direction:column;
    gap:4px;
    min-width:0;
    flex:1;
}
.toggle-caret{
    font-size:38px;
    line-height:1;
    color:var(--muted);
    transition:transform .18s ease;
    flex:0 0 auto;
}
.section-toggle.collapsed .toggle-caret{
    transform:rotate(-90deg);
}
.section-title{
    margin:0;
    font-size:19px;
    letter-spacing:-.02em;
}
.section-subtitle{
    color:var(--muted);
    font-size:13px;
}
.section-body{
    padding:18px;
    background-color:var(--panel);
}
.section-body.collapsed{
    display:none;
}
.kv{
    display:grid;
    grid-template-columns:160px 1fr;
    gap:10px 14px;
    align-items:start;
}
.kv + .kv{
    margin-top:10px;
}
.kv-key{
    color:var(--muted);
    font-size:13px;
}
.kv-value{
    color:var(--text);
    font-size:14px;
    line-height:1.5;
}
.subsection-title{
    margin-top:8px;
    font-size:13px;
    font-weight:600;
    color:var(--text-soft);
}
.other-media-groups{
    display:flex;
    flex-direction:column;
    gap:18px;
}
.other-media-group + .other-media-group{
    padding-top:16px;
    border-top:1px solid var(--line);
}
.other-media-group-header{
    display:flex;
    flex-wrap:wrap;
    gap:8px;
    margin:0 0 10px 0;
}
.other-media-group-title{
    margin:0;
}";
        private static string OverviewCss => @"
.fact-grid{
    display:grid;
    grid-template-columns:repeat(auto-fit, minmax(240px, 1fr));
    gap:12px;
}
.fact-card{
    border:1px solid var(--line);
    border-radius:14px;
    background-color:var(--panel2);
    background:linear-gradient(180deg, rgba(255,255,255,.02), rgba(255,255,255,.01)), var(--panel2);
    padding:14px;
}
.fact-card .fact-label{
    color:var(--muted);
    font-size:12px;
    margin-bottom:6px;
}
.fact-card .fact-value{
    font-size:14px;
    line-height:1.45;
    word-break:break-word;
}";
        private static string ReleasesCss => @"
.release-list{
    display:grid;
    grid-template-columns:1fr;
    gap:16px;
}
.release-card{
    border:1px solid var(--line);
    border-radius:16px;
    background-color:var(--panel2);
    background:linear-gradient(180deg, rgba(255,255,255,.02), rgba(255,255,255,.01)), var(--panel2);
    overflow:hidden;
}
.release-top{
    display:flex;
    align-items:flex-start;
    justify-content:space-between;
    gap:16px;
    padding:16px;
    border-bottom:1px solid var(--line);
    background-color:transparent;
}
.release-top-left{
    display:grid;
    grid-template-columns:74px 1fr;
    gap:14px;
    align-items:start;
    min-width:0;
}
.release-top-left.no-thumb{
    grid-template-columns:1fr;
    gap:0;
}
.release-thumb-wrap{
    width:74px;
    height:74px;
    border:1px solid var(--line);
    border-radius:12px;
    overflow:hidden;
    background-color:var(--panel2);
    background:rgba(255,255,255,.03);
    box-shadow:var(--shadow);
}
.release-thumb{
    width:100%;
    height:100%;
    object-fit:contain;
    display:block;
    background-color:var(--panel2);
    background:rgba(255,255,255,.03);
}
.release-top-text{
    min-width:0;
}
.release-title{
    margin:0;
    font-size:20px;
    letter-spacing:-.02em;
}
.release-summary-line{
    margin-top:10px;
    display:flex;
    flex-wrap:wrap;
    gap:8px;
}
.release-summary-chip{
    display:inline-flex;
    align-items:center;
    gap:6px;
    min-height:30px;
    padding:7px 10px;
    border-radius:999px;
    background-color:var(--chip);
    background:linear-gradient(180deg, var(--chip2), var(--chip));
    border:1px solid rgba(255,255,255,.08);
    color:var(--text);
    font-size:13px;
    line-height:1;
    white-space:nowrap;
}
.release-summary-label{
    color:var(--muted);
}
.release-body{
    display:grid;
    grid-template-columns:320px 1fr;
    gap:16px;
    padding:16px;
    background-color:transparent;
}
.release-body.no-assets{
    grid-template-columns:1fr;
}
.release-asset-stack{
    display:grid;
    grid-template-columns:1fr 1fr;
    gap:10px;
    align-content:start;
}
.other-media-grid{
    display:grid;
    grid-template-columns:repeat(auto-fill, minmax(180px, 1fr));
    gap:10px;
    align-content:start;
}
.release-asset{
    border:1px solid var(--line);
    border-radius:12px;
    overflow:hidden;
    background-color:var(--panel2);
    background:rgba(255,255,255,.03);
}
.release-asset img{
    width:100%;
    height:170px;
    object-fit:contain;
    display:block;
    background-color:var(--panel2);
    background:rgba(255,255,255,.03);
}
.release-asset .asset-label{
    padding:8px 10px;
    border-top:1px solid var(--line);
    color:var(--muted);
    font-size:12px;
    word-break:break-word;
    background-color:var(--panel2);
}
.release-details{
    display:flex;
    flex-direction:column;
    gap:10px;
}
.details-table{
    display:grid;
    grid-template-columns:150px 1fr;
    gap:8px 14px;
}
.details-table .dt{
    color:var(--muted);
    font-size:13px;
}
.details-table .dd{
    color:var(--text);
    font-size:14px;
    line-height:1.5;
    word-break:break-word;
}";
        private static string ScreenshotsCss => @"
.media-grid{
    display:grid;
    grid-template-columns:repeat(auto-fill, minmax(180px, 1fr));
    gap:12px;
}
.media-card{
    border:1px solid var(--line);
    border-radius:14px;
    background-color:var(--panel2);
    background:linear-gradient(180deg, rgba(255,255,255,.02), rgba(255,255,255,.01)), var(--panel2);
    overflow:hidden;
}
.media-card img{
    width:100%;
    height:160px;
    object-fit:cover;
    display:block;
    background-color:var(--panel2);
    background:rgba(255,255,255,.03);
}
.media-caption{
    padding:10px 12px;
    font-size:13px;
    color:var(--muted);
    background-color:transparent;
}
.side-feature-shot{
    display:block;
}
.side-feature-shot-body{
    padding:12px;
    background-color:var(--panel);
}
.side-feature-shot-image{
    width:100%;
    display:block;
    border-radius:14px;
    border:1px solid var(--line);
    background-color:var(--panel2);
    background:rgba(255,255,255,.03);
    object-fit:cover;
    box-shadow:var(--shadow);
}";
        private static string CreditsCss => @"
.credit-category-block + .credit-category-block{
    margin-top:16px;
    padding-top:16px;
    border-top:1px solid var(--line);
}
.credit-category{
    font-size:17px;
    color:var(--text);
    margin:0 0 10px 0;
}
.credit-role-grid{
    column-count:3;
    column-gap:12px;
}
.credit-role-card{
    display:inline-block;
    width:100%;
    margin:0 0 12px 0;
    break-inside:avoid;
    border:1px solid var(--line);
    border-radius:16px;
    background-color:var(--panel2);
    background:linear-gradient(180deg, rgba(255,255,255,.02), rgba(255,255,255,.01)), var(--panel2);
    padding:14px;
    vertical-align:top;
}
.credit-role{
    margin:0 0 10px 0;
    font-size:15px;
    color:var(--text-soft);
}
.credit-names{
    display:flex;
    flex-wrap:wrap;
    gap:8px;
}
.credit-name{
    display:inline-flex;
    align-items:center;
    min-height:30px;
    padding:7px 10px;
    border-radius:999px;
    font-size:13px;
    background-color:var(--pill);
    background:linear-gradient(180deg, var(--chip2), var(--pill));
    border:1px solid rgba(255,255,255,.08);
    min-width:0;
    max-width:100%;
    white-space:normal;
    overflow-wrap:anywhere;
}";
        private static string ResponsiveCss => @"
@media (max-width:1200px){
    .credit-role-grid{
        column-count:2;
    }
}
@media (max-width:1100px){
    .page-grid.has-aside{
        grid-template-columns:1fr;
    }
    .side-feature-shot{
        display:none;
    }
}
@media (max-width:900px){
    .hero-content{
        grid-template-columns:1fr !important;
    }
    .hero-media-column{
        width:min(__HERO_POSTER_WIDTH_PX__px, 100%);
    }
    .poster-wrap{
        width:min(__HERO_POSTER_WIDTH_PX__px, 100%);
        max-width:__HERO_POSTER_WIDTH_PX__px;
    }
    .release-body{
        grid-template-columns:1fr;
    }
}
@media (max-width:760px){
    .credit-role-grid{
        column-count:1;
    }
}
@media (max-width:640px){
    body{
        padding:12px;
    }
    .hero-content,
    .section-body,
    .section-header,
    .release-top,
    .release-body{
        padding:14px;
    }
    .kv,
    .details-table{
        grid-template-columns:1fr;
        gap:4px;
    }
    .release-top-left{
        grid-template-columns:62px 1fr;
    }
    .release-top-left.no-thumb{
        grid-template-columns:1fr;
    }
    .release-thumb-wrap{
        width:62px;
        height:62px;
    }
}";
        private static string UtilityCss => @"
.simple-list{
    margin:0;
    padding-left:18px;
}
.simple-list li{
    line-height:1.6;
}
.muted{
    color:var(--muted);
}
.empty-note{
    color:var(--muted);
    font-style:italic;
}
.hidden{
    display:none !important;
}
.clickable{
    cursor:zoom-in;
}
img[data-asset],
#lightbox-image,
.lightbox-nav{
    -webkit-user-select:none;
    user-select:none;
    -webkit-user-drag:none;
    user-drag:none;
}
img[data-asset]::selection,
#lightbox-image::selection{
    background:transparent;
}
.lightbox{
    -webkit-user-select:none;
    user-select:none;
}
html.lightbox-open,
body.lightbox-open{
    overflow:hidden;
    overscroll-behavior:none;
}
.asset-tooltip{
    position:fixed;
    left:0;
    top:0;
    display:none;
    max-width:min(72vw, 560px);
    padding:8px 12px;
    border-radius:12px;
    background-color:var(--tooltip-bottom);
    background:linear-gradient(180deg, var(--tooltip-top), var(--tooltip-bottom));
    border:1px solid rgba(255,255,255,.12);
    color:var(--text);
    font-size:12px;
    line-height:1.35;
    word-break:break-all;
    box-shadow:0 16px 36px rgba(0,0,0,.42);
    pointer-events:none;
    z-index:100001;
    opacity:0;
    transform:translateY(4px);
    transition:opacity .12s ease, transform .12s ease;
}
.asset-tooltip.open{
    display:block;
    opacity:1;
    transform:translateY(0);
}
.lightbox{
    position:fixed;
    inset:0;
    display:none;
    align-items:center;
    justify-content:center;
    padding:28px 88px;
    background-color:rgba(0,0,0,.82);
    background:rgba(0,0,0,.82);
    z-index:99999;
    overflow:hidden;
    overscroll-behavior:contain;
}
.lightbox.open{
    display:flex;
}
.lightbox-inner{
    display:flex;
    flex-direction:column;
    align-items:center;
    gap:12px;
    max-width:min(96vw, 1600px);
}
.lightbox img{
    max-width:min(96vw, 1600px);
    max-height:84vh;
    object-fit:contain;
    display:block;
    border-radius:0;
    outline:1px solid rgba(255,255,255,.16);
    box-shadow:
        0 0 0 1px var(--accent-ring),
        0 20px 60px rgba(0,0,0,.55);
    background-color:var(--bg);
    background:var(--bg);
}
.lightbox-caption{
    max-width:min(96vw, 1600px);
    padding:8px 14px;
    border-radius:999px;
    background-color:rgba(255,255,255,.08);
    background:rgba(255,255,255,.08);
    border:1px solid rgba(255,255,255,.12);
    color:var(--text);
    font-size:13px;
    line-height:1.3;
    word-break:break-all;
    text-align:center;
}
.lightbox-nav{
    position:absolute;
    top:50%;
    transform:translateY(-50%);
    width:54px;
    height:54px;
    padding:0;
    border-radius:999px;
    border:1px solid rgba(255,255,255,.16);
    background-color:rgba(255,255,255,.10);
    background:rgba(255,255,255,.10);
    color:var(--text);
    display:flex;
    align-items:center;
    justify-content:center;
    font-size:0;
    line-height:1;
    cursor:pointer;
    box-shadow:0 12px 30px rgba(0,0,0,.34);
    backdrop-filter:blur(8px);
    -webkit-backdrop-filter:blur(8px);
    transition:
        opacity .14s ease,
        transform .14s ease,
        background .14s ease,
        border-color .14s ease;
    z-index:100000;
}
.lightbox-nav::before{
    content:'';
    display:block;
    width:14px;
    height:14px;
    box-sizing:border-box;
    border-top:3px solid rgba(255,255,255,.92);
    border-right:3px solid rgba(255,255,255,.92);
}
.lightbox-nav-prev::before{
    transform:rotate(-135deg);
    margin-left:4px;
}
.lightbox-nav-next::before{
    transform:rotate(45deg);
    margin-right:4px;
}
.lightbox-nav:hover{
    background-color:rgba(255,255,255,.16);
    background:rgba(255,255,255,.16);
    border-color:rgba(255,255,255,.26);
}
.lightbox-nav:active{
    transform:translateY(-50%) scale(.97);
}
.lightbox-nav.hidden{
    opacity:0;
    pointer-events:none;
    transform:translateY(-50%) scale(.92);
}
.lightbox-nav-prev{
    left:22px;
}
.lightbox-nav-next{
    right:22px;
}
.lightbox-close{
    position:absolute;
    top:18px;
    right:18px;
    width:42px;
    height:42px;
    border-radius:999px;
    border:1px solid rgba(255,255,255,.16);
    background-color:rgba(255,255,255,.08);
    background:rgba(255,255,255,.08);
    color:var(--text);
    font-size:22px;
    line-height:1;
    cursor:pointer;
}
@media (max-width:900px){
    .lightbox{
        padding:20px 64px;
    }
    .lightbox-nav{
        width:46px;
        height:46px;
    }
    .lightbox-nav::before{
        width:12px;
        height:12px;
        border-top-width:3px;
        border-right-width:3px;
    }
    .lightbox-nav-prev{
        left:12px;
    }
    .lightbox-nav-next{
        right:12px;
    }
}";
        private static readonly string AdditionalCss = @"

.hero-backdrop-wrap-composite {
    overflow: hidden;
}

.hero-backdrop-collage {
    position: absolute;
    inset: -48px;
    display: flex;
    flex-wrap: wrap;
    justify-content: center;
    align-content: center;
    align-items: stretch;
    gap: var(--hero-backdrop-composition-gap);
}

.hero-backdrop-tile {
    overflow: hidden;
    min-width: 0;
    min-height: 0;
    border-radius: 0;
    background: #000;
}

.hero-backdrop-collage-2 .hero-backdrop-tile {
    flex: 0 0 calc((100% - var(--hero-backdrop-composition-gap)) / 2);
    height: 100%;
}

.hero-backdrop-collage-3 .hero-backdrop-tile {
    flex: 0 0 calc((100% - var(--hero-backdrop-composition-gap) - var(--hero-backdrop-composition-gap)) / 3);
    height: 100%;
}

.hero-backdrop-collage-4 .hero-backdrop-tile {
    flex: 0 0 calc((100% - var(--hero-backdrop-composition-gap)) / 2);
    height: calc((100% - var(--hero-backdrop-composition-gap)) / 2);
}

.hero-backdrop-collage-5 .hero-backdrop-tile,
.hero-backdrop-collage-6 .hero-backdrop-tile {
    flex: 0 0 calc((100% - var(--hero-backdrop-composition-gap) - var(--hero-backdrop-composition-gap)) / 3);
    height: calc((100% - var(--hero-backdrop-composition-gap)) / 2);
}
.hero-backdrop-collage-7 .hero-backdrop-tile,
.hero-backdrop-collage-8 .hero-backdrop-tile,
.hero-backdrop-collage-9 .hero-backdrop-tile {
    flex: 0 0 calc((100% - var(--hero-backdrop-composition-gap) - var(--hero-backdrop-composition-gap)) / 3);
    height: calc((100% - var(--hero-backdrop-composition-gap) - var(--hero-backdrop-composition-gap)) / 3);
}

.hero-backdrop-collage-10 .hero-backdrop-tile,
.hero-backdrop-collage-11 .hero-backdrop-tile,
.hero-backdrop-collage-12 .hero-backdrop-tile {
    flex: 0 0 calc((100% - var(--hero-backdrop-composition-gap) - var(--hero-backdrop-composition-gap) - var(--hero-backdrop-composition-gap)) / 4);
    height: calc((100% - var(--hero-backdrop-composition-gap) - var(--hero-backdrop-composition-gap)) / 3);
}

.hero-backdrop-tile-image {
    display: block;
    width: 100%;
    height: 100%;
    object-fit: cover;
}

.standalone-media-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
    gap: 16px;
}

.standalone-media-card {
    overflow: hidden;
    border: 1px solid var(--line);
    border-radius: 18px;
    background: rgba(255, 255, 255, 0.035);
}

.video-preview-shell {
    aspect-ratio: 16 / 9;
    background: #000;
}

.video-preview {
    display: block;
    width: 100%;
    height: 100%;
    background: #000;
}

.manual-preview-shell {
    height: 360px;
    background: #fff;
}

.manual-preview {
    display: block;
    width: 100%;
    height: 100%;
    border: 0;
    background: #fff;
}

.standalone-media-body {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 12px;
    padding: 12px 14px;
}

.standalone-media-title {
    min-width: 0;
    color: var(--text);
    font-weight: 700;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}

.standalone-media-link {
    flex: 0 0 auto;
    color: var(--accent);
    text-decoration: none;
    font-weight: 700;
}
.standalone-media-link:hover {
    text-decoration: underline;
}
";
        private static string CssTemplate =>
            GlobalCss +
            LayoutCss +
            HeroCss +
            SectionCss +
            OverviewCss +
            ScreenshotsCss +
            ReleasesCss +
            CreditsCss +
            UtilityCss +
            ResponsiveCss +
            AdditionalCss;
    }
}
