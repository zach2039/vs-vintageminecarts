using ProtoBuf;

namespace VintageMinecarts.ModNetwork
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class SyncConfigClientPacket
    {
        public double MaxCartSpeedMetersPerSecond;
    }
}