using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Runs;

namespace PotionOrganizer.PotionOrganizerCode.Networking;

public static class PotionReorderHandler
{
    private static readonly AccessTools.FieldRef<Player, List<PotionModel?>> _potionSlotsRef =
        AccessTools.FieldRefAccess<Player, List<PotionModel?>>("_potionSlots");

    private static INetGameService? _netService;
    private static IPlayerCollection? _playerCollection;

    public static void Initialize(INetGameService netService, IPlayerCollection playerCollection)
    {
        Cleanup();
        _netService = netService;
        _playerCollection = playerCollection;
        _netService.RegisterMessageHandler<PotionReorderMessage>(OnReorderReceived);
    }

    public static void Cleanup()
    {
        _netService?.UnregisterMessageHandler<PotionReorderMessage>(OnReorderReceived);
        _netService = null;
        _playerCollection = null;
    }

    public static void SendReorder(Player? player)
    {
        if (player == null || _netService == null || !_netService.IsConnected) return;

        var slots = _potionSlotsRef(player);
        _netService.SendMessage(new PotionReorderMessage
        {
            SlotIds = slots.Select(p => p?.Id).ToArray()
        });
    }

    private static void OnReorderReceived(PotionReorderMessage msg, ulong senderId)
    {
        if (_playerCollection == null) return;

        Player? player = _playerCollection.GetPlayer(senderId);
        if (player == null) return;

        var slots = _potionSlotsRef(player);
        var snapshot = slots.ToList();

        slots.Clear();
        slots.AddRange(msg.SlotIds.Select(id => id != null
            ? snapshot.FirstOrDefault(p => p?.Id == id)
            : null));
    }
}
