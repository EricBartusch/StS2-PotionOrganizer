using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Potions;

namespace PotionOrganizer.PotionOrganizerCode.Patches;

[HarmonyPatch(typeof(Node))]
public static class NodePatches
{
    // cleanup stuff in weird cases with this
    [HarmonyPostfix]
    [HarmonyPatch("_ExitTree")]
    static void ExitTreePostfix(Node __instance)
    {
        if (__instance is not NPotionHolder potion)
            return;
        
        if (NPotionHolderPatches.isDragging.Get(potion))
        {
            NPotionHolderPatches.isDragging.Set(potion, false);
            DragManager.EndDrag();
        }
        
        NPotionHolderPatches.activeTween.Get(potion)?.Kill();
        NPotionHolderPatches.activeTween.Set(potion, null);
    }

}
