using System;
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

    /// <summary>
    /// Message that can hold any type readable and writeable by <see cref="NetworkReader"/> and <see cref="NetworkWriter"/>.
    /// </summary>
    public class VarArgsNetIdMessasge : NetIdMessage
    {
        /// <summary>
        /// An argument list for a callback method. This is an array of objects with the same number, order, and type as
        /// the parameters of the method. It will be null if the method has no parameters.
        /// </summary>
        public object[] args;

        /// <summary>
        /// Message id for the callback method. This MUST be set in order to call <see cref="Deserialize"/>.
        /// </summary>
        public short msgType = -1;

        public VarArgsNetIdMessasge()
        {
        }

        public VarArgsNetIdMessasge(NetworkInstanceId id, params object[] values) : base(id)
        {
            args = values;
        }

        public override object[] VarargsGetter()
        {
            return args;
        }

        public override void VarargsSetter(params object[] values)
        {
            args = values;
        }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);

            if (msgType == -1)
            {
                if (LogFilter.logFatal) { Debug.LogError("Cannot deserialize message: the field " + nameof(msgType) + "hasn't been set."); }
                return;
            }

            Type[] types;
            if (Registration.ParameterTypes.TryGetValue(msgType, out types))
            {
                if (types == null) return;

                var length = types.Length;
                args = new object[length];

                for (int i = 0; i < length; ++i)
                {
                    args[i] = reader.Read(types[i]);
                }
            }
            else
            {
                if (LogFilter.logFatal) { Debug.LogError("Cannot deserialize message: message id " + msgType + " not found "); }
            }
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);

            if (args == null) return;
            for (int i = 0; i < args.Length; ++i)
            {
                writer.Write(args[i]);
            }
        }
    }
}