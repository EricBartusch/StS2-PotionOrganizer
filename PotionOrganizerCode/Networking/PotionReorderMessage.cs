using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;

namespace PotionOrganizer.PotionOrganizerCode.Networking;

public class PotionReorderMessage : INetMessage
{
    public ModelId?[] SlotIds = [];

    public bool ShouldBroadcast => true;
    public NetTransferMode Mode => NetTransferMode.Reliable;
    public LogLevel LogLevel => LogLevel.Debug;
    public bool ShouldBuffer => false;

    public void Serialize(PacketWriter writer)
    {
        writer.WriteInt(SlotIds.Length);
        foreach (var id in SlotIds)
        {
            writer.WriteBool(id != null);
            if (id != null)
                writer.WriteModelEntry(id);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        int count = reader.ReadInt();
        SlotIds = new ModelId?[count];
        for (int i = 0; i < count; i++)
            SlotIds[i] = reader.ReadBool() ? reader.ReadModelIdAssumingType<PotionModel>() : null;
    }
}
