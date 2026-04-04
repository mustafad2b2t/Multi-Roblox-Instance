using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;   // NotifyIcon
using Newtonsoft.Json;
using RobloxMultiLauncher.Dialogs;
using RobloxMultiLauncher.Models;
using RobloxMultiLauncher.Services;
using RobloxMultiLauncher.Views;
using RobloxMultiLauncher.Core;
using MessageBox = System.Windows.MessageBox;


namespace RobloxMultiLauncher
{
    public partial class MainWindow : Window
    {
        // ── Constants ──────────────────────────────────────────────────────────
        private string AccountsFile => AppPaths.Accounts;
        private string SettingsFile => AppPaths.Settings;


        // ── State ──────────────────────────────────────────────────────────────
        public ObservableCollection<RobloxAccount> Accounts { get; }
            = new ObservableCollection<RobloxAccount>();

        private AppSettings        _settings;
        private readonly AfkService      _afk     = new AfkService();
        private readonly WindowManager   _wm      = new WindowManager();
        private CancellationTokenSource  _launchCts;

        // Byfron Mutex / Locks
        private Mutex _robloxMutex1;
        private Mutex _robloxMutex2;
        private FileStream _cookieLock;

        // Tray
        private NotifyIcon  _trayIcon;
        private bool        _closeToTray = true;   // set false only on explicit Exit

        // ── Constructor ────────────────────────────────────────────────────────
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            _settings = SettingsWindow.LoadOrDefault();
            _wm.Load();

            SetupBypasses();

            LoadAccounts();
            DetectRobloxPath();
            InitTray();
            UpdateCounts();
            RefreshGameList();
        }

        private void RefreshGameList()
        {
            CbGlobalGame.ItemsSource = null;
            CbGlobalGame.ItemsSource = _settings.SavedGames;
            if (_settings.SavedGames.Count > 0)
                CbGlobalGame.SelectedIndex = 0;
        }

