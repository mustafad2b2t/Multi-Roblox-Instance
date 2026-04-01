namespace RobloxMultiLauncher.Models
{
    public class AppSettings
    {
        public int LaunchDelayMs { get; set; } = 4000;
        public int MaxInstances { get; set; } = 5;
        public int AfkIntervalMinSeconds { get; set; } = 30;
        public int AfkIntervalMaxSeconds { get; set; } = 60;
        public int AfkMovementRadiusPx { get; set; } = 50;
    }
}
