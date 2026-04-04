using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using RobloxMultiLauncher.Models;

namespace RobloxMultiLauncher.Views
{
    public partial class ManageGamesWindow : Window
    {
        public ObservableCollection<SavedGame> Games { get; set; }
        public bool DidChange { get; private set; } = false;

        public ManageGamesWindow(List<SavedGame> existingGames)
        {
            InitializeComponent();
            Games = new ObservableCollection<SavedGame>(existingGames ?? new List<SavedGame>());
            ListGames.ItemsSource = Games;
        }

        private void BtnAddGame_Click(object sender, RoutedEventArgs e)
        {
            string name = TxtName.Text.Trim();
            string pid = TxtPlaceId.Text.Trim();
            string plink = TxtPrivateLink.Text.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Please enter a Game Name.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(pid) && !string.IsNullOrWhiteSpace(plink))
            {
                // Try to extract Place ID from various URL formats
                var match = System.Text.RegularExpressions.Regex.Match(plink, @"roblox\.com/games/(\d+)");
                if (match.Success) 
                {
                    pid = match.Groups[1].Value;
                    TxtPlaceId.Text = pid; // Auto-fill if found
                }
            }

            if (string.IsNullOrWhiteSpace(pid) || !System.Text.RegularExpressions.Regex.IsMatch(pid, @"^\d+$"))
            {
                string errorMsg = "Please enter a numeric Place ID.";
                if (!string.IsNullOrWhiteSpace(plink))
                {
                    errorMsg = "We couldn't find a Place ID in your Private Server Link. Please enter it manually in the 'Place ID' field.";
                }
                MessageBox.Show(errorMsg, "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Games.Add(new SavedGame { Name = name, PlaceId = pid, PrivateServerLink = plink });
            TxtName.Clear();
            TxtPlaceId.Clear();
            TxtPrivateLink.Clear();
            DidChange = true;
        }

        private void BtnRemoveGame_Click(object sender, RoutedEventArgs e)
        {
            if (ListGames.SelectedItem is SavedGame sel)
            {
                Games.Remove(sel);
                DidChange = true;
            }
        }

        private void BtnSaveClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
