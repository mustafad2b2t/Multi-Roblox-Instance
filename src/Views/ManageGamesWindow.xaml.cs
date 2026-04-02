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

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(pid) || !System.Text.RegularExpressions.Regex.IsMatch(pid, @"^\d+$"))
            {
                MessageBox.Show("Please enter a valid Game Name and a numeric Place ID.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Games.Any(g => g.PlaceId == pid))
            {
                MessageBox.Show("A game with this Place ID already exists.", "Duplicate", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Games.Add(new SavedGame { Name = name, PlaceId = pid });
            TxtName.Clear();
            TxtPlaceId.Clear();
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
