using System;
using System.IO;
using System.Windows;
using Newtonsoft.Json;
using RobloxMultiLauncher.Models;

namespace RobloxMultiLauncher.Views
{
    public partial class SettingsWindow : Window
    {
        private const string SettingsFile = "settings.json";

        public AppSettings Settings { get; private set; }

        public SettingsWindow(AppSettings current)
        {
            InitializeComponent();
            Settings = current ?? new AppSettings();
            LoadSliders();
        }

        // ── Load from model into UI ────────────────────────────────────────────
        private void LoadSliders()
        {
            SliderDelay.Value   = Settings.LaunchDelayMs;
            SliderAfkMin.Value  = Settings.AfkIntervalMinSeconds;
            SliderAfkMax.Value  = Settings.AfkIntervalMaxSeconds;
            SliderRadius.Value  = Settings.AfkMovementRadiusPx;

            UpdateLabels();
        }

        private void UpdateLabels()
        {
            if (TxtDelay == null || TxtAfkMin == null || TxtAfkMax == null || TxtRadius == null) return;

            TxtDelay.Text   = $"{(int)SliderDelay.Value} ms";
            TxtAfkMin.Text  = $"{(int)SliderAfkMin.Value} s";
            TxtAfkMax.Text  = $"{(int)SliderAfkMax.Value} s";
            TxtRadius.Text  = $"{(int)SliderRadius.Value} px";
        }

        // ── Slider events ──────────────────────────────────────────────────────
        private void SliderDelay_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
            => UpdateLabels();

        private void SliderAfkMin_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
            => UpdateLabels();

        private void SliderAfkMax_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
            => UpdateLabels();

        private void SliderRadius_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
            => UpdateLabels();

        // ── Save ───────────────────────────────────────────────────────────────
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            int afkMin = (int)SliderAfkMin.Value;
            int afkMax = (int)SliderAfkMax.Value;

            if (afkMin >= afkMax)
            {
                MessageBox.Show("AFK Interval Min must be less than Max.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Settings.LaunchDelayMs          = (int)SliderDelay.Value;
            Settings.AfkIntervalMinSeconds  = afkMin;
            Settings.AfkIntervalMaxSeconds  = afkMax;
            Settings.AfkMovementRadiusPx    = (int)SliderRadius.Value;

            try
            {
                File.WriteAllText(SettingsFile,
                    JsonConvert.SerializeObject(Settings, Formatting.Indented));
                MessageBox.Show("Settings saved successfully! ✅",
                    "Settings", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save settings:\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ── Static Helper ──────────────────────────────────────────────────────
        public static AppSettings LoadOrDefault()
        {
            if (!File.Exists(SettingsFile)) return new AppSettings();
            try
            {
                string json = File.ReadAllText(SettingsFile);
                return JsonConvert.DeserializeObject<AppSettings>(json) ?? new AppSettings();
            }
            catch { return new AppSettings(); }
        }
    }
}
