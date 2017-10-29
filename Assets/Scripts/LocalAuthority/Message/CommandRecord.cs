using System;
using UnityEngine.Networking;
using Object = System.Object;

namespace LocalAuthority.Message
{
    /// <summary>
    /// Container class for recording commands send by client objects.
    /// </summary>
    [Serializable]
    public struct CommandRecord
    {
        // Data ----------------------------------------------------------------
        /// <summary>
        /// Network Id of the calling player.
        /// </summary>
        public NetworkInstanceId NetId { get { return netId; } private set { netId = value; } }

        /// <summary>
        /// N'th command from the calling object.
        /// </summary>
        public uint CmdNumber { get { return cmdNumber; } private set { cmdNumber = value; } }

        private NetworkInstanceId netId;
        private uint cmdNumber;


        // Methods -------------------------------------------------------------
        public CommandRecord(NetworkInstanceId netId, uint cmdNumber)
        {
            this.netId = netId;
            this.cmdNumber = cmdNumber;
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
            writer.WritePackedUInt32(CmdNumber);
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
