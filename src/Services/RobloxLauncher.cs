using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using RobloxMultiLauncher.Models;

namespace RobloxMultiLauncher.Services
{
    public class RobloxLauncher
    {
        public static string FindRobloxExe()
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string versionFolder = Path.Combine(localAppData, "Roblox", "Versions");

            if (!Directory.Exists(versionFolder))
                return null;

            foreach (string dir in Directory.GetDirectories(versionFolder))
            {
                string exe = Path.Combine(dir, "RobloxPlayerBeta.exe");
                if (File.Exists(exe))
                    return exe;
            }
            return null;
        }

        public static async Task<string> GetAuthTicketAsync(string cookie)
        {
            using (var handler = new HttpClientHandler { UseCookies = false })
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("Cookie", $".ROBLOSECURITY={cookie}");
                client.DefaultRequestHeaders.Add("Referer", "https://www.roblox.com/");
                
                // 1. Trigger a harmless POST request to extract the anti-CSRF token
                var csrfReq = new HttpRequestMessage(HttpMethod.Post, "https://auth.roblox.com/v2/logout");
                var csrfRes = await client.SendAsync(csrfReq);
                
                string csrf = null;
                if (csrfRes.Headers.Contains("x-csrf-token"))
                    csrf = csrfRes.Headers.GetValues("x-csrf-token").First();
                
                if (string.IsNullOrEmpty(csrf)) 
                    throw new Exception("Failed to fetch CSRF token from Roblox API.");
                
                // 2. Request the actual one-time launch ticket
                var ticketReq = new HttpRequestMessage(HttpMethod.Post, "https://auth.roblox.com/v1/authentication-ticket");
                ticketReq.Headers.Add("x-csrf-token", csrf);
                ticketReq.Content = new StringContent(""); // Empty body required
                
                var ticketRes = await client.SendAsync(ticketReq);
                if (!ticketRes.IsSuccessStatusCode) 
                    throw new Exception("Auth Ticket rejected. Cookie might be expired or invalid.");
                
                if (ticketRes.Headers.Contains("rbx-authentication-ticket"))
                    return ticketRes.Headers.GetValues("rbx-authentication-ticket").First();
                    
                throw new Exception("Auth ticket missing in API response headers.");
            }
        }

        public static async Task<Process> LaunchAccountAsync(RobloxAccount account, AppSettings settings)
        {
            string exePath = FindRobloxExe();
            if (exePath == null)
                throw new FileNotFoundException("Roblox installation not found. Please install Roblox first.");

            // 1) Securely fetch one-time launch ticket
            string ticket = await GetAuthTicketAsync(account.Cookie);

            // 2) Launch Roblox Player directly using strict command-line arguments to bypass browser hooks
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = $"--app --play -t {ticket} -j \"https://assetgame.roblox.com/game/PlaceLauncher.ashx?request=RequestGame&browserTrackerId=0&placeId={account.PlaceId}&isPlayTogetherGame=false\" -b 0 --launchtime={DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()} --rloc en_us --gloc en_us",
                UseShellExecute = false,
                CreateNoWindow = false
            };

            Process proc = Process.Start(psi);
            return proc;
        }

        public static async Task LaunchAllAsync(
            System.Collections.Generic.IList<RobloxAccount> accounts,
            AppSettings settings,
            Action<RobloxAccount, Process> statusCallback,
            CancellationToken ct)
        {
            int launched = 0;
            foreach (RobloxAccount acct in accounts)
            {
                if (ct.IsCancellationRequested) break;

                Process proc = null;
                try
                {
                    proc = await LaunchAccountAsync(acct, settings);
                }
                catch (Exception ex)
                {
                    acct.Status = $"❌ Error: {ex.Message}";
                }

                statusCallback?.Invoke(acct, proc);
                launched++;

                if (proc != null && launched < accounts.Count)
                    await Task.Delay(settings.LaunchDelayMs, ct);
            }
        }
    }
}
