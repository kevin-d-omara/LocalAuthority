using UnityEngine;
using UnityEngine.Networking;

namespace LocalAuthority.Message
{
    /// <summary>
    /// Message base class for using Message-invoked Commands.
    /// </summary>
    public class NetIdMessage : MessageBase
    {
        /// <summary>
        /// The NetworkInstanceId of the object that sent this message. Used to local the object within the scene.
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
        /// <param name="args">Arguments to the full constructor, in that order, with netId omitted.</param>
        public virtual void VarargsSetter(params object[] args) { }

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





    // Client-side Prediction ==========================================================================================

    /// <summary>
    /// Message base class for using Message-invoked Commands with Client-side prediction enabled.
    /// </summary>
    public class CommandRecordMessage : NetIdMessage
    {
        public CommandRecord cmdRecord;

        public CommandRecordMessage()
        {
        }

        public CommandRecordMessage(NetworkInstanceId id, CommandRecord cmdRecord) : base(id)
        {
            this.cmdRecord = cmdRecord;
        }

        public override void VarargsSetter(params object[] args)
        {
            cmdRecord = (CommandRecord) args[0];
        }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            cmdRecord = CommandRecord.Read(reader);
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            cmdRecord.Write(writer);
        }
    }

    public class FloatCommandRecordMessage : CommandRecordMessage
    {
        public float value;

        public FloatCommandRecordMessage()
        {
        }

        public FloatCommandRecordMessage(NetworkInstanceId id, CommandRecord cmdRecord, float value) : base(id, cmdRecord)
        {
            this.value = value;
        }

        public override void VarargsSetter(params object[] args)
        {
            base.VarargsSetter(args);
            value = (float) args[1];
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

    public class IntCommandRecordMessage : CommandRecordMessage
    {
        public int value;

        public IntCommandRecordMessage()
        {
        }

        public IntCommandRecordMessage(NetworkInstanceId id, CommandRecord cmdRecord, int value) : base(id, cmdRecord)
        {
            this.value = value;
        }

        public override void VarargsSetter(params object[] args)
        {
            base.VarargsSetter(args);
            value = (int) args[1];
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

    public class Vector3CommandRecordMessage : CommandRecordMessage
    {
        public Vector3 value;

        public Vector3CommandRecordMessage()
        {
        }

        public Vector3CommandRecordMessage(NetworkInstanceId id, CommandRecord cmdRecord, Vector3 value) : base(id, cmdRecord)
        {
            this.value = value;
        }

        public override void VarargsSetter(params object[] args)
        {
            base.VarargsSetter(args);
            value = (Vector3) args[1];
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
}