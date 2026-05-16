using com.RADIO.Datinate;
using Datinate.Shared;
using System.Text.RegularExpressions;
using static datinate.app.WebSourcesUI;

namespace datinate.app
{
    public partial class WebSearchView : UserControl, IRbWebSearchView
    {
        public event Action<string>? LoadUrlEvt;
        public WebSearchView()
        {
            InitializeComponent();
            Facade.RegisterActor(this);

            webSourcesUI.SetSourceSelected(WEB_SOURCE_ENUM.Google);
            webSourcesUI.SourceSelectedEvt += OnSourceSelected;
        }

        private void OnSourceSelected(WEB_SOURCE_ENUM webSourceEnum)
        {
            Search();
        }

        public void SetSearchTerms(string? gameName, string? systemName)
        {
            Ui(() =>
            {
                if (!string.IsNullOrWhiteSpace(gameName))
                    searchGameText.Text = gameName;

                if (!string.IsNullOrWhiteSpace(systemName))
                    searchSystemText.Text = NormaliseSytemName(systemName);
            });
        }

        public void ClearView()
        {
            Ui(() =>
            {
                searchSystemText.Text = string.Empty;
                searchGameText.Text = string.Empty;
            });
        }

        private void Search()
        {
            string? url = null;

            var sysGame = Uri.EscapeDataString((searchSystemText.Text + " " + searchGameText.Text).Trim());
            var gameOnly = Uri.EscapeDataString(searchGameText.Text);

            switch (webSourcesUI.SelectedSource)
            {
                case WEB_SOURCE_ENUM.Google:
                    url = "https://www.google.com/search?q=" + sysGame;
                    break;

                case WEB_SOURCE_ENUM.Youtube:
                    url = "https://www.youtube.com/results?search_query=" + sysGame;
                    break;

                case WEB_SOURCE_ENUM.ChatGPT:
                    url = "https://chatgpt.com/?temporary-chat=true&q=" + Uri.EscapeDataString(BuildChatGptQuestion());
                    break;

                case WEB_SOURCE_ENUM.Wikipedia:
                    url = "https://en.wikipedia.org/w/index.php?search=" + gameOnly;
                    break;
            }

            if (!string.IsNullOrWhiteSpace(url))
                LoadUrlEvt?.Invoke(url);
        }
        private void searchBtn_Click(object sender, EventArgs e)
        {
            Search();
        }
        private void Ui(Action action)
        {
            if (InvokeRequired)
            {
                if (IsDisposed || !IsHandleCreated) return;
                BeginInvoke(action);
                return;
            }

            action();
        }
        private static string NormaliseSytemName(string systemName)
        {
            int idx = systemName.IndexOf(" - ", StringComparison.Ordinal);
            if (idx < 0) return systemName;

            string result = systemName.Substring(idx + 3).Replace("-", " ");
            return Regex.Replace(result, @"\s+", " ").Trim();
        }
        protected void HandleDisposing()
        {
            Facade.UnregisterActor(this);
        }
        private string BuildChatGptQuestion()
        {
            var game = searchGameText.Text;
            var system = searchSystemText.Text;

            return
                $"""
                Research the game "{game}" for "{system}".
                Respond FAST and in the exact layout below (use headings + bullet points; keep it concise).

                ## Summary
                - One-sentence overview.

                ## Names and variants
                - Known title variations (spelling/casing/alt titles).
                - Regional title differences (US/EU/JP etc.), if any.

                ## Release details
                - First release date (with confidence: high/medium/low).
                - Publisher / Developer.
                - Formats: disk/cart/tape/digital; key editions.

                ## Re-releases / compilations / demos
                - Re-issues, compilations, coverdisks/cover tapes, demos (if applicable).

                ## Evidence links (3–6)
                - Provide direct source links (official, MobyGames, Wikipedia, archive/databases, magazine scans) — no more than 6.

                ## Images (2–4)
                - Provide 2–4 image links (box art, title screen, disk/cart, manual) or suggested image search queries if direct links are uncertain.

                ## Notes / uncertainty
                - List any unclear points and what would confirm them.
                """;
        }

        public void SearchCurrent()
        {
            Search();
        }
    }
}
