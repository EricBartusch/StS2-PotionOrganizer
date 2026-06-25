using BaseLib.Utils;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Potions;

namespace PotionOrganizer.PotionOrganizerCode.Patches;

[HarmonyPatch(typeof(NPotionHolder))]
public static class NPotionHolderPatches
{
    public static SpireField<NPotionHolder, bool> isDragging = new(() => false);
    public static SpireField<NPotionHolder, bool> hasMoved = new(() => false);
    public static SpireField<NPotionHolder, Vector2> dragOffset = new(() => new Vector2());
    public static SpireField<NPotionHolder, Tween?>  activeTween = new(() => null);

    // if we're dragging, this skips the bounce animation
    [HarmonyPatch("OnFocus")]
    [HarmonyPrefix]
    static bool OnFocusPrefix()
    {
        return !DragManager.IsDragging;
    }

    // skips the small grow animation
    [HarmonyPatch("OnUnfocus")]
    [HarmonyPrefix]
    static bool OnUnfocusPrefix()
    {
        return !DragManager.IsDragging;
    }
    
    [HarmonyPatch("OnPress")]
    [HarmonyPostfix]
    static void OnPressPostfix(NPotionHolder __instance)
    {
        // dragging empty slots looks weird
        if (__instance.Potion == null)
            return;
        
        activeTween.Get(__instance)?.Kill();
        activeTween.Set(__instance, null);
        
        isDragging.Set(__instance, true);
        hasMoved.Set(__instance, false);

        // offset so the potion stays relative to the place it was when grabbed
        dragOffset.Set(__instance,
            __instance.GetGlobalMousePosition() - __instance.GlobalPosition);

        DragManager.StartDrag(__instance);
        PotionReorderer.InitializeDrag(__instance);
    }

}
