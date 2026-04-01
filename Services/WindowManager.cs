using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using Newtonsoft.Json;
using RobloxMultiLauncher.Models;

namespace RobloxMultiLauncher.Services
{
    /// <summary>
    /// Window tiling, position save/restore for Roblox instances.
    /// </summary>
    public class WindowManager
    {
        // ── Win32 ──────────────────────────────────────────────────────────────
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
            int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool IsWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left, Top, Right, Bottom;
        }

        private const uint SWP_NOZORDER    = 0x0004;
        private const uint SWP_SHOWWINDOW  = 0x0040;
        private const int  SW_RESTORE      = 9;

        private static readonly IntPtr HWND_TOP = IntPtr.Zero;

        // ── Data ───────────────────────────────────────────────────────────────
        private const string PositionsFile = "positions.json";
        private List<WindowPosition> _saved = new List<WindowPosition>();

        // ── Init ───────────────────────────────────────────────────────────────
        public void Load()
        {
            if (!File.Exists(PositionsFile)) return;
            try
            {
                string json = File.ReadAllText(PositionsFile);
                _saved = JsonConvert.DeserializeObject<List<WindowPosition>>(json)
                         ?? new List<WindowPosition>();
            }
            catch { _saved = new List<WindowPosition>(); }
        }

        public void Save()
        {
            File.WriteAllText(PositionsFile, JsonConvert.SerializeObject(_saved, Formatting.Indented));
        }

        // ── Public API ─────────────────────────────────────────────────────────

        /// <summary>
        /// Tile all running windows in a grid across the primary screen.
        /// </summary>
        public void TileWindows(IList<RobloxAccount> runningAccounts)
        {
            var running = runningAccounts
                .Where(a => a.ProcessId > 0)
                .ToList();

            if (running.Count == 0) return;

            int screenW = (int)SystemParameters.WorkArea.Width;
            int screenH = (int)SystemParameters.WorkArea.Height;

            int cols = (int)Math.Ceiling(Math.Sqrt(running.Count));
            int rows = (int)Math.Ceiling((double)running.Count / cols);

            int cellW = screenW / cols;
            int cellH = screenH / rows;

            for (int i = 0; i < running.Count; i++)
            {
                int col = i % cols;
                int row = i / cols;
                int x = col * cellW;
                int y = row * cellH;

                IntPtr hwnd = GetHwnd(running[i]);
                if (hwnd == IntPtr.Zero || !IsWindow(hwnd)) continue;

                ShowWindow(hwnd, SW_RESTORE);
                SetWindowPos(hwnd, HWND_TOP, x, y, cellW, cellH,
                    SWP_NOZORDER | SWP_SHOWWINDOW);

                // Persist
                SavePosition(running[i].Id, x, y, cellW, cellH);
            }
            Save();
        }

        /// <summary>
        /// Restore previously saved position for a specific account.
        /// </summary>
        public void RestorePosition(RobloxAccount account)
        {
            var pos = _saved.FirstOrDefault(p => p.AccountId == account.Id);
            if (pos == null) return;

            IntPtr hwnd = GetHwnd(account);
            if (hwnd == IntPtr.Zero || !IsWindow(hwnd)) return;

            ShowWindow(hwnd, SW_RESTORE);
            SetWindowPos(hwnd, HWND_TOP, pos.X, pos.Y, pos.Width, pos.Height,
                SWP_NOZORDER | SWP_SHOWWINDOW);
        }

        /// <summary>
        /// Record current position of an account's window.
        /// </summary>
        public void SnapshotPosition(RobloxAccount account)
        {
            IntPtr hwnd = GetHwnd(account);
            if (hwnd == IntPtr.Zero || !IsWindow(hwnd)) return;

            if (!GetWindowRect(hwnd, out RECT r)) return;
            SavePosition(account.Id, r.Left, r.Top, r.Right - r.Left, r.Bottom - r.Top);
            Save();
        }

        // ── Helpers ────────────────────────────────────────────────────────────
        private void SavePosition(string accountId, int x, int y, int w, int h)
        {
            var existing = _saved.FirstOrDefault(p => p.AccountId == accountId);
            if (existing == null)
            {
                existing = new WindowPosition { AccountId = accountId };
                _saved.Add(existing);
            }
            existing.X = x;
            existing.Y = y;
            existing.Width = w;
            existing.Height = h;
        }

        private static IntPtr GetHwnd(RobloxAccount account)
        {
            if (account.ProcessId <= 0) return IntPtr.Zero;
            try
            {
                var proc = System.Diagnostics.Process.GetProcessById(account.ProcessId);
                return proc.MainWindowHandle;
            }
            catch { return IntPtr.Zero; }
        }
    }
}
