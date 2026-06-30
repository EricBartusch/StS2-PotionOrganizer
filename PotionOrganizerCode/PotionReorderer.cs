using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Potions;
using MegaCrit.Sts2.Core.Random;
using PotionOrganizer.PotionOrganizerCode.Networking;
using PotionOrganizer.PotionOrganizerCode.Patches;

namespace PotionOrganizer.PotionOrganizerCode;
public static class PotionReorderer
{
    private const float AnimDuration = 0.15f;
    
    private static readonly AccessTools.FieldRef<NPotionContainer, List<NPotionHolder>> _holdersRef =
        AccessTools.FieldRefAccess<NPotionContainer, List<NPotionHolder>>("_holders");

    private static readonly AccessTools.FieldRef<NPotionContainer, Player?> _playerRef =
        AccessTools.FieldRefAccess<NPotionContainer, Player?>("_player");

    private static readonly AccessTools.FieldRef<Player, List<PotionModel?>> _potionSlotsRef =
        AccessTools.FieldRefAccess<Player, List<PotionModel?>>("_potionSlots");
    
    public static void InitializeDrag(NPotionHolder draggedPotion)
    {
        var container = FindParentContainer(draggedPotion);
        if (container == null) return;

        var potions = _holdersRef(container);
        if (potions == null || potions.Count == 0) return;

        DragManager.PotionsXCoords = potions.Select(h => h.Position.X).OrderBy(x => x).ToArray();
        DragManager.PotionYCoord = potions[0].Position.Y;
        DragManager.Potions = potions;
        DragManager.Player = _playerRef(container);
        DragManager.Container = container;
    }

    public static void ReorderPotions(NPotionHolder draggedPotion)
    {
        var potions = DragManager.Potions;

        int oldIndex = potions.IndexOf(draggedPotion);
        int newIndex = GetClosestSlot(draggedPotion);
        if (newIndex == oldIndex) return;
        
        // visual potion update
        potions.RemoveAt(oldIndex);
        potions.Insert(newIndex, draggedPotion);
        
        // actual potion update
        var slots = _potionSlotsRef(DragManager.Player);
        var model = slots[oldIndex];
        slots.RemoveAt(oldIndex);
        slots.Insert(newIndex, model);
        
        AnimatePotionsMoving(true);
    }

    public static void FinalizeReorder()
    {
        NDebugAudioManager.Instance?.Play(Rng.Chaotic.NextItem(TmpSfx.PotionSlosh), 0.5f, PitchVariance.Large);
        AnimatePotionsMoving(false);
        PotionReorderHandler.SendReorder(DragManager.Player);
    }

    private static void AnimatePotionsMoving(bool skipPotionBeingDragged)
    {
        var potions = DragManager.Potions;
        float y = DragManager.PotionYCoord;
        for (int i = 0; i < potions.Count; i++)
        {
            var potion = potions[i];
            if (skipPotionBeingDragged && potion == DragManager.DraggedPotion) continue;

            var target = new Vector2(DragManager.PotionsXCoords[i], y);
            NPotionHolderPatches.activeTween.Get(potion)?.Kill();
            var tween = potion.CreateTween();
            tween.TweenProperty(potion, "position", target, AnimDuration)
                 .SetEase(Tween.EaseType.Out)
                 .SetTrans(Tween.TransitionType.Cubic);
            NPotionHolderPatches.activeTween.Set(potion, tween);
        }
    }

    // fancy Claude method
    private static int GetClosestSlot(NPotionHolder draggedPotion)
    {
        float[] xs = DragManager.PotionsXCoords;
        float centerX = draggedPotion.Position.X + draggedPotion.Size.X * 0.5f;

        int best = 0;
        float bestDist = float.MaxValue;
        for (int i = 0; i < xs.Length; i++)
        {
            float dist = Mathf.Abs(centerX - (xs[i] + draggedPotion.Size.X * 0.5f));
            if (dist < bestDist) { bestDist = dist; best = i; }
        }
        return best;
    }

    private static NPotionContainer? FindParentContainer(Node node)
    {
        Node? current = node.GetParent();
        while (current != null)
        {
            if (current is NPotionContainer c) return c;
            current = current.GetParent();
        }
        return null;
    }
}
