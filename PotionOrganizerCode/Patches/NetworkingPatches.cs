using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Multiplayer.Game.PeerInput;
using MegaCrit.Sts2.Core.Runs;
using PotionOrganizer.PotionOrganizerCode.Networking;

namespace PotionOrganizer.PotionOrganizerCode.Patches;

[HarmonyPatch]
public static class NetworkingPatches
{
    [HarmonyPatch(typeof(HoveredModelTracker), MethodType.Constructor, typeof(PeerInputSynchronizer), typeof(IPlayerCollection))]
    [HarmonyPostfix]
    static void OnHoveredModelTrackerConstructed(PeerInputSynchronizer inputSynchronizer, IPlayerCollection playerCollection)
    {
        PotionReorderHandler.Initialize(inputSynchronizer.NetService, playerCollection);
    }

    [HarmonyPatch(typeof(PeerInputSynchronizer), nameof(PeerInputSynchronizer.Dispose))]
    [HarmonyPostfix]
    static void OnPeerInputSynchronizerDisposed()
    {
        PotionReorderHandler.Cleanup();
    }
}
