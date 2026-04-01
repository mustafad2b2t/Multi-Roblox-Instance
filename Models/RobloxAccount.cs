using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace RobloxMultiLauncher.Models
{
    public class RobloxAccount : System.ComponentModel.INotifyPropertyChanged
    {
        private string _username;
        private string _cookie;
        private string _placeId;
        private string _status;
        private bool _afkEnabled;
        private string _afkStatus;
        private int _processId;

        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(nameof(Username)); }
        }

        [JsonIgnore]
        public string Cookie
        {
            get => _cookie;
            set { _cookie = value; OnPropertyChanged(nameof(Cookie)); }
        }

        public string EncryptedCookie { get; set; }

        public string PlaceId
        {
            get => _placeId;
            set { _placeId = value; OnPropertyChanged(nameof(PlaceId)); }
        }

        [JsonIgnore]
        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(nameof(Status)); }
        }

        [JsonIgnore]
        public bool AfkEnabled
        {
            get => _afkEnabled;
            set
            {
                _afkEnabled = value;
                AfkStatus = value ? "AFK ON 🟢" : "AFK OFF 🔴";
                OnPropertyChanged(nameof(AfkEnabled));
            }
        }

        [JsonIgnore]
        public string AfkStatus
        {
            get => _afkStatus;
            set { _afkStatus = value; OnPropertyChanged(nameof(AfkStatus)); }
        }

        [JsonIgnore]
        public int ProcessId
        {
            get => _processId;
            set { _processId = value; OnPropertyChanged(nameof(ProcessId)); }
        }

        public RobloxAccount()
        {
            Status = "⬜ Stopped";
            AfkEnabled = false;
            AfkStatus = "AFK OFF 🔴";
            ProcessId = 0;
        }

        public void Encrypt()
        {
            if (string.IsNullOrEmpty(Cookie)) return;
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(Cookie);
                byte[] enc = ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
                EncryptedCookie = Convert.ToBase64String(enc);
            }
            catch { }
        }

        public void Decrypt()
        {
            if (string.IsNullOrEmpty(EncryptedCookie)) return;
            try
            {
                byte[] enc = Convert.FromBase64String(EncryptedCookie);
                byte[] dec = ProtectedData.Unprotect(enc, null, DataProtectionScope.CurrentUser);
                Cookie = Encoding.UTF8.GetString(dec);
            }
            catch { Cookie = "Decryption Failed - Re-add account"; }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
        }
    }
}
