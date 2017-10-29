using UnityEngine.Networking;
using Object = System.Object;

namespace LocalAuthority.Message
{
    /// <summary>
    /// Container class for recording commands send by client objects.
    /// </summary>
    public struct CommandRecord
    {
        // Data ----------------------------------------------------------------
        /// <summary>
        /// Network Id of the calling object.
        /// </summary>
        public NetworkInstanceId NetId { get; }

        /// <summary>
        /// N'th command from the calling object.
        /// </summary>
        public uint CmdNumber { get; }


        // Methods -------------------------------------------------------------
        public CommandRecord(NetworkInstanceId netId, uint cmdNumber)
        {
            NetId = netId;
            CmdNumber = cmdNumber;
        }

        /// <summary>
        /// Deserialize using Unity networking.
        /// </summary>
        public static CommandRecord Read(NetworkReader reader)
        {
            var netId = reader.ReadNetworkId();
            var cmdNumber = reader.ReadPackedUInt32();
            return new CommandRecord(netId, cmdNumber);
        }

        /// <summary>
        /// Serialize using Unity networking.
        /// </summary>
        public void Write(NetworkWriter writer)
        {
            writer.Write(NetId);
            writer.Write(CmdNumber);
        }


        // Overrides -----------------------------------------------------------
        public override bool Equals(Object obj)
        {
            return obj is CommandRecord && this == (CommandRecord) obj;
        }

        public override int GetHashCode()
        {
            return NetId.GetHashCode() ^ CmdNumber.GetHashCode();
        }

        public static bool operator ==(CommandRecord a, CommandRecord b)
        {
            return a.NetId == b.NetId && a.CmdNumber == b.CmdNumber;
        }

        public static bool operator !=(CommandRecord a, CommandRecord b)
        {
            return !(a == b);
        }
    }
}
