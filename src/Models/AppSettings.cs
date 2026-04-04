using System.Collections.Generic;

namespace RobloxMultiLauncher.Models
{
    public class SavedGame
    {
        public string Name { get; set; }
        public string PlaceId { get; set; }
        public string PrivateServerLink { get; set; }

        public override string ToString() => string.IsNullOrWhiteSpace(PrivateServerLink) ? $"{Name} ({PlaceId})" : $"🔒 {Name} (Private)";
    }

    public class AppSettings
    {
        public int LaunchDelayMs { get; set; } = 4000;
        public int MaxInstances { get; set; } = 5;
        public int AfkIntervalMinSeconds { get; set; } = 30;
        public int AfkIntervalMaxSeconds { get; set; } = 60;
        public int AfkMovementRadiusPx { get; set; } = 50;
        
        public List<SavedGame> SavedGames { get; set; } = new List<SavedGame>();
    }
}
