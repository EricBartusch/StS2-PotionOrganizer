using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Potions;

namespace PotionOrganizer.PotionOrganizerCode;

public static class DragManager
{
    public static NPotionHolder? DraggedPotion { get; private set; }
    public static bool IsDragging => DraggedPotion != null;
    public static float[] PotionsXCoords { get; set; } = Array.Empty<float>();
    public static float  PotionYCoord  { get; set; }
    public static List<NPotionHolder>? Potions { get; set; }
    public static Player? Player { get; set; }
    public static NPotionContainer? Container { get; set; }
    
    public static void StartDrag(NPotionHolder potion)
    {
        DraggedPotion = potion;
        PotionsXCoords = Array.Empty<float>();
        Potions  = null;
        Player = null;
        Container = null;
    }

    public static void EndDrag()
    {
        DraggedPotion = null;
        Potions = null;
        Player = null;
        Container = null;
    }
}
