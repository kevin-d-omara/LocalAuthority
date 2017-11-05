using UnityEngine;
using UnityEngine.Networking;

namespace LocalAuthority.Message
{
    // Note: All messages have explicit Deserialize and Serialize methods, despite that Unity.Networking will
    //       code-generate these for you. I found that even simple classes like IntNetIdMessage were not being
    //       code-generated correctly, and the value would always be zero.


    public static class MessageFactory
    {
        /// <summary>
        /// Return a new instance of TMsg with all of its fields initialized.
        /// </summary>
        /// <typeparam name="TMsg">Type of message to create.</typeparam>
        /// <param name="args">An argument list for TMsg's full constructor, with netId omitted. This is an array of
        /// objects with the same number, order, and type as the parameters of TMsg's full constructor, but with netId
        /// omitted.</param>
        /// <returns>New TMsg instance with all fields initialized.</returns>
        public static TMsg New<TMsg>(NetworkInstanceId netId, params object[] args) where TMsg : NetIdMessage, new()
        {
            var msg = new TMsg();
            msg.netId = netId;
            msg.VarargsSetter(args);
            return msg;
        }
    }

    /// <summary>
    /// Message base class for using Message-invoked Commands.
    /// </summary>
    public class NetIdMessage : MessageBase
    {
        /// <summary>
        /// The NetworkInstanceId of the object that sent this message. Used to find the object within the scene.
        /// </summary>
        public NetworkInstanceId netId;

        public NetIdMessage()
        {
        }

        public NetIdMessage(NetworkInstanceId id)
        {
            netId = id;
        }

        /// <summary>
        /// Set all class fields, except netId.
        /// </summary>
        /// <param name="args">An argument list for TMsg's full constructor, with netId omitted. This is an array of
        /// objects with the same number, order, and type as the parameters of TMsg's full constructor, but with netId
        /// omitted.</param>
        public virtual void VarargsSetter(params object[] args) { }

        /// <summary>
        /// Return all class fields, except netId. This is an array of objects with the same number, order, and type as
        /// the parameters of the full constructor, but with netId omitted.
        /// </summary>
        public virtual object[] VarargsGetter()
        {
            return null;    // (?) null vs new object[0]
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

        public FloatNetIdMessage(NetworkInstanceId id, float value) : base(id)
        {
            this.value = value;
        }

        public override object[] VarargsGetter()
        {
            return new object[] { value };
        }

        public override void VarargsSetter(params object[] args)
        {
            value = (float) args[0];
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

        public IntNetIdMessage(NetworkInstanceId id, int value) : base(id)
        {
            this.value = value;
        }

        public override object[] VarargsGetter()
        {
            return new object[] { value };
        }

        public override void VarargsSetter(params object[] args)
        {
            value = (int) args[0];
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

        public Vector2NetIdMessage(NetworkInstanceId id, Vector2 value) : base(id)
        {
            this.value = value;
        }

        public override void VarargsSetter(params object[] args)
        {
            value = (Vector2) args[0];
        }

        public override object[] VarargsGetter()
        {
            return new object[] { value };
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

        public Vector3NetIdMessage(NetworkInstanceId id, Vector3 value) : base(id)
        {
            this.value = value;
        }

        public override void VarargsSetter(params object[] args)
        {
            value = (Vector3)args[0];
        }

        public override object[] VarargsGetter()
        {
            return new object[] { value };
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

        public TwoNetIdMessage(NetworkInstanceId id, NetworkInstanceId id2) : base(id)
        {
            netId2 = id2;
        }

        public override void VarargsSetter(params object[] args)
        {
            netId2 = (NetworkInstanceId) args[0];
        }

        public override object[] VarargsGetter()
        {
            return new object[] { netId2 };
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