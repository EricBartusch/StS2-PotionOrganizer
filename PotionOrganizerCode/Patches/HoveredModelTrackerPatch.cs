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
    
    [HarmonyPrefix]
    public static bool Prefix(HoveredModelTracker __instance, ulong playerId)
    {
        var hoveredModels = _hoveredModelsRef(__instance);
        var playerCollection = _playerCollectionRef(__instance);
        var inputSynchronizer = _inputSynchronizerRef(__instance);

        Player player = playerCollection.GetPlayer(playerId);
        int playerSlotIndex = playerCollection.GetPlayerSlotIndex(player);
        if (playerSlotIndex >= hoveredModels.Count)
            return false;

        HoveredModelData data = inputSynchronizer.GetHoveredModelData(playerId);
        AbstractModel? abstractModel = data.type switch
        {
            HoveredModelType.None => null,
            HoveredModelType.Card => data.hoveredCombatCard?.ToCardModelOrNull(),
            HoveredModelType.Relic => (data.hoveredRelicIndex < player.Relics.Count && data.hoveredRelicIndex >= 0)
                ? player.Relics[data.hoveredRelicIndex!.Value]
                : null,
            HoveredModelType.Potion => data.hoveredModelId != null
                ? player.PotionSlots.FirstOrDefault(p => p?.Id == data.hoveredModelId)
                : null,
            _ => throw new InvalidOperationException($"Unsupported hover type {data.type}"),
        };

        if (abstractModel == null && data.type is not HoveredModelType.None and not HoveredModelType.Potion)
            abstractModel = data.hoveredModelId != null
                ? ModelDb.GetByIdOrNull<AbstractModel>(data.hoveredModelId)
                : null;

        AbstractModel? previous = hoveredModels[playerSlotIndex];
        if (previous != abstractModel)
        {
            hoveredModels[playerSlotIndex] = abstractModel;
            _hoverChangedRef(__instance)?.Invoke(playerId);
        }

        return false;
    }
}
