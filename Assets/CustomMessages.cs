using UnityEngine.Networking;

namespace NonPlayerClientAuthority
{
    public class NetIdMessage : MessageBase
    {
        public NetworkInstanceId netId;

        public NetIdMessage()
        {
        }

        public NetIdMessage(NetworkInstanceId id)
        {
            netId = id;
        }

        public override void Deserialize(NetworkReader reader)
        {
            netId = reader.ReadNetworkId();
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(netId);
        }
    }

    public class FloatNetIdMessage : NetIdMessage
    {
        public float value;

        public FloatNetIdMessage()
        {
        }

        public FloatNetIdMessage(NetworkInstanceId id) : base(id)
        {
        }

        public FloatNetIdMessage(float value)
        {
            this.value = value;
        }

        public FloatNetIdMessage(NetworkInstanceId id, float value) : base(id)
        {
            this.value = value;
        }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            value = reader.ReadSingle();
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.Write(value);
        }
    }

    public class IntNetIdMessage : NetIdMessage
    {
        public float value;

        public IntNetIdMessage()
        {
        }

        public IntNetIdMessage(NetworkInstanceId id) : base(id)
        {
        }

        public IntNetIdMessage(int value)
        {
            this.value = value;
        }

        public IntNetIdMessage(NetworkInstanceId id, int value) : base(id)
        {
            this.value = value;
        }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            value = (int)reader.ReadPackedUInt32();
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.WritePackedUInt32((uint)value);
        }
    }
}