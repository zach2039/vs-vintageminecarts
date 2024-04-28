
namespace beehivekiln
{
    public class VintageMinecartsConfig 
    {
        public static VintageMinecartsConfig Loaded { get; set; } = new VintageMinecartsConfig();

        public double MaxCartSpeedMetersPerSecond { get; set; } = 8.0;
    }
}