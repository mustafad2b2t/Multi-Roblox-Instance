using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using RobloxMultiLauncher.Models;

namespace RobloxMultiLauncher.Services
{
    /// <summary>
    /// Manages per-account AFK simulation by sending synthetic mouse messages
    /// directly to the Roblox process window (no global cursor movement).
    /// </summary>
    public class AfkService : IDisposable
    {
        // ── Win32 ──────────────────────────────────────────────────────────────
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool IsWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left, Top, Right, Bottom;
        }

        private const uint WM_MOUSEMOVE  = 0x0200;
        private const uint WM_LBUTTONDOWN = 0x0201;
        private const uint WM_LBUTTONUP   = 0x0202;

        private static IntPtr MakeLParam(int x, int y) =>
            (IntPtr)(((y & 0xFFFF) << 16) | (x & 0xFFFF));

        // ── State ──────────────────────────────────────────────────────────────
        private readonly ConcurrentDictionary<string, CancellationTokenSource> _tokens =
            new ConcurrentDictionary<string, CancellationTokenSource>();

        private readonly Random _rng = new Random();

        // ── Public API ─────────────────────────────────────────────────────────
        /// <summary>Start AFK loop for an account.</summary>
        public void Start(RobloxAccount account, AppSettings settings)
        {
            Stop(account); // stop existing loop first

            var cts = new CancellationTokenSource();
            _tokens[account.Id] = cts;

            Task.Run(() => AfkLoop(account, settings, cts.Token), cts.Token);
        }

        /// <summary>Stop AFK loop for an account.</summary>
        public void Stop(RobloxAccount account)
        {
            if (_tokens.TryRemove(account.Id, out var cts))
            {
                cts.Cancel();
                cts.Dispose();
            }
        }

        /// <summary>Stop all AFK loops.</summary>
        public void StopAll()
        {
            foreach (var kvp in _tokens)
            {
                kvp.Value.Cancel();
                kvp.Value.Dispose();
            }
            _tokens.Clear();
        }

        public void Dispose() => StopAll();

        // ── Loop ───────────────────────────────────────────────────────────────
        private async Task AfkLoop(RobloxAccount account, AppSettings settings, CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                // Randomised wait
                int waitSec = _rng.Next(settings.AfkIntervalMinSeconds, settings.AfkIntervalMaxSeconds + 1);
                try { await Task.Delay(waitSec * 1000, ct); }
                catch (TaskCanceledException) { break; }

                if (ct.IsCancellationRequested) break;

                // Find the Roblox window for this account's PID
                IntPtr hwnd = GetRobloxWindow(account.ProcessId);
                if (hwnd == IntPtr.Zero || !IsWindow(hwnd)) continue;

                // Get client area size
                RECT rect;
                if (!GetClientRect(hwnd, out rect)) continue;

                int cx = rect.Right / 2;
                int cy = rect.Bottom / 2;
                int radius = settings.AfkMovementRadiusPx;

                // Send a few random mouse moves + a harmless click
                for (int i = 0; i < 5; i++)
                {
                    int dx = _rng.Next(-radius, radius + 1);
                    int dy = _rng.Next(-radius, radius + 1);
                    int nx = Math.Max(0, Math.Min(rect.Right, cx + dx));
                    int ny = Math.Max(0, Math.Min(rect.Bottom, cy + dy));

                    PostMessage(hwnd, WM_MOUSEMOVE, IntPtr.Zero, MakeLParam(nx, ny));
                    await Task.Delay(80, ct);
                }

                // Harmless click in centre of window
                PostMessage(hwnd, WM_LBUTTONDOWN, (IntPtr)1, MakeLParam(cx, cy));
                await Task.Delay(50, ct);
                PostMessage(hwnd, WM_LBUTTONUP, IntPtr.Zero, MakeLParam(cx, cy));
            }
        }

        // ── Helpers ────────────────────────────────────────────────────────────
        private static IntPtr GetRobloxWindow(int processId)
        {
            if (processId <= 0) return IntPtr.Zero;

            try
            {
                Process proc = Process.GetProcessById(processId);
                return proc.MainWindowHandle;
            }
            catch
            {
                return IntPtr.Zero;
            }
        }
    }
}