        private void SetupBypasses()
        {
            try { Mutex.OpenExisting("ROBLOX_singletonMutex").Close(); } catch { }
            try { Mutex.OpenExisting("ROBLOX_singletonEvent").Close(); } catch { }

            _robloxMutex1 = new Mutex(true, "ROBLOX_singletonMutex");
            _robloxMutex2 = new Mutex(true, "ROBLOX_singletonEvent");

            try
            {
                string rblxPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Roblox");
                string datPath = Path.Combine(rblxPath, "LocalStorage", "RobloxCookies.dat");
                
                if (File.Exists(datPath)) {
                    _cookieLock = new FileStream(datPath, FileMode.Open, FileAccess.Read, FileShare.None);
                }
            }
            catch { /* File might be locked or doesn't exist yet */ }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  ACCOUNTS PERSISTENCE
        // ══════════════════════════════════════════════════════════════════════
        private void LoadAccounts()
        {
            if (!File.Exists(AccountsFile)) return;
            try
            {
                var list = JsonConvert.DeserializeObject<List<RobloxAccount>>(
                               File.ReadAllText(AccountsFile))
                           ?? new List<RobloxAccount>();

                foreach (var a in list)
                {
                    a.Status    = "⬜ Stopped";
                    a.AfkStatus = "AFK OFF 🔴";
                    a.Decrypt(); // DECRYPT THE COOKIE
                    Accounts.Add(a);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load accounts:\n{ex.Message}",
                    "Load Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SaveAccounts()
        {
            try
            {
                var list = Accounts.ToList();
                foreach (var a in list) {
                    a.Encrypt(); // ENCRYPT THE COOKIE
                }
                
                File.WriteAllText(AccountsFile,
                    JsonConvert.SerializeObject(list, Formatting.Indented));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save accounts:\n{ex.Message}",
                    "Save Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  ROBLOX PATH DETECTION
        // ══════════════════════════════════════════════════════════════════════
        private void DetectRobloxPath()
        {
            string exe = RobloxLauncher.FindRobloxExe();
            TxtRobloxPath.Text = exe != null
                ? $"✅  {exe}"
                : "❌  Roblox not found — install Roblox first";
        }

        // ══════════════════════════════════════════════════════════════════════
        //  TOOLBAR BUTTON HANDLERS
        // ══════════════════════════════════════════════════════════════════════

        // ── Add Account ───────────────────────────────────────────────────────
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new AddAccountDialog { Owner = this };
            if (dlg.ShowDialog() == true && dlg.Result != null)
            {
                Accounts.Add(dlg.Result);
                SaveAccounts();
                UpdateCounts();
                SetStatus($"Account '{dlg.Result.Username}' added.");
            }
        }

        // ── Launch All ────────────────────────────────────────────────────────
        private async void BtnLaunchAll_Click(object sender, RoutedEventArgs e)
        {
            if (Accounts.Count == 0)
            {
                MessageBox.Show("No accounts added yet.", "Launch All",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            await LaunchAccountsAsync(Accounts.ToList());
        }

        // ── Launch Selected ───────────────────────────────────────────────────
        private async void BtnLaunchSel_Click(object sender, RoutedEventArgs e)
        {
            var selected = DgAccounts.SelectedItems
                                     .OfType<RobloxAccount>()
                                     .ToList();
            if (selected.Count == 0)
            {
                MessageBox.Show("Select at least one account first.", "Launch Selected",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            await LaunchAccountsAsync(selected);
        }

        // ── Stop All ──────────────────────────────────────────────────────────
        private void BtnStopAll_Click(object sender, RoutedEventArgs e)
        {
            _launchCts?.Cancel();
            _afk.StopAll();

            foreach (var a in Accounts)
                StopAccount(a);

            UpdateCounts();
            SetStatus("All instances stopped.");
        }

        // ── Auto Arrange ──────────────────────────────────────────────────────
        private void BtnArrange_Click(object sender, RoutedEventArgs e)
        {
            _wm.TileWindows(Accounts);
            SetStatus("Windows arranged in grid.");
        }

        // ── AFK All ON ────────────────────────────────────────────────────────
        private void BtnAfkAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var a in Accounts.Where(x => x.ProcessId > 0))
                EnableAfk(a);
            SetStatus("AFK enabled for all running instances.");
        }

        // ── AFK All OFF ───────────────────────────────────────────────────────
        private void BtnAfkNone_Click(object sender, RoutedEventArgs e)
        {
            foreach (var a in Accounts)
                DisableAfk(a);
            SetStatus("AFK disabled for all instances.");
        }

        // ── Remove Selected ───────────────────────────────────────────────────
        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            var selected = DgAccounts.SelectedItems
                                     .OfType<RobloxAccount>()
                                     .ToList();
            if (selected.Count == 0) return;

            var r = MessageBox.Show(
                $"Remove {selected.Count} account(s)? This cannot be undone.",
                "Remove Accounts",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (r != MessageBoxResult.Yes) return;

            foreach (var a in selected)
            {
                StopAccount(a);
                DisableAfk(a);
                Accounts.Remove(a);
            }
            SaveAccounts();
            UpdateCounts();
            SetStatus($"Removed {selected.Count} account(s).");
        }

        private void SaveSettings()
        {
            try
            {
                System.IO.File.WriteAllText(SettingsFile,
                    Newtonsoft.Json.JsonConvert.SerializeObject(_settings, Newtonsoft.Json.Formatting.Indented));
            }
            catch { /* Silently fail */ }
        }


        // ── Manage Games ──────────────────────────────────────────────────────
        private void BtnManageGames_Click(object sender, RoutedEventArgs e)
        {
            var win = new ManageGamesWindow(_settings.SavedGames) { Owner = this };
            if (win.ShowDialog() == true && win.DidChange)
            {
                _settings.SavedGames = win.Games.ToList();
                SaveSettings();
                RefreshGameList();
                SetStatus("Saved games updated.");
            }
        }

        // ── Set Target Game for Selected ──────────────────────────────────────
        private void BtnSetTargetGame_Click(object sender, RoutedEventArgs e)
        {
            if (CbGlobalGame.SelectedValue == null)
            {
                MessageBox.Show("Please select a target game from the dropdown first.", "No Game Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string newPlaceId = CbGlobalGame.SelectedValue.ToString();
            
            var selected = DgAccounts.SelectedItems.OfType<RobloxAccount>().ToList();
            if (selected.Count == 0)
            {
                MessageBox.Show("Please select one or more accounts from the list below to apply the game to.", "No Accounts Selected", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            foreach (var acc in selected)
            {
                acc.PlaceId = newPlaceId;
            }

            DgAccounts.Items.Refresh();
            SaveAccounts();
            SetStatus($"Updated {selected.Count} account(s) to play '{((SavedGame)CbGlobalGame.SelectedItem).Name}'.");
        }

        // ── Settings ──────────────────────────────────────────────────────────
        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            var sw = new SettingsWindow(_settings) { Owner = this };
            if (sw.ShowDialog() == true)
            {
                _settings = sw.Settings;
                SetStatus("Settings updated.");
            }
        }

        // ── Minimize to Tray ──────────────────────────────────────────────────
        private void BtnMinimizeTray_Click(object sender, RoutedEventArgs e)
        {
            HideToTray();
        }

        private void BtnOpenFolder_Click(object sender, RoutedEventArgs e)
        {
            AppPaths.OpenDataFolder();
        }


        // ── Per-row buttons ───────────────────────────────────────────────────
        private async void BtnLaunchSingle_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as System.Windows.Controls.Button)?.Tag is RobloxAccount acct)
                await LaunchAccountsAsync(new List<RobloxAccount> { acct });
        }

        private void BtnStopSingle_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as System.Windows.Controls.Button)?.Tag is RobloxAccount acct)
            {
                DisableAfk(acct);
                StopAccount(acct);
                UpdateCounts();
                SetStatus($"Stopped: {acct.Username}");
            }
        }

        private void BtnToggleAfk_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as System.Windows.Controls.Button)?.Tag is RobloxAccount acct)
            {
                if (acct.AfkEnabled)
                    DisableAfk(acct);
                else
                    EnableAfk(acct);
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  LAUNCH CORE
        // ══════════════════════════════════════════════════════════════════════
        private async Task LaunchAccountsAsync(List<RobloxAccount> tolaunch)
        {
            if (RobloxLauncher.FindRobloxExe() == null)
            {
                MessageBox.Show("Roblox installation not found.\nPlease install Roblox first.",
                    "Not Found", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _launchCts?.Cancel();
            _launchCts = new CancellationTokenSource();
            var ct = _launchCts.Token;

            int runningCount = Accounts.Count(a => a.ProcessId > 0);
            int slotsLeft    = _settings.MaxInstances - runningCount;
            var batch        = tolist(tolaunch: tolaunch, limit: slotsLeft);

            if (batch.Count == 0)
            {
                MessageBox.Show(
                    $"Max simultaneous instances ({_settings.MaxInstances}) already reached.",
                    "Limit Reached", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            SetStatus($"Launching {batch.Count} account(s)…");

            await RobloxLauncher.LaunchAllAsync(
                batch,
                _settings,
                (acct, proc) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        if (proc != null)
                        {
                            acct.ProcessId = proc.Id;
                            acct.Status    = "🟢 Running";

                            // Restore saved window position after a short wait
                            Task.Delay(5000).ContinueWith(_ =>
                                Dispatcher.Invoke(() => _wm.RestorePosition(acct)));
                        }
                        else if (!acct.Status.StartsWith("❌"))
                        {
                            acct.Status = "❌ Launch failed";
                        }
                        UpdateCounts();
                    });
                },
                ct);

            SetStatus($"Launch complete.  {Accounts.Count(a => a.ProcessId > 0)} running.");
        }

        // helper — avoids LINQ expression statement warning
        private static List<RobloxAccount> tolist(List<RobloxAccount> tolaunch, int limit)
            => tolaunch.Take(Math.Max(0, limit)).ToList();

        // ══════════════════════════════════════════════════════════════════════
        //  STOP HELPER
        // ══════════════════════════════════════════════════════════════════════
        private void StopAccount(RobloxAccount acct)
        {
            if (acct.ProcessId <= 0) return;
            try
            {
                var proc = Process.GetProcessById(acct.ProcessId);
                _wm.SnapshotPosition(acct);  // save position before killing
                proc.Kill();
            }
            catch { /* process already gone */ }
            finally
            {
                acct.ProcessId = 0;
                acct.Status    = "⬜ Stopped";
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  AFK HELPERS
        // ══════════════════════════════════════════════════════════════════════
        private void EnableAfk(RobloxAccount acct)
        {
            acct.AfkEnabled = true;
            _afk.Start(acct, _settings);
        }

        private void DisableAfk(RobloxAccount acct)
        {
            acct.AfkEnabled = false;
            _afk.Stop(acct);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  STATUS BAR
        // ══════════════════════════════════════════════════════════════════════
        private void SetStatus(string msg)
        {
            TxtStatus.Text = msg;
        }

        private void UpdateCounts()
        {
            int total   = Accounts.Count;
            int running = Accounts.Count(a => a.ProcessId > 0);
            TxtCounts.Text = $"{total} account{(total == 1 ? "" : "s")}  •  {running} running";
        }

        // ══════════════════════════════════════════════════════════════════════
        //  SYSTEM TRAY
        // ══════════════════════════════════════════════════════════════════════
        private void InitTray()
        {
            _trayIcon = new NotifyIcon
            {
                Text    = "Roblox Multi Launcher",
                Visible = false
            };

            // Use logo.png from assembly resources
            try
            {
                var uri = new Uri("pack://application:,,,/logo.png");
                var streamInfo = System.Windows.Application.GetResourceStream(uri);
                if (streamInfo != null)
                {
                    using (var ms = streamInfo.Stream)
                    using (var bmp = new System.Drawing.Bitmap(ms))
                    {
                        var hIcon = bmp.GetHicon();
                        _trayIcon.Icon = System.Drawing.Icon.FromHandle(hIcon);
                    }
                }
                else
                {
                    _trayIcon.Icon = System.Drawing.SystemIcons.Application;
                }
            }
            catch
            {
                _trayIcon.Icon = System.Drawing.SystemIcons.Application;
            }

            var ctx = new ContextMenuStrip();
            ctx.Items.Add("Show / Hide", null, (s, e) => ToggleVisibility());
            ctx.Items.Add(new ToolStripSeparator());
            ctx.Items.Add("Exit", null, (s, e) => ExitApp());

            _trayIcon.ContextMenuStrip = ctx;
            _trayIcon.DoubleClick     += (s, e) => ToggleVisibility();
        }

        private void HideToTray()
        {
            _trayIcon.Visible = true;
            Hide();
        }

        private void ToggleVisibility()
        {
            if (IsVisible)
                HideToTray();
            else
            {
                Show();
                WindowState      = WindowState.Normal;
                _trayIcon.Visible = false;
                Activate();
            }
        }

        private void ExitApp()
        {
            _closeToTray = false;
            _afk.StopAll();
            _trayIcon.Dispose();
            System.Windows.Application.Current.Shutdown();
        }

        // ══════════════════════════════════════════════════════════════════════
        //  WINDOW EVENTS
        // ══════════════════════════════════════════════════════════════════════
        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized && _closeToTray)
                HideToTray();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_closeToTray)
            {
                e.Cancel = true;
                HideToTray();
            }
            else
            {
                _afk.StopAll();
                _trayIcon?.Dispose();
                SaveAccounts();
            }
        }
    }
}
