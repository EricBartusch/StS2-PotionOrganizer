using BaseLib.Config;

namespace PotionOrganizer.PotionOrganizerCode;

[ConfigHoverTipsByDefault]
public class Config : SimpleModConfig
{
    [ConfigSlider(5, 100, 5)]
    public static double DragThreshold { get; set; } = 5f;
}