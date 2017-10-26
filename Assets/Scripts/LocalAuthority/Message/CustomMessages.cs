using UnityEngine;
using UnityEngine.Networking;

namespace LocalAuthority.Message
{
    /// <summary>
    /// Message base class for using Message-invoked RPCs. Contains the NetworkInstanceId of the object that is sending the message.
    /// </summary>
    public class NetIdMessage : MessageBase
    {
        /// <summary>
        /// The NetworkInstanceId of the object that send this message.
        /// </summary>
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
        public int value;

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

    public class Vector2NetIdMessage : NetIdMessage
    {
        public Vector2 value;

        public Vector2NetIdMessage()
        {
        }

        public Vector2NetIdMessage(NetworkInstanceId id) : base(id)
        {
        }

        public Vector2NetIdMessage(Vector2 value)
        {
            this.value = value;
        }

        public Vector2NetIdMessage(NetworkInstanceId id, Vector2 value) : base(id)
        {
            this.value = value;
        }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            value = reader.ReadVector2();
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.Write(value);
        }
    }

    public class Vector3NetIdMessage : NetIdMessage
    {
        public Vector3 value;

        public Vector3NetIdMessage()
        {
        }

        public Vector3NetIdMessage(NetworkInstanceId id) : base(id)
        {
        }

        public Vector3NetIdMessage(Vector3 value)
        {
            this.value = value;
        }

        public Vector3NetIdMessage(NetworkInstanceId id, Vector3 value) : base(id)
        {
            this.value = value;
        }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            value = reader.ReadVector3();
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.Write(value);
        }
    }

    public class TwoNetIdMessage : NetIdMessage
    {
        public NetworkInstanceId netId2;

        public TwoNetIdMessage()
        {
        }

        public TwoNetIdMessage(NetworkInstanceId id) : base(id)
        {
        }

        public TwoNetIdMessage(NetworkInstanceId id, NetworkInstanceId id2) : base(id)
        {
            netId2 = id2;
        }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            netId2 = reader.ReadNetworkId();
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.Write(netId2);
        }
    }
}