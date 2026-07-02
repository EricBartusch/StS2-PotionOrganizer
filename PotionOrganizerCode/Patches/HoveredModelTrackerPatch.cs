using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game.PeerInput;
using MegaCrit.Sts2.Core.Runs;

namespace PotionOrganizer.PotionOrganizerCode.Patches;

[HarmonyPatch(typeof(HoveredModelTracker), "OnPlayerStateChanged")]
public static class HoveredModelTrackerPatch
{
    private static readonly AccessTools.FieldRef<HoveredModelTracker, List<AbstractModel?>> _hoveredModelsRef =
        AccessTools.FieldRefAccess<HoveredModelTracker, List<AbstractModel?>>("_hoveredModels");

    private static readonly AccessTools.FieldRef<HoveredModelTracker, IPlayerCollection> _playerCollectionRef =
        AccessTools.FieldRefAccess<HoveredModelTracker, IPlayerCollection>("_playerCollection");

    private static readonly AccessTools.FieldRef<HoveredModelTracker, PeerInputSynchronizer> _inputSynchronizerRef =
        AccessTools.FieldRefAccess<HoveredModelTracker, PeerInputSynchronizer>("_inputSynchronizer");

    private static readonly AccessTools.FieldRef<HoveredModelTracker, Action<ulong>?> _hoverChangedRef =
        AccessTools.FieldRefAccess<HoveredModelTracker, Action<ulong>?>("HoverChanged");
    
    [HarmonyPostfix]
    public static void Postfix(HoveredModelTracker __instance, ulong playerId)
    {
        var playerCollection = _playerCollectionRef(__instance);
        var inputSynchronizer = _inputSynchronizerRef(__instance);

        HoveredModelData data = inputSynchronizer.GetHoveredModelData(playerId);
        if (data.type != HoveredModelType.Potion || data.hoveredModelId == null) return;

        Player player = playerCollection.GetPlayer(playerId);
        int playerSlotIndex = playerCollection.GetPlayerSlotIndex(player);

        var hoveredModels = _hoveredModelsRef(__instance);

        // if both clients agree on what is being hovered bail out
        if (hoveredModels[playerSlotIndex]?.Id == data.hoveredModelId) return;

        var correctPotion = player.PotionSlots.FirstOrDefault(p => p?.Id == data.hoveredModelId);
        hoveredModels[playerSlotIndex] = correctPotion;
        _hoverChangedRef(__instance)?.Invoke(playerId);
    }
}
