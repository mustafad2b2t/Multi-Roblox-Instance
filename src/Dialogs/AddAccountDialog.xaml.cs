using System.Windows;
using RobloxMultiLauncher.Models;

namespace RobloxMultiLauncher.Dialogs
{
    public partial class AddAccountDialog : Window
    {
        public RobloxAccount Result { get; private set; }

        public AddAccountDialog()
        {
            InitializeComponent();
        }

        private void HelpCookie_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "HOW TO GET YOUR ROBLOX COOKIE\n" +
                "───────────────────────────────\n\n" +
                "1) Open Chrome or Edge and log into your account at www.roblox.com\n\n" +
                "2) Press F12 on your keyboard (or right-click and select 'Inspect').\n\n" +
                "3) In the menu that opens, find the 'Application' tab at the top.\n\n" +
                "4) On the left sidebar, expand 'Cookies' and click on 'https://www.roblox.com'.\n\n" +
                "5) In the table on the right, look for the row named:  .ROBLOSECURITY\n\n" +
                "6) Double-click the very long text next to it (Value column).\n    (It usually starts with _|WARNING:-DO-NOT-SHARE-THIS...)\n\n" +
                "7) Copy that entire text and close the developer tools.\n\n" +
                "8) Paste it into the 'Roblox Cookie' box here!",
                "How to get Cookie?", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            string username = TxtUsername.Text.Trim();
            string cookie   = PbCookie.Password.Trim();
            string placeId  = TxtPlaceId.Text.Trim();
            string privateLink = TxtPrivateLink.Text.Trim();

            // ── Validation ─────────────────────────────────────────────────────
            if (string.IsNullOrWhiteSpace(username))
            {
                ShowError("Please enter a display username.");
                TxtUsername.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(cookie) || !cookie.StartsWith("_|WARNING"))
            {
                ShowError("The cookie must start with  _|WARNING\nPlease paste your full .ROBLOSECURITY cookie.");
                PbCookie.Focus();
                return;
            }

            // If private link is provided, try to extract placeId if not already set
            if (string.IsNullOrWhiteSpace(placeId) && !string.IsNullOrWhiteSpace(privateLink))
            {
                var match = System.Text.RegularExpressions.Regex.Match(privateLink, @"roblox\.com/games/(\d+)");
                if (match.Success)
                    placeId = match.Groups[1].Value;
            }

            if (string.IsNullOrWhiteSpace(placeId) || !System.Text.RegularExpressions.Regex.IsMatch(placeId, @"^\d+$"))
            {
                ShowError("Place ID must be a numeric value.\nExample: 480700040\n(Required unless a valid Private Server Link is provided)");
                TxtPlaceId.Focus();
                return;
            }

            Result = new RobloxAccount
            {
                Username = username,
                Cookie   = cookie,
                PlaceId  = placeId,
                PrivateServerLink = privateLink,
                Status   = "⬜ Stopped"
            };

            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ShowError(string msg)
        {
            MessageBox.Show(msg, "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
